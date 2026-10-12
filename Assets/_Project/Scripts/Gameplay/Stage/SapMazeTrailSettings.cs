using UnityEngine;

namespace Herbalist.Levels
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Sap Maze Trail Settings")]
    public sealed class SapMazeTrailSettings : ScriptableObject
    {
        [Min(1)] public float capacityMl = 500;
        [Min(0)] public float minimumHeadMl = 40;
        [Min(1)] public float millilitersPerUnit = 80;
        [Min(.02f)] public float sampleSpacing = .08f;
        [Min(.01f)] public float trailWidth = .16f;
        public float localZ = -.53f;
    }
}
