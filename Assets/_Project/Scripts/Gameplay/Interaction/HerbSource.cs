using UnityEngine;
namespace Herbalist.StageOne
{
    public sealed class HerbSource : MonoBehaviour
    {
        public StageItemDefinition item;
        public Transform interactionPoint;
        public GameObject visual;
        public GameObject glow;
        public Vector3 Point => interactionPoint != null ? interactionPoint.position : transform.position;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private void Awake()
        {
            if (item == null || visual == null) return;
            var block = new MaterialPropertyBlock();
            block.SetColor(BaseColor, item.color);
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true)) renderer.SetPropertyBlock(block);
        }
        public void Present(bool harvested)
        {
            if (visual != null) visual.SetActive(!harvested);
            if (glow != null) glow.SetActive(!harvested);
        }
    }
}
