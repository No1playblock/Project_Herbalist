using UnityEngine;

namespace Herbalist.Abilities
{
    // Presentation-only raised layer placed above the conforming decal.
    [DisallowMultipleComponent]
    public sealed class SapSurfaceVolumePresentation : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Mesh surfaceMesh;
        [SerializeField] private Mesh attachedBlobMesh;

        public bool Available => meshFilter != null && meshFilter.sharedMesh != null &&
                                 meshRenderer != null && meshRenderer.sharedMaterial != null;
        public bool AttachedBlobAvailable => Available && attachedBlobMesh != null;

        public void SetVisible(bool visible, SapAbilitySettings settings, float growth, float absorptionProgress,
            bool attachedBlob = false)
        {
            gameObject.SetActive(visible);
            if (!visible || settings == null) return;

            if (attachedBlob && attachedBlobMesh != null)
            {
                meshFilter.sharedMesh = attachedBlobMesh;
                Bounds bounds = attachedBlobMesh.bounds;
                float width = Mathf.Max(0.01f, settings.attachedBlobSize.x * growth);
                float height = Mathf.Max(0.01f, settings.attachedBlobSize.y * growth);
                float depth = Mathf.Max(0.01f, settings.attachedBlobDepth * growth);
                Vector3 scale = new Vector3(
                    width / Mathf.Max(0.001f, bounds.size.x),
                    height / Mathf.Max(0.001f, bounds.size.y),
                    depth / Mathf.Max(0.001f, bounds.size.z));
                transform.localScale = scale;
                transform.localPosition = new Vector3(
                    0f,
                    -bounds.min.y * scale.y,
                    -bounds.center.z * scale.z);
                return;
            }

            if (surfaceMesh != null)
                meshFilter.sharedMesh = surfaceMesh;

            float absorption = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(absorptionProgress));
            Vector2 footprint = settings.surfaceMarkSize *
                                (settings.surfaceVolumeFootprintRatio * Mathf.Max(0.01f, growth)) *
                                Mathf.Lerp(1f, 0.06f, absorption);
            float thickness = settings.surfaceVolumeThickness * (1f - absorption);
            transform.localScale = new Vector3(
                footprint.x,
                footprint.y,
                Mathf.Max(0.001f, thickness));
            transform.localPosition = Vector3.back * settings.surfaceVolumeThickness * absorption;
        }
    }
}
