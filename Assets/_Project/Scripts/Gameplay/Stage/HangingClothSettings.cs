using UnityEngine;

namespace Herbalist.Levels
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Hanging Cloth Tuning")]
    public sealed class HangingClothSettings : ScriptableObject
    {
        [Header("Interaction")]
        [Min(.1f)] public float grabRange = 16f;
        [Min(0)] public float minimumHoldSeconds = 1f;

        [Header("Swing")]
        [Range(0, 60)] public float maxAngle = 30f;
        [Min(0)] public float driveAcceleration = 15f;
        [Min(0)] public float pumpImpulseSeconds = .4f;
        [Min(0)] public float reversalInputWindow = .45f;
        [Min(0)] public float spring = 4f;
        [Min(0)] public float damping = .22f;
        [Min(0), Tooltip("Maximum angular speed in degrees per second. 0 removes the limit.")]
        public float maxSwingSpeed = 10000f;
        [Min(0)] public float releaseSpeed = 1f;

        [Header("Branch bend")]
        [Range(0, 30)] public float bendMaxAngle = 30f;
        [Min(.01f)] public float bendProbeRadius = .08f;
        [Min(.1f)] public float minBendSegmentLength = 1f;
        public LayerMask obstructionMask = 1;
    }
}
