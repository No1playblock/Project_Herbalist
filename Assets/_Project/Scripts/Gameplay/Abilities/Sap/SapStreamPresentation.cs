using UnityEngine;
namespace Herbalist.Abilities
{
    // Replace this presentation without changing extraction, collision or networking.
    [DefaultExecutionOrder(150)]
    public sealed class SapStreamPresentation : MonoBehaviour
    {
        [SerializeField] private SapDeposit sap;
        [SerializeField] private LineRenderer stream;
        [SerializeField] private MeshFilter _tubeFilter;
        [SerializeField] private MeshRenderer _tubeRenderer;
        [SerializeField] private Transform[] beads;
        [Header("Water stream tube")]
        [SerializeField, Range(6, 32)] private int _pathSegments = 22;
        [SerializeField, Range(6, 16)] private int _radialSegments = 12;
        [SerializeField, Min(0.1f), Tooltip("Path length in meters that receives one full set of the material's lengthwise pattern tiles.")]
        private float _tubePatternReferenceLength = 5f;
        [SerializeField, Min(0)] private float _pathSway = 0.018f;
        [SerializeField, Range(0, 0.2f)] private float _surfacePulse = 0.035f;
        [SerializeField, Range(0f, 0.1f)] private float _hoseSurfacePulse = 0.035f;
        [SerializeField, Range(0f, 0.1f)] private float _hoseSurfaceDetail = 0.015f;
        [SerializeField, Range(1f, 20f)] private float _hoseSurfaceWaves = 5.5f;
        [SerializeField, Range(1f, 20f)] private float _hoseSurfaceDetailWaves = 11f;
        [SerializeField, Min(0f)] private float _hoseSurfaceSpeed = 6f;
        [SerializeField, Range(0.1f, 1)] private float _coreWidth = 0.22f;
        [SerializeField, Range(0.02f, 1f)] private float _assetTrailWidth = 0.18f;
        [Header("Namu water column")]
        [SerializeField] private ParticleSystem _assetBeamParticles;
        [SerializeField, Min(0f)] private float _assetBeamSampleTime = 0.63f;
        [SerializeField, Range(1, 16)] private int _assetBeamSegments = 8;
        [SerializeField, Range(1f, 2f)] private float _assetBeamOverlap = 1.25f;
        [Header("Namu water ball")]
        [SerializeField] private ParticleSystem _assetBallParticles;
        [SerializeField] private ParticleSystem _assetBallCoreParticles;
        [SerializeField, Min(0f)] private float _assetBallSampleTime = 1.21f;
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
        [Header("Impact splash droplets")]
        [SerializeField, Min(0f)] private float _impactSplashDropsPerSecond = 36f;
        [SerializeField] private Vector2 _impactSplashDropSize = new Vector2(0.07f, 0.16f);
        [SerializeField] private Vector2 _impactSplashSideSpeed = new Vector2(1.2f, 3f);
        [SerializeField] private Vector2 _impactSplashNormalSpeed = new Vector2(1f, 2.5f);
        [Header("Water impact")]
        [SerializeField] private Transform _impactCrown;
        [SerializeField] private Transform _wallCrownVisual;
        [SerializeField] private Transform _groundCrownVisual;
        [SerializeField] private Transform _groundCrownRim;
        [SerializeField] private ParticleSystem _assetImpactParticles;
        [SerializeField, Range(0f, 1f)] private float _groundCrownNormalThreshold = 0.7f;
        [SerializeField, Range(0.1f, 4f)] private float _groundCrownHeightScale = 2.9f;
        [SerializeField, Range(0.5f, 2f)] private float _groundCrownWidthScale = 1.32f;
        [SerializeField, Min(0f)] private float _groundCrownInset;
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
        private Mesh _tubeMesh;
        private Vector3[] _tubeVertices;
        private Vector3[] _tubeNormals;
        private Vector2[] _tubeUvs;
        private int[] _tubeTriangles;
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
        private float _impactSplashBudget;
        private Vector3 _impactCrownBaseScale;
        private Vector3 _assetImpactBaseScale;
        private bool _wasImpactVisible;
        private ParticleSystem.Particle _assetBeamSample;
        private Vector3 _assetBeamMeshSize = Vector3.one;
        private ParticleSystem.Particle[] _assetBeamInstances;
        private System.Collections.Generic.List<Vector4> _assetBeamCustom1;
        private System.Collections.Generic.List<Vector4> _assetBeamCustom2;
        private bool _assetBeamReady;
        private bool _assetBallPrepared;
        private readonly ParticleSystem.Particle[] _assetBallCoreSample = new ParticleSystem.Particle[1];


private void Awake()
        {
            if (_impactCrown != null) _impactCrownBaseScale = _impactCrown.localScale;
            if (_assetImpactParticles != null) _assetImpactBaseScale = _assetImpactParticles.transform.localScale;
            InitializeStream();
            InitializeAssetBeam();
            PrepareAssetBall();
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
            bool showStream = visible && settings != null;
            bool hasTube = _tubeRenderer != null && _tubeFilter != null;
            bool showTube = showStream && sap.HoseStream && hasTube;
            bool showAssetBeam = showStream && sap.HoseStream && _assetBeamReady && !hasTube;
            stream.enabled = showStream && !showTube && !showAssetBeam;
            if (_tubeRenderer != null) _tubeRenderer.enabled = showTube;
            if (_assetBeamParticles != null && !showAssetBeam && _assetBeamParticles.particleCount > 0)
                _assetBeamParticles.Clear();
            if (_assetBallParticles != null && !_assetBallParticles.gameObject.activeInHierarchy)
                _assetBallPrepared = false;
            else if (!_assetBallPrepared) PrepareAssetBall();
            UpdateImpactCrown(visible && sap.HoseStream && !sap.HoseRetracting && sap.HoseImpact);

            if (visible && settings != null)
            {
                Vector3 start = sap.StreamOrigin;
                Vector3 end = sap.StreamDestination;
                float streamDiameter = GetHoverBlobDiameter(settings);
                BuildPath(start, end);
                if (showAssetBeam) UpdateAssetBeam(settings);
                UpdateRetractionDrips(sap.HoseStream && sap.HoseRetracting && sap.HasStream,
                    _pathPoints[0], end, settings, paused ? 0f : Time.deltaTime);

                stream.useWorldSpace = true;
                stream.positionCount = _pathPoints.Length;
                stream.SetPositions(_pathPoints);
                stream.widthMultiplier = streamDiameter * (showTube ? _assetTrailWidth : Mathf.Max(1f, _coreWidth));
                if (showTube) UpdateTube(streamDiameter);
                UpdateBeads(start, end, settings.streamFlowSpeed, settings.streamBeadSize, settings.radius);
                UpdateSprayDroplets(sap.HoseStream && !sap.HoseRetracting, paused ? 0f : Time.deltaTime);
                UpdateImpactSplashDroplets(sap.HoseStream && !sap.HoseRetracting && sap.HoseImpact,
                    paused ? 0f : Time.deltaTime, streamDiameter);
            }
            else
            {
                UpdateSprayDroplets(false, 0f);
                _impactSplashBudget = 0f;
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
            InitializeTube();
        }

        private void InitializeTube()
        {
            if (_tubeFilter == null || _tubeRenderer == null) return;
            int sides = Mathf.Max(6, _radialSegments);
            int ringSize = sides + 1;
            int vertexCount = _pathPoints.Length * ringSize;
            _tubeVertices = new Vector3[vertexCount];
            _tubeNormals = new Vector3[vertexCount];
            _tubeUvs = new Vector2[vertexCount];
            _tubeTriangles = new int[(_pathPoints.Length - 1) * sides * 6];
            for (int ring = 0; ring < _pathPoints.Length; ring++)
                for (int side = 0; side <= sides; side++)
                    _tubeUvs[ring * ringSize + side] = new Vector2(0f, side / (float)sides);
            for (int ring = 0; ring < _pathPoints.Length - 1; ring++)
                for (int side = 0; side < sides; side++)
                {
                    int first = ring * ringSize + side;
                    int next = first + ringSize;
                    int index = (ring * sides + side) * 6;
                    _tubeTriangles[index] = first;
                    _tubeTriangles[index + 1] = first + 1;
                    _tubeTriangles[index + 2] = next;
                    _tubeTriangles[index + 3] = first + 1;
                    _tubeTriangles[index + 4] = next + 1;
                    _tubeTriangles[index + 5] = next;
                }
            _tubeMesh = new Mesh { name = "Sap Stream Tube" };
            _tubeMesh.MarkDynamic();
            _tubeMesh.vertices = _tubeVertices;
            _tubeMesh.uv = _tubeUvs;
            _tubeMesh.triangles = _tubeTriangles;
            _tubeFilter.sharedMesh = _tubeMesh;
            _tubeRenderer.enabled = false;
            stream.enabled = false;
        }

        private void InitializeAssetBeam()
        {
            if (_assetBeamParticles == null || _pathPoints == null) return;
            _assetBeamParticles.Simulate(_assetBeamSampleTime, false, true, true);
            var sample = new ParticleSystem.Particle[1];
            if (_assetBeamParticles.GetParticles(sample) == 0) return;
            _assetBeamSample = sample[0];
            var custom1 = new System.Collections.Generic.List<Vector4>(1);
            var custom2 = new System.Collections.Generic.List<Vector4>(1);
            _assetBeamParticles.GetCustomParticleData(custom1, ParticleSystemCustomData.Custom1);
            _assetBeamParticles.GetCustomParticleData(custom2, ParticleSystemCustomData.Custom2);
            _assetBeamSample.startColor = sample[0].GetCurrentColor(_assetBeamParticles);
            var size = sample[0].GetCurrentSize3D(_assetBeamParticles);
            _assetBeamSample.startSize3D = size;
            var beamRenderer = _assetBeamParticles.GetComponent<ParticleSystemRenderer>();
            if (beamRenderer != null && beamRenderer.mesh != null)
                _assetBeamMeshSize = beamRenderer.mesh.bounds.size;
            _assetBeamParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            var emission = _assetBeamParticles.emission;
            emission.enabled = false;
            var sizeOverLifetime = _assetBeamParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = false;
            var colorOverLifetime = _assetBeamParticles.colorOverLifetime;
            colorOverLifetime.enabled = false;
            var main = _assetBeamParticles.main;
            main.playOnAwake = false;
            main.simulationSpeed = 0f;
            // Keep the system alive while manually replacing its frozen column particles.
            _assetBeamParticles.Play(false);
            _assetBeamParticles.Pause(false);
            _assetBeamInstances = new ParticleSystem.Particle[Mathf.Clamp(_assetBeamSegments, 1, 16)];
            _assetBeamCustom1 = new System.Collections.Generic.List<Vector4>(_assetBeamInstances.Length);
            _assetBeamCustom2 = new System.Collections.Generic.List<Vector4>(_assetBeamInstances.Length);
            for (int i = 0; i < _assetBeamInstances.Length; i++)
            {
                _assetBeamCustom1.Add(custom1.Count > 0 ? custom1[0] : Vector4.zero);
                _assetBeamCustom2.Add(custom2.Count > 0 ? custom2[0] : Vector4.zero);
            }
            _assetBeamReady = true;
        }

        private void PrepareAssetBall()
        {
            if (_assetBallPrepared || _assetBallParticles == null || !_assetBallParticles.gameObject.activeInHierarchy)
                return;
            _assetBallParticles.Simulate(_assetBallSampleTime, true, true, true);
            _assetBallParticles.Pause(true);
            _assetBallPrepared = true;
        }

        private Vector3 SamplePath(float t)
        {
            float index = Mathf.Clamp01(t) * (_pathPoints.Length - 1);
            int before = Mathf.Min(Mathf.FloorToInt(index), _pathPoints.Length - 2);
            return Vector3.Lerp(_pathPoints[before], _pathPoints[before + 1], index - before);
        }

        private void UpdateAssetBeam(SapAbilitySettings settings)
        {
            int count = _assetBeamInstances.Length;
            Transform particleTransform = _assetBeamParticles.transform;
            float diameter = GetHoverBlobDiameter(settings);
            Vector3 beamScale = particleTransform.lossyScale;
            for (int i = 0; i < count; i++)
            {
                Vector3 from = SamplePath(i / (float)count);
                Vector3 to = SamplePath((i + 1f) / count);
                Vector3 delta = to - from;
                var particle = _assetBeamSample;
                particle.startLifetime = 60f;
                particle.remainingLifetime = 60f;
                particle.position = particleTransform.InverseTransformPoint((from + to) * 0.5f);
                particle.rotation3D = (Quaternion.Inverse(particleTransform.rotation) *
                    Quaternion.FromToRotation(Vector3.up, delta.normalized)).eulerAngles;
                particle.startSize3D = new Vector3(
                    diameter / Mathf.Max(0.0001f, Mathf.Abs(beamScale.x * _assetBeamMeshSize.x)),
                    delta.magnitude * _assetBeamOverlap / Mathf.Max(0.0001f, Mathf.Abs(beamScale.y * _assetBeamMeshSize.y)),
                    diameter / Mathf.Max(0.0001f, Mathf.Abs(beamScale.z * _assetBeamMeshSize.z)));
                _assetBeamInstances[i] = particle;
            }
            _assetBeamParticles.SetParticles(_assetBeamInstances, count);
            _assetBeamParticles.SetCustomParticleData(_assetBeamCustom1, ParticleSystemCustomData.Custom1);
            _assetBeamParticles.SetCustomParticleData(_assetBeamCustom2, ParticleSystemCustomData.Custom2);
        }

        private void UpdateTube(float diameter)
        {
            if (_tubeMesh == null) return;
            int sides = Mathf.Max(6, _radialSegments);
            int ringSize = sides + 1;
            Vector3 previousSide = Vector3.zero;
            Transform tubeTransform = _tubeFilter.transform;
            float distanceAlongPath = 0f;
            float inversePatternLength = 1f / Mathf.Max(0.1f, _tubePatternReferenceLength);
            for (int ring = 0; ring < _pathPoints.Length; ring++)
            {
                if (ring > 0)
                    distanceAlongPath += Vector3.Distance(_pathPoints[ring - 1], _pathPoints[ring]);
                float patternPosition = distanceAlongPath * inversePatternLength;
                int before = Mathf.Max(0, ring - 1);
                int after = Mathf.Min(_pathPoints.Length - 1, ring + 1);
                Vector3 tangent = (_pathPoints[after] - _pathPoints[before]).normalized;
                if (tangent.sqrMagnitude < .0001f) tangent = Vector3.forward;
                Vector3 sideAxis = ring == 0 ? Vector3.Cross(tangent, Vector3.up) : Vector3.ProjectOnPlane(previousSide, tangent);
                if (sideAxis.sqrMagnitude < .0001f) sideAxis = Vector3.Cross(tangent, Vector3.right);
                sideAxis.Normalize();
                Vector3 upAxis = Vector3.Cross(tangent, sideAxis).normalized;
                previousSide = sideAxis;
                float t = ring / (float)(_pathPoints.Length - 1);
                float taper = sap.HoseStream ? 1f : Mathf.Lerp(.82f, 1f, Mathf.SmoothStep(0f, 1f, t / .12f)) *
                    Mathf.Lerp(1f, .28f, Mathf.SmoothStep(0f, 1f, (t - .82f) / .18f));
                for (int segment = 0; segment <= sides; segment++)
                {
                    float angle = segment * Mathf.PI * 2f / sides;
                    Vector3 radial = sideAxis * Mathf.Cos(angle) + upAxis * Mathf.Sin(angle);
                    float pulse = sap.HoseStream
                        ? 1f + _hoseSurfacePulse * Mathf.Sin(patternPosition * Mathf.PI * 2f * _hoseSurfaceWaves
                            - flowTime * _hoseSurfaceSpeed + angle * 3f)
                            + _hoseSurfaceDetail * Mathf.Sin(patternPosition * Mathf.PI * 2f * _hoseSurfaceDetailWaves
                                + flowTime * _hoseSurfaceSpeed * 0.67f + angle * 5f)
                        : 1f + _surfacePulse * Mathf.Sin(t * 24f - flowTime * 7f + angle * 3f);
                    int index = ring * ringSize + segment;
                    Vector3 worldPoint = _pathPoints[ring] + radial * (diameter * .5f * taper * pulse);
                    _tubeVertices[index] = tubeTransform.InverseTransformPoint(worldPoint);
                    _tubeNormals[index] = tubeTransform.InverseTransformDirection(radial).normalized;
                    _tubeUvs[index].x = patternPosition;
                }
            }
            _tubeMesh.vertices = _tubeVertices;
            _tubeMesh.normals = _tubeNormals;
            _tubeMesh.uv = _tubeUvs;
            _tubeMesh.RecalculateBounds();
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
            if (_assetBallCoreParticles != null && _assetBallCoreParticles.GetParticles(_assetBallCoreSample) > 0)
            {
                Vector3 size = _assetBallCoreSample[0].GetCurrentSize3D(_assetBallCoreParticles);
                // The sampled particle size matches the visible sphere; multiplying by its transform scale overstates it.
                float assetDiameter = Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.y));
                if (assetDiameter > 0.001f) return assetDiameter;
            }
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
            bool useAssetImpact = _assetImpactParticles != null;
            if (_impactCrown.gameObject.activeSelf != (visible && !useAssetImpact))
                _impactCrown.gameObject.SetActive(visible && !useAssetImpact);
            if (!visible)
            {
                if (_wasImpactVisible && useAssetImpact)
                    _assetImpactParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _wasImpactVisible = false;
                return;
            }

