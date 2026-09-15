using System;
using System.Collections.Generic;
using Herbalist.Player;
using UnityEngine;
namespace Herbalist.Abilities
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class SapControlAbility : MonoBehaviour
    {
        [SerializeField] private SapAbilitySettings settings;
        private readonly List<SapDeposit> deposits = new();
        private Func<SapDeposit> create;
        private Action<SapDeposit> destroy;
        private PlayerController player;
        private SapSource source;
        private Vector3 controlOrigin;
        private SapDeposit held;
        private bool externalTick;
        public SapAbilitySettings Settings => settings;
        public bool Controlling { get; private set; }
        public bool CanPlace { get; private set; }
        public int ActiveCount => deposits.Count;
        public SapDeposit Held => held;
        private void Awake() => player = GetComponent<PlayerController>();
        public void Configure(Func<SapDeposit> factory, Action<SapDeposit> release, bool network)
        { create = factory; destroy = release; externalTick = network; }
        public void Toggle()
        {
            if (Controlling) { Cancel(); return; }
            if (settings == null || create == null || deposits.Count >= settings.capacity) return;
            source = SapSource.FindNearest(transform.position + settings.extractionProbeOffset, settings.extractionRange, settings.radius + settings.surfaceOffset, out controlOrigin);
            if (source == null || !source.TryExtract()) return;
            held = create();
            if (held == null) { source.Refund(); source = null; return; }
            deposits.Add(held); held.Initialize(settings, controlOrigin, Remove, externalTick);
            Controlling = true; player.Motor.SetMovementLock(this, true);
        }
        public void Tick(Ray aim, bool use, float dt)
        {
            CanPlace = false;
            if (!Controlling) return;
            if (held == null || source == null || !source.isActiveAndEnabled ||
                Vector3.Distance(transform.position, controlOrigin) > settings.controlRange)
            { Cancel(); return; }
            Vector3 desired = aim.GetPoint(settings.freeAimDistance);
            SapReceiver receiver = null; Vector3 placement = default, normal = default;
            bool valid = false;
            if (Physics.Raycast(aim, out var hit, settings.controlRange + Vector3.Distance(aim.origin, controlOrigin), settings.collisionMask, QueryTriggerInteraction.Ignore))
            {
                desired = hit.point + hit.normal * (settings.radius + settings.surfaceOffset);
                receiver = hit.collider.GetComponentInParent<SapReceiver>();
                valid = receiver != null && receiver.TryPlacement(hit, settings.surfaceOffset, out placement, out normal);
                valid &= Vector3.Distance(controlOrigin, placement) <= settings.controlRange;
            }
            desired = controlOrigin + Vector3.ClampMagnitude(desired - controlOrigin, settings.controlRange);
            Vector3 next = Vector3.MoveTowards(held.transform.position, desired, settings.moveSpeed * dt);
            Vector3 delta = next - held.transform.position;
            if (delta.sqrMagnitude > 0.000001f && Physics.SphereCast(held.transform.position, settings.radius, delta.normalized, out var block, delta.magnitude, settings.collisionMask, QueryTriggerInteraction.Ignore))
                next = held.transform.position + delta.normalized * Mathf.Max(0, block.distance - settings.surfaceOffset);
            held.transform.position = next;
            CanPlace = valid && Vector3.Distance(next, placement) <= settings.radius + settings.placementTolerance;
            if (!use || !CanPlace) return;
            var deposited = held;
            held = null; EndControl();
            if (deposited.RefreshAt(receiver, placement)) deposited.Finish();
            else deposited.Place(receiver, placement, normal);
        }
        public void Cancel()
        {
            if (held != null)
            {
                if (source != null) source.Refund();
                var cancelled = held; held = null; cancelled.Finish();
            }
            EndControl();
        }
        private void EndControl()
        { source = null; Controlling = false; CanPlace = false; if (player != null) player.Motor.SetMovementLock(this, false); }
        private void Remove(SapDeposit sap) { deposits.Remove(sap); destroy?.Invoke(sap); }
        public void Clear() { Cancel(); foreach (var sap in deposits.ToArray()) if (sap != null) sap.Finish(); deposits.Clear(); }
        public void ApplyReplica(bool controlling, bool canPlace) { Controlling = controlling; CanPlace = canPlace; }
        private void OnDisable() { Clear(); }
    }
}
