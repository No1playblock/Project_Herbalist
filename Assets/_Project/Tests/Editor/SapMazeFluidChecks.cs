using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Levels;
using UnityEngine;
using UnityEditor;

namespace Herbalist.Editor
{
    public static class SapMazeFluidChecks
    {
        private static void Check(bool value, string message)
        { if (!value) throw new Exception("Maze fluid: " + message); }

        public static string RunNarrowRoute(SapMazeBoard board, bool reverse = false, Transform destination = null)
        {
            var surface = board.Surface; Check(surface.Build(board.transform), "maze surface ready");
            var view = board.GetComponentInChildren<SapMazeTrailView>(true);
            var start = board.transform.InverseTransformPoint((reverse ? board.GoalMarker : board.StartMarker).position);
            var goal = board.transform.InverseTransformPoint((destination != null ? destination : reverse ? board.StartMarker : board.GoalMarker).position);
            var path = new List<Vector2>();
            Check(surface.TryFindPath(new Vector2(start.x, start.y), new Vector2(goal.x, goal.y), path), "narrow route exists");
            var field = new SapMazeFluidField(surface, view.Settings.volumePerLocalArea);
            field.WorldScale = board.transform.TransformVector(Vector3.right).magnitude;
            field.Reset(surface.GridCell(path[0]), board.Definition.initialVolume);
            int index = Mathf.Min(8, path.Count - 1), stale = 0, tick = 0;
            float best = float.PositiveInfinity; bool reached = false;
            for (; tick < 8000 && stale < 240; tick++)
            {
                field.Step(surface.GridCell(path[index]), board.Definition.speed, 1f / 60, view.Settings);
                float distance = field.Distance(path[index]);
                if (distance < surface.CellSize * 1.5f)
                {
                    if (index == path.Count - 1) { reached = true; break; }
                    index = Mathf.Min(index + 8, path.Count - 1); stale = 0; best = float.PositiveInfinity;
                }
                else if (distance < best - .00001f) { best = distance; stale = 0; }
                else stale++;
            }
            Check(reached, board.name + " stalled at route " + index + "/" + path.Count + " " + path[index]);
            Check(Mathf.Abs(field.ActiveCells.Sum(field.Mass) - board.Definition.initialVolume) < .01f, "narrow route preserves volume");
            Check(field.ActiveCells.All(surface.GridOpen), "narrow route cannot enter wall cells");
            return board.name + (reverse ? " reverse" : " forward") + ": " + path.Count + " route points, " + tick + " ticks, volume/walls PASS";
        }

        public static string RunNearestIndex()
        {
            var open = Enumerable.Range(0, 221).Select(i => i % 7 != 0 && i % 13 != 5).ToArray();
            Func<int, Vector2> point = i => new Vector2(i % 17, i / 17) * .04f;
            var index = new SapMazeNearestCells(); index.Build(open, point);
            var random = new System.Random(391);
            for (int query = 0; query < 300; query++)
            {
                var p = query == 0 ? new Vector2(.1f, .1f) :
                    new Vector2((float)random.NextDouble() * 2 - .5f, (float)random.NextDouble() * 2 - .5f);
                Func<int, bool> accepts = query % 2 == 0 ? null : i => i % 3 != 0;
                int expected = -1; float distance = float.PositiveInfinity;
                for (int cell = 0; cell < open.Length; cell++)
                {
                    if (!open[cell] || accepts != null && !accepts(cell)) continue;
                    float d = (point(cell) - p).sqrMagnitude;
                    if (d < distance) { distance = d; expected = cell; }
                }
                Check(index.Find(p, accepts) == expected, "nearest index matches full scan " + query);
            }
            return "nearest-cell index matches 300 exact full scans including filtered/outside queries PASS";
        }

