using UnityEngine;

namespace Herbalist.Levels
{
    // Nodes may be children of rotating pivots. The carrier follows their actual world poses.
    public sealed class BranchRideTrack : MonoBehaviour
    {
        [SerializeField] private Transform[] _points;
        [SerializeField] private bool _drop;
        [SerializeField] private int _switchIndex = -1;
        [Tooltip("Actual outgoing branches. A switch span can connect to either A or B by its tip pose.")]
        [SerializeField] private BranchRideTrack[] _exits = System.Array.Empty<BranchRideTrack>();
        [SerializeField] private bool _isFinish;
        public bool IsDrop => _drop;
        public int SwitchIndex => _switchIndex;
        public BranchRideTrack[] Exits => _exits;
        public bool IsFinish => _isFinish;
        public Vector3 Start => _points[0].position;
        public Vector3 End => _points[_points.Length - 1].position;
        public bool Valid => _points != null && _points.Length >= 2 && System.Array.TrueForAll(_points, p => p != null);
        public float Length
        {
            get
            {
                float length = 0;
                if (Valid) for (int i = 1; i < _points.Length; i++) length += Vector3.Distance(_points[i - 1].position, _points[i].position);
                return length;
            }
        }
        public Vector3 Sample(float distance, out Vector3 forward, float cornerDistance = 0f,
            Vector3 incoming = default, Vector3 outgoing = default)
        {
            forward = Vector3.forward;
            for (int i = 1; i < _points.Length; i++)
            {
                Vector3 start = _points[i - 1].position;
                Vector3 end = _points[i].position;
                Vector3 delta = end - start;
                float length = delta.magnitude;
                if (length < .0001f) continue;
                forward = delta / length;
                if (distance <= length)
                {
                    float along = Mathf.Max(0, distance);
                    float blend = Mathf.Min(cornerDistance, length * .5f);
                    if (blend > .0001f && along < blend)
                    {
                        Vector3 previous = i > 1 ? (start - _points[i - 2].position).normalized : incoming;
                        Vector3 tangent = (previous + forward).normalized;
                        if (Vector3.Dot(tangent, forward) > 0)
                            return BlendCorner(start, forward, tangent, blend, along / blend, out forward);
                    }
                    if (blend > .0001f && length - along < blend)
                    {
                        Vector3 next = i + 1 < _points.Length ? (_points[i + 1].position - end).normalized : outgoing;
                        Vector3 tangent = (-next - forward).normalized;
                        if (Vector3.Dot(tangent, -forward) > 0)
                        {
                            Vector3 point = BlendCorner(end, -forward, tangent, blend, (length - along) / blend, out forward);
                            forward = -forward;
                            return point;
                        }
                    }
                    return start + forward * along;
                }
                distance -= length;
            }
            return End;
        }

        // Hermite between the corner and the nearby straight section. Both sides share
        // a tangent at the node while the authored node itself stays on the path.
        private static Vector3 BlendCorner(Vector3 corner, Vector3 straight, Vector3 tangent, float length,
            float t, out Vector3 forward)
        {
            Vector3 difference = tangent - straight;
            float remaining = 1f - t;
            forward = (straight + difference * (remaining * (1f - 3f * t))).normalized;
            return corner + length * t * (straight + difference * (remaining * remaining));
        }
        public static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            float t = delta.sqrMagnitude > .000001f ? Mathf.Clamp01(Vector3.Dot(point - a, delta) / delta.sqrMagnitude) : 0;
            return Vector3.Distance(point, a + delta * t);
        }
        private void OnDrawGizmosSelected()
        {
            if (!Valid) return;
            Gizmos.color = _drop ? Color.cyan : Color.yellow;
            for (int i = 1; i < _points.Length; i++) Gizmos.DrawLine(_points[i - 1].position, _points[i].position);
        }
    }
}
