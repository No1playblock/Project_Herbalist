using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Point-sized liquid follows open recesses. The mesh, rather than the visual
    // water width or a selected connected component, determines solid barriers.
    public sealed class SapMazeSurface : MonoBehaviour
    {
        [SerializeField] private MeshCollider _mazeCollider;
        [Tooltip("Authored unit XY quad. Its transform defines the liquid plane and rectangular bounds; no collider is required.")]
        [SerializeField] private Transform _floorPlane;
        [Tooltip("Baked wall silhouettes extruded across the liquid plane. When assigned, the decorative relief no longer blocks water.")]
        [SerializeField] private MeshCollider _waterWallCollider;
        [Tooltip("Editor wall bake: height slice from wall crest (0) towards channel floor (1). Does not change the water plane.")]
        [SerializeField, Range(.1f, .7f)] private float _wallBakeDepthFraction = .6f;
        [SerializeField, Min(0)] private float _wallBakePaddingWorld = .01f;
        [Tooltip("Wall bake resolution independent of the runtime fluid grid. Fine samples preserve thin walls.")]
        [SerializeField, Range(.005f, .04f)] private float _wallBakeSampleSize = .01f;
        [SerializeField, Min(256)] private int _pointQueryCacheLimit = 32768;
        [SerializeField, Range(.005f, .25f)] private float _cellSize = .04f;
        [SerializeField, Min(10000)] private int _maximumSamples = 1000000;
        [Tooltip("Depth below the front wall band; shallow openings do not need to reach the deepest floor.")]
        [SerializeField, Range(.01f, .3f)] private float _channelDepthTolerance = .08f;
        [Tooltip("Navigation slice between the front wall band (0) and channel floor (1). Calibrate to the relief mesh.")]
        [SerializeField, Range(0, 1)] private float _channelDepthFraction = .5f;
        [SerializeField, Range(0, .2f)] private float _visualOffset = .04f;
        [Tooltip("Maximum floor slope relative to the maze face. Steeper relief is a wall, never a liquid surface.")]
        [SerializeField, Range(0, 80)] private float _maximumFloorSlope = 45f;

        private bool[] _walkable;
        private int _width, _height;
        private Vector2 _origin;
        private Transform _board;
        private float _sampleSize, _passageZ, _rayStartZ, _rayLength;
        private Vector3 _rayDirection;
        private int[] _routeDistance, _routeQueue;
        private int _routeRoot = -1, _routeHead, _routeTail;
        private readonly SapMazeNearestCells _nearest = new SapMazeNearestCells();
        private readonly List<Vector2> _cachedPath = new List<Vector2>();
        private Vector2 _cachedStart, _cachedGoal;
        private bool _hasCachedPath, _cachedPathFound;
        private readonly Dictionary<Vector2, bool> _pointQueries = new Dictionary<Vector2, bool>();
        private readonly Queue<Vector2> _pointQueryOrder = new Queue<Vector2>();
        private bool _cachePointQueries;
        private byte[] _checkedEdges, _openEdges;
        private Mesh _builtMesh;
        private Matrix4x4 _builtBoardMatrix, _builtMeshMatrix;
        private float _builtCellSize, _builtDepthTolerance, _builtDepthFraction;
        private float _builtFloorSlope;
        private Matrix4x4 _builtPlaneMatrix;
        private Transform _builtPlane;
        private Vector3 _planePoint, _planeNormal;
        private Mesh _builtWaterWallMesh;
        private Matrix4x4 _builtWaterWallMatrix;

        public Transform FloorPlane => _floorPlane;
        public bool UsesFloorPlane => _floorPlane != null;
        public MeshCollider WaterWallCollider => _waterWallCollider;
        private bool UsesWaterWalls => UsesFloorPlane && _waterWallCollider != null && _waterWallCollider.sharedMesh != null;
        public int GeometryVersion { get; private set; }

        private float PlaneDepth(Vector2 point) => _planePoint.z -
            (_planeNormal.x * (point.x - _planePoint.x) + _planeNormal.y * (point.y - _planePoint.y)) / _planeNormal.z;

        private bool InsidePlane(Vector2 point)
        {
            var local = _floorPlane.InverseTransformPoint(_board.TransformPoint(new Vector3(point.x, point.y, PlaneDepth(point))));
            return Mathf.Abs(local.x) <= .5f && Mathf.Abs(local.y) <= .5f;
        }

        public MeshCollider MazeCollider => _mazeCollider;
        public bool Ready => _walkable != null;
        public float CellSize => _sampleSize > 0 ? _sampleSize : _cellSize;
        public int GridWidth => _width;
        public int GridCount => _walkable != null ? _walkable.Length : 0;
        public Vector2 GridPoint(int index) => CellPosition(index % _width, index / _width);
        public bool GridOpen(int index) => index >= 0 && index < GridCount && _walkable[index];
        public int GridCell(Vector2 point) => NearestReachableCell(point);
        public bool IsSurfaceOpen(Vector2 point) => Ready && OpenPoint(point);
        public bool IsSegmentOpen(Vector2 from, Vector2 to) => Ready && ClearSegment(from, to);

        // All fluid transfers use the same mesh-tested edges as navigation.
        public int GridNeighbour(int from, int direction)
        {
            int x = from % _width, y = from / _width;
            int nx = x + (direction == 0 ? -1 : direction == 1 ? 1 : 0);
            int ny = y + (direction == 2 ? -1 : direction == 3 ? 1 : 0);
            if (nx < 0 || ny < 0 || nx >= _width || ny >= _height) return -1;
            int index = ny * _width + nx;
            if (!GridOpen(index)) return -1;
            byte bit = (byte)(1 << direction);
            byte reverse = (byte)(1 << (direction ^ 1));
            if ((_checkedEdges[from] & bit) == 0)
            {
                _checkedEdges[from] |= bit; _checkedEdges[index] |= reverse;
                if (ClearSegment(GridPoint(from), GridPoint(index)))
                { _openEdges[from] |= bit; _openEdges[index] |= reverse; }
            }
            return (_openEdges[from] & bit) != 0 ? index : -1;
        }

        public bool Build(Transform board)
        {
            if (_walkable != null && _mazeCollider != null && _builtMesh == _mazeCollider.sharedMesh &&
                _builtBoardMatrix == board.localToWorldMatrix && _builtMeshMatrix == _mazeCollider.transform.localToWorldMatrix &&
                _builtCellSize == _cellSize && _builtDepthTolerance == _channelDepthTolerance &&
                _builtDepthFraction == _channelDepthFraction && _builtFloorSlope == _maximumFloorSlope &&
                _builtWaterWallMesh == (_waterWallCollider != null ? _waterWallCollider.sharedMesh : null) &&
                (_waterWallCollider == null || _builtWaterWallMatrix == _waterWallCollider.transform.localToWorldMatrix) &&
                _builtPlane == _floorPlane && (_floorPlane == null || _builtPlaneMatrix == _floorPlane.localToWorldMatrix)) return true;
            _board = board;
            _walkable = null;
            _cachePointQueries = false; _pointQueries.Clear(); _pointQueryOrder.Clear();
            if (_mazeCollider == null || _mazeCollider.sharedMesh == null) return false;
            if (_floorPlane != null)
            {
                _planePoint = board.InverseTransformPoint(_floorPlane.position);
                _planeNormal = board.localToWorldMatrix.transpose.MultiplyVector(_floorPlane.forward).normalized;
                if (Mathf.Abs(_planeNormal.z) < .001f) return false;
            }
            var bounds = LocalBounds();
            _origin = new Vector2(bounds.min.x, bounds.min.y);
            _sampleSize = Mathf.Max(.005f, _cellSize);
            _width = Mathf.CeilToInt(bounds.size.x / _sampleSize) + 1;
            _height = Mathf.CeilToInt(bounds.size.y / _sampleSize) + 1;
            if (_width < 2 || _height < 2 || (long)_width * _height > _maximumSamples) return false;

            int sampleCount = _width * _height;
            var depths = UsesWaterWalls ? null : new float[sampleCount];
            var floors = UsesWaterWalls ? null : new bool[sampleCount];
            var interiorDepths = UsesWaterWalls ? null : new List<float>(sampleCount);
            // Start strictly outside the extruded proxy, never on its front cap.
            // PhysX rays starting on/inside a collider can miss that collider.
            _rayStartZ = bounds.min.z - 2f;
            _rayLength = (bounds.size.z + 4f) * board.TransformVector(Vector3.forward).magnitude;
            _rayDirection = board.TransformDirection(Vector3.forward).normalized;
            if (!UsesWaterWalls)
            {
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                var local = CellPosition(x, y);
                var world = board.TransformPoint(new Vector3(local.x, local.y, _rayStartZ));
                RaycastHit hit;
                int index = y * _width + x;
                bool found = _mazeCollider.Raycast(new Ray(world, _rayDirection), out hit, _rayLength);
                depths[index] = found ? board.InverseTransformPoint(hit.point).z : float.NegativeInfinity;
                floors[index] = found && IsFloorNormal(hit.normal);
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
            }
            _walkable = new bool[sampleCount];
            int count = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                var point = GridPoint(i);
                bool open = UsesWaterWalls ? OpenPoint(point) : UsesFloorPlane ? InsidePlane(point) && depths[i] > PlaneDepth(point) :
                    depths[i] > _passageZ && floors[i];
                if (open) { _walkable[i] = true; count++; }
            }
            if (count < 2) { _walkable = null; return false; }
            _nearest.Build(_walkable, GridPoint);
            _routeDistance = new int[sampleCount]; _routeQueue = new int[sampleCount];
            _routeRoot = -1; _hasCachedPath = false;
            _checkedEdges = new byte[sampleCount]; _openEdges = new byte[sampleCount];
            _builtMesh = _mazeCollider.sharedMesh;
            _builtBoardMatrix = board.localToWorldMatrix; _builtMeshMatrix = _mazeCollider.transform.localToWorldMatrix;
            _builtCellSize = _cellSize; _builtDepthTolerance = _channelDepthTolerance;
            _builtDepthFraction = _channelDepthFraction;
            _builtFloorSlope = _maximumFloorSlope;
            _builtPlane = _floorPlane;
            _builtPlaneMatrix = _floorPlane != null ? _floorPlane.localToWorldMatrix : Matrix4x4.identity;
            _builtWaterWallMesh = _waterWallCollider != null ? _waterWallCollider.sharedMesh : null;
            _builtWaterWallMatrix = _waterWallCollider != null ? _waterWallCollider.transform.localToWorldMatrix : Matrix4x4.identity;
            GeometryVersion++;
            _cachePointQueries = true;
            return true;
        }
        public void InvalidateGeometry()
        {
            _walkable = null; _sampleSize = 0f; _cachePointQueries = false;
            _pointQueries.Clear(); _pointQueryOrder.Clear(); _hasCachedPath = false; _routeRoot = -1;
        }
        private void OnValidate() => InvalidateGeometry();

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

        private bool IsFloorNormal(Vector3 normal) =>
            Vector3.Dot(normal, -_rayDirection) >= Mathf.Cos(_maximumFloorSlope * Mathf.Deg2Rad);

        private bool OpenPoint(Vector2 point)
        {
            if (_cachePointQueries && _pointQueries.TryGetValue(point, out bool open)) return open;
            open = QueryOpenPoint(point);
            if (_cachePointQueries)
            {
                if (_pointQueries.Count >= Mathf.Max(256, _pointQueryCacheLimit)) _pointQueries.Remove(_pointQueryOrder.Dequeue());
                _pointQueries.Add(point, open); _pointQueryOrder.Enqueue(point);
            }
            return open;
        }

        private bool QueryOpenPoint(Vector2 point)
        {
            if (UsesFloorPlane && !InsidePlane(point)) return false;
            var start = _board.TransformPoint(new Vector3(point.x, point.y, _rayStartZ));
            if (UsesWaterWalls) return !_waterWallCollider.Raycast(new Ray(start, _rayDirection), out _, _rayLength);
            return _mazeCollider.Raycast(new Ray(start, _rayDirection), out var hit, _rayLength) &&
                (UsesFloorPlane ? _board.InverseTransformPoint(hit.point).z > PlaneDepth(point) :
                _board.InverseTransformPoint(hit.point).z > _passageZ && IsFloorNormal(hit.normal));
        }
        private bool ClearSegment(Vector2 from, Vector2 to)
        {
            // Reject covered points and intersect the wall at the same plane used by rendering.
            int samples = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / (CellSize * .5f)));
            for (int i = 0; i <= samples; i++)
                if (!OpenPoint(Vector2.Lerp(from, to, (float)i / samples))) return false;
            var start = _board.TransformPoint(new Vector3(from.x, from.y, UsesFloorPlane ? PlaneDepth(from) : _passageZ));
            var end = _board.TransformPoint(new Vector3(to.x, to.y, UsesFloorPlane ? PlaneDepth(to) : _passageZ));
            var delta = end - start;
            if (delta.sqrMagnitude < .00000001f) return true;
            float distance = Mathf.Max(0f, delta.magnitude - .00001f);
            var walls = UsesWaterWalls ? _waterWallCollider : _mazeCollider;
            return !walls.Raycast(new Ray(start, delta.normalized), out _, distance) &&
                !walls.Raycast(new Ray(end, -delta.normalized), out _, distance);
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
            int best = _nearest.Find(point);
            return best >= 0 ? GridPoint(best) : point;
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
            if (!_hasCachedPath || !_cachedStart.Equals(start) || !_cachedGoal.Equals(goal))
            {
                _cachedPath.Clear();
                _cachedPathFound = FindPath(start, goal, _cachedPath);
                _cachedStart = start; _cachedGoal = goal; _hasCachedPath = true;
            }
            result.AddRange(_cachedPath);
            return _cachedPathFound;
        }

        private bool FindPath(Vector2 start, Vector2 goal, List<Vector2> result)
        {
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
            // Reuse a goal-rooted distance tree as the water's starting point changes.
            // Greedy L/R/D/U descent yields the same tie-break as the original forward BFS.
            if (_routeRoot != last)
            {
                System.Array.Fill(_routeDistance, -1);
                _routeRoot = last; _routeHead = 0; _routeTail = 1;
                _routeQueue[0] = last; _routeDistance[last] = 0;
            }
            while (_routeHead < _routeTail && _routeDistance[first] < 0)
            {
                int cell = _routeQueue[_routeHead++];
                for (int direction = 0; direction < 4; direction++)
                {
                    int next = GridNeighbour(cell, direction);
                    if (next < 0 || _routeDistance[next] >= 0) continue;
                    _routeDistance[next] = _routeDistance[cell] + 1;
                    _routeQueue[_routeTail++] = next;
                }
            }
            if (_routeDistance[first] < 0) return false;
            for (int cell = first;;)
            {
                result.Add(GridPoint(cell));
                if (cell == last) break;
                for (int direction = 0; direction < 4; direction++)
                {
                    int next = GridNeighbour(cell, direction);
                    if (next < 0 || _routeDistance[next] != _routeDistance[cell] - 1) continue;
                    cell = next; break;
                }
            }
            var exactGoal = Snap(goal);
            if ((exactGoal - result[result.Count - 1]).sqrMagnitude > .00000001f) result.Add(exactGoal);
            return true;
        }

        private int NearestReachableCell(Vector2 point)
        {
            point = Snap(point);
            int cell = CellIndex(point);
            if (cell >= 0 && _walkable[cell] && ClearSegment(point, CellPosition(cell % _width, cell / _width))) return cell;
            return _nearest.Find(point, candidate => ClearSegment(point, GridPoint(candidate)));
        }

        private int CellIndex(Vector2 point)
        {
            int x = Mathf.RoundToInt((point.x - _origin.x) / CellSize);
            int y = Mathf.RoundToInt((point.y - _origin.y) / CellSize);
            return x >= 0 && y >= 0 && x < _width && y < _height ? y * _width + x : -1;
        }


        public float SurfaceZ(Vector2 point, float fallback)
        {
            if (_walkable == null) return fallback;
            if (UsesFloorPlane) return PlaneDepth(point);
            var world = _board.TransformPoint(new Vector3(point.x, point.y, _rayStartZ));
            RaycastHit hit;
            return _mazeCollider.Raycast(new Ray(world, _rayDirection), out hit, _rayLength) &&
                _board.InverseTransformPoint(hit.point).z > _passageZ && IsFloorNormal(hit.normal)
                ? _board.InverseTransformPoint(hit.point - _rayDirection * _visualOffset).z : fallback;
        }

        // Rendering only. The authored plane and dedicated wall graph remain the gameplay surface.
        public float ReliefSurfaceZ(Vector2 point, float fallback)
        {
            if (!Ready || _board == null || _mazeCollider == null) return fallback;
            var world = _board.TransformPoint(new Vector3(point.x, point.y, _rayStartZ));
            return _mazeCollider.Raycast(new Ray(world, _rayDirection), out var hit, _rayLength)
                ? _board.InverseTransformPoint(hit.point).z : SurfaceZ(point, fallback);
        }
    }
}
