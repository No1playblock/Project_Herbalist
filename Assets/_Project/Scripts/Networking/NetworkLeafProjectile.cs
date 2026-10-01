using Fusion;
using Herbalist.Abilities;
using UnityEngine;
namespace Herbalist.Networking
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(LeafProjectile))]
    public sealed class NetworkLeafProjectile : NetworkBehaviour
    {
        private LeafProjectile leaf;
        [Networked] private LeafState State { get; set; }
        [Networked] private int TargetId { get; set; }
        private void Awake() => leaf = GetComponent<LeafProjectile>();
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            leaf.Tick(Runner.DeltaTime);
            if (Object == null || !Object.IsValid) return;
            State = leaf.State; TargetId = leaf.Target != null ? leaf.Target.Id : 0;
        }
        public override void Render() { if (!HasStateAuthority) leaf.ApplyReplica(State, TargetId); }
        public override void Despawned(NetworkRunner runner, bool hasState) => leaf.ApplyReplica(LeafState.Complete, 0);
    }
}
