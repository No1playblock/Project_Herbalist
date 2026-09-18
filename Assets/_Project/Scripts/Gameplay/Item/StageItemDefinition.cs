using UnityEngine;
using Herbalist.Abilities;
namespace Herbalist.StageOne
{
    public enum StageItemType { Herb, Potion }
    [CreateAssetMenu(menuName = "Herbalist/Stage/Item")]
    public sealed class StageItemDefinition : ScriptableObject
    {
        public string displayName;
        public StageItemType type;
        public PlayerAbilityKind ability;
        public Color color = Color.green;
    }
}
