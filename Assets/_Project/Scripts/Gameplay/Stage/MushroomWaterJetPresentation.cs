using UnityEngine;

namespace Herbalist.Levels
{
    // Presentation only: the jump pad owns charge, rider movement and network state.
    public sealed class MushroomWaterJetPresentation : MonoBehaviour
    {
        [SerializeField] private LineRenderer _stream;
        [SerializeField] private ParticleSystem _sprayDroplets;
        [SerializeField] private ParticleSystem _sprayTemplate;

        private SapJumpPadSettings _settings;
        private Vector3[] _pathPoints;
        private float _flowTime;
        private float _sprayBudget;
        private float _fanSpread;
        private const int ConeSides = 12;
        private Mesh _cone;
        private Vector3[] _coneVertices;
        private float _coneMouth;

        public void Initialize(SapJumpPadSettings settings, float width = 0f, float fanWidth = 0f, bool conical = false)
        {
            _settings = settings;
            if (_stream == null || _settings == null) return;
            if (_sprayDroplets == null && _sprayTemplate != null)
            {
                _sprayDroplets = Instantiate(_sprayTemplate, transform);
                var main = _sprayDroplets.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
            }

            _pathPoints = new Vector3[Mathf.Max(6, _settings.waterStreamSegments) + 1];
            // Match the ability stream's LineRenderer setup. Its material is authored in the scene.
            _stream.useWorldSpace = true;
            _stream.enabled = true;
            _stream.positionCount = _pathPoints.Length;
            _stream.startWidth = 1f;
            _stream.endWidth = 1f;
            float mouthWidth = width > 0f ? width : _settings.waterStreamWidth;
            _stream.widthMultiplier = Mathf.Max(mouthWidth, fanWidth);
            _stream.widthCurve = AnimationCurve.Linear(0f, mouthWidth / _stream.widthMultiplier, 1f, 1f);
            _fanSpread = _stream.widthMultiplier - mouthWidth;
            _stream.numCapVertices = _stream.alignment == LineAlignment.TransformZ ? 0 : 5;
            _stream.numCornerVertices = 4;
            _stream.colorGradient = new Gradient
            {
                colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            };
            if (conical) InitializeCone(mouthWidth);
        }

        internal static Vector3 PathPoint(Vector3 start, Vector3 end, float t, float rise, float drop)
            => Vector3.Lerp(start, end, t) + Vector3.up * (rise * t * (1f - t) - drop * t * t);

        public void Present(Vector3 start, Vector3 end, float rise = 0f, float drop = 0f)
        {
            if (_stream == null || _settings == null) return;
            if (_pathPoints == null) Initialize(_settings);
            float dt = Herbalist.GameUI.GameplayPause.IsPaused ? 0f : Time.deltaTime;
            _flowTime += dt;

            Vector3 axis = end - start;
            float length = axis.magnitude;
            if (length < 0.0001f) axis = Vector3.up;
            else axis /= length;
            Vector3 reference = Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > .94f ? Vector3.right : Vector3.up;
            Vector3 side = Vector3.Cross(reference, axis).normalized;
            Vector3 lift = Vector3.Cross(axis, side).normalized;
            float sway = _settings.waterStreamSway * Mathf.Clamp(length, .3f, 1.4f);
            for (int i = 0; i < _pathPoints.Length; i++)
            {
                float t = (float)i / (_pathPoints.Length - 1);
                float envelope = Mathf.Max(0f, Mathf.Sin(Mathf.PI * t));
                float phase = _flowTime * 2.4f + t * 5.6f;
                _pathPoints[i] = PathPoint(start, end, t, rise, drop)
                    + side * (Mathf.Sin(phase) * sway * envelope)
                    + lift * (Mathf.Sin(phase * .73f + 1.2f) * sway * .34f * envelope);
            }
            if (_cone == null) _stream.SetPositions(_pathPoints);
            else PresentCone(side, lift);
            PresentDroplets(dt);
        }

