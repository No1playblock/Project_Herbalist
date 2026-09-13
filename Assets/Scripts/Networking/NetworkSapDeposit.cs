using Fusion;
using Herbalist.Abilities;
namespace Herbalist.Networking
{
    [UnityEngine.RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(SapDeposit))]
    public sealed class NetworkSapDeposit : NetworkBehaviour
    {
        private SapDeposit sap;
        [Networked] private SapState State { get; set; }
        [Networked] private int ReceiverId { get; set; }
        private void Awake() => sap = GetComponent<SapDeposit>();
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            sap.Tick(Runner.DeltaTime);
            if (Object == null || !Object.IsValid) return;
            State = sap.State; ReceiverId = sap.Receiver != null ? sap.Receiver.Id : 0;
        }
        public override void Render() { if (!HasStateAuthority) sap.ApplyReplica(State, ReceiverId); }
        public override void Despawned(NetworkRunner runner, bool hasState) => sap.ApplyReplica(SapState.Complete, 0);
    }
}
