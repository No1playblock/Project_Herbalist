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
        public void Present(bool harvested)
        {
            if (visual != null) visual.SetActive(!harvested);
            if (glow != null) glow.SetActive(!harvested);
        }
    }
}
