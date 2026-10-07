using UnityEngine;
namespace Herbalist.Abilities
{
    // Replace this presentation without changing extraction, collision or networking.
    [DefaultExecutionOrder(150)]
    public sealed class SapStreamPresentation : MonoBehaviour
    {
        [SerializeField] private SapDeposit sap;
        [SerializeField] private LineRenderer stream;
        [SerializeField] private Transform[] beads;
        [Header("Liquid ribbon")]
        [SerializeField, Range(6, 32)] private int _pathSegments = 22;
        [SerializeField, Range(6, 16)] private int _radialSegments = 12;
        [SerializeField, Min(0)] private float _pathSway = 0.018f;
        [SerializeField, Range(0, 0.2f)] private float _surfacePulse = 0.035f;
        [SerializeField, Range(0.1f, 1)] private float _coreWidth = 0.22f;
        [Header("Blob motion")]
        [SerializeField, Min(0)] private float _hoverBob = 0.022f;
        [SerializeField, Min(0)] private float _hoverPulse = 0.035f;
        [Header("Spray droplets")]
        [SerializeField] private ParticleSystem _sprayDroplets;
        [SerializeField, Min(0)] private float _sprayDropsPerMeterPerSecond = 1.8f;
        [SerializeField] private Vector2 _sprayDropSize = new Vector2(.035f, .085f);
        [SerializeField] private Vector2 _spraySideSpeed = new Vector2(.45f, 1.25f);
        [SerializeField, Min(0)] private float _sprayForwardSpeed = .6f;
        [SerializeField, Range(1, 12)] private int _sprayMaxDropsPerFrame = 6;
        [Header("Water impact")]
        [SerializeField] private Transform _impactCrown;
        [SerializeField, Range(0f, 1f)] private float _groundCrownNormalThreshold = 0.7f;
        [SerializeField, Range(0.1f, 1f)] private float _groundCrownHeightScale = 0.45f;
        [SerializeField, Min(0f)] private float _groundCrownInset = 0.015f;
        [Header("Release drips")]
        [SerializeField, Range(1, 8), Tooltip("Maximum simultaneous drops while the stream retracts.")] private int _releaseDripCount = 5;
        [SerializeField, Min(0.1f)] private float _releaseDripLifetime = 1.5f;
        [SerializeField, Min(0)] private float _releaseDripGravity = 9.81f;
        [SerializeField, Min(0)] private float _releaseDripForwardSpeed = 0.35f;
        [SerializeField, Min(0)] private float _releaseDripStartFallSpeed = 0.8f;

        [SerializeField, Min(0)] private float _releaseDripBlobRatio = 0.24f;
        [SerializeField, Min(0.02f), Tooltip("Time between drops from the retracting tip.")] private float _releaseDripStagger = 0.04f;
        [SerializeField, Min(0)] private float _releaseDripBounceSpeed = 0.18f;
        [SerializeField, Range(0, 3)] private int _releaseDripMaxBounces = 1;


        private float flowTime;
        
