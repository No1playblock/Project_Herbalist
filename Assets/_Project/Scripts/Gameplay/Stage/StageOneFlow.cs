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
            Temperature = recipe.initialTemperature; CraftingGauge = 0; MinigameTime = 0;
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
        public bool TryInteractionHint(StageActor actor, out Vector3 point, out bool transfer)
        {
            point = default; transfer = false;
            if (actor == null || Progress == null || Progress.Cleared || Minigame != StageMinigameKind.None || actor.Slot < 0 || actor.Slot > 1) return false;
            int slot = actor.Slot;
            if (Herbalist.Levels.SoloPotionPickup.TryHint(actor, out point)) return true;
            if (Progress.Held[slot] != 0)
            {
                if (settings.slotRoles[slot] == CharacterRole.Sodam && settings.Item(Progress.Held[slot])?.type == StageItemType.Herb &&
                    settings.RecipeIndexForIngredient(Progress.Held[slot]) >= 0)
                { point = actor.transform.position + settings.interactionOffset; return true; }
                var other = Actors[1-slot];
                if (other == null || Progress.Held[1-slot] != 0) return false;
                point = other.transform.position + settings.interactionOffset;
                transfer = true; return Reach(actor, point);
            }
            if (TryHerbTransferPair(slot, out _) && settings.slotRoles[slot] == CharacterRole.Sodam)
            {
                point = Actors[settings.SlotForRole(CharacterRole.Duyeong)].transform.position + settings.interactionOffset;
                transfer = true; return true;
            }
            if (RoleFor(actor) != CharacterRole.Duyeong) return false;
            float best = float.PositiveInfinity; bool found = false;
            for (int i=0;i<sources.Length;i++)
            {
                if (sources[i] == null || (Progress.Harvested & (1UL<<i)) != 0 || !Reach(actor,sources[i].Point)) continue;
                float d=(actor.transform.position-sources[i].Point).sqrMagnitude;
                if (d>=best) continue;
                best=d; point=sources[i].Point; found=true;
            }
            return found;
        }
        public bool IsCraftHint(StageActor actor) => actor != null && Progress != null && actor.Slot >= 0 && actor.Slot < 2 &&
            settings.slotRoles[actor.Slot] == CharacterRole.Sodam && settings.Item(Progress.Held[actor.Slot])?.type == StageItemType.Herb;
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
                if (Minigame == StageMinigameKind.Crafting && settings.slotRoles[slot] == CharacterRole.Sodam)
                {
                    if (command == StageCommand.Craft)
                    {
                        TrimFanPresses(); _fanPressTimes.Enqueue(MinigameTime); FanPressCount = _fanPressTimes.Count;
                        adapter?.Publish(); return string.Empty;
                    }
                }
                return settings.waitingMessage;
            }
            bool done = false;
            if (command == StageCommand.Interact)
            {
                if (TryHerbTransferPair(slot, out int donor))
                {
                    if (TransferPendingSlot == 1 - slot && TransferRemaining > 0)
                    {
                        TransferPendingSlot = -1; TransferRemaining = 0;
                        done = Progress.Give(donor, 1 - donor);
                        if (done) { Present(); adapter?.Publish(); }
                        return done ? settings.successMessage : settings.invalidItemMessage;
                    }
                    TransferPendingSlot = slot; TransferRemaining = settings.transferConfirmSeconds;
                    adapter?.Publish(); return settings.transferRequestedMessage;
                }
                if (Progress.Held[slot] != 0)
                {
                    if (settings.slotRoles[slot] == CharacterRole.Sodam && settings.Item(Progress.Held[slot])?.type == StageItemType.Herb)
                    {
                        int recipeIndex = settings.RecipeIndexForIngredient(Progress.Held[slot]);
                        if (recipeIndex < 0) return settings.invalidItemMessage;
                        if (Actors[0] == null || Actors[1] == null) return settings.waitingMessage;
                        StartCrafting(recipeIndex); return settings.minigameStartedMessage;
                    }
                    int other = 1 - slot;
                    if (Actors[other] == null || !Reach(actor, Actors[other].transform.position + settings.interactionOffset)) return settings.rangeMessage;
                    if (Progress.Held[other] != 0) return settings.fullMessage;
                    done = Progress.Give(slot, other);
                }
                else
                {
                    if (RoleFor(actor) != CharacterRole.Duyeong) return settings.roleMessage;
                    int best = -1; float distance = float.PositiveInfinity;
                    for (int i = 0; i < sources.Length; i++)
                    {
                        if (sources[i] == null || (Progress.Harvested & (1UL << i)) != 0 || !Reach(actor, sources[i].Point)) continue;
                        float d = (actor.transform.position + settings.interactionOffset - sources[i].Point).sqrMagnitude;
                        if (d < distance) { best = i; distance = d; }
                    }
                    if (best < 0) return settings.rangeMessage;
                    if ((Actors[0] == null || Actors[1] == null) && !IsSoloRoleTest(actor)) return settings.waitingMessage;
                    StartGathering(actor, best); return settings.minigameStartedMessage;
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
                if (actor.Abilities.Unlocked || Progress.Consumed[slot] != 0) return settings.drinkBlockedMessage;
                done = Progress.Consume(slot, item => item.ability == PlayerAbilityKind.Leaf ? actor.Abilities.UnlockLeaf() : actor.Abilities.UnlockSap());
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