        private static void CheckCachedRoutes(SapMazeSurface surface)
        {
            var starts = new[] { new Vector2(-2, 0), new Vector2(-2.2f, 1), new Vector2(-1, -2.4f), new Vector2(2, 0) };
            foreach (var start in starts) foreach (var goal in starts)
            {
                var expected = new List<Vector2>(); var actual = new List<Vector2>();
                var a = surface.Snap(start); var b = surface.Snap(goal);
                if (surface.IsSegmentOpen(a, b)) { expected.Add(a); expected.Add(b); }
                else
                {
                    int first = surface.GridCell(start), last = surface.GridCell(goal);
                    var previous = Enumerable.Repeat(-1, surface.GridCount).ToArray();
                    var queue = new Queue<int>(); queue.Enqueue(first); previous[first] = first;
                    while (queue.Count > 0 && previous[last] < 0)
                    {
                        int cell = queue.Dequeue();
                        for (int direction = 0; direction < 4; direction++)
                        {
                            int next = surface.GridNeighbour(cell, direction);
                            if (next < 0 || previous[next] >= 0) continue;
                            previous[next] = cell; queue.Enqueue(next);
                        }
                    }
                    if (previous[last] >= 0)
                    {
                        for (int cell = last; cell != first; cell = previous[cell]) expected.Add(surface.GridPoint(cell));
                        expected.Add(surface.GridPoint(first)); expected.Reverse();
                        if ((b - expected[expected.Count - 1]).sqrMagnitude > .00000001f) expected.Add(b);
                    }
                }
                for (int repeat = 0; repeat < 2; repeat++)
                {
                    bool found = surface.TryFindPath(start, goal, actual);
                    Check(found == (expected.Count > 0) && actual.SequenceEqual(expected), "cached route matches forward BFS including tie order");
                }
            }
        }

