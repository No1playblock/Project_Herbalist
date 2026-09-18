using UnityEngine;
using Herbalist.Presentation;
namespace Herbalist.StageOne
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Stage One Settings")]
    public sealed class StageOneSettings : ScriptableObject
    {
        public StageItemDefinition[] items;
        public StageRecipe[] recipes;
        public CharacterRole[] slotRoles = { CharacterRole.Duyeong, CharacterRole.Sodam };
        [Min(0.1f)] public float interactionRange = 3;
        public Vector3 interactionOffset = new Vector3(0, 1, 0);
        public LayerMask obstructionMask = 1;
        [Range(0,31)] public int herbGlowLayer = 30;
        public string gatherMessage = "Find herbs. Duyeong gathers; Sodam crafts.";
        public string craftMessage = "Deliver the herbs to Sodam and craft both potions.";
        public string drinkMessage = "Drink one potion each. Abilities are not tied to characters.";
        public string enterMessage = "Both abilities acquired. Enter the tree together.";
        public string clearMessage = "STAGE 1 CLEAR";
        public string controlsFormat = "{0}: Gather / Give   {1}: Craft   {2}: Drink";
        public string pocketFormat = "{0} | Holding: {1} | Ability: {2}";
        public string emptyText = "Empty";
        public string lockedText = "Locked";
        public string fullMessage = "Your partner must have empty hands.";
        public string rangeMessage = "Move closer to a herb or your partner, with a clear path.";
        public string roleMessage = "Duyeong gathers herbs; Sodam crafts potions.";
        public string drinkBlockedMessage = "You already have an ability. You cannot drink another potion.";
        public string invalidItemMessage = "Hold the required herb or potion first.";
        public string successMessage = "Done.";
        public string waitingMessage = "Waiting for both players.";
        public string clearedFormat = "{0} / {1} players inside";
        public int ItemId(StageItemDefinition item) => System.Array.IndexOf(items, item) + 1;
        public StageItemDefinition Item(int id) => id > 0 && id <= items.Length ? items[id - 1] : null;
        public uint RequiredMask => recipes.Length == 32 ? uint.MaxValue : (1u << recipes.Length) - 1;
        public bool Valid(out string error)
        {
            error = null;
            if (slotRoles == null || slotRoles.Length != 2 || slotRoles[0] == slotRoles[1]) error = "Stage One requires two distinct character roles.";
            else if (items == null || recipes == null || recipes.Length == 0 || recipes.Length > 32) error = "Assign 1 to 32 recipes and the item catalog.";
            else
            {
                var ingredients = new System.Collections.Generic.HashSet<StageItemDefinition>();
                var outputs = new System.Collections.Generic.HashSet<StageItemDefinition>();
                foreach (var r in recipes)
                    if (r == null || r.ingredient == null || r.result == null || r.ingredient.type != StageItemType.Herb || r.result.type != StageItemType.Potion || ItemId(r.ingredient) <= 0 || ItemId(r.result) <= 0 || !ingredients.Add(r.ingredient) || !outputs.Add(r.result))
                    { error = "Recipes require unique catalog herbs and potion outputs."; break; }
            }
            return error == null;
        }
    }
}
