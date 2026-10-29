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
        [SerializeField] private Transform _authoredVisual;
        [SerializeField] private HangingClothSettings _settings;
        [SerializeField] private Vector3[] _gripOffsets = { new(-.45f, -1, 0), new(.45f, -1, 0) };
        [SerializeField, Min(.1f)] private float _length = 7; // Fallback for a legacy GripPoint left at the pivot.
        [SerializeField] private string _grabText = "천의 매듭을 잡았습니다 · WASD로 흔들림에 맞춰 힘 주기 · 상호작용/점프: 놓기";
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
        private Vector2 _pendingDrive, _pendingDriveTime;
        private Vector2 _bendAngles, _bendVelocity, _clothSize;
        private Vector3 _bendPoint;
        private float _bendLength;
        private bool _bendActive;
        private readonly RaycastHit[] _bendProbeHits = new RaycastHit[32];
        private readonly StageActor[] _riders = new StageActor[2];
        private readonly float[] _grabbedAt = new float[2];
        private readonly Vector3[] _lastPoints = new Vector3[2];
        private Quaternion _authoredRootRotation;
        private Vector3 _authoredRootPosition;
        private Vector3 _visualTopLocal;
        private Vector3 _fixedVisualTop;
        private Vector3 _authoredGripLocalPosition;
        private Vector3 _restGripOffset;
        private static readonly HashSet<HangingCloth> Cloths = new();
        private bool Online => Object != null && Object.IsValid;
        public bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        private void Awake()
        {
            _authoredRootPosition = transform.position;
            _authoredRootRotation = transform.rotation;
            if (_knot != null) _authoredGripLocalPosition = _knot.localPosition;
            if (_cloth != null) _clothSize = new Vector2(_cloth.localScale.x, _cloth.localScale.z);
            if (_authoredVisual != null && _authoredVisual.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
            {
                Bounds bounds = meshFilter.sharedMesh.bounds;
                Vector3 top = new(bounds.center.x, bounds.max.y, bounds.center.z);
                _fixedVisualTop = _authoredVisual.TransformPoint(top);
                _visualTopLocal = transform.InverseTransformPoint(_fixedVisualTop);
            }
            else _fixedVisualTop = _authoredRootPosition;
            _restGripOffset = _knot != null ? _knot.position - _fixedVisualTop : Vector3.zero;
            if (_restGripOffset.sqrMagnitude < .0001f)
                _restGripOffset = transform.TransformVector(Vector3.down * _length);
        }
        private void OnEnable() => Cloths.Add(this);
        public static bool TryInteractionHint(StageActor actor, out Vector3 point, out bool release)
        {
            point = default;
            release = false;
            if (actor == null || !actor.Available) return false;
            var controller = actor.GetComponent<CharacterController>();
            Vector3 grabPosition = controller != null ? controller.bounds.center : actor.transform.position + Vector3.up;
            HangingCloth nearest = null;
            float distance = float.PositiveInfinity;
            foreach (var cloth in Cloths)
            {
                if (cloth._knot == null) continue;
                bool riding = cloth.Online ? cloth.Rider0 == actor.Slot || cloth.Rider1 == actor.Slot :
                    cloth._riders[0] == actor || cloth._riders[1] == actor;
                if (riding)
                {
                    point = cloth._knot.position;
                    release = true;
                    return true;
                }
                bool full = cloth.Online ? cloth.Rider0 >= 0 && cloth.Rider1 >= 0 :
                    cloth._riders[0] != null && cloth._riders[1] != null;
                if (full) continue;
                float d = Vector3.Distance(grabPosition, cloth._knot.position);
                if (cloth._settings != null && d <= cloth._settings.grabRange && d < distance) { nearest = cloth; distance = d; }
            }
            if (nearest == null || actor.GetComponent<PlayerController>().Motor.MovementLocked) return false;
            point = nearest._knot.position;
            return true;
        }
        public override void Spawned() { if (HasStateAuthority) { Rider0 = Rider1 = -1; Angles = Vector2.zero; BendActive = false; BendPoint = Vector3.zero; BendLength = 0; BendAngles = Vector2.zero; } }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        public static bool IsRiding(StageActor actor)
        {
            if (actor == null || !actor.Available) return false;
            foreach (var cloth in Cloths)
                if (cloth.Online ? cloth.Rider0 == actor.Slot || cloth.Rider1 == actor.Slot :
                    cloth._riders[0] == actor || cloth._riders[1] == actor)
                    return true;
            return false;
        }
        public static bool TryRelease(StageActor actor, out string feedback)
        {
            feedback = null;
            if (actor == null || !actor.Available) return false;
            foreach (var cloth in Cloths)
            {
                if (!cloth.Authority) continue;
                for (int i = 0; i < 2; i++)
                    if (cloth._riders[i] == actor)
                    {
                        cloth.Detach(i);
                        feedback = cloth._releaseText;
                        return true;
                    }
            }
            return false;
        }
        public static bool TryInteract(StageActor actor, out string feedback)
        {
            feedback = null;
            HangingCloth nearest = null; float distance = float.PositiveInfinity;
            var controller = actor.GetComponent<CharacterController>();
            Vector3 grabPosition = controller != null ? controller.bounds.center : actor.transform.position + Vector3.up;
            foreach (var cloth in Cloths)
            {
                if (!cloth.Authority) continue;
                for (int i = 0; i < 2; i++) if (cloth._riders[i] == actor)
                {
                    if (Time.time - cloth._grabbedAt[i] >= cloth._settings.minimumHoldSeconds)
                    {
                        cloth.Detach(i);
                        feedback = cloth._releaseText;
                    }
                    return true;
                }
                if (cloth._knot == null) continue;
                float d = Vector3.Distance(grabPosition, cloth._knot.position);
                if (cloth._settings != null && d <= cloth._settings.grabRange && d < distance) { nearest = cloth; distance = d; }
            }
            if (nearest == null) return false;
            var player = actor.GetComponent<PlayerController>();
            if (player.Motor.MovementLocked) return false;
            for (int i = 0; i < 2; i++)
                if (nearest._riders[i] == null)
                {
                    nearest._riders[i] = actor;
                    nearest._grabbedAt[i] = Time.time;
                    nearest._lastPoints[i] = actor.transform.position;
                    if (i == 0) { nearest._heldInput = Vector2.zero; nearest._drive = Vector2.zero; nearest._pendingDrive = Vector2.zero; nearest._pendingDriveTime = Vector2.zero; }
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
            float cameraYaw = player.View.Yaw;
            var networkPlayer = player.GetComponent<NetworkPlayer>();
            if (networkPlayer != null && networkPlayer.Object != null && networkPlayer.Object.IsValid)
                cameraYaw = networkPlayer.LookAngles.x;
            _drive += CameraRelativeDrive(pressed, cameraYaw);
        }
        private static Vector2 CameraRelativeDrive(Vector2 pressed, float cameraYaw)
        {
            Vector3 world = Quaternion.Euler(0, cameraYaw, 0) * new Vector3(pressed.x, 0, pressed.y);
            // Positive X rotation moves a hanging point toward -Z; positive Z toward +X.
            return Vector2.ClampMagnitude(new Vector2(-world.z, world.x), 1);
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused || dt <= 0) return;
            for (int i = 0; i < 2; i++) if (_riders[i] != null && !_riders[i].Available) Detach(i);
            Vector2 angles = _bendActive ? _bendAngles : _angles;
            Vector2 velocityAtInput = _bendActive ? _bendVelocity : _velocity;
            Vector2 pump = new(
                ResolvePump(_drive.x, angles.x, velocityAtInput.x, ref _pendingDrive.x, ref _pendingDriveTime.x, dt),
                ResolvePump(_drive.y, angles.y, velocityAtInput.y, ref _pendingDrive.y, ref _pendingDriveTime.y, dt));
            Vector2 impulse = pump * (_settings.driveAcceleration * _settings.pumpImpulseSeconds);
            _drive = Vector2.zero;
            if (_bendActive)
            {
                Vector2 velocity = _bendVelocity;
                Vector2 next = Step(_bendAngles, ref velocity, impulse, dt, _settings.bendMaxAngle);
                if (RiderBlocked(_angles, next)) _bendVelocity = Vector2.zero;
                else { _bendAngles = next; _bendVelocity = velocity; }
            }
            else
            {
                Vector2 velocity = _velocity;
                Vector2 next = Step(_angles, ref velocity, impulse, dt, _settings.maxAngle);
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
                else
                {
                    var player = actor.GetComponent<PlayerController>();
                    player.SetTransport(this, point);
                    player.Motor.Teleport(point);
                }
            }
            Publish();
        }
        private bool HasRider => _riders[0] != null || _riders[1] != null;
        private float ResolvePump(float pressed, float angle, float angularVelocity, ref float pending, ref float remaining, float dt)
        {
            if (remaining > 0 && (remaining -= dt) <= 0) pending = 0;
            float impulse = 0;
            if (Mathf.Abs(pressed) > .01f)
            {
                // An opposite press just before the turning point should power the return trip,
                // rather than brake the last part of the outgoing trip.
                float frequency = Mathf.Sqrt(_settings.spring);
                float timeToTurn = frequency > .001f
                    ? Mathf.Atan2(Mathf.Abs(angularVelocity), frequency * Mathf.Abs(angle)) / frequency
                    : float.PositiveInfinity;
                bool approachingTurn = angle * angularVelocity > 0 && angle * pressed < 0 &&
                                       angularVelocity * pressed < 0 && timeToTurn <= _settings.reversalInputWindow;
                if (approachingTurn)
                {
                    pending += pressed;
                    remaining = _settings.reversalInputWindow + .2f;
                }
                else
                {
                    pending = 0;
                    remaining = 0;
                    impulse = pressed;
                }
            }
            if (Mathf.Abs(pending) > .01f && angularVelocity * pending >= -.01f)
            {
                impulse += pending;
                pending = 0;
                remaining = 0;
            }
            return impulse;
        }
        private Vector2 Step(Vector2 angles, ref Vector2 velocity, Vector2 impulse, float dt, float maxAngle)
        {
            velocity += impulse;
            velocity += (-angles * _settings.spring - velocity * _settings.damping) * dt;
            if (_settings.maxSwingSpeed > 0)
                velocity = Vector2.ClampMagnitude(velocity, _settings.maxSwingSpeed);
            Vector2 next = Vector2.ClampMagnitude(angles + velocity * dt, maxAngle);
            if (next.sqrMagnitude >= maxAngle * maxAngle && Vector2.Dot(velocity, next) > 0)
                velocity -= Vector2.Dot(velocity, next.normalized) * next.normalized;
            return next;
        }
        private bool TryFindBend(Vector2 angles, out Vector3 point, out float lowerLength)
        {
            point = Vector3.zero; lowerLength = 0;
            Vector3 origin = _fixedVisualTop;
            float length = _restGripOffset.magnitude;
            Vector3 direction = Quaternion.Euler(angles.x, 0, angles.y) * (_restGripOffset / length);
            int count = Physics.SphereCastNonAlloc(origin, _settings.bendProbeRadius, direction, _bendProbeHits, length, _settings.obstructionMask, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var hit = _bendProbeHits[i];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform) || hit.collider.GetComponentInParent<ClothBendSurface>() == null) continue;
                float upperLength = Vector3.Distance(origin, hit.point);
                if (upperLength < _settings.minBendSegmentLength || length - upperLength < _settings.minBendSegmentLength || upperLength >= nearest) continue;
                nearest = upperLength; point = hit.point; lowerLength = length - upperLength;
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
                Vector3 probe = from + cc.transform.TransformVector(cc.center);
                float radius = cc.radius * Mathf.Max(Mathf.Abs(cc.transform.lossyScale.x), Mathf.Abs(cc.transform.lossyScale.z));
                foreach (var hit in Physics.SphereCastAll(probe, radius, delta.normalized, delta.magnitude, _settings.obstructionMask, QueryTriggerInteraction.Ignore))
                    if (hit.collider.GetComponentInParent<StageActor>() == null && !hit.collider.transform.IsChildOf(transform)) return true;
            }
            return false;
        }
        private Vector3 Position(Vector2 primary, Vector2 secondary, int side)
        {
            Quaternion baseRotation = Quaternion.Euler(primary.x, 0, primary.y);
            if (!_bendActive) return _fixedVisualTop + baseRotation * (_restGripOffset + _gripOffsets[side]);
            Quaternion lowerRotation = Quaternion.Euler(secondary.x, 0, secondary.y) * baseRotation;
            return _bendPoint + lowerRotation * (_restGripOffset.normalized * _bendLength + _gripOffsets[side]);
        }
        private void Present(Vector2 primary, bool bent, Vector3 point, float lowerLength, Vector2 secondary)
        {
            Quaternion baseRotation = Quaternion.Euler(primary.x, 0, primary.y);
            Vector3 knot = bent ? point + Quaternion.Euler(secondary.x, 0, secondary.y) * baseRotation * (_restGripOffset.normalized * lowerLength)
                                 : _fixedVisualTop + baseRotation * _restGripOffset;
            if (_authoredVisual != null)
            {
                // Keep the authored mesh's top at its original world position, even when
                // the mesh top does not coincide with the prefab root's rotation pivot.
                _knot.localPosition = _authoredGripLocalPosition;
                transform.position = _authoredRootPosition;
                transform.rotation = baseRotation * _authoredRootRotation;
                transform.position += _fixedVisualTop - transform.TransformPoint(_visualTopLocal);
                _knot.position = knot;
            }
            else if (_cloth != null) SetSegment(_cloth, _fixedVisualTop, bent ? point : knot);
            if (_authoredVisual == null) _knot.position = knot;
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
                                        : Position(_angles + _velocity * .01f, _bendAngles, side) - Position(_angles, _bendAngles, side)) / .01f * _settings.releaseSpeed;
            player.ClearMovementReceiver(this); player.ClearTransport(this); player.Motor.SetMovementLock(this, false); actor.Abilities.SetInputLock(this, false);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority) { net.SetTransportAuthoritatively(false, actor.transform.position); net.LaunchAuthoritatively(velocity, player.Tuning.gravity, true); }
            else if (!Online) player.Motor.Launch(velocity, player.Tuning.gravity, true);
            _riders[side] = null;
            if (side == 0 && _riders[1] != null)
            {
                _riders[0] = _riders[1];
                _grabbedAt[0] = _grabbedAt[1];
                _riders[1] = null;
            }
            if (side == 0) { _heldInput = Vector2.zero; _drive = Vector2.zero; _pendingDrive = Vector2.zero; _pendingDriveTime = Vector2.zero; }
            Publish();
        }
        private void Update() { if (!Online && Authority) Tick(Time.deltaTime); else if (Online && !HasStateAuthority) Present(Angles, BendActive, BendPoint, BendLength, BendAngles); }
        private void OnDisable()
        {
            Cloths.Remove(this);
            if (Authority) { Detach(1); Detach(0); }
            if (_authoredVisual != null)
            {
                transform.position = _authoredRootPosition;
                transform.rotation = _authoredRootRotation;
                if (_knot != null) _knot.localPosition = _authoredGripLocalPosition;
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_knot == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _knot.position);
            Gizmos.DrawWireSphere(_knot.position, 0.6f);
            UnityEditor.Handles.Label(_knot.position + Vector3.up, "GripPoint / E");
        }
#endif
    }
}
