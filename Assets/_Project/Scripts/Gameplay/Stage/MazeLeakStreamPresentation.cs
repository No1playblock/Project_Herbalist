using UnityEngine;

namespace Herbalist.Levels
{
    // Visual only. MazeLeakSocket gates this authored object using the Host's replicated leak mask.
    public sealed class MazeLeakStreamPresentation : MonoBehaviour
    {
        [SerializeField] private MeshFilter _tubeFilter;
        [SerializeField] private ParticleSystem _droplets;
        [SerializeField, Min(.01f)] private float _diameter = .44f;
        [SerializeField, Min(.01f)] private float _launchSpeed = 4f;
        [SerializeField, Min(0)] private float _gravity = 9.81f;
        [SerializeField, Min(.05f)] private float _flightTime = .8f;
        [SerializeField, Range(6, 32)] private int _pathSegments = 22;
        [SerializeField, Range(6, 16)] private int _radialSegments = 12;
        [SerializeField, Min(.1f)] private float _patternReferenceLength = 100f;
        [SerializeField, Min(0)] private float _pathSway = .018f;
        [SerializeField, Range(0, .1f)] private float _surfacePulse = .035f;
        [SerializeField, Range(0, .1f)] private float _surfaceDetail = .015f;
        [SerializeField, Min(1)] private float _surfaceWaves = 5.5f;
        [SerializeField, Min(1)] private float _detailWaves = 11f;
        [SerializeField, Min(0)] private float _surfaceSpeed = 6f;
        [SerializeField, Min(0)] private float _dropsPerMeterPerSecond = 1.8f;
        [SerializeField] private Vector2 _dropSize = new Vector2(.035f, .085f);
        [SerializeField] private Vector2 _sideSpeed = new Vector2(.45f, 1.25f);
        [SerializeField, Min(0)] private float _dropForwardSpeed = .6f;
        [SerializeField, Range(1, 12)] private int _maxDropsPerFrame = 6;

        private Mesh _mesh;
        private Vector3[] _path, _vertices, _normals;
        private Vector2[] _uv;
        private float _flowTime, _sprayBudget;

        private void Awake()
        {
            if (_tubeFilter == null) return;
            int rings = _pathSegments + 1, stride = _radialSegments + 1;
            _path = new Vector3[rings];
            _vertices = new Vector3[rings * stride];
            _normals = new Vector3[_vertices.Length];
            _uv = new Vector2[_vertices.Length];
            var triangles = new int[_pathSegments * _radialSegments * 6];
            int index = 0;
            for (int ring = 0; ring < _pathSegments; ring++)
            for (int side = 0; side < _radialSegments; side++)
            {
                int a = ring * stride + side, b = a + stride;
                triangles[index++] = a; triangles[index++] = a + 1; triangles[index++] = b;
                triangles[index++] = a + 1; triangles[index++] = b + 1; triangles[index++] = b;
            }
            _mesh = new Mesh { name = "MazeLeakWaterTube" };
            _mesh.MarkDynamic();
            _mesh.vertices = _vertices; _mesh.triangles = triangles;
            _tubeFilter.sharedMesh = _mesh;
        }

        private void OnEnable() { _flowTime = 0; _sprayBudget = 0; }

        private void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Herbalist.GameUI.GameplayPause.IsPaused ? 0 : Time.deltaTime;
            _flowTime += dt;
            Vector3 origin = transform.position, velocity = transform.forward * _launchSpeed;
            Vector3 previousSide = Vector3.zero;
            float distance = 0;
            for (int ring = 0; ring < _path.Length; ring++)
            {
                float t = ring / (float)_pathSegments, time = t * _flightTime;
                float envelope = Mathf.Sin(Mathf.PI * t);
                _path[ring] = origin + velocity * time + Vector3.down * (.5f * _gravity * time * time)
                    + transform.right * (Mathf.Sin(_flowTime * 2.4f + t * 5.6f) * _pathSway * envelope);
            }
            // Same tube cross-section, lengthwise UVs and surface waves as SapStreamPresentation.
            for (int ring = 0; ring < _path.Length; ring++)
            {
                if (ring > 0) distance += Vector3.Distance(_path[ring - 1], _path[ring]);
                float pattern = distance / Mathf.Max(.1f, _patternReferenceLength);
                var tangent = (_path[Mathf.Min(ring + 1, _pathSegments)] - _path[Mathf.Max(ring - 1, 0)]).normalized;
                var side = ring == 0 ? Vector3.Cross(tangent, Vector3.up) : Vector3.ProjectOnPlane(previousSide, tangent);
                if (side.sqrMagnitude < .0001f) side = Vector3.Cross(tangent, Vector3.right);
                side.Normalize();
                var up = Vector3.Cross(tangent, side).normalized;
                previousSide = side;
                for (int segment = 0; segment <= _radialSegments; segment++)
                {
                    float angle = segment * Mathf.PI * 2f / _radialSegments;
                    var radial = side * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                    float pulse = 1 + _surfacePulse * Mathf.Sin(pattern * Mathf.PI * 2 * _surfaceWaves - _flowTime * _surfaceSpeed + angle * 3)
                        + _surfaceDetail * Mathf.Sin(pattern * Mathf.PI * 2 * _detailWaves + _flowTime * _surfaceSpeed * .67f + angle * 5);
                    int i = ring * (_radialSegments + 1) + segment;
                    _vertices[i] = _tubeFilter.transform.InverseTransformPoint(_path[ring] + radial * (_diameter * .5f * pulse));
                    _normals[i] = _tubeFilter.transform.InverseTransformDirection(radial).normalized;
                    _uv[i] = new Vector2(pattern, segment / (float)_radialSegments);
                }
            }
            _mesh.vertices = _vertices; _mesh.normals = _normals; _mesh.uv = _uv; _mesh.RecalculateBounds();
            EmitDroplets(dt, distance);
        }

        private void EmitDroplets(float dt, float length)
        {
            if (_droplets == null) return;
            if (dt <= 0) { if (_droplets.isPlaying) _droplets.Pause(false); return; }
            if (!_droplets.isPlaying) _droplets.Play(false);
            _sprayBudget = Mathf.Min(_maxDropsPerFrame, _sprayBudget + length * _dropsPerMeterPerSecond * dt);
            int count = Mathf.FloorToInt(_sprayBudget); _sprayBudget -= count;
            for (int i = 0; i < count; i++)
            {
                float at = Random.Range(.08f, .92f) * _pathSegments;
                int segment = Mathf.Min(Mathf.FloorToInt(at), _pathSegments - 1);
                var tangent = (_path[segment + 1] - _path[segment]).normalized;
                var side = Vector3.Cross(tangent, Vector3.up);
                if (side.sqrMagnitude < .0001f) side = Vector3.right;
                side = Quaternion.AngleAxis(Random.Range(0, 360f), tangent) * side.normalized;
                _droplets.Emit(new ParticleSystem.EmitParams
                {
                    position = Vector3.Lerp(_path[segment], _path[segment + 1], at - segment),
                    velocity = tangent * _dropForwardSpeed + side * Random.Range(_sideSpeed.x, _sideSpeed.y),
                    startSize = Random.Range(_dropSize.x, _dropSize.y),
                    startLifetime = Random.Range(.25f, .55f)
                }, 1);
            }
        }

        private void OnDisable()
        {
            _sprayBudget = 0;
            if (_droplets != null) _droplets.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnDestroy() { if (_mesh != null) Destroy(_mesh); }
    }
}
