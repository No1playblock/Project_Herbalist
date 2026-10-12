using UnityEngine;
namespace Herbalist.Abilities
{
    public enum LeafState { Flying = 0, Installed = 1, Returning = 3, Complete = 4 }
    [CreateAssetMenu(menuName = "Herbalist/Abilities/Leaf Settings")]
    public sealed class LeafAbilitySettings : ScriptableObject
    {
        public LeafProjectile offlinePrefab;
        public bool faceAimOnThrow;
        public bool permanentInstallation;
        public bool installedCapacity;
        // Legacy serialized values kept for existing assets; the flight limit is range.
        [HideInInspector] public bool requireTarget;
        [HideInInspector] public float rangeHeightMultiplier;
        [Min(1)] public int capacity = 3;
        [Min(0.01f)] public float lifetime = 5;
        [Tooltip("Maximum distance the leaf can fly from its launch position before turning back.")]
        [Min(0.1f)] public float range = 18;
        [Min(0.1f)] public float flightSpeed = 16;
        [Min(0.1f)] public float returnSpeed = 22;
        [Tooltip("World-space radius of the U-turn after a missed throw.")]
        [Min(0.1f)] public float missTurnRadius = 1.8f;
        [Min(0.1f)] public float installedScaleMultiplier = 1f;
        [Range(0.01f, 1f)] public float flyingVisualScaleMultiplier = 0.5f;
        [Min(0)] public float curveWidth = 0f;
        [Min(0)] public float collisionRadius = 0.08f;
        [Min(0)] public float surfaceOffset = 0.12f;
        [Min(0)] public float tipEmbedDepth = 0.12f;
        [Range(0, 0.49f)] public float maxEmbedFraction = 0.1f;
        [Min(0.01f)] public float catchDistance = 0.25f;
        [Min(0)] public float cooldown = 0.25f;
        [Tooltip("Launch and catch position in the body frame's local coordinates; scales with the character.")]
        public Vector3 launchOffset = new Vector3(0f, 1.3f, 0.5f);
        public LayerMask hitMask = 1;
    }
}
