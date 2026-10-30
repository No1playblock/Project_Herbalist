using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Presentation-only water volume: the moving head and visible trail share one budget.
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
        private readonly Dictionary<int, float> _depths = new Dictionary<int, float>();
        private readonly List<Vector3> _contour = new List<Vector3>(4);
        private readonly Dictionary<Vector2Int, int> _weld = new Dictionary<Vector2Int, int>();
        private readonly List<Vector3> _shared = new List<Vector3>();
        private readonly List<int> _sharedTriangles = new List<int>();
        private readonly Dictionary<ulong, int> _edges = new Dictionary<ulong, int>();
        private readonly Dictionary<int, List<int>> _boundary = new Dictionary<int, List<int>>();
        private readonly Dictionary<int, List<int>> _neighbours = new Dictionary<int, List<int>>();
        private readonly List<Vector2> _displacement = new List<Vector2>();
        private readonly List<Vector2> _relaxed = new List<Vector2>();
        private readonly List<Vector3> _rounded = new List<Vector3>();
        private readonly List<List<int>> _neighbourPool = new List<List<int>>();
        private readonly List<List<int>> _boundaryPool = new List<List<int>>();
        private int _neighbourCursor, _boundaryCursor;
        private SapMazeFluidField _renderedField;
        private ulong _renderStamp;
        private readonly List<Vector3> _visible = new List<Vector3>(128);
        private readonly List<Vector2> _path = new List<Vector2>(128);
        private readonly List<float> _widths = new List<float>(128);

        public SapMazeTrailSettings Settings => _settings;
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
            ulong stamp = 14695981039346656037UL;
            unchecked
            {
                for (int i = 0; i < field.ActiveCells.Count; i++) stamp = (stamp ^ field.Pack(i)) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceRoundPasses) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceRoundRadiusWorld.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceRoundStrength.GetHashCode()) * 1099511628211UL;
                stamp = (stamp ^ (uint)_settings.surfaceThreshold.GetHashCode()) * 1099511628211UL;
            }
            if (_mesh != null && _renderedField == field && _renderStamp == stamp) return;
            if (_mesh == null)
            {
                _mesh = new Mesh { name = "Maze occupied water surface", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                _mesh.MarkDynamic(); _fluidMesh.sharedMesh = _mesh;
            }
            _vertices.Clear(); _normals.Clear(); _triangles.Clear(); _squares.Clear();
            int width = field.Width;
            foreach (int cell in field.ActiveCells)
            {
                if (field.Mass(cell) / field.CellCapacity < _settings.surfaceThreshold) continue;
                _squares.Add(cell); _squares.Add(cell - 1);
                _squares.Add(cell - width); _squares.Add(cell - width - 1);
            }
            foreach (int square in _squares)
            {
                if (square < 0 || square + width + 1 >= field.Count || square % width == width - 1) continue;
                DrawTriangle(field, surface, square, square + width, square + width + 1);
                DrawTriangle(field, surface, square, square + width + 1, square + 1);
            }
            RoundBoundary(surface);
            _mesh.Clear(); _mesh.SetVertices(_vertices); _mesh.SetTriangles(_triangles, 0);
            for (int i = 0; i < _vertices.Count; i++) _normals.Add(Vector3.back);
            _mesh.SetNormals(_normals); _mesh.RecalculateBounds();
            _renderedField = field; _renderStamp = stamp; _lastWasTube = false;
        }

        // Round the exposed outline, rather than blurring mass through walls or
        // changing Host gameplay. Internal triangles and the floor stay in place.
        private void RoundBoundary(SapMazeSurface surface)
        {
            if (_settings.surfaceRoundPasses == 0 || _settings.surfaceRoundRadiusWorld <= 0) return;
            _weld.Clear(); _shared.Clear(); _sharedTriangles.Clear(); _edges.Clear(); _boundary.Clear(); _neighbours.Clear();
            _neighbourCursor = _boundaryCursor = 0;
            foreach (int vertex in _triangles)
            {
                var p = _vertices[vertex];
                var key = new Vector2Int(Mathf.RoundToInt(p.x * 100000), Mathf.RoundToInt(p.y * 100000));
                if (!_weld.TryGetValue(key, out int index))
                { index = _shared.Count; _weld.Add(key, index); _shared.Add(p); }
                _sharedTriangles.Add(index);
            }
            for (int i = 0; i < _sharedTriangles.Count; i += 3)
            {
                CountEdge(_sharedTriangles[i], _sharedTriangles[i + 1]);
                CountEdge(_sharedTriangles[i + 1], _sharedTriangles[i + 2]);
                CountEdge(_sharedTriangles[i + 2], _sharedTriangles[i]);
            }
            foreach (var edge in _edges)
            {
                int a = (int)(edge.Key >> 32), b = (int)(edge.Key & 0xffffffff);
                AddNeighbour(_neighbours, a, b); AddNeighbour(_neighbours, b, a);
                if (edge.Value != 1) continue;
                AddBoundary(a, b); AddBoundary(b, a);
            }
            float worldScale = transform.TransformVector(Vector3.right).magnitude;
            float radius = _settings.surfaceRoundRadiusWorld / Mathf.Max(.001f, worldScale);
            for (int pass = 0; pass < _settings.surfaceRoundPasses; pass++)
            {
                _rounded.Clear(); _rounded.AddRange(_shared);
                foreach (var pair in _boundary)
                {
                    if (pair.Value.Count != 2) continue;
                    int index = pair.Key;
                    var p = _shared[index];
                    var left = AlongBoundary(index, pair.Value[0], radius);
                    var right = AlongBoundary(index, pair.Value[1], radius);
                    var wanted = Vector3.Lerp(p, (left + p * 2 + right) * .25f, _settings.surfaceRoundStrength);
                    var start = new Vector2(p.x, p.y);
                    var moved = surface.Move(start, new Vector2(wanted.x - p.x, wanted.y - p.y));
                    _rounded[index] = new Vector3(moved.x, moved.y,
                        surface.IsSurfaceOpen(moved) ? surface.SurfaceZ(moved, p.z) : p.z);
                }
                // Carry the boundary displacement into the interior so rounded
                // corners do not fold triangles over an unmoved inner grid.
                _displacement.Clear();
                for (int i = 0; i < _shared.Count; i++) _displacement.Add((Vector2)(_rounded[i] - _shared[i]));
                int iterations = Mathf.Max(1, Mathf.CeilToInt(radius / surface.CellSize));
                for (int iteration = 0; iteration < iterations; iteration++)
                {
                    _relaxed.Clear(); _relaxed.AddRange(_displacement);
                    foreach (var pair in _neighbours)
                    {
                        if (_boundary.ContainsKey(pair.Key)) continue;
                        var delta = Vector2.zero;
                        foreach (int neighbour in pair.Value) delta += _displacement[neighbour];
                        _relaxed[pair.Key] = delta / pair.Value.Count;
                    }
                    _displacement.Clear(); _displacement.AddRange(_relaxed);
                }
                // Test the actual wall-clamped displacement, not the requested
                // displacement: a clamped boundary can otherwise flip a thin triangle.
                for (int i = 0; i < _shared.Count; i++)
                {
                    var p = _shared[i];
                    var start = new Vector2(p.x, p.y);
                    if (_displacement[i].sqrMagnitude < .0000000001f) continue;
                    _displacement[i] = surface.Move(start, _displacement[i]) - start;
                }
                float amount = 1;
                for (int attempt = 0; attempt < 8 && !KeepsTriangleOrientation(amount); attempt++) amount *= .5f;
                if (!KeepsTriangleOrientation(amount)) amount = 0;
                for (int i = 0; i < _shared.Count && amount > 0; i++)
                {
                    if (_displacement[i].sqrMagnitude < .0000000001f) continue;
                    var p = _shared[i];
                    var moved = new Vector2(p.x, p.y) + _displacement[i] * amount;
                    _shared[i] = new Vector3(moved.x, moved.y,
                        surface.IsSurfaceOpen(moved) ? surface.SurfaceZ(moved, p.z) : p.z);
                }
            }
            _vertices.Clear(); _vertices.AddRange(_shared);
            _triangles.Clear(); _triangles.AddRange(_sharedTriangles);
        }

        private bool KeepsTriangleOrientation(float amount)
        {
            for (int i = 0; i < _sharedTriangles.Count; i += 3)
            {
                int a = _sharedTriangles[i], b = _sharedTriangles[i + 1], c = _sharedTriangles[i + 2];
                Vector2 pa = _shared[a], pb = _shared[b], pc = _shared[c];
                var ab = pb - pa; var ac = pc - pa;
                float original = ab.x * ac.y - ab.y * ac.x;
                if (Mathf.Abs(original) < .0000000001f) continue;
                pa += _displacement[a] * amount; pb += _displacement[b] * amount; pc += _displacement[c] * amount;
                ab = pb - pa; ac = pc - pa;
                if ((ab.x * ac.y - ab.y * ac.x) * original <= 0) return false;
            }
            return true;
        }

        private void CountEdge(int a, int b)
        {
            if (a == b) return;
            ulong key = ((ulong)(uint)Mathf.Min(a, b) << 32) | (uint)Mathf.Max(a, b);
            _edges.TryGetValue(key, out int count); _edges[key] = count + 1;
        }

        private void AddBoundary(int from, int to)
        {
            AddNeighbour(_boundary, from, to);
        }

        private void AddNeighbour(Dictionary<int, List<int>> graph, int from, int to)
        {
            if (!graph.TryGetValue(from, out var neighbours))
            {
                bool boundary = graph == _boundary;
                var pool = boundary ? _boundaryPool : _neighbourPool;
                int cursor = boundary ? _boundaryCursor++ : _neighbourCursor++;
                if (cursor == pool.Count) pool.Add(new List<int>(6));
                neighbours = pool[cursor]; neighbours.Clear(); graph.Add(from, neighbours);
            }
            neighbours.Add(to);
        }

        private Vector3 AlongBoundary(int from, int next, float remaining)
        {
            int first = from;
            for (int step = 0; step < _boundary.Count; step++)
            {
                float distance = Vector2.Distance(_shared[from], _shared[next]);
                if (distance >= remaining && distance > .000001f)
                    return Vector3.Lerp(_shared[from], _shared[next], remaining / distance);
                remaining -= distance;
                if (!_boundary.TryGetValue(next, out var neighbours) || neighbours.Count != 2) return _shared[next];
                int after = neighbours[0] == from ? neighbours[1] : neighbours[0];
                from = next; next = after;
                if (next == first) return _shared[from];
            }
            return _shared[from];
        }

        private Vector3 Vertex(SapMazeFluidField field, SapMazeSurface surface, int cell)
        {
            var p = field.Point(cell);
            if (!_depths.TryGetValue(cell, out float z))
            { z = surface.SurfaceZ(p, _settings.localZ); _depths[cell] = z; }
            return new Vector3(p.x, p.y, z);
        }

        private void DrawTriangle(SapMazeFluidField field, SapMazeSurface surface, int a, int b, int c)
        {
            _contour.Clear();
            ClipEdge(field, surface, a, b); ClipEdge(field, surface, b, c); ClipEdge(field, surface, c, a);
            if (_contour.Count < 3) return;
            int first = _vertices.Count;
            foreach (var vertex in _contour) _vertices.Add(vertex);
            for (int i = 1; i + 1 < _contour.Count; i++)
            { _triangles.Add(first); _triangles.Add(first + i); _triangles.Add(first + i + 1); }
        }

        private void ClipEdge(SapMazeFluidField field, SapMazeSurface surface, int a, int b)
        {
            float da = field.Mass(a) / field.CellCapacity, db = field.Mass(b) / field.CellCapacity;
            float threshold = _settings.surfaceThreshold;
            bool insideA = da >= threshold, insideB = db >= threshold;
            if (insideA) _contour.Add(Vertex(field, surface, a));
            if (insideA != insideB)
            {
                var pa = Vertex(field, surface, a); var pb = Vertex(field, surface, b);
                var edge = Vector3.Lerp(pa, pb, (threshold - da) / (db - da));
                // An empty sample may be on top of a wall. Keep the water edge
                // at the wet floor depth rather than stretching up that wall.
                edge.z = insideA ? pa.z : pb.z;
                _contour.Add(edge);
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