        public static string RunPlaneSurface()
        {
            var root = new GameObject("Maze plane regression fixture");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = new Mesh();
            try
            {
                var cubeMesh = cube.GetComponent<MeshFilter>().sharedMesh;
                mesh.CombineMeshes(new[]
                {
                    new CombineInstance { mesh = cubeMesh, transform = Matrix4x4.TRS(new Vector3(0, 0, 1), Quaternion.identity, new Vector3(6, 6, 1)) },
                    new CombineInstance { mesh = cubeMesh, transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(.08f, 4, 2)) }
                });
                var collider = root.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
                var floor = new GameObject("WaterFloorPlane").transform;
                floor.SetParent(root.transform, false); floor.localScale = new Vector3(6, 6, 1);
                var surface = root.AddComponent<SapMazeSurface>();
                var so = new SerializedObject(surface);
                so.FindProperty("_mazeCollider").objectReferenceValue = collider;
                so.FindProperty("_floorPlane").objectReferenceValue = floor;
                so.FindProperty("_cellSize").floatValue = .2f;
                so.ApplyModifiedPropertiesWithoutUndo();
                Check(surface.Build(root.transform), "authored plane builds");
                Check(surface.IsSurfaceOpen(new Vector2(-2, 0)), "plane corridor open");
                Check(!surface.IsSurfaceOpen(Vector2.zero), "wall covers plane");
                Check(!surface.IsSurfaceOpen(new Vector2(4, 0)), "outside plane rejected");
                Check(!surface.IsSegmentOpen(new Vector2(-2, 0), new Vector2(2, 0)), "thin wall blocks direct crossing");
                var path = new List<Vector2>();
                Check(surface.TryFindPath(new Vector2(-2, 0), new Vector2(2, 0), path), "route around wall stays open");
                Check(path.All(p => Mathf.Abs(surface.SurfaceZ(p, 100)) < .00001f), "all rendered depths equal authored plane");
                for (int i = 1; i < path.Count; i++)
                    Check(surface.IsSegmentOpen(path[i - 1], path[i]), "route never crosses wall");
                int version = surface.GeometryVersion;
                floor.localPosition = new Vector3(0, 0, .1f);
                Check(surface.Build(root.transform) && surface.GeometryVersion > version, "moving plane invalidates geometry");
                Check(Mathf.Abs(surface.SurfaceZ(new Vector2(-2, 0), 100) - .1f) < .00001f, "depth follows moved plane");
                // A decorative ridge crossing the plane must not matter once dedicated walls are assigned.
                mesh.Clear();
                mesh.CombineMeshes(new[]
                {
                    new CombineInstance { mesh = cubeMesh, transform = Matrix4x4.TRS(new Vector3(0, 0, 1), Quaternion.identity, new Vector3(6, 6, 1)) },
                    new CombineInstance { mesh = cubeMesh, transform = Matrix4x4.TRS(new Vector3(-2, 0, 0), Quaternion.identity, new Vector3(1, 1, 2)) }
                });
                collider.sharedMesh = null; collider.sharedMesh = mesh;
                var wall = new GameObject("WaterWalls"); wall.transform.SetParent(root.transform, false);
                wall.transform.localScale = new Vector3(.08f, 4, 4);
                var wallCollider = wall.AddComponent<MeshCollider>(); wallCollider.sharedMesh = cubeMesh;
                wallCollider.excludeLayers = ~0;
                so.Update(); so.FindProperty("_waterWallCollider").objectReferenceValue = wallCollider;
                so.ApplyModifiedPropertiesWithoutUndo();
                Check(surface.Build(root.transform), "dedicated walls build");
                Check(surface.IsSurfaceOpen(new Vector2(-2, 0)), "decorative floor ridge ignored");
                Check(!surface.IsSurfaceOpen(Vector2.zero), "dedicated wall still blocks");
                Check(!surface.IsSegmentOpen(new Vector2(-2, 0), new Vector2(2, 0)), "dedicated thin wall blocks segment");
                Check(surface.TryFindPath(new Vector2(-2, 0), new Vector2(2, 0), path), "dedicated wall detour");
                CheckCachedRoutes(surface);
                return "authored plane, fixed depth, bounds, thin-wall blocking, detour, plane-edit invalidation and floor-independent wall collider PASS";
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(cube);
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

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
                // A tiny pull must shrink the far boundary first, preserving the full interior.
                Func<int, int, int> line = (cell, d) => d == 0 && cell > 0 ? cell - 1 :
                    d == 1 && cell < 20 ? cell + 1 : -1;
                var strip = new SapMazeFluidField(21, 21, 1, 1, line, i => new Vector2(i, 0));
                strip.Reset(10, 5);
                var before = Enumerable.Range(0, strip.Count).Select(strip.Mass).ToArray();
                int far = strip.ActiveCells.Min();
                strip.Step(20, .01f, .1f, settings);
                Check(strip.Mass(far) < before[far], "farthest boundary shrinks first");
                for (int i = far + 1; i <= 12; i++)
                    Check(Math.Abs(strip.Mass(i) - before[i]) < .00001f, "filled interior does not thin " + i);
                Check(Math.Abs(strip.ActiveCells.Sum(strip.Mass) - 5) < .00001f, "boundary transfer conserves mass");
                strip.Drain(new Vector2(10, 0), 5);
                Check(strip.ActiveCells.Count == 0, "empty cells are not replicated as water");
                results.Add("farthest boundary first, full interior, empty snapshot PASS");
                const int poolWidth = 31;
                Func<int, int, int> poolEdges = (cell, d) =>
                {
                    int x = cell % poolWidth + (d == 0 ? -1 : d == 1 ? 1 : 0);
                    int y = cell / poolWidth + (d == 2 ? -1 : d == 3 ? 1 : 0);
                    return x >= 0 && x < poolWidth && y >= 0 && y < poolWidth ? y * poolWidth + x : -1;
                };
                var pool = new SapMazeFluidField(poolWidth * poolWidth, poolWidth, 1, 1,
                    poolEdges, i => new Vector2(i % poolWidth, i / poolWidth));
                settings.minimumFlowWidthWorld = 5;
                pool.Reset(15 * poolWidth + 10, 25);
                for (int tick = 0; tick < 240; tick++) pool.Step(15 * poolWidth + 25, 2, 1f / 60, settings);
                int front = pool.ActiveCells.Where(i => pool.Mass(i) > .01f).Max(i => i % poolWidth);
                int section = Enumerable.Range(0, poolWidth).Count(y => pool.Mass(y * poolWidth + front - 1) >= pool.OccupancyThreshold);
                float sectionMass = Enumerable.Range(0, poolWidth).Sum(y => pool.Mass(y * poolWidth + front - 1));
                Check(front > 14, "minimum-width flow still advances");
                Check(section >= 5, "advancing front retains five-cell width: " + section);
                Check(sectionMass >= 5 * settings.minimumFlowDensity, "front width has real water, not a faint outline");
                Check(Math.Abs(pool.ActiveCells.Sum(pool.Mass) - 25) < .001f, "minimum width conserves mass");
                var tiny = new SapMazeFluidField(21, 21, 1, 1, line, i => new Vector2(i, 0));
                tiny.Reset(5, .1f); tiny.Step(15, 1, .1f, settings);
                Check(tiny.NearestWet(new Vector2(15, 0)) > 5, "small remaining drop still moves in a narrow channel");
                settings.minimumFlowWidthWorld = 1.2f;
                results.Add("minimum front width, narrow passage, small drop, conservation PASS");
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
                var outline = new SapMazeFluidOutline();
                int cellsBefore = pool.ActiveCells.Count;
                float volumeBefore = pool.ActiveCells.Sum(pool.Mass);
                outline.Build(pool, 3, 12);
                Check(outline.Cells.Any(i => pool.Mass(i) == 0 && outline.Value(i) > .01f), "rounded influence extends smoothly into open dry space");
                Check(outline.Cells.All(i => outline.Value(i) >= 0 && outline.Value(i) <= 1.00001f), "display density is bounded");
                Check(pool.ActiveCells.Count == cellsBefore && Math.Abs(pool.ActiveCells.Sum(pool.Mass) - volumeBefore) < .00001f,
                    "outline never changes authoritative volume");
                outline.Build(blocked, .25f, 12);
                Check(outline.Cells.All(i => i % width < mid), "rounding never blends across a disconnected wall");
                outline.Build(tiny, 3, 12);
                Check(outline.Maximum > 0 && outline.Cells.Any(i => outline.Value(i) >= outline.Maximum * .4f), "small drop keeps a visible contour");
                results.Add("continuous outline, wall isolation, unchanged mass, small drop PASS");
                pool.Reset(3 * poolWidth + 3, 8);
                outline.Build(pool, 3, 12);
                pool.Reset(26 * poolWidth + 26, 8);
                outline.Build(pool, 3, 12);
                var freshOutline = new SapMazeFluidOutline(); freshOutline.Build(pool, 3, 12);
                Check(Enumerable.Range(0, pool.Count).All(i => outline.Value(i) == freshOutline.Value(i)), "reused density buffers match fresh buffers after moving pool");
                results.Add("reused density buffers have no stale water PASS");
                var contact = new SapMazeFluidField(9, 3, 1, 1, (i, d) => -1,
                    i => new Vector2(i % 3, i / 3));
                contact.Reset(4, 1);
                Check(contact.Contains(new Vector2(1, 1)), "hole center covered");
                Check(contact.Contains(new Vector2(1.49f, 1)), "occupied cell edge covered");
                Check(contact.Contains(new Vector2(1.5f, 1.5f)), "shared cell corner contact is not lost to rounding");
                Check(!contact.Contains(new Vector2(1.51f, 1)), "nearby dry cell is not hole contact");
                Check(!contact.Contains(new Vector2(-.51f, 1)), "outside field never snaps into contact");
                Check(!contact.Contains(new Vector2(float.NaN, 1)), "nonfinite contact rejected");
                results.Add("exact occupied-cell leak contact, dry neighbour and outside bounds PASS");

                var parked = new SapMazeFluidField(9, 9, 1, 1,
                    (cell, direction) => direction == 0 && cell > 0 ? cell - 1 :
                        direction == 1 && cell < 8 ? cell + 1 : -1,
                    cell => new Vector2(cell, 0));
                parked.Reset(4, 5);
                for (int tick = 0; tick < 40; tick++)
                {
                    Check(parked.Contains(new Vector2(4, 0)), "stationary water keeps feeding the hole");
                    Check(Math.Abs(parked.Drain(new Vector2(4, 0), .05f) - .05f) < .00001f,
                        "stationary leak drains on every tick");
                }
                Check(Math.Abs(parked.TotalVolume - 3) < .0001f &&
                    Math.Abs(parked.ActiveCells.Sum(parked.Mass) - parked.TotalVolume) < .0001f,
                    "stationary leak conserves the remaining mass");
                Check(parked.Drain(new Vector2(8, 0), 1) == 0, "a dry hole cannot drain nearby water");
                var isolatedPool = new SapMazeFluidField(3, 3, 1, 1, (cell, direction) => -1,
                    cell => new Vector2(cell, 0));
                isolatedPool.ReadSnapshot(new uint[] { 4095u << 20, 2u | 4095u << 20 });
                Check(Math.Abs(isolatedPool.Drain(Vector2.zero, 2) - 1) < .00001f &&
                    Math.Abs(isolatedPool.Mass(2) - 1) < .00001f,
                    "a leak drains only its connected water");
                results.Add("stationary continuous drainage, exact remaining volume and isolated pools PASS");

                return string.Join("\n", results);
            }
            finally { UnityEngine.Object.DestroyImmediate(settings); }
        }
    }
}
