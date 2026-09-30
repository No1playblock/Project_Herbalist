using Herbalist.Player;
using UnityEngine;
namespace Herbalist.Abilities
{
    [RequireComponent(typeof(AbilityInputReader), typeof(LeafThrowAbility), typeof(PlayerController))]
    public sealed class PlayerAbilityController : MonoBehaviour
    {
        [SerializeField] private bool prototypeOfflineUnlock = true;
        [SerializeField] private PlayerAbilityKind prototypeOfflineAbility = PlayerAbilityKind.Leaf;
        private readonly System.Collections.Generic.HashSet<object> _inputLocks = new();
        public void SetInputLock(object owner,bool value) { if(value)_inputLocks.Add(owner);else _inputLocks.Remove(owner); }
        private IAbilityCycleReceiver _cycleReceiver;
        public void SetCycleReceiver(IAbilityCycleReceiver receiver) => _cycleReceiver=receiver;
        public void ClearCycleReceiver(IAbilityCycleReceiver receiver) { if(ReferenceEquals(_cycleReceiver,receiver))_cycleReceiver=null; }
        private uint lastCycle, lastUse;
        private bool network, authority;
        private bool _offlineBothUnlocked;
        public bool OfflineBothUnlocked => _offlineBothUnlocked;
        private PlayerController player;
        public AbilityInputReader Input { get; private set; }
        public LeafThrowAbility Leaf { get; private set; }
        public SapControlAbility Sap { get; private set; }
        public PlayerAbilityKind Kind { get; private set; }
        public bool Unlocked { get; private set; }
        public int DisplayCount { get; private set; }
        public bool DisplayRecovering { get; private set; }
        private void Awake() { player = GetComponent<PlayerController>(); Input = GetComponent<AbilityInputReader>(); Leaf = GetComponent<LeafThrowAbility>(); Sap = GetComponent<SapControlAbility>(); }
        private void Start()
        {
            if (network) return;
            authority = true;
            Leaf.Configure(() => Instantiate(Leaf.Settings.offlinePrefab), leaf => Destroy(leaf.gameObject), false);
            if (Sap != null) Sap.Configure(() => Instantiate(Sap.Settings.offlinePrefab), sap => Destroy(sap.gameObject), false);
            Herbalist.Levels.StageTransition.RestoreOffline(this);
            if (!Unlocked && prototypeOfflineUnlock && Herbalist.StageOne.StageOneFlow.Instance == null) { if (prototypeOfflineAbility == PlayerAbilityKind.Sap) UnlockSap(); else UnlockLeaf(); }
        }
        public void ConfigureNetwork(bool isAuthority) { network = true; authority = isAuthority; }
        // Potion effects call this on the authoritative player. It never grants client authority.
        public bool UnlockLeaf() { if (!authority) return false; if (Unlocked && Kind != PlayerAbilityKind.Leaf) return false; Kind = PlayerAbilityKind.Leaf; Unlocked = true; return true; }
        public bool UnlockSap() { if (!authority || Sap == null || (Unlocked && Kind != PlayerAbilityKind.Sap)) return false; Kind = PlayerAbilityKind.Sap; Unlocked = true; return true; }
        public void RevokeLeaf() { if (!authority) return; Unlocked = false; Leaf.Clear(); if (Sap != null) Sap.Clear(); }
        // Temporary solo test hook. Existing deposits/leaves keep their normal lifecycle.
        public bool TryUnlockBothOfflineAbilities()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (network || !authority || !player.LocallyControlled || Sap == null || Herbalist.GameUI.GameplayPause.IsPaused) return false;
            _offlineBothUnlocked = true;
            Unlocked = true;
            lastCycle = Input.CycleSequence; lastUse = Input.UseSequence;
            return true;
#else
            return false;
#endif
        }
        public bool TrySwitchOfflineAbility()
        {
            if (network || !authority || !player.LocallyControlled || Sap == null || Herbalist.GameUI.GameplayPause.IsPaused || (Herbalist.StageOne.StageOneFlow.Instance != null && !_offlineBothUnlocked)) return false;
            Sap.Cancel();
            Kind = Kind == PlayerAbilityKind.Sap ? PlayerAbilityKind.Leaf : PlayerAbilityKind.Sap;
            Unlocked = true;
            lastCycle = Input.CycleSequence; lastUse = Input.UseSequence;
            DisplayCount = Kind == PlayerAbilityKind.Sap ? Sap.ActiveCount : Leaf.ActiveCount;
            DisplayRecovering = Kind == PlayerAbilityKind.Leaf && Leaf.Recovering;
            return true;
        }
        private void Update()
        {
            if (network || !player.LocallyControlled) return;
            Tick(Input.CycleSequence, Input.UseSequence, player.View.GetAimRay(player.View.Yaw, player.View.Pitch), Time.deltaTime, Input.UseHeld, Input.CycleHeld);
        }
        public void Tick(uint cycle, uint use, Ray aim, float dt, bool useHeld = false, bool cycleHeld = false)
        {
            if (!authority) return;
            if (Herbalist.GameUI.GameplayPause.IsPaused) { lastCycle = cycle; lastUse = use; return; }
            Leaf.Tick(dt);
            uint steps = unchecked(cycle - lastCycle); lastCycle = cycle;
            bool fire = use != lastUse; lastUse = use;
            if(_cycleReceiver!=null && _cycleReceiver.ReceiveAbilityCycle(this,steps))return;
            if(_inputLocks.Count>0)return;
            var rules = Herbalist.Levels.StageLevel.Instance != null ? Herbalist.Levels.StageLevel.Instance.abilityRules : null;
            if (rules != null && rules.contextualUse)
            {
                if (Unlocked && Kind == PlayerAbilityKind.Sap && Sap != null) Sap.TickContextual(aim, cycleHeld, dt);
                else if (Unlocked)
                {
                    if (steps != 0) Leaf.TryThrowAtTarget(aim);
                    if (fire) Leaf.RecallOldest();
                }
                DisplayCount = Kind == PlayerAbilityKind.Sap && Sap != null ? Sap.ActiveCount : Leaf.ActiveCount;
                DisplayRecovering = Leaf.Recovering;
                return;
            }
            if (Unlocked && Kind == PlayerAbilityKind.Sap && Sap != null)
            {
                if (steps % 2 != 0) Sap.Toggle();
                Sap.Tick(aim, useHeld, dt);
            }
            else if (Unlocked)
            {
                if (steps != 0) Leaf.TryThrowAtTarget(aim);
                if (fire) Leaf.RecallOldest();
            }
            DisplayCount = Kind == PlayerAbilityKind.Sap && Sap != null ? Sap.ActiveCount : Leaf.ActiveCount; DisplayRecovering = Leaf.Recovering;
        }
        public void ApplyReplica(bool unlocked, int count, bool recovering, PlayerAbilityKind kind = PlayerAbilityKind.Leaf, bool controlling = false, bool canPlace = false, bool ready = false)
        { Kind = kind; if (Sap != null) Sap.ApplyReplica(controlling, canPlace, ready); Unlocked = unlocked; DisplayCount = count; DisplayRecovering = recovering; }
        private void OnDisable() { if (authority && Leaf != null) Leaf.Clear(); if (authority && Sap != null) Sap.Clear(); }
    }
}
