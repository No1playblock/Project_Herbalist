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
        private SapDeposit held;
        private bool externalTick, replicaReady;
        private float sourceRefresh;
        private SapHoseSprayer hose;
        public SapAbilitySettings Settings => settings;
        public bool Controlling { get; private set; }
        public bool CanPlace { get; private set; }
        public bool Ready => held != null ? held.State == SapState.Controlled : replicaReady;
        public int ActiveCount => deposits.Count;
        public SapDeposit Held => held;
        private void Awake() { player = GetComponent<PlayerController>(); hose = new SapHoseSprayer(); }
        public void Configure(Func<SapDeposit> factory, Action<SapDeposit> release, bool network)
        { create = factory; destroy = release; externalTick = network; }
        public Vector3 HoverPosition(Ray aim)
        {
            if (settings.controlMode == SapControlMode.Hose)
                return transform.position + Quaternion.Euler(0, player.View.BodyYaw, 0) * settings.hoseHoverOffset;
            var forward = Vector3.ProjectOnPlane(aim.direction, Vector3.up);
            var rotation = forward.sqrMagnitude > 0.000001f ? Quaternion.LookRotation(forward) : player.View.PlanarRotation;
            return transform.position + rotation * settings.hoverOffset;
        }
        public void Toggle()
        {
            if (Controlling) { Cancel(); return; }
            if (settings == null || create == null ||
                (settings.controlMode == SapControlMode.Placement && deposits.Count >= settings.capacity)) return;
            source = SapSource.FindNearest(transform.position + settings.extractionProbeOffset, settings.extractionRange,
                settings.radius + settings.surfaceOffset, out var origin);
            if (source == null || !source.TryExtract()) return;
            held = create();
            if (held == null) { source.Refund(); source = null; return; }
            deposits.Add(held); held.Initialize(settings, origin, Remove, externalTick);
            source.TryClosestSurface(origin, out var surface, out _);
            held.BeginExtraction(surface); sourceRefresh = 0; hose.Reset();
            if (settings.controlMode == SapControlMode.Hose) held.SetStream(surface, false);
            Controlling = true; player.Motor.SetMovementLock(this, true);
        }
        // Shared by the authoritative shot check and the owner's purely visual preview.
        public bool TryPlacement(Ray aim, Vector3 origin, out SapReceiver receiver, out Vector3 placement, out Vector3 normal)
        {
            receiver = null; placement = normal = default;
            float rayRange = settings.controlRange + Vector3.Distance(aim.origin, origin);
            if (!Physics.Raycast(aim, out var hit, rayRange, settings.collisionMask, QueryTriggerInteraction.Ignore)) return false;
            receiver = hit.collider.GetComponentInParent<SapReceiver>();
            if (receiver == null || !receiver.TryPlacement(hit, settings.surfaceOffset, out placement, out normal)) return false;
            Vector3 delta = placement - origin;
            if (delta.magnitude > settings.controlRange) return false;
            if (Physics.SphereCast(origin, settings.radius, delta.normalized, out var block, delta.magnitude,
                settings.collisionMask, QueryTriggerInteraction.Ignore))
                return block.collider.GetComponentInParent<SapReceiver>() == receiver &&
                    Vector3.Distance(block.point, placement) <= settings.radius + settings.placementTolerance;
            return true;
        }
        public void Tick(Ray aim, bool use, float dt)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused) return;
            CanPlace = false;
            if (!Controlling) return;
            if (held == null || source == null || !source.isActiveAndEnabled || held.State == SapState.Complete)
            { Cancel(); return; }
            Vector3 hover = HoverPosition(aim);
            Vector3 next = Vector3.MoveTowards(held.transform.position, hover, settings.extractionSpeed * dt);
            // The connected source is allowed at extraction's start; unrelated obstacles stop the pull.
            Vector3 delta = next - held.transform.position;
            if (delta.sqrMagnitude > 0.000001f && Physics.SphereCast(held.transform.position, settings.radius,
                delta.normalized, out var obstruction, delta.magnitude, settings.collisionMask, QueryTriggerInteraction.Ignore) &&
                obstruction.collider.GetComponentInParent<SapSource>() != source)
            { Cancel(); return; }
            held.transform.position = next;
            if (Vector3.Distance(next, hover) <= settings.arrivalTolerance) held.SetHeld();
            if (settings.controlMode == SapControlMode.Hose)
            {
                CanPlace = Ready;
                hose.Tick(held, aim, Ready && use, dt, settings, CreateHoseMark);
                return;
            }
            sourceRefresh -= dt;
            if (sourceRefresh <= 0)
            {
                sourceRefresh = settings.sourceRefreshInterval;
                if (!source.TryClosestSurface(next, out var surface, out _) ||
                    Vector3.Distance(surface, transform.position + settings.extractionProbeOffset) > settings.controlRange)
                { Cancel(); return; }
                held.SetStream(surface, true);
            }
            CanPlace = Ready && TryPlacement(aim, next, out _, out _, out _);
            if (!use || !CanPlace || !TryPlacement(aim, next, out var receiver, out var placement, out var normal)) return;
            var launched = held; held = null;
            launched.Launch(receiver, placement, normal);
            EndControl();
        }
        private SapDeposit CreateHoseMark()
        {
            // Hose marks are bounded by last-hit expiry, not the legacy placement inventory.
            // A full inventory used to make new aim points silently stop receiving sap.
            var mark = create();
            if (mark == null) return null;
            deposits.Add(mark);
            mark.Initialize(settings, Vector3.zero, Remove, externalTick);
            return mark;
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
        { source = null; Controlling = false; CanPlace = false; replicaReady = false; if (player != null) player.Motor.SetMovementLock(this, false); }
        private void Remove(SapDeposit sap) { deposits.Remove(sap); destroy?.Invoke(sap); }
        public void Clear() { Cancel(); foreach (var sap in deposits.ToArray()) if (sap != null) sap.Finish(); deposits.Clear(); }
        public void ApplyReplica(bool controlling, bool canPlace, bool ready = false)
        { Controlling = controlling; CanPlace = canPlace; replicaReady = ready; }
        private void OnDisable() { Clear(); }
    }
}
