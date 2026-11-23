using UnityEngine;
using UnityEngine.Serialization;

namespace Herbalist.GameUI
{
    public sealed class SpotlightWorldRingSetup : MonoBehaviour
    {
        [FormerlySerializedAs("_ringRadius"), Min(.01f)] public float radius = 1.2f;
        [FormerlySerializedAs("_cameraOffset"), Min(0)] public float cameraOffset = .2f;
        [FormerlySerializedAs("_dashCount"), Range(3, 128)] public int dashCount = 18;
        [FormerlySerializedAs("_dashCoverage"), Range(.05f, .95f)] public float dashFraction = .44f;
        [FormerlySerializedAs("_coreWidth"), Min(.001f)] public float coreWidth = .04f;
        [FormerlySerializedAs("_glowWidth"), Min(.001f)] public float glowWidth = .10f;
        [FormerlySerializedAs("_coreColor")] public Color coreColor = new(1f, .79f, .42f, 1f);
        [FormerlySerializedAs("_glowColor")] public Color glowColor = new(1f, .56f, .18f, .38f);
        [Tooltip("Scene view preview target only; gameplay selects its own guide target.")]
        public Transform previewTarget;
        public Vector3 previewCenter;
    }
}
