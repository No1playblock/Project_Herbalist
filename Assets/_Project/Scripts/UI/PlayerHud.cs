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
        public GameObject leafStatus;
        public Image[] leafStatusDots;
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
        private void Awake() { screen=FindFirstObjectByType<PlayScreenController>(); }
        private void OnEnable()
        {
            foreach (var marker in markers)
                if (marker != null) marker.gameObject.SetActive(false);
            help=settings.helpAction.action.Clone(); help.performed+=_=>expanded=!expanded; help.Enable();
        }
        private void Update()
        {
            var view=screen!=null?screen.ViewForSlot(slot):null;
            if(view==null)
            {
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
            helpHint.text=string.Format(settings.controlHintFormat, settings.helpAction.action.GetBindingDisplayString(), settings.helpHint);
            helpPanel.SetActive(local&&expanded&&!mazeView);
            if(local&&expanded)
            {
                var lines=new System.Text.StringBuilder();
                var switcher = player.GetComponent<OfflineAbilitySwitcher>();
                bool offlineTest = switcher != null && switcher.isActiveAndEnabled;
                foreach(var h in settings.controls)
                    if(h.action!=null && (!h.offlineTestOnly || offlineTest))
                        lines.AppendLine(string.Format(settings.controlHintFormat, h.action.action.GetBindingDisplayString(), h.label));
                helpText.text=lines.ToString();
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
                    int packed = ability.DisplayLeafSlotStates;
                    for (int i = 0; i < leafStatusDots.Length; i++)
                    {
                        int state = (packed >> (i * 2)) & 3;
                        leafStatusDots[i].color = state == 2 ? settings.leafReturningColor :
                            state == 1 ? settings.leafInstalledColor : settings.leafHeldColor;
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
            Vector3 clothPoint = default;
            bool releasing = false;
            bool clothHint = local && actor != null && clothPrompt != null &&
                HangingCloth.TryInteractionHint(actor, out clothPoint, out releasing);
            prompt.SetActive(false);
            if(!clothHint&&local&&flow!=null&&actor!=null&&flow.TryInteractionHint(actor,out var point,out bool transfer))
            {
                promptText.text=string.Format(settings.controlHintFormat, actor.Binding(0), flow.IsCraftHint(actor)?settings.craftText:transfer?settings.giveText:settings.gatherText);
                Position(prompt.GetComponent<RectTransform>(),view.Camera,point+promptOffset);
            }
            if (clothPrompt != null)
            {
                clothPrompt.SetActive(false);
                if (clothHint)
                {
                    clothPromptText.text = releasing
                        ? string.Format(settings.alternativeControlHintFormat, actor.Binding(0), actor.JumpBinding, settings.clothReleaseText)
                        : string.Format(settings.controlHintFormat, actor.Binding(0), settings.clothGrabText);
                    Position((RectTransform)clothPrompt.transform, view.Camera, clothPoint + promptOffset);
                }
            }
            if (sapPrompt != null)
            {
                sapPrompt.SetActive(false);
                if (local && actor != null && SapInjectionPort.TryInteractionHint(actor, out var portPoint))
                {
                    sapPromptText.text = string.Format(settings.controlHintFormat, ability.Input.CycleBinding, settings.sapInjectText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, portPoint + promptOffset);
                }
                else if (local && actor != null &&
                    ExteriorJumpPad.TryLeafInstallHint(actor, out var leafPoint))
                {
                    sapPromptText.text = string.Format(settings.controlHintFormat, ability.Input.CycleBinding, settings.leafInstallText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, leafPoint + promptOffset);
                }
            }
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
        private void OnDisable() { help?.Dispose(); help=null; }
    }
}
