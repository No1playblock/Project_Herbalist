using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class HangingCloth : NetworkBehaviour, IPlayerMovementReceiver
    {
        [SerializeField] private Transform _knot;
        [SerializeField] private Transform _cloth;
        [SerializeField] private Transform _lowerCloth;
        [SerializeField] private Vector3[] _gripOffsets = { new(-.45f, -1, 0), new(.45f, -1, 0) };
        [SerializeField, Min(.1f)] private float _length = 7;
        [SerializeField, Range(0, 60)] private float _maxAngle = 30;
        [SerializeField, Min(.1f)] private float _grabRange = 2.5f;
        [SerializeField, Min(0)] private float _driveAcceleration = 65;
        [SerializeField, Min(0)] private float _pumpImpulseSeconds = .15f;
        [SerializeField, Min(0)] private float _spring = 4;
        [SerializeField, Min(0)] private float _damping = 1.2f;
        [SerializeField, Min(0)] private float _releaseSpeed = 1;
        [SerializeField, Range(0, 30)] private float _bendMaxAngle = 30;
        [SerializeField, Min(.01f)] private float _bendProbeRadius = .08f;
        [SerializeField, Min(.1f)] private float _minBendSegmentLength = 1f;
        [SerializeField] private LayerMask _obstructionMask = 1;
        [SerializeField] private string _grabText = "천의 매듭을 잡았습니다 · 먼저 잡은 플레이어가 WASD를 번갈아 눌러 힘 주기 · E: 놓기";
        [SerializeField] private string _releaseText = "천을 놓았습니다.";
        [SerializeField] private string _fullText = "매듭에 두 명이 매달려 있습니다.";
        [Networked] private Vector2 Angles { get; set; }
        [Networked] private NetworkBool BendActive { get; set; }
        [Networked] private Vector3 BendPoint { get; set; }
        [Networked] private float BendLength { get; set; }
        [Networked] private Vector2 BendAngles { get; set; }
        [Networked] private int Rider0 { get; set; }
        [Networked] private int Rider1 { get; set; }
        private Vector2 _angles, _velocity, _drive, _heldInput;
        private Vector2 _bendAngles, _bendVelocity, _clothSize;
        private Vector3 _bendPoint;
        private float _bendLength;
        private bool _bendActive;
        private readonly RaycastHit[] _bendProbeHits = new RaycastHit[32];
        private readonly StageActor[] _riders = new StageActor[2];
        private readonly Vector3[] _lastPoints = new Vector3[2];
        private static readonly HashSet<HangingCloth> Cloths = new();
        private bool Online => Object != null && Object.IsValid;
        public bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        private void Awake() { if (_cloth != null) _clothSize = new Vector2(_cloth.localScale.x, _cloth.localScale.z); }
        private void OnEnable() => Cloths.Add(this);
        public override void Spawned() { if (HasStateAuthority) { Rider0 = Rider1 = -1; Angles = Vector2.zero; BendActive = false; BendPoint = Vector3.zero; BendLength = 0; BendAngles = Vector2.zero; } }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        public static bool TryInteract(StageActor actor, out string feedback)
        {
            feedback = null;
            HangingCloth nearest = null; float distance = float.PositiveInfinity;
            foreach (var cloth in Cloths)
            {
                if (!cloth.Authority) continue;
                for (int i = 0; i < 2; i++) if (cloth._riders[i] == actor) { cloth.Detach(i); feedback = cloth._releaseText; return true; }
                float d = Vector3.Distance(actor.transform.position + Vector3.up, cloth._knot.position);
                if (d <= cloth._grabRange && d < distance) { nearest = cloth; distance = d; }
            }
            if (nearest == null) return false;
            var player = actor.GetComponent<PlayerController>();
            if (player.Motor.MovementLocked) return false;
            for (int i = 0; i < 2; i++)
                if (nearest._riders[i] == null)
                {
                    nearest._riders[i] = actor;
                    nearest._lastPoints[i] = actor.transform.position;
                    player.SetMovementReceiver(nearest); player.Motor.SetMovementLock(nearest, true);
                    actor.Abilities.Sap?.Cancel(); actor.Abilities.SetInputLock(nearest, true);
                    nearest.Publish(); feedback = nearest._grabText; return true;
                }
            feedback = nearest._fullText; return true;
        }
        public void ReceiveMovement(PlayerController player, Vector2 move)
        {
            if (!Authority || _riders[0] == null || _riders[0].GetComponent<PlayerController>() != player) return;
            Vector2 held = new(move.x > .5f ? 1 : move.x < -.5f ? -1 : 0,
                               move.y > .5f ? 1 : move.y < -.5f ? -1 : 0);
            Vector2 pressed = new(held.x != 0 && held.x != _heldInput.x ? held.x : 0,
                                  held.y != 0 && held.y != _heldInput.y ? held.y : 0);
            _heldInput = held;
            if (pressed == Vector2.zero) return;
            Vector3 world = Quaternion.Euler(0, player.View.Yaw, 0) * new Vector3(pressed.x, 0, pressed.y);
            // Positive X rotation moves a hanging point toward -Z; positive Z toward +X.
            _drive += Vector2.ClampMagnitude(new Vector2(-world.z, world.x), 1);
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused || dt <= 0) return;
            for (int i = 0; i < 2; i++) if (_riders[i] != null && !_riders[i].Available) Detach(i);
            Vector2 impulse = _drive * (_driveAcceleration * _pumpImpulseSeconds);
            _drive = Vector2.zero;
            if (_bendActive)
            {
                Vector2 velocity = _bendVelocity;
                Vector2 next = Step(_bendAngles, ref velocity, impulse, dt, _bendMaxAngle);
                if (RiderBlocked(_angles, next)) _bendVelocity = Vector2.zero;
                else { _bendAngles = next; _bendVelocity = velocity; }
            }
            else
            {
                Vector2 velocity = _velocity;
                Vector2 next = Step(_angles, ref velocity, impulse, dt, _maxAngle);
                if (HasRider && velocity.sqrMagnitude > .01f && TryFindBend(next, out var point, out var length) && !RiderBlocked(next, Vector2.zero))
                {
                    _angles = next;
                    _bendActive = true; _bendPoint = point; _bendLength = length;
                    _bendAngles = Vector2.zero; _bendVelocity = velocity; _velocity = Vector2.zero;
                }
                else if (RiderBlocked(next, Vector2.zero)) _velocity = Vector2.zero;
                else { _angles = next; _velocity = velocity; }
            }
            Present(_angles, _bendActive, _bendPoint, _bendLength, _bendAngles);
            for (int i = 0; i < 2; i++)
            {
                var actor = _riders[i]; if (actor == null) continue;
                Vector3 point = Position(_angles, _bendAngles, i); _lastPoints[i] = point;
                var net = actor.GetComponent<NetworkPlayer>();
                if (net != null && net.Object != null && net.Object.IsValid) net.SetTransportAuthoritatively(true, point);
                else actor.GetComponent<PlayerController>().SetTransport(this, point);
            }
            Publish();
        }
        private bool HasRider => _riders[0] != null || _riders[1] != null;
        private Vector2 Step(Vector2 angles, ref Vector2 velocity, Vector2 impulse, float dt, float maxAngle)
        {
            velocity += impulse;
            velocity += (-angles * _spring - velocity * _damping) * dt;
            Vector2 next = Vector2.ClampMagnitude(angles + velocity * dt, maxAngle);
            if (next.sqrMagnitude >= maxAngle * maxAngle && Vector2.Dot(velocity, next) > 0)
                velocity -= Vector2.Dot(velocity, next.normalized) * next.normalized;
            return next;
        }
        private bool TryFindBend(Vector2 angles, out Vector3 point, out float lowerLength)
        {
            point = Vector3.zero; lowerLength = 0;
            Vector3 origin = transform.position;
            Vector3 direction = Quaternion.Euler(angles.x, 0, angles.y) * Vector3.down;
            int count = Physics.SphereCastNonAlloc(origin, _bendProbeRadius, direction, _bendProbeHits, _length, _obstructionMask, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var hit = _bendProbeHits[i];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform) || hit.collider.GetComponentInParent<ClothBendSurface>() == null) continue;
                float upperLength = Vector3.Distance(origin, hit.point);
                if (upperLength < _minBendSegmentLength || _length - upperLength < _minBendSegmentLength || upperLength >= nearest) continue;
                nearest = upperLength; point = hit.point; lowerLength = _length - upperLength;
            }
            return nearest < float.PositiveInfinity;
        }
        private bool RiderBlocked(Vector2 primaryNext, Vector2 secondaryNext)
        {
            for (int i = 0; i < 2; i++)
            {
                if (_riders[i] == null) continue;
                Vector3 from = Position(_angles, _bendAngles, i), to = Position(primaryNext, secondaryNext, i);
                var cc = _riders[i].GetComponent<CharacterController>();
                Vector3 delta = to - from;
                if (delta.sqrMagnitude <= .000001f) continue;
                Vector3 probe = from + cc.center;
                foreach (var hit in Physics.SphereCastAll(probe, cc.radius, delta.normalized, delta.magnitude, _obstructionMask, QueryTriggerInteraction.Ignore))
                    if (hit.collider.GetComponentInParent<StageActor>() == null && !hit.collider.transform.IsChildOf(transform)) return true;
            }
            return false;
        }
        private Vector3 Position(Vector2 primary, Vector2 secondary, int side)
        {
            Quaternion baseRotation = Quaternion.Euler(primary.x, 0, primary.y);
            if (!_bendActive) return transform.position + baseRotation * (Vector3.down * _length + _gripOffsets[side]);
            Quaternion lowerRotation = Quaternion.Euler(secondary.x, 0, secondary.y) * baseRotation;
            return _bendPoint + lowerRotation * (Vector3.down * _bendLength + _gripOffsets[side]);
        }
        private void Present(Vector2 primary, bool bent, Vector3 point, float lowerLength, Vector2 secondary)
        {
            Quaternion baseRotation = Quaternion.Euler(primary.x, 0, primary.y);
            Vector3 knot = bent ? point + Quaternion.Euler(secondary.x, 0, secondary.y) * baseRotation * (Vector3.down * lowerLength)
                                 : transform.position + baseRotation * (Vector3.down * _length);
            _knot.position = knot;
            SetSegment(_cloth, transform.position, bent ? point : knot);
            if (_lowerCloth == null) return;
            if (_lowerCloth.gameObject.activeSelf != bent) _lowerCloth.gameObject.SetActive(bent);
            if (bent) SetSegment(_lowerCloth, point, knot);
        }
        private void SetSegment(Transform segment, Vector3 start, Vector3 end)
        {
            segment.position = (start + end) * .5f;
            segment.rotation = Quaternion.FromToRotation(Vector3.down, end - start);
            segment.localScale = new Vector3(_clothSize.x, Vector3.Distance(start, end), _clothSize.y);
        }
        private void Publish() { if (Online) { Angles = _angles; BendActive = _bendActive; BendPoint = _bendPoint; BendLength = _bendLength; BendAngles = _bendAngles; Rider0 = _riders[0] != null ? _riders[0].Slot : -1; Rider1 = _riders[1] != null ? _riders[1].Slot : -1; } }
        private void Detach(int side)
        {
            var actor = _riders[side]; if (actor == null) return;
            var player = actor.GetComponent<PlayerController>();
            var velocity = (_bendActive ? Position(_angles, _bendAngles + _bendVelocity * .01f, side) - Position(_angles, _bendAngles, side)
                                        : Position(_angles + _velocity * .01f, _bendAngles, side) - Position(_angles, _bendAngles, side)) / .01f * _releaseSpeed;
            player.ClearMovementReceiver(this); player.ClearTransport(this); player.Motor.SetMovementLock(this, false); actor.Abilities.SetInputLock(this, false);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority) { net.SetTransportAuthoritatively(false, actor.transform.position); net.LaunchAuthoritatively(velocity, player.Tuning.gravity, true); }
            else if (!Online) player.Motor.Launch(velocity, player.Tuning.gravity, true);
            _riders[side] = null;
            if (side == 0 && _riders[1] != null) { _riders[0] = _riders[1]; _riders[1] = null; }
            if (side == 0) { _heldInput = Vector2.zero; _drive = Vector2.zero; }
            Publish();
        }
        private void Update() { if (!Online && Authority) Tick(Time.deltaTime); else if (Online && !HasStateAuthority) Present(Angles, BendActive, BendPoint, BendLength, BendAngles); }
        private void OnDisable() { Cloths.Remove(this); if (Authority) { Detach(1); Detach(0); } }
    }
}