        private void InitializeCone(float mouthWidth)
        {
            _coneMouth = mouthWidth;
            _stream.enabled = false;
            if (_cone != null) return;
            _cone = new Mesh { name = "Conical water spray" };
            _cone.MarkDynamic();
            _coneVertices = new Vector3[_pathPoints.Length * (ConeSides + 1)];
            var uv = new Vector2[_coneVertices.Length];
            var triangles = new int[(_pathPoints.Length - 1) * ConeSides * 6];
            int index = 0;
            for (int ring = 0; ring < _pathPoints.Length; ring++)
            for (int side = 0; side <= ConeSides; side++)
            {
                int a = ring * (ConeSides + 1) + side;
                uv[a] = new Vector2((float)ring / (_pathPoints.Length - 1), (float)side / ConeSides);
                if (ring == _pathPoints.Length - 1 || side == ConeSides) continue;
                int b = a + ConeSides + 1;
                triangles[index++] = a; triangles[index++] = a + 1; triangles[index++] = b;
                triangles[index++] = a + 1; triangles[index++] = b + 1; triangles[index++] = b;
            }
            _cone.vertices = _coneVertices;
            _cone.uv = uv;
            _cone.triangles = triangles;
            var spray = new GameObject("Conical Water Mesh", typeof(MeshFilter), typeof(MeshRenderer));
            spray.layer = gameObject.layer;
            spray.transform.SetParent(transform, false);
            spray.GetComponent<MeshFilter>().sharedMesh = _cone;
            var renderer = spray.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = _stream.sharedMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void PresentCone(Vector3 side, Vector3 lift)
        {
            var worldToLocal = transform.worldToLocalMatrix;
            for (int ring = 0; ring < _pathPoints.Length; ring++)
            {
                float radius = (_coneMouth + _fanSpread * ring / (_pathPoints.Length - 1)) * .5f;
                for (int edge = 0; edge <= ConeSides; edge++)
                {
                    float angle = edge * (2f * Mathf.PI / ConeSides);
                    var point = _pathPoints[ring] + radius * (Mathf.Cos(angle) * side + Mathf.Sin(angle) * lift);
                    _coneVertices[ring * (ConeSides + 1) + edge] = worldToLocal.MultiplyPoint3x4(point);
                }
            }
            _cone.vertices = _coneVertices;
            _cone.RecalculateNormals();
            _cone.RecalculateBounds();
        }

        private void PresentDroplets(float dt)
        {
            if (_sprayDroplets == null || dt <= 0f) return;
            if (!_sprayDroplets.isPlaying) _sprayDroplets.Play();
            float length = 0f;
            for (int i = 1; i < _pathPoints.Length; i++)
                length += Vector3.Distance(_pathPoints[i - 1], _pathPoints[i]);
            _sprayBudget = Mathf.Min(_settings.waterSprayMaxDropsPerFrame,
                _sprayBudget + length * _settings.waterSprayDropsPerMeterPerSecond * dt);
            int count = Mathf.Min(_settings.waterSprayMaxDropsPerFrame, Mathf.FloorToInt(_sprayBudget));
            _sprayBudget -= count;
            for (int i = 0; i < count; i++)
            {
                float pathPosition = Random.Range(.08f, .92f) * (_pathPoints.Length - 1);
                int index = Mathf.Min(Mathf.FloorToInt(pathPosition), _pathPoints.Length - 2);
                Vector3 tangent = (_pathPoints[index + 1] - _pathPoints[index]).normalized;
                if (tangent.sqrMagnitude < .0001f) continue;
                Vector3 side = Vector3.Cross(tangent, Vector3.up);
                if (side.sqrMagnitude < .0001f) side = Vector3.Cross(tangent, Vector3.right);
                side.Normalize();
                Vector3 fanDirection = _cone != null ? Quaternion.AngleAxis(Random.Range(0f, 360f), tangent) * side : side;
                float lateral = Random.Range(-1f, 1f) * _fanSpread * .5f;
                Vector3 fanOffset = fanDirection * lateral * (pathPosition / (_pathPoints.Length - 1));
                side = Quaternion.AngleAxis(Random.Range(0f, 360f), tangent) * side.normalized;
                var particle = new ParticleSystem.EmitParams
                {
                    position = Vector3.Lerp(_pathPoints[index], _pathPoints[index + 1], pathPosition - index) + fanOffset,
                    velocity = tangent * _settings.waterSprayForwardSpeed +
                        side * Random.Range(_settings.waterSpraySideSpeed.x, _settings.waterSpraySideSpeed.y),
                    startSize = Random.Range(_settings.waterSprayDropSize.x, _settings.waterSprayDropSize.y)
                };
                _sprayDroplets.Emit(particle, 1);
            }
        }

        private void OnDisable()
        {
            _sprayBudget = 0f;
            if (_sprayDroplets != null)
                _sprayDroplets.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnDestroy()
        {
            if (_cone != null) Destroy(_cone);
        }
    }
}
