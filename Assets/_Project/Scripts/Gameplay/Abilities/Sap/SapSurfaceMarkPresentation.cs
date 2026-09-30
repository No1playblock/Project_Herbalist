using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Herbalist.Abilities
{
    // Presentation-only adapter. SapDeposit remains the source of gameplay state and lifetime.
    [DisallowMultipleComponent]
    public sealed class SapSurfaceMarkPresentation : MonoBehaviour
    {
        [SerializeField] private DecalProjector projector;

        public bool Available => projector != null && projector.material != null;

        public void SetVisible(bool visible, SapAbilitySettings settings, float growth)
        {
            if (projector == null) return;
            gameObject.SetActive(visible);
            if (!visible || settings == null) return;

            Vector2 footprint = settings.surfaceMarkSize * Mathf.Max(0.01f, growth);
            projector.size = new Vector3(footprint.x, footprint.y, settings.surfaceMarkDepth);
            projector.pivot = new Vector3(0, 0, settings.surfaceMarkDepth * .5f);
            projector.drawDistance = settings.surfaceMarkDrawDistance;
            projector.fadeScale = .8f;
            projector.fadeFactor = settings.surfaceMarkOpacity;
        }
    }
}
