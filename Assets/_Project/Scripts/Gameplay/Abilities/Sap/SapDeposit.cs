using System;
using System.Collections.Generic;
using UnityEngine;
namespace Herbalist.Abilities
{
    public sealed class SapDeposit : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private SapSurfaceMarkPresentation surfaceMark;
        [SerializeField] private SapSurfaceVolumePresentation surfaceVolume;
        [SerializeField] private SapAbilitySettings settings;
        private static readonly HashSet<SapDeposit> all = new();
        private Action<SapDeposit> remove;
        private bool authority, externalTick;
        private float remaining;
        private Transform surface;
        private bool hoseMark;
        private float markScale = 1;
        public float MarkScale => markScale;
        public bool IsHoseMark => hoseMark;
        public Vector3 StreamEnd { get; private set; }
        public Vector3 HoseLaunchVelocity { get; private set; }
        public float HoseTravelTime { get; private set; }
        public bool HoseStream { get; private set; }
        public bool HoseRetracting { get; private set; }

        public Vector3 StreamOrigin => HoseStream ? transform.position : StreamStart;
        public Vector3 StreamDestination => HoseStream ? StreamEnd : transform.position;
public void SetHoseStream(Vector3 end, bool retracting, Vector3 launchVelocity, float travelTime)
        {
            HoseStream = true;
            HoseRetracting = retracting;
            StreamEnd = end;
            HoseLaunchVelocity = launchVelocity;
            HoseTravelTime = travelTime;
            HasStream = true;
        }
public void SetHoseReplica(bool hose, bool retracting, Vector3 end, Vector3 launchVelocity, float travelTime, float scale)
        {
            HoseStream = hose;
            HoseRetracting = hose && retracting;
            StreamEnd = end;
            HoseLaunchVelocity = launchVelocity;
            HoseTravelTime = travelTime;
            markScale = scale;
            RefreshVisual();
        }
        private Vector3 localPosition;
        private Quaternion localRotation;
        public Vector3 StreamStart { get; private set; }
        public bool HasStream { get; private set; }
        public SapAbilitySettings Settings => settings;
        public Transform Visual => visual;
public void SetStream(Vector3 start, bool active)
        {
            StreamStart = start;
            HasStream = active;
            HoseStream = false;
            HoseRetracting = false;
        }
        public void BeginExtraction(Vector3 sourcePoint) { State = SapState.Extracting; SetStream(sourcePoint, true); RefreshVisual(); }
        public void SetHeld() { if (State == SapState.Extracting) State = SapState.Controlled; }
        public SapState State { get; private set; }
        public SapReceiver Receiver { get; private set; }
        public bool IsPlaced => State == SapState.Attached;
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
            if (Herbalist.GameUI.GameplayPause.IsPaused) return;
            if (!authority) return;
            if (!IsPlaced) return;
            if (hoseMark)
            {
                if (surface == null || !surface.gameObject.activeInHierarchy) { Finish(); return; }
                transform.SetPositionAndRotation(surface.TransformPoint(localPosition), surface.rotation * localRotation);
                remaining -= dt; if (remaining <= 0) Finish();
                return;
            }
        }
        public static void ApplyHoseHit(RaycastHit hit, SapAbilitySettings settings, float dt, Func<SapDeposit> create)
        {
            var inlet = hit.collider.GetComponentInParent<SapInjectionPort>();
            if (inlet != null && inlet.Inject(dt)) return;
            SapDeposit nearest = null;
            float distance = settings.hoseMarkSpacing;
            foreach (var mark in all)
            {
                if (!mark.authority || !mark.hoseMark || !mark.IsPlaced || mark.surface != hit.collider.transform) continue;
                if (Vector3.Dot(mark.transform.forward, hit.normal) < settings.hoseMergeNormalDot) continue;
                float candidate = Vector3.Distance(mark.transform.position, hit.point);
                if (candidate <= distance) { nearest = mark; distance = candidate; }
            }
            if (nearest == null)
            {
                nearest = create();
                if (nearest == null) return;
                nearest.hoseMark = true; nearest.surface = hit.collider.transform;
                nearest.Receiver = hit.collider.GetComponentInParent<SapReceiver>();
                nearest.State = SapState.Attached;
                nearest.transform.SetPositionAndRotation(hit.point + hit.normal * settings.surfaceOffset, Quaternion.LookRotation(hit.normal));
                nearest.localPosition = nearest.surface.InverseTransformPoint(nearest.transform.position);
                nearest.localRotation = Quaternion.Inverse(nearest.surface.rotation) * nearest.transform.rotation;
                if (nearest.Receiver != null) nearest.Receiver.Attach(nearest);
            }
            nearest.remaining = nearest.settings.hoseMarkLifetime;
            nearest.markScale = Mathf.Min(nearest.settings.hoseMaxScale, nearest.markScale + nearest.settings.hoseGrowthPerSecond * dt);
            nearest.RefreshVisual();
        }
        public void Finish()
        {
            if (!authority || State == SapState.Complete) return;
            HasStream = false; State = SapState.Complete;
            Detach(); RefreshVisual(); remove?.Invoke(this);
        }
        private void Detach() { if (Receiver != null) Receiver.Detach(this); Receiver = null; }
        public void ApplyReplica(SapState state, int receiverId, bool replicatedHoseMark = false, float replicatedMarkScale = 1f)
        {
            var receiver = SapReceiver.Find(receiverId);
            if (State != state || Receiver != receiver) Detach();
            State = state; Receiver = receiver; hoseMark = replicatedHoseMark; markScale = replicatedMarkScale;
            if (IsPlaced && Receiver != null) Receiver.Attach(this);
            RefreshVisual();
        }
        private void RefreshVisual()
        {
            if (visual == null || settings == null) return;
            bool active = State != SapState.Complete && State != SapState.Inactive;
            bool attached = active && IsPlaced;
            bool projected = attached && surfaceMark != null && surfaceMark.Available;
            bool raised = attached && surfaceVolume != null && surfaceVolume.Available;
            visual.gameObject.SetActive(active && !projected && !raised);
            visual.localScale = IsPlaced ? Vector3.Scale(settings.attachedScale, new Vector3(markScale, markScale, 1)) : settings.controlledScale;
            if (surfaceMark != null) surfaceMark.SetVisible(projected, settings, markScale);
            if (surfaceVolume != null) surfaceVolume.SetVisible(raised, settings, markScale);
        }
    }
}
