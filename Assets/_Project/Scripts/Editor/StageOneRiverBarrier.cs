using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.EditorTools
{
    [InitializeOnLoad]
    internal static class StageOneRiverBarrier
    {
        private const string ScenePath = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference.unity";
        private const string Request = "Library/HerbalistRiverBarrier.request";
        private const string BarrierName = "RiverEntryBarrier";
        private const float Step = 3f;
        private const float WallHeight = 60f;
        private const float WallThickness = 2f;

        static StageOneRiverBarrier()
        {
            if (File.Exists(Request))
                EditorApplication.delayCall += Apply;
        }

        [MenuItem("Tools/Herbalist/Stage 1/Create River Entry Barrier")]
        private static void Apply()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var roots = scene.GetRootGameObjects();
            var water = roots.SingleOrDefault(root => root.name == "water");
            if (water == null)
                return;

            var planes = water.GetComponentsInChildren<MeshFilter>()
                .Where(filter => filter.transform.parent == water.transform &&
                    filter.name.StartsWith("Plane", System.StringComparison.Ordinal) &&
                    filter.sharedMesh != null && filter.GetComponent<MeshRenderer>()?.enabled == true)
                .ToArray();
            var terrains = Terrain.activeTerrains.Where(terrain => terrain.gameObject.scene == scene).ToArray();
            if (planes.Length == 0 || terrains.Length == 0)
                return;

            var area = planes[0].GetComponent<MeshRenderer>().bounds;
            foreach (var plane in planes)
                area.Encapsulate(plane.GetComponent<MeshRenderer>().bounds);
            float waterY = planes[0].transform.position.y;
            int width = Mathf.CeilToInt(area.size.x / Step);
            int length = Mathf.CeilToInt(area.size.z / Step);
            var heights = new float[width + 1, length + 1];
            for (int x = 0; x <= width; x++)
            for (int z = 0; z <= length; z++)
            {
                var point = new Vector3(area.min.x + x * Step, waterY, area.min.z + z * Step);
                heights[x, z] = HeightFromWater(point, terrains);
            }

            // In this scene the playable bank is on the positive X side of the river.
            var segments = new System.Collections.Generic.List<(Vector3 start, Vector3 end)>();
            var bank = new System.Collections.Generic.List<Vector3>();
            for (int z = 0; z <= length; z++)
            {
                Vector3? shore = null;
                for (int x = width - 1; x >= 0; x--)
                {
                    float wet = heights[x, z];
                    float dry = heights[x + 1, z];
                    if (!(wet < 0f && dry >= 0f))
                        continue;

                    float crossing = x + wet / (wet - dry);
                    var point = new Vector3(area.min.x + crossing * Step, waterY, area.min.z + z * Step);
                    if (!planes.Any(plane => Covers(plane, point)))
                        continue;

                    shore = point;
                    break;
                }

                if (!shore.HasValue || (bank.Count > 0 &&
                    Vector3.Distance(bank[bank.Count - 1], shore.Value) > Step * 4f))
                {
                    AddBankSegments(bank, segments);
                    bank.Clear();
                }

                if (shore.HasValue)
                    bank.Add(shore.Value);
            }
            AddBankSegments(bank, segments);
            if (segments.Count == 0)
            {
                Debug.LogWarning("[StageOneRiverBarrier] No water shoreline found.");
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Stage One River Entry Barrier");
            bool wasDirty = scene.isDirty;
            var previous = water.transform.Find(BarrierName);
            var barrier = new GameObject(BarrierName);
            SceneManager.MoveGameObjectToScene(barrier, scene);
            Undo.RegisterCreatedObjectUndo(barrier, "Create River Entry Barrier");
            Undo.SetTransformParent(barrier.transform, water.transform, "Parent River Entry Barrier");
            int segmentIndex = 0;
            foreach (var (start, end) in segments)
            {
                var wall = new GameObject($"Bank_{++segmentIndex:D3}");
                SceneManager.MoveGameObjectToScene(wall, scene);
                wall.transform.SetParent(barrier.transform, false);
                wall.transform.SetPositionAndRotation(
                    (start + end) * 0.5f + Vector3.up * (WallHeight * 0.5f - 1.5f),
                    Quaternion.LookRotation(end - start));
                var box = wall.AddComponent<BoxCollider>();
                box.size = new Vector3(WallThickness, WallHeight, Vector3.Distance(start, end) + WallThickness);
                Undo.RegisterCreatedObjectUndo(wall, "Create Bank Collider");
            }

            if (previous != null)
                Undo.DestroyObjectImmediate(previous.gameObject);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = !wasDirty && EditorSceneManager.SaveScene(scene);
            if (File.Exists(Request))
                File.Delete(Request);
            Debug.Log($"[StageOneRiverBarrier] Created {segments.Count} bank colliders. Saved: {saved}.");
        }

        private static void AddBankSegments(System.Collections.Generic.List<Vector3> bank,
            System.Collections.Generic.List<(Vector3 start, Vector3 end)> segments)
        {
            const float maxDeviation = 1.25f;
            const float maxLength = 30f;
            int start = 0;
            while (start + 1 < bank.Count)
            {
                int end = start + 1;
                while (end + 1 < bank.Count &&
                    Vector3.Distance(bank[start], bank[end + 1]) <= maxLength)
                {
                    var span = bank[end + 1] - bank[start];
                    bool fits = true;
                    for (int i = start + 1; i <= end; i++)
                    {
                        var offset = bank[i] - bank[start];
                        if (Vector3.Cross(span, offset).magnitude <= maxDeviation * span.magnitude)
                            continue;

                        fits = false;
                        break;
                    }
                    if (!fits)
                        break;
                    end++;
                }
                segments.Add((bank[start], bank[end]));
                start = end;
            }
        }
        private static float HeightFromWater(Vector3 point, Terrain[] terrains)
        {
            float height = float.NaN;
            foreach (var terrain in terrains)
            {
                var origin = terrain.GetPosition();
                var size = terrain.terrainData.size;
                if (point.x < origin.x || point.z < origin.z ||
                    point.x > origin.x + size.x || point.z > origin.z + size.z)
                    continue;

                float level = origin.y + terrain.SampleHeight(point) - point.y;
                height = float.IsNaN(height) ? level : Mathf.Max(level, height);
            }

            return height;
        }

        private static bool Covers(MeshFilter plane, Vector3 point)
        {
            var local = plane.transform.InverseTransformPoint(point);
            var bounds = plane.sharedMesh.bounds;
            return local.x >= bounds.min.x && local.x <= bounds.max.x &&
                   local.z >= bounds.min.z && local.z <= bounds.max.z;
        }
    }
}