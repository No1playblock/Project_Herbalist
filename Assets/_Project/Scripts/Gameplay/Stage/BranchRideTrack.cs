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
        public Vector3 Sample(float distance, out Vector3 forward)
        {
            forward = Vector3.forward;
            for (int i = 1; i < _points.Length; i++)
            {
                Vector3 delta = _points[i].position - _points[i - 1].position;
                float length = delta.magnitude;
                if (length < .0001f) continue;
                forward = delta / length;
                if (distance <= length) return _points[i - 1].position + forward * Mathf.Max(0, distance);
                distance -= length;
            }
            return End;
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
