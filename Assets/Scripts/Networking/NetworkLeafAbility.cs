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
            if (!HasStateAuthority) return;
            abilities.Leaf.Configure(() => Runner.Spawn(leafPrefab, transform.position, Quaternion.identity, Object.InputAuthority).GetComponent<LeafProjectile>(),
                leaf => { if (leaf != null && Runner != null && Runner.IsRunning) Runner.Despawn(leaf.GetComponent<NetworkObject>()); }, true);
            if (abilities.Sap != null && sapPrefab != null)
            {
                abilities.Sap.Configure(() => Runner.Spawn(sapPrefab, transform.position, Quaternion.identity, Object.InputAuthority).GetComponent<SapDeposit>(),
                    sap => { if (sap != null && Runner != null && Runner.IsRunning) Runner.Despawn(sap.GetComponent<NetworkObject>()); }, true);
                foreach (int slot in prototypeSapSlots) if (player.Slot == slot) abilities.UnlockSap();
            }
            foreach (int slot in prototypeUnlockSlots) if (player.Slot == slot) abilities.UnlockLeaf();
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
                abilities.Tick(input.AbilityCycle, input.AbilityUse, player.Player.View.GetAimRay(yaw, pitch), Runner.DeltaTime);
            }
            Publish();
        }
        private void Publish() { Unlocked = abilities.Unlocked; Selected = abilities.Mode; Count = abilities.DisplayCount; Kind = abilities.Kind; Controlling = abilities.Sap != null && abilities.Sap.Controlling; CanPlace = abilities.Sap != null && abilities.Sap.CanPlace; Recovering = abilities.Leaf.Recovering; }
        public override void Render() { if (!HasStateAuthority) abilities.ApplyReplica(Unlocked, Selected, Count, Recovering, Kind, Controlling, CanPlace); }
        public override void Despawned(NetworkRunner runner, bool hasState) { abilities.RevokeLeaf(); }
    }
}
