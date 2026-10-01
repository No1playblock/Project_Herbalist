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
        public LeafAbilitySettings Settings => settings;
        public int ActiveCount => leaves.Count;
        public bool Recovering { get; private set; }
        public void Configure(Func<LeafProjectile> create, Action<LeafProjectile> remove, bool online)
        { spawn = create; despawn = remove; network = online; }
        public void Tick(float dt) { cooldown = Mathf.Max(0, cooldown - dt); }
        public bool TryThrow(LeafMode mode, Ray aim)
        {
            if (mode == LeafMode.Off || Recovering || cooldown > 0) return false;
            if (leaves.Count >= settings.capacity)
            {
                LeafProjectile oldest = null;
                foreach (var leaf in leaves) if (leaf.Installed && (oldest == null || leaf.InstalledAt < oldest.InstalledAt)) oldest = leaf;
                if (oldest != null) { Recovering = true; pendingRecovery = oldest; oldest.BeginReturn(); }
                return false;
            }
            Vector3 endpoint = aim.GetPoint(settings.range);
            if (Physics.Raycast(aim, out var hit, settings.range, settings.hitMask, QueryTriggerInteraction.Ignore)) endpoint = hit.point + aim.direction * settings.collisionRadius;
            Transform frame = launchFrame != null ? launchFrame : transform;
            Vector3 origin = frame.position + Quaternion.Euler(0, frame.eulerAngles.y, 0) * settings.launchOffset;
            var projectile = spawn();
            leaves.Add(projectile);
            projectile.Initialize(settings, transform, mode, origin, endpoint, OnReturned, network);
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
