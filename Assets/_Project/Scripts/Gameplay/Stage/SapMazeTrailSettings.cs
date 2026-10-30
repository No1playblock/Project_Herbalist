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

        [Header("Conservative 2D fluid")]
        public bool useVolumeField;
        [Tooltip("Water volume held by one square board-local unit at full density.")]
        [Min(1)] public float volumePerLocalArea = 50f;
        [Tooltip("Pressure spread in navigation cells. Larger values make a wider pool.")]
        [Min(1)] public float pressureCells = 4f;
        [Min(.01f)] public float grabRadiusWorld = 1.5f;
        [Range(.001f, .25f)] public float surfaceThreshold = .04f;
        [Min(.001f)] public float simulationStep = .008f;
        [Tooltip("Rebuild the attraction field after the grabbed point moves this many navigation cells.")]
        [Min(1)] public float potentialRefreshDistance = 3f;

        [Header("Water boundary rounding (presentation)")]
        [Min(0)] public float surfaceRoundRadiusWorld = .8f;
        [Range(0, 4)] public int surfaceRoundPasses = 3;
        [Range(0, 1)] public float surfaceRoundStrength = .75f;

        [Header("Flexible tube body")]
        public bool useTubeBody;
        [Min(.1f)] public float tubeWidthWorld = .85f;
        [Min(.1f)] public float tubeInitialLengthWorld = 8f;
        [Min(.1f)] public float tubeMaxLengthWorld = 32f;
        [Min(.1f)] public float tubePointSpacingWorld = .35f;
        [Range(1, 4)] public int tubeConstraintPasses = 2;
        [Range(4, 12)] public int tubeRadialSegments = 8;

        public float VisibleLength(float fraction) =>
            Mathf.Max(0, capacityMl * Mathf.Clamp01(fraction) - minimumHeadMl) /
            Mathf.Max(1, millilitersPerUnit);
    }
}
