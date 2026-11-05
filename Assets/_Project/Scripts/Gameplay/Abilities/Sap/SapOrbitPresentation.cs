using Herbalist.Player;
using UnityEngine;

namespace Herbalist.Abilities
{
    // Authored on player prefabs; only the deforming mesh is allocated at runtime.
    [DefaultExecutionOrder(160)]
    [DisallowMultipleComponent]
    public sealed class SapOrbitPresentation : MonoBehaviour
    {
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField, Range(32, 192)] private int _segments = 96;
        [SerializeField, Range(6, 16)] private int _sides = 10;
        [SerializeField, Min(.1f)] private float _patternReferenceLength = 5f;
        [SerializeField, Range(.05f, .4f)] private float _outletBlendFraction = .2f;
        private PlayerAbilityController _abilities;
        private PlayerController _player;
        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector2[] _uvs;
        private float _unwind;
        private float _time;

        private void Awake()
        {
            _abilities = GetComponent<PlayerAbilityController>();
            _player = GetComponent<PlayerController>();
            if (_filter == null || _renderer == null) return;
            _vertices = new Vector3[(_segments + 1) * (_sides + 1)];
            _normals = new Vector3[_vertices.Length];
            _uvs = new Vector2[_vertices.Length];
            var triangles = new int[_segments * _sides * 6];
            for (int i = 0; i < _segments; i++)
                for (int j = 0; j < _sides; j++)
                {
                    int a = i * (_sides + 1) + j, b = a + _sides + 1;
                    int k = (i * _sides + j) * 6;
                    triangles[k] = a; triangles[k + 1] = a + 1; triangles[k + 2] = b;
                    triangles[k + 3] = a + 1; triangles[k + 4] = b + 1; triangles[k + 5] = b;
                }
            _mesh = new Mesh { name = "Water orbit (instance)" };
            _mesh.MarkDynamic();
            _mesh.vertices = _vertices;
            _mesh.triangles = triangles;
            _filter.sharedMesh = _mesh;
        }

        private void LateUpdate()
        {
            UpdateOrbit(Herbalist.GameUI.GameplayPause.IsPaused ? 0f : Time.deltaTime);
        }

        private void UpdateOrbit(float dt)
        {
            if (_renderer == null || _mesh == null || _abilities == null || _player == null) return;
            var sap = _abilities.Sap;
            var settings = sap != null ? sap.Settings : null;
            bool visible = settings != null && settings.orbitEnabled && _abilities.Unlocked &&
                _abilities.Kind == PlayerAbilityKind.Sap;
            _renderer.enabled = visible;
            if (!visible) { _unwind = 0f; return; }
            _time += dt * settings.orbitFlowSpeed;
            // Consume the arc at a constant rate, matching restoration when durations match.
            if (sap.Spraying) _unwind = Mathf.MoveTowards(_unwind, 1f, dt *
                settings.orbitUnwindAngle / (360f * Mathf.Max(.01f, settings.orbitUnwindDuration)));
            else _unwind = Mathf.MoveTowards(_unwind, 0f, dt / Mathf.Max(.01f, settings.orbitRestoreDuration));
            Vector3 center = sap.OrbitCenter;
            Ray aim = _player.View.GetAimRay(_player.View.Yaw, _player.View.Pitch);
            Vector3 outlet = sap.Held != null ? sap.Held.transform.position : sap.HoverPosition(aim);
            Vector3 radial = Vector3.ProjectOnPlane(outlet - center, Vector3.up).normalized;
            if (radial.sqrMagnitude < .001f) radial = Vector3.forward;
            float endAngle = Mathf.Atan2(radial.z, radial.x);
            float gap = Mathf.PI * 2f * _unwind;
            float remainingAngle = Mathf.PI * 2f - gap;
            float blendStart = 1f - _outletBlendFraction;
            float joinAngle = endAngle + gap + blendStart * remainingAngle;
            Vector3 joinRadial = new Vector3(Mathf.Cos(joinAngle), 0f, Mathf.Sin(joinAngle));
            Vector3 join = center + joinRadial * settings.orbitRadius;
            float handle = settings.orbitRadius * remainingAngle * _outletBlendFraction / 3f;
            Vector3 emission = sap.Held != null && sap.Held.HoseLaunchVelocity.sqrMagnitude > .001f
                ? sap.Held.HoseLaunchVelocity.normalized : radial;
            Vector3 controlA = join + new Vector3(-joinRadial.z, 0f, joinRadial.x) * handle;
            Vector3 controlB = outlet - emission * handle;
            float bend = Mathf.Clamp01(_unwind * 4f);
            Transform meshTransform = _filter.transform;
            for (int i = 0; i <= _segments; i++)
            {
                float t = i / (float)_segments;
                float angle = endAngle + gap + t * (Mathf.PI * 2f - gap);
                var outward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                // Fade waves at the outlet so the ring and authoritative stream meet exactly.
                float envelope = Mathf.Sin(t * Mathf.PI);
                float wave = settings.orbitRipple * Mathf.Sin(angle * 3f - _time) * envelope;
                Vector3 point = center + outward * (settings.orbitRadius + wave) + Vector3.up * wave;
                Vector3 tangent = new Vector3(-outward.z, 0f, outward.x);
                if (t > blendStart && bend > 0f)
                {
                    float u = (t - blendStart) / _outletBlendFraction;
                    float v = 1f - u;
                    Vector3 curve = v * v * v * join + 3f * v * v * u * controlA +
                        3f * v * u * u * controlB + u * u * u * outlet;
                    Vector3 derivative = 3f * v * v * (controlA - join) +
                        6f * v * u * (controlB - controlA) + 3f * u * u * (outlet - controlB);
                    point = Vector3.Lerp(point, curve, bend);
                    tangent = Vector3.Lerp(tangent, derivative.normalized, bend).normalized;
                }
                Vector3 side = Vector3.ProjectOnPlane(outward, tangent).normalized;
                if (side.sqrMagnitude < .001f) side = Vector3.Cross(tangent, Vector3.up).normalized;
                Vector3 up = Vector3.Cross(tangent, side).normalized;
                float radius = settings.orbitTubeRadius * (1f + .12f * Mathf.Sin(angle * 6f - _time) * envelope);
                radius *= Mathf.Lerp(1f, Mathf.SmoothStep(.05f, 1f, t / .12f), Mathf.Clamp01(_unwind));
                for (int j = 0; j <= _sides; j++)
                {
                    float cross = j * Mathf.PI * 2f / _sides;
                    Vector3 normal = side * Mathf.Cos(cross) + up * Mathf.Sin(cross);
                    int index = i * (_sides + 1) + j;
                    _vertices[index] = meshTransform.InverseTransformPoint(point + normal * radius);
                    _normals[index] = meshTransform.InverseTransformDirection(normal).normalized;
                    _uvs[index] = new Vector2(t * settings.orbitRadius * (Mathf.PI * 2f - gap) /
                        Mathf.Max(.1f, _patternReferenceLength), j / (float)_sides);
                }
            }
            _mesh.vertices = _vertices; _mesh.normals = _normals; _mesh.uv = _uvs;
            _mesh.RecalculateBounds();
        }

        private void OnDisable() { if (_renderer != null) _renderer.enabled = false; }
        private void OnDestroy()
        {
            if (_mesh == null) return;
            if (Application.isPlaying) Destroy(_mesh); else DestroyImmediate(_mesh);
        }
    }
}
