using UnityEngine;
using Herbalist.Abilities;

namespace Herbalist.Levels
{
    public sealed class BranchRideSwitch : MonoBehaviour
    {
        [SerializeField] private SapInjectionPort _port;
        [SerializeField] private Transform[] _pivots;
        [Tooltip("A branch poses. The common entrance remains connected in both states.")]
        [SerializeField] private Vector3[] _disconnectedEuler;
        [Tooltip("B branch poses. Field identifiers are retained for serialized compatibility.")]
        [SerializeField] private Vector3[] _connectedEuler;
        [SerializeField, Min(.01f)] private float _chargeRequired = .15f;
        [SerializeField, Min(.01f)] private float _rearmDelay = .35f;
        [SerializeField, Min(.01f)] private float _rotationDuration = .6f;
        private float _charge, _dry;
        private bool _armed = true;
        public SapInjectionPort Port => _port;
        public int BranchCount => _pivots?.Length ?? 0;
        public float RotationDuration => _rotationDuration;
        public bool ConsumeActivation(float dt)
        {
            float hit = _port != null ? _port.Consume() : 0;
            _dry = hit > 0 ? 0 : _dry + dt;
            if (_dry >= _rearmDelay) { _armed = true; _charge = 0; }
            if (!_armed) return false;
            _charge += hit;
            if (_charge < _chargeRequired) return false;
            _charge = 0;
            _armed = false;
            return true;
        }
        public void ResetInput()
        {
            _charge = _dry = 0;
            _armed = true;
            if (_port != null) _port.Consume();
        }
        public void Present(float fraction)
        {
            if (_pivots == null) return;
            float t = Mathf.SmoothStep(0, 1, fraction);
            for (int i = 0; i < _pivots.Length; i++)
                if (_pivots[i] != null && i < _connectedEuler.Length && i < _disconnectedEuler.Length)
                    _pivots[i].localRotation = Quaternion.Slerp(Quaternion.Euler(_disconnectedEuler[i]), Quaternion.Euler(_connectedEuler[i]), t);
        }
    }
}
