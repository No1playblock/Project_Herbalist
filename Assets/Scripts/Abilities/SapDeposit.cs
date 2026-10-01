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
            if (!authority || !IsPlaced) return;
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
            if (!authority || State != SapState.Controlled) return;
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
                float candidate = Vector3.Distance(leaf.transform.position, sap.transform.position);
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
            State = SapState.Complete;
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
