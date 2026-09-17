using UnityEngine;
using UnityEngine.Events;
using Herbalist.Networking;
using Herbalist.Abilities;
using Herbalist.Presentation;
namespace Herbalist.StageOne
{
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
        public StageProgress Progress { get; private set; }
        public StageActor[] Actors { get; private set; } = new StageActor[2];
        public int InsideCount { get; private set; }
        private NetworkStageState adapter;
        private bool openNotified, clearNotified;
        public bool Authority => adapter != null && adapter.Object != null && adapter.Object.IsValid ? adapter.HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() { Instance = null; }
        private void Awake()
        {
            if (settings == null || !settings.Valid(out _) || sources == null || sources.Length > 64 || interior == null) { Debug.LogError("Stage One configuration is incomplete.", this); enabled = false; return; }
            Instance = this; Progress = new StageProgress(settings); adapter = GetComponent<NetworkStageState>();
        }
        private void Update()
        {
            RefreshActors();
            if (Authority && !Herbalist.GameUI.GameplayPause.IsPaused)
            {
                bool a = IsInside(Actors[0]), b = IsInside(Actors[1]); InsideCount = (a ? 1 : 0) + (b ? 1 : 0);
                Progress.CheckClear(a, b);
            }
            Present();
        }
        public void RefreshActors()
        {
            Actors[0] = Actors[1] = null;
            foreach (var actor in StageActor.All)
                if (actor.Available && actor.Slot >= 0 && actor.Slot < Actors.Length) Actors[actor.Slot] = actor;
        }
        private bool IsInside(StageActor actor)
        {
            if (actor == null) return false;
            Vector3 point = interior.transform.InverseTransformPoint(actor.transform.position + settings.interactionOffset) - interior.center;
            var half = interior.size * .5f;
            return Mathf.Abs(point.x) <= half.x && Mathf.Abs(point.y) <= half.y && Mathf.Abs(point.z) <= half.z;
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
        public string Execute(StageActor actor, StageCommand command)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused || !Authority || actor == null || !actor.Available || Progress.Cleared) return settings.waitingMessage;
            RefreshActors(); int slot = actor.Slot;
            if (slot < 0 || slot >= Actors.Length || Actors[slot] != actor) return settings.waitingMessage;
            bool done = false;
            if (command == StageCommand.Interact)
            {
                if (Progress.Held[slot] != 0)
                {
                    int other = 1 - slot;
                    if (Actors[other] == null || !Reach(actor, Actors[other].transform.position + settings.interactionOffset)) return settings.rangeMessage;
                    if (Progress.Held[other] != 0) return settings.fullMessage;
                    done = Progress.Give(slot, other);
                }
                else
                {
                    if (settings.slotRoles[slot] != CharacterRole.Duyeong) return settings.roleMessage;
                    int best = -1; float distance = float.PositiveInfinity;
                    for (int i = 0; i < sources.Length; i++)
                    {
                        if (sources[i] == null || (Progress.Harvested & (1UL << i)) != 0 || !Reach(actor, sources[i].Point)) continue;
                        float d = (actor.transform.position + settings.interactionOffset - sources[i].Point).sqrMagnitude;
                        if (d < distance) { best = i; distance = d; }
                    }
                    if (best < 0) return settings.rangeMessage;
                    done = Progress.Gather(slot, best, settings.ItemId(sources[best].item));
                }
            }
            else if (command == StageCommand.Craft)
            {
                if (settings.slotRoles[slot] != CharacterRole.Sodam) return settings.roleMessage;
                done = Progress.Craft(slot);
            }
            else if (command == StageCommand.Drink)
            {
                if (actor.Abilities.Unlocked || Progress.Consumed[slot] != 0) return settings.drinkBlockedMessage;
                done = Progress.Consume(slot, item => item.ability == PlayerAbilityKind.Leaf ? actor.Abilities.UnlockLeaf() : actor.Abilities.UnlockSap());
            }
            if (done) { Present(); if (adapter != null) adapter.Publish(); }
            return done ? settings.successMessage : settings.invalidItemMessage;
        }
        public void SetReplica(ulong harvested, uint crafted, int held0, int held1, int consumed0, int consumed1, bool cleared, int inside)
        {
            if (Authority || Progress == null) return;
            Progress.Harvested = harvested; Progress.Crafted = crafted; Progress.Held[0] = held0; Progress.Held[1] = held1;
            Progress.Consumed[0] = consumed0; Progress.Consumed[1] = consumed1; Progress.Cleared = cleared; InsideCount = inside;
        }
        private void Present()
        {
            for (int i = 0; i < sources.Length; i++) if (sources[i] != null) sources[i].Present((Progress.Harvested & (1UL << i)) != 0);
            if (entranceBlocker != null) entranceBlocker.SetActive(!Progress.GateOpen);
            if (Progress.GateOpen && !openNotified) { openNotified = true; onEntranceOpened.Invoke(); }
            if (Progress.Cleared && !clearNotified) { clearNotified = true; onCleared.Invoke(); }
        }
        public string Objective => Progress.Cleared ? settings.clearMessage : Progress.GateOpen ? settings.enterMessage : (Progress.Crafted & settings.RequiredMask) == settings.RequiredMask ? settings.drinkMessage : Progress.Harvested != 0 ? settings.craftMessage : settings.gatherMessage;
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