        private Vector3[] _pathPoints;
        private Transform _blobVisual;
        private MeshFilter _blobMeshFilter;
        private Vector3 _blobBaseLocalPosition;
        private Quaternion _blobBaseLocalRotation;
        private Transform[] _beadParents;
        private Vector3[] _beadLocalPositions;
        private Quaternion[] _beadLocalRotations;
        private Vector3[] _beadLocalScales;
        private bool[] _dropFalling;
        private float[] _dropDelayRemaining;
        private float[] _dropLifetimeRemaining;
        private Vector3[] _dropVelocities;
        private int[] _dropBounceCounts;
        private bool _wasRetracting;
        private float _retractDripTimer;
        private float _sprayBudget;
        private Vector3 _impactCrownBaseScale;


private void Awake()
        {
            if (_impactCrown != null) _impactCrownBaseScale = _impactCrown.localScale;
            InitializeStream();
            InitializeBlobMotion();
            InitializeDropPool();
        }

private void LateUpdate()
        {
            if (sap == null || stream == null) return;
            bool paused = Herbalist.GameUI.GameplayPause.IsPaused;
            if (!paused) flowTime += Time.deltaTime;

            var settings = sap.Settings;
            UpdateBlobMotion(settings);

            bool visible = sap.HasStream && (sap.State == SapState.Extracting || sap.State == SapState.Controlled);
            stream.enabled = visible && settings != null;
            UpdateImpactCrown(visible && sap.HoseStream && !sap.HoseRetracting && sap.HoseImpact);

            if (visible && settings != null)
            {
                Vector3 start = sap.StreamOrigin;
                Vector3 end = sap.StreamDestination;
                float streamDiameter = GetHoverBlobDiameter(settings);
                BuildPath(start, end);
                UpdateRetractionDrips(sap.HoseStream && sap.HoseRetracting && sap.HasStream,
                    _pathPoints[0], end, settings, paused ? 0f : Time.deltaTime);

                // One view-facing stream uses the hover blob's measured world-space diameter.
                stream.useWorldSpace = true;
                stream.positionCount = _pathPoints.Length;
                stream.SetPositions(_pathPoints);
                stream.widthMultiplier = streamDiameter * Mathf.Max(1f, _coreWidth);
                UpdateBeads(start, end, settings.streamFlowSpeed, settings.streamBeadSize, settings.radius);
                UpdateSprayDroplets(sap.HoseStream && !sap.HoseRetracting, paused ? 0f : Time.deltaTime);
            }
            else
            {
                UpdateSprayDroplets(false, 0f);
                _wasRetracting = false;
                _retractDripTimer = 0f;
                if (beads != null)
                {
                    for (int i = 0; i < beads.Length; i++)
                        if (beads[i] != null && (_dropFalling == null || !_dropFalling[i]))
                            beads[i].gameObject.SetActive(false);
                }
            }

            UpdateFallingDrips(settings, paused ? 0f : Time.deltaTime);
        }

private void InitializeStream()
        {
            if (stream == null) return;
            _pathPoints = new Vector3[Mathf.Max(4, _pathSegments) + 1];
            ConfigureCoreLine();
        }

private void ConfigureCoreLine()
        {
            stream.useWorldSpace = true;
            stream.startWidth = 1f;
            stream.endWidth = 1f;
            stream.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);
            stream.widthMultiplier = 1f;
            stream.numCapVertices = 5;
            stream.numCornerVertices = 4;
            // The WaterTest beam shader supplies its own colour and transparency.
            // Keep renderer vertex colours neutral so the old blue gradient does not tint it.
            stream.colorGradient = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            };
            // Colour and transparency come from the assigned water material.
        }

        private void BuildPath(Vector3 start, Vector3 end)
        {
            if (sap.HoseStream)
            {
                Vector3 velocity = sap.HoseLaunchVelocity;
                float duration = sap.HoseTravelTime;
                float startTime = Mathf.Clamp(sap.HoseStartTime, 0f, duration);
                float gravity = sap.Settings.hoseGravity;
                for (int i = 0; i < _pathPoints.Length; i++)
                {
                    float time = Mathf.Lerp(startTime, duration, (float)i / (_pathPoints.Length - 1));
                    _pathPoints[i] = start + velocity * time + Vector3.down * (0.5f * gravity * time * time);
                }
                _pathPoints[_pathPoints.Length - 1] = end;
                return;
            }
            Vector3 axis = end - start;
            float length = axis.magnitude;
            if (length < 0.0001f) axis = Vector3.forward;
            else axis /= length;

            Vector3 reference = Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > 0.94f ? Vector3.right : Vector3.up;
            Vector3 side = Vector3.Cross(reference, axis).normalized;
            Vector3 lift = Vector3.Cross(axis, side).normalized;
            float sway = _pathSway * Mathf.Clamp(length, 0.3f, 1.4f);

            for (int i = 0; i < _pathPoints.Length; i++)
            {
                float t = (float)i / (_pathPoints.Length - 1);
                float envelope = Mathf.Max(0f, Mathf.Sin(Mathf.PI * t));
                float phase = flowTime * 2.4f + t * 5.6f;
                _pathPoints[i] = Vector3.Lerp(start, end, t)
                    + side * (Mathf.Sin(phase) * sway * envelope)
                    + lift * (Mathf.Sin(phase * 0.73f + 1.2f) * sway * 0.34f * envelope);
            }
        }



