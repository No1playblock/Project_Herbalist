using UnityEngine;
namespace Herbalist.Abilities
{
    public enum PlayerAbilityKind { Leaf, Sap }
    public enum SapState { Inactive, Controlled, Attached, Bound, Complete }
    [CreateAssetMenu(menuName = "Herbalist/Sap Ability Settings")]
    public sealed class SapAbilitySettings : ScriptableObject
    {
        public SapDeposit offlinePrefab;
        public Vector3 extractionProbeOffset = new Vector3(0, 1.3f, 0);
        [Min(0.1f)] public float extractionRange = 3;
        [Min(0.1f)] public float controlRange = 18;
        [Min(0.1f)] public float freeAimDistance = 10;
        [Min(0.1f)] public float moveSpeed = 9;
        [Min(0.01f)] public float radius = 0.22f;
        [Min(0.01f)] public float surfaceOffset = 0.025f;
        [Min(0.01f)] public float placementTolerance = 0.3f;
        [Min(0.01f)] public float lifetime = 5;
        [Min(0.01f)] public float bindingRadius = 0.7f;
        [Min(0.01f)] public float refreshRadius = 0.5f;
        [Min(1)] public int capacity = 8;
        public LayerMask collisionMask = 1;
        public Vector3 attachedScale = new Vector3(0.85f, 0.85f, 0.12f);
        public Vector3 controlledScale = Vector3.one * 0.44f;
    }
}
