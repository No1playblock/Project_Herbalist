using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Point-sized liquid follows open recesses. The mesh, rather than the visual
    // water width or a selected connected component, determines solid barriers.
    public sealed class SapMazeSurface : MonoBehaviour
    {
        [SerializeField] private MeshCollider _mazeCollider;
        [SerializeField, Range(.005f, .25f)] private float _cellSize = .04f;
        [SerializeField, Min(10000)] private int _maximumSamples = 1000000;
        [Tooltip("Depth below the front wall band; shallow openings do not need to reach the deepest floor.")]
        [SerializeField, Range(.01f, .3f)] private float _channelDepthTolerance = .08f;
        [Tooltip("Navigation slice between the front wall band (0) and channel floor (1). Calibrate to the relief mesh.")]
        [SerializeField, Range(0, 1)] private float _channelDepthFraction = .5f;
        [SerializeField, Range(0, .2f)] private float _visualOffset = .04f;

        private bool[] _walkable;
        private int _width, _height;
        private Vector2 _origin;
        private Transform _board;
        private float _sampleSize, _passageZ, _rayStartZ, _rayLength;
        private Vector3 _rayDirection;
        private int[] _previous, _queue;
        private byte[] _checkedEdges, _openEdges;
        private Mesh _builtMesh;
        private Matrix4x4 _builtBoardMatrix, _builtMeshMatrix;
        private float _builtCellSize, _builtDepthTolerance, _builtDepthFraction;

        public MeshCollider MazeCollider => _mazeCollider;
        public bool Ready => _walkable != null;
        public float CellSize => _sampleSize > 0 ? _sampleSize : _cellSize;

        public bool Build(Transform board)
        {
            if (_walkable != null && _mazeCollider != null && _builtMesh == _mazeCollider.sharedMesh &&
                _builtBoardMatrix == board.localToWorldMatrix && _builtMeshMatrix == _mazeCollider.transform.localToWorldMatrix &&
                _builtCellSize == _cellSize && _builtDepthTolerance == _channelDepthTolerance &&
                _builtDepthFraction == _channelDepthFraction) return true;
            _board = board;
            _walkable = null;
            if (_mazeCollider == null || _mazeCollider.sharedMesh == null) return false;
            var bounds = LocalBounds();
            _origin = new Vector2(bounds.min.x, bounds.min.y);
            _sampleSize = Mathf.Max(.005f, _cellSize);
            _width = Mathf.CeilToInt(bounds.size.x / _sampleSize) + 1;
            _height = Mathf.CeilToInt(bounds.size.y / _sampleSize) + 1;
            if (_width < 2 || _height < 2 || (long)_width * _height > _maximumSamples) return false;

            var depths = new float[_width * _height];
            var interiorDepths = new List<float>(_width * _height);
            _rayStartZ = bounds.min.z - 1f;
            _rayLength = (bounds.size.z + 2f) * board.TransformVector(Vector3.forward).magnitude;
            _rayDirection = board.TransformDirection(Vector3.forward).normalized;
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                var local = CellPosition(x, y);
                var world = board.TransformPoint(new Vector3(local.x, local.y, _rayStartZ));
                RaycastHit hit;
                int index = y * _width + x;
                depths[index] = _mazeCollider.Raycast(new Ray(world, _rayDirection), out hit, _rayLength)
                    ? board.InverseTransformPoint(hit.point).z : float.NegativeInfinity;
                // Include boundary walls: a thin interior barrier may not cover 5% of the board.
                // The percentile still ignores isolated extreme decorations.
                if (!float.IsNegativeInfinity(depths[index]))
                    interiorDepths.Add(depths[index]);
            }
            if (interiorDepths.Count == 0) return false;
            interiorDepths.Sort();
            float front = interiorDepths[Mathf.FloorToInt((interiorDepths.Count - 1) * .05f)];
            float floor = interiorDepths[Mathf.FloorToInt((interiorDepths.Count - 1) * .99f)];
            _passageZ = Mathf.Max(front + _channelDepthTolerance, Mathf.Lerp(front, floor, _channelDepthFraction));
            _walkable = new bool[depths.Length];
            int count = 0;
            for (int i = 0; i < depths.Length; i++) if (depths[i] > _passageZ) { _walkable[i] = true; count++; }
            if (count < 2) { _walkable = null; return false; }
            _previous = new int[depths.Length]; _queue = new int[depths.Length];
            _checkedEdges = new byte[depths.Length]; _openEdges = new byte[depths.Length];
            _builtMesh = _mazeCollider.sharedMesh;
            _builtBoardMatrix = board.localToWorldMatrix; _builtMeshMatrix = _mazeCollider.transform.localToWorldMatrix;
            _builtCellSize = _cellSize; _builtDepthTolerance = _channelDepthTolerance;
            _builtDepthFraction = _channelDepthFraction;
            return true;
        }
        private void OnValidate() { _walkable = null; _sampleSize = 0f; }

        private Bounds LocalBounds()
        {
            var meshBounds = _mazeCollider.sharedMesh.bounds;
            var min = meshBounds.min;
            var max = meshBounds.max;
            var result = new Bounds();
            bool first = true;
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
            for (int z = 0; z < 2; z++)
            {
                var meshPoint = new Vector3(x == 0 ? min.x : max.x,
                    y == 0 ? min.y : max.y, z == 0 ? min.z : max.z);
                var point = _board.InverseTransformPoint(_mazeCollider.transform.TransformPoint(meshPoint));
                if (first) { result = new Bounds(point, Vector3.zero); first = false; }
                else result.Encapsulate(point);
            }
            return result;
        }

        private bool OpenPoint(Vector2 point)
        {
            var start = _board.TransformPoint(new Vector3(point.x, point.y, _rayStartZ));
            return _mazeCollider.Raycast(new Ray(start, _rayDirection), out var hit, _rayLength) &&
                _board.InverseTransformPoint(hit.point).z > _passageZ;
        }
        private bool ClearSegment(Vector2 from, Vector2 to)
        {
            var start = _board.TransformPoint(new Vector3(from.x, from.y, _passageZ));
            var end = _board.TransformPoint(new Vector3(to.x, to.y, _passageZ));
            var delta = end - start;
            if (delta.sqrMagnitude < .00000001f) return true;
            float distance = Mathf.Max(0f, delta.magnitude - .00001f);
            return !_mazeCollider.Raycast(new Ray(start, delta.normalized), out _, distance) &&
                !_mazeCollider.Raycast(new Ray(end, -delta.normalized), out _, distance);
        }
        private Vector2 CellPosition(int x, int y) => _origin + new Vector2(x * CellSize, y * CellSize);
        private bool CanOccupy(Vector2 point)
        {
            if (_walkable == null) return false;
            return IsWalkable(point);
        }

        private bool IsWalkable(Vector2 point)
        {
            int x = Mathf.RoundToInt((point.x - _origin.x) / CellSize);
            int y = Mathf.RoundToInt((point.y - _origin.y) / CellSize);
            return x >= 0 && y >= 0 && x < _width && y < _height && _walkable[y * _width + x];
        }

        // Measure the actual traversable cross-section, then center the visual water in it.
        // Width is returned in world units for the LineRenderer; center remains board-local.
        public float ChannelSection(Vector2 point, Vector2 travel, float maxWorldWidth,
            float edgeInsetWorld, out Vector2 center)
        {
            center = point;
            if (_walkable == null || _board == null || !OpenPoint(point)) return 0;
            var forward = travel.sqrMagnitude > .000001f ? travel.normalized : Vector2.up;
            var across = new Vector2(-forward.y, forward.x);
            float worldPerLocal = _board.TransformVector(new Vector3(across.x, across.y, 0)).magnitude;
            if (worldPerLocal < .000001f) return 0;
            float step = CellSize * .25f;
            float limit = Mathf.Max(step, maxWorldWidth / worldPerLocal * .5f + CellSize);
            float left = ScanChannel(point, -across, step, limit);
            float right = ScanChannel(point, across, step, limit);
            center = point + across * ((right - left) * .5f);
            float measured = (left + right) * worldPerLocal;
            float width = Mathf.Min(measured, maxWorldWidth);
            return Mathf.Max(0, width - 2f * Mathf.Min(edgeInsetWorld, width * .2f));
        }

        private float ScanChannel(Vector2 start, Vector2 direction, float step, float limit)
        {
            float distance = 0;
            while (distance + step <= limit)
            {
                var next = start + direction * (distance + step);
                if (!OpenPoint(next) || !ClearSegment(start + direction * distance, next)) break;
                distance += step;
            }
            float low = distance, high = Mathf.Min(limit, distance + step);
            for (int i = 0; i < 8; i++)
            {
                float middle = (low + high) * .5f;
                var next = start + direction * middle;
                if (OpenPoint(next) && ClearSegment(start, next)) low = middle;
                else high = middle;
            }
            return low;
        }

        public Vector2 Snap(Vector2 point)
        {
            if (_walkable == null) return point;
            if (OpenPoint(point)) return point;
            Vector2 best = point;
            float distance = float.PositiveInfinity;
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                if (!_walkable[y * _width + x]) continue;
                var candidate = CellPosition(x, y);
                if (!CanOccupy(candidate)) continue;
                float d = (candidate - point).sqrMagnitude;
                if (d < distance) { distance = d; best = candidate; }
            }
            return best;
        }

        public Vector2 Move(Vector2 current, Vector2 delta)
        {
            if (_walkable == null) return current;
            int steps = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / (CellSize * .35f)));
            var step = delta / steps;
            for (int i = 0; i < steps; i++)
            {
                if (OpenPoint(current + step) && ClearSegment(current, current + step)) current += step;
                else
                {
                    var before = current;
                    var horizontal = current + new Vector2(step.x, 0);
                    if (OpenPoint(horizontal) && ClearSegment(current, horizontal)) current = horizontal;
                    var vertical = current + new Vector2(0, step.y);
                    if (OpenPoint(vertical) && ClearSegment(current, vertical)) current = vertical;
                    if ((current - before).sqrMagnitude < .00000001f)
                    {
                        // A sampled channel can be only one cell wide. When a bend
                        // catches the point against a cell edge, draw it back toward
                        // the channel centre before the next movement step.
                        int cell = CellIndex(current);
                        if (cell >= 0 && _walkable[cell])
                        {
                            var centre = CellPosition(cell % _width, cell / _width);
                            var correction = Vector2.ClampMagnitude(centre - current, step.magnitude);
                            if (correction.sqrMagnitude > .00000001f && OpenPoint(current + correction) && ClearSegment(current, current + correction))
                                current += correction;
                        }
                    }
                }
            }
            return current;
        }

        public bool TryFindPath(Vector2 start, Vector2 goal, List<Vector2> result)
        {
            result.Clear();
            if (_walkable == null) return false;
            // A direct liquid drag must not require a grid cell to fit inside the gap.
            // Use the actual mesh first; the grid is only needed to route around bends.
            var exactStart = Snap(start);
            var exactEnd = Snap(goal);
            if (ClearSegment(exactStart, exactEnd))
            {
                result.Add(exactStart); result.Add(exactEnd); return true;
            }
            int first = NearestReachableCell(start);
            int last = NearestReachableCell(goal);
            if (first < 0 || last < 0) return false;
            var previous = _previous;
            for (int i = 0; i < previous.Length; i++) previous[i] = -1;
            var queue = _queue;
            int head = 0, tail = 0;
            queue[tail++] = first;
            previous[first] = first;
            while (head < tail && previous[last] < 0)
            {
                int cell = queue[head++];
                int x = cell % _width, y = cell / _width;
                PathVisit(x - 1, y, cell, 1, 2, previous, queue, ref tail);
                PathVisit(x + 1, y, cell, 2, 1, previous, queue, ref tail);
                PathVisit(x, y - 1, cell, 4, 8, previous, queue, ref tail);
                PathVisit(x, y + 1, cell, 8, 4, previous, queue, ref tail);
            }
            if (previous[last] < 0) return false;
            for (int cell = last; cell != first; cell = previous[cell])
                result.Add(CellPosition(cell % _width, cell / _width));
            result.Add(CellPosition(first % _width, first / _width));
            result.Reverse();
            var exactGoal = Snap(goal);
            if ((exactGoal - result[result.Count - 1]).sqrMagnitude > .00000001f) result.Add(exactGoal);
            return true;
        }

        private int NearestReachableCell(Vector2 point)
        {
            point = Snap(point);
            int cell = CellIndex(point);
            if (cell >= 0 && _walkable[cell] && ClearSegment(point, CellPosition(cell % _width, cell / _width))) return cell;
            float distance = float.PositiveInfinity; int best = -1;
            for (int i = 0; i < _walkable.Length; i++)
            {
                if (!_walkable[i]) continue;
                var p = CellPosition(i % _width, i / _width);
                float d = (p - point).sqrMagnitude;
                if (d < distance && ClearSegment(point, p)) { distance = d; best = i; }
            }
            return best;
        }

        private int CellIndex(Vector2 point)
        {
            int x = Mathf.RoundToInt((point.x - _origin.x) / CellSize);
            int y = Mathf.RoundToInt((point.y - _origin.y) / CellSize);
            return x >= 0 && y >= 0 && x < _width && y < _height ? y * _width + x : -1;
        }

        private void PathVisit(int x, int y, int from, byte direction, byte reverse, int[] previous, int[] queue, ref int tail)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height) return;
            int index = y * _width + x;
            if (previous[index] >= 0 || !CanOccupy(CellPosition(x, y))) return;
            if ((_checkedEdges[from] & direction) == 0)
            {
                _checkedEdges[from] |= direction; _checkedEdges[index] |= reverse;
                if (ClearSegment(CellPosition(from % _width, from / _width), CellPosition(x, y)))
                { _openEdges[from] |= direction; _openEdges[index] |= reverse; }
            }
            if ((_openEdges[from] & direction) == 0) return;
            previous[index] = from;
            queue[tail++] = index;
        }

        public float SurfaceZ(Vector2 point, float fallback)
        {
            if (_walkable == null) return fallback;
            var bounds = LocalBounds();
            var world = _board.TransformPoint(new Vector3(point.x, point.y, bounds.min.z - 1f));
            RaycastHit hit;
            var normal = _board.TransformDirection(Vector3.forward).normalized;
            var distance = (bounds.size.z + 2f) * _board.TransformVector(Vector3.forward).magnitude;
            return _mazeCollider.Raycast(new Ray(world, normal), out hit, distance)
                ? _board.InverseTransformPoint(hit.point - normal * _visualOffset).z : fallback;
        }
    }
}
