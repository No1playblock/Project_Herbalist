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
        [Min(.01f)] public float maximumChannelWidth = .8f;
        [Min(0)] public float channelEdgeInset = .01f;
        public float localZ = -.53f;

        public float VisibleLength(float fraction) =>
            Mathf.Max(0, capacityMl * Mathf.Clamp01(fraction) - minimumHeadMl) /
            Mathf.Max(1, millilitersPerUnit);
    }
}