private float GetHoverBlobDiameter(SapAbilitySettings settings)
        {
            if (_blobMeshFilter == null || _blobMeshFilter.sharedMesh == null)
                return settings.radius * 2f;

            Vector3 meshSize = _blobMeshFilter.sharedMesh.bounds.size;
            Vector3 parentScale = _blobVisual != null && _blobVisual.parent != null
                ? _blobVisual.parent.lossyScale
                : Vector3.one;
            float diameterX = meshSize.x * Mathf.Abs(settings.controlledScale.x * parentScale.x);
            float diameterY = meshSize.y * Mathf.Abs(settings.controlledScale.y * parentScale.y);
            float diameter = Mathf.Min(diameterX, diameterY);
            return diameter > 0.001f ? diameter : settings.radius * 2f;
        }


private void UpdateBeads(Vector3 start, Vector3 end, float speed, float size, float minimumLength)
        {
            if (beads == null || beads.Length == 0) return;
            float length = 0f;
            for (int i = 1; i < _pathPoints.Length; i++)
                length += Vector3.Distance(_pathPoints[i - 1], _pathPoints[i]);
            int activeCount = Mathf.Clamp(Mathf.CeilToInt(length * 1.6f), 1, beads.Length);
            float denominator = Mathf.Max(length, minimumLength);
            for (int i = 0; i < beads.Length; i++)
            {
                if (beads[i] == null || (_dropFalling != null && _dropFalling[i])) continue;
                bool active = i < activeCount;
                beads[i].gameObject.SetActive(active);
                if (!active) continue;
                float t = Mathf.Repeat(flowTime * speed / denominator + (float)i / activeCount, 1f);
                float pathPosition = t * (_pathPoints.Length - 1);
                int index = Mathf.Clamp(Mathf.FloorToInt(pathPosition), 0, _pathPoints.Length - 1);
                int nextIndex = Mathf.Min(index + 1, _pathPoints.Length - 1);
                float pathBlend = pathPosition - index;
                Vector3 direction = (_pathPoints[nextIndex] - _pathPoints[index]).normalized;
                beads[i].position = Vector3.Lerp(_pathPoints[index], _pathPoints[nextIndex], pathBlend);
                if (direction.sqrMagnitude > 0.0001f) beads[i].rotation = Quaternion.LookRotation(direction);
                float beadSize = size * 0.36f;
                float elongation = 1.12f;
                float pulse = 0.92f + 0.08f * Mathf.Sin(flowTime * 4f + i * 2.1f);
                beads[i].localScale = new Vector3(beadSize * pulse, beadSize * pulse, beadSize * elongation * pulse);
            }
        }

        private void UpdateImpactCrown(bool visible)
        {
            if (_impactCrown == null) return;
            if (_impactCrown.gameObject.activeSelf != visible) _impactCrown.gameObject.SetActive(visible);
            if (!visible) return;

            Vector3 normal = sap.HoseImpactNormal;
            if (normal.sqrMagnitude < 0.0001f) normal = Vector3.up;
            normal.Normalize();
            bool ground = normal.y >= _groundCrownNormalThreshold;
            _impactCrown.localScale = Vector3.Scale(_impactCrownBaseScale,
                new Vector3(1f, ground ? _groundCrownHeightScale : 1f, 1f));
            _impactCrown.SetPositionAndRotation(sap.StreamDestination - normal * (ground ? _groundCrownInset : 0f),
                Quaternion.FromToRotation(Vector3.up, normal));
        }

        private void UpdateSprayDroplets(bool spraying, float deltaTime)
        {
            if (_sprayDroplets == null) return;
            if (!spraying)
            {
                _sprayBudget = 0f;
                if (_sprayDroplets.isPlaying) _sprayDroplets.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                return;
            }
            if (!_sprayDroplets.isPlaying) _sprayDroplets.Play();
            if (deltaTime <= 0f) return;
            float length = 0f;
            for (int i = 1; i < _pathPoints.Length; i++)
                length += Vector3.Distance(_pathPoints[i - 1], _pathPoints[i]);
            _sprayBudget = Mathf.Min(_sprayMaxDropsPerFrame, _sprayBudget + length * _sprayDropsPerMeterPerSecond * deltaTime);
            int count = Mathf.Min(_sprayMaxDropsPerFrame, Mathf.FloorToInt(_sprayBudget));
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
                    velocity = tangent * _sprayForwardSpeed + side * Random.Range(_spraySideSpeed.x, _spraySideSpeed.y),
                    startSize = Random.Range(_sprayDropSize.x, _sprayDropSize.y)
                };
                _sprayDroplets.Emit(particle, 1);
            }
        }

