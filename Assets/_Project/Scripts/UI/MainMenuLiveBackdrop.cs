using UnityEngine;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(Camera))]
    public sealed class MainMenuLiveBackdrop : MonoBehaviour
    {
        [SerializeField] private Vector3 _lookAt;
        [SerializeField, Min(1f)] private float _orbitRadius = 70f;
        [SerializeField, Min(1f)] private float _heightAboveFocus = 95f;
        [SerializeField] private float _startingYaw = -45f;
        [SerializeField] private float _degreesPerSecond = 0.6f;

        private float _yaw;

        private void Awake()
        {
            _yaw = _startingYaw;
            PositionCamera();
        }

        private void LateUpdate()
        {
            _yaw += _degreesPerSecond * Time.unscaledDeltaTime;
            PositionCamera();
        }

        private void PositionCamera()
        {
            float radians = _yaw * Mathf.Deg2Rad;
            transform.position = _lookAt + new Vector3(
                Mathf.Cos(radians) * _orbitRadius,
                _heightAboveFocus,
                Mathf.Sin(radians) * _orbitRadius);
            transform.LookAt(_lookAt);
        }
    }
}
