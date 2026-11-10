using System.Collections.Generic;
using Fusion;
using Herbalist.Abilities;
using UnityEngine;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    public enum ClothRiderAbility { Any, Leaf, Sap }

    [RequireComponent(typeof(NetworkObject))]
    public sealed class HangingCloth : NetworkBehaviour, IPlayerMovementReceiver
    {
        [SerializeField] private Transform _knot;
        [SerializeField] private Transform _cloth;
        [SerializeField] private Transform _lowerCloth;
        [SerializeField] private Transform _authoredVisual;
        [SerializeField] private HangingClothSettings _settings;
        [SerializeField] private bool _waterDriven;
        [SerializeField] private SapInjectionPort _loweringPort;
        [SerializeField, Min(0)] private float _raisedMeters = 2f;
        [SerializeField, Min(0)] private float _deployedGripRaise;
        [SerializeField, Min(.01f)] private float _lowerSeconds = 1f;
        [Header("Optional stage-one authored-art binding")]
        [SerializeField] private Transform _externalVisualRoot;
        [SerializeField] private Transform _waterHitProxy;
        [SerializeField] private Transform _externalAnchor;
        [SerializeField] private StageOneDocumentMechanism _deploymentMechanism;
        [SerializeField] private Transform _deployedGrip;
        [SerializeField, Min(1)] private float _swingDriveMultiplier = 1f;
        [SerializeField, Min(0)] private float _releaseGravityOverride;
        [SerializeField] private bool _singleUse;
        [SerializeField] private ClothRiderAbility _requiredAbility;
        [SerializeField, Range(1, 2)] private int _riderCapacity = 2;
        [SerializeField, Min(.01f)] private float _retractSeconds = 1.5f;
        [SerializeField] private Vector3[] _gripOffsets = { new(-.45f, -1, 0), new(.45f, -1, 0) };
        [SerializeField, Min(0), Tooltip("Rider origin below the knot in world units. Zero uses the legacy grip offsets.")]
        private float _knotHangHeight;
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
        [Networked] private Vector2 GrabRemaining { get; set; }
        [Networked] private NetworkBool Used { get; set; }
        [Networked] private float Retraction { get; set; }
        [Networked] private float Deployment { get; set; }
        private bool _used;
        private float _retraction;
        private float _deployment;
        private Vector3 _externalRestPosition, _externalRestScale;
        private Vector3 _hitProxyRestPosition, _hitProxyRestScale;
        private Quaternion _externalRestRotation;
        private Vector3 _anchorTop, _anchorRest;
        private Vector3 _anchorGrip;
        private Vector3 _carriedGripOffset;
        private Quaternion _anchorRotation;
        public bool WasUsed => Online ? Used : _used;
        public bool HasRetracted => (Online ? Retraction : _retraction) >= 1f;
        private float DeploymentProgress => Online && !HasStateAuthority ? Deployment : _deployment;
        private float GripExtension => _loweringPort == null ? 1f :
            Mathf.Max(.001f, 1f - Mathf.Lerp(_raisedMeters, _deployedGripRaise, DeploymentProgress) /
                Mathf.Max(.001f, _restGripOffset.magnitude));
        private bool CanBoard => (_loweringPort == null || DeploymentProgress >= 1f) &&
            (_deploymentMechanism == null || _deploymentMechanism.Progress >= 1f) && (!_singleUse || !WasUsed);
        private bool AllowsRider(StageActor actor) => _requiredAbility == ClothRiderAbility.Any ||
            (actor.Abilities is { Unlocked: true } abilities &&
             abilities.Kind == (_requiredAbility == ClothRiderAbility.Leaf ? PlayerAbilityKind.Leaf : PlayerAbilityKind.Sap));
        private Vector2 _angles, _velocity, _drive, _heldInput;
        private Vector2 _waterImpulse;
        public bool WaterDriven => _waterDriven;
        // Called only by the authoritative hose simulation, with its actual impact direction.
        public bool PushWithWater(Vector3 direction, float seconds)
        {
            if (!_waterDriven || (_loweringPort != null && DeploymentProgress < 1f) ||
                !isActiveAndEnabled || !Authority || _settings == null ||
                Herbalist.GameUI.GameplayPause.IsPaused || seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return false;
            Vector3 planar = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (planar.sqrMagnitude < .000001f) return true;
            planar.Normalize();
            _waterImpulse += new Vector2(-planar.z, planar.x) * (_settings.driveAcceleration * seconds);
            return true;
        }
        private Vector2 _pendingDrive, _pendingDriveTime;
        private Vector2 _bendAngles, _bendVelocity, _clothSize;
        private Vector3 _bendPoint;
        private float _bendLength;
        private bool _bendActive;
        private readonly RaycastHit[] _bendProbeHits = new RaycastHit[32];
        private readonly StageActor[] _riders = new StageActor[2];
        private Vector2 _grabRemaining;
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
            if (_waterHitProxy != null)
            {
                _hitProxyRestPosition = _waterHitProxy.localPosition;
                _hitProxyRestScale = _waterHitProxy.localScale;
            }
            if (_authoredVisual != null && _authoredVisual.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
            {
                Bounds bounds = meshFilter.sharedMesh.bounds;
                Vector3 top = new(bounds.center.x, bounds.max.y, bounds.center.z);
                _fixedVisualTop = _authoredVisual.TransformPoint(top);
                _visualTopLocal = transform.InverseTransformPoint(_fixedVisualTop);
            }
            else _fixedVisualTop = _authoredRootPosition;
            if (_externalVisualRoot != null)
            {
                _externalRestPosition = _externalVisualRoot.position;
                _externalRestRotation = _externalVisualRoot.rotation;
                _externalRestScale = _externalVisualRoot.localScale;
                if (_externalAnchor != null)
                {
                    _anchorTop = _externalAnchor.InverseTransformPoint(_fixedVisualTop);
                    _anchorRest = _externalAnchor.InverseTransformPoint(_externalRestPosition);
                    _anchorRotation = Quaternion.Inverse(_externalAnchor.rotation) * _externalRestRotation;
                }
            }
            _restGripOffset = _knot != null ? _knot.position - _fixedVisualTop : Vector3.zero;
            if (_restGripOffset.sqrMagnitude < .0001f)
                _restGripOffset = transform.TransformVector(Vector3.down * _length);
            if (_deploymentMechanism != null && _externalAnchor != null)
                _anchorGrip = _externalAnchor.InverseTransformVector(_restGripOffset);
            if (_loweringPort != null) Present(Vector2.zero, false, Vector3.zero, 0f, Vector2.zero);
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
                int side = cloth.RiderSide(actor);
                if (side >= 0)
                {
                    if (cloth.RemainingGrab(side) > 0f) return false;
                    point = cloth._knot.position;
                    release = true;
                    return true;
                }
                bool full = cloth.Online ? cloth.Rider0 >= 0 && (cloth._riderCapacity == 1 || cloth.Rider1 >= 0) :
                    cloth._riders[0] != null && (cloth._riderCapacity == 1 || cloth._riders[1] != null);
                if (full || !cloth.CanBoard || !cloth.AllowsRider(actor)) continue;
                float d = Vector3.Distance(grabPosition, cloth._knot.position);
                if (cloth._settings != null && d <= cloth._settings.grabRange && d < distance) { nearest = cloth; distance = d; }
            }
            if (nearest == null || actor.GetComponent<PlayerController>().Motor.MovementLocked) return false;
            point = nearest._knot.position;
            return true;
        }
        public override void Spawned() { if (HasStateAuthority) { Rider0 = Rider1 = -1; GrabRemaining = Vector2.zero; Angles = Vector2.zero; BendActive = false; BendPoint = Vector3.zero; BendLength = 0; BendAngles = Vector2.zero; Used = false; Retraction = 0; Deployment = 0; } }
        public override void FixedUpdateNetwork() { if (isActiveAndEnabled && HasStateAuthority) Tick(Runner.DeltaTime); }
        public static bool IsRiding(StageActor actor)
            => FindRiderCloth(actor, out float remaining) != null && remaining <= 0f;

        public static bool IsGrabbing(StageActor actor)
            => FindRiderCloth(actor, out float remaining) != null && remaining > 0f;

        public static bool TryGetHandGripPoint(StageActor actor, out Vector3 point, out float remaining)
        {
            var cloth = FindRiderCloth(actor, out remaining);
            if (cloth != null && cloth._knotHangHeight > 0f && cloth._knot != null)
            {
                point = cloth._knot.position;
                return true;
            }
            point = default;
            return false;
        }

        private int RiderSide(StageActor actor)
            => Online ? (Rider0 == actor.Slot ? 0 : Rider1 == actor.Slot ? 1 : -1) :
                        (_riders[0] == actor ? 0 : _riders[1] == actor ? 1 : -1);

        private float RemainingGrab(int side) => (Online ? GrabRemaining : _grabRemaining)[side];

        private static HangingCloth FindRiderCloth(StageActor actor, out float remaining)
        {
            remaining = 0f;
            if (actor == null || !actor.Available) return null;
            foreach (var cloth in Cloths)
            {
                int side = cloth.RiderSide(actor);
                if (side >= 0)
                {
                    remaining = cloth.RemainingGrab(side);
                    return cloth;
                }
            }
            return null;
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
                    if (cloth._grabRemaining[i] <= 0f && Time.time - cloth._grabbedAt[i] >= cloth._settings.minimumHoldSeconds)
                    {
                        cloth.Detach(i);
                        feedback = cloth._releaseText;
                    }
                    return true;
                }
                if (cloth._knot == null || !cloth.CanBoard || !cloth.AllowsRider(actor)) continue;
                float d = Vector3.Distance(grabPosition, cloth._knot.position);
                if (cloth._settings != null && d <= cloth._settings.grabRange && d < distance) { nearest = cloth; distance = d; }
            }
            if (nearest == null) return false;
            var player = actor.GetComponent<PlayerController>();
            if (player.Motor.MovementLocked) return false;
            for (int i = 0; i < nearest._riderCapacity; i++)
                if (nearest._riders[i] == null)
                {
                    nearest._riders[i] = actor;
                    nearest._grabRemaining[i] = nearest._knotHangHeight > 0f
                        ? actor.GetComponent<SodamAnimationPresentation>()?.Profile?.Find(SodamPose.ClothGrab)?.ContactTime ?? 0f
                        : 0f;
                    if (nearest._grabRemaining[i] <= 0f) nearest.Attach(i);
                    nearest._lastPoints[i] = actor.transform.position;
                    if (i == 0) { nearest._heldInput = Vector2.zero; nearest._drive = Vector2.zero; nearest._pendingDrive = Vector2.zero; nearest._pendingDriveTime = Vector2.zero; }
                    player.SetMovementReceiver(nearest); player.Motor.SetMovementLock(nearest, true);
                    actor.Abilities.Sap?.Cancel(); actor.Abilities.SetInputLock(nearest, true);
                    nearest.Publish();
                    feedback = nearest._grabRemaining[i] > 0f ? "매듭을 잡는 중입니다." : nearest._grabText;
                    return true;
                }
            feedback = nearest._fullText; return true;
        }
        public void ReceiveMovement(PlayerController player, Vector2 move)
        {
            if (_waterDriven) return;
            if (!Authority || _riders[0] == null || _grabRemaining[0] > 0f || _riders[0].GetComponent<PlayerController>() != player) return;
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
            if (_loweringPort != null)
            {
                float injected = _loweringPort.Consume();
                if (injected > 0f || _deployment > 0f)
                    _deployment = Mathf.MoveTowards(_deployment, 1f, dt / _lowerSeconds);
                if (Online) Deployment = _deployment;
            }
            RefreshAnchor();
            for (int i = 0; i < 2; i++) if (_riders[i] != null && !_riders[i].Available) Detach(i);
            if (_singleUse && _used && !HasRider) _retraction = Mathf.MoveTowards(_retraction, 1, dt / _retractSeconds);
            Vector2 angles = _bendActive ? _bendAngles : _angles;
            Vector2 velocityAtInput = _bendActive ? _bendVelocity : _velocity;
            Vector2 pump = new(
                ResolvePump(_drive.x, angles.x, velocityAtInput.x, ref _pendingDrive.x, ref _pendingDriveTime.x, dt),
                ResolvePump(_drive.y, angles.y, velocityAtInput.y, ref _pendingDrive.y, ref _pendingDriveTime.y, dt));
            Vector2 impulse = _waterDriven ? _waterImpulse :
                pump * (_settings.driveAcceleration * _settings.pumpImpulseSeconds * _swingDriveMultiplier);
            _waterImpulse = Vector2.zero;
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
                if (HasAttachedRider && velocity.sqrMagnitude > .01f && TryFindBend(next, out var point, out var length) && !RiderBlocked(next, Vector2.zero))
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
                if (_grabRemaining[i] > 0f)
                {
                    _grabRemaining[i] = Mathf.Max(0f, _grabRemaining[i] - dt);
                    if (_grabRemaining[i] > 0f) continue;
                    Attach(i);
                }
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
        public void RestoreAfterRespawn()
        {
            if (!isActiveAndEnabled || !Authority || (!_used && !HasRider)) return;
            Detach(1);
            Detach(0);
            _used = false;
            _retraction = 0f;
            _angles = _velocity = _bendAngles = _bendVelocity = _waterImpulse = Vector2.zero;
            _heldInput = _drive = _pendingDrive = _pendingDriveTime = Vector2.zero;
            _bendActive = false;
            _bendPoint = Vector3.zero;
            _bendLength = 0f;
            Publish();
            Present(Vector2.zero, false, Vector3.zero, 0f, Vector2.zero);
        }
        private bool HasAttachedRider => (_riders[0] != null && _grabRemaining[0] <= 0f) ||
                                         (_riders[1] != null && _grabRemaining[1] <= 0f);
        private void Attach(int side)
        {
            _used = true;
            _grabbedAt[side] = Time.time;
        }
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
                velocity = Vector2.ClampMagnitude(velocity, _settings.maxSwingSpeed * _swingDriveMultiplier);
            Vector2 next = Vector2.ClampMagnitude(angles + velocity * dt, maxAngle);
            if (next.sqrMagnitude >= maxAngle * maxAngle && Vector2.Dot(velocity, next) > 0)
                velocity -= Vector2.Dot(velocity, next.normalized) * next.normalized;
            return next;
        }
        private bool TryFindBend(Vector2 angles, out Vector3 point, out float lowerLength)
        {
            point = Vector3.zero; lowerLength = 0;
            Vector3 origin = _fixedVisualTop;
            float length = _restGripOffset.magnitude * GripExtension;
            Vector3 direction = Quaternion.Euler(angles.x, 0, angles.y) * _restGripOffset.normalized;
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
                if (_riders[i] == null || _grabRemaining[i] > 0f) continue;
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
            Quaternion rotation = _bendActive ? Quaternion.Euler(secondary.x, 0, secondary.y) * baseRotation : baseRotation;
            Vector3 top = _bendActive ? _bendPoint : _fixedVisualTop;
            Vector3 offset = _bendActive ? _restGripOffset.normalized * _bendLength : _restGripOffset * GripExtension;
            if (_knotHangHeight > 0f)
                return top + rotation * offset + Vector3.down * _knotHangHeight;
            return top + rotation * (offset + _gripOffsets[side]);
        }
        private void Present(Vector2 primary, bool bent, Vector3 point, float lowerLength, Vector2 secondary)
        {
            RefreshAnchor();
            Quaternion baseRotation = Quaternion.Euler(primary.x, 0, primary.y);
            float extension = GripExtension;
            Vector3 knot = bent ? point + Quaternion.Euler(secondary.x, 0, secondary.y) * baseRotation * (_restGripOffset.normalized * lowerLength)
                                 : _fixedVisualTop + baseRotation * _restGripOffset * extension;
            if (_externalVisualRoot != null)
            {
                transform.SetPositionAndRotation(_fixedVisualTop, baseRotation);
                float scale = Mathf.Max(.001f, (1f - (Online && !HasStateAuthority ? Retraction : _retraction)) * extension);
                var visualRotation = baseRotation;
                if (_deploymentMechanism != null && _carriedGripOffset.sqrMagnitude > .0001f)
                {
                    visualRotation *= Quaternion.FromToRotation(_carriedGripOffset, _restGripOffset);
                    scale *= _restGripOffset.magnitude / _carriedGripOffset.magnitude;
                }
                _externalVisualRoot.rotation = visualRotation * _externalRestRotation;
                _externalVisualRoot.position = _fixedVisualTop + visualRotation * (_externalRestPosition - _fixedVisualTop) * scale;
                _externalVisualRoot.localScale = _externalRestScale * scale;
                ScaleWaterHitProxy(scale);
                _knot.position = knot;
            }
            else if (_authoredVisual != null)
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
            if (_authoredVisual == null && _externalVisualRoot == null) _knot.position = knot;
            if (_lowerCloth == null) return;
            if (_lowerCloth.gameObject.activeSelf != bent) _lowerCloth.gameObject.SetActive(bent);
            if (bent) SetSegment(_lowerCloth, point, knot);
        }
        private void RefreshAnchor()
        {
            if (_externalAnchor == null) return;
            _fixedVisualTop = _externalAnchor.TransformPoint(_anchorTop);
            _externalRestPosition = _externalAnchor.TransformPoint(_anchorRest);
            _externalRestRotation = _externalAnchor.rotation * _anchorRotation;
            if (_deploymentMechanism != null && _deployedGrip != null)
            {
                _carriedGripOffset = _externalAnchor.TransformVector(_anchorGrip);
                _restGripOffset = Vector3.Lerp(_carriedGripOffset, _deployedGrip.position - _fixedVisualTop,
                    _deploymentMechanism.Progress);
            }
        }
        private void ScaleWaterHitProxy(float scale)
        {
            if (_waterHitProxy == null) return;
            _waterHitProxy.localPosition = _hitProxyRestPosition * scale;
            _waterHitProxy.localScale = _hitProxyRestScale * scale;
        }
        private void SetSegment(Transform segment, Vector3 start, Vector3 end)
        {
            segment.position = (start + end) * .5f;
            segment.rotation = Quaternion.FromToRotation(Vector3.down, end - start);
            segment.localScale = new Vector3(_clothSize.x, Vector3.Distance(start, end), _clothSize.y);
        }
        private void Publish() { if (Online) { Angles = _angles; BendActive = _bendActive; BendPoint = _bendPoint; BendLength = _bendLength; BendAngles = _bendAngles; Rider0 = _riders[0] != null ? _riders[0].Slot : -1; Rider1 = _riders[1] != null ? _riders[1].Slot : -1; GrabRemaining = _grabRemaining; Used = _used; Retraction = _retraction; } }
        private void Detach(int side)
        {
            var actor = _riders[side]; if (actor == null) return;
            var player = actor.GetComponent<PlayerController>();
            var velocity = (_bendActive ? Position(_angles, _bendAngles + _bendVelocity * .01f, side) - Position(_angles, _bendAngles, side)
                                        : Position(_angles + _velocity * .01f, _bendAngles, side) - Position(_angles, _bendAngles, side)) / .01f * _settings.releaseSpeed;
            player.ClearMovementReceiver(this); player.ClearTransport(this); player.Motor.SetMovementLock(this, false); actor.Abilities.SetInputLock(this, false);
            if (_grabRemaining[side] <= 0f)
            {
                var net = actor.GetComponent<NetworkPlayer>();
                float gravity = _releaseGravityOverride > 0f ? _releaseGravityOverride : player.Tuning.gravity;
                if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority) { net.SetTransportAuthoritatively(false, actor.transform.position); net.LaunchAuthoritatively(velocity, gravity, true); }
                else if (!Online) player.Motor.Launch(velocity, gravity, true);
            }
            _riders[side] = null;
            _grabRemaining[side] = 0f;
            if (side == 0 && _riders[1] != null)
            {
                _riders[0] = _riders[1];
                _grabbedAt[0] = _grabbedAt[1];
                _grabRemaining[0] = _grabRemaining[1];
                _grabRemaining[1] = 0f;
                _riders[1] = null;
            }
            if (side == 0) { _heldInput = Vector2.zero; _drive = Vector2.zero; _pendingDrive = Vector2.zero; _pendingDriveTime = Vector2.zero; }
            Publish();
        }
        private void Update() { if (!Online && Authority) Tick(Time.deltaTime); else if (Online && !HasStateAuthority) Present(Angles, BendActive, BendPoint, BendLength, BendAngles); }
        private void OnDisable()
        {
            _waterImpulse = Vector2.zero;
            Cloths.Remove(this);
            if (Authority) { Detach(1); Detach(0); }
            if (_externalVisualRoot != null)
            {
                _externalVisualRoot.SetPositionAndRotation(_externalRestPosition, _externalRestRotation);
                _externalVisualRoot.localScale = _externalRestScale;
                ScaleWaterHitProxy(1f);
            }
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
