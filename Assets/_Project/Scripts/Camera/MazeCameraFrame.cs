using UnityEngine;

namespace Herbalist.Player
{
    // An authored, disabled camera marks the maze view. The player's own camera
    // renders the frame so its viewport, HUD, and audio ownership remain intact.
    [RequireComponent(typeof(Camera))]
    public sealed class MazeCameraFrame : MonoBehaviour
    {
        [SerializeField] private Collider _mazeBounds;
        [SerializeField, Min(1f)] private float _padding = 1.06f;

        public void SetMazeBounds(Collider value) => _mazeBounds = value;

        public float RequiredFarClip(Vector3 cameraPosition) => _mazeBounds == null ? 0f :
            Vector3.Distance(cameraPosition, _mazeBounds.bounds.center) + _mazeBounds.bounds.extents.magnitude + 1f;

        public bool TryGetFrame(float aspect, out Vector3 position, out Quaternion rotation, out float orthographicSize)
        {
            position = transform.position;
            rotation = transform.rotation;
            orthographicSize = 1f;
            if (_mazeBounds == null) return false;

            Bounds bounds = _mazeBounds.bounds;
            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 extent = bounds.extents;
            float halfWidth = Mathf.Abs(right.x) * extent.x + Mathf.Abs(right.y) * extent.y + Mathf.Abs(right.z) * extent.z;
            float halfHeight = Mathf.Abs(up.x) * extent.x + Mathf.Abs(up.y) * extent.y + Mathf.Abs(up.z) * extent.z;
            orthographicSize = Mathf.Max(halfHeight, halfWidth / Mathf.Max(.1f, aspect)) * _padding;

            float existingDistance = Vector3.Dot(bounds.center - position, forward);
            float distance = Mathf.Max(existingDistance, extent.magnitude + 1f);
            position = bounds.center - forward * distance;
            return orthographicSize > .01f;
        }
    }
}
