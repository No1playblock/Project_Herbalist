using System;
using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Finite-volume flow: cells own mass, never a record of where a head travelled.
    // Pulling moves a bounded amount from the far boundary into the advancing wet boundary.
    public sealed class SapMazeFluidField
    {
        // Leave room for the board's other network fields under Fusion's 32 KiB object limit.
        // Authored mazes hold at most 220 volume (2750 full cells), well below this ceiling.
        public const int SnapshotCapacity = 7680;
        private readonly Func<int, int, int> _neighbour;
        private readonly Func<int, Vector2> _point;
        private readonly float[] _mass;
        private readonly int[] _distance, _queue;
        private readonly bool[] _listed;
        private readonly List<int> _active = new List<int>();
        private readonly System.Collections.Generic.Dictionary<int, int> _channelWidths = new System.Collections.Generic.Dictionary<int, int>();
        private int _channelWidthLimit;
        private int _potentialTarget = -1;
        private bool _flowPotential;
        public IReadOnlyList<int> ActiveCells => _active;
        public float CellSize { get; }
        public float CellCapacity { get; }
        public float TotalVolume { get; private set; }
        public int Width { get; }
        public int Count => _mass.Length;
        public float OccupancyThreshold { get; set; } = .04f;
        public float WorldScale { get; set; } = 1f;
        public float Mass(int cell) => cell >= 0 && cell < Count ? _mass[cell] : 0;
        public Vector2 Point(int cell) => _point(cell);
        public int Neighbour(int cell, int direction) => _neighbour(cell, direction);

        public SapMazeFluidField(SapMazeSurface surface, float volumePerArea)
            : this(surface.GridCount, surface.GridWidth, surface.CellSize, volumePerArea,
                surface.GridNeighbour, surface.GridPoint) { }

        // Graph injection also allows deterministic cross/U-bend tests without a scene.
        public SapMazeFluidField(int count, int width, float cellSize, float volumePerArea,
            Func<int, int, int> neighbour, Func<int, Vector2> point)
        {
            Width = width; CellSize = cellSize;
            CellCapacity = cellSize * cellSize * Mathf.Max(1f, volumePerArea);
            _neighbour = neighbour; _point = point;
            _mass = new float[count];
            _distance = new int[count]; _queue = new int[count]; _listed = new bool[count];
        }

        public void Reset(int start, float volume)
        {
            foreach (var cell in _active) { _mass[cell] = 0; _listed[cell] = false; }
            _active.Clear(); TotalVolume = 0; _potentialTarget = -1;
            SetVolume(volume, start);
        }

        private void Add(int cell, float amount)
        {
            if (amount <= 0) return;
            if (!_listed[cell]) { _listed[cell] = true; _active.Add(cell); }
            _mass[cell] += amount;
        }

        public void SetVolume(float volume, int source)
        {
            volume = Mathf.Max(0, volume);
            if (volume < TotalVolume && TotalVolume > 0)
            {
                float scale = volume / TotalVolume;
                foreach (var cell in _active) _mass[cell] *= scale;
            }
            else if (volume > TotalVolume && source >= 0)
            {
                float remaining = volume - TotalVolume;
                BuildPotential(source, remaining, 0);
                // Fill nearest reachable cells, including after a refill. No mass is discarded.
                for (int i = 0; i < _visited && remaining > 0; i++)
                {
                    int cell = _queue[i];
                    float add = Mathf.Min(remaining, Mathf.Max(0, CellCapacity - _mass[cell]));
                    if (!_listed[cell] && _active.Count >= SnapshotCapacity) continue;
                    Add(cell, add); remaining -= add;
                }
                if (remaining > 0) Add(source, remaining);
            }
            PruneEmpty();
            TotalVolume = volume;
        }

        private int _visited;
        private void BuildPotential(int target, float fillVolume, int margin)
        {
            Array.Fill(_distance, -1);
            int head = 0, tail = 0;
            int needed = _active.Count, finishDistance = int.MaxValue;
            float room = 0;
            _queue[tail++] = target; _distance[target] = 0;
            while (head < tail)
            {
                int cell = _queue[head++];
                if (_distance[cell] > finishDistance) break;
                if (_listed[cell]) needed--;
                room += Mathf.Max(0, CellCapacity - _mass[cell]);
                if (needed <= 0 && room >= fillVolume && finishDistance == int.MaxValue)
                    finishDistance = _distance[cell] + margin;
                for (int d = 0; d < 4; d++)
                {
                    int next = _neighbour(cell, d);
                    if (next < 0 || _distance[next] >= 0) continue;
                    _distance[next] = _distance[cell] + 1;
                    _queue[tail++] = next;
                }
            }
            _visited = tail; _potentialTarget = target; _flowPotential = margin > 0;
        }

        public int NearestWet(Vector2 point, float radius = float.PositiveInfinity)
        {
            int best = -1; float distance = radius * radius;
            foreach (var cell in _active)
            {
                if (_mass[cell] < CellCapacity * OccupancyThreshold) continue;
                float delta = (_point(cell) - point).sqrMagnitude;
                if (delta <= distance) { best = cell; distance = delta; }
            }
            return best;
        }

        public float Distance(Vector2 point)
        {
            int cell = NearestWet(point);
            return cell >= 0 ? Vector2.Distance(point, _point(cell)) : float.PositiveInfinity;
        }

        // Contact is the occupied cell under the hole, not a nearby cell snapped across a wall.
        public bool Contains(Vector2 point) => WetCellAt(point) >= 0;

        private int WetCellAt(Vector2 point)
        {
            if (!float.IsFinite(point.x) || !float.IsFinite(point.y)) return -1;
            var grid = (point - _point(0)) / CellSize;
            int x = Mathf.RoundToInt(grid.x), y = Mathf.RoundToInt(grid.y);
            if (x < 0 || x >= Width || y < 0 || y >= Count / Width) return -1;
            // At a shared edge/corner either touching cell may supply water.
            // The tiny local-space tolerance only absorbs grid/transform round-off.
            float half = CellSize * .5f + .00001f;
            for (int cy = Mathf.Max(0, y - 1); cy <= Mathf.Min(Count / Width - 1, y + 1); cy++)
            for (int cx = Mathf.Max(0, x - 1); cx <= Mathf.Min(Width - 1, x + 1); cx++)
            {
                int cell = cy * Width + cx;
                if (_mass[cell] < CellCapacity * OccupancyThreshold) continue;
                var delta = point - _point(cell);
                if (Mathf.Abs(delta.x) <= half && Mathf.Abs(delta.y) <= half) return cell;
            }
            return -1;
        }

        public float Drain(Vector2 point, float amount)
        {
            int source = WetCellAt(point);
            if (source < 0 || !float.IsFinite(amount) || amount <= 0) return 0;
            amount = Mathf.Clamp(amount, 0, TotalVolume);
            bool emptyConnected = amount >= TotalVolume;
            // Connected water feeds the hole even while parked. Recede from the
            // far boundary so draining does not punch a dry cell under the hole.
            Array.Fill(_distance, -1);
            int head = 0, tail = 1;
            _queue[0] = source;
            _distance[source] = 0;
            while (head < tail)
            {
                int cell = _queue[head++];
                for (int direction = 0; direction < 4; direction++)
                {
                    int next = _neighbour(cell, direction);
                    if (next < 0 || _distance[next] >= 0 || _mass[next] <= 0) continue;
                    _distance[next] = _distance[cell] + 1;
                    _queue[tail++] = next;
                }
            }
            float drained = 0;
            for (int i = tail - 1; i >= 0 && (emptyConnected || amount > 0); i--)
            {
                int cell = _queue[i];
                float take = emptyConnected ? _mass[cell] : Mathf.Min(amount, _mass[cell]);
                _mass[cell] -= take;
                amount -= take;
                drained += take;
            }
            // The movement potential shares these scratch buffers.
            _flowPotential = false;
            _potentialTarget = -1;
            PruneEmpty();
            drained = Mathf.Min(drained, TotalVolume);
            TotalVolume = Mathf.Max(0, TotalVolume - drained);
            return drained;
        }

        private void PruneEmpty()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                int cell = _active[i];
                if (_mass[cell] > 0) continue;
                _listed[cell] = false;
                _active[i] = _active[_active.Count - 1];
                _active.RemoveAt(_active.Count - 1);
            }
        }

        private bool HasFlowWidth(int cell, int direction, int required, float density)
        {
            if (required <= 1) return true;
            int available = 1, wet = 1;
            // Test only the transverse section, never disqualify a genuinely narrow passage.
            for (int side = 0; side < 2; side++)
            {
                int current = cell;
                int transverse = direction < 2 ? 2 + side : side;
                bool continuous = true;
                for (int step = 1; step < required; step++)
                {
                    current = _neighbour(current, transverse);
                    if (current < 0) break;
                    available++;
                    continuous &= _mass[current] >= CellCapacity * density;
                    if (continuous) wet++;
                }
            }
            // At a narrow bend, waiting for every transverse cell to fill can deadlock:
            // those side cells need the very same width check before they can receive water.
            // Preserve the minimum in open space; let a physically narrower channel squeeze it.
            if (available <= required || wet >= required) return true;
            int next = _neighbour(cell, direction);
            return next >= 0 && ChannelWidth(next, direction, required) <= required;
        }

        private int ChannelWidth(int cell, int direction, int limit)
        {
            if (_channelWidthLimit != limit) { _channelWidths.Clear(); _channelWidthLimit = limit; }
            int key = cell * 2 + (direction < 2 ? 0 : 1);
            if (_channelWidths.TryGetValue(key, out int cached)) return cached;
            int width = 1;
            for (int side = 0; side < 2; side++)
            {
                int current = cell, transverse = direction < 2 ? 2 + side : side;
                for (int step = 1; step < limit; step++)
                {
                    current = _neighbour(current, transverse);
                    if (current < 0) break;
                    width++;
                }
            }
            _channelWidths[key] = width;
            return width;
        }

        public void Step(int target, float speed, float dt, SapMazeTrailSettings settings)
        {
            if (target < 0 || target >= Count || dt <= 0 || !float.IsFinite(dt) || TotalVolume <= 0) return;
            if (!_flowPotential || _potentialTarget < 0 || Vector2.Distance(_point(target), _point(_potentialTarget)) >=
                CellSize * settings.potentialRefreshDistance)
                BuildPotential(target, 0, Mathf.CeilToInt(settings.pressureCells));

            // Translate a bounded volume, rather than integrating tiny pressure steps.
            // A full interior transmits the pull: only its far boundary loses water.
            // New water may appear only beside existing water across an open mesh edge.
            float budget = Mathf.Max(0, speed) * dt / CellSize * CellCapacity * settings.pressureCells;
            int remainingTransfers = settings.maximumTransfersPerTick;
            int width = Mathf.Max(1, Mathf.CeilToInt(settings.minimumFlowWidthWorld /
                Mathf.Max(.001f, CellSize * WorldScale)));
            float density = Mathf.Min(settings.minimumFlowDensity, TotalVolume / CellCapacity);
            // Low water gathers as a smaller pool rather than requiring unavailable mass.
            width = Mathf.Min(width, Mathf.Max(1, Mathf.FloorToInt(Mathf.Sqrt(TotalVolume /
                (CellCapacity * settings.minimumFlowDensity)) * .7f)));
            while (budget > .000001f && remainingTransfers-- > 0)
            {
                int donor = -1, receiver = -1, farthest = -1;
                float best = float.PositiveInfinity;
                foreach (int cell in _active)
                {
                    if (_mass[cell] <= .000001f || _distance[cell] < 0) continue;
                    if (_distance[cell] > farthest || (_distance[cell] == farthest &&
                        (donor < 0 || _mass[cell] > _mass[donor])))
                    { donor = cell; farthest = _distance[cell]; }
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int next = _neighbour(cell, direction);
                        if (next < 0 || _distance[next] < 0 || _mass[next] >= CellCapacity - .000001f) continue;
                        if (!_listed[next] && _active.Count >= SnapshotCapacity) continue;
                        float score = _distance[next] + settings.pressureCells * _mass[next] / CellCapacity;
                        if (score >= best) continue;
                        if (_mass[cell] + .000001f < CellCapacity * density) continue;
                        if (!HasFlowWidth(cell, direction, width, density)) continue;
                        best = score; receiver = next;
                    }
                }
                if (donor < 0 || receiver < 0 || farthest <= _distance[receiver]) break;
                float amount = Mathf.Min(budget, Mathf.Min(_mass[donor], CellCapacity - _mass[receiver]));
                _mass[donor] -= amount; Add(receiver, amount); budget -= amount;
                if (_mass[donor] <= 0) { _listed[donor] = false; _active.Remove(donor); }
            }
        }

        // 20 bits identify the mesh grid cell; 12 bits encode density for presentation.
        // Host retains full precision; clients receive the occupied region, never history.
        public uint Pack(int index)
        {
            int cell = _active[index];
            uint density = (uint)Mathf.Clamp(Mathf.RoundToInt(_mass[cell] / CellCapacity * 4095), 1, 4095);
            return (uint)cell | density << 20;
        }
        public void ReadSnapshot(IReadOnlyList<uint> snapshot)
        {
            foreach (var cell in _active) { _mass[cell] = 0; _listed[cell] = false; }
            _active.Clear(); TotalVolume = 0;
            foreach (uint packed in snapshot)
            {
                int cell = (int)(packed & 0xfffff);
                if (cell >= Count) continue;
                float mass = (packed >> 20) / 4095f * CellCapacity;
                Add(cell, mass); TotalVolume += mass;
            }
        }
    }
}
