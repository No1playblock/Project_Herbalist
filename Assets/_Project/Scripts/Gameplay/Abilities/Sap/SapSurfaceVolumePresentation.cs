using UnityEngine;

namespace Herbalist.Abilities
{
    // Presentation-only raised layer placed above the conforming decal.
    [DisallowMultipleComponent]
    public sealed class SapSurfaceVolumePresentation : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        public bool Available => meshFilter != null && meshFilter.sharedMesh != null &&
                                 meshRenderer != null && meshRenderer.sharedMaterial != null;

        public void SetVisible(bool visible, SapAbilitySettings settings, float growth, float absorptionProgress)
        {
            gameObject.SetActive(visible);
            if (!visible || settings == null) return;

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