private void InitializeBlobMotion()
        {
            if (sap == null) return;
            _blobVisual = sap.Visual;
            if (_blobVisual != null)
            {
                _blobMeshFilter = _blobVisual.GetComponent<MeshFilter>();
                _blobBaseLocalPosition = _blobVisual.localPosition;
                _blobBaseLocalRotation = _blobVisual.localRotation;
            }
        }

        private void UpdateBlobMotion(SapAbilitySettings settings)
        {
            if (sap == null || settings == null || _blobVisual == null) return;
            if (sap.State == SapState.Extracting || sap.State == SapState.Controlled)
            {
                _blobVisual.localPosition = _blobBaseLocalPosition + Vector3.up * (Mathf.Sin(flowTime * 2.5f) * _hoverBob);
                float pulse = 1f + Mathf.Sin(flowTime * 4f) * _hoverPulse;
                _blobVisual.localScale = settings.controlledScale * pulse;
                _blobVisual.localRotation = _blobBaseLocalRotation;
            }
            else
            {
                _blobVisual.localPosition = _blobBaseLocalPosition;
                _blobVisual.localRotation = _blobBaseLocalRotation;
            }
        }

private void OnDestroy()
        {
            if (beads == null || _dropFalling == null) return;
            for (int i = 0; i < beads.Length; i++)
            {
                if (!_dropFalling[i] || beads[i] == null) continue;
                if (Application.isPlaying) Destroy(beads[i].gameObject);
                else DestroyImmediate(beads[i].gameObject);
            }
        }
    

