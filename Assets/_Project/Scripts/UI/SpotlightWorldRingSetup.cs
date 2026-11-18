using UnityEngine;

namespace Herbalist.GameUI
{
    // Inspector settings for the runtime-drawn spotlight ring.
    public sealed class SpotlightWorldRingSetup : MonoBehaviour
    {
        [SerializeField] private Material _ringMaterial;
        [SerializeField, Min(.01f)] private float _ringRadius = 1.2f;
        [SerializeField, Min(0f)] private float _cameraOffset = .2f;
        [SerializeField, Range(3, 128)] private int _dashCount = 24;
        [SerializeField, Range(.1f, .95f)] private float _dashCoverage = .62f;
        [SerializeField, Min(.001f)] private float _coreWidth = .04f;
        [SerializeField, Min(.001f)] private float _glowWidth = .18f;
        [SerializeField] private Color _coreColor = new Color(1f, .79f, .42f, 1f);
        [SerializeField] private Color _glowColor = new Color(1f, .56f, .18f, .38f);

        public Material RingMaterial => _ringMaterial;
        public float RingRadius => Mathf.Max(.01f, _ringRadius);
        public float CameraOffset => Mathf.Max(0f, _cameraOffset);
        public int DashCount => Mathf.Max(3, _dashCount);
        public float DashCoverage => Mathf.Clamp(_dashCoverage, .1f, .95f);
        public float CoreWidth => Mathf.Max(.001f, _coreWidth);
        public float GlowWidth => Mathf.Max(.001f, _glowWidth);
        public Color CoreColor => _coreColor;
        public Color GlowColor => _glowColor;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            Camera sceneCamera = Camera.current;
            Quaternion facing = sceneCamera != null ? sceneCamera.transform.rotation : transform.rotation;
            Vector3 axisX = facing * Vector3.right;
            Vector3 axisY = facing * Vector3.up;
            Vector3 center = transform.position;
            Color previous = Gizmos.color;

            DrawPreviewDashes(center, axisX, axisY, GlowWidth, GlowColor);
            DrawPreviewDashes(center, axisX, axisY, CoreWidth, CoreColor);
            Gizmos.color = previous;
        }

        private void DrawPreviewDashes(Vector3 center, Vector3 axisX, Vector3 axisY,
            float width, Color color)
        {
            Gizmos.color = color;
            float cellAngle = Mathf.PI * 2f / DashCount;
            float dashAngle = cellAngle * DashCoverage;
            float radius = RingRadius;
            float halfWidth = radius * width * .5f;

            for (int dash = 0; dash < DashCount; dash++)
            {
                float middle = (dash + .5f) * cellAngle;
                float start = middle - dashAngle * .5f;
                Vector3 previousOuter = default;
                Vector3 previousInner = default;
                const int steps = 12;
                for (int step = 0; step <= steps; step++)
                {
                    float angle = start + dashAngle * step / steps;
                    Vector3 direction = axisX * Mathf.Cos(angle) + axisY * Mathf.Sin(angle);
                    Vector3 outer = center + direction * (radius + halfWidth);
                    Vector3 inner = center + direction * Mathf.Max(.001f, radius - halfWidth);
                    if (step > 0)
                    {
                        Gizmos.DrawLine(previousOuter, outer);
                        Gizmos.DrawLine(previousInner, inner);
                    }
                    previousOuter = outer;
                    previousInner = inner;
                }
            }
        }
#endif
    }
}
