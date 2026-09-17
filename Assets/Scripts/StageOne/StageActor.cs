using UnityEngine;
using UnityEngine.InputSystem;
using Herbalist.Player;
using Herbalist.Abilities;
using Herbalist.Networking;
namespace Herbalist.StageOne
{
    public enum StageCommand { Interact, Craft, Drink }
    [RequireComponent(typeof(PlayerController))]
    public sealed class StageActor : MonoBehaviour
    {
        public static readonly System.Collections.Generic.HashSet<StageActor> All = new System.Collections.Generic.HashSet<StageActor>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetActors() { All.Clear(); }
        private void OnEnable() { All.Add(this); }
        public InputActionReference interact, craft, drink;
        public int offlineSlot;
        private InputAction[] actions;
        private PlayerController player;
        private NetworkPlayer networkPlayer;
        private NetworkStageActor adapter;
        public PlayerAbilityController Abilities { get; private set; }
        public int Slot => networkPlayer != null && networkPlayer.Object != null && networkPlayer.Object.IsValid ? networkPlayer.Slot : offlineSlot;
        public bool Available => isActiveAndEnabled && (networkPlayer == null || (networkPlayer.Object != null && networkPlayer.Object.IsValid));
        public bool Local => Available && player.LocallyControlled;
        public Camera Camera => player.View.Camera;
        public string Feedback { get; private set; }
        private int originalMask;
        private void Awake()
        {
            player = GetComponent<PlayerController>(); Abilities = GetComponent<PlayerAbilityController>();
            networkPlayer = GetComponent<NetworkPlayer>(); adapter = GetComponent<NetworkStageActor>();
            originalMask = Camera.cullingMask;
            if (interact == null || craft == null || drink == null) { enabled = false; return; }
            actions = new[] { interact.action.Clone(), craft.action.Clone(), drink.action.Clone() };
            for (int i = 0; i < actions.Length; i++) { int index = i; actions[i].performed += _ => Request((StageCommand)index); }
        }
        private void Update()
        {
            var flow = StageOneFlow.Instance;
            bool active = Local && flow != null && !flow.Progress.Cleared;
            if (actions != null) foreach (var action in actions) { if (active && !action.enabled) action.Enable(); else if (!active && action.enabled) action.Disable(); }
            if (flow != null && Available && Slot >= 0 && Slot < flow.settings.slotRoles.Length)
            {
                int bit = 1 << flow.settings.herbGlowLayer;
                Camera.cullingMask = flow.settings.slotRoles[Slot] == Herbalist.Presentation.CharacterRole.Duyeong ? originalMask | bit : originalMask & ~bit;
            }
        }
        public void Request(StageCommand command)
        {
            if (!Local || StageOneFlow.Instance == null || !Herbalist.Presentation.GameplayCursor.AllowsPointerInput) return;
            if (adapter != null && adapter.Object != null && adapter.Object.IsValid) adapter.RPC_Command(command);
            else SetFeedback(StageOneFlow.Instance.Execute(this, command));
        }
        public void SetFeedback(string text) { Feedback = text; }
        public string Binding(int index) => actions != null ? actions[index].GetBindingDisplayString() : string.Empty;
        private void OnDisable() { All.Remove(this); if (actions != null) foreach (var a in actions) a.Disable(); if (player != null && Camera != null) Camera.cullingMask = originalMask; }
        private void OnDestroy() { if (actions != null) foreach (var a in actions) a.Dispose(); }
    }
}
