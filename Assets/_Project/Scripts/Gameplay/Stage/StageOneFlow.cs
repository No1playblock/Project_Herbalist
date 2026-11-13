using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Herbalist.Networking;
using Herbalist.Abilities;
using Herbalist.Presentation;
namespace Herbalist.StageOne
{
    public enum StageMinigameKind { None, Gathering, Crafting }
    [DefaultExecutionOrder(-500)]
    public sealed class StageOneFlow : MonoBehaviour
    {
        public static StageOneFlow Instance { get; private set; }
        public StageOneSettings settings;
        public HerbSource[] sources;
        public GameObject entranceBlocker;
        public BoxCollider interior;
        public UnityEvent onEntranceOpened = new UnityEvent();
        public UnityEvent onCleared = new UnityEvent();
        public MonoBehaviour entranceCondition;
        public bool GateOpen => Progress != null && Progress.GateOpen && (!(entranceCondition is Herbalist.Levels.IStageExitCondition condition) || condition.CanExit);
        public StageProgress Progress { get; private set; }
        public StageActor[] Actors { get; private set; } = new StageActor[2];
        public int InsideCount { get; private set; }
        public StageMinigameKind Minigame { get; private set; }
        public int MinigameItemId { get; private set; }
        public int MinigameSteps { get; private set; }
        public float Temperature { get; private set; }
        public float CraftingGauge { get; private set; }
        public float MinigameTime { get; private set; }
        public int FanPressCount { get; private set; }
        public uint MinigameResultSequence { get; private set; }
        public bool MinigameSucceeded { get; private set; }
        public int TransferPendingSlot { get; private set; } = -1;
        public float TransferRemaining { get; private set; }
        private int _sourceIndex = -1;
        private int _recipeIndex = -1;
        private readonly Queue<float> _fanPressTimes = new();
        private StageActor _soloGatherActor;
        private NetworkStageState adapter;
        private readonly uint[] _recoverySequences = new uint[2];
        private int _leafPlatformFailureMask;
        private int _airborneAfterLeafFailureMask;
        public int FirstLeafFallSlot { get; private set; } = -1;
        public uint RecoverySequence(int slot) => _recoverySequences[slot];

        internal void MarkLeafPlatformFailure(StageActor actor)
        {
            if (FirstLeafFallSlot < 0 && actor != null && actor.Slot is >= 0 and < 2)
                _leafPlatformFailureMask |= 1 << actor.Slot;
        }

        internal void TrackLeafPlatformFall(StageActor actor, bool grounded)
        {
            if (actor.Slot is < 0 or >= 2) return;
            int bit = 1 << actor.Slot;
            if ((_leafPlatformFailureMask & bit) == 0) return;
            if (!grounded) _airborneAfterLeafFailureMask |= bit;
            else if ((_airborneAfterLeafFailureMask & bit) != 0)
            {
                _leafPlatformFailureMask &= ~bit;
                _airborneAfterLeafFailureMask &= ~bit;
            }
        }

        internal void MarkRecovery(StageActor actor, bool fell)
        {
            int slot = actor.Slot;
            if (slot is < 0 or >= 2) return;
            _recoverySequences[slot]++;
            int bit = 1 << slot;
            if (fell && FirstLeafFallSlot < 0 && (_leafPlatformFailureMask & bit) != 0)
                FirstLeafFallSlot = slot;
            _leafPlatformFailureMask &= ~bit;
            _airborneAfterLeafFailureMask &= ~bit;
            adapter?.Publish();
        }

