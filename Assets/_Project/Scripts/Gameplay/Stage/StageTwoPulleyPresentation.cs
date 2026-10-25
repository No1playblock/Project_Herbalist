using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // The imported wheels and rope links follow the authoritative raft height.
    // No presentation transform needs its own network state.
    public sealed class StageTwoPulleyPresentation : MonoBehaviour
    {
        [SerializeField] private Transform[] _wheels;
        [SerializeField] private Transform[] _ropeRuns;
        [SerializeField, Min(.01f)] private float _wheelRadius = 1.2f;
        [SerializeField, Min(.01f)] private float _ropeLinkPitch = 6f;
        [SerializeField] private Vector3 _wheelLocalAxis = Vector3.forward;

        private Quaternion[] _wheelRestRotations;
        private Transform[] _ropeLinks;
        private Vector3[] _ropeRestPositions;

        private void Awake() => CacheRestPose();

        private void CacheRestPose()
        {
            _wheelRestRotations = new Quaternion[_wheels.Length];
            for (int i = 0; i < _wheels.Length; i++)
                if (_wheels[i] != null) _wheelRestRotations[i] = _wheels[i].localRotation;

            var links = new List<Transform>();
            foreach (var run in _ropeRuns)
            {
                if (run == null) continue;
                foreach (Transform link in run) links.Add(link);
            }
            _ropeLinks = links.ToArray();
            _ropeRestPositions = new Vector3[_ropeLinks.Length];
            for (int i = 0; i < _ropeLinks.Length; i++)
                _ropeRestPositions[i] = _ropeLinks[i].position;
        }

        public void Present(float distanceDescended)
        {
            if (_wheelRestRotations == null) CacheRestPose();
            var distance = Mathf.Max(0f, distanceDescended);
            var angle = distance / _wheelRadius * Mathf.Rad2Deg;
            var axis = _wheelLocalAxis.sqrMagnitude > .0001f ? _wheelLocalAxis.normalized : Vector3.forward;
            for (int i = 0; i < _wheels.Length; i++)
                if (_wheels[i] != null)
                    _wheels[i].localRotation = _wheelRestRotations[i] * Quaternion.AngleAxis(angle, axis);

            // The supplied rope is a chain of repeated links. Scroll the links by one
            // pitch so that the visible run moves without detaching from its pulley.
            var offset = Mathf.Repeat(distance, _ropeLinkPitch);
            for (int i = 0; i < _ropeLinks.Length; i++)
                if (_ropeLinks[i] != null)
                    _ropeLinks[i].position = _ropeRestPositions[i] + Vector3.down * offset;
        }
    }
}
