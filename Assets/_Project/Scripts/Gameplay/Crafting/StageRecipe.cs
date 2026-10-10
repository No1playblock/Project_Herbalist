using UnityEngine;
namespace Herbalist.StageOne
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Recipe")]
    public sealed class StageRecipe : ScriptableObject
    {
        public StageItemDefinition ingredient;
        public StageItemDefinition result;
        [Header("Cooperative brewing")]
        [Min(0)] public float initialTemperature = 20f;
        [Min(0)] public float targetMin = 60f;
        [Min(0)] public float targetMax = 75f;
        [Min(0)] public float overheatTemperature = 90f;
        [Min(0.1f)] public float holdSeconds = 4f;
        [Min(0.1f)] public float timeLimit = 30f;
        [Min(0)] public float fanHeatPerSecond = 22f;
        [Min(0)] public float passiveCoolingPerSecond = 8f;
    }
}
