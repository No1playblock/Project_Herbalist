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
        [Networked] private StageMinigameKind Minigame { get; set; }
        [Networked] private int MinigameItemId { get; set; }
        [Networked] private int MinigameSteps { get; set; }
        [Networked] private float Temperature { get; set; }
        [Networked] private float CraftingGauge { get; set; }
        [Networked] private float MinigameTime { get; set; }
        [Networked] private int FanPressCount { get; set; }
        [Networked] private int TransferPendingSlot { get; set; }
        [Networked] private float TransferRemaining { get; set; }
        [Networked] private uint MinigameResultSequence { get; set; }
        [Networked] private NetworkBool MinigameSucceeded { get; set; }
        private StageOneFlow flow;
        private void Awake() { flow = GetComponent<StageOneFlow>(); }
        public override void Spawned() { Publish(); }
        public void Publish()
        {
            if (Object == null || !Object.IsValid || !HasStateAuthority || flow.Progress == null) return;
            var p = flow.Progress;
            Harvested = p.Harvested; Crafted = p.Crafted; Held0 = p.Held[0]; Held1 = p.Held[1];
            Consumed0 = p.Consumed[0]; Consumed1 = p.Consumed[1]; Cleared = p.Cleared; Inside = flow.InsideCount;
            Minigame = flow.Minigame; MinigameItemId = flow.MinigameItemId; MinigameSteps = flow.MinigameSteps;
            Temperature = flow.Temperature; CraftingGauge = flow.CraftingGauge; MinigameTime = flow.MinigameTime; FanPressCount = flow.FanPressCount;
            TransferPendingSlot = flow.TransferPendingSlot; TransferRemaining = flow.TransferRemaining;
            MinigameResultSequence = flow.MinigameResultSequence; MinigameSucceeded = flow.MinigameSucceeded;
        }
        public override void FixedUpdateNetwork() { Publish(); }
        public override void Render() { if (!HasStateAuthority) flow.SetReplica(Harvested, Crafted, Held0, Held1, Consumed0, Consumed1, Cleared, Inside,
            Minigame, MinigameItemId, MinigameSteps, Temperature, CraftingGauge, MinigameTime, FanPressCount, TransferPendingSlot, TransferRemaining,
            MinigameResultSequence, MinigameSucceeded); }
    }
}
