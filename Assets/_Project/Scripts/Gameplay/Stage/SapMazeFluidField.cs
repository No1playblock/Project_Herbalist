using System;
using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Finite-volume flow: cells own mass, never a record of where a head travelled.
    // Fluxes are computed together and limited by both donor mass and receiver room.
    public sealed class SapMazeFluidField
    {
        public const int SnapshotCapacity = 8192;
        private readonly Func<int, int, int> _neighbour;
        private readonly Func<int, Vector2> _point;
        private readonly float[] _mass, _incoming;
        private readonly int[] _distance, _queue;
        private readonly bool[] _listed;
        private readonly List<int> _active = new List<int>();
        private readonly List<Flux> _flux = new List<Flux>();
        private readonly HashSet<int> _reserved = new HashSet<int>();
        private int _potentialTarget = -1;
        private bool _flowPotential;
        private struct Flux { public int From, To; public float Amount; }
        public IReadOnlyList<int> ActiveCells => _active;
        public float CellSize { get; }
        public float CellCapacity { get; }
        public float TotalVolume { get; private set; }
        public int Width { get; }
        public int Count => _mass.Length;
        public float OccupancyThreshold { get; set; } = .04f;
        public float Mass(int cell) => cell >= 0 && cell < Count ? _mass[cell] : 0;
        public Vector2 Point(int cell) => _point(cell);

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
            _mass = new float[count]; _incoming = new float[count];
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

        public void Drain(Vector2 point, float amount)
        {
            amount = Mathf.Min(amount, TotalVolume);
            float remainingVolume = TotalVolume - amount;
            while (amount > .000001f)
            {
                int best = -1; float distance = float.PositiveInfinity;
                foreach (int cell in _active)
                {
                    if (_mass[cell] <= 0) continue;
                    float d = (_point(cell) - point).sqrMagnitude;
                    if (d < distance) { best = cell; distance = d; }
                }
                if (best < 0) break;
                float take = Mathf.Min(amount, _mass[best]);
                _mass[best] -= take; amount -= take;
            }
            TotalVolume = remainingVolume;
        }

        public void Step(int target, float speed, float dt, SapMazeTrailSettings settings)
        {
            if (target < 0 || dt <= 0 || TotalVolume <= 0) return;
            if (!_flowPotential || _potentialTarget < 0 || Vector2.Distance(_point(target), _point(_potentialTarget)) >=
                CellSize * settings.potentialRefreshDistance)
                BuildPotential(target, 0, Mathf.CeilToInt(settings.pressureCells));
            int steps = Mathf.Max(1, Mathf.CeilToInt(dt / Mathf.Min(settings.simulationStep,
                CellSize / Mathf.Max(.001f, speed) * .25f / Mathf.Max(1, settings.pressureCells))));
            float rate = speed * (dt / steps) / CellSize;
            for (int step = 0; step < steps; step++)
            {
                _flux.Clear(); _reserved.Clear();
                foreach (int cell in _active)
                {
                    if (_mass[cell] <= 0 || _distance[cell] < 0) continue;
                    float potential = _distance[cell] + settings.pressureCells * _mass[cell] / CellCapacity;
                    float weight = 0;
                    for (int d = 0; d < 4; d++)
                    {
                        int next = _neighbour(cell, d);
                        if (next < 0 || _distance[next] < 0) continue;
                        weight += Mathf.Max(0, potential - _distance[next] - settings.pressureCells * _mass[next] / CellCapacity);
                    }
                    if (weight <= 0) continue;
                    float budget = Mathf.Min(_mass[cell], CellCapacity * rate * Mathf.Min(1, weight));
                    for (int d = 0; d < 4; d++)
                    {
                        int next = _neighbour(cell, d);
                        if (next < 0 || _distance[next] < 0) continue;
                        float gradient = Mathf.Max(0, potential - _distance[next] - settings.pressureCells * _mass[next] / CellCapacity);
                        float amount = budget * gradient / weight;
                        if (amount <= 0) continue;
                        if (!_listed[next] && !_reserved.Contains(next))
                        {
                            if (_active.Count + _reserved.Count >= SnapshotCapacity) continue;
                            _reserved.Add(next);
                        }
                        _flux.Add(new Flux { From = cell, To = next, Amount = amount });
                        _incoming[next] += amount;
                    }
                }
                // Resolve capacity from the original state before applying any transfer.
                for (int i = 0; i < _flux.Count; i++)
                {
                    var flow = _flux[i];
                    flow.Amount *= Mathf.Min(1, Mathf.Max(0, CellCapacity - _mass[flow.To]) / _incoming[flow.To]);
                    _flux[i] = flow;
                }
                foreach (var flow in _flux)
                {
                    _mass[flow.From] -= flow.Amount; Add(flow.To, flow.Amount);
                }
                foreach (var flow in _flux) _incoming[flow.To] = 0;
                for (int i = _active.Count - 1; i >= 0; i--)
                    if (_mass[_active[i]] <= 0) { _listed[_active[i]] = false; _active.RemoveAt(i); }
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
