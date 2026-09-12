using Herbalist.Player;
using UnityEngine;
namespace Herbalist.Abilities
{
    [RequireComponent(typeof(AbilityInputReader), typeof(LeafThrowAbility), typeof(PlayerController))]
    public sealed class PlayerAbilityController : MonoBehaviour
    {
        [SerializeField] private bool prototypeOfflineUnlock = true;
        private uint lastCycle, lastUse;
        private bool network, authority;
        private PlayerController player;
        public AbilityInputReader Input { get; private set; }
        public LeafThrowAbility Leaf { get; private set; }
        public bool Unlocked { get; private set; }
        public LeafMode Mode { get; private set; }
        public int DisplayCount { get; private set; }
        public bool DisplayRecovering { get; private set; }
        private void Awake() { player = GetComponent<PlayerController>(); Input = GetComponent<AbilityInputReader>(); Leaf = GetComponent<LeafThrowAbility>(); }
        private void Start()
        {
            if (network) return;
            authority = true;
            Leaf.Configure(() => Instantiate(Leaf.Settings.offlinePrefab), leaf => Destroy(leaf.gameObject), false);
            if (prototypeOfflineUnlock) UnlockLeaf();
        }
        public void ConfigureNetwork(bool isAuthority) { network = true; authority = isAuthority; }
        // Potion effects call this on the authoritative player. It never grants client authority.
        public bool UnlockLeaf() { if (!authority) return false; Unlocked = true; return true; }
        public void RevokeLeaf() { if (!authority) return; Unlocked = false; Mode = LeafMode.Off; Leaf.Clear(); }
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
            if (Unlocked)
            {
                Mode = (LeafMode)(((uint)Mode + steps % 3) % 3);
                if (fire) Leaf.TryThrow(Mode, aim);
            }
            DisplayCount = Leaf.ActiveCount; DisplayRecovering = Leaf.Recovering;
        }
        public void ApplyReplica(bool unlocked, LeafMode mode, int count, bool recovering)
        { Unlocked = unlocked; Mode = mode; DisplayCount = count; DisplayRecovering = recovering; }
        private void OnDisable() { if (authority && Leaf != null) Leaf.Clear(); }
    }
}
