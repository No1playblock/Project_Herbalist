using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;

namespace Herbalist.Levels
{
    public enum BranchRidePhase { Boarding, Riding, Falling, Complete }

    // Host owns track, contacts, riders and reset. Actor input and abilities remain separate.
    [RequireComponent(typeof(NetworkObject))]
    public sealed class BranchRideFlow : NetworkBehaviour
    {
        [SerializeField] private StageLevel _level;
        [SerializeField] private Transform _talisman;
        [SerializeField] private BranchRideCarrier _carrier;
        // Existing identifiers are retained for serialized/editor/audio compatibility.
        [SerializeField, HideInInspector] private Transform[] _route, _branches, _stones;
        [SerializeField, HideInInspector] private SapInjectionPort[] _targets;
        [SerializeField, HideInInspector] private LeafInstallTarget _stopSocket;
        [SerializeField] private BranchRideTrack[] _track;
        [SerializeField] private BranchRideSwitch[] _switches;
        [SerializeField] private BranchRideStone[] _hazards;
        [SerializeField] private LeafInstallTarget[] _leafSurfaces;
        [SerializeField] private Transform[] _respawns, _dismounts;
        [SerializeField] private GameObject _endingPanel;
        [SerializeField, Min(.1f)] private float _boardRange = 7;
        [SerializeField, Min(.1f)] private float _speed = 3;
        [SerializeField, Min(.01f)] private float _connectionTolerance = .25f;
        [SerializeField, Min(.01f)] private float _carrierHeight = .5f;
        [SerializeField, Min(.1f)] private float _leafStopRadius = 1.4f;
        [SerializeField, Min(.1f)] private float _collisionRadius = 1.1f;
        [SerializeField, Min(.1f)] private float _dropGravity = 14;
        [SerializeField, Min(.1f)] private float _failureDuration = 1.1f;
        [SerializeField] private Vector3[] _riderOffsets = { new(-1.5f, .25f, 0), new(1.5f, .25f, 0) };
        [SerializeField] private string _boardText = "부적에 탑승했습니다. 두 명이 탑승하면 출발합니다.";
        [SerializeField] private string _waitingText = "두 명 모두 E로 부적에 탑승하세요.";
        [SerializeField] private string _ridingText = "수액으로 과녁을 맞혀 길 연결 · 경로에 잎을 설치해 정지 · 회수하면 출발";
        [SerializeField] private string _blockedText = "나뭇잎 앞에서 정지 · 회수하면 다시 이동합니다";
        [SerializeField] private string _failedText = "추락 또는 돌 충돌 · 시작 지점에서 다시 탑승하세요";
        [SerializeField] private string _completeText = "서낭당 최하단 도착 · 3스테이지 완료";
        [Networked] private int RiderMask { get; set; }
        [Networked] private int Segment { get; set; }
        [Networked] private float SegmentDistance { get; set; }
        [Networked] private int BranchMask { get; set; }
        [Networked] private Vector4 BranchTurns { get; set; }
        [Networked] private float StoneClock { get; set; }
        [Networked] private BranchRidePhase Phase { get; set; }
        [Networked] private Vector3 CarrierPosition { get; set; }
        [Networked] private Quaternion CarrierRotation { get; set; }
        [Networked] private NetworkBool Blocked { get; set; }
        [Networked] private int Failures { get; set; }
        private static readonly HashSet<BranchRideFlow> Active = new();
        private readonly StageActor[] _riders = new StageActor[2];
        private readonly List<LeafProjectile> _leaves = new();
        private int _mask, _segment, _branchMask, _failures;
        private float _distance, _stoneClock, _fallTime, _failureBanner;
        private Vector4 _turns;
        private Vector3 _fallOrigin, _carrierPosition;
        private Quaternion _carrierRotation = Quaternion.identity;
        private BranchRidePhase _phase;
        private bool _blocked;
        private bool Online => Object != null && Object.IsValid;
        private bool Authority => Online ? HasStateAuthority : !(FusionLobbySession.Instance is { HasNetworkSession: true });
        public BranchRidePhase CurrentPhase => Online ? Phase : _phase;
        public int CurrentSegment => Online ? Segment : _segment;
        public int FailureCount => Online ? Failures : _failures;
        public int PresentationBranchMask => Online ? BranchMask : _branchMask;
        public float PresentationStoneClock => Online ? StoneClock : _stoneClock;
        public bool PresentationBlockedByLeaf => Online ? Blocked : _blocked;
        public Vector3 Position => Online ? CarrierPosition : _carrierPosition;
        public IReadOnlyList<BranchRideTrack> Track => _track;
        public string Objective => CurrentPhase == BranchRidePhase.Complete ? _completeText :
            CurrentPhase == BranchRidePhase.Falling ? _failedText : CurrentPhase == BranchRidePhase.Boarding ?
            (_failureBanner > 0 ? _failedText : _waitingText) : PresentationBlockedByLeaf ? _blockedText : _ridingText;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() => Active.Clear();
        public static bool IsRiding(StageActor actor)
        {
            if (actor == null || !actor.Available || actor.Slot < 0 || actor.Slot > 1) return false;
            foreach (var flow in Active)
                if (((flow.Online ? flow.RiderMask : flow._mask) & (1 << actor.Slot)) != 0 &&
                    (flow.Online || flow._riders[actor.Slot] == actor)) return true;
            return false;
        }
        private bool Configured => _carrier != null && _talisman != null && _track != null && _track.Length > 0 &&
            _track[0] != null && _track[0].Valid && _switches != null && _switches.Length == 4;
        private void OnEnable() => Active.Add(this);
        private void Start()
        {
            if (!Configured) { Debug.LogError("Branch ride needs authored track and four switches. Rebuild Stage Three Blockout.", this); return; }
            if (!Online && Authority) ResetRide(false);
        }
        private void OnDisable() { Active.Remove(this); foreach (var actor in _riders) Release(actor); }
        public override void Spawned() { if (HasStateAuthority && Configured) { ResetRide(false); Publish(); } }
        public static bool TryBoard(StageActor actor, out string feedback)
        {
            feedback = null;
            if (actor == null || !actor.Available || actor.Slot < 0 || actor.Slot > 1) return false;
            foreach (var flow in Active)
            {
                if (!flow.Authority || !flow.Configured || flow._phase != BranchRidePhase.Boarding) continue;
                int slot = actor.Slot;
                if (flow._riders[slot] == actor) { feedback = flow._boardText; return true; }
                if (flow._riders[slot] != null || Vector3.Distance(actor.transform.position, flow._carrierPosition) > flow._boardRange) continue;
                var controller = actor.GetComponent<PlayerController>();
                if (controller == null || controller.Motor.MovementLocked) continue;
                flow._riders[slot] = actor;
                flow._mask |= 1 << slot;
                flow.Carry(actor, slot);
                if (flow._mask == 3) flow._phase = BranchRidePhase.Riding;
                flow.Publish();
                feedback = flow._boardText;
                return true;
            }
            return false;
        }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        private void Update()
        {
            if (!Configured) return;
            if (!Online && Authority) Tick(Time.deltaTime);
            if (!Herbalist.GameUI.GameplayPause.IsPaused) _failureBanner = Mathf.Max(0, _failureBanner - Time.deltaTime);
            Present();
        }
        public void Tick(float dt)
        {
            if (!Authority || !Configured || Herbalist.GameUI.GameplayPause.IsPaused || dt <= 0) return;
            while (dt > 0) { float step = Mathf.Min(.02f, dt); Simulate(step); dt -= step; }
            Publish();
        }
        private void Simulate(float dt)
        {
            for (int i = 0; i < 2; i++)
                if ((_mask & (1 << i)) != 0 && (_riders[i] == null || !_riders[i].Available)) { ResetRide(true); return; }
            if (_phase == BranchRidePhase.Complete) return;
            if (_phase == BranchRidePhase.Falling)
            {
                _fallTime += dt;
                _carrierPosition = _fallOrigin + Vector3.down * (.5f * _dropGravity * _fallTime * _fallTime);
                CarryRiders();
                if (_fallTime >= _failureDuration) ResetRide(true);
                return;
            }
            for (int i = 0; i < 4; i++)
            {
                if (_switches[i].ConsumeActivation(dt)) _branchMask ^= 1 << i;
                _turns[i] = Mathf.MoveTowards(_turns[i], (_branchMask & (1 << i)) != 0 ? 1 : 0, dt / _switches[i].RotationDuration);
                _switches[i].Present(_turns[i]);
            }
            if (_phase != BranchRidePhase.Riding) { CarryRiders(); return; }
            GatherLeaves();
            Vector3 previous = _carrierPosition;
            _blocked = false;
            Advance(dt);
            float previousClock = _stoneClock;
            _stoneClock += dt;
            if (_phase == BranchRidePhase.Riding) ResolveStones(previousClock, previous);
            CarryRiders();
        }
        private void GatherLeaves()
        {
            _leaves.Clear();
            if (_leafSurfaces == null) return;
            foreach (var target in _leafSurfaces) if (target != null) target.CopyInstalledLeavesTo(_leaves);
        }
        private void Advance(float dt)
        {
            var result = _carrier.Step(_track[_segment], _distance, _carrierRotation, dt,
                _speed, _dropGravity, _carrierHeight, _leafStopRadius, _leaves);
            _distance = result.distance; _carrierPosition = result.position;
            _carrierRotation = result.rotation; _blocked = result.blocked;
            if (result.atEnd) NextTrack();
        }
        private void NextTrack()
        {
            var current = _track[_segment];
            if (current.IsFinish) { FinishRide(); return; }
            // The rotating tip selects a physical A/B branch, rather than closing an abstract gate.
            int nextIndex = -1;
            float nearest = _connectionTolerance;
            foreach (var candidate in current.Exits)
            {
                if (candidate == null) continue;
                float gap = Vector3.Distance(current.End, candidate.Start);
                if (gap > nearest) continue;
                int index = System.Array.IndexOf(_track, candidate);
                if (index < 0) continue;
                nearest = gap;
                nextIndex = index;
            }
            if (nextIndex < 0) { FailRide(); return; }
            _segment = nextIndex;
            _distance = 0;
        }
        private void ResolveStones(float previousClock, Vector3 previousCarrier)
        {
            if (_hazards == null) return;
            foreach (var stone in _hazards)
            {
                if (stone == null || !stone.Sample(_stoneClock, out var now)) continue;
                bool continuous = stone.Sample(previousClock, out var before) && stone.CycleAt(previousClock) == stone.CycleAt(_stoneClock);
                if (!continuous) before = now;
                foreach (var leaf in _leaves)
                    if (leaf.Installed && BranchRideTrack.DistanceToSegment(leaf.ContactPoint, before, now) <= stone.Radius + .25f)
                        leaf.BeginReturn();
                if (BranchRideTrack.DistanceToSegment(Vector3.zero, before - previousCarrier, now - _carrierPosition) <= stone.Radius + _collisionRadius)
                { FailRide(); return; }
            }
        }
        private void FailRide()
        {
            if (_phase != BranchRidePhase.Riding) return;
            _phase = BranchRidePhase.Falling; _fallOrigin = _carrierPosition; _fallTime = 0; _blocked = false;
        }
        private void FinishRide()
        {
            _phase = BranchRidePhase.Complete; _blocked = false;
            for (int i = 0; i < 2; i++)
            {
                var actor = _riders[i]; Release(actor);
                if (actor != null && _dismounts != null && i < _dismounts.Length) Teleport(actor, _dismounts[i].position);
                _riders[i] = null;
            }
            _mask = 0;
        }
        private void CarryRiders() { for (int i = 0; i < 2; i++) Carry(_riders[i], i); }
        private void Carry(StageActor actor, int slot)
        {
            if (actor == null) return;
            Vector3 point = _carrierPosition + _carrierRotation * _riderOffsets[slot];
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid) net.SetTransportAuthoritatively(true, point);
            else actor.GetComponent<PlayerController>().SetTransport(this, point);
        }
        private void Release(StageActor actor)
        {
            if (actor == null) return;
            actor.GetComponent<PlayerController>().ClearTransport(this);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority) net.SetTransportAuthoritatively(false, actor.transform.position);
        }
        private static void Teleport(StageActor actor, Vector3 position)
        {
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid) net.TeleportAuthoritatively(position);
            else actor.GetComponent<PlayerController>().Motor.Teleport(position);
        }
        private void ResetRide(bool failure)
        {
            for (int i = 0; i < 2; i++)
            {
                var actor = _riders[i]; Release(actor);
                if (actor != null)
                {
                    actor.Abilities.Leaf.Clear(); actor.Abilities.Sap?.Clear();
                    if (_respawns != null && i < _respawns.Length) Teleport(actor, _respawns[i].position);
                }
                _riders[i] = null;
            }
            GatherLeaves(); foreach (var leaf in _leaves) if (leaf != null) leaf.Finish();
            if (failure) { _failures++; _failureBanner = 4; }
            _mask = _segment = _branchMask = 0; _distance = _stoneClock = _fallTime = 0;
            _phase = BranchRidePhase.Boarding; _turns = Vector4.zero; _blocked = false;
            foreach (var toggle in _switches) { toggle.ResetInput(); toggle.Present(0); }
            _carrierPosition = _track[0].Sample(0, out var heading) + Vector3.up * _carrierHeight;
            _carrierRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(heading, Vector3.up));
            Present();
        }
        private void Publish()
        {
            if (!Online || !HasStateAuthority) return;
            RiderMask = _mask; Segment = _segment; SegmentDistance = _distance; BranchMask = _branchMask;
            BranchTurns = _turns; StoneClock = _stoneClock; Phase = _phase;
            CarrierPosition = _carrierPosition; CarrierRotation = _carrierRotation; Blocked = _blocked; Failures = _failures;
        }
        private void Present()
        {
            Vector4 turns = Online ? BranchTurns : _turns;
            for (int i = 0; i < _switches.Length; i++) _switches[i].Present(turns[i]);
            _talisman.SetPositionAndRotation(Position, Online ? CarrierRotation : _carrierRotation);
            if (_hazards != null) foreach (var stone in _hazards) if (stone != null) stone.Present(PresentationStoneClock, CurrentPhase == BranchRidePhase.Riding);
            if (_endingPanel != null) _endingPanel.SetActive(CurrentPhase == BranchRidePhase.Complete);
        }
    }
}
