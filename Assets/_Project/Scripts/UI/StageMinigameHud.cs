using System.Text;
using System;
using System.Collections.Generic;
using Herbalist.Player;
using Herbalist.Presentation;
using Herbalist.StageOne;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(PlayerHud))]
    public sealed class StageMinigameHud : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text progress;
        [SerializeField] private GameObject brewingHud;
        [SerializeField] private BrewingPotRattle _potRattle;
        [SerializeField] private Image brewingGaugeFill;
        [SerializeField] private RectTransform targetTemperatureBand;
        [SerializeField] private RectTransform currentTemperatureMarker;
        [SerializeField] private TMP_Text thermometerTitle;
        [SerializeField] private TMP_Text thermometerValue;
        [SerializeField] private TMP_Text fireStrength;
        [SerializeField] private TMP_Text brewingGaugeLabel;
        [SerializeField] private TMP_Text brewingTime;
        [SerializeField] private GameObject fanPrompt;
        [SerializeField] private TMP_Text fanPromptText;
        [SerializeField] private ControlHintView _fanHint;
        [SerializeField] private TMP_Text _minigameHelp;
        [SerializeField] private StageMinigameUiText copy;
        [SerializeField] private StageGatheringView _gatheringView;
        [SerializeField] private Sprite duyeongDialPanelSprite;
        [SerializeField] private Sprite duyeongTemperatureNeedleSprite;
        [SerializeField] private Sprite duyeongProgressTimePanelSprite;

        [Header("제조 UI 글자 설정")]
        [SerializeField, Min(1f)] private float statusTextFontSize = 30f;
        [SerializeField] private FontWeight statusTextFontWeight = FontWeight.Bold;
        [SerializeField, Min(1f)] private float brewingTimeFontSize = 30f;
        [SerializeField, Min(1f)] private float thermometerTitleFontSize = 28f;
        [SerializeField] private FontWeight thermometerTitleFontWeight = FontWeight.Bold;
        [SerializeField, Min(1f)] private float thermometerValueFontSize = 44f;
        [SerializeField] private FontWeight thermometerValueFontWeight = FontWeight.Bold;
        [SerializeField, Min(1f)] private float fireStrengthFontSize = 28f;
        [SerializeField] private FontWeight fireStrengthFontWeight = FontWeight.Bold;
        [SerializeField, TextArea] private string fanPromptDescription = "";

        [Header("제조 UI 배치 설정")]
        [SerializeField] private AnchorPreset potAndFireAnchors = new(new Vector2(.03f, .08f), new Vector2(.55f, .84f));
        [SerializeField] private AnchorPreset thermometerFrameAnchors = new(new Vector2(.62f, .15f), new Vector2(.97f, .93f));
        [SerializeField] private AnchorPreset headingAnchors = new(new Vector2(.07f, .90f), new Vector2(.93f, .98f));
        [SerializeField] private AnchorPreset temperatureValueAnchors = new(new Vector2(.10f, .81f), new Vector2(.90f, .89f));
        [SerializeField] private AnchorPreset temperatureColumnFrameAnchors = new(new Vector2(.17f, .22f), new Vector2(.83f, .74f));
        [SerializeField] private AnchorPreset temperatureColumnAnchors = new(Vector2.zero, Vector2.one);
        [SerializeField] private AnchorPreset fireStrengthAnchors = new(new Vector2(.08f, .045f), new Vector2(.92f, .15f));
        [SerializeField] private AnchorPreset statusFrameAnchors = new(new Vector2(.02f, -.155f), new Vector2(.98f, .375f));
        [SerializeField] private AnchorPreset statusInnerAnchors = new(new Vector2(.02f, .373f), new Vector2(.98f, .622f));
        [SerializeField] private AnchorPreset brewingGaugeTrackAnchors = new(new Vector2(.16f, .12f), new Vector2(.40f, .28f));
        [SerializeField] private AnchorPreset brewingGaugeFillAnchors = new(new Vector2(.035f, .18f), new Vector2(.965f, .82f));
        [SerializeField] private AnchorPreset brewingGaugeLabelAnchors = new(new Vector2(.04f, 0f), new Vector2(.46f, 1f));
        [SerializeField] private AnchorPreset brewingTimeAnchors = new(new Vector2(.54f, 0f), new Vector2(.96f, 1f));
        [SerializeField] private Vector2 temperatureNeedlePivot = new(.5f, 0f);
        [SerializeField] private Vector2 temperatureNeedleSize = new(92f, 167f);
        [SerializeField] private float needleAngleAtMinimumTemperature = 90f;
        [SerializeField] private float needleAngleAtMaximumTemperature = -90f;

        [Serializable]
        private struct AnchorPreset
        {
            [Tooltip("RectTransform의 Anchor Min 값입니다.")]
            public Vector2 min;
            [Tooltip("RectTransform의 Anchor Max 값입니다.")]
            public Vector2 max;

            public AnchorPreset(Vector2 min, Vector2 max)
            {
                this.min = min;
                this.max = max;
            }
        }

        private sealed class RectState
        {
            public Transform parent;
            public int siblingIndex;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector2 pivot;
            public Quaternion localRotation;
            public Vector3 localScale;
        }

        private readonly Dictionary<RectTransform, RectState> _rectStates = new();
        private RectTransform _potAndFireRect;
        private RectTransform _thermometerFrameRect;
        private RectTransform _thermometerInnerRect;
        private RectTransform _headingRect;
        private RectTransform _temperatureValueRect;
        private RectTransform _temperatureColumnFrameRect;
        private RectTransform _temperatureColumnRect;
        private RectTransform _fireStrengthRect;
        private RectTransform _statusFrameRect;
        private RectTransform _statusInnerRect;
        private RectTransform _brewingGaugeTrackRect;
        private RectTransform _brewingGaugeFillRect;
        private Image _brewingGaugeTrackImage;
        private Image _thermometerPanelImage;
        private Image _progressTimePanelImage;
        private Image _temperatureColumnFrameImage;
        private Image _temperatureNeedleImage;
        private Sprite _defaultThermometerPanelSprite;
        private Sprite _defaultProgressTimePanelSprite;
        private Color _defaultProgressTimePanelColor;
        private bool _defaultTemperatureColumnFrameEnabled;
        private bool _defaultGaugeTrackEnabled;
        private bool _defaultGaugeFillEnabled;
        private bool _layoutInitialized;
        private bool _duyeongLayoutApplied;
        private int _lastFanPressStep = -1;
        private Image.FillMethod _defaultGaugeFillMethod;
        private int _defaultGaugeFillOrigin;
        private Color _defaultGaugeFillColor;
        private Color _defaultGaugeLabelColor;
        private Color _defaultBrewingTimeColor;
        private TextAlignmentOptions _defaultGaugeLabelAlignment;
        private TextAlignmentOptions _defaultBrewingTimeAlignment;
        private float _defaultGaugeLabelFontSize;
        private float _defaultBrewingTimeFontSize;
        private FontWeight _defaultGaugeLabelFontWeight;
        private FontWeight _defaultBrewingTimeFontWeight;
        private FontStyles _defaultGaugeLabelFontStyle;
        private FontStyles _defaultBrewingTimeFontStyle;
        private PlayerHud _hud;
        private PlayScreenController _screen;
        private bool _personalViewOverride;
        private void Awake()
        {
            _hud = GetComponent<PlayerHud>();
            _screen = FindFirstObjectByType<PlayScreenController>();
            CacheBrewingVisuals();
        }

        private void CacheBrewingVisuals()
        {
            if (brewingHud == null) return;

            _potAndFireRect = FindRect("BrewingPotAndFire");
            _thermometerFrameRect = FindRect("ThermometerFrame");
            _thermometerInnerRect = FindRect("ThermometerFrame/ThermometerInner");
            _headingRect = FindRect("ThermometerFrame/ThermometerInner/Heading");
            _temperatureValueRect = FindRect("ThermometerFrame/ThermometerInner/TemperatureValue");
            _temperatureColumnFrameRect = FindRect("ThermometerFrame/ThermometerInner/TemperatureColumnFrame");
            _temperatureColumnRect = FindRect("ThermometerFrame/ThermometerInner/TemperatureColumnFrame/Art_TemperatureColumnFrame/TemperatureColumn");
            _fireStrengthRect = fireStrength != null ? fireStrength.rectTransform : null;
            _statusFrameRect = FindRect("BrewingStatusFrame");
            _statusInnerRect = FindRect("BrewingStatusFrame/BrewingStatusInner");
            _brewingGaugeTrackRect = brewingGaugeFill != null && brewingGaugeFill.transform.parent is RectTransform track
                ? track : FindRect("ThermometerFrame/ThermometerInner/TemperatureColumnFrame/Art_TemperatureColumnFrame/BrewingGaugeTrack");
            _brewingGaugeFillRect = brewingGaugeFill != null ? brewingGaugeFill.rectTransform : null;
            _brewingGaugeTrackImage = _brewingGaugeTrackRect != null ? _brewingGaugeTrackRect.GetComponent<Image>() : null;

            _thermometerPanelImage = FindImage("ThermometerFrame/Art_ThermometerFrame");
            _progressTimePanelImage = FindImage("BrewingStatusFrame");
            _temperatureColumnFrameImage = FindImage("ThermometerFrame/ThermometerInner/TemperatureColumnFrame/Art_TemperatureColumnFrame");
            _temperatureNeedleImage = FindImage("ThermometerFrame/ThermometerInner/TemperatureColumnFrame/Art_TemperatureColumnFrame/TemperatureColumn/CurrentTemperatureMarker/Art_CurrentTemperatureMarker");

            CacheRect(_potAndFireRect);
            CacheRect(_thermometerFrameRect);
            CacheRect(_thermometerInnerRect);
            CacheRect(_headingRect);
            CacheRect(_temperatureValueRect);
            CacheRect(_temperatureColumnFrameRect);
            CacheRect(_temperatureColumnRect);
            CacheRect(_fireStrengthRect);
            CacheRect(_statusFrameRect);
            CacheRect(_statusInnerRect);
            CacheRect(_brewingGaugeTrackRect);
            CacheRect(_brewingGaugeFillRect);
            CacheRect(currentTemperatureMarker);
            CacheRect(brewingGaugeLabel != null ? brewingGaugeLabel.rectTransform : null);
            CacheRect(brewingTime != null ? brewingTime.rectTransform : null);

            _defaultThermometerPanelSprite = _thermometerPanelImage != null ? _thermometerPanelImage.sprite : null;
            _defaultProgressTimePanelSprite = _progressTimePanelImage != null ? _progressTimePanelImage.sprite : null;
            _defaultProgressTimePanelColor = _progressTimePanelImage != null ? _progressTimePanelImage.color : Color.white;
            _defaultTemperatureColumnFrameEnabled = _temperatureColumnFrameImage != null && _temperatureColumnFrameImage.enabled;
            _defaultGaugeTrackEnabled = _brewingGaugeTrackImage != null && _brewingGaugeTrackImage.enabled;
            _defaultGaugeFillEnabled = brewingGaugeFill != null && brewingGaugeFill.enabled;
            if (brewingGaugeFill != null)
            {
                _defaultGaugeFillMethod = brewingGaugeFill.fillMethod;
                _defaultGaugeFillOrigin = brewingGaugeFill.fillOrigin;
                _defaultGaugeFillColor = brewingGaugeFill.color;
            }
            if (brewingGaugeLabel != null)
            {
                _defaultGaugeLabelColor = brewingGaugeLabel.color;
                _defaultGaugeLabelAlignment = brewingGaugeLabel.alignment;
                _defaultGaugeLabelFontSize = brewingGaugeLabel.fontSize;
                _defaultGaugeLabelFontWeight = brewingGaugeLabel.fontWeight;
                _defaultGaugeLabelFontStyle = brewingGaugeLabel.fontStyle;
            }
            if (brewingTime != null)
            {
                _defaultBrewingTimeColor = brewingTime.color;
                _defaultBrewingTimeAlignment = brewingTime.alignment;
                _defaultBrewingTimeFontSize = brewingTime.fontSize;
                _defaultBrewingTimeFontWeight = brewingTime.fontWeight;
                _defaultBrewingTimeFontStyle = brewingTime.fontStyle;
            }
            if (_temperatureNeedleImage != null && duyeongTemperatureNeedleSprite != null)
            {
                _temperatureNeedleImage.sprite = duyeongTemperatureNeedleSprite;
                _temperatureNeedleImage.preserveAspect = false;
            }
        }

        private RectTransform FindRect(string path)
        {
            var child = brewingHud != null ? brewingHud.transform.Find(path) : null;
            return child as RectTransform;
        }

        private Image FindImage(string path)
        {
            var child = brewingHud != null ? brewingHud.transform.Find(path) : null;
            return child != null ? child.GetComponent<Image>() : null;
        }

        private void CacheRect(RectTransform rect)
        {
            if (rect == null || _rectStates.ContainsKey(rect)) return;
            _rectStates.Add(rect, new RectState
            {
                parent = rect.parent,
                siblingIndex = rect.GetSiblingIndex(),
                anchorMin = rect.anchorMin,
                anchorMax = rect.anchorMax,
                anchoredPosition = rect.anchoredPosition,
                sizeDelta = rect.sizeDelta,
                pivot = rect.pivot,
                localRotation = rect.localRotation,
                localScale = rect.localScale
            });
        }

        private void RestoreRect(RectTransform rect)
        {
            if (rect == null || !_rectStates.TryGetValue(rect, out var state)) return;
            if (rect.parent != state.parent) rect.SetParent(state.parent, false);
            rect.anchorMin = state.anchorMin;
            rect.anchorMax = state.anchorMax;
            rect.anchoredPosition = state.anchoredPosition;
            rect.sizeDelta = state.sizeDelta;
            rect.pivot = state.pivot;
            rect.localRotation = state.localRotation;
            rect.localScale = state.localScale;
            if (rect.parent != null)
                rect.SetSiblingIndex(Mathf.Clamp(state.siblingIndex, 0, rect.parent.childCount - 1));
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            if (rect == null) return;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private void ApplyCraftingVisualLayout(bool duyeong)
        {
            if (_layoutInitialized && _duyeongLayoutApplied == duyeong) return;
            _layoutInitialized = true;
            _duyeongLayoutApplied = duyeong;

            if (!duyeong)
            {
                RestoreRect(_brewingGaugeTrackRect);
                foreach (var rect in _rectStates.Keys)
                    if (rect != _brewingGaugeTrackRect) RestoreRect(rect);
                if (_thermometerPanelImage != null) _thermometerPanelImage.sprite = _defaultThermometerPanelSprite;
                if (_progressTimePanelImage != null)
                {
                    _progressTimePanelImage.sprite = _defaultProgressTimePanelSprite;
                    _progressTimePanelImage.color = _defaultProgressTimePanelColor;
                }
                if (_temperatureColumnFrameImage != null) _temperatureColumnFrameImage.enabled = _defaultTemperatureColumnFrameEnabled;
                if (brewingGaugeFill != null)
                {
                    brewingGaugeFill.enabled = _defaultGaugeFillEnabled;
                    brewingGaugeFill.fillMethod = _defaultGaugeFillMethod;
                    brewingGaugeFill.fillOrigin = _defaultGaugeFillOrigin;
                    brewingGaugeFill.color = _defaultGaugeFillColor;
                }
                if (_brewingGaugeTrackImage != null) _brewingGaugeTrackImage.enabled = _defaultGaugeTrackEnabled;
                if (brewingGaugeLabel != null)
                {
                    brewingGaugeLabel.color = _defaultGaugeLabelColor;
                    brewingGaugeLabel.alignment = _defaultGaugeLabelAlignment;
                    brewingGaugeLabel.fontSize = _defaultGaugeLabelFontSize;
                    brewingGaugeLabel.fontWeight = _defaultGaugeLabelFontWeight;
                    brewingGaugeLabel.fontStyle = _defaultGaugeLabelFontStyle;
                }
                if (brewingTime != null)
                {
                    brewingTime.color = _defaultBrewingTimeColor;
                    brewingTime.alignment = _defaultBrewingTimeAlignment;
                    brewingTime.fontSize = _defaultBrewingTimeFontSize;
                    brewingTime.fontWeight = _defaultBrewingTimeFontWeight;
                    brewingTime.fontStyle = _defaultBrewingTimeFontStyle;
                }
                return;
            }

            if (_thermometerPanelImage != null && duyeongDialPanelSprite != null)
                _thermometerPanelImage.sprite = duyeongDialPanelSprite;
            if (_progressTimePanelImage != null && duyeongProgressTimePanelSprite != null)
            {
                _progressTimePanelImage.sprite = duyeongProgressTimePanelSprite;
                var statusPanelColor = _defaultProgressTimePanelColor;
                statusPanelColor.a = 1f;
                _progressTimePanelImage.color = statusPanelColor;
            }
            if (_temperatureColumnFrameImage != null) _temperatureColumnFrameImage.enabled = false;
            // Keep the live percentage value, but remove the decorative progress track and fill.
            if (_brewingGaugeTrackImage != null) _brewingGaugeTrackImage.enabled = false;
            if (brewingGaugeFill != null) brewingGaugeFill.enabled = false;

            SetAnchors(_potAndFireRect, potAndFireAnchors.min, potAndFireAnchors.max);
            SetAnchors(_thermometerFrameRect, thermometerFrameAnchors.min, thermometerFrameAnchors.max);
            SetAnchors(_headingRect, headingAnchors.min, headingAnchors.max);
            SetAnchors(_temperatureValueRect, temperatureValueAnchors.min, temperatureValueAnchors.max);
            SetAnchors(_temperatureColumnFrameRect, temperatureColumnFrameAnchors.min, temperatureColumnFrameAnchors.max);
            SetAnchors(_temperatureColumnRect, temperatureColumnAnchors.min, temperatureColumnAnchors.max);
            SetAnchors(_fireStrengthRect, fireStrengthAnchors.min, fireStrengthAnchors.max);

            // The generated footer has transparent padding; shrink its on-screen height and keep
            // the live text aligned with the visible middle strip.
            SetAnchors(_statusFrameRect, statusFrameAnchors.min, statusFrameAnchors.max);
            SetAnchors(_statusInnerRect, statusInnerAnchors.min, statusInnerAnchors.max);
            if (_brewingGaugeTrackRect != null && _statusInnerRect != null && _brewingGaugeTrackRect.parent != _statusInnerRect)
                _brewingGaugeTrackRect.SetParent(_statusInnerRect, false);
            SetAnchors(_brewingGaugeTrackRect, brewingGaugeTrackAnchors.min, brewingGaugeTrackAnchors.max);
            SetAnchors(_brewingGaugeFillRect, brewingGaugeFillAnchors.min, brewingGaugeFillAnchors.max);
            if (brewingGaugeLabel != null)
            {
                SetAnchors(brewingGaugeLabel.rectTransform, brewingGaugeLabelAnchors.min, brewingGaugeLabelAnchors.max);
                brewingGaugeLabel.alignment = TextAlignmentOptions.Center;
                brewingGaugeLabel.color = new Color(.98f, .91f, .73f, 1f);
                brewingGaugeLabel.fontSize = statusTextFontSize;
                brewingGaugeLabel.fontWeight = statusTextFontWeight;
                brewingGaugeLabel.fontStyle = FontStyles.Bold;
                brewingGaugeLabel.enableAutoSizing = false;
            }
            if (brewingTime != null)
            {
                SetAnchors(brewingTime.rectTransform, brewingTimeAnchors.min, brewingTimeAnchors.max);
                brewingTime.alignment = TextAlignmentOptions.Center;
                brewingTime.color = new Color(.98f, .91f, .73f, 1f);
                brewingTime.fontSize = brewingTimeFontSize;
                brewingTime.fontWeight = statusTextFontWeight;
                brewingTime.fontStyle = FontStyles.Bold;
                brewingTime.enableAutoSizing = false;
            }
            if (thermometerTitle != null)
            {
                thermometerTitle.fontSize = thermometerTitleFontSize;
                thermometerTitle.fontWeight = thermometerTitleFontWeight;
                thermometerTitle.fontStyle = FontStyles.Bold;
                thermometerTitle.enableAutoSizing = false;
            }
            if (thermometerValue != null)
            {
                thermometerValue.fontSize = thermometerValueFontSize;
                thermometerValue.fontWeight = thermometerValueFontWeight;
                thermometerValue.fontStyle = FontStyles.Bold;
                thermometerValue.enableAutoSizing = false;
            }
            if (fireStrength != null)
            {
                fireStrength.fontSize = fireStrengthFontSize;
                fireStrength.fontWeight = fireStrengthFontWeight;
                fireStrength.fontStyle = FontStyles.Bold;
                fireStrength.enableAutoSizing = false;
            }
            if (brewingGaugeFill != null)
            {
                brewingGaugeFill.fillMethod = Image.FillMethod.Horizontal;
                brewingGaugeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                brewingGaugeFill.color = new Color(.92f, .66f, .25f, 1f);
            }
            if (currentTemperatureMarker != null)
            {
                currentTemperatureMarker.anchorMin = currentTemperatureMarker.anchorMax = new Vector2(.5f, .5f);
                currentTemperatureMarker.anchoredPosition = Vector2.zero;
                // The needle sprite's round base is at its lower end. Keep that end
                // pinned to the dial center while the leaf tip sweeps around it.
                currentTemperatureMarker.pivot = temperatureNeedlePivot;
                currentTemperatureMarker.sizeDelta = temperatureNeedleSize;
                if (_temperatureNeedleImage != null)
                    _temperatureNeedleImage.preserveAspect = true;
            }
        }
        private void Update()
        {
            var flow = StageOneFlow.Instance;
            var view = _screen != null ? _screen.ViewForSlot(_hud.slot) : null;
            bool local = copy != null && flow != null &&
                view != null && view.GetComponent<PlayerController>().LocallyControlled;
            var actor = local ? view.GetComponent<StageActor>() : null;
            var role = actor != null ? flow.RoleFor(actor) : CharacterRole.Duyeong;
            bool gathering = actor != null && flow.Minigame == StageMinigameKind.Gathering;
            bool crafting = actor != null && flow.Minigame == StageMinigameKind.Crafting;
            if (!crafting) _lastFanPressStep = -1;
            bool illustratedGathering = _gatheringView != null &&
                _gatheringView.Present(actor != null ? flow : null, role, copy);
            if (_potRattle != null)
                _potRattle.Present(flow, crafting ? actor : null, crafting);
            bool personal = crafting || illustratedGathering;
            if (_screen != null && _personalViewOverride != personal)
            {
                _personalViewOverride = personal;
                _screen.SetOverride(this, personal ? PlayScreenMode.Personal : (PlayScreenMode?)null);
            }
            bool legacyGathering = gathering && _gatheringView == null && panel != null;
            if (panel != null) panel.SetActive(legacyGathering);
            if (brewingHud != null) brewingHud.SetActive(crafting);
            if (fanPrompt != null) fanPrompt.SetActive(crafting && role == CharacterRole.Sodam);
            bool showHelp = _hud.ControlHelpExpanded && (gathering || crafting);
            if (_minigameHelp != null)
            {
                _minigameHelp.transform.parent.gameObject.SetActive(showHelp);
                if (showHelp)
                    _minigameHelp.text = gathering
                        ? role == CharacterRole.Duyeong ? copy.gatherDuyeongHelp : copy.gatherSodamHelp
                        : role == CharacterRole.Sodam ? copy.craftSodamHelp : copy.craftDuyeongHelp;
            }
            if (legacyGathering)
            {
                var rect = (RectTransform)panel.transform;
                bool small = role == CharacterRole.Duyeong;
                rect.anchorMin = small ? new Vector2(.03f, .57f) : new Vector2(.07f, .18f);
                rect.anchorMax = small ? new Vector2(.55f, .94f) : new Vector2(.93f, .86f);
                ShowGathering(flow, role);
            }
            else if (crafting) ShowCrafting(flow, role);
        }
        private void ShowGathering(StageOneFlow flow, CharacterRole role)
        {
            if (role == CharacterRole.Duyeong)
            {
                title.text = copy.gatherDuyeongTitle;
                body.text = copy.gatherDuyeongBody;
                progress.text = string.Format(copy.gatherProgressFormat, flow.MinigameSteps);
                return;
            }
            title.text = copy.gatherSodamTitle;
            var text = new StringBuilder(copy.gatherSodamIntro).Append("\n\n");
            foreach (var recipe in flow.settings.recipes)
            {
                var herb = recipe.ingredient;
                text.Append(herb.displayName).Append("\n").Append(herb.appearance).Append("\n");
                text.Append(herb.harvestingMethod).Append('\n').Append(copy.codexStepsLabel).Append(' ');
                foreach (var step in herb.harvestingSteps) text.Append(DirectionLabel(step)).Append(' ');
                text.Append("\n\n");
            }
            body.text = text.ToString();
            progress.text = copy.gatherSodamProgress;
        }
        private void ShowCrafting(StageOneFlow flow, CharacterRole role)
        {
            int recipeIndex = flow.settings.RecipeIndexForIngredient(flow.MinigameItemId);
            var recipe = recipeIndex >= 0 ? flow.settings.recipes[recipeIndex] : null;
            if (recipe == null) return;
            bool sodam = role == CharacterRole.Sodam;
            // Both roles share the same illustrated panels and crafting status labels.
            ApplyCraftingVisualLayout(true);
            if (brewingGaugeFill != null) brewingGaugeFill.fillAmount = Mathf.Clamp01(flow.CraftingGauge / 100f);
            int remainingSeconds = Mathf.CeilToInt(Mathf.Max(0f, recipe.timeLimit - flow.MinigameTime));
            if (brewingGaugeLabel != null)
                brewingGaugeLabel.text = string.Format(
                    copy != null ? copy.brewingGaugeFormat : "제조 진행률\n<b>{0:0}%</b>",
                    flow.CraftingGauge);
            if (brewingTime != null)
                brewingTime.text = string.Format(
                    copy != null ? copy.brewingTimeFormat : "남은 시간\n<b>{0:00}초</b>",
                    remainingSeconds);
            var craftingView = _screen != null ? _screen.ViewForSlot(_hud.slot) : null;
            var craftingActor = craftingView != null ? craftingView.GetComponent<StageActor>() : null;
            if (_fanHint != null) _fanHint.Present(craftingActor != null ? craftingActor.FanBinding : string.Empty, fanPromptDescription);
            else if (fanPromptText != null) fanPromptText.text = fanPromptDescription;
            if (sodam)
            {
                if (_lastFanPressStep >= 0 && flow.MinigameSteps > _lastFanPressStep)
                    _fanHint?.PulseSpaceKey();
                _lastFanPressStep = flow.MinigameSteps;
            }
            else _lastFanPressStep = -1;
            if (targetTemperatureBand != null)
            {
                targetTemperatureBand.gameObject.SetActive(sodam);
                targetTemperatureBand.anchorMin = new Vector2(0f, Mathf.Clamp01(recipe.targetMin / flow.settings.maximumTemperature));
                targetTemperatureBand.anchorMax = new Vector2(1f, Mathf.Clamp01(recipe.targetMax / flow.settings.maximumTemperature));
            }
            if (currentTemperatureMarker != null)
            {
                currentTemperatureMarker.gameObject.SetActive(true);
                float level = Mathf.Clamp01(flow.Temperature / flow.settings.maximumTemperature);
                currentTemperatureMarker.anchorMin = currentTemperatureMarker.anchorMax = new Vector2(.5f, .5f);
                currentTemperatureMarker.anchoredPosition = Vector2.zero;
                float needleAngle = Mathf.Lerp(needleAngleAtMinimumTemperature, needleAngleAtMaximumTemperature, level);
                currentTemperatureMarker.localRotation = Quaternion.Euler(0f, 0f, needleAngle);
            }
            if (thermometerTitle != null) thermometerTitle.text = sodam ? copy.targetTemperatureTitle : copy.currentTemperatureTitle;
            if (thermometerValue != null) thermometerValue.text = sodam
                ? string.Format(copy.targetTemperatureFormat, recipe.targetMin, recipe.targetMax)
                : string.Format(copy.currentTemperatureFormat, flow.Temperature);
            if (fireStrength != null)
            {
                fireStrength.gameObject.SetActive(true);
                if (sodam)
                {
                    fireStrength.text = flow.Temperature < recipe.targetMin ? copy.temperatureLow :
                        flow.Temperature > recipe.targetMax ? copy.temperatureHigh : copy.temperatureInRange;
                }
                else
                {
                    var fire = StageBrewingRules.FireStrength(flow.FanPressCount, flow.settings);
                    string fireLabel = fire == StageFireStrength.High ? copy.fireStrong :
                        fire == StageFireStrength.Medium ? copy.fireMedium : copy.fireWeak;
                    fireStrength.text = string.Format(copy.fireStrengthFormat, fireLabel);
                }
            }
        }
        private string DirectionLabel(StageDirection step) => copy.DirectionLabel(step);
        private void OnDisable()
        {
            if (_gatheringView != null) _gatheringView.Hide();
            if (_minigameHelp != null) _minigameHelp.transform.parent.gameObject.SetActive(false);
            if (fanPrompt != null) fanPrompt.SetActive(false);
            if (!_personalViewOverride) return;
            _personalViewOverride = false;
            if (_screen != null) _screen.SetOverride(this, null);
        }
    }
}
