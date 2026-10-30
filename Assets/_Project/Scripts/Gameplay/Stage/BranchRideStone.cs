using UnityEngine;

namespace Herbalist.Levels
{
    // Deterministic authored fall/roll/fall cycle; the host resolves swept contacts.
    public sealed class BranchRideStone : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private Transform[] _rollPoints;
        [SerializeField, Min(.1f)] private float _fallHeight = 8;
        [SerializeField, Min(.1f)] private float _fallDuration = .8f;
        [SerializeField, Min(.1f)] private float _rollSpeed = 6;
        [SerializeField, Min(.1f)] private float _exitDuration = 1;
        [SerializeField, Min(.1f)] private float _respawnDelay = 3;
        [SerializeField, Min(0)] private float _phaseOffset;
        [SerializeField, Min(.1f)] private float _radius = .8f;
        public Transform Visual => _visual;
        public float Radius => _radius;
        public float RollLength
        {
            get
            {
                float result = 0;
                for (int i = 1; i < _rollPoints.Length; i++) result += Vector3.Distance(_rollPoints[i - 1].position, _rollPoints[i].position);
                return result;
            }
        }
        public float CycleDuration => _fallDuration + RollLength / _rollSpeed + _exitDuration + _respawnDelay;
        public int CycleAt(float clock) => Mathf.FloorToInt((clock + _phaseOffset) / CycleDuration);
        public bool Sample(float clock, out Vector3 position)
        {
            position = transform.position;
            if (_rollPoints == null || _rollPoints.Length < 2) return false;
            float t = Mathf.Repeat(clock + _phaseOffset, CycleDuration);
            Vector3 up = Vector3.up * _radius;
            if (t < _fallDuration)
            {
                float f = t / _fallDuration;
                position = _rollPoints[0].position + up + Vector3.up * (_fallHeight * (1 - f * f));
                return true;
            }
            t -= _fallDuration;
            float rollTime = RollLength / _rollSpeed;
            if (t < rollTime)
            {
                float distance = t * _rollSpeed;
                for (int i = 1; i < _rollPoints.Length; i++)
                {
                    Vector3 a = _rollPoints[i - 1].position, b = _rollPoints[i].position;
                    float length = Vector3.Distance(a, b);
                    if (distance <= length) { position = Vector3.Lerp(a, b, distance / Mathf.Max(.001f, length)) + up; return true; }
                    distance -= length;
                }
            }
            t -= rollTime;
            position = _rollPoints[_rollPoints.Length - 1].position + up - Vector3.up * (_fallHeight * Mathf.Pow(t / _exitDuration, 2));
            return t < _exitDuration;
        }
        public void Present(float clock, bool running)
        {
            bool active = Sample(clock, out var position) && running;
            if (_visual == null) return;
            _visual.gameObject.SetActive(active);
            if (active)
            {
                _visual.position = position;
                _visual.rotation = Quaternion.Euler(clock * _rollSpeed / _radius * Mathf.Rad2Deg, 0, 0);
            }
        }
    }
}
