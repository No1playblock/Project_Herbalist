using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Levels;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class SapMazeFluidChecks
    {
        private static void Check(bool value, string message)
        { if (!value) throw new Exception("Maze fluid: " + message); }

        public static string Run()
        {
            var settings = ScriptableObject.CreateInstance<SapMazeTrailSettings>();
            var results = new List<string>();
            try
            {
                const int width = 61, mid = 30;
                Func<int, bool> open = i => i >= 0 && i < width * width &&
                    (Math.Abs(i % width - mid) <= 2 || Math.Abs(i / width - mid) <= 2);
                Func<int, int, int> neighbour = (cell, d) =>
                {
                    int x = cell % width + (d == 0 ? -1 : d == 1 ? 1 : 0);
                    int y = cell / width + (d == 2 ? -1 : d == 3 ? 1 : 0);
                    int next = y * width + x;
                    return x >= 0 && x < width && y >= 0 && y < width && open(next) ? next : -1;
                };
                Func<int, Vector2> point = i => new Vector2(i % width - mid, i / width - mid) * .04f;
                int center = mid * width + mid;
                for (int direction = 0; direction < 4; direction++)
                {
                    var field = new SapMazeFluidField(width * width, width, .04f, 50, neighbour, point);
                    field.Reset(center, 8);
                    int target = center + (direction == 0 ? -22 : direction == 1 ? 22 : direction == 2 ? -22 * width : 22 * width);
                    for (int tick = 0; tick < 240; tick++) field.Step(target, .8f, 1f / 60, settings);
                    float total = field.ActiveCells.Sum(i => field.Mass(i));
                    Vector2 axis = point(target).normalized;
                    float selected = field.ActiveCells.Where(i => Vector2.Dot(point(i), axis) > .12f).Sum(i => field.Mass(i));
                    Check(Math.Abs(total - 8) < .002f, "conserved cross mass " + direction + ": " + total);
                    Check(selected > 8 * .65f, "other arms retract into selected arm " + direction + ": " + selected);
                    Check(field.ActiveCells.All(i => open(i) && field.Mass(i) >= -.00001f && field.Mass(i) <= field.CellCapacity + .00001f), "walls and capacity " + direction);
                    // Reverse through the same junction. Cells merge instead of duplicating a path.
                    int reverse = center - (target - center);
                    for (int tick = 0; tick < 300; tick++) field.Step(reverse, .8f, 1f / 60, settings);
                    Check(Math.Abs(field.ActiveCells.Sum(i => field.Mass(i)) - 8) < .004f, "U-turn conserves volume " + direction);
                    Check(field.ActiveCells.Distinct().Count() == field.ActiveCells.Count, "one region per cell");
                    var snapshot = Enumerable.Range(0, field.ActiveCells.Count).Select(field.Pack).ToArray();
                    var client = new SapMazeFluidField(width * width, width, .04f, 50, neighbour, point);
                    client.ReadSnapshot(snapshot);
                    Check(Math.Abs(client.TotalVolume - field.TotalVolume) < .02f, "snapshot density precision");
                    field.Drain(point(reverse), 2);
                    Check(Math.Abs(field.ActiveCells.Sum(i => field.Mass(i)) - 6) < .004f, "local leak removes exactly requested volume");
                    field.SetVolume(10, center);
                    Check(Math.Abs(field.ActiveCells.Sum(i => field.Mass(i)) - 10) < .004f, "refill adds exactly requested volume");
                    results.Add("direction " + direction + ": selected=" + selected.ToString("F2") + "/8, conserved, reverse/merge, snapshot, leak/refill PASS");
                }
                // Two adjacent but disconnected corridors must never exchange water.
                Func<int, int, int> isolated = (cell, d) =>
                {
                    int next = neighbour(cell, d);
                    return next >= 0 && (cell % width < mid) == (next % width < mid) ? next : -1;
                };
                var blocked = new SapMazeFluidField(width * width, width, .04f, 50, isolated, point);
                blocked.Reset(center - 10, 2);
                for (int tick = 0; tick < 60; tick++) blocked.Step(center + 10, .8f, 1f / 60, settings);
                Check(blocked.ActiveCells.All(i => i % width < mid), "no transfer through closed wall");
                results.Add("disconnected wall PASS");
                return string.Join("\n", results);
            }
            finally { UnityEngine.Object.DestroyImmediate(settings); }
        }
    }
}
