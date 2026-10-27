using System.Text;
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
        [SerializeField] private StageMinigameUiText copy;
        private PlayerHud _hud;
        private PlayScreenController _screen;
        private bool _craftingViewOverride;
        private void Awake() { _hud = GetComponent<PlayerHud>(); _screen = FindFirstObjectByType<PlayScreenController>(); }
        private void Update()
        {
            var flow = StageOneFlow.Instance;
            var view = _screen != null ? _screen.ViewForSlot(_hud.slot) : null;
            bool visible = copy != null && flow != null && flow.Minigame != StageMinigameKind.None &&
                view != null && view.GetComponent<PlayerController>().LocallyControlled;
            bool gathering = visible && flow.Minigame == StageMinigameKind.Gathering;
            bool crafting = visible && flow.Minigame == StageMinigameKind.Crafting;
            if (_screen != null && _craftingViewOverride != crafting)
            {
                _screen.SetOverride(this, crafting ? PlayScreenMode.Personal : (PlayScreenMode?)null);
                _craftingViewOverride = crafting;
            }
            if (panel != null) panel.SetActive(gathering);
            if (brewingHud != null) brewingHud.SetActive(crafting);
            if (!visible) return;
            var role = flow.RoleFor(view.GetComponent<StageActor>());
            if (gathering)
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
            if (brewingGaugeFill != null) brewingGaugeFill.fillAmount = Mathf.Clamp01(flow.CraftingGauge / 100f);
            if (brewingGaugeLabel != null) brewingGaugeLabel.text = string.Format(copy.brewingGaugeFormat, flow.CraftingGauge);
            if (brewingTime != null) brewingTime.text = string.Format(copy.brewingTimeFormat, Mathf.Max(0, recipe.timeLimit - flow.MinigameTime));
            if (fanPrompt != null) fanPrompt.SetActive(sodam);
            if (fanPromptText != null) fanPromptText.text = copy.fanPrompt;
            if (targetTemperatureBand != null)
            {
                targetTemperatureBand.gameObject.SetActive(sodam);
                targetTemperatureBand.anchorMin = new Vector2(0f, Mathf.Clamp01(recipe.targetMin / flow.settings.maximumTemperature));
                targetTemperatureBand.anchorMax = new Vector2(1f, Mathf.Clamp01(recipe.targetMax / flow.settings.maximumTemperature));
            }
            if (currentTemperatureMarker != null)
            {
                currentTemperatureMarker.gameObject.SetActive(!sodam);
                float level = Mathf.Clamp01(flow.Temperature / flow.settings.maximumTemperature);
                currentTemperatureMarker.anchorMin = currentTemperatureMarker.anchorMax = new Vector2(.5f, level);
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
        private string DirectionLabel(StageDirection step) => step switch
        {
            StageDirection.Up => copy.stepUp, StageDirection.Left => copy.stepLeft,
            StageDirection.Down => copy.stepDown, _ => copy.stepRight
        };
        private void OnDisable()
        {
            if (!_craftingViewOverride || _screen == null) return;
            _screen.SetOverride(this, null);
            _craftingViewOverride = false;
        }
    }
}
