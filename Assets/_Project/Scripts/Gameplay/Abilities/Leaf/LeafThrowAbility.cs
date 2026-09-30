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
        private float cooldown;
        public event Action<float> FacingRequested;
        public LeafAbilitySettings Settings => settings;
        public int ActiveCount { get { if (!settings.installedCapacity) return leaves.Count; int count = 0; foreach (var leaf in leaves) if (leaf.Installed || leaf.State == LeafState.Flying) count++; return count; } }
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
        public float PlayerRange
        {
            get
            {
                var character = GetComponent<CharacterController>();
                return settings.rangeHeightMultiplier > 0 && character != null ? character.height * Mathf.Abs(transform.lossyScale.y) * settings.rangeHeightMultiplier : settings.range;
            }
        }
        public bool TryThrowAtTarget(Ray aim)
        {
            float distance = PlayerRange;
            if (!Physics.Raycast(aim, out var hit, distance + Vector3.Distance(aim.origin, transform.position), settings.hitMask, QueryTriggerInteraction.Ignore)) return Reject(true);
            if (Vector3.Distance(transform.position, hit.point) > distance) return Reject(true);
            var target = hit.collider.GetComponentInParent<LeafInstallTarget>();
            if (target == null || !target.CanInstall) return Reject(false);
            return TryThrow(aim);
        }
        public bool RecallOldest()
        {
            LeafProjectile oldest = null;
            foreach (var leaf in leaves) if (leaf.Installed && (oldest == null || leaf.InstalledAt < oldest.InstalledAt)) oldest = leaf;
            if (oldest == null) return false;
            oldest.BeginReturn(); return true;
        }
        public void Configure(Func<LeafProjectile> create, Action<LeafProjectile> remove, bool online)
        { spawn = create; despawn = remove; network = online; }
        public void Tick(float dt) { cooldown = Mathf.Max(0, cooldown - dt); }
        public bool TryThrow(Ray aim)
        {
            if ((!settings.installedCapacity && Recovering) || cooldown > 0) return false;
            if (settings.requireTarget)
            {
                if (!Physics.Raycast(aim, out var requiredHit, PlayerRange + Vector3.Distance(aim.origin, transform.position), settings.hitMask, QueryTriggerInteraction.Ignore)) return Reject(true);
                if (Vector3.Distance(transform.position, requiredHit.point) > PlayerRange) return Reject(true);
                var requiredTarget = requiredHit.collider.GetComponentInParent<LeafInstallTarget>();
                if (requiredTarget == null || !requiredTarget.CanInstall) return Reject(false);
            }
            if (ActiveCount >= settings.capacity)
            {
                LeafProjectile oldest = null;
                foreach (var leaf in leaves) if (leaf.Installed && (oldest == null || leaf.InstalledAt < oldest.InstalledAt)) oldest = leaf;
                if (oldest != null)
                {
                    if (!settings.installedCapacity) { Recovering = true; pendingRecovery = oldest; }
                    oldest.BeginReturn();
                }
                if (!settings.installedCapacity || oldest == null) return false;
            }
            float rayRange = settings.requireTarget ? PlayerRange + Vector3.Distance(aim.origin, transform.position) : settings.range;
            Vector3 endpoint = aim.GetPoint(rayRange);
            if (Physics.Raycast(aim, out var hit, rayRange, settings.hitMask, QueryTriggerInteraction.Ignore)) endpoint = hit.point + aim.direction * settings.collisionRadius;
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
            Vector3 origin = frame.position + Quaternion.Euler(0, frame.eulerAngles.y, 0) * settings.launchOffset;
            var projectile = spawn();
            leaves.Add(projectile);
            projectile.Initialize(settings, transform, origin, endpoint, OnReturned, network);
            cooldown = settings.cooldown; return true;
        }
        private void OnReturned(LeafProjectile leaf)
        {
            leaves.Remove(leaf);
            if (pendingRecovery == leaf) { Recovering = false; pendingRecovery = null; }
            despawn(leaf);
        }
        public void Clear()
        {
            foreach (var leaf in leaves.ToArray()) if (leaf != null) leaf.Finish();
            leaves.Clear(); Recovering = false;
        }
    }
}
