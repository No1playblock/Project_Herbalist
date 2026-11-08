using System;
using System.Linq;
using System.Reflection;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class MazeReliefVisibilityChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static bool Hidden(SapMazeSurface surface, Transform view, Vector3 vertex)
        {
            var p = surface.transform.InverseTransformPoint(view.TransformPoint(vertex));
            var ray = new Ray(surface.transform.TransformPoint(new Vector3(p.x, p.y, -10)), surface.transform.forward);
            return surface.MazeCollider.Raycast(ray, out var hit, 60) && surface.transform.InverseTransformPoint(hit.point).z < p.z - .0001f;
        }
        public static string Run()
        {
            var scene = EditorSceneManager.OpenPreviewScene("Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity");
            var result = new System.Collections.Generic.List<string>();
            try
            {
                foreach (var surface in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<SapMazeSurface>(true)))
                {
                    surface.Build(surface.transform);
                    int source = -1;
                    for (int i = 0; i < surface.GridCount; i += 13)
                    {
                        if (!surface.GridOpen(i)) continue;
                        var p = surface.GridPoint(i);
                        if (surface.ReliefSurfaceZ(p, 0) < surface.SurfaceZ(p, 0) - .02f) { source = i; break; }
                    }
                    if (source < 0) throw new Exception("Occlusion fixture not found on " + surface.name);
                    var view = surface.GetComponentInChildren<SapMazeTrailView>(true);
                    var settings = UnityEngine.Object.Instantiate(view.Settings);
                    try
                    {
                        typeof(SapMazeTrailView).GetField("_settings", Private).SetValue(view, settings);
                        var field = new SapMazeFluidField(surface, settings.volumePerLocalArea);
                        field.Reset(source, 60);
                        var snapshot = Enumerable.Range(0, field.ActiveCells.Count).Select(field.Pack).ToArray();
                        settings.followReliefSurface = false; view.PresentField(field, surface);
                        var mesh = (Mesh)typeof(SapMazeTrailView).GetField("_mesh", Private).GetValue(view);
                        int before = mesh.vertices.Count(p => Hidden(surface, view.transform, p));
                        settings.followReliefSurface = true;
                        typeof(SapMazeTrailView).GetField("_nextFluidRender", Private).SetValue(view, 0f);
                        view.PresentField(field, surface);
                        var points = mesh.vertices; var triangles = mesh.triangles;
                        int after = points.Count(p => Hidden(surface, view.transform, p)), hiddenTriangles = 0;
                        for (int i = 0; i < triangles.Length; i += 3)
                            if (Hidden(surface, view.transform, (points[triangles[i]] + points[triangles[i + 1]] + points[triangles[i + 2]]) / 3f)) hiddenTriangles++;
                        if (after != 0 || hiddenTriangles != 0) throw new Exception(surface.name + ": hidden vertices=" + after + ", hidden triangle centres=" + hiddenTriangles);
                        if (field.TotalVolume != 60 || !snapshot.SequenceEqual(Enumerable.Range(0, field.ActiveCells.Count).Select(field.Pack)))
                            throw new Exception("Presentation altered water data.");
                        result.Add(surface.name + ": hidden vertices " + before + " -> " + after + ", hidden triangle centres " + hiddenTriangles + ", volume/snapshot unchanged PASS");
                    }
                    finally { UnityEngine.Object.DestroyImmediate(settings); }
                }
                return string.Join("\n", result);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
