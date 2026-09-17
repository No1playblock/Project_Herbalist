using Fusion;
using UnityEngine;
namespace Herbalist.StageOne
{
    [RequireComponent(typeof(NetworkObject), typeof(StageOneFlow))]
    public sealed class NetworkStageState : NetworkBehaviour
    {
        [Networked] private ulong Harvested { get; set; }
        [Networked] private uint Crafted { get; set; }
        [Networked] private int Held0 { get; set; }
        [Networked] private int Held1 { get; set; }
        [Networked] private int Consumed0 { get; set; }
        [Networked] private int Consumed1 { get; set; }
        [Networked] private NetworkBool Cleared { get; set; }
        [Networked] private int Inside { get; set; }
        private StageOneFlow flow;
        private void Awake() { flow = GetComponent<StageOneFlow>(); }
        public override void Spawned() { Publish(); }
        public void Publish()
        {
            if (Object == null || !Object.IsValid || !HasStateAuthority || flow.Progress == null) return;
            var p = flow.Progress;
            Harvested = p.Harvested; Crafted = p.Crafted; Held0 = p.Held[0]; Held1 = p.Held[1];
            Consumed0 = p.Consumed[0]; Consumed1 = p.Consumed[1]; Cleared = p.Cleared; Inside = flow.InsideCount;
        }
        public override void FixedUpdateNetwork() { Publish(); }
        public override void Render() { if (!HasStateAuthority) flow.SetReplica(Harvested, Crafted, Held0, Held1, Consumed0, Consumed1, Cleared, Inside); }
    }
}
