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
        [Networked] private UnityEngine.Vector3 StreamStart { get; set; }
        [Networked] private NetworkBool HasStream { get; set; }
        [Networked] private NetworkBool HoseStream { get; set; }
        [Networked] private NetworkBool HoseRetracting { get; set; }

        [Networked] private UnityEngine.Vector3 StreamEnd { get; set; }
        [Networked] private UnityEngine.Vector3 HoseLaunchVelocity { get; set; }
        [Networked] private float HoseTravelTime { get; set; }
        [Networked] private float MarkScale { get; set; }
        [Networked] private NetworkBool HoseMark { get; set; }
        private void Awake() => sap = GetComponent<SapDeposit>();
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            sap.Tick(Runner.DeltaTime);
            if (Object == null || !Object.IsValid) return;
            HoseStream = sap.HoseStream; HoseRetracting = sap.HoseRetracting; StreamEnd = sap.StreamEnd;
            HoseLaunchVelocity = sap.HoseLaunchVelocity; HoseTravelTime = sap.HoseTravelTime;
            MarkScale = sap.MarkScale; HoseMark = sap.IsHoseMark;
            StreamStart = sap.StreamStart; HasStream = sap.HasStream;
            State = sap.State; ReceiverId = sap.Receiver != null ? sap.Receiver.Id : 0;
        }
public override void Render()
        {
            if (HasStateAuthority) return;
            sap.ApplyReplica(State, ReceiverId, HoseMark, MarkScale);
            sap.SetStream(StreamStart, HasStream);
            sap.SetHoseReplica(HoseStream, HoseRetracting, StreamEnd, HoseLaunchVelocity, HoseTravelTime, MarkScale);
        }
        public override void Despawned(NetworkRunner runner, bool hasState) => sap.ApplyReplica(SapState.Complete, 0);
    }
}
