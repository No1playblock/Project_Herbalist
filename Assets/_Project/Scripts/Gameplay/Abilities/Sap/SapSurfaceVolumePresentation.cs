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

        public void SetVisible(bool visible, SapAbilitySettings settings, float growth)
        {
            gameObject.SetActive(visible);
            if (!visible || settings == null) return;

            Vector2 footprint = settings.surfaceMarkSize *
                                (settings.surfaceVolumeFootprintRatio * Mathf.Max(0.01f, growth));
            transform.localScale = new Vector3(
                footprint.x,
                footprint.y,
                settings.surfaceVolumeThickness);
        }
    }
}
