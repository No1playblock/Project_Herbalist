using UnityEngine;
namespace Herbalist.Abilities
{
    public enum SapControlMode { Placement, Hose }
    public enum PlayerAbilityKind { Leaf, Sap }
    public enum SapState { Inactive, Controlled, Attached, Bound, Complete, Extracting, Flying }
    [CreateAssetMenu(menuName = "Herbalist/Sap Ability Settings")]
    public sealed class SapAbilitySettings : ScriptableObject
    {
        public SapDeposit offlinePrefab;
        [Header("Reversible control mode")]
        public SapControlMode controlMode = SapControlMode.Placement;
        public Vector3 hoseHoverOffset = new Vector3(0.55f, 1.15f, 0.15f);
        [Min(0.01f)] public float hoseGrowthPerSecond = 0.5f;
        [Min(1)] public float hoseMaxScale = 3f;
        [Range(-1, 1)] public float hoseMergeNormalDot = 0.85f;
        [Min(0.1f)] public float hoseMarkLifetime = 5f;
        [Tooltip("Hits within this distance grow the existing mark instead of creating another.")]
        [Min(0.01f)] public float hoseMarkSpacing = 1f;
        public Vector3 hoverOffset = new Vector3(0.55f, 1.5f, 0.15f);
        [Min(0.01f)] public float extractionSpeed = 4;
        [Min(0.01f)] public float flightSpeed = 12;
        [Min(0.1f)] public float flightTimeout = 5;
        [Min(0.01f)] public float sourceRefreshInterval = 0.1f;
        [Min(0.001f)] public float arrivalTolerance = 0.03f;
        [Min(0.001f)] public float streamWidth = 0.045f;
        [Min(0)] public float streamFlowSpeed = 2;
        [Min(0)] public float streamBeadSize = 0.075f;
        public Vector3 extractionProbeOffset = new Vector3(0, 1.3f, 0);
        [Min(0.1f)] public float extractionRange = 3;
        [Min(0.1f)] public float controlRange = 18;
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
