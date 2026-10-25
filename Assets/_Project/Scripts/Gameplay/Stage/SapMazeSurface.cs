using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Samples the front surface of an authored relief mesh. The deepest connected
    // region is the channel; its boundary constrains the sap without authored edges.
    public sealed class SapMazeSurface : MonoBehaviour
    {
        [SerializeField] private MeshCollider _mazeCollider;
        [SerializeField, Range(.04f, .25f)] private float _cellSize = .1f;
        [SerializeField, Range(.01f, .3f)] private float _channelDepthTolerance = .08f;
        [SerializeField, Range(0, .2f)] private float _visualOffset = .04f;

        private bool[] _walkable;
        private int _width, _height;
        private Vector2 _origin;
        private Transform _board;

        public MeshCollider MazeCollider => _mazeCollider;
        public bool Ready => _walkable != null;
        public float CellSize => _cellSize;

        public bool Build(Transform board)
        {
            _board = board;
            _walkable = null;
            if (_mazeCollider == null || _mazeCollider.sharedMesh == null) return false;
            var bounds = LocalBounds();
            _origin = new Vector2(bounds.min.x, bounds.min.y);
            _width = Mathf.CeilToInt(bounds.size.x / _cellSize) + 1;
            _height = Mathf.CeilToInt(bounds.size.y / _cellSize) + 1;
            if (_width < 2 || _height < 2 || _width * _height > 100000) return false;

            var depths = new float[_width * _height];
            var interiorDepths = new List<float>(_width * _height);
            var rayStart = bounds.min.z - 1f;
            var rayLength = (bounds.size.z + 2f) * board.TransformVector(Vector3.forward).magnitude;
            var rayDirection = board.TransformDirection(Vector3.forward).normalized;
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                var local = CellPosition(x, y);
                var world = board.TransformPoint(new Vector3(local.x, local.y, rayStart));
                RaycastHit hit;
                int index = y * _width + x;
                depths[index] = _mazeCollider.Raycast(new Ray(world, rayDirection), out hit, rayLength)
                    ? board.InverseTransformPoint(hit.point).z : float.NegativeInfinity;
                // The backing board and decorative rim can contain a few points
                // deeper than the carved channels. Exclude the rim and use a high
                // percentile so one such point cannot close narrow passages.
                if (!float.IsNegativeInfinity(depths[index]) && x >= _width / 10 && x < _width * 9 / 10 &&
                    y >= _height / 10 && y < _height * 9 / 10)
                    interiorDepths.Add(depths[index]);
            }
            if (interiorDepths.Count == 0) return false;
            interiorDepths.Sort();
            float deepest = interiorDepths[Mathf.FloorToInt((interiorDepths.Count - 1) * .99f)];

            var candidate = new bool[depths.Length];
            for (int i = 0; i < candidate.Length; i++) candidate[i] = depths[i] >= deepest - _channelDepthTolerance;
            // Discard isolated recesses and select the connected route that spans
            // the greatest vertical distance. This also excludes decorative pits.
            var labels = new int[candidate.Length];
            var queue = new int[candidate.Length];
            int bestLabel = 0, bestSpan = -1, bestSize = 0, label = 0;
            for (int i = 0; i < candidate.Length; i++)
            {
                if (!candidate[i] || labels[i] != 0) continue;
                label++;
                int head = 0, tail = 0, low = _height, high = 0;
                queue[tail++] = i;
                labels[i] = label;
                while (head < tail)
                {
                    int cell = queue[head++];
                    int x = cell % _width, y = cell / _width;
                    low = Mathf.Min(low, y); high = Mathf.Max(high, y);
                    Visit(x - 1, y, label, candidate, labels, queue, ref tail);
                    Visit(x + 1, y, label, candidate, labels, queue, ref tail);
                    Visit(x, y - 1, label, candidate, labels, queue, ref tail);
                    Visit(x, y + 1, label, candidate, labels, queue, ref tail);
                }
                int span = high - low;
                if (span > bestSpan || span == bestSpan && tail > bestSize)
                { bestLabel = label; bestSpan = span; bestSize = tail; }
            }
            if (bestSize < 2) return false;
            _walkable = new bool[candidate.Length];
            for (int i = 0; i < labels.Length; i++) _walkable[i] = labels[i] == bestLabel;
            return true;
        }

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

        private void Visit(int x, int y, int label, bool[] candidate, int[] labels, int[] queue, ref int tail)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height) return;
            int index = y * _width + x;
            if (!candidate[index] || labels[index] != 0) return;
            labels[index] = label;
            queue[tail++] = index;
        }

        private Vector2 CellPosition(int x, int y) => _origin + new Vector2(x * _cellSize, y * _cellSize);
        private bool CanOccupy(Vector2 point)
        {
            if (_walkable == null) return false;
            return IsWalkable(point);
        }

        private bool IsWalkable(Vector2 point)
        {
            int x = Mathf.RoundToInt((point.x - _origin.x) / _cellSize);
            int y = Mathf.RoundToInt((point.y - _origin.y) / _cellSize);
            return x >= 0 && y >= 0 && x < _width && y < _height && _walkable[y * _width + x];
        }

        // Measure the actual traversable cross-section, then center the visual water in it.
        // Width is returned in world units for the LineRenderer; center remains board-local.
        public float ChannelSection(Vector2 point, Vector2 travel, float maxWorldWidth,
            float edgeInsetWorld, out Vector2 center)
        {
            center = point;
            if (_walkable == null || _board == null || !IsWalkable(point)) return 0;
            var forward = travel.sqrMagnitude > .000001f ? travel.normalized : Vector2.up;
            var across = new Vector2(-forward.y, forward.x);
            float worldPerLocal = _board.TransformVector(new Vector3(across.x, across.y, 0)).magnitude;
            if (worldPerLocal < .000001f) return 0;
            float step = _cellSize * .25f;
            float limit = Mathf.Max(step, maxWorldWidth / worldPerLocal * .5f + _cellSize);
            float left = ScanChannel(point, -across, step, limit);
            float right = ScanChannel(point, across, step, limit);
            center = point + across * ((right - left) * .5f);
            float measured = (left + right + step) * worldPerLocal;
            return Mathf.Max(0, Mathf.Min(measured, maxWorldWidth) - 2f * edgeInsetWorld);
        }

        private float ScanChannel(Vector2 start, Vector2 direction, float step, float limit)
        {
            float distance = 0;
            while (distance + step <= limit && IsWalkable(start + direction * (distance + step)))
                distance += step;
            return distance;
        }

        public Vector2 Snap(Vector2 point)
        {
            if (_walkable == null) return point;
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
            int steps = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / (_cellSize * .35f)));
            var step = delta / steps;
            for (int i = 0; i < steps; i++)
            {
                if (CanOccupy(current + step)) current += step;
                else
                {
                    var before = current;
                    if (CanOccupy(current + new Vector2(step.x, 0))) current.x += step.x;
                    if (CanOccupy(current + new Vector2(0, step.y))) current.y += step.y;
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
                            if (correction.sqrMagnitude > .00000001f && CanOccupy(current + correction))
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
            int first = CellIndex(Snap(start));
            int last = CellIndex(Snap(goal));
            if (first < 0 || last < 0) return false;
            var previous = new int[_walkable.Length];
            for (int i = 0; i < previous.Length; i++) previous[i] = -1;
            var queue = new int[_walkable.Length];
            int head = 0, tail = 0;
            queue[tail++] = first;
            previous[first] = first;
            while (head < tail && previous[last] < 0)
            {
                int cell = queue[head++];
                int x = cell % _width, y = cell / _width;
                PathVisit(x - 1, y, cell, previous, queue, ref tail);
                PathVisit(x + 1, y, cell, previous, queue, ref tail);
                PathVisit(x, y - 1, cell, previous, queue, ref tail);
                PathVisit(x, y + 1, cell, previous, queue, ref tail);
            }
            if (previous[last] < 0) return false;
            for (int cell = last; cell != first; cell = previous[cell])
                result.Add(CellPosition(cell % _width, cell / _width));
            result.Add(CellPosition(first % _width, first / _width));
            result.Reverse();
            return true;
        }

        private int CellIndex(Vector2 point)
        {
            int x = Mathf.RoundToInt((point.x - _origin.x) / _cellSize);
            int y = Mathf.RoundToInt((point.y - _origin.y) / _cellSize);
            return x >= 0 && y >= 0 && x < _width && y < _height ? y * _width + x : -1;
        }

        private void PathVisit(int x, int y, int from, int[] previous, int[] queue, ref int tail)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height) return;
            int index = y * _width + x;
            if (previous[index] >= 0 || !CanOccupy(CellPosition(x, y))) return;
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
