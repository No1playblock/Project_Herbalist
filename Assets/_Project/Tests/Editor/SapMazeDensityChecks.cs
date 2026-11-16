using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class SapMazeDensityChecks
    {
        private const string ScenePath = "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity";
        private const string Output = "Logs/Maze1Density";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo NextRender = typeof(SapMazeTrailView).GetField("_nextFluidRender", Private);
        private const int Width = 480;
        private const int Height = 360;

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        public static string Run()
        {
            var scene = EditorSceneManager.OpenPreviewScene(ScenePath);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            var previous = RenderTexture.active;
            try
            {
                var boards = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SapMazeBoard>(true)).ToArray();
                var board = boards.Single(b => b.name == "SapMaze_1");
                var view = board.GetComponentInChildren<SapMazeTrailView>(true);
                var density = DensityFor(view);
                Check(density != null && boards.Count(b => DensityFor(b.GetComponentInChildren<SapMazeTrailView>(true)) != null) == 1,
                    "Only Maze 1 should contain a density renderer.");
                var surface = board.Surface;
                Check(surface.Build(board.transform), "Maze surface must build.");
                var renderer = density.GetComponent<MeshRenderer>();
                var waterMaterial = renderer.sharedMaterial;
                Check(waterMaterial.shader.isSupported && !ShaderUtil.ShaderHasError(waterMaterial.shader), "Density shader must compile.");
                var settings = view.Settings;
                var field = new SapMazeFluidField(surface, settings.volumePerLocalArea)
                {
                    WorldScale = board.transform.TransformVector(Vector3.right).magnitude
                };
                Check(density.CanPresent(field), "Density texture dimensions and graphics format must match the board.");
                var start = board.transform.InverseTransformPoint(board.StartMarker.position);
                var goal = board.transform.InverseTransformPoint(board.GoalMarker.position);
                var path = new List<Vector2>();
                Check(surface.TryFindPath(start, goal, path), "A route from the authored start to goal must exist.");
                field.Reset(surface.GridCell(path[0]), board.Definition.initialVolume);

                var camera = CreateCamera(scene, target);
                var legacyCpu = new List<double>();
                var densityCpu = new List<double>();
                var legacyFrame = new List<double>();
                var densityFrame = new List<double>();
                int routeIndex = Mathf.Min(8, path.Count - 1);
                bool reached = false;
                int ticks = 0;
                int[] capturePoints = { 0, path.Count / 2, path.Count - 1 };
                int capture = 0;
                var frames = new List<(Vector2 focus, uint[] cells)>();
                for (; ticks < 8000; ticks++)
                {
                    if (ticks != 0)
                        field.Step(surface.GridCell(path[routeIndex]), board.Definition.speed, 1f / 60f, settings);
                    bool atTarget = field.Distance(path[routeIndex]) < surface.CellSize * 1.5f;
                    reached = atTarget && routeIndex == path.Count - 1;
                    if (ticks % 3 == 0 || reached)
                    {
                        var before = Enumerable.Range(0, field.ActiveCells.Count).Select(field.Pack).ToArray();
                        bool render = ticks % 30 == 0;
                        if (render) Aim(camera, board.transform, path[routeIndex], 5f);
                        bool densityFirst = legacyCpu.Count % 2 != 0;
                        for (int pass = 0; pass < 2; pass++)
                        {
                            bool useDensity = pass == 0 ? densityFirst : !densityFirst;
                            var watch = Stopwatch.StartNew();
                            Present(view, density, field, surface, useDensity);
                            double cpu = watch.Elapsed.TotalMilliseconds;
                            (useDensity ? densityCpu : legacyCpu).Add(cpu);
                            if (render)
                            {
                                Read(camera, target, pixels, false);
                                (useDensity ? densityFrame : legacyFrame).Add(watch.Elapsed.TotalMilliseconds);
                            }
                        }
                        Check(field.ActiveCells.Count == before.Length &&
                              before.SequenceEqual(Enumerable.Range(0, before.Length).Select(field.Pack)),
                            "Presentation changed authoritative density.");
                        if (capture < capturePoints.Length &&
                            (capture == 0 || capture == 1 && routeIndex >= capturePoints[1] || capture == 2 && reached))
                        {
                            frames.Add((path[routeIndex], before));
                            capture++;
                        }
                    }
                    if (reached) break;
                    if (atTarget) routeIndex = Mathf.Min(routeIndex + 8, path.Count - 1);
                }
                Check(reached && frames.Count == 3 && Mathf.Abs(field.TotalVolume - board.Definition.initialVolume) < .01f,
                    "The same authored route must reach the goal with conserved volume.");
                Directory.CreateDirectory(Output);
                field.ReadSnapshot(frames[1].cells);
                Aim(camera, board.transform, frames[1].focus, 5f);
                Present(view, density, field, surface, false);
                Save(camera, target, pixels, "legacy");
                Present(view, density, field, surface, true);
                Save(camera, target, pixels, "density");

                int[] validation = VerifyPixels(scene, board, view, density, field, frames, camera, target, pixels);
                string report =
                    $"Maze 1, {path.Count} path points, {ticks} movement ticks, {legacyCpu.Count} paired updates. Same cell snapshots and conserved volume.\n" +
                    Stats("CPU legacy", legacyCpu) + "\n" + Stats("CPU density", densityCpu) + "\n" +
                    Stats("Offscreen frame + GPU readback legacy", legacyFrame) + "\n" +
                    Stats("Offscreen frame + GPU readback density", densityFrame) + "\n" +
                    $"Visible samples {validation[0]}, occluded by relief {validation[1]}, on walls {validation[2]}, " +
                    $"without reachable density {validation[3]}. Adjacent-cell blocked-edge fixture checked.";
                File.WriteAllText(Output + "/comparison.txt", report);
                Check(validation[0] > 100 && validation.Skip(1).All(v => v == 0), "Density wall or floor pixel verification failed. " + report);
                return report;
            }
            finally
            {
                RenderTexture.active = previous;
                EditorSceneManager.ClosePreviewScene(scene);
                Object.DestroyImmediate(pixels);
                Object.DestroyImmediate(target);
            }
        }

        private static SapMazeDensitySurface DensityFor(SapMazeTrailView view) =>
            new SerializedObject(view).FindProperty("_densitySurface").objectReferenceValue as SapMazeDensitySurface;

        private static Camera CreateCamera(Scene scene, RenderTexture target)
        {
            var obj = new GameObject("Maze A/B Camera");
            SceneManager.MoveGameObjectToScene(obj, scene);
            var camera = obj.AddComponent<Camera>();
            camera.scene = scene;
            camera.enabled = false;
            camera.orthographic = true;
            camera.nearClipPlane = .03f;
            camera.farClipPlane = 45f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.targetTexture = target;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            return camera;
        }

        private static void Aim(Camera camera, Transform board, Vector2 center, float size)
        {
            camera.orthographicSize = size;
            camera.transform.SetPositionAndRotation(board.TransformPoint(new Vector3(center.x, center.y, -7)),
                Quaternion.LookRotation(board.forward, board.up));
        }

        private static void Present(SapMazeTrailView view, SapMazeDensitySurface density, SapMazeFluidField field,
            SapMazeSurface surface, bool useDensity)
        {
            density.enabled = useDensity;
            NextRender.SetValue(view, 0f);
            view.PresentField(field, surface);
        }

        private static Color32[] Read(Camera camera, RenderTexture target, Texture2D pixels, bool full)
        {
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, full ? Width : 1, full ? Height : 1), 0, 0, false);
            return full ? pixels.GetPixels32() : null;
        }

        private static void Save(Camera camera, RenderTexture target, Texture2D pixels, string name)
        {
            Read(camera, target, pixels, true);
            File.WriteAllBytes(Output + "/" + name + ".png", pixels.EncodeToPNG());
        }

        private static string Stats(string name, List<double> samples)
        {
            samples.Sort();
            return $"{name} ({samples.Count}): mean {samples.Average():F3} ms, p95 {samples[Mathf.CeilToInt(samples.Count * .95f) - 1]:F3} ms, max {samples.Last():F3} ms";
        }

        private static int[] VerifyPixels(Scene scene, SapMazeBoard board, SapMazeTrailView view,
            SapMazeDensitySurface density, SapMazeFluidField field, List<(Vector2 focus, uint[] cells)> frames,
            Camera camera, RenderTexture target, Texture2D pixels)
        {
            var surface = board.Surface;
            var renderer = density.GetComponent<MeshRenderer>();
            var previousMaterial = renderer.sharedMaterial;
            var marker = new Material(previousMaterial);
            marker.SetColor("_BaseColor", Color.magenta);
            marker.SetColor("_EdgeColor", Color.magenta);
            renderer.sharedMaterial = marker;
            density.gameObject.layer = 31;
            camera.cullingMask = 1 << 31;

            // An opaque copy of the real relief acts as the depth occluder.
            var relief = surface.MazeCollider;
            var reliefObject = new GameObject("Original floor depth", typeof(MeshFilter), typeof(MeshRenderer));
            SceneManager.MoveGameObjectToScene(reliefObject, scene);
            reliefObject.transform.SetParent(relief.transform, false);
            reliefObject.layer = 31;
            reliefObject.GetComponent<MeshFilter>().sharedMesh = relief.sharedMesh;
            var occluder = reliefObject.GetComponent<MeshRenderer>();
            var depthMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            depthMaterial.SetColor("_BaseColor", Color.black);
            occluder.sharedMaterial = depthMaterial;
            try
            {
                var outline = new SapMazeFluidOutline();
                float radius = view.Settings.outlineBlendRadiusWorld / view.transform.TransformVector(Vector3.right).magnitude;
                AddBlockedEdgeFixture(surface, field, frames);
                int[] counts = new int[4];
                foreach (var frame in frames)
                {
                    field.ReadSnapshot(frame.cells);
                    outline.Build(field, radius, view.Settings.outlineBlendPasses);
                    float threshold = Mathf.Min(view.Settings.outlineThreshold, outline.Maximum * .4f);
                    Aim(camera, board.transform, frame.focus, frame.cells.Length == 1 ? .65f : 5f);
                    // Make the previous field stamp irrelevant even if this fixture duplicates a capture.
                    Present(view, density, field, surface, false);
                    Present(view, density, field, surface, true);
                    occluder.enabled = false;
                    var alone = Read(camera, target, pixels, true);
                    occluder.enabled = true;
                    var withFloor = Read(camera, target, pixels, true);
                    Vector2 origin = surface.GridPoint(0);
                    int visibleInFixture = 0;
                    for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        int i = y * Width + x;
                        if (!IsMarker(alone[i])) continue;
                        visibleInFixture++;
                        counts[0]++;
                        if (!IsMarker(withFloor[i])) counts[1]++;
                        var world = camera.ViewportToWorldPoint(new Vector3((x + .5f) / Width, (y + .5f) / Height, 1));
                        Vector2 p = board.transform.InverseTransformPoint(world);
                        if (!surface.IsSurfaceOpen(p)) counts[2]++;
                        Vector2 grid = (p - origin) / field.CellSize;
                        int col = Mathf.FloorToInt(grid.x), row = Mathf.FloorToInt(grid.y);
                        int cell = row * field.Width + col;
                        bool reachable = false;
                        for (int tap = 0; tap < 4; tap++)
                        {
                            int sample = cell + (tap & 1) + (tap >> 1) * field.Width;
                            if (outline.Value(sample) + .00005f < threshold) continue;
                            if (!surface.IsSegmentOpen(p, surface.GridPoint(sample))) continue;
                            reachable = true;
                            break;
                        }
                        if (!reachable) counts[3]++;
                    }
                    Check(visibleInFixture > 0, "The shader must draw water in each visual fixture.");
                }
                return counts;
            }
            finally
            {
                renderer.sharedMaterial = previousMaterial;
                Object.DestroyImmediate(reliefObject);
                Object.DestroyImmediate(marker);
                Object.DestroyImmediate(depthMaterial);
            }
        }

        private static bool IsMarker(Color32 pixel) => pixel.r > 170 && pixel.b > 170 && pixel.g < 65;

        private static void AddBlockedEdgeFixture(SapMazeSurface surface, SapMazeFluidField field,
            List<(Vector2 focus, uint[] cells)> frames)
        {
            for (int cell = 0; cell < field.Count; cell++)
            {
                if (!surface.GridOpen(cell)) continue;
                foreach (int direction in new[] { 1, 3 })
                {
                    if (direction == 1 && cell % field.Width == field.Width - 1) continue;
                    int other = cell + (direction == 1 ? 1 : field.Width);
                    if (!surface.GridOpen(other) || surface.GridNeighbour(cell, direction) >= 0) continue;
                    field.Reset(cell, field.CellCapacity);
                    Check(field.ActiveCells.Count == 1, "The thin-wall fixture should have exactly one wet cell.");
                    frames.Add(((surface.GridPoint(cell) + surface.GridPoint(other)) * .5f, new[] { field.Pack(0) }));
                    return;
                }
            }
            throw new InvalidOperationException("No thin wall between adjacent open cells was found.");
        }
    }
}
