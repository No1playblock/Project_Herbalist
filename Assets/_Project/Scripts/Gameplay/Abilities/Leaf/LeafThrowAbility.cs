using System;
using System.Collections.Generic;
using UnityEngine;
namespace Herbalist.Abilities
{
    public sealed class LeafThrowAbility : MonoBehaviour
    {
        [SerializeField] private LeafAbilitySettings settings;
        [SerializeField] private Transform launchFrame;
        private readonly List<LeafProjectile> leaves = new();
        private Func<LeafProjectile> spawn;
        private Action<LeafProjectile> despawn;
        private bool network;
        private LeafProjectile pendingRecovery;
        private bool pendingThrow;
        private Ray pendingAim;
        private float cooldown;
        public event Action<float> FacingRequested;
        public LeafAbilitySettings Settings => settings;
        public int ActiveCount => leaves.Count;
        public bool Recovering { get; private set; }
        public uint FeedbackSequence { get; private set; }
        public bool RangeRejected { get; private set; }
        private void Awake()
        {
            var rules = Herbalist.Levels.StageLevel.Instance != null ? Herbalist.Levels.StageLevel.Instance.abilityRules : null;
            if (rules != null && rules.leaf != null) settings = rules.leaf;
        }
        public void ApplyFeedback(uint sequence, bool rangeRejected) { FeedbackSequence = sequence; RangeRejected = rangeRejected; }
        private bool Reject(bool range) { RangeRejected = range; FeedbackSequence++; return false; }
        public bool TryThrowAtTarget(Ray aim)
        {
            return TryThrow(aim);
        }
        public void Configure(Func<LeafProjectile> create, Action<LeafProjectile> remove, bool online)
        { spawn = create; despawn = remove; network = online; }
        public void Tick(float dt, Ray aim)
        {
            cooldown = Mathf.Max(0, cooldown - dt);
            if (pendingThrow) pendingAim = aim;
        }
        public bool TryThrow(Ray aim)
        {
            if (Recovering || cooldown > 0) return false;
            if (ActiveCount >= settings.capacity)
            {
                LeafProjectile oldest = leaves[0];
                if (oldest.State == LeafState.Complete) return false;
                Recovering = true;
                pendingRecovery = oldest;
                pendingThrow = true;
                pendingAim = aim;
                if (oldest.State != LeafState.Returning) oldest.BeginReturn();
                return true;
            }
            Launch(aim);
            return true;
        }
        private void Launch(Ray aim)
        {
            if (settings.faceAimOnThrow)
            {
                var player = GetComponent<Herbalist.Player.PlayerController>();
                var planar = Vector3.ProjectOnPlane(aim.direction, Vector3.up);
                if (player != null && planar.sqrMagnitude > 0.0001f)
                {
                    float yaw = Quaternion.LookRotation(planar).eulerAngles.y;
                    player.View.SetBodyYaw(yaw);
                    FacingRequested?.Invoke(yaw);
                }
            }
            Transform frame = launchFrame != null ? launchFrame : transform;
            Vector3 origin = frame.TransformPoint(settings.launchOffset);
            float maxDistance = settings.range;
            float aimRayDistance = maxDistance + Vector3.Distance(aim.origin, origin);
            Vector3 aimPoint = aim.GetPoint(aimRayDistance);
            if (Physics.Raycast(aim, out var hit, Mathf.Infinity, settings.hitMask, QueryTriggerInteraction.Ignore))
                aimPoint = hit.point + aim.direction * settings.collisionRadius;
            Vector3 towardAim = aimPoint - origin;
            if (towardAim.sqrMagnitude < 0.000001f) towardAim = aim.direction;
            Vector3 endpoint = origin + Vector3.ClampMagnitude(towardAim, maxDistance);
            var projectile = spawn();
            leaves.Add(projectile);
            projectile.Initialize(settings, transform, origin, endpoint, OnReturned, network, frame);
            RangeRejected = false;
            cooldown = settings.cooldown;
        }
        private void OnReturned(LeafProjectile leaf)
        {
            bool launchPending = pendingRecovery == leaf && pendingThrow;
            leaves.Remove(leaf);
            if (pendingRecovery == leaf) { Recovering = false; pendingRecovery = null; pendingThrow = false; }
            despawn(leaf);
            if (launchPending && isActiveAndEnabled && spawn != null) Launch(pendingAim);
        }
        public void Clear()
        {
            pendingThrow = false; pendingRecovery = null; Recovering = false;
            foreach (var leaf in leaves.ToArray()) if (leaf != null) leaf.Finish();
            leaves.Clear();
        }
    }
}