private void InitializeDropPool()
        {
            int count = beads != null ? beads.Length : 0;
            _beadParents = new Transform[count];
            _beadLocalPositions = new Vector3[count];
            _beadLocalRotations = new Quaternion[count];
            _beadLocalScales = new Vector3[count];
            _dropFalling = new bool[count];
            _dropDelayRemaining = new float[count];
            _dropLifetimeRemaining = new float[count];
            _dropVelocities = new Vector3[count];
            _dropBounceCounts = new int[count];

            for (int i = 0; i < count; i++)
            {
                if (beads[i] == null) continue;
                _beadParents[i] = beads[i].parent;
                _beadLocalPositions[i] = beads[i].localPosition;
                _beadLocalRotations[i] = beads[i].localRotation;
                _beadLocalScales[i] = beads[i].localScale;
            }
        }



        private void UpdateRetractionDrips(bool retracting, Vector3 start, Vector3 end, SapAbilitySettings settings, float deltaTime)
        {
            if (!retracting)
            {
                _wasRetracting = false;
                _retractDripTimer = 0f;
                return;
            }

            float interval = Mathf.Max(0.02f, _releaseDripStagger);
            if (!_wasRetracting)
            {
                SpawnRetractionDrip(start, end, settings);
                _retractDripTimer = interval;
            }
            else
            {
                _retractDripTimer -= deltaTime;
                if (_retractDripTimer <= 0f)
                {
                    SpawnRetractionDrip(start, end, settings);
                    _retractDripTimer = interval;
                }
            }
            _wasRetracting = true;
        }

        private void SpawnRetractionDrip(Vector3 start, Vector3 end, SapAbilitySettings settings)
        {
            if (settings == null || beads == null || _dropFalling == null) return;
            int activeCount = 0;
            for (int i = 0; i < beads.Length; i++)
                if (_dropFalling[i] && beads[i] != null) activeCount++;
            if (activeCount >= Mathf.Min(_releaseDripCount, beads.Length)) return;

            int index = -1;
            for (int i = 0; i < beads.Length; i++)
            {
                if (beads[i] != null && !_dropFalling[i]) { index = i; break; }
            }
            if (index < 0) return;

            float dropDiameter = Mathf.Max(settings.streamBeadSize * 1.25f, GetHoverBlobDiameter(settings) * _releaseDripBlobRatio);
            Transform bead = beads[index];
            bead.SetParent(null, true);
            bead.position = end;
            bead.rotation = Quaternion.identity;
            bead.localScale = Vector3.one * dropDiameter;
            bead.gameObject.SetActive(true);

            _dropFalling[index] = true;
            _dropDelayRemaining[index] = 0f;
            _dropLifetimeRemaining[index] = Mathf.Max(0.1f, _releaseDripLifetime);
            Vector3 streamDirection = (end - start).sqrMagnitude > 0.000001f ? (end - start).normalized : Vector3.forward;
            _dropVelocities[index] = streamDirection * _releaseDripForwardSpeed + Vector3.down * _releaseDripStartFallSpeed;
            _dropBounceCounts[index] = 0;
        }



        private void UpdateFallingDrips(SapAbilitySettings settings, float deltaTime)
        {
            if (beads == null || _dropFalling == null) return;
            if (deltaTime <= 0f) return;
            int collisionMask = settings != null ? settings.collisionMask : Physics.DefaultRaycastLayers;

            for (int i = 0; i < beads.Length; i++)
            {
                if (!_dropFalling[i] || beads[i] == null) continue;
                if (_dropDelayRemaining[i] > 0f)
                {
                    _dropDelayRemaining[i] -= deltaTime;
                    if (_dropDelayRemaining[i] > 0f) continue;
                    beads[i].gameObject.SetActive(true);
                }

                _dropLifetimeRemaining[i] -= deltaTime;
                if (_dropLifetimeRemaining[i] <= 0f)
                {
                    RecycleDrip(i);
                    continue;
                }

                Vector3 velocity = _dropVelocities[i];
                Vector3 displacement = velocity * deltaTime + Vector3.down * (0.5f * _releaseDripGravity * deltaTime * deltaTime);
                float distance = displacement.magnitude;
                float radius = Mathf.Max(0.006f, beads[i].lossyScale.x * 0.45f);
                if (distance > 0.0001f && Physics.SphereCast(beads[i].position, radius, displacement / distance,
                    out var hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    beads[i].position = hit.point + hit.normal * (radius + 0.003f);
                    if (_dropBounceCounts[i] >= _releaseDripMaxBounces || velocity.magnitude < 0.12f)
                    {
                        RecycleDrip(i);
                        continue;
                    }
                    _dropBounceCounts[i]++;
                    Vector3 reflected = Vector3.Reflect(velocity, hit.normal);
                    _dropVelocities[i] = Vector3.ProjectOnPlane(reflected, hit.normal) * 0.2f
                        + hit.normal * _releaseDripBounceSpeed;
                }
                else
                {
                    beads[i].position += displacement;
                    _dropVelocities[i] = velocity + Vector3.down * (_releaseDripGravity * deltaTime);
                }
            }
        }

private void RecycleDrip(int index)
        {
            Transform bead = beads[index];
            if (bead == null) { _dropFalling[index] = false; return; }
            Transform parent = _beadParents[index];
            if (parent == null)
            {
                if (Application.isPlaying) Destroy(bead.gameObject);
                else DestroyImmediate(bead.gameObject);
                _dropFalling[index] = false;
                return;
            }
            bead.SetParent(parent, false);
            bead.localPosition = _beadLocalPositions[index];
            bead.localRotation = _beadLocalRotations[index];
            bead.localScale = _beadLocalScales[index];
            bead.gameObject.SetActive(false);
            _dropFalling[index] = false;
            _dropDelayRemaining[index] = 0f;
            _dropLifetimeRemaining[index] = 0f;
            _dropVelocities[index] = Vector3.zero;
            _dropBounceCounts[index] = 0;
        }


private void OnDisable()
        {
            if (_sprayDroplets != null) _sprayDroplets.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _wasRetracting = false;
            _retractDripTimer = 0f;
            if (_dropFalling == null) return;
            for (int i = 0; i < _dropFalling.Length; i++)
                if (_dropFalling[i] && beads != null && i < beads.Length && beads[i] != null)
                    RecycleDrip(i);
        }
}
}
