using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Display-only continuous density. Diffusion follows open graph edges, not XY proximity.
    public sealed class SapMazeFluidOutline
    {
        private float[] _values = System.Array.Empty<float>();
        private float[] _scratch = System.Array.Empty<float>();
        private bool[] _listed = System.Array.Empty<bool>();
        private int[] _adjacent = System.Array.Empty<int>();
        private readonly List<int> _cells = new List<int>();
        public IReadOnlyList<int> Cells => _cells;
        public float Maximum { get; private set; }
        public float Value(int cell) => cell >= 0 && cell < _values.Length ? _values[cell] : 0;

        public void Build(SapMazeFluidField field, float radiusLocal, int maximumPasses)
        {
            if (_values.Length != field.Count)
            {
                _values = new float[field.Count]; _scratch = new float[field.Count]; _listed = new bool[field.Count];
            }
            else foreach (int cell in _cells) { _values[cell] = 0; _scratch[cell] = 0; _listed[cell] = false; }
            _cells.Clear(); Maximum = 0;
            foreach (int cell in field.ActiveCells)
            {
                if (field.Mass(cell) <= 0) continue;
                _listed[cell] = true; _cells.Add(cell);
                _values[cell] = Mathf.Clamp01(field.Mass(cell) / field.CellCapacity);
            }
            float radiusCells = Mathf.Min(radiusLocal / field.CellSize,
                Mathf.Sqrt(field.TotalVolume / field.CellCapacity) * .5f);
            int passes = Mathf.Clamp(Mathf.CeilToInt(radiusCells * radiusCells), 0, maximumPasses);
            int head = 0, end = _cells.Count;
            for (int ring = 0; ring < Mathf.CeilToInt(radiusCells) + 2; ring++)
            {
                for (; head < end; head++)
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int next = field.Neighbour(_cells[head], direction);
                        if (next >= 0 && !_listed[next]) { _listed[next] = true; _cells.Add(next); }
                    }
                end = _cells.Count;
            }
            if (_adjacent.Length < _cells.Count * 4) _adjacent = new int[Mathf.NextPowerOfTwo(_cells.Count * 4)];
            for (int i = 0; i < _cells.Count; i++)
                for (int direction = 0; direction < 4; direction++) _adjacent[i * 4 + direction] = field.Neighbour(_cells[i], direction);
            for (int pass = 0; pass < passes; pass++)
            {
                for (int i = 0; i < _cells.Count; i++)
                {
                    int cell = _cells[i];
                    float center = _values[cell], value = center * .5f;
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int next = _adjacent[i * 4 + direction];
                        value += (next < 0 ? center : _values[next]) * .125f;
                    }
                    _scratch[cell] = value;
                }
                var previous = _values; _values = _scratch; _scratch = previous;
            }
            foreach (int cell in _cells) Maximum = Mathf.Max(Maximum, Value(cell));
        }

        public float Sample(SapMazeFluidField field, float x, float y)
        {
            int nearestX = Mathf.RoundToInt(x), nearestY = Mathf.RoundToInt(y);
            int nearest = nearestY * field.Width + nearestX;
            if (nearestX < 0 || nearestX >= field.Width || nearest < 0 || nearest >= _listed.Length || !_listed[nearest]) return 0;
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
            float tx = x - ix, ty = y - iy;
            float a = ix < 0 || ix >= field.Width ? 0 : Value(iy * field.Width + ix);
            float b = ix + 1 < 0 || ix + 1 >= field.Width ? 0 : Value(iy * field.Width + ix + 1);
            float c = ix < 0 || ix >= field.Width ? 0 : Value((iy + 1) * field.Width + ix);
            float d = ix + 1 < 0 || ix + 1 >= field.Width ? 0 : Value((iy + 1) * field.Width + ix + 1);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }
    }
}
