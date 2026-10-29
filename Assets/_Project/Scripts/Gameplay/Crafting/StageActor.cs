using UnityEngine;
using UnityEngine.InputSystem;
using Herbalist.Player;
using Herbalist.Abilities;
using Herbalist.Networking;
namespace Herbalist.StageOne
{
    public enum StageCommand { Interact, Craft, Drink, Up, Left, Down, Right, FanEnd, CancelMinigame, ReleaseCloth }
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
        public event System.Action<StageCommand> LocalCommandRequested;
        private int originalMask;
        private void Awake()
        {
            player = GetComponent<PlayerController>(); Abilities = GetComponent<PlayerAbilityController>();
            networkPlayer = GetComponent<NetworkPlayer>(); adapter = GetComponent<NetworkStageActor>();
            originalMask = Camera.cullingMask;
            if (interact == null || craft == null || drink == null) { enabled = false; return; }
            actions = new[] { interact.action.Clone(), craft.action.Clone(), drink.action.Clone() };
            for (int i = 0; i < actions.Length; i++) { int index = i; actions[i].performed += _ => Request((StageCommand)index); }
            player.Input.MovePerformed += OnMovePerformed;
            player.Input.JumpPerformed += OnJumpPerformed;
        }
        private void OnJumpPerformed()
        {
            if (Local && Herbalist.Levels.HangingCloth.IsRiding(this)) Request(StageCommand.ReleaseCloth);
        }
        private void OnMovePerformed(Vector2 move)
        {
            if (StageOneFlow.Instance == null || StageOneFlow.Instance.Minigame != StageMinigameKind.Gathering || !Local) return;
            if (StageOneFlow.Instance.RoleFor(this) != Herbalist.Presentation.CharacterRole.Duyeong) return;
            if (Mathf.Abs(move.x) < .5f && Mathf.Abs(move.y) < .5f) return;
            if (Mathf.Abs(move.x) > .5f && Mathf.Abs(move.y) > .5f) return;
            Request(Mathf.Abs(move.y) > .5f ? move.y > 0 ? StageCommand.Up : StageCommand.Down : move.x > 0 ? StageCommand.Right : StageCommand.Left);
        }
        private void Update()
        {
            var flow = StageOneFlow.Instance;
            bool active = Local && ((flow != null && !flow.Progress.Cleared) || (Herbalist.Levels.StageLevel.Instance != null && Herbalist.Levels.StageLevel.Instance.abilityRules != null));
            if (actions != null) foreach (var action in actions) { if (active && !action.enabled) action.Enable(); else if (!active && action.enabled) action.Disable(); }
            if (flow != null && Available && Slot >= 0 && Slot < flow.settings.slotRoles.Length)
            {
                int bit = 1 << flow.settings.herbGlowLayer;
                Camera.cullingMask = flow.RoleFor(this) == Herbalist.Presentation.CharacterRole.Duyeong ? originalMask | bit : originalMask & ~bit;
            }
        }
        public void Request(StageCommand command)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused || !Local ||
                (command != StageCommand.CancelMinigame && command != StageCommand.ReleaseCloth && !Herbalist.Presentation.GameplayCursor.AllowsPointerInput)) return;
            // Space is also Jump. E starts brewing; each Space press fans while crafting.
            if (command == StageCommand.Craft && StageOneFlow.Instance?.Minigame != StageMinigameKind.Crafting) return;
            LocalCommandRequested?.Invoke(command);
            if (adapter != null && adapter.Object != null && adapter.Object.IsValid) adapter.RPC_Command(command);
            else SetFeedback(ExecuteCommand(command));
        }
        public string ExecuteCommand(StageCommand command)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused || !Available) return string.Empty;
            if (command == StageCommand.Interact && Herbalist.Levels.SoloPotionPickup.TryPickup(this, out var potionMessage)) return potionMessage;
            if (command == StageCommand.Interact && Herbalist.Levels.HangingCloth.TryInteract(this, out var message)) return message;
            if (command == StageCommand.ReleaseCloth)
                return Herbalist.Levels.HangingCloth.TryRelease(this, out var releaseMessage) ? releaseMessage : string.Empty;
            if (command == StageCommand.Interact && Herbalist.Levels.BranchRideFlow.TryBoard(this, out var rideMessage)) return rideMessage;
            return StageOneFlow.Instance != null ? StageOneFlow.Instance.Execute(this, command) : string.Empty;
        }
        public void SetFeedback(string text) { Feedback = text; }
        public string Binding(int index) => actions != null ? actions[index].GetBindingDisplayString() : string.Empty;
        public string JumpBinding => player != null ? player.Input.JumpBinding : string.Empty;
        private void OnDisable() { All.Remove(this); if (actions != null) foreach (var a in actions) a.Disable(); if (player != null && Camera != null) Camera.cullingMask = originalMask; }
        private void OnDestroy() { if (player != null && player.Input != null) { player.Input.MovePerformed -= OnMovePerformed; player.Input.JumpPerformed -= OnJumpPerformed; } if (actions != null) foreach (var a in actions) a.Dispose(); }
    }
}
