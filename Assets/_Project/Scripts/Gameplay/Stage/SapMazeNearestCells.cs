using System;
using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Static exact nearest-cell index. Equal distances retain the original row-major tie break.
    public sealed class SapMazeNearestCells
    {
        private int[] _cells = Array.Empty<int>();
        private Func<int, Vector2> _point;

        public void Build(IReadOnlyList<bool> open, Func<int, Vector2> point)
        {
            _point = point;
            var cells = new List<int>();
            for (int i = 0; i < open.Count; i++) if (open[i]) cells.Add(i);
            _cells = cells.ToArray();
            Split(0, _cells.Length - 1, 0);
        }

        private float Axis(int cell, int axis) => axis == 0 ? _point(cell).x : _point(cell).y;
        private int Compare(int a, int b, int axis)
        {
            int result = Axis(a, axis).CompareTo(Axis(b, axis));
            return result != 0 ? result : a.CompareTo(b);
        }
        private void Split(int first, int last, int axis)
        {
            if (first >= last) return;
            int middle = (first + last) / 2, left = first, right = last;
            while (left < right)
            {
                int pivot = _cells[(left + right) / 2], i = left, j = right;
                while (i <= j)
                {
                    while (Compare(_cells[i], pivot, axis) < 0) i++;
                    while (Compare(_cells[j], pivot, axis) > 0) j--;
                    if (i <= j) { int value = _cells[i]; _cells[i++] = _cells[j]; _cells[j--] = value; }
                }
                if (middle <= j) right = j;
                else if (middle >= i) left = i;
                else break;
            }
            Split(first, middle - 1, axis ^ 1); Split(middle + 1, last, axis ^ 1);
        }

        public int Find(Vector2 point, Func<int, bool> accepts = null)
        {
            int best = -1; float distance = float.PositiveInfinity;
            Search(0, _cells.Length - 1, 0, point, accepts, ref best, ref distance);
            return best;
        }
        private void Search(int first, int last, int axis, Vector2 point, Func<int, bool> accepts,
            ref int best, ref float distance)
        {
            if (first > last) return;
            int middle = (first + last) / 2, cell = _cells[middle];
            var position = _point(cell);
            float candidate = (position - point).sqrMagnitude;
            if ((candidate < distance || candidate == distance && (best < 0 || cell < best)) &&
                (accepts == null || accepts(cell))) { distance = candidate; best = cell; }
            float delta = axis == 0 ? point.x - position.x : point.y - position.y;
            if (delta <= 0)
            {
                Search(first, middle - 1, axis ^ 1, point, accepts, ref best, ref distance);
                if (delta * delta <= distance) Search(middle + 1, last, axis ^ 1, point, accepts, ref best, ref distance);
            }
            else
            {
                Search(middle + 1, last, axis ^ 1, point, accepts, ref best, ref distance);
                if (delta * delta <= distance) Search(first, middle - 1, axis ^ 1, point, accepts, ref best, ref distance);
            }
        }
    }
}
