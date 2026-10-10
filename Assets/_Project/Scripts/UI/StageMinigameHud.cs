using System.Text;
using Herbalist.Player;
using Herbalist.Presentation;
using Herbalist.StageOne;
using TMPro;
using UnityEngine;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(PlayerHud))]
    public sealed class StageMinigameHud : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text progress;
        [SerializeField] private StageMinigameUiText copy;
        private PlayerHud _hud;
        private PlayScreenController _screen;
        private void Awake() { _hud = GetComponent<PlayerHud>(); _screen = FindFirstObjectByType<PlayScreenController>(); }
        private void Update()
        {
            var flow = StageOneFlow.Instance;
            var view = _screen != null ? _screen.ViewForSlot(_hud.slot) : null;
            bool visible = copy != null && flow != null && flow.Minigame != StageMinigameKind.None &&
                view != null && view.GetComponent<PlayerController>().LocallyControlled;
            if (panel == null) return;
            panel.SetActive(visible);
            if (!visible) return;
            var role = flow.RoleFor(view.GetComponent<StageActor>());
            var rect = (RectTransform)panel.transform;
            bool small = flow.Minigame == StageMinigameKind.Gathering && role == CharacterRole.Duyeong;
            rect.anchorMin = small ? new Vector2(.03f, .57f) : new Vector2(.07f, .18f);
            rect.anchorMax = small ? new Vector2(.55f, .94f) : new Vector2(.93f, .86f);
            if (flow.Minigame == StageMinigameKind.Gathering) ShowGathering(flow, role);
            else ShowCrafting(flow, role);
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
            var ingredient = flow.settings.Item(flow.MinigameItemId);
            int recipeIndex = flow.settings.RecipeIndexForIngredient(flow.MinigameItemId);
            var recipe = recipeIndex >= 0 ? flow.settings.recipes[recipeIndex] : null;
            if (recipe == null) return;
            if (role == CharacterRole.Sodam)
            {
                title.text = copy.craftSodamTitle;
                body.text = string.Format(copy.craftSodamBodyFormat, ingredient.displayName, recipe.targetMin, recipe.targetMax);
                progress.text = string.Format(copy.craftSodamProgressFormat, flow.StableTime, recipe.holdSeconds, Mathf.Max(0, recipe.timeLimit - flow.MinigameTime));
            }
            else
            {
                title.text = copy.craftDuyeongTitle;
                body.text = string.Format(copy.craftDuyeongBodyFormat, flow.Temperature, flow.FanHeld ? copy.fireStrong : copy.fireWeak);
                progress.text = string.Format(copy.craftDuyeongProgressFormat, Mathf.Max(0, recipe.timeLimit - flow.MinigameTime));
            }
        }
        private string DirectionLabel(StageDirection step) => step switch
        {
            StageDirection.Up => copy.stepUp, StageDirection.Left => copy.stepLeft,
            StageDirection.Down => copy.stepDown, _ => copy.stepRight
        };
    }
}
