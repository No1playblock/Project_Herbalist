using Herbalist.Player;
using UnityEngine;
namespace Herbalist.Abilities
{
    [RequireComponent(typeof(AbilityInputReader), typeof(LeafThrowAbility), typeof(PlayerController))]
    public sealed class PlayerAbilityController : MonoBehaviour
    {
        [SerializeField] private bool prototypeOfflineUnlock = true;
        [SerializeField] private PlayerAbilityKind prototypeOfflineAbility = PlayerAbilityKind.Leaf;
        private uint lastCycle, lastUse;
        private bool network, authority;
        private PlayerController player;
        public AbilityInputReader Input { get; private set; }
        public LeafThrowAbility Leaf { get; private set; }
        public SapControlAbility Sap { get; private set; }
        public PlayerAbilityKind Kind { get; private set; }
        public bool Unlocked { get; private set; }
        public LeafMode Mode { get; private set; }
        public int DisplayCount { get; private set; }
        public bool DisplayRecovering { get; private set; }
        private void Awake() { player = GetComponent<PlayerController>(); Input = GetComponent<AbilityInputReader>(); Leaf = GetComponent<LeafThrowAbility>(); Sap = GetComponent<SapControlAbility>(); }
        private void Start()
        {
            if (network) return;
            authority = true;
            Leaf.Configure(() => Instantiate(Leaf.Settings.offlinePrefab), leaf => Destroy(leaf.gameObject), false);
            if (Sap != null) Sap.Configure(() => Instantiate(Sap.Settings.offlinePrefab), sap => Destroy(sap.gameObject), false);
            if (prototypeOfflineUnlock && Herbalist.StageOne.StageOneFlow.Instance == null) { if (prototypeOfflineAbility == PlayerAbilityKind.Sap) UnlockSap(); else UnlockLeaf(); }
        }
        public void ConfigureNetwork(bool isAuthority) { network = true; authority = isAuthority; }
        // Potion effects call this on the authoritative player. It never grants client authority.
        public bool UnlockLeaf() { if (!authority) return false; if (Unlocked && Kind != PlayerAbilityKind.Leaf) return false; Kind = PlayerAbilityKind.Leaf; Unlocked = true; return true; }
        public bool UnlockSap() { if (!authority || Sap == null || (Unlocked && Kind != PlayerAbilityKind.Sap)) return false; Kind = PlayerAbilityKind.Sap; Unlocked = true; return true; }
        public void RevokeLeaf() { if (!authority) return; Unlocked = false; Mode = LeafMode.Off; Leaf.Clear(); if (Sap != null) Sap.Clear(); }
        // Temporary solo test hook. Existing deposits/leaves keep their normal lifecycle.
        public bool TrySwitchOfflineAbility()
        {
            if (network || !authority || !player.LocallyControlled || Sap == null || Herbalist.StageOne.StageOneFlow.Instance != null) return false;
            Sap.Cancel();
            Kind = Kind == PlayerAbilityKind.Sap ? PlayerAbilityKind.Leaf : PlayerAbilityKind.Sap;
            Unlocked = true; Mode = LeafMode.Off;
            lastCycle = Input.CycleSequence; lastUse = Input.UseSequence;
            DisplayCount = Kind == PlayerAbilityKind.Sap ? Sap.ActiveCount : Leaf.ActiveCount;
            DisplayRecovering = Kind == PlayerAbilityKind.Leaf && Leaf.Recovering;
            return true;
        }
        private void Update()
        {
            if (network || !player.LocallyControlled) return;
            Tick(Input.CycleSequence, Input.UseSequence, player.View.GetAimRay(player.View.Yaw, player.View.Pitch), Time.deltaTime);
        }
        public void Tick(uint cycle, uint use, Ray aim, float dt)
        {
            if (!authority) return;
            Leaf.Tick(dt);
            uint steps = unchecked(cycle - lastCycle); lastCycle = cycle;
            bool fire = use != lastUse; lastUse = use;
            if (Unlocked && Kind == PlayerAbilityKind.Sap && Sap != null)
            {
                if (steps % 2 != 0) Sap.Toggle();
                Sap.Tick(aim, fire, dt);
            }
            else if (Unlocked)
            {
                Mode = (LeafMode)(((uint)Mode + steps % 3) % 3);
                if (fire) Leaf.TryThrow(Mode, aim);
            }
            DisplayCount = Kind == PlayerAbilityKind.Sap && Sap != null ? Sap.ActiveCount : Leaf.ActiveCount; DisplayRecovering = Leaf.Recovering;
        }
        public void ApplyReplica(bool unlocked, LeafMode mode, int count, bool recovering, PlayerAbilityKind kind = PlayerAbilityKind.Leaf, bool controlling = false, bool canPlace = false, bool ready = false)
        { Kind = kind; if (Sap != null) Sap.ApplyReplica(controlling, canPlace, ready); Unlocked = unlocked; Mode = mode; DisplayCount = count; DisplayRecovering = recovering; }
        private void OnDisable() { if (authority && Leaf != null) Leaf.Clear(); if (authority && Sap != null) Sap.Clear(); }
    }
}
