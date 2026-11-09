using System;
using System.Collections.Generic;
using System.IO;
using Herbalist.Levels;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Herbalist.Editor
{
    public static class SapMazeDensityBaker
    {
        private const string MaterialPath = "Assets/_Project/Art/Materials/Environments/Stage_02/MAT_Maze1Density.mat";
        private const string MaskPath = "Assets/_Project/Art/Environments/Stage_02/Textures/TEX_Maze1VisibleCorners.png";

        [MenuItem("Tools/Herbalist/Stage 2/Bake Maze 1 Density View")]
        public static void BakeSelectedMaze()
        {
            var board = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<SapMazeBoard>() : null;
            Bake(board);
        }

        public static string Bake(SapMazeBoard board)
        {
            if (Application.isPlaying || board == null || board.name != "SapMaze_1")
                throw new InvalidOperationException("Select the first maze in Edit mode.");
            var surface = board.Surface;
            if (!surface.Build(board.transform) || surface.WaterWallCollider == null)
                throw new InvalidOperationException("Bake the maze's WaterWalls first.");
            var view = board.GetComponentInChildren<SapMazeTrailView>(true);
            var viewData = new SerializedObject(view);
            var legacyRenderer = (MeshRenderer)viewData.FindProperty("_fluidRenderer").objectReferenceValue;
            var mask = BakeMask(board.transform, surface);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                var shader = Shader.Find("Herbalist/Maze Density Water");
                if (shader == null) throw new InvalidOperationException("Maze density shader is missing.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            // Reuse the relief geometry so the surface never interpolates through floor ridges.
            var relief = surface.MazeCollider;
            var child = relief.transform.Find("Density Water");
            if (child == null)
            {
                var obj = new GameObject("Density Water", typeof(MeshFilter), typeof(MeshRenderer), typeof(SapMazeDensitySurface));
                Undo.RegisterCreatedObjectUndo(obj, "Author Maze 1 density view");
                child = obj.transform;
                child.SetParent(relief.transform, false);
            }
            child.gameObject.layer = legacyRenderer.gameObject.layer;
            child.position = relief.transform.position - board.transform.forward * view.Settings.fluidSurfaceLiftWorld;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
            child.GetComponent<MeshFilter>().sharedMesh = relief.sharedMesh;
            var renderer = child.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.enabled = false;
            var density = child.GetComponent<SapMazeDensitySurface>();
            var data = new SerializedObject(density);
            data.FindProperty("_renderer").objectReferenceValue = renderer;
            data.FindProperty("_visibleCorners").objectReferenceValue = mask;
            data.ApplyModifiedProperties();
            viewData.FindProperty("_densitySurface").objectReferenceValue = density;
            viewData.ApplyModifiedProperties();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(board.gameObject.scene);
            AssetDatabase.SaveAssets();
            return $"Maze 1 density view: {mask.width}x{mask.height} wall mask, {relief.sharedMesh.vertexCount} static vertices.";
        }

        private static Texture2D BakeMask(Transform board, SapMazeSurface surface)
        {
            int subdivisions = SapMazeDensitySurface.MaskSamplesPerCell;
            int columns = surface.GridWidth - 1;
            int rows = surface.GridCount / surface.GridWidth - 1;
            int width = columns * subdivisions, height = rows * subdivisions;
            float cell = surface.CellSize, step = cell / subdivisions;
            Vector2 origin = surface.GridPoint(0);
            var blocked = new List<Rect>[columns * rows];
            var wall = surface.WaterWallCollider;
            var vertices = wall.sharedMesh.vertices;
            if (vertices.Length == 0 || vertices.Length % 8 != 0)
                throw new InvalidOperationException("The wall mesh must come from the WaterWalls box baker.");
            var toBoard = board.worldToLocalMatrix * wall.transform.localToWorldMatrix;
            for (int i = 0; i < vertices.Length; i += 8)
            {
                Vector2 low = toBoard.MultiplyPoint3x4(vertices[i]), high = low;
                for (int j = 1; j < 8; j++)
                {
                    Vector2 point = toBoard.MultiplyPoint3x4(vertices[i + j]);
                    low = Vector2.Min(low, point);
                    high = Vector2.Max(high, point);
                }
                // Padding by half a mask texel makes the visibility conservative over its entire footprint.
                low -= Vector2.one * (step * .5f + .00001f);
                high += Vector2.one * (step * .5f + .00001f);
                Rect bounds = Rect.MinMaxRect(low.x, low.y, high.x, high.y);
                int left = Mathf.Max(0, Mathf.FloorToInt((low.x - origin.x) / cell) - 1);
                int right = Mathf.Min(columns - 1, Mathf.FloorToInt((high.x - origin.x) / cell) + 1);
                int bottom = Mathf.Max(0, Mathf.FloorToInt((low.y - origin.y) / cell) - 1);
                int top = Mathf.Min(rows - 1, Mathf.FloorToInt((high.y - origin.y) / cell) + 1);
                for (int y = bottom; y <= top; y++)
                for (int x = left; x <= right; x++)
                {
                    int index = y * columns + x;
                    (blocked[index] ??= new List<Rect>()).Add(bounds);
                }
            }

            var pixels = new Color32[width * height];
            var corners = new Vector2[4];
            for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                int index = y * surface.GridWidth + x;
                int open = 0;
                for (int tap = 0; tap < 4; tap++)
                {
                    int cellIndex = index + (tap & 1) + (tap >> 1) * surface.GridWidth;
                    corners[tap] = surface.GridPoint(cellIndex);
                    if (surface.GridOpen(cellIndex)) open |= 1 << tap;
                }
                if (open == 0) continue;
                var walls = blocked[y * columns + x];
                for (int sy = 0; sy < subdivisions; sy++)
                for (int sx = 0; sx < subdivisions; sx++)
                {
                    int mask = open;
                    if (walls != null)
                    {
                        Vector2 point = corners[0] + new Vector2((sx + .5f) * step, (sy + .5f) * step);
                        for (int tap = 0; tap < 4; tap++)
                        {
                            int bit = 1 << tap;
                            if ((mask & bit) == 0) continue;
                            foreach (Rect bounds in walls)
                            {
                                if (!Intersects(point, corners[tap], bounds)) continue;
                                mask &= ~bit;
                                break;
                            }
                        }
                    }
                    pixels[(y * subdivisions + sy) * width + x * subdivisions + sx] = new Color32((byte)(mask * 17), 0, 0, 255);
                }
            }

            var source = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                source.SetPixels32(pixels);
                File.WriteAllBytes(MaskPath, source.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
            AssetDatabase.ImportAsset(MaskPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(MaskPath);
            importer.textureType = TextureImporterType.SingleChannel;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.singleChannelComponent = TextureImporterSingleChannelComponent.Red;
            importer.SetTextureSettings(settings);
            importer.sRGBTexture = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(MaskPath);
        }

        private static bool Intersects(Vector2 from, Vector2 to, Rect bounds)
        {
            float enter = 0f, exit = 1f;
            Vector2 delta = to - from;
            return ClipAxis(from.x, delta.x, bounds.xMin, bounds.xMax, ref enter, ref exit) &&
                   ClipAxis(from.y, delta.y, bounds.yMin, bounds.yMax, ref enter, ref exit);
        }

        private static bool ClipAxis(float origin, float direction, float low, float high, ref float enter, ref float exit)
        {
            if (Mathf.Abs(direction) < .000001f) return origin >= low && origin <= high;
            float a = (low - origin) / direction, b = (high - origin) / direction;
            enter = Mathf.Max(enter, Mathf.Min(a, b));
            exit = Mathf.Min(exit, Mathf.Max(a, b));
            return enter <= exit;
        }
    }
}
