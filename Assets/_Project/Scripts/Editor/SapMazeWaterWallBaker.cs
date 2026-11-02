using System;
using System.Collections.Generic;
using Herbalist.Levels;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    // Bake once in Edit mode. Water never queries the sculpted floor at runtime.
    public static class SapMazeWaterWallBaker
    {
        [MenuItem("Herbalist/Stage 2/Bake Water Walls")]
        private static void BakeScene()
        {
            foreach (var surface in UnityEngine.Object.FindObjectsByType<SapMazeSurface>(FindObjectsSortMode.None))
                Debug.Log(Bake(surface), surface);
            AssetDatabase.SaveAssets();
        }

        public static string Bake(SapMazeSurface surface)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Bake water walls in Edit mode.");
            var board = surface.GetComponent<SapMazeBoard>();
            if (board == null || surface.FloorPlane == null || surface.MazeCollider == null)
                throw new InvalidOperationException("A maze, floor plane and relief collider are required.");
            var root = board.transform;
            var source = surface.MazeCollider;
            var serialized = new SerializedObject(surface);
            float step = serialized.FindProperty("_cellSize").floatValue;
            float fraction = serialized.FindProperty("_wallBakeDepthFraction").floatValue;
            float padding = serialized.FindProperty("_wallBakePaddingWorld").floatValue /
                Mathf.Max(.001f, root.TransformVector(Vector3.right).magnitude);
            var localBounds = new Bounds(); bool first = true;
            var meshBounds = source.sharedMesh.bounds;
            for (int x = 0; x < 2; x++) for (int y = 0; y < 2; y++) for (int z = 0; z < 2; z++)
            {
                var p = root.InverseTransformPoint(source.transform.TransformPoint(new Vector3(
                    x == 0 ? meshBounds.min.x : meshBounds.max.x, y == 0 ? meshBounds.min.y : meshBounds.max.y,
                    z == 0 ? meshBounds.min.z : meshBounds.max.z)));
                if (first) { localBounds = new Bounds(p, Vector3.zero); first = false; }
                else localBounds.Encapsulate(p);
            }
            int width = Mathf.CeilToInt(localBounds.size.x / step);
            int height = Mathf.CeilToInt(localBounds.size.y / step);
            var depths = new float[width * height]; var sorted = new List<float>(depths.Length);
            float startZ = localBounds.min.z - 1;
            float length = (localBounds.size.z + 2) * root.TransformVector(Vector3.forward).magnitude;
            var direction = root.TransformDirection(Vector3.forward).normalized;
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
            {
                var p = localBounds.min + new Vector3((x + .5f) * step, (y + .5f) * step, 0);
                var start = root.TransformPoint(new Vector3(p.x, p.y, startZ));
                int i = y * width + x;
                depths[i] = source.Raycast(new Ray(start, direction), out var hit, length) ?
                    root.InverseTransformPoint(hit.point).z : float.NegativeInfinity;
                if (!float.IsNegativeInfinity(depths[i])) sorted.Add(depths[i]);
            }
            if (sorted.Count == 0) throw new InvalidOperationException("Relief mesh sampling failed.");
            sorted.Sort();
            float crest = sorted[Mathf.FloorToInt((sorted.Count - 1) * .05f)];
            float floor = sorted[Mathf.FloorToInt((sorted.Count - 1) * .99f)];
            float cut = Mathf.Lerp(crest, floor, fraction);
            // Merge equal row spans into rectangles. Extrusion removes all floor height variation.
            var rectangles = new List<RectInt>(); var previous = new Dictionary<long, int>();
            int occupied = 0;
            for (int y = 0; y < height; y++)
            {
                var current = new Dictionary<long, int>();
                for (int x = 0; x < width;)
                {
                    if (depths[y * width + x] > cut) { x++; continue; }
                    int left = x;
                    while (x < width && depths[y * width + x] <= cut) { x++; occupied++; }
                    long key = ((long)left << 32) | (uint)x;
                    if (previous.TryGetValue(key, out int index))
                    { var rect = rectangles[index]; rect.height++; rectangles[index] = rect; current[key] = index; }
                    else { current[key] = rectangles.Count; rectangles.Add(new RectInt(left, y, x - left, 1)); }
                }
                previous = current;
            }
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            foreach (var r in rectangles)
            {
                var min = localBounds.min + new Vector3(r.x * step, r.y * step, -1);
                var max = new Vector3(min.x + r.width * step, min.y + r.height * step, localBounds.max.z + 1);
                min.x -= padding; min.y -= padding; max.x += padding; max.y += padding;
                AddBox(vertices, triangles, min, max);
            }
            const string folder = "Assets/_Project/Data/Stages/Stage_02/Collision";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_Project/Data/Stages/Stage_02", "Collision");
            string path = folder + "/" + board.name + "_WaterWalls.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool create = mesh == null;
            if (create) mesh = new Mesh(); else mesh.Clear();
            mesh.name = board.name + " water wall silhouettes";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            if (create) AssetDatabase.CreateAsset(mesh, path); else EditorUtility.SetDirty(mesh);
            var child = root.Find("WaterWalls");
            if (child == null)
            { child = new GameObject("WaterWalls").transform; Undo.RegisterCreatedObjectUndo(child.gameObject, "Author water walls"); child.SetParent(root, false); }
            child.localPosition = Vector3.zero; child.localRotation = Quaternion.identity; child.localScale = Vector3.one;
            child.gameObject.layer = 2; // Ignore ordinary camera/ability raycasts.
            var collider = child.GetComponent<MeshCollider>();
            if (collider == null) collider = Undo.AddComponent<MeshCollider>(child.gameObject);
            collider.sharedMesh = null; collider.sharedMesh = mesh; collider.convex = false;
            collider.excludeLayers = ~0; // Queries only: no physical contacts with players/abilities.
            serialized.FindProperty("_waterWallCollider").objectReferenceValue = collider;
            serialized.ApplyModifiedProperties(); surface.InvalidateGeometry();
            EditorUtility.SetDirty(surface); EditorUtility.SetDirty(collider);
            AssetDatabase.SaveAssets();
            return board.name + ": cut=" + cut.ToString("F4") + ", walls=" + occupied + ", boxes=" + rectangles.Count;
        }

        private static void AddBox(List<Vector3> vertices, List<int> triangles, Vector3 min, Vector3 max)
        {
            int start = vertices.Count;
            vertices.Add(new Vector3(min.x,min.y,min.z)); vertices.Add(new Vector3(max.x,min.y,min.z));
            vertices.Add(new Vector3(max.x,max.y,min.z)); vertices.Add(new Vector3(min.x,max.y,min.z));
            vertices.Add(new Vector3(min.x,min.y,max.z)); vertices.Add(new Vector3(max.x,min.y,max.z));
            vertices.Add(new Vector3(max.x,max.y,max.z)); vertices.Add(new Vector3(min.x,max.y,max.z));
            int[] faces = {0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};
            foreach (int i in faces) triangles.Add(start + i);
        }
    }
}
