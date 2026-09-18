using UnityEngine;
namespace Herbalist.StageOne
{
    public sealed class StageCarryPresentation : MonoBehaviour
    {
        public StageActor actor;
        public GameObject herb, potion;
        public Renderer herbRenderer, potionRenderer;
        private MaterialPropertyBlock block;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private void Awake() { block = new MaterialPropertyBlock(); }
        private void LateUpdate()
        {
            var flow = StageOneFlow.Instance;
            var item = flow != null && actor.Available && actor.Slot >= 0 && actor.Slot < 2 ? flow.settings.Item(flow.Progress.Held[actor.Slot]) : null;
            herb.SetActive(item != null && item.type == StageItemType.Herb);
            potion.SetActive(item != null && item.type == StageItemType.Potion);
            if (item == null) return;
            block.SetColor(BaseColor, item.color);
            herbRenderer.SetPropertyBlock(block); potionRenderer.SetPropertyBlock(block);
        }
    }
}
