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
        [Header("Leaf Size and Range")]
        [Tooltip("Maximum distance the leaf can fly from its launch position before turning back.")]
        [Min(0.1f)] public float range = 18;
        [Min(0.1f)] public float flightSpeed = 16;
        [Min(0.1f)] public float returnSpeed = 22;
        [Tooltip("A returning leaf accelerates when needed to reach its owner within this many seconds.")]
        [Min(0.1f)] public float maxReturnDuration = 1f;
        [Tooltip("Full revolutions of the visual mesh per second while flying and returning.")]
        [Min(0f)] public float flightSpinRevolutionsPerSecond = 2f;
        [Tooltip("World-space radius of the U-turn after a missed throw.")]
        [Min(0.1f)] public float missTurnRadius = 1.8f;
        [Tooltip("Multiplier for the installed leaf mesh and its platform collider, relative to the projectile prefab.")]
        [Min(0.1f)] public float installedScaleMultiplier = 1f;
        [Tooltip("Flying and returning leaf size relative to the projectile prefab, independent of the installed size.")]
        [Min(0.01f)] public float flyingVisualScaleMultiplier = 0.5f;
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
