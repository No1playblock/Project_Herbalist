using UnityEngine;

namespace Herbalist.GameUI
{
    public sealed class SpotlightWorldRingSetup : MonoBehaviour
    {
        [Min(.01f)] public float radius = 1.2f;
        [Min(0)] public float cameraOffset = .2f;
        [Range(3, 128)] public int dashCount = 18;
        [Range(.05f, .95f)] public float dashFraction = .44f;
        [Min(.001f)] public float coreWidth = .04f;
        [Min(.001f)] public float glowWidth = .10f;
        public Color coreColor = new(1f, .79f, .42f, 1f);
        public Color glowColor = new(1f, .56f, .18f, .38f);
        [Tooltip("Scene view preview target only; gameplay selects its own guide target.")]
        public Transform previewTarget;
        public Vector3 previewCenter;
    }
}
