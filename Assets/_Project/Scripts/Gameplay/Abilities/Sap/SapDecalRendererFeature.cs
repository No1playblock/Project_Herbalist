using Herbalist.StageOne;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Herbalist.Abilities
{
    // Stage One uses projected sap marks as its only decal draw source.
    public sealed class SapDecalRendererFeature : DecalRendererFeature
    {
        private static bool NeedsDecals(CameraType cameraType)
        {
            return cameraType != CameraType.Game || !Application.isPlaying ||
                StageOneFlow.Instance == null || SapSurfaceMarkPresentation.HasActiveMarks;
        }

        public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
        {
            if (NeedsDecals(cameraData.cameraType))
                base.OnCameraPreCull(renderer, in cameraData);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (NeedsDecals(renderingData.cameraData.cameraType))
                base.AddRenderPasses(renderer, ref renderingData);
        }
    }
}
