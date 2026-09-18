using Fusion;
using Herbalist.Abilities;
using UnityEngine;
namespace Herbalist.Networking
{
    [RequireComponent(typeof(PlayerAbilityController))]
    public sealed class NetworkLeafAbility : NetworkBehaviour
    {
        [SerializeField] private NetworkObject leafPrefab;
        [SerializeField] private NetworkObject sapPrefab;
        [SerializeField] private int[] prototypeSapSlots = { 0 };
        [Tooltip("Prototype grants only. Replace with authoritative potion effects later.")]
        [SerializeField] private int[] prototypeUnlockSlots = { 1 };
        private PlayerAbilityController abilities;
        private NetworkPlayer player;
        [Networked] private PlayerAbilityKind Kind { get; set; }
        [Networked] private NetworkBool Controlling { get; set; }
        [Networked] private NetworkBool CanPlace { get; set; }
        [Networked] private NetworkBool Ready { get; set; }
        [Networked] private NetworkBool Unlocked { get; set; }
        [Networked] private LeafMode Selected { get; set; }
        [Networked] private int Count { get; set; }
        [Networked] private NetworkBool Recovering { get; set; }
        private void Awake()
        {
            abilities = GetComponent<PlayerAbilityController>(); player = GetComponent<NetworkPlayer>();
            abilities.ConfigureNetwork(false);
        }
        public override void Spawned()
        {
            abilities.ConfigureNetwork(HasStateAuthority);
            abilities.Leaf.FacingRequested += SetShotFacing;
            if (!HasStateAuthority) return;
            abilities.Leaf.Configure(() => Runner.Spawn(leafPrefab, transform.position, Quaternion.identity, Object.InputAuthority).GetComponent<LeafProjectile>(),
                leaf => { if (leaf != null && Runner != null && Runner.IsRunning) Runner.Despawn(leaf.GetComponent<NetworkObject>()); }, true);
            if (abilities.Sap != null && sapPrefab != null)
            {
                abilities.Sap.Configure(() => Runner.Spawn(sapPrefab, transform.position, Quaternion.identity, Object.InputAuthority).GetComponent<SapDeposit>(),
                    sap => { if (sap != null && Runner != null && Runner.IsRunning) Runner.Despawn(sap.GetComponent<NetworkObject>()); }, true);
                if (Herbalist.StageOne.StageOneFlow.Instance == null && !(Herbalist.Levels.StageLevel.Instance != null && Herbalist.Levels.StageLevel.Instance.requireEarnedAbilities)) foreach (int slot in prototypeSapSlots) if (player.Slot == slot) abilities.UnlockSap();
            }
            if (Herbalist.StageOne.StageOneFlow.Instance == null && !(Herbalist.Levels.StageLevel.Instance != null && Herbalist.Levels.StageLevel.Instance.requireEarnedAbilities)) foreach (int slot in prototypeUnlockSlots) if (player.Slot == slot) abilities.UnlockLeaf();
            if (FusionLobbySession.Instance != null) FusionLobbySession.Instance.RestoreStageAbility(Object.InputAuthority, abilities);
            Publish();
        }
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            if (GetInput(out PlayerNetworkInput input))
            {
                // Reconstruct aim from authoritative player position; never trust a client hit target.
                var yaw = input.LookAngles.x; var pitch = input.LookAngles.y;
                if (float.IsNaN(yaw) || float.IsInfinity(yaw) || float.IsNaN(pitch) || float.IsInfinity(pitch)) return;
                abilities.Tick(input.AbilityCycle, input.AbilityUse, player.Player.View.GetAimRay(yaw, pitch), Runner.DeltaTime, input.AbilityHeld);
            }
            else if (abilities.Sap != null && abilities.Sap.Controlling)
                abilities.Sap.Tick(player.Player.View.GetAimRay(player.LookAngles.x, player.LookAngles.y), false, Runner.DeltaTime);
            Publish();
        }
        private void Publish() { Unlocked = abilities.Unlocked; Selected = abilities.Mode; Count = abilities.DisplayCount; Kind = abilities.Kind; Controlling = abilities.Sap != null && abilities.Sap.Controlling; CanPlace = abilities.Sap != null && abilities.Sap.CanPlace; Ready = abilities.Sap != null && abilities.Sap.Ready; Recovering = abilities.Leaf.Recovering; }
        public override void Render() { if (!HasStateAuthority) abilities.ApplyReplica(Unlocked, Selected, Count, Recovering, Kind, Controlling, CanPlace, Ready); }
        private void SetShotFacing(float yaw) { if (HasStateAuthority) player.BodyYaw = yaw; }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            abilities.Leaf.FacingRequested -= SetShotFacing;
            abilities.RevokeLeaf();
        }
    }
}
