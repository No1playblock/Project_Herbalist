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
        private const float ContextHintShowDelaySeconds = .16f;
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
        private float _contextHintCandidateSince = -1f;
        private bool _contextHintCandidateSeenThisFrame;
        private string previousFeedback;
        private float feedbackTime;
        private uint _abilityFeedback;
        private uint _minigameResult;
        private StageNarrativePresenter _narrative;
        private StagePlaytestHud _stagePlaytestHud;
        [SerializeField] private TMP_Text _questTitle, _questCount, _questDetail;
        [SerializeField] private RectTransform _questPanel;
        [SerializeField] private RectTransform[] _questLayoutTargets = System.Array.Empty<RectTransform>();
        [SerializeField, Min(0)] private float _questRightMargin = 24f;
        private Vector3[] _questOriginalPositions;
        private static Texture2D _softQuestBackdropTexture;


        private void Awake()
        {
            screen=FindFirstObjectByType<PlayScreenController>();
            _narrative=FindAnyObjectByType<StageNarrativePresenter>();
            ApplyQuestTrackerStyle();
            _questOriginalPositions = new Vector3[_questLayoutTargets.Length];
            for (int i = 0; i < _questLayoutTargets.Length; i++)
                if (_questLayoutTargets[i] != null) _questOriginalPositions[i] = _questLayoutTargets[i].anchoredPosition3D;
        }

        private void ApplyQuestTrackerStyle()
        {
            if (_questPanel == null || _questLayoutTargets == null || _questLayoutTargets.Length < 2) return;

            var content = _questLayoutTargets[1];
            var hudRoot = transform as RectTransform;
            Vector2 viewport = hudRoot != null ? hudRoot.rect.size : Vector2.zero;
            if (viewport.x <= 0f || viewport.y <= 0f)
            {
                var canvas = _questPanel.GetComponentInParent<Canvas>();
                float scale = canvas != null ? Mathf.Max(.001f, canvas.scaleFactor) : 1f;
                viewport = new Vector2(Screen.width / scale, Screen.height / scale);
            }
            float width = viewport.x * .22f;
            float height = viewport.y * .09f;
            float rightMargin = viewport.x * .02f;
            float topMargin = viewport.y * .21f;
            ConfigureQuestRoot(_questPanel, width, height, rightMargin, topMargin);
            ConfigureQuestRoot(content, width, height, rightMargin, topMargin);
            _questRightMargin = rightMargin;

            var banner = _questPanel.GetComponent<Image>();
            if (banner != null)
            {
                banner.enabled = false;
                banner.raycastTarget = false;
            }
            EnsureSoftQuestOverlay(_questPanel, content);
            ConfigureQuestProgressLine(content);


            if (_questTitle != null)
            {
                var rect = _questTitle.rectTransform;
                rect.anchorMin = new Vector2(0f, .5f);
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(16f, 0f);
                rect.offsetMax = new Vector2(-44f, 0f);
                _questTitle.alignment = TextAlignmentOptions.MidlineLeft;
                _questTitle.enableAutoSizing = true;
                _questTitle.fontSizeMin = 20f;
                _questTitle.fontSizeMax = 28f;
                _questTitle.fontSize = 28f;
                _questTitle.color = new Color(.96f, .93f, .84f, 1f);
                _questTitle.margin = Vector4.zero;
                AddQuestTextShadow(_questTitle);
            }

            if (_questCount != null)
            {
                var rect = _questCount.rectTransform;
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, .5f);
                rect.pivot = new Vector2(1f, .5f);
                rect.anchoredPosition = new Vector2(-16f, 0f);
                rect.sizeDelta = new Vector2(72f, 0f);
                _questCount.alignment = TextAlignmentOptions.MidlineRight;
                _questCount.enableAutoSizing = true;
                _questCount.fontSizeMin = 13f;
                _questCount.fontSizeMax = 15f;
                _questCount.fontSize = 15f;
                _questCount.margin = Vector4.zero;
                _questCount.color = new Color(.96f, .93f, .84f, 1f);
                _questCount.text = string.Empty;
                _questCount.gameObject.SetActive(false);
                AddQuestTextShadow(_questCount);
            }

            if (_questDetail != null)
            {
                var rect = _questDetail.rectTransform;
                rect.SetParent(content, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(1f, .5f);
                rect.offsetMin = new Vector2(28f, 0f);
                rect.offsetMax = new Vector2(-132f, 0f);
                _questDetail.alignment = TextAlignmentOptions.MidlineLeft;
                _questDetail.enableAutoSizing = true;
                _questDetail.fontSizeMin = 14f;
                _questDetail.fontSizeMax = 16f;
                _questDetail.fontSize = 16f;
                _questDetail.color = new Color(.96f, .93f, .84f, 1f);
                _questDetail.margin = Vector4.zero;
                AddQuestTextShadow(_questDetail);
            }

            var leafIcon = transform.Find("QuestLogIcon") as RectTransform;
            if (leafIcon != null)
            {
                leafIcon.SetParent(content, false);
                leafIcon.anchorMin = Vector2.one;
                leafIcon.anchorMax = Vector2.one;
                leafIcon.pivot = Vector2.one;
                leafIcon.anchoredPosition = new Vector2(-16f, -8f);
                leafIcon.sizeDelta = new Vector2(20f, 20f);
                var iconImage = leafIcon.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.color = new Color(.78f, .64f, .31f, 1f);

                }
                leafIcon.localScale = Vector3.one;
            }

            if (_questOriginalPositions != null)
            {
                for (int i = 0; i < _questLayoutTargets.Length && i < _questOriginalPositions.Length; i++)
                    if (_questLayoutTargets[i] != null) _questOriginalPositions[i] = _questLayoutTargets[i].anchoredPosition3D;
            }
        }

        private static void ConfigureQuestRoot(RectTransform rect, float width, float height, float rightMargin, float topMargin)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-rightMargin, -topMargin);
            rect.sizeDelta = new Vector2(width, height);
            rect.localScale = Vector3.one;
        }

        private void EnsureSoftQuestOverlay(RectTransform panel, RectTransform content)
        {
            var overlay = content.Find("QuestSoftOverlay") as RectTransform;
            if (overlay == null) return;

            overlay.anchorMin = Vector2.zero;
            overlay.anchorMax = Vector2.one;
            overlay.pivot = new Vector2(.5f, .5f);
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;
            overlay.localScale = Vector3.one;
            overlay.SetAsFirstSibling();

            var image = overlay.GetComponent<RawImage>();
            if (image != null)
            {
                image.texture = GetSoftQuestBackdropTexture();
                image.raycastTarget = false;
                image.color = Color.white;
            }
        }

        private static void ConfigureQuestProgressLine(RectTransform content)
        {
            var line = content.Find("QuestProgressBar") as RectTransform;
            if (line == null) return;

            line.anchorMin = new Vector2(1f, .25f);
            line.anchorMax = line.anchorMin;
            line.pivot = new Vector2(1f, .5f);
            line.anchoredPosition = new Vector2(-100f, 0f);
            line.sizeDelta = new Vector2(22f, 1.5f);
            line.localScale = Vector3.one;

            var image = line.GetComponent<RawImage>();
            if (image != null)
            {
                image.raycastTarget = false;
                image.color = new Color(.72f, .56f, .29f, .9f);
            }
            line.gameObject.SetActive(false);
        }
        private static Texture2D GetSoftQuestBackdropTexture()
        {
            if (_softQuestBackdropTexture != null) return _softQuestBackdropTexture;

            const int width = 512;
            const int height = 160;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "SoftQuestBackdropTexture";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float vertical = y / (float)(height - 1) * 2f - 1f;
                float verticalT = Mathf.Clamp01((Mathf.Abs(vertical) - .5f) / .5f);
                float verticalFade = 1f - verticalT * verticalT * (3f - 2f * verticalT);
                for (int x = 0; x < width; x++)
                {
                    float horizontal = x / (float)(width - 1) * 2f - 1f;
                    float horizontalT = Mathf.Clamp01((Mathf.Abs(horizontal) - .55f) / .45f);
                    float horizontalFade = 1f - horizontalT * horizontalT * (3f - 2f * horizontalT);
                    float alpha = .70f * horizontalFade * verticalFade;
                    pixels[y * width + x] = new Color(0f, 0f, 0f, alpha);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            _softQuestBackdropTexture = texture;
            return texture;
        }
        private static void AddQuestTextShadow(TMP_Text text)
        {
            var shadow = text.GetComponent<Shadow>();
            if (shadow == null) shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.025f, .035f, .02f, .9f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            shadow.useGraphicAlpha = true;
        }

        private void OnEnable()
        {
            foreach (var marker in markers)
                if (marker != null) marker.gameObject.SetActive(false);
            help=settings.helpAction.action.Clone(); help.performed+=_=>expanded=!expanded; help.Enable();
        }
        private void Update()
        {
            _contextHintCandidateSeenThisFrame = false;
            var rootRect = transform as RectTransform;
            if (rootRect != null && _questPanel != null && rootRect.rect.width > 0f && rootRect.rect.height > 0f &&
                (Mathf.Abs(_questPanel.sizeDelta.x - rootRect.rect.width * .22f) > 1f || Mathf.Abs(_questPanel.sizeDelta.y - rootRect.rect.height * .09f) > 1f))
                ApplyQuestTrackerStyle();
            PositionQuestPanel((RectTransform)transform, _questPanel, _questLayoutTargets,
                _questOriginalPositions, screen != null && screen.IsSplitVisible, _questRightMargin);
            var view=screen!=null?screen.ViewForSlot(slot):null;
            if(view==null)
            {
                _contextHintCandidateSince = -1f;
                PresentQuest(false);
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
            if (_stagePlaytestHud == null) _stagePlaytestHud = FindFirstObjectByType<StagePlaytestHud>();
            bool showContextHints = _stagePlaytestHud == null || !_stagePlaytestHud.GuideActive;
            PresentQuest(local || (screen != null && screen.IsSplitVisible));
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
            if (showContextHints && local && actor != null)
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
                    bool useClothPrompt = clothPrompt != null && interaction is StageInteraction.GrabCloth or StageInteraction.ReleaseCloth;
                    string hintBinding = interaction == StageInteraction.ReleaseCloth && !string.IsNullOrEmpty(player.Input.JumpBinding)
                        ? player.Input.JumpBinding
                        : actor.InteractBinding;
                    PresentHint(useClothPrompt ? _clothHintView : _interactionHintView,
                        useClothPrompt ? clothPromptText : promptText, hintBinding, description);
                    Position((RectTransform)(useClothPrompt ? clothPrompt : prompt).transform, view.Camera, point + promptOffset);
                }
            }
            if (sapPrompt != null)
            {
                sapPrompt.SetActive(false);
                if (showContextHints && local && actor != null && SapInjectionPort.TryInteractionHint(actor, out var portPoint))
                {
                    string binding = rules != null && rules.usePrimaryButton ? ability.Input.UseBinding : ability.Input.CycleBinding;
                    PresentHint(_sapHintView, sapPromptText, binding, settings.sapInjectText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, portPoint + promptOffset);
                }
                else if (showContextHints && local && !mazeView && rules is { contextualUse: true } &&
                    HangingCloth.TryWaterPushHint(actor, view.GetAimRay(view.Yaw, view.Pitch), out var clothPoint))
                {
                    string binding = rules.usePrimaryButton ? ability.Input.UseBinding : ability.Input.CycleBinding;
                    PresentHint(_sapHintView, sapPromptText, binding, settings.sapPushClothText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, clothPoint + promptOffset);
                    if (sapPrompt.activeSelf && interaction == StageInteraction.GrabCloth && clothPrompt != null)
                        clothPrompt.SetActive(false);
                }
                else if (showContextHints && local && actor != null &&
                    ExteriorJumpPad.TryLeafInstallHint(actor, out var leafPoint))
                {
                    string binding = rules != null && rules.usePrimaryButton ? ability.Input.UseBinding : ability.Input.CycleBinding;
                    PresentHint(_sapHintView, sapPromptText, binding, settings.leafInstallText);
                    Position((RectTransform)sapPrompt.transform, view.Camera, leafPoint + promptOffset);
                }
            }
            if ((_stagePlaytestHud == null || !_stagePlaytestHud.GuideActive) &&
                !_contextHintCandidateSeenThisFrame)
                _contextHintCandidateSince = -1f;
        }
        private void PresentQuest(bool local)
        {
            if (_questTitle == null || _narrative == null) return;
            bool visible = local && _narrative.QuestVisible;
            for (int i = 0; i < _questLayoutTargets.Length; i++)
                if (_questLayoutTargets[i] != null) _questLayoutTargets[i].gameObject.SetActive(visible && i <= 1);
            string title = _narrative.MainQuestText;
            int countStart = title.LastIndexOf('(');
            string progress = string.Empty;
            bool hasCount = false;
            if (countStart >= 0 && title.EndsWith(")"))
            {
                string candidate = title.Substring(countStart + 1, title.Length - countStart - 2).Trim();
                string[] parts = candidate.Split('/');
                hasCount = parts.Length == 2 && System.Int32.TryParse(parts[0].Trim(), out _) && System.Int32.TryParse(parts[1].Trim(), out _);
                if (hasCount) progress = candidate;
            }
            _questTitle.text = hasCount ? title.Substring(0, countStart).TrimEnd() : title;
            if (_questCount != null)
            {
                _questCount.text = hasCount ? progress : string.Empty;
                _questCount.gameObject.SetActive(hasCount);
            }
            var progressLine = _questLayoutTargets.Length > 1
                ? _questLayoutTargets[1].Find("QuestProgressBar")
                : null;
            if (progressLine != null) progressLine.gameObject.SetActive(visible && hasCount);

            if (_questDetail != null)
            {
                _questDetail.rectTransform.offsetMax = new Vector2(hasCount ? -132f : -16f, 0f);
                _questDetail.text = _narrative.DetailQuestText;
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

        internal void HideContextHints()
        {
            if (prompt != null) prompt.SetActive(false);
            if (clothPrompt != null) clothPrompt.SetActive(false);
            if (sapPrompt != null) sapPrompt.SetActive(false);
        }

        private void Position(RectTransform rect,Camera camera,Vector3 point)
        {
            Vector3 pos=camera.WorldToScreenPoint(point);
            bool visible=pos.z>0&&camera.pixelRect.Contains(pos);
            if (visible)
            {
                _contextHintCandidateSeenThisFrame = true;
                if (_contextHintCandidateSince < 0f) _contextHintCandidateSince = Time.unscaledTime;
                visible = Time.unscaledTime - _contextHintCandidateSince >= ContextHintShowDelaySeconds;
            }
            else if (_stagePlaytestHud == null || !_stagePlaytestHud.GuideActive)
            {
                _contextHintCandidateSince = -1f;
            }
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

            show = show && _narrative.TryNavigationTarget(out point, out _);
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
            if (markerLabels.Length > 0 && markerLabels[0] != null) markerLabels[0].text = string.Empty;
        }
        private void OnDisable() { help?.Dispose(); help=null; }
    }
}
