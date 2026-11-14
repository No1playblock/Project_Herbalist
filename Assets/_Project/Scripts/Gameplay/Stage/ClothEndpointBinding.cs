using UnityEngine;

namespace Herbalist.Levels
{
    // Presentation only: native Cloth deforms between the host-driven endpoints.
    // Its renderer transform stays at unit scale; lowering is skinned by two bones.
    [ExecuteAlways, DefaultExecutionOrder(200)]
    public sealed class ClothEndpointBinding : MonoBehaviour
    {
        [SerializeField] private HangingCloth _driver;
        [SerializeField] private Transform _end;
        [SerializeField] private Transform _topBone;
        [SerializeField] private Transform _endBone;
        [SerializeField] private SkinnedMeshRenderer _renderer;
        [SerializeField] private Vector3 _restAxis;
        [SerializeField, Min(.1f)] private float _boundsPadding = 2f;

        private void Update()
        {
            if (_driver == null || _end == null || _topBone == null || _endBone == null || _renderer == null) return;
            Vector3 top = _driver.transform.position, end = _end.position;
            Vector3 direction = end - top;
            Quaternion rotation = direction.sqrMagnitude > .000001f ? Quaternion.FromToRotation(_restAxis, direction) : _topBone.rotation;
            _topBone.SetPositionAndRotation(top, rotation);
            _endBone.SetPositionAndRotation(end, rotation);
            _renderer.enabled = _driver.isActiveAndEnabled && (!Application.isPlaying || !_driver.HasRetracted);
            var bounds = new Bounds(transform.InverseTransformPoint(top), Vector3.zero);
            bounds.Encapsulate(transform.InverseTransformPoint(end)); bounds.Expand(_boundsPadding * 2f);
            _renderer.localBounds = bounds;
        }
    }
}