        internal void ApplyRecoveryReplica(uint first, uint second, int leafFallSlot)
        {
            _recoverySequences[0] = first;
            _recoverySequences[1] = second;
            FirstLeafFallSlot = leafFallSlot;
        }
        private bool openNotified, clearNotified;
        public bool Authority => adapter != null && adapter.Object != null && adapter.Object.IsValid ? adapter.HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() { Instance = null; }
        private bool EnsureInitialized()
        {
            if (Progress != null) return true;
            string error = null;
            if (settings == null || !settings.Valid(out error) || sources == null || sources.Length > 64 || interior == null)
            {
                Debug.LogError("Stage One configuration is incomplete. " + error, this);
                enabled = false;
                return false;
            }
            Instance = this; Progress = new StageProgress(settings); adapter = GetComponent<NetworkStageState>();
            return true;
        }
        private void Awake() => EnsureInitialized();
        private void Update()
        {
            if (!EnsureInitialized()) return;
            RefreshActors();
            if (Authority && !Herbalist.GameUI.GameplayPause.IsPaused)
            {
                TickMinigame(Time.deltaTime);
                if (TransferRemaining > 0 && (TransferRemaining -= Time.deltaTime) <= 0) TransferPendingSlot = -1;
                bool a = IsInside(Actors[0]), b = IsInside(Actors[1]); InsideCount = (a ? 1 : 0) + (b ? 1 : 0);
                if (GateOpen) Progress.CheckClear(a, b);
            }
            ApplyMinigameLocks();
            Present();
        }
        private void ApplyMinigameLocks()
        {
            foreach (var actor in Actors)
            {
                if (actor == null) continue;
                var player = actor.GetComponent<Herbalist.Player.PlayerController>();
                player.Motor.SetMovementLock(this, Minigame != StageMinigameKind.None);
                actor.Abilities?.SetInputLock(this, Minigame != StageMinigameKind.None);
            }
        }
        private void TickMinigame(float dt)
        {
            if (Minigame == StageMinigameKind.None) return;
            if (Actors[0] == null || Actors[1] == null)
            {
                if (Minigame != StageMinigameKind.Gathering || _soloGatherActor == null ||
                    !IsSoloRoleTest(_soloGatherActor) || Actors[_soloGatherActor.Slot] != _soloGatherActor)
                { EndMinigame(); return; }
            }
            if (Minigame != StageMinigameKind.Crafting) return;
            var recipe = settings.recipes[_recipeIndex];
            MinigameTime += dt;
            TrimFanPresses();
            float temperature = Temperature, gauge = CraftingGauge;
            StageBrewingRules.Advance(settings, recipe, FanPressCount, dt, ref temperature, ref gauge);
            Temperature = temperature; CraftingGauge = gauge;
            if (Temperature >= settings.maximumTemperature) EndMinigame();
            else if (CraftingGauge >= 100f)
            {
                bool crafted = Progress.Craft(settings.SlotForRole(CharacterRole.Sodam));
                if (crafted) { Present(); adapter?.Publish(); }
                EndMinigame(crafted);
            }
            else if (MinigameTime >= recipe.timeLimit) EndMinigame();
        }
        private void TrimFanPresses()
        {
            while (_fanPressTimes.Count > 0 && MinigameTime - _fanPressTimes.Peek() >= settings.fanCountWindowSeconds)
                _fanPressTimes.Dequeue();
            FanPressCount = _fanPressTimes.Count;
        }
        private void StartGathering(StageActor actor, int sourceIndex)
        {
            _sourceIndex = sourceIndex; _recipeIndex = -1;
            _soloGatherActor = Actors[0] == null || Actors[1] == null ? actor : null;
            MinigameItemId = settings.ItemId(sources[sourceIndex].item);
            MinigameSteps = 0; MinigameTime = 0; Minigame = StageMinigameKind.Gathering;
            adapter?.Publish();
        }
        private void StartCrafting(int recipeIndex)
        {
            _recipeIndex = recipeIndex; _sourceIndex = -1;
            var recipe = settings.recipes[recipeIndex];
            MinigameItemId = settings.ItemId(recipe.ingredient);
            Temperature = settings.BrewingStartTemperature(recipe); CraftingGauge = 0; MinigameTime = 0; MinigameSteps = 0;
            _fanPressTimes.Clear(); FanPressCount = 0;
            Minigame = StageMinigameKind.Crafting;
            adapter?.Publish();
        }
        private void EndMinigame(bool success = false, bool showResult = true)
        {
            if (Minigame != StageMinigameKind.None && showResult) { MinigameSucceeded = success; MinigameResultSequence++; }
            Minigame = StageMinigameKind.None; MinigameItemId = 0; MinigameSteps = 0;
            Temperature = CraftingGauge = MinigameTime = 0;
            _fanPressTimes.Clear(); FanPressCount = 0;
            _sourceIndex = _recipeIndex = -1;
            _soloGatherActor = null;
            ApplyMinigameLocks(); adapter?.Publish();
        }
        private bool TryHerbTransferPair(int slot, out int donor)
        {
            donor = settings.SlotForRole(CharacterRole.Duyeong);
            int receiver = settings.SlotForRole(CharacterRole.Sodam);
            if (slot != donor && slot != receiver) return false;
            var held = settings.Item(Progress.Held[donor]);
            if (held == null || held.type != StageItemType.Herb || Progress.Held[receiver] != 0 || Actors[donor] == null || Actors[receiver] == null) return false;
            return Reach(Actors[donor], Actors[receiver].transform.position + settings.interactionOffset);
        }
        public void RefreshActors()
        {
            Actors[0] = Actors[1] = null;
            foreach (var actor in StageActor.All)
                if (actor.Available && actor.Slot >= 0 && actor.Slot < Actors.Length) Actors[actor.Slot] = actor;
        }
        private static bool IsSoloRoleTest(StageActor actor) =>
            actor != null && (FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession) &&
            actor.GetComponent<OfflineAbilitySwitcher>() is { isActiveAndEnabled: true };
        public CharacterRole RoleFor(StageActor actor)
        {
            if (actor == null) return CharacterRole.Duyeong;
            return IsSoloRoleTest(actor) ? actor.GetComponent<OfflineAbilitySwitcher>().Role : settings.slotRoles[actor.Slot];
        }
        private bool IsInside(StageActor actor)
        {
            if (actor == null || !actor.Available || interior == null ||
                !interior.enabled || !interior.gameObject.activeInHierarchy) return false;
            var controller = actor.GetComponent<CharacterController>();
            if (controller == null || !controller.enabled) return false;
            // Evaluate current contact every tick: the first player can wait for the
            // second, without relying on enter events or a point inside the body.
            return Physics.ComputePenetration(interior, interior.transform.position, interior.transform.rotation,
                controller, controller.transform.position, controller.transform.rotation, out _, out _);
        }
        private bool Reach(StageActor actor, Vector3 target)
        {
            Vector3 origin = actor.transform.position + settings.interactionOffset;
            Vector3 delta = target - origin;
            if (delta.sqrMagnitude > settings.interactionRange * settings.interactionRange) return false;
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, settings.obstructionMask, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<StageActor>() == null && hit.collider.GetComponentInParent<HerbSource>() == null) return false;
            return true;
        }
        internal StageInteraction ResolveInteraction(StageActor actor, out Vector3 point, out int target)
        {
            point = default;
            target = -1;
            int slot = actor.Slot;
            if (Progress == null || Progress.Cleared || Minigame != StageMinigameKind.None || slot is < 0 or > 1)
                return StageInteraction.None;

            int otherSlot = 1 - slot;
            var other = Actors[otherSlot];
            Vector3 origin = actor.transform.position + settings.interactionOffset;
            Vector3 otherPoint = other != null ? other.transform.position + settings.interactionOffset : default;
            int heldId = Progress.Held[slot];
            var held = settings.Item(heldId);

            if (TryHerbTransferPair(slot, out int donor))
            {
                point = otherPoint;
                target = donor;
                return slot == donor ? StageInteraction.OfferHerb : StageInteraction.Receive;
            }
            if (held?.type == StageItemType.Potion)
            {
                Vector3 direction = Vector3.ProjectOnPlane(otherPoint - origin, Vector3.up);
                Vector3 facing = Quaternion.Euler(0, actor.InteractionYaw, 0) * Vector3.forward;
                if (other != null && Progress.Held[otherSlot] == 0 && Progress.Consumed[otherSlot] == 0 &&
                    !other.Abilities.Unlocked && direction.sqrMagnitude > .0001f &&
                    Vector3.Angle(facing, direction) <= settings.potionTransferFacingAngle && Reach(actor, otherPoint))
                {
                    point = otherPoint;
                    target = otherSlot;
                    return StageInteraction.Give;
                }
                point = origin;
                return CanDrink(actor) ? StageInteraction.Drink : StageInteraction.None;
            }
            if (held != null)
            {
                if (settings.slotRoles[slot] == CharacterRole.Sodam && held.type == StageItemType.Herb)
                {
                    target = settings.RecipeIndexForIngredient(heldId);
                    point = origin;
                    return target >= 0 && other != null ? StageInteraction.Brew : StageInteraction.None;
                }
                point = otherPoint;
                target = otherSlot;
                return other != null && Progress.Held[otherSlot] == 0 && Reach(actor, point)
                    ? StageInteraction.Give : StageInteraction.None;
            }
            if (RoleFor(actor) != CharacterRole.Duyeong || (other == null && !IsSoloRoleTest(actor)))
                return StageInteraction.None;

            float nearest = float.PositiveInfinity;
            for (int i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                if (source == null || (Progress.Harvested & (1UL << i)) != 0 || !Reach(actor, source.Point)) continue;
                float distance = (origin - source.Point).sqrMagnitude;
                if (distance >= nearest) continue;
                nearest = distance;
                target = i;
                point = source.Point;
            }
            return target >= 0 ? StageInteraction.Gather : StageInteraction.None;
        }

