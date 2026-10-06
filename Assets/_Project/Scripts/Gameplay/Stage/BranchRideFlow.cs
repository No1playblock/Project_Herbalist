using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;

namespace Herbalist.Levels
{
    // Scene-authored route and targets. Only the host advances the carrier and resolves hits.
    [RequireComponent(typeof(NetworkObject))]
    public sealed class BranchRideFlow : NetworkBehaviour
    {
        [SerializeField] private StageLevel _level;
        [SerializeField] private Transform _talisman;
        [SerializeField] private Transform[] _route;
        [SerializeField] private Transform[] _branches;
        [SerializeField] private SapInjectionPort[] _targets;
        [SerializeField] private LeafInstallTarget _stopSocket;
        [SerializeField] private Transform[] _stones;
        [SerializeField] private GameObject _endingPanel;
        [SerializeField, Min(.1f)] private float _boardRange = 3;
        [SerializeField, Min(.1f)] private float _speed = 3;
        [SerializeField, Min(.01f)] private float _targetCharge = .35f;
        [SerializeField, Min(.01f)] private float _targetRearmDelay = .4f;
        [SerializeField, Min(.1f)] private float _stonePeriod = 5;
        [SerializeField, Min(.1f)] private float _stoneCrossingTime = 2;
        [SerializeField, Min(.1f)] private float _collisionRadius = 1.2f;
        [SerializeField] private Vector3[] _riderOffsets = { new(-.65f, .5f, 0), new(.65f, .5f, 0) };
        [SerializeField] private string _boardText = "부적에 탑승했습니다. 두 명이 탑승하면 출발합니다.";
        [SerializeField] private string _waitingText = "두 명 모두 E로 부적에 탑승하세요.";
        [SerializeField] private string _ridingText = "R로 T1~T4를 맞혀 가지를 돌리세요. 나뭇잎은 부적을 멈춥니다.";
        [Networked] private int RiderMask { get; set; }
        [Networked] private int Segment { get; set; }
        [Networked] private float SegmentDistance { get; set; }
        [Networked] private int BranchMask { get; set; }
        [Networked] private float StoneClock { get; set; }
        [Networked] private NetworkBool Complete { get; set; }
        private static readonly HashSet<BranchRideFlow> Active = new();
        private readonly StageActor[] _riders = new StageActor[2];
        private readonly float[] _charges = new float[4];
        private readonly float[] _targetDry = new float[4];
        private readonly bool[] _targetArmed = { true, true, true, true };
        private int _mask, _segment;
        private float _distance, _stoneClock;
        private bool _complete;
        private bool Online => Object != null && Object.IsValid;
        private bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        public string Objective => (Online ? RiderMask : _mask) == 3 ? _ridingText : _waitingText;
        private void OnEnable() => Active.Add(this);
        private void OnDisable()
        {
            Active.Remove(this);
            for (int i = 0; i < _riders.Length; i++) Release(_riders[i]);
        }
        public override void Spawned()
        {
            if (!HasStateAuthority) return;
            RiderMask = Segment = BranchMask = 0;
            SegmentDistance = StoneClock = 0;
            Complete = false;
        }
        public static bool TryBoard(StageActor actor, out string feedback)
        {
            feedback = null;
            foreach (var flow in Active)
            {
                if (!flow.Authority || flow._complete || flow._segment != 0 || flow._distance > 0) continue;
                if (Vector3.Distance(actor.transform.position, flow._talisman.position) > flow._boardRange) continue;
                int slot = actor.Slot;
                if (slot < 0 || slot > 1) continue;
                if (flow._riders[slot] == actor) { feedback = flow._boardText; return true; }
                if (flow._riders[slot] != null || actor.GetComponent<PlayerController>().Motor.MovementLocked) continue;
                flow._riders[slot] = actor;
                flow._mask |= 1 << slot;
                if (flow.Online) flow.RiderMask = flow._mask;
                flow.Carry(actor, slot);
                feedback = flow._boardText;
                return true;
            }
            return false;
        }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        private void Update()
        {
            if (!Online && Authority) Tick(Time.deltaTime);
            Present(Online ? Segment : _segment, Online ? SegmentDistance : _distance,
                    Online ? BranchMask : _branchMask, Online ? StoneClock : _stoneClock);
            if (_endingPanel != null) _endingPanel.SetActive(Online ? Complete : _complete);
        }
        private int _branchMask;
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused || _route == null || _route.Length < 2) return;
            for (int i = 0; i < _riders.Length; i++)
                if ((_mask & (1 << i)) != 0 && (_riders[i] == null || !_riders[i].Available)) { ResetRide(); break; }
            for (int i = 0; i < _targets.Length; i++)
            {
                float hit = _targets[i].Consume();
                _targetDry[i] = hit > 0 ? 0 : _targetDry[i] + dt;
                if (!_targetArmed[i] && _targetDry[i] >= _targetRearmDelay) _targetArmed[i] = true;
                if (!_targetArmed[i]) continue;
                _charges[i] += hit;
                if (_charges[i] < _targetCharge) continue;
                _charges[i] = 0;
                _targetArmed[i] = false;
                _branchMask ^= 1 << i;
            }
            if (_mask == 3 && !_complete)
            {
                _stoneClock += dt;
                // Four targets align successive branches before the carrier passes each gap.
                bool gateOpen = _segment >= _targets.Length || (_branchMask & (1 << _segment)) != 0;
                if (_stopSocket == null || !_stopSocket.HasLeaf)
                {
                    if (gateOpen) _distance += _speed * dt;
                    float length = Vector3.Distance(_route[_segment].position, _route[_segment + 1].position);
                    if (_distance >= length)
                    {
                        _distance -= length;
                        _segment++;
                        if (_segment >= _route.Length - 1) { _segment = _route.Length - 2; _distance = Vector3.Distance(_route[_segment].position, _route[_segment + 1].position); _complete = true; }
                    }
                }
                Present(_segment, _distance, _branchMask, _stoneClock);
                for (int i = 0; i < 2; i++) Carry(_riders[i], i);
                if (StoneStrike()) ResetRide();
            }
            if (Online)
            {
                RiderMask = _mask; Segment = _segment; SegmentDistance = _distance;
                BranchMask = _branchMask; StoneClock = _stoneClock; Complete = _complete;
            }
        }
        private bool StoneStrike()
        {
            for (int i = 0; i < _stones.Length; i++)
            {
                float crossing = Mathf.Repeat(_stoneClock + i * _stonePeriod * .5f, _stonePeriod);
                if (crossing > _stoneCrossingTime) continue;
                if (Vector3.Distance(_stones[i].position, _talisman.position) > _collisionRadius) continue;
                if (_stopSocket != null && _stopSocket.HasLeaf)
                {
                    _stopSocket.InstalledLeaf.BeginReturn();
                    _stoneClock += _stoneCrossingTime;
                    return false;
                }
                return true;
            }
            return false;
        }
        private void Present(int segment, float distance, int branches, float clock)
        {
            segment = Mathf.Clamp(segment, 0, _route.Length - 2);
            Vector3 a = _route[segment].position, b = _route[segment + 1].position;
            _talisman.position = Vector3.MoveTowards(a, b, Mathf.Max(0, distance));
            for (int i = 0; i < _branches.Length; i++)
                _branches[i].localRotation = Quaternion.Euler(0, (branches & (1 << i)) != 0 ? 0 : 75, 0);
            for (int i = 0; i < _stones.Length; i++)
            {
                float crossing = Mathf.Repeat(clock + i * _stonePeriod * .5f, _stonePeriod);
                _stones[i].gameObject.SetActive(crossing <= _stoneCrossingTime);
                if (crossing <= _stoneCrossingTime)
                {
                    Vector3 p = _stones[i].parent.position;
                    p.y += Mathf.Lerp(8, -3, crossing / _stoneCrossingTime);
                    _stones[i].position = p;
                }
            }
        }
        private void Carry(StageActor actor, int slot)
        {
            if (actor == null) return;
            Vector3 point = _talisman.TransformPoint(_riderOffsets[slot]);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid) net.SetTransportAuthoritatively(true, point);
            else actor.GetComponent<PlayerController>().SetTransport(this, point);
        }
        private void Release(StageActor actor)
        {
            if (actor == null) return;
            actor.GetComponent<PlayerController>().ClearTransport(this);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority)
                net.SetTransportAuthoritatively(false, actor.transform.position);
        }
        private void ResetRide()
        {
            for (int i = 0; i < 2; i++)
            {
                var actor = _riders[i];
                Release(actor);
                if (actor != null)
                {
                    Vector3 spawn = _route[0].position + new Vector3(i == 0 ? -2 : 2, .6f, -3);
                    var net = actor.GetComponent<NetworkPlayer>();
                    if (net != null && net.Object != null && net.Object.IsValid) net.TeleportAuthoritatively(spawn);
                    else actor.GetComponent<PlayerController>().Motor.Teleport(spawn);
                }
                _riders[i] = null;
            }
            _mask = _segment = _branchMask = 0; _distance = _stoneClock = 0; _complete = false;
            for (int i = 0; i < _charges.Length; i++)
            {
                _charges[i] = _targetDry[i] = 0; _targetArmed[i] = true; _targets[i].Consume();
            }
            if (_stopSocket != null && _stopSocket.HasLeaf) _stopSocket.InstalledLeaf.BeginReturn();
            Present(0, 0, 0, 0);
        }
    }
}
