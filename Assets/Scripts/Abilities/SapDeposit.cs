using System;
using System.Collections.Generic;
using UnityEngine;
namespace Herbalist.Abilities
{
    public sealed class SapDeposit : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private SapAbilitySettings settings;
        private static readonly HashSet<SapDeposit> all = new();
        private Action<SapDeposit> remove;
        private bool authority, externalTick;
        private float remaining;
        private LeafProjectile boundLeaf;
        private Vector3 localPosition;
        private Quaternion localRotation;
        private SapReceiver flightReceiver;
        private Vector3 flightLocalPosition, flightLocalNormal;
        private float flightRemaining;
        public Vector3 StreamStart { get; private set; }
        public bool HasStream { get; private set; }
        public SapAbilitySettings Settings => settings;
        public void SetStream(Vector3 start, bool active) { StreamStart = start; HasStream = active; }
        public void BeginExtraction(Vector3 sourcePoint) { State = SapState.Extracting; SetStream(sourcePoint, true); RefreshVisual(); }
        public void SetHeld() { if (State == SapState.Extracting) State = SapState.Controlled; }
        public void Launch(SapReceiver receiver, Vector3 position, Vector3 normal)
        {
            if (!authority || State != SapState.Controlled || receiver == null) return;
            flightReceiver = receiver;
            flightLocalPosition = receiver.transform.InverseTransformPoint(position);
            flightLocalNormal = receiver.transform.InverseTransformDirection(normal);
            flightRemaining = settings.flightTimeout;
            SetStream(transform.position, true); State = SapState.Flying;
        }
        private void TickFlight(float dt)
        {
            flightRemaining -= dt;
            if (flightReceiver == null || !flightReceiver.isActiveAndEnabled || flightRemaining <= 0) { Finish(); return; }
            var destination = flightReceiver.transform.TransformPoint(flightLocalPosition);
            var normal = flightReceiver.transform.TransformDirection(flightLocalNormal).normalized;
            var delta = destination - transform.position;
            float travel = Mathf.Min(delta.magnitude, settings.flightSpeed * dt);
            if (Physics.SphereCast(transform.position, settings.radius, delta.normalized, out var hit, travel,
                settings.collisionMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<SapReceiver>() != flightReceiver ||
                    Vector3.Distance(hit.point, destination) > settings.radius + settings.placementTolerance) { Finish(); return; }
                Arrive(destination, normal); return;
            }
            transform.position = Vector3.MoveTowards(transform.position, destination, travel);
            if (Vector3.Distance(transform.position, destination) <= settings.arrivalTolerance) Arrive(destination, normal);
        }
        private void Arrive(Vector3 destination, Vector3 normal)
        {
            HasStream = false;
            if (RefreshAt(flightReceiver, destination)) Finish();
            else Place(flightReceiver, destination, normal);
            flightReceiver = null;
        }
        public SapState State { get; private set; }
        public SapReceiver Receiver { get; private set; }
        public bool IsPlaced => State == SapState.Attached || State == SapState.Bound;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() => all.Clear();
        private void OnEnable() => all.Add(this);
        private void OnDisable() { all.Remove(this); Detach(); }
        public void Initialize(SapAbilitySettings config, Vector3 origin, Action<SapDeposit> release, bool network)
        {
            settings = config; remove = release; externalTick = network; authority = true;
            State = SapState.Controlled; transform.position = origin; RefreshVisual();
        }
        private void Update() { if (authority && !externalTick) Tick(Time.deltaTime); }
        public void Tick(float dt)
        {
            if (!authority) return;
            if (State == SapState.Flying) { TickFlight(dt); return; }
            if (!IsPlaced) return;
            if (Receiver == null || !Receiver.isActiveAndEnabled) { Finish(); return; }
            transform.SetPositionAndRotation(Receiver.transform.TransformPoint(localPosition), Receiver.transform.rotation * localRotation);
            if (State == SapState.Bound)
            {
                if (boundLeaf == null || boundLeaf.State == LeafState.Returning || boundLeaf.State == LeafState.Complete) Finish();
            }
            else { remaining -= dt; if (remaining <= 0) Finish(); }
        }
        public void Place(SapReceiver receiver, Vector3 position, Vector3 normal)
        {
            if (!authority || (State != SapState.Controlled && State != SapState.Flying)) return;
            HasStream = false;
            Receiver = receiver; State = SapState.Attached; remaining = settings.lifetime;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal));
            localPosition = receiver.transform.InverseTransformPoint(position);
            localRotation = Quaternion.Inverse(receiver.transform.rotation) * transform.rotation;
            receiver.Attach(this); RefreshVisual();
        }
        public bool RefreshAt(SapReceiver receiver, Vector3 position)
        {
            foreach (var sap in all)
            {
                if (sap == this || !sap.authority || sap.State != SapState.Attached || sap.Receiver != receiver) continue;
                if (Vector3.Distance(sap.transform.position, position) > settings.refreshRadius) continue;
                sap.remaining = sap.settings.lifetime; return true;
            }
            return false;
        }
        public static bool TryBind(LeafProjectile leaf)
        {
            SapDeposit nearest = null; float distance = float.PositiveInfinity;
            foreach (var sap in all)
            {
                if (!sap.authority || sap.State != SapState.Attached || sap.remaining <= 0 || sap.Receiver == null || sap.Receiver.LeafTarget != leaf.Target) continue;
                float candidate = Vector3.Distance(leaf.ContactPoint, sap.transform.position);
                if (candidate <= sap.settings.bindingRadius && candidate < distance) { nearest = sap; distance = candidate; }
            }
            if (nearest == null) return false;
            nearest.boundLeaf = leaf; nearest.State = SapState.Bound; return true;
        }
        public static void Release(LeafProjectile leaf)
        {
            foreach (var sap in new List<SapDeposit>(all)) if (sap.authority && sap.boundLeaf == leaf) sap.Finish();
        }
        public void Finish()
        {
            if (!authority || State == SapState.Complete) return;
            HasStream = false; State = SapState.Complete;
            var leaf = boundLeaf; boundLeaf = null;
            if (leaf != null && leaf.Installed) leaf.BeginReturn();
            Detach(); RefreshVisual(); remove?.Invoke(this);
        }
        private void Detach() { if (Receiver != null) Receiver.Detach(this); Receiver = null; }
        public void ApplyReplica(SapState state, int receiverId)
        {
            var receiver = SapReceiver.Find(receiverId);
            if (State != state || Receiver != receiver) Detach();
            State = state; Receiver = receiver;
            if (IsPlaced && Receiver != null) Receiver.Attach(this);
            RefreshVisual();
        }
        private void RefreshVisual()
        {
            if (visual == null || settings == null) return;
            visual.gameObject.SetActive(State != SapState.Complete && State != SapState.Inactive);
            visual.localScale = IsPlaced ? settings.attachedScale : settings.controlledScale;
        }
    }
}