        private bool CanDrink(StageActor actor) => !actor.Abilities.Unlocked && Progress.Consumed[actor.Slot] == 0;
        private bool Drink(StageActor actor) => Progress.Consume(actor.Slot, item =>
            item.ability == PlayerAbilityKind.Leaf ? actor.Abilities.UnlockLeaf() : actor.Abilities.UnlockSap());

        internal bool CanFan(StageActor actor) => Minigame == StageMinigameKind.Crafting &&
            actor.Slot is >= 0 and < 2 && settings.slotRoles[actor.Slot] == CharacterRole.Sodam;

        public bool CanCancelMinigame(StageActor actor) => actor != null && actor.Available && Minigame != StageMinigameKind.None &&
            ((Minigame == StageMinigameKind.Gathering && RoleFor(actor) == CharacterRole.Duyeong) ||
             (Minigame == StageMinigameKind.Crafting && RoleFor(actor) == CharacterRole.Sodam));
        public string Execute(StageActor actor, StageCommand command)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused || !Authority || actor == null || !actor.Available || Progress.Cleared) return settings.waitingMessage;
            RefreshActors(); int slot = actor.Slot;
            if (slot < 0 || slot >= Actors.Length || Actors[slot] != actor) return settings.waitingMessage;
            if (Minigame != StageMinigameKind.None)
            {
                if (command == StageCommand.CancelMinigame && CanCancelMinigame(actor))
                {
                    EndMinigame(showResult: false);
                    return settings.minigameCancelledMessage;
                }
                if (Minigame == StageMinigameKind.Gathering && command >= StageCommand.Up && command <= StageCommand.Right && RoleFor(actor) == CharacterRole.Duyeong)
                {
                    var steps = settings.Item(MinigameItemId)?.harvestingSteps;
                    if (steps == null || MinigameSteps >= steps.Length || (StageDirection)((int)command - (int)StageCommand.Up) != steps[MinigameSteps])
                    { EndMinigame(); return settings.minigameFailedMessage; }
                    MinigameSteps++;
                    if (MinigameSteps == steps.Length)
                    {
                        bool gathered = Progress.Gather(slot, _sourceIndex, MinigameItemId, RoleFor(actor));
                        EndMinigame(gathered); Present(); adapter?.Publish();
                        return gathered ? settings.successMessage : settings.invalidItemMessage;
                    }
                    adapter?.Publish(); return string.Empty;
                }
                if (CanFan(actor) && command == StageCommand.Craft)
                {
                    TrimFanPresses(); _fanPressTimes.Enqueue(MinigameTime); FanPressCount = _fanPressTimes.Count;
                    MinigameSteps++;
                    adapter?.Publish(); return string.Empty;
                }
                return settings.waitingMessage;
            }
            bool done = false;
            if (command == StageCommand.Interact)
            {
                var interaction = ResolveInteraction(actor, out _, out int target);
                switch (interaction)
                {
                    case StageInteraction.Give:
                        done = Progress.Give(slot, target);
                        break;
                    case StageInteraction.OfferHerb:
                    case StageInteraction.Receive:
                        if (TransferPendingSlot != 1 - slot || TransferRemaining <= 0)
                        {
                            TransferPendingSlot = slot;
                            TransferRemaining = settings.transferConfirmSeconds;
                            adapter?.Publish();
                            return settings.transferRequestedMessage;
                        }
                        TransferPendingSlot = -1;
                        TransferRemaining = 0;
                        done = Progress.Give(target, 1 - target);
                        break;
                    case StageInteraction.Brew:
                        StartCrafting(target);
                        return settings.minigameStartedMessage;
                    case StageInteraction.Gather:
                        StartGathering(actor, target);
                        return settings.minigameStartedMessage;
                    case StageInteraction.Drink:
                        done = Drink(actor);
                        break;
                    default:
                        return string.Empty;
                }
            }
            else if (command == StageCommand.Craft)
            {
                if (settings.slotRoles[slot] != CharacterRole.Sodam) return settings.roleMessage;
                int recipeIndex = settings.RecipeIndexForIngredient(Progress.Held[slot]);
                if (recipeIndex < 0) return settings.invalidItemMessage;
                if (Actors[0] == null || Actors[1] == null) return settings.waitingMessage;
                StartCrafting(recipeIndex); return settings.minigameStartedMessage;
            }
            else if (command == StageCommand.Drink)
            {
                if (!CanDrink(actor)) return settings.drinkBlockedMessage;
                done = Drink(actor);
            }
            if (done) { Present(); if (adapter != null) adapter.Publish(); }
            return done ? settings.successMessage : settings.invalidItemMessage;
        }
        public void SetReplica(ulong harvested, uint crafted, int held0, int held1, int consumed0, int consumed1, bool cleared, int inside,
            StageMinigameKind minigame, int minigameItemId, int steps, float temperature, float craftingGauge, float minigameTime, int fanPressCount,
            int transferPendingSlot, float transferRemaining, uint minigameResultSequence, bool minigameSucceeded)
        {
            if (Authority || Progress == null) return;
            Progress.Harvested = harvested; Progress.Crafted = crafted; Progress.Held[0] = held0; Progress.Held[1] = held1;
            Progress.Consumed[0] = consumed0; Progress.Consumed[1] = consumed1; Progress.Cleared = cleared; InsideCount = inside;
            Minigame = minigame; MinigameItemId = minigameItemId; MinigameSteps = steps;
            Temperature = temperature; CraftingGauge = craftingGauge; MinigameTime = minigameTime; FanPressCount = fanPressCount;
            TransferPendingSlot = transferPendingSlot; TransferRemaining = transferRemaining;
            MinigameResultSequence = minigameResultSequence; MinigameSucceeded = minigameSucceeded;
        }
        private void Present()
        {
            for (int i = 0; i < sources.Length; i++) if (sources[i] != null) sources[i].Present((Progress.Harvested & (1UL << i)) != 0);
            if (entranceBlocker != null) entranceBlocker.SetActive(!GateOpen);
            if (GateOpen && !openNotified) { openNotified = true; onEntranceOpened.Invoke(); }
            if (Progress.Cleared && !clearNotified) { clearNotified = true; onCleared.Invoke(); }
        }
        public string Objective => Progress.Cleared ? settings.clearMessage : GateOpen ? settings.enterMessage : Progress.GateOpen && entranceCondition is Herbalist.Levels.ExteriorStageFlow exterior ? exterior.Objective : (Progress.Crafted & settings.RequiredMask) == settings.RequiredMask ? settings.drinkMessage : Progress.Harvested != 0 ? settings.craftMessage : settings.gatherMessage;
        private void OnDestroy() { if (Instance == this) Instance = null; }
        private void OnDisable() { Minigame = StageMinigameKind.None; ApplyMinigameLocks(); }
    }
}
