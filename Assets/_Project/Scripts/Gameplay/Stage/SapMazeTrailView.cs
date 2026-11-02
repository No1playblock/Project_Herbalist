using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Water presentation consumes the authoritative occupied volume; its outline is continuous.
    [RequireComponent(typeof(LineRenderer))]
    public sealed class SapMazeTrailView : MonoBehaviour
    {
        [SerializeField] private SapMazeTrailSettings _settings;
        [SerializeField] private LineRenderer _line;
        [SerializeField] private MeshFilter _fluidMesh;
        [SerializeField] private MeshRenderer _fluidRenderer;
        private Mesh _mesh;
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<int> _triangles = new List<int>();
        private readonly HashSet<int> _squares = new HashSet<int>();
        private readonly SapMazeFluidOutline _outline = new SapMazeFluidOutline();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly Dictionary<int, FloorSample> _floor = new Dictionary<int, FloorSample>();
        private readonly Dictionary<int, float> _sampleDensity = new Dictionary<int, float>();
        private float _surfaceLiftLocal;
        private bool _flatSurface;
        private Vector3 _flatNormal;
        private readonly Dictionary<ulong, bool> _clearEdges = new Dictionary<ulong, bool>();
        private readonly Dictionary<ulong, FloorSample> _wallLimits = new Dictionary<ulong, FloorSample>();
        private readonly Dictionary<ulong, int> _edgeVertices = new Dictionary<ulong, int>();
        private readonly Dictionary<int, int> _cornerVertices = new Dictionary<int, int>();
        private readonly List<int> _polygon = new List<int>(4);
        private SapMazeSurface _cachedSurface;
        private Matrix4x4 _cachedMatrix;
        private Mesh _cachedFloorMesh;
        private int _cachedGeometryVersion;
        private int _refinedWidth, _subdivisions;
        private float _outlineLevel;
        private struct FloorSample { public Vector2 Point; public float Z; public bool Open; }
        private SapMazeFluidField _renderedField;
        private ulong _renderStamp;
        private float _nextFluidRender;
        private readonly List<Vector3> _visible = new List<Vector3>(128);
        private readonly List<Vector2> _path = new List<Vector2>(128);
        private readonly List<float> _widths = new List<float>(128);

        public SapMazeTrailSettings Settings => _settings;
        public Vector3 LastBuildTimingsMs { get; private set; }
        public Vector2 HeadCenter { get; private set; }
        public float HeadWidthWorld { get; private set; }

        private ulong _tubeStamp;
        private bool _lastWasTube;
        private readonly List<Vector2> _tubeCenters = new List<Vector2>(128);
        private readonly List<Vector2> _tubeTangents = new List<Vector2>(128);
        private readonly List<float> _tubeWidths = new List<float>(128);

        public void PresentTube(IReadOnlyList<Vector2> points, SapMazeSurface surface)
        {
            if (_line != null) _line.positionCount = 0;
            if (_fluidMesh == null || _fluidRenderer == null || points.Count < 2) return;
            _fluidRenderer.enabled = true;
            ulong stamp = 14695981039346656037UL;
            unchecked
            {
                foreach (var p in points) stamp = (stamp ^ (uint)p.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.tubeWidthWorld.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.tubeRadialSegments) * 1099511628211UL;
            }
            if (_mesh != null && _lastWasTube && _tubeStamp == stamp) return;
            if (_mesh == null)
            {
                _mesh = new Mesh { name = "Maze flexible tube", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                _mesh.MarkDynamic(); _fluidMesh.sharedMesh = _mesh;
            }
            _vertices.Clear(); _normals.Clear(); _triangles.Clear();
            _tubeCenters.Clear(); _tubeTangents.Clear(); _tubeWidths.Clear();
            float scale = transform.TransformVector(Vector3.right).magnitude;
            for (int i = 0; i < points.Count; i++)
            {
                var tangent = points[Mathf.Min(points.Count - 1, i + 1)] - points[Mathf.Max(0, i - 1)];
                if (tangent.sqrMagnitude < .0000001f) tangent = Vector2.up;
                tangent.Normalize();
                float width = surface.ChannelSection(points[i], tangent, _settings.tubeWidthWorld, 0, out var center);
                _tubeCenters.Add(center); _tubeTangents.Add(tangent);
                _tubeWidths.Add(Mathf.Max(.005f, width) / scale * .5f);
            }
            AddTubeRing(surface, 0, -1f);
            AddTubeRing(surface, 0, -.8660254f);
            AddTubeRing(surface, 0, -.5f);
            for (int i = 0; i < points.Count; i++) AddTubeRing(surface, i, 0);
            AddTubeRing(surface, points.Count - 1, .5f);
            AddTubeRing(surface, points.Count - 1, .8660254f);
            AddTubeRing(surface, points.Count - 1, 1f);
            int stride = _settings.tubeRadialSegments + 1;
            int rings = _vertices.Count / stride;
            for (int i = 0; i < rings - 1; i++)
            for (int j = 0; j < stride - 1; j++)
            {
                int a = i * stride + j, b = a + stride;
                _triangles.Add(a); _triangles.Add(a + 1); _triangles.Add(b);
                _triangles.Add(a + 1); _triangles.Add(b + 1); _triangles.Add(b);
            }
            _mesh.Clear(); _mesh.SetVertices(_vertices); _mesh.SetNormals(_normals);
            _mesh.SetTriangles(_triangles, 0); _mesh.RecalculateBounds();
            _tubeStamp = stamp; _lastWasTube = true;
        }

        private void AddTubeRing(SapMazeSurface surface, int index, float axial)
        {
            var tangent = _tubeTangents[index];
            float radius = _tubeWidths[index];
            var center = _tubeCenters[index];
            if (axial != 0) center = surface.Move(center, tangent * (radius * axial));
            float z = surface.SurfaceZ(center, _settings.localZ);
            float radial = Mathf.Sqrt(Mathf.Max(0, 1 - axial * axial));
            var across = new Vector3(tangent.y, -tangent.x, 0);
            var along = new Vector3(tangent.x, tangent.y, 0);
            for (int j = 0; j <= _settings.tubeRadialSegments; j++)
            {
                float angle = Mathf.PI * j / _settings.tubeRadialSegments;
                var normal = across * Mathf.Cos(angle) + Vector3.back * Mathf.Sin(angle);
                _vertices.Add(new Vector3(center.x, center.y, z) + normal * (radius * radial));
                _normals.Add(normal * radial + along * axial);
            }
        }

        public void PresentField(SapMazeFluidField field, SapMazeSurface surface)
        {
            if (_line != null) _line.positionCount = 0;
            if (_fluidMesh == null || _fluidRenderer == null || field == null || _settings == null) return;
            _fluidRenderer.enabled = true;
            if (Time.unscaledTime < _nextFluidRender && _renderedField == field) return;
            _nextFluidRender = Time.unscaledTime + _settings.fluidRenderInterval;
            ulong stamp = 14695981039346656037UL;
            unchecked
            {
                for (int i = 0; i < field.ActiveCells.Count; i++) stamp = (stamp ^ field.Pack(i)) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceRoundPasses) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceRoundRadiusWorld.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceRoundStrength.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceThreshold.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.outlineBlendRadiusWorld.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.outlineBlendPasses) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.outlineThreshold.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.outlineSubdivisions) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.waterUvScale.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.fluidSurfaceLiftWorld.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.outlineWallInsetWorld.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)surface.GeometryVersion) * 1099511628211UL;
            }
            if (_mesh != null && _renderedField == field && _renderStamp == stamp) return;
            if (_mesh == null)
            {
                _mesh = new Mesh { name = "Maze occupied water surface", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                _mesh.MarkDynamic(); _fluidMesh.sharedMesh = _mesh;
            }
            _vertices.Clear(); _normals.Clear(); _triangles.Clear(); _squares.Clear(); _uvs.Clear();
            int subdivisions = Mathf.Clamp(_settings.outlineSubdivisions, 1, 3);
            var matrix = transform.worldToLocalMatrix * surface.MazeCollider.transform.localToWorldMatrix;
            if (_cachedSurface != surface || _cachedGeometryVersion != surface.GeometryVersion || subdivisions != _subdivisions || _cachedMatrix != matrix ||
                _cachedFloorMesh != surface.MazeCollider.sharedMesh || _floor.Count > _settings.outlineGeometryCacheLimit ||
                _clearEdges.Count > _settings.outlineGeometryCacheLimit * 3)
            {
                _floor.Clear(); _clearEdges.Clear(); _wallLimits.Clear(); _cachedSurface = surface;
                _cachedMatrix = matrix; _cachedFloorMesh = surface.MazeCollider.sharedMesh;
                _cachedGeometryVersion = surface.GeometryVersion;
            }
            _subdivisions = subdivisions;
            _refinedWidth = (field.Width - 1) * subdivisions + 1;
            float scale = transform.TransformVector(Vector3.right).magnitude;
            var buildTimer = System.Diagnostics.Stopwatch.StartNew();
            _outline.Build(field, _settings.outlineBlendRadiusWorld / Mathf.Max(.001f, scale), _settings.outlineBlendPasses);
            _outlineLevel = Mathf.Min(_settings.outlineThreshold, _outline.Maximum * .4f);
            float outlineTime = (float)buildTimer.Elapsed.TotalMilliseconds;
            buildTimer.Restart();
            _edgeVertices.Clear(); _cornerVertices.Clear(); _sampleDensity.Clear();
            _surfaceLiftLocal = surface.UsesFloorPlane ? 0 : _settings.fluidSurfaceLiftWorld / Mathf.Max(.001f, transform.TransformVector(Vector3.forward).magnitude);
            _flatSurface = surface.UsesFloorPlane;
            if (_flatSurface) _flatNormal = transform.localToWorldMatrix.transpose.MultiplyVector(-surface.FloorPlane.forward).normalized;
            foreach (int cell in _outline.Cells)
            {
                if (_outline.Value(cell) < _outlineLevel * .25f) continue;
                _squares.Add(cell); _squares.Add(cell - 1);
                _squares.Add(cell - field.Width); _squares.Add(cell - field.Width - 1);
            }
            foreach (int square in _squares)
            {
                if (square < 0 || square + field.Width + 1 >= field.Count || square % field.Width == field.Width - 1) continue;
                int x = square % field.Width * subdivisions, y = square / field.Width * subdivisions;
                for (int sy = 0; sy < subdivisions; sy++)
                for (int sx = 0; sx < subdivisions; sx++)
                {
                    int a = (y + sy) * _refinedWidth + x + sx;
                    DrawOutlineTriangle(field, surface, a, a + _refinedWidth, a + _refinedWidth + 1);
                    DrawOutlineTriangle(field, surface, a, a + _refinedWidth + 1, a + 1);
                }
            }
            float tessellationTime = (float)buildTimer.Elapsed.TotalMilliseconds;
            buildTimer.Restart();
            _mesh.Clear(); _mesh.SetVertices(_vertices); _mesh.SetTriangles(_triangles, 0, false);
            _mesh.SetUVs(0, _uvs);
            if (_flatSurface) _mesh.SetNormals(_normals); else _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            LastBuildTimingsMs = new Vector3(outlineTime, tessellationTime, (float)buildTimer.Elapsed.TotalMilliseconds);
            _renderedField = field; _renderStamp = stamp; _lastWasTube = false;
        }

        private FloorSample Floor(SapMazeFluidField field, SapMazeSurface surface, int index)
        {
            if (_floor.TryGetValue(index, out var sample)) return sample;
            sample.Point = field.Point(0) + new Vector2(index % _refinedWidth, index / _refinedWidth) * (field.CellSize / _subdivisions);
            sample.Open = surface.IsSurfaceOpen(sample.Point);
            if (sample.Open) sample.Z = surface.SurfaceZ(sample.Point, _settings.localZ);
            _floor[index] = sample;
            return sample;
        }
        private float Density(SapMazeFluidField field, SapMazeSurface surface, int index)
        {
            if (_sampleDensity.TryGetValue(index, out float value)) return value;
            value = _outline.Sample(field, (float)(index % _refinedWidth) / _subdivisions,
                (float)(index / _refinedWidth) / _subdivisions);
            value = value > 0 && Floor(field, surface, index).Open ? value : 0;
            _sampleDensity[index] = value;
            return value;
        }
        // UInt64's default hash XORs its halves: neighbouring grid pairs otherwise
        // collapse into the same dictionary bucket. Mix the pair bijectively first.
        private static ulong MixEdge(ulong key)
        {
            unchecked
            {
                key ^= key >> 30; key *= 0xbf58476d1ce4e5b9UL;
                key ^= key >> 27; key *= 0x94d049bb133111ebUL;
                return key ^ (key >> 31);
            }
        }
        private static ulong EdgeKey(int a, int b) => MixEdge(((ulong)(uint)Mathf.Min(a, b) << 32) | (uint)Mathf.Max(a, b));
        private bool ClearEdge(SapMazeFluidField field, SapMazeSurface surface, int a, int b)
        {
            ulong key = EdgeKey(a, b);
            if (_clearEdges.TryGetValue(key, out bool clear)) return clear;
            clear = surface.IsSegmentOpen(Floor(field, surface, a).Point, Floor(field, surface, b).Point);
            _clearEdges[key] = clear; return clear;
        }
        private int AddSurfaceVertex(Vector2 point, float z)
        {
            int index = _vertices.Count;
            _vertices.Add(new Vector3(point.x, point.y, z - _surfaceLiftLocal));
            if (_flatSurface) _normals.Add(_flatNormal);
            _uvs.Add(point * _settings.waterUvScale);
            return index;
        }
        private int CornerVertex(SapMazeFluidField field, SapMazeSurface surface, int corner)
        {
            if (_cornerVertices.TryGetValue(corner, out int index)) return index;
            var sample = Floor(field, surface, corner);
            index = AddSurfaceVertex(sample.Point, sample.Z); _cornerVertices[corner] = index;
            return index;
        }
        private int BoundaryVertex(SapMazeFluidField field, SapMazeSurface surface, int a, int b, float da, float db)
        {
            ulong key = EdgeKey(a, b);
            if (_edgeVertices.TryGetValue(key, out int index)) return index;
            var first = Floor(field, surface, a); var second = Floor(field, surface, b);
            var point = Vector2.Lerp(first.Point, second.Point, (_outlineLevel - da) / (db - da));
            var wet = da >= _outlineLevel ? first : second;
            float z;
            if (first.Open && second.Open && ClearEdge(field, surface, a, b))
                z = Mathf.Lerp(first.Z, second.Z, Mathf.Clamp01((_outlineLevel - da) / (db - da)));
            else
            {
                int wetIndex = da >= _outlineLevel ? a : b, dryIndex = wetIndex == a ? b : a;
                ulong directed = MixEdge(((ulong)(uint)wetIndex << 32) | (uint)dryIndex);
                if (!_wallLimits.TryGetValue(directed, out var limit))
                {
                    var destination = wetIndex == a ? second : first;
                    limit.Point = surface.Move(wet.Point, destination.Point - wet.Point);
                    float inset = _settings.outlineWallInsetWorld / Mathf.Max(.001f, transform.TransformVector(Vector3.right).magnitude);
                    var delta = limit.Point - wet.Point;
                    limit.Point = wet.Point + delta.normalized * Mathf.Max(0, delta.magnitude - inset);
                    limit.Z = surface.SurfaceZ(limit.Point, wet.Z);
                    _wallLimits[directed] = limit;
                }
                float wanted = Vector2.Distance(wet.Point, point), available = Vector2.Distance(wet.Point, limit.Point);
                float fraction = available > .000001f ? Mathf.Clamp01(wanted / available) : 0;
                point = Vector2.Lerp(wet.Point, limit.Point, fraction);
                z = Mathf.Lerp(wet.Z, limit.Z, fraction);
            }
            index = AddSurfaceVertex(point, z);
            _edgeVertices[key] = index; return index;
        }
        private void OutlineEdge(SapMazeFluidField field, SapMazeSurface surface, int a, int b, float da, float db)
        {
            bool insideA = da >= _outlineLevel, insideB = db >= _outlineLevel;
            if (insideA) _polygon.Add(CornerVertex(field, surface, a));
            if (insideA != insideB) _polygon.Add(BoundaryVertex(field, surface, a, b, da, db));
        }
        private void DrawOutlineTriangle(SapMazeFluidField field, SapMazeSurface surface, int a, int b, int c)
        {
            float da = Density(field, surface, a), db = Density(field, surface, b), dc = Density(field, surface, c);
            if (da < _outlineLevel && db < _outlineLevel && dc < _outlineLevel) return;
            if ((da >= _outlineLevel && db >= _outlineLevel && !ClearEdge(field, surface, a, b)) ||
                (db >= _outlineLevel && dc >= _outlineLevel && !ClearEdge(field, surface, b, c)) ||
                (dc >= _outlineLevel && da >= _outlineLevel && !ClearEdge(field, surface, c, a))) return;
            _polygon.Clear();
            OutlineEdge(field, surface, a, b, da, db); OutlineEdge(field, surface, b, c, db, dc);
            OutlineEdge(field, surface, c, a, dc, da);
            for (int i = 1; i + 1 < _polygon.Count; i++)
            {
                int first = _polygon[0], second = _polygon[i], third = _polygon[i + 1];
                Vector2 ab = _vertices[second] - _vertices[first], ac = _vertices[third] - _vertices[first];
                if (Mathf.Abs(ab.x * ac.y - ab.y * ac.x) < .000000001f) continue;
                _triangles.Add(first); _triangles.Add(second); _triangles.Add(third);
            }
        }

        private void OnDestroy() { if (_mesh != null) Destroy(_mesh); }

        public float Present(IReadOnlyList<Vector2> history, Vector2 head, float fraction, SapMazeSurface surface = null)
        {
            if (_line == null || _settings == null) return Mathf.Pow(Mathf.Clamp01(fraction), 1f / 3f);
            var remainingMl = _settings.capacityMl * Mathf.Clamp01(fraction);
            var budget = _settings.VisibleLength(fraction);
            _visible.Clear();
            _widths.Clear();
            var cursor = head;
            _visible.Add(new Vector3(head.x, head.y, surface != null && surface.Ready ? surface.SurfaceZ(head, _settings.localZ) : _settings.localZ));
            var usedLength = 0f;
            for (var i = history.Count - 1; i >= 0 && usedLength < budget; i--)
            {
                var previous = history[i];
                var length = Vector2.Distance(cursor, previous);
                if (length < .0001f) continue;
                if (usedLength + length >= budget)
                {
                    var tail = Vector2.Lerp(cursor, previous, (budget - usedLength) / length);
                    _visible.Add(new Vector3(tail.x, tail.y, surface != null && surface.Ready ? surface.SurfaceZ(tail, _settings.localZ) : _settings.localZ));
                    usedLength = budget;
                    break;
                }
                _visible.Add(new Vector3(previous.x, previous.y, surface != null && surface.Ready ? surface.SurfaceZ(previous, _settings.localZ) : _settings.localZ));
                usedLength += length;
                cursor = previous;
            }
            _visible.Reverse();
            HeadCenter = head;
            HeadWidthWorld = 0;
            if (surface != null && surface.Ready)
            {
                _line.alignment = LineAlignment.TransformZ;
                _path.Clear();
                for (var i = 0; i < _visible.Count; i++)
                    _path.Add(new Vector2(_visible[i].x, _visible[i].y));
                for (var i = 0; i < _path.Count; i++)
                {
                    var previous = _path[Mathf.Max(0, i - 1)];
                    var next = _path[Mathf.Min(_path.Count - 1, i + 1)];
                    var travel = next - previous;
                    var width = surface.ChannelSection(_path[i], travel,
                        _settings.maximumChannelWidth, _settings.channelEdgeInset, out var center);
                    if (width <= 0) { width = _settings.trailWidth; center = _path[i]; }
                    _widths.Add(width);
                    _visible[i] = new Vector3(center.x, center.y, surface.SurfaceZ(center, _settings.localZ));
                }
                HeadCenter = new Vector2(_visible[_visible.Count - 1].x, _visible[_visible.Count - 1].y);
                HeadWidthWorld = _widths[_widths.Count - 1];
            }
            _line.positionCount = _visible.Count >= 2 && usedLength > .001f ? _visible.Count : 0;
            for (var i = 0; i < _line.positionCount; i++) _line.SetPosition(i, _visible[i]);
            if (_line.positionCount > 1 && _widths.Count == _visible.Count)
            {
                var length = 0f;
                for (var i = 1; i < _visible.Count; i++) length += Vector3.Distance(_visible[i - 1], _visible[i]);
                var keys = new Keyframe[_visible.Count];
                var distance = 0f;
                for (var i = 0; i < _visible.Count; i++)
                {
                    if (i > 0) distance += Vector3.Distance(_visible[i - 1], _visible[i]);
                    keys[i] = new Keyframe(length > .0001f ? distance / length : (float)i / (_visible.Count - 1), _widths[i]);
                }
                _line.widthCurve = new AnimationCurve(keys);
                _line.widthMultiplier = 1f;
            }
            else _line.widthMultiplier = _settings.trailWidth * Mathf.Sqrt(Mathf.Clamp01(fraction));
            var headMl = Mathf.Max(0, remainingMl - usedLength * _settings.millilitersPerUnit);
            return Mathf.Pow(Mathf.Clamp01(headMl / _settings.capacityMl), 1f / 3f);
        }

        public void Clear()
        {
            if (_line != null) _line.positionCount = 0;
            if (_fluidRenderer != null) _fluidRenderer.enabled = false;
        }
    }
}
