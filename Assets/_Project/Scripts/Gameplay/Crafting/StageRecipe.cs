using UnityEngine;
namespace Herbalist.StageOne
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Recipe")]
    public sealed class StageRecipe : ScriptableObject
    {
        public StageItemDefinition ingredient;
        public StageItemDefinition result;
    }
}
