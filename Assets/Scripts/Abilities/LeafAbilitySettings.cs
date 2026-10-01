using UnityEngine;
namespace Herbalist.Abilities
{
    public enum LeafMode { Off, Pin, Platform }
    public enum LeafState { Flying, Installed, Bound, Returning, Complete }
    [CreateAssetMenu(menuName = "Herbalist/Abilities/Leaf Settings")]
    public sealed class LeafAbilitySettings : ScriptableObject
    {
        public LeafProjectile offlinePrefab;
        [Min(1)] public int capacity = 3;
        [Min(0.01f)] public float lifetime = 5;
        [Min(0.1f)] public float range = 18;
        [Min(0.1f)] public float flightSpeed = 16;
        [Min(0.1f)] public float returnSpeed = 22;
        [Min(0)] public float curveWidth = 0f;
        [Min(0)] public float collisionRadius = 0.08f;
        [Min(0)] public float surfaceOffset = 0.12f;
        [Min(0.01f)] public float catchDistance = 0.25f;
        [Min(0)] public float cooldown = 0.25f;
        public Vector3 launchOffset = new Vector3(0f, 1.3f, 0.5f);
        public LayerMask hitMask = 1;
    }
}
