using System;
using UnityEngine;
using UnityEngine.Serialization;
namespace Herbalist.Abilities
{
    public sealed class LeafProjectile : MonoBehaviour
    {
        [FormerlySerializedAs("pinVisual"), SerializeField] private GameObject _visual;
        [FormerlySerializedAs("pinCollider"), SerializeField] private Collider _surface;
        [FormerlySerializedAs("pinAttachmentTip"), SerializeField] private Transform _attachmentTip;
        [SerializeField] private LeafAbilitySettings _replicaSettings;
        private LeafAbilitySettings settings;
        private Transform owner;
        private Transform _returnFrame;
        private Action<LeafProjectile> release;
        private Vector3 start, destination, curveRight, localPosition, localContactPoint;
        private Quaternion localRotation;
        private float progress, duration, remaining;
        private Vector3 _turnStart, _turnForward, _turnSide;
        private float _turnProgress, _turnDuration;
        private float _returnElapsed, _flightSpinAngle;
        private bool _turningAfterMiss;
        private bool initialized, externalTick;
        private Vector3 _installedVisualScale;
        private bool _visualScaleCaptured;
        private float _appliedVisualScaleMultiplier = float.NaN;
        private Quaternion _baseVisualRotation;
        public LeafState State { get; private set; } = LeafState.Complete;
        public LeafInstallTarget Target { get; private set; }
        public float InstalledAt { get; private set; }
        public Vector3 ContactPoint => Target != null ? Target.transform.TransformPoint(localContactPoint) : transform.position;
        public bool Installed => State == LeafState.Installed;
        public void Initialize(LeafAbilitySettings config, Transform returningOwner, Vector3 origin, Vector3 aim, Action<LeafProjectile> onRelease, bool network, Transform returnFrame = null)
        {
            settings = config; owner = returningOwner; start = origin; destination = aim;
            _returnFrame = returnFrame != null ? returnFrame : returningOwner;
            release = onRelease; externalTick = network; initialized = true; State = LeafState.Flying;
            _turningAfterMiss = false;
            _flightSpinAngle = 0f;
            transform.position = origin;
            var direction = destination - start;
            transform.rotation = HorizontalRotation(direction);
            duration = Mathf.Max(0.01f, direction.magnitude / settings.flightSpeed);
            curveRight = Vector3.Cross(Vector3.up, direction.normalized);
            if (curveRight.sqrMagnitude < 0.001f) curveRight = Vector3.right;
            curveRight.Normalize(); RefreshVisuals();
        }
        private void Update()
        {
            if (settings != null && State != LeafState.Complete)
                SetVisualScale(Installed ? settings.installedScaleMultiplier : settings.flyingVisualScaleMultiplier);
            if (initialized && !externalTick) Tick(Time.deltaTime);
        }
        public void Tick(float dt)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused) return;
            if (!initialized || State == LeafState.Complete) return;
            if (owner == null || _returnFrame == null) { Finish(); return; }
            if (State == LeafState.Flying)
            {
                AdvanceSpinVisual(dt);
                progress = Mathf.Min(1, progress + dt / duration);
                Vector3 next = Vector3.Lerp(start, destination, progress) + curveRight * (Mathf.Sin(progress * Mathf.PI) * settings.curveWidth);
                Vector3 delta = next - transform.position;
                if (delta.sqrMagnitude > 0.000001f && Physics.SphereCast(transform.position, settings.collisionRadius, delta.normalized, out var hit, delta.magnitude, settings.hitMask, QueryTriggerInteraction.Ignore))
                {
                    var target = hit.collider.GetComponentInParent<LeafInstallTarget>();
                    if (target != null && target.CanInstall) Install(target, hit.point, hit.normal, delta);
                    else BeginMissReturn(hit.normal);
                }
                else
                {
                    transform.position = next;
                    if (delta.sqrMagnitude > 0.000001f) transform.rotation = HorizontalRotation(delta);
                    if (progress >= 1) BeginMissReturn();
                }
            }
            else if (Installed)
            {
                if (Target == null || !Target.isActiveAndEnabled) { BeginReturn(); return; }
                var rotation = Target.transform.rotation * localRotation;
                if (!Target.ExactSocketPlacement && !Target.PerpendicularToSurfaceOnInstall) rotation = HorizontalRotation(rotation * Vector3.forward);
                if (Target.ExactSocketPlacement)
                    transform.SetPositionAndRotation(Target.SocketPosition, Target.SocketRotation);
                else transform.SetPositionAndRotation(Target.transform.TransformPoint(localPosition), rotation);
                if (State == LeafState.Installed && !settings.permanentInstallation) { remaining -= dt; if (remaining <= 0) BeginReturn(); }
            }
            else if (State == LeafState.Returning)
            {
                AdvanceSpinVisual(dt);
                _returnElapsed += dt;
                if (_turningAfterMiss)
                {
                    float radius = settings.missTurnRadius;
                    _turnProgress = Mathf.Min(1f, _turnProgress + dt / _turnDuration);
                    float t = _turnProgress;
                    float inverse = 1f - t;
                    Vector3 control1 = _turnStart + _turnForward * radius;
                    Vector3 control2 = control1 + _turnSide * (2f * radius);
                    Vector3 end = _turnStart + _turnSide * (2f * radius);
                    Vector3 turnNext = inverse * inverse * inverse * _turnStart +
                                   3f * inverse * inverse * t * control1 +
                                   3f * inverse * t * t * control2 + t * t * t * end;
                    Vector3 turnDelta = turnNext - transform.position;
                    transform.position = turnNext;
                    if (turnDelta.sqrMagnitude > 0.000001f) transform.rotation = HorizontalRotation(turnDelta);
                    if (_turnProgress >= 1f) _turningAfterMiss = false;
                    return;
                }
                Vector3 catchPoint = _returnFrame.TransformPoint(settings.launchOffset);
                float timeLeft = Mathf.Max(0.0001f, settings.maxReturnDuration - _returnElapsed);
                float speed = Mathf.Max(settings.returnSpeed, Vector3.Distance(transform.position, catchPoint) / timeLeft);
                Vector3 next = Vector3.MoveTowards(transform.position, catchPoint, speed * dt);
                Vector3 delta = next - transform.position;
                transform.position = next;
                if (delta.sqrMagnitude > 0.000001f) transform.rotation = HorizontalRotation(delta);
                if (Vector3.Distance(transform.position, catchPoint) <= settings.catchDistance) Finish();
            }
        }
        public bool Supports(CharacterController character, float tolerance)
        {
            var surface = _surface;
            if (!Installed || surface == null || !surface.enabled || character == null ||
                Physics.GetIgnoreCollision(character, surface)) return false;
            var feet = character.transform.TransformPoint(character.center);
            feet.y -= character.height * Mathf.Abs(character.transform.lossyScale.y) * .5f;
            return surface.Raycast(new Ray(feet + Vector3.up * tolerance, Vector3.down), out var hit, tolerance * 2) &&
                hit.point.y <= feet.y + character.skinWidth;
        }
        private void Install(LeafInstallTarget target, Vector3 hit, Vector3 normal, Vector3 heading)
        {
            Target = target;
            localContactPoint = target.transform.InverseTransformPoint(hit);
            transform.SetPositionAndRotation(hit, target.Rotation(normal, heading));
            SetVisualScale(settings.installedScaleMultiplier);
            if (_visual != null) _visual.transform.localRotation = _baseVisualRotation;
            transform.position = target.Position(hit, normal, 0) + (target.ExactSocketPlacement ? Vector3.zero : TipPlacementOffset(normal));
            localPosition = target.transform.InverseTransformPoint(transform.position);
            localRotation = Quaternion.Inverse(target.transform.rotation) * transform.rotation;
            State = LeafState.Installed;
            remaining = settings.lifetime; InstalledAt = Time.time;
            target.Attach(this); RefreshVisuals();
        }
        private Vector3 TipPlacementOffset(Vector3 normal)
        {
            var visual = _visual;
            var filter = visual != null ? visual.GetComponentInChildren<MeshFilter>() : null;
            if (filter == null || filter.sharedMesh == null) return normal * settings.surfaceOffset;
            var mesh = filter.sharedMesh;
            Vector3[] points;
            if (mesh.isReadable) points = mesh.vertices;
            else
            {
                // Conservative fallback for imported meshes without CPU-readable vertices.
                var bounds = mesh.bounds; points = new Vector3[8];
                for (int i = 0; i < points.Length; i++)
                    points[i] = bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            }
            float minimum = float.PositiveInfinity, maximum = float.NegativeInfinity;
            Vector3 contactOffset = Vector3.zero;
            foreach (var point in points)
            {
                Vector3 offset = filter.transform.TransformPoint(point) - transform.position;
                float projection = Vector3.Dot(offset, normal);
                if (projection < minimum) { minimum = projection; contactOffset = offset; }
                maximum = Mathf.Max(maximum, projection);
            }
            if (points.Length == 0) return normal * settings.surfaceOffset;
            // Thin faces must not disappear into floors when the configured depth exceeds their thickness.
            float depth = Mathf.Min(settings.tipEmbedDepth, (maximum - minimum) * settings.maxEmbedFraction);
            // The authored long-axis tip is the anchor even for oblique shots.
            // The broad sides of a rounded leaf may intersect the wall at steep angles.
            if (_attachmentTip != null)
                contactOffset = _attachmentTip.position - transform.position;
            return -contactOffset - normal * depth;
        }
        public void BeginReturn()
        {
            if (State == LeafState.Returning || State == LeafState.Complete) return;
            _turningAfterMiss = false;
            Detach(); State = LeafState.Returning; _returnElapsed = 0f; RefreshVisuals();
        }
        private void BeginMissReturn(Vector3 obstacleNormal = default)
        {
            Vector3 forward = Vector3.ProjectOnPlane(destination - start, Vector3.up);
            Vector3 awayFromObstacle = Vector3.ProjectOnPlane(obstacleNormal, Vector3.up);
            if (awayFromObstacle.sqrMagnitude > 0.000001f) forward = awayFromObstacle;
            if (forward.sqrMagnitude < 0.000001f) forward = transform.forward;
            forward.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, forward);
            Vector3 toOwner = Vector3.ProjectOnPlane(owner.position - transform.position, Vector3.up);
            if (Vector3.Dot(side, toOwner) < 0f) side = -side;
            BeginReturn();
            _turnStart = transform.position;
            _turnForward = forward;
            _turnSide = side.normalized;
            _turnProgress = 0f;
            _turnDuration = Mathf.Clamp(3.6f * settings.missTurnRadius / settings.returnSpeed,
                0.01f, settings.maxReturnDuration * 0.3f);
            _turningAfterMiss = true;
        }
        public void Finish()
        {
            if (State == LeafState.Complete) return;
            Detach(); State = LeafState.Complete; RefreshVisuals(); release?.Invoke(this);
        }
        private void Detach() { if (Target != null) Target.Detach(this); Target = null; }
        public void ApplyReplica(LeafState state, int targetId)
        {
            var rules = Herbalist.Levels.StageLevel.Instance != null
                ? Herbalist.Levels.StageLevel.Instance.abilityRules
                : null;
            settings = rules != null && rules.leaf != null ? rules.leaf : _replicaSettings;
            var target = LeafInstallTarget.Find(targetId);
            if (Target != target || State != state) Detach();
            State = state; Target = target;
            if (Installed && Target != null) Target.Attach(this);
            RefreshVisuals();
        }
        private void RefreshVisuals()
        {
            var rules = Herbalist.Levels.StageLevel.Instance != null ? Herbalist.Levels.StageLevel.Instance.abilityRules : null;
            if (rules != null)
            {
                int layer = Installed && Target != null ? Target.gameObject.layer : 0;
                foreach (var renderer in GetComponentsInChildren<Renderer>(true)) renderer.gameObject.layer = layer;
            }
            if (_visual != null) _visual.SetActive(State != LeafState.Complete);
            if (_visual != null && settings != null)
                SetVisualScale(Installed ? settings.installedScaleMultiplier : settings.flyingVisualScaleMultiplier);
            if (_visual != null && (Installed || State == LeafState.Complete) && _visualScaleCaptured)
                _visual.transform.localRotation = _baseVisualRotation;
            if (_surface != null) _surface.enabled = Installed;
        }
        private void SetVisualScale(float multiplier)
        {
            if (_visual == null) return;
            if (!_visualScaleCaptured)
            {
                _installedVisualScale = _visual.transform.localScale;
                _baseVisualRotation = _visual.transform.localRotation;
                _visualScaleCaptured = true;
            }
            if (Mathf.Approximately(_appliedVisualScaleMultiplier, multiplier)) return;
            _visual.transform.localScale = _installedVisualScale * multiplier;
            _appliedVisualScaleMultiplier = multiplier;
        }
        public void AdvanceSpinVisual(float dt)
        {
            if ((State != LeafState.Flying && State != LeafState.Returning) ||
                _visual == null || settings == null || dt <= 0f || Herbalist.GameUI.GameplayPause.IsPaused) return;
            if (!_visualScaleCaptured) SetVisualScale(settings.flyingVisualScaleMultiplier);
            _flightSpinAngle = Mathf.Repeat(_flightSpinAngle + 360f * settings.flightSpinRevolutionsPerSecond * dt, 360f);
            _visual.transform.localRotation = Quaternion.AngleAxis(_flightSpinAngle, Vector3.up) * _baseVisualRotation;
        }
        // Leaves stay flat even when their trajectory rises, falls or curves.
        private Quaternion HorizontalRotation(Vector3 direction)
        {
            Vector3 forward = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (forward.sqrMagnitude < 0.000001f) forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.000001f) forward = Vector3.forward;
            return Quaternion.LookRotation(forward, Vector3.up);
        }
        private void OnDestroy() => Detach();
    }
}
