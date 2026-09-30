using UnityEngine;
namespace Herbalist.Abilities
{
    public enum PlayerAbilityKind { Leaf, Sap }
    public enum SapState { Inactive = 0, Controlled = 1, Attached = 2, Complete = 4, Extracting = 5 }
    [CreateAssetMenu(menuName = "Herbalist/Sap Ability Settings")]
    public sealed class SapAbilitySettings : ScriptableObject
    {
        public SapDeposit offlinePrefab;
        [Header("Hose control")]
        public Vector3 hoseHoverOffset = new Vector3(0.55f, 1.15f, 0.15f);
        [Tooltip("Stream length growth speed while the hose button is held, in meters per second.")]
        [Min(0.01f)] public float hoseExtendSpeed = 8f;
        [Tooltip("Stream length retraction speed after the hose button is released, in meters per second.")]
        [Min(0.01f)] public float hoseRetractSpeed = 12f;

        [Min(0.01f)] public float hoseGrowthPerSecond = 0.5f;
        [Min(1)] public float hoseMaxScale = 3f;
        [Range(-1, 1)] public float hoseMergeNormalDot = 0.85f;
        [Min(0.1f)] public float hoseMarkLifetime = 5f;
        [Tooltip("Hits within this distance grow the existing mark instead of creating another.")]
        [Min(0.01f)] public float hoseMarkSpacing = 1f;
        [Tooltip("World-space footprint of an attached sap decal before hose growth is applied.")]
        public Vector2 surfaceMarkSize = new Vector2(0.85f, 0.85f);
        [Tooltip("Projection depth used to wrap the sap decal over uneven receiver meshes.")]
        [Min(0.01f)] public float surfaceMarkDepth = 0.45f;
        [Tooltip("Maximum camera distance for attached sap decals.")]
        [Min(1f)] public float surfaceMarkDrawDistance = 50f;
        [Tooltip("Opacity of attached sap decals. Lower values make the liquid more transparent.")]
        [Range(0f, 1f)] public float surfaceMarkOpacity = 0.55f;
        [Header("Attached surface volume")]
        [Tooltip("Footprint of the raised liquid layer relative to the decal footprint.")]
        [Range(0.1f, 1f)] public float surfaceVolumeFootprintRatio = 0.72f;
        [Tooltip("Maximum outward thickness of the raised liquid layer in world units.")]
        [Min(0.001f)] public float surfaceVolumeThickness = 0.09f;
        [Min(0.01f)] public float extractionSpeed = 4;
        [Min(0.001f)] public float arrivalTolerance = 0.03f;
        [Min(0.001f)] public float streamWidth = 0.045f;
        [Min(0)] public float streamFlowSpeed = 2;
        [Min(0)] public float streamBeadSize = 0.075f;
        public Vector3 extractionProbeOffset = new Vector3(0, 1.3f, 0);
        [Min(0.1f)] public float extractionRange = 3;
        [Min(0.1f)] public float controlRange = 18;
        [Min(0.01f)] public float radius = 0.22f;
        [Min(0.01f)] public float surfaceOffset = 0.025f;
        public LayerMask collisionMask = 1;
        public Vector3 attachedScale = new Vector3(0.85f, 0.85f, 0.12f);
        public Vector3 controlledScale = Vector3.one * 0.44f;
    }
}