            Vector3 normal = sap.HoseImpactNormal;
            if (normal.sqrMagnitude < 0.0001f) normal = Vector3.up;
            normal.Normalize();
            bool ground = normal.y >= _groundCrownNormalThreshold;
            Vector3 impactPosition = sap.StreamDestination - normal * (ground ? _groundCrownInset : 0f);
            Quaternion impactRotation = Quaternion.FromToRotation(Vector3.up, normal);
            if (useAssetImpact)
            {
                _assetImpactParticles.transform.SetPositionAndRotation(impactPosition, impactRotation);
                float assetEffectScale = sap.Settings != null ? sap.Settings.hoseEffectScale * sap.Settings.impactCrownScale : 1f;
                _assetImpactParticles.transform.localScale = _assetImpactBaseScale * assetEffectScale;
                if (!_assetImpactParticles.isPlaying) _assetImpactParticles.Play(true);
                _wasImpactVisible = true;
                return;
            }

            _wasImpactVisible = true;
            if (_wallCrownVisual != null && _wallCrownVisual.gameObject.activeSelf == ground)
                _wallCrownVisual.gameObject.SetActive(!ground);
            if (_groundCrownVisual != null && _groundCrownVisual.gameObject.activeSelf != ground)
                _groundCrownVisual.gameObject.SetActive(ground);
            if (_groundCrownRim != null && _groundCrownRim.gameObject.activeSelf != ground)
                _groundCrownRim.gameObject.SetActive(ground);
            float effectScale = sap.Settings != null ? sap.Settings.hoseEffectScale * sap.Settings.impactCrownScale : 1f;
            _impactCrown.localScale = Vector3.Scale(_impactCrownBaseScale * effectScale,
                new Vector3(ground ? _groundCrownWidthScale : 1f,
                    ground ? _groundCrownHeightScale : 1f,
                    ground ? _groundCrownWidthScale : 1f));
            _impactCrown.SetPositionAndRotation(impactPosition, impactRotation);
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
                    startSize = Random.Range(_sprayDropSize.x, _sprayDropSize.y) *
                        (sap.Settings != null ? sap.Settings.hoseEffectScale : 1f)
                };
                _sprayDroplets.Emit(particle, 1);
            }
        }

        private void UpdateImpactSplashDroplets(bool impacting, float deltaTime, float streamDiameter)
        {
            if (!impacting || _sprayDroplets == null)
            {
                _impactSplashBudget = 0f;
                return;
            }
            if (deltaTime <= 0f) return;
            if (!_sprayDroplets.isPlaying) _sprayDroplets.Play();

            _impactSplashBudget = Mathf.Min(_sprayMaxDropsPerFrame,
                _impactSplashBudget + _impactSplashDropsPerSecond * deltaTime);
            int count = Mathf.Min(_sprayMaxDropsPerFrame, Mathf.FloorToInt(_impactSplashBudget));
            _impactSplashBudget -= count;

            Vector3 normal = sap.HoseImpactNormal;
            if (normal.sqrMagnitude < 0.0001f) normal = Vector3.up;
            normal.Normalize();
            Vector3 tangent = Vector3.Cross(normal, Vector3.up);
            if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.Cross(normal, Vector3.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);
            float effectScale = sap.Settings != null ? sap.Settings.hoseEffectScale : 1f;
            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                Vector3 radial = tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle);
                var particle = new ParticleSystem.EmitParams
                {
                    position = sap.StreamDestination + normal * 0.03f
                        + radial * Random.Range(0f, streamDiameter * 0.35f),
                    velocity = radial * Random.Range(_impactSplashSideSpeed.x, _impactSplashSideSpeed.y)
                        + normal * Random.Range(_impactSplashNormalSpeed.x, _impactSplashNormalSpeed.y),
                    startSize = Random.Range(_impactSplashDropSize.x, _impactSplashDropSize.y) * effectScale
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
            if (_tubeMesh != null)
            {
                if (_tubeFilter != null) _tubeFilter.sharedMesh = null;
                if (Application.isPlaying) Destroy(_tubeMesh);
                else DestroyImmediate(_tubeMesh);
            }
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
            _wasImpactVisible = false;
            _impactSplashBudget = 0f;
            if (_assetImpactParticles != null)
                _assetImpactParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
