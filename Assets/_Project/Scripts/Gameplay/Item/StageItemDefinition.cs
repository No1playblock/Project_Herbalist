using UnityEngine;
using Herbalist.Abilities;
namespace Herbalist.StageOne
{
    public enum StageItemType { Herb, Potion }
    [CreateAssetMenu(menuName = "Herbalist/Stage/Item")]
    public sealed class StageItemDefinition : ScriptableObject
    {
        public string displayName;
        public string contentId;
        public StageItemType type;
        public PlayerAbilityKind ability;
        public Color color = Color.green;
        [Header("Herb identification")]
        public Sprite codexImage;
        public bool isMedicineMaterial;
        [TextArea] public string appearance;
        [TextArea] public string harvestingMethod;
        public StageDirection[] harvestingSteps;
    }
}
