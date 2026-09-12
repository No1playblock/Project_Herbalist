using System;
using UnityEngine;
namespace Herbalist.Abilities
{
    public sealed class LeafProjectile : MonoBehaviour
    {
        [SerializeField] private GameObject platformVisual;
        [SerializeField] private GameObject pinVisual;
        [SerializeField] private Collider platformCollider;
        [SerializeField] private Collider pinCollider;
        private LeafAbilitySettings settings;
        private Transform owner;
        private Action<LeafProjectile> release;
        private Vector3 start, destination, curveRight, localPosition;
        private Quaternion localRotation;
        private float progress, duration, remaining;
        private bool initialized, externalTick;
        public LeafMode Mode { get; private set; }
        public LeafState State { get; private set; } = LeafState.Complete;
        public LeafInstallTarget Target { get; private set; }
        public float InstalledAt { get; private set; }
        public bool Installed => State == LeafState.Installed || State == LeafState.Bound;
        public void Initialize(LeafAbilitySettings config, Transform returningOwner, LeafMode mode, Vector3 origin, Vector3 aim, Action<LeafProjectile> onRelease, bool network)
        {
            settings = config; owner = returningOwner; Mode = mode; start = origin; destination = aim;
            release = onRelease; externalTick = network; initialized = true; State = LeafState.Flying;
            transform.position = origin;
            var direction = destination - start;
            if (Mode == LeafMode.Pin) transform.rotation = HorizontalRotation(direction);
            duration = Mathf.Max(0.01f, direction.magnitude / settings.flightSpeed);
            curveRight = Vector3.Cross(Vector3.up, direction.normalized);
            if (curveRight.sqrMagnitude < 0.001f) curveRight = Vector3.right;
            curveRight.Normalize(); RefreshVisuals();
        }
        private void Update() { if (initialized && !externalTick) Tick(Time.deltaTime); }
        public void Tick(float dt)
        {
            if (!initialized || State == LeafState.Complete) return;
            if (owner == null) { Finish(); return; }
            if (State == LeafState.Flying)
            {
                progress = Mathf.Min(1, progress + dt / duration);
                Vector3 next = Vector3.Lerp(start, destination, progress) + curveRight * (Mathf.Sin(progress * Mathf.PI) * settings.curveWidth);
                Vector3 delta = next - transform.position;
                if (delta.sqrMagnitude > 0.000001f && Physics.SphereCast(transform.position, settings.collisionRadius, delta.normalized, out var hit, delta.magnitude, settings.hitMask, QueryTriggerInteraction.Ignore))
                {
                    var target = hit.collider.GetComponentInParent<LeafInstallTarget>();
                    if (target != null && target.Accepts(Mode)) Install(target, hit.point, hit.normal, delta);
                    else BeginReturn();
                }
                else
                {
                    transform.position = next;
                    if (delta.sqrMagnitude > 0.000001f) transform.rotation = Mode == LeafMode.Pin ? HorizontalRotation(delta) : Quaternion.LookRotation(delta.normalized, curveRight);
                    if (progress >= 1) BeginReturn();
                }
            }
            else if (Installed)
            {
                if (Target == null || !Target.isActiveAndEnabled) { BeginReturn(); return; }
                var rotation = Target.transform.rotation * localRotation;
                if (Mode == LeafMode.Pin) rotation = HorizontalRotation(rotation * Vector3.forward);
                transform.SetPositionAndRotation(Target.transform.TransformPoint(localPosition), rotation);
                if (State == LeafState.Installed) { remaining -= dt; if (remaining <= 0) BeginReturn(); }
            }
            else if (State == LeafState.Returning)
            {
                Vector3 catchPoint = owner.position + Vector3.up * settings.launchOffset.y;
                transform.position = Vector3.MoveTowards(transform.position, catchPoint, settings.returnSpeed * dt);
                if (Vector3.Distance(transform.position, catchPoint) <= settings.catchDistance) Finish();
            }
        }
        private void Install(LeafInstallTarget target, Vector3 hit, Vector3 normal, Vector3 heading)
        {
            Target = target;
            transform.SetPositionAndRotation(target.Position(hit, normal, settings.surfaceOffset), target.Rotation(Mode, normal, heading));
            localPosition = target.transform.InverseTransformPoint(transform.position);
            localRotation = Quaternion.Inverse(target.transform.rotation) * transform.rotation;
            State = target.TryBind(this) ? LeafState.Bound : LeafState.Installed;
            remaining = settings.lifetime; InstalledAt = Time.time;
            target.Attach(this, State == LeafState.Bound); RefreshVisuals();
        }
        public void BeginReturn()
        {
            if (State == LeafState.Returning || State == LeafState.Complete) return;
            Detach(); State = LeafState.Returning; RefreshVisuals();
        }
        public void Finish()
        {
            if (State == LeafState.Complete) return;
            Detach(); State = LeafState.Complete; RefreshVisuals(); release?.Invoke(this);
        }
        private void Detach() { if (Target != null) Target.Detach(this); Target = null; }
        public void ApplyReplica(LeafMode mode, LeafState state, int targetId)
        {
            var target = LeafInstallTarget.Find(targetId);
            if (Target != target || State != state) Detach();
            Mode = mode; State = state; Target = target;
            if (Installed && Target != null) Target.Attach(this, state == LeafState.Bound);
            RefreshVisuals();
        }
        private void RefreshVisuals()
        {
            if (platformVisual != null) platformVisual.SetActive(Mode == LeafMode.Platform && State != LeafState.Complete);
            if (pinVisual != null) pinVisual.SetActive(Mode == LeafMode.Pin && State != LeafState.Complete);
            if (platformCollider != null) platformCollider.enabled = Mode == LeafMode.Platform && Installed;
            if (pinCollider != null) pinCollider.enabled = Mode == LeafMode.Pin && Installed;
        }
        // Pin leaves stay flat even when their trajectory rises, falls or curves.
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
