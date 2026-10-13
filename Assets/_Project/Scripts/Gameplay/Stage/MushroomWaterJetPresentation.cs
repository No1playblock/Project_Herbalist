using UnityEngine;

namespace Herbalist.Levels
{
    // Presentation only: the jump pad owns charge, rider movement and network state.
    public sealed class MushroomWaterJetPresentation : MonoBehaviour
    {
        [SerializeField] private LineRenderer _stream;
        [SerializeField] private ParticleSystem _sprayDroplets;

        private SapJumpPadSettings _settings;
        private Vector3[] _pathPoints;
        private float _flowTime;
        private float _sprayBudget;

        public void Initialize(SapJumpPadSettings settings)
        {
            _settings = settings;
            if (_stream == null || _settings == null) return;

            _pathPoints = new Vector3[Mathf.Max(6, _settings.waterStreamSegments) + 1];
            // Match the ability stream's LineRenderer setup. Its material is authored in the scene.
            _stream.useWorldSpace = true;
            _stream.enabled = true;
            _stream.positionCount = _pathPoints.Length;
            _stream.startWidth = 1f;
            _stream.endWidth = 1f;
            _stream.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
            _stream.widthMultiplier = _settings.waterStreamWidth;
            _stream.numCapVertices = 5;
            _stream.numCornerVertices = 4;
            _stream.colorGradient = new Gradient
            {
                colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            };
        }

        public void Present(Vector3 start, Vector3 end)
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
                _pathPoints[i] = Vector3.Lerp(start, end, t)
                    + side * (Mathf.Sin(phase) * sway * envelope)
                    + lift * (Mathf.Sin(phase * .73f + 1.2f) * sway * .34f * envelope);
            }
            _stream.SetPositions(_pathPoints);
            PresentDroplets(dt);
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
                side = Quaternion.AngleAxis(Random.Range(0f, 360f), tangent) * side.normalized;
                var particle = new ParticleSystem.EmitParams
                {
                    position = Vector3.Lerp(_pathPoints[index], _pathPoints[index + 1], pathPosition - index),
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
    }
}
