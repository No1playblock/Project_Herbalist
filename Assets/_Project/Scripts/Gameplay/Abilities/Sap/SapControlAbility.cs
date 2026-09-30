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
        private SapHoseSprayer hose;
        public SapAbilitySettings Settings => settings;
        public bool Controlling { get; private set; }
        public bool CanPlace { get; private set; }
        public bool Ready => held != null ? held.State == SapState.Controlled : replicaReady;
        public int ActiveCount => deposits.Count;
        public SapDeposit Held => held;
        private void Awake()
        {
            player = GetComponent<PlayerController>(); hose = new SapHoseSprayer();
            var rules = Herbalist.Levels.StageLevel.Instance != null ? Herbalist.Levels.StageLevel.Instance.abilityRules : null;
            if (rules != null && rules.sap != null) settings = rules.sap;
        }
        // New stage rules generate sap on demand at an eligible inlet. Legacy extraction stays available.
        public void TickContextual(Ray aim, bool useHeld, float dt)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused || settings == null || create == null) return;
            Vector3 origin = HoverPosition(aim);
            if (!useHeld || !Physics.Raycast(aim, out var hit, settings.controlRange + Vector3.Distance(aim.origin, origin), settings.collisionMask, QueryTriggerInteraction.Ignore) ||
                hit.collider.GetComponentInParent<SapInjectionPort>() == null || Vector3.Distance(origin, hit.point) > settings.controlRange)
            { Cancel(); return; }
            if (held == null)
            {
                held = create(); if (held == null) return;
                deposits.Add(held); held.Initialize(settings, origin, Remove, externalTick);
                held.BeginExtraction(origin); held.SetHeld(); hose.Reset();
            }
            held.transform.position = origin;
            Controlling = true; CanPlace = true;
            hose.Tick(held, aim, true, dt, settings, CreateHoseMark);
        }
        public void Configure(Func<SapDeposit> factory, Action<SapDeposit> release, bool network)
        { create = factory; destroy = release; externalTick = network; }
        public Vector3 HoverPosition(Ray aim)
        {
            return transform.position + Quaternion.Euler(0, player.View.BodyYaw, 0) * settings.hoseHoverOffset;
        }
        public void Toggle()
        {
            if (Controlling) { Cancel(); return; }
            if (settings == null || create == null) return;
            source = SapSource.FindNearest(transform.position + settings.extractionProbeOffset, settings.extractionRange,
                settings.radius + settings.surfaceOffset, out var origin);
            if (source == null || !source.TryExtract()) return;
            held = create();
            if (held == null) { source.Refund(); source = null; return; }
            deposits.Add(held); held.Initialize(settings, origin, Remove, externalTick);
            source.TryClosestSurface(origin, out var surface, out _);
            held.BeginExtraction(surface); hose.Reset();
            held.SetStream(surface, false);
            Controlling = true; player.Motor.SetMovementLock(this, true);
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
            CanPlace = Ready;
            hose.Tick(held, aim, Ready && use, dt, settings, CreateHoseMark);
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
