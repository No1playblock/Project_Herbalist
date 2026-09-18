using Fusion;
using Herbalist.Abilities;
using UnityEngine;
namespace Herbalist.Networking
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(LeafProjectile))]
    public sealed class NetworkLeafProjectile : NetworkBehaviour
    {
        private LeafProjectile leaf;
        [Networked] private LeafMode Mode { get; set; }
        [Networked] private LeafState State { get; set; }
        [Networked] private int TargetId { get; set; }
        private void Awake() => leaf = GetComponent<LeafProjectile>();
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            leaf.Tick(Runner.DeltaTime);
            if (Object == null || !Object.IsValid) return;
            Mode = leaf.Mode; State = leaf.State; TargetId = leaf.Target != null ? leaf.Target.Id : 0;
        }
        public override void Render() { if (!HasStateAuthority) leaf.ApplyReplica(Mode, State, TargetId); }
        public override void Despawned(NetworkRunner runner, bool hasState) => leaf.ApplyReplica(LeafMode.Off, LeafState.Complete, 0);
    }
}
