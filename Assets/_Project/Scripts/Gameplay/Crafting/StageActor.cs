using UnityEngine;
using UnityEngine.InputSystem;
using Herbalist.Player;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.Levels;
namespace Herbalist.StageOne
{
    public enum StageCommand { Interact, Craft, Drink, Up, Left, Down, Right, FanEnd, CancelMinigame, ReleaseCloth, Recover }
    public enum StageInteraction { None, Gather, Give, OfferHerb, Receive, Brew, Drink, PickupPotion, GrabCloth, ReleaseCloth, BoardTalisman }
    [RequireComponent(typeof(PlayerController))]
    public sealed class StageActor : MonoBehaviour
    {
        public static readonly System.Collections.Generic.HashSet<StageActor> All = new System.Collections.Generic.HashSet<StageActor>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetActors() { All.Clear(); }
        private void OnEnable() { All.Add(this); }
        public InputActionReference interact, craft, drink;
        public int offlineSlot;
        private InputAction _interactAction;
        private InputAction _fanAction;
        private InputAction _recoverAction;
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
            if (interact == null) { enabled = false; return; }
            _interactAction = interact.action.Clone();
            _interactAction.performed += _ => Request(StageCommand.Interact);
            if (craft != null)
            {
                _fanAction = craft.action.Clone();
                _fanAction.performed += _ => Request(StageCommand.Craft);
            }
            player.Input.MovePerformed += OnMovePerformed;
        }
        private void Start()
        {
            var recover = Herbalist.Levels.StageOneTraversal.Instance?.RecoverAction;
            if (recover == null) return;
            _recoverAction = recover.action.Clone();
            _recoverAction.performed += _ => Request(StageCommand.Recover);
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
            if (_interactAction != null && active != _interactAction.enabled)
            {
                if (active) _interactAction.Enable();
                else _interactAction.Disable();
            }
            bool canFan = active && flow != null && flow.CanFan(this);
            if (_fanAction != null && canFan != _fanAction.enabled)
            {
                if (canFan) _fanAction.Enable();
                else _fanAction.Disable();
            }
            if (_recoverAction != null)
            {
                if (active && !_recoverAction.enabled) _recoverAction.Enable();
                else if (!active && _recoverAction.enabled) _recoverAction.Disable();
            }
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
            if (command == StageCommand.Craft && StageOneFlow.Instance?.CanFan(this) != true) return;
            var interaction = command == StageCommand.Interact ? ResolveInteraction(out _) : StageInteraction.None;
            if (command == StageCommand.Interact && interaction == StageInteraction.None) return;
            LocalCommandRequested?.Invoke(command);
            if (adapter != null && adapter.Object != null && adapter.Object.IsValid) adapter.RPC_Command(command, interaction);
            else SetFeedback(ExecuteCommand(command, interaction));
        }
        public string ExecuteCommand(StageCommand command, StageInteraction expected)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused || !Available) return string.Empty;
            if (command == StageCommand.Recover)
            {
                Herbalist.Levels.StageOneTraversal.Instance?.TryRecover(this);
                return string.Empty;
            }
            var flow = StageOneFlow.Instance;
            if (command == StageCommand.Craft && flow?.CanFan(this) != true) return string.Empty;
            if (command == StageCommand.Interact)
            {
                if (flow != null && flow.Authority) flow.RefreshActors();
                var interaction = ResolveInteraction(out _);
                // A stale client hint must never turn an offer into drinking the potion.
                if (interaction != expected) return string.Empty;
                switch (interaction)
                {
                    case StageInteraction.PickupPotion:
                        return SoloPotionPickup.TryPickup(this, out var potionMessage) ? potionMessage : string.Empty;
                    case StageInteraction.GrabCloth:
                    case StageInteraction.ReleaseCloth:
                        return HangingCloth.TryInteract(this, out var clothMessage) ? clothMessage : string.Empty;
                    case StageInteraction.BoardTalisman:
                        return BranchRideFlow.TryBoard(this, out var rideMessage) ? rideMessage : string.Empty;
                    case StageInteraction.None:
                        return string.Empty;
                }
            }
            if (command == StageCommand.ReleaseCloth)
                return Herbalist.Levels.HangingCloth.TryRelease(this, out var releaseMessage) ? releaseMessage : string.Empty;
            return flow != null ? flow.Execute(this, command) : string.Empty;
        }
        public StageInteraction ResolveInteraction(out Vector3 point)
        {
            point = default;
            var flow = StageOneFlow.Instance;
            if (!Available || Herbalist.GameUI.GameplayPause.IsPaused ||
                (flow != null && flow.Minigame != StageMinigameKind.None)) return StageInteraction.None;
            if (HangingCloth.TryInteractionHint(this, out point, out bool release))
                return release ? StageInteraction.ReleaseCloth : StageInteraction.GrabCloth;
            if (HangingCloth.IsGrabbing(this) || player.Motor.MovementLocked) return StageInteraction.None;
            if (BranchRideFlow.TryBoardHint(this, out point)) return StageInteraction.BoardTalisman;
            if (SoloPotionPickup.TryHint(this, out point)) return StageInteraction.PickupPotion;
            return flow != null ? flow.ResolveInteraction(this, out point, out _) : StageInteraction.None;
        }
        internal float InteractionYaw => networkPlayer != null && networkPlayer.Object != null &&
            networkPlayer.Object.IsValid && !Local ? networkPlayer.LookAngles.x : player.View.Yaw;
        public void SetFeedback(string text) { Feedback = text; }
        public string InteractBinding => _interactAction?.GetBindingDisplayString() ?? string.Empty;
        public string FanBinding => _fanAction?.GetBindingDisplayString() ?? string.Empty;
        private void OnDisable() { All.Remove(this); _interactAction?.Disable(); _fanAction?.Disable(); _recoverAction?.Disable(); if (player != null && Camera != null) Camera.cullingMask = originalMask; }
        private void OnDestroy() { if (player != null && player.Input != null) player.Input.MovePerformed -= OnMovePerformed; _interactAction?.Dispose(); _fanAction?.Dispose(); _recoverAction?.Dispose(); }
    }
}
