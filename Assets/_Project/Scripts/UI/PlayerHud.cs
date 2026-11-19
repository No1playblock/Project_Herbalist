using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Herbalist.Abilities;
using Herbalist.StageOne;
using Herbalist.Presentation;
using Herbalist.Levels;
namespace Herbalist.GameUI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Herbalist.GameUI", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerHudUI")]
    public sealed class PlayerHud : MonoBehaviour
    {
        public GameUiSettings settings;
        public int slot;
        public GameObject helpPanel, reticle, prompt, clothPrompt, sapPrompt;
        public TMP_Text helpText, helpHint, pocket, abilityText, feedback, promptText, clothPromptText, sapPromptText;
        public TMP_Text jumpPadCountdown;
        [SerializeField] private SapMazeVolumeDisplay _mazeVolumeDisplay;
        [SerializeField] private ControlHelpView _helpControls;
        [SerializeField] private ControlHintView _helpHintView, _interactionHintView, _clothHintView, _sapHintView;
        public GameObject leafStatus;
        public Image[] leafStatusDots;
        [SerializeField] private UnityEngine.UI.Image[] _leafReturnFills;
        public TMP_Text[] leafStatusLabels;
        public RectTransform[] markers;
        public TMP_Text[] markerLabels;
        public Vector3 promptOffset=new Vector3(0,.3f,0);
        private PlayScreenController screen;
        private InputAction help;
        private bool expanded;
        private string previousFeedback;
        private float feedbackTime;
        private uint _abilityFeedback;
        private uint _minigameResult;
        private StageNarrativePresenter _narrative;
        [SerializeField] private RectTransform _questPanel;
        [SerializeField] private RectTransform[] _questLayoutTargets = System.Array.Empty<RectTransform>();
        [SerializeField, Min(0)] private float _questRightMargin = 24f;
        private Vector3[] _questOriginalPositions;
        private void Awake()
        {
            screen=FindFirstObjectByType<PlayScreenController>();
            _narrative=FindAnyObjectByType<StageNarrativePresenter>();
            _questOriginalPositions = new Vector3[_questLayoutTargets.Length];
            for (int i = 0; i < _questLayoutTargets.Length; i++)
                if (_questLayoutTargets[i] != null) _questOriginalPositions[i] = _questLayoutTargets[i].anchoredPosition3D;
        }
        private void OnEnable()
        {
            foreach (var marker in markers)
                if (marker != null) marker.gameObject.SetActive(false);
            help=settings.helpAction.action.Clone(); help.performed+=_=>expanded=!expanded; help.Enable();
        }
        private void Update()
        {
            PositionQuestPanel((RectTransform)transform, _questPanel, _questLayoutTargets,
                _questOriginalPositions, screen != null && screen.IsSplitVisible, _questRightMargin);
            var view=screen!=null?screen.ViewForSlot(slot):null;
            if(view==null)
            {
                if (markers.Length > 0 && markers[0] != null) markers[0].gameObject.SetActive(false);
                if (_mazeVolumeDisplay != null) _mazeVolumeDisplay.Present(0, 0, false);
                if(jumpPadCountdown!=null) jumpPadCountdown.gameObject.SetActive(false);
                prompt.SetActive(false);
                if(clothPrompt!=null) clothPrompt.SetActive(false);
                if(sapPrompt!=null) sapPrompt.SetActive(false);
                if(leafStatus!=null) leafStatus.SetActive(false);
                return;
            }
            var player=view.GetComponent<Herbalist.Player.PlayerController>();
            bool local=player.LocallyControlled;
            PresentNavigation(local, view.Camera);
            var maze = SapMazeBoard.LocalMazeView;
            bool mazeView = maze != null && maze.IsViewedBy(view);
            helpHint.gameObject.SetActive(!mazeView);
            pocket.gameObject.SetActive(!mazeView);
            abilityText.gameObject.SetActive(!mazeView);
            feedback.gameObject.SetActive(!mazeView);
            if (jumpPadCountdown != null)
            {
                float remaining = 0;
                bool show = local && ExteriorJumpPad.TryGetLaunchCountdown(out remaining);
                jumpPadCountdown.gameObject.SetActive(show);
                if (show) jumpPadCountdown.text = string.Format(settings.jumpPadCountdownFormat, remaining);
            }
            PresentHint(_helpHintView, helpHint, settings.helpAction.action.GetBindingDisplayString(), settings.helpHint);
            bool showHelp = local && expanded && !mazeView;
            helpPanel.SetActive(showHelp);
            if(showHelp)
            {
                if (_helpControls != null) _helpControls.Present(settings, (RectTransform)helpPanel.transform);
                else
                {
                    var lines = new System.Text.StringBuilder();
                    foreach (var h in settings.controls)
                        if (settings.ShowsInControlHelp(h))
                            lines.AppendLine(string.Format(settings.controlHintFormat, h.Action.action.GetBindingDisplayString(), h.Label));
                    helpText.text = lines.ToString();
                }
            }
            var ability=view.GetComponent<PlayerAbilityController>();
            if (_mazeVolumeDisplay != null)
            {
                var mazeFlow = StageLevel.Instance != null ? StageLevel.Instance.GetComponent<DescendingMazeFlow>() : null;
                var board = mazeFlow != null ? mazeFlow.CurrentBoard : null;
                bool show = local && ability != null && ability.Unlocked && ability.Kind == PlayerAbilityKind.Leaf &&
                    board != null && board.Active && board.UsesRefillVolumeRules && board.State.Phase == MazePhase.Running;
                _mazeVolumeDisplay.Present(board != null ? board.RemainingVolume : 0,
                    board != null ? board.RequiredVolume : 0, show);
            }
            if (leafStatus != null)
            {
                bool showLeaves = !mazeView && local && ability != null && ability.Unlocked && ability.Kind == PlayerAbilityKind.Leaf;
                leafStatus.SetActive(showLeaves);
                if (showLeaves)
                {
                    int packed = ability.DisplayLeafSlotPresentation;
                    for (int i = 0; i < leafStatusDots.Length; i++)
                    {
                        int state = (packed >> (i * 2)) & 3;
                        var icon = leafStatusDots[i];
                        if (settings.LeafRecallAvailableIcon != null && settings.LeafRecallUnavailableIcon != null)
                        {
                            icon.sprite = state == 0 ? settings.LeafRecallAvailableIcon : settings.LeafRecallUnavailableIcon;
                            icon.color = Color.white;
                            if (_leafReturnFills != null && i < _leafReturnFills.Length && _leafReturnFills[i] != null)
                            {
                                var fill = _leafReturnFills[i];
                                fill.gameObject.SetActive(state == 2);
                                if (state == 2)
                                {
                                    fill.sprite = settings.LeafRecallAvailableIcon;
                                    fill.fillAmount = LeafThrowAbility.ReadSlotReturnProgress(packed, i);
                                }
                            }
                        }
                        else
                        {
                            icon.color = state == 2 ? settings.leafReturningColor :
                                state == 1 ? settings.leafInstalledColor : settings.leafHeldColor;
                        }
                        leafStatusLabels[i].text = state == 2 ? settings.leafReturningText :
                            state == 1 ? settings.leafInstalledText : settings.leafHeldText;
                    }
                }
            }
            var rules = Herbalist.Levels.StageLevel.Instance != null ? Herbalist.Levels.StageLevel.Instance.abilityRules : null;
            reticle.SetActive(!mazeView&&local&&ability!=null&&ability.Unlocked&&(rules != null || ability.Kind!=PlayerAbilityKind.Sap || ability.Sap.Controlling));
            abilityText.text=ability!=null&&ability.Unlocked?(ability.Kind==PlayerAbilityKind.Sap?settings.sapText:settings.leafText):"";
            if (rules != null && ability != null && ability.Unlocked) abilityText.text = ability.Kind == PlayerAbilityKind.Sap ? rules.sapInstructions : rules.leafInstructions;
            pocket.text="";
            var flow=StageOneFlow.Instance;
            var actor=view.GetComponent<StageActor>();
            if (actor != null && actor.Feedback != previousFeedback) { previousFeedback = actor.Feedback; feedbackTime = settings.feedbackDuration; }
            if (local && rules != null && ability != null && ability.Leaf.FeedbackSequence != _abilityFeedback)
            {
                _abilityFeedback = ability.Leaf.FeedbackSequence;
                var messages = slot == 0 ? rules.duyeongRangeMessages : rules.sodamRangeMessages;
                previousFeedback = ability.Leaf.RangeRejected && messages.Length > 0 ? messages[Random.Range(0, messages.Length)] : rules.invalidTargetMessage;
                feedbackTime = settings.feedbackDuration;
            }
            if(flow!=null&&actor!=null&&actor.Available)
            {
                var item=flow.settings.Item(flow.Progress.Held[actor.Slot]); pocket.text=item!=null?item.displayName:settings.emptyPocket;
                if(actor.Feedback!=previousFeedback) { previousFeedback=actor.Feedback; feedbackTime=settings.feedbackDuration; }
            }
            if (local && flow != null && flow.MinigameResultSequence != _minigameResult)
            {
                _minigameResult = flow.MinigameResultSequence;
                previousFeedback = flow.MinigameSucceeded ? flow.settings.successMessage : flow.settings.minigameFailedMessage;
                feedbackTime = settings.feedbackDuration;
            }
            if(!GameplayPause.IsPaused)feedbackTime-=Time.unscaledDeltaTime;
            feedback.text=local&&feedbackTime>0?previousFeedback:"";
            prompt.SetActive(false);
            if (clothPrompt != null) clothPrompt.SetActive(false);
            StageInteraction interaction = StageInteraction.None;
            if (local && actor != null)
            {
                interaction = actor.ResolveInteraction(out var point);
                string description = interaction switch
                {
                    StageInteraction.Gather => settings.gatherText,
                    StageInteraction.Give or StageInteraction.OfferHerb => settings.giveText,
                    StageInteraction.Receive => settings.receiveText,
                    StageInteraction.Brew => settings.craftText,
                    StageInteraction.Drink => settings.drinkText,
                    StageInteraction.PickupPotion => settings.pickupPotionText,
                    StageInteraction.GrabCloth => settings.clothGrabText,
                    StageInteraction.ReleaseCloth => settings.clothReleaseText,
                    StageInteraction.BoardTalisman => settings.boardTalismanText,
                    StageInteraction.DismountTalisman => settings.dismountTalismanText,
                    _ => null
                };
                if (description != null)
                {
                    if (interaction == StageInteraction.ReleaseCloth && !string.IsNullOrEmpty(player.Input.JumpBinding))
                        description += $" ({player.Input.JumpBinding})";
                    bool useClothPrompt = clothPrompt != null && interaction is StageInteraction.GrabCloth or StageInteraction.ReleaseCloth;
                    PresentHint(useClothPrompt ? _clothHintView : _interactionHintView,
                        useClothPrompt ? clothPromptText : promptText, actor.InteractBinding, description);
                    Position((RectTransform)(useClothPrompt ? clothPrompt : prompt).transform, view.Camera, point + promptOffset);
                }
            }
            if (sapPrompt != null)
            {
                sapPrompt.SetActive(false);
                if (local && actor != null && SapInjectionPort.TryInteractionHint(actor, out var portPoint))
                {
                    string binding = rules != null && rules.usePrimaryButton ? ability.Input.UseBinding : ability.Input.CycleBinding;
                    PresentHint(_sapHintView, sapPromptText, binding, settings.sapInjectText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, portPoint + promptOffset);
                }
                else if (local && !mazeView && rules is { contextualUse: true } &&
                    HangingCloth.TryWaterPushHint(actor, view.GetAimRay(view.Yaw, view.Pitch), out var clothPoint))
                {
                    string binding = rules.usePrimaryButton ? ability.Input.UseBinding : ability.Input.CycleBinding;
                    PresentHint(_sapHintView, sapPromptText, binding, settings.sapPushClothText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, clothPoint + promptOffset);
                    if (sapPrompt.activeSelf && interaction == StageInteraction.GrabCloth && clothPrompt != null)
                        clothPrompt.SetActive(false);
                }
                else if (local && actor != null &&
                    ExteriorJumpPad.TryLeafInstallHint(actor, out var leafPoint))
                {
                    string binding = rules != null && rules.usePrimaryButton ? ability.Input.UseBinding : ability.Input.CycleBinding;
                    PresentHint(_sapHintView, sapPromptText, binding, settings.leafInstallText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, leafPoint + promptOffset);
                }
            }
        }
        internal static void PositionQuestPanel(RectTransform root, RectTransform panel, RectTransform[] targets,
            Vector3[] originals, bool split, float margin)
        {
            if (panel == null || originals == null) return;
            for (int i = 0; i < targets.Length && i < originals.Length; i++)
                // CanvasScaler and viewport layout can resize the parent after Awake.
                // Preserve authored anchor offsets instead of the pre-layout local pose.
                if (targets[i] != null) targets[i].anchoredPosition3D = originals[i];
            if (split) return;
            var canvas = root.GetComponentInParent<Canvas>();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            // Positions always start from the authored pose, including after a mode change.
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(root, panel);
            float delta = root.rect.xMax - margin / Mathf.Max(.001f, scale) - bounds.max.x;
            Vector3 worldDelta = root.TransformVector(Vector3.right * delta);
            foreach (var target in targets)
                if (target != null) target.position += worldDelta;
        }
        private void PresentHint(ControlHintView view, TMP_Text fallback, string binding, string description)
        {
            if (view != null) view.Present(binding, description);
            else if (fallback != null) fallback.text = string.Format(settings.controlHintFormat, binding, description);
        }
        private void Position(RectTransform rect,Camera camera,Vector3 point)
        {
            Vector3 pos=camera.WorldToScreenPoint(point);
            bool visible=pos.z>0&&camera.pixelRect.Contains(pos);
            rect.gameObject.SetActive(visible);
            if(visible&&RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,pos,null,out var local))
            {
                var bounds=((RectTransform)transform).rect; var half=rect.rect.size*.5f;
                local.x=Mathf.Clamp(local.x,bounds.xMin+half.x,bounds.xMax-half.x);
                local.y=Mathf.Clamp(local.y,bounds.yMin+half.y,bounds.yMax-half.y);
                rect.anchoredPosition=local;
            }
        }
        private void PresentNavigation(bool local, Camera camera)
        {
            if (markers.Length == 0 || markers[0] == null) return;
            var marker = markers[0];
            bool show = local && _narrative != null && !GameplayPause.IsPaused;
            Vector3 point = default;
            string label = string.Empty;
            show = show && _narrative.TryNavigationTarget(out point, out label);
            marker.gameObject.SetActive(show);
            if (!show) return;
            Vector3 viewport = camera.WorldToViewportPoint(point);
            if (viewport.z < 0f)
            {
                viewport.x = 1f - viewport.x;
                viewport.y = 1f - viewport.y;
            }
            Vector2 screenPoint = camera.pixelRect.min + Vector2.Scale(camera.pixelRect.size,
                new Vector2(Mathf.Clamp(viewport.x,.08f,.92f), Mathf.Clamp(viewport.y,.12f,.88f)));
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,
                screenPoint, null, out var position)) marker.anchoredPosition = position;
            if (markerLabels.Length > 0 && markerLabels[0] != null) markerLabels[0].text = label;
        }
        private void OnDisable() { help?.Dispose(); help=null; }
    }
}
