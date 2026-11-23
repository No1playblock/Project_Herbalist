using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.EditorTools
{
    [InitializeOnLoad]
    internal static class StageOneSmallRockColliders
    {
        private const string ScenePath = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference.unity";
        private const string Request = "Library/HerbalistTreeRockColliders.request";
        private const string TreeMeshGuid = "f133c2dcc84b2894293207a0b5705595";
        private const string UndoName = "Set Stage One Rock Colliders";

        static StageOneSmallRockColliders()
        {
            if (File.Exists(Request))
                EditorApplication.delayCall += Apply;
        }

        [MenuItem("Tools/Herbalist/Stage 1/Set Rock Colliders")]
        private static void Apply()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var transforms = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var treeMesh = AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(TreeMeshGuid));
            var tree = transforms.Select(target => target.GetComponent<MeshCollider>())
                .FirstOrDefault(collider => collider != null && treeMesh != null && collider.sharedMesh == treeMesh && collider.enabled);
            if (tree == null)
            {
                Debug.LogWarning("[StageOneRockColliders] Playable tree collision mesh was not found.");
                return;
            }

            Physics.SyncTransforms();
            bool wasDirty = scene.isDirty;
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            int meshCount = 0;
            int boxCount = 0;
            int changed = 0;
            foreach (var target in transforms)
            {
                if (!target.gameObject.activeInHierarchy || !IsRockRoot(target.name))
                    continue;

                var meshCollider = target.GetComponentsInChildren<MeshCollider>(false).FirstOrDefault(collider =>
                    !collider.isTrigger && collider.sharedMesh != null &&
                    (collider.transform == target || collider.name.EndsWith("_MeshCollider", System.StringComparison.Ordinal)));
                var filter = target.GetComponent<MeshFilter>();
                if (filter == null)
                    filter = target.GetComponentsInChildren<MeshFilter>(false)
                        .FirstOrDefault(candidate => candidate.name.EndsWith("_LOD0", System.StringComparison.Ordinal));
                var source = meshCollider != null ? meshCollider.transform : filter != null ? filter.transform : null;
                var mesh = meshCollider != null ? meshCollider.sharedMesh : filter != null ? filter.sharedMesh : null;
                if (source == null || mesh == null || mesh.bounds.size.sqrMagnitude < 0.000001f ||
                    source.GetComponentInParent<Rigidbody>() != null)
                    continue;

                var box = target.GetComponentsInChildren<BoxCollider>(false).FirstOrDefault(collider =>
                    !collider.isTrigger && (collider.transform == target || collider.transform == source ||
                        collider.name.EndsWith("_MeshCollider", System.StringComparison.Ordinal)));
                bool touchesTree = TouchesTree(tree, source, mesh.bounds);
                if (touchesTree)
                {
                    meshCount++;
                    if (meshCollider == null)
                    {
                        meshCollider = Undo.AddComponent<MeshCollider>(source.gameObject);
                        meshCollider.sharedMesh = mesh;
                        CopySettings(box, meshCollider);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(meshCollider);
                        changed++;
                    }
                    if (box != null)
                    {
                        Undo.DestroyObjectImmediate(box);
                        changed++;
                    }
                }
                else
                {
                    boxCount++;
                    var fit = BoundsInTransform(source, mesh.bounds, box != null ? box.transform : source);
                    if (box == null || meshCollider != null || box.center != fit.center || box.size != fit.size)
                    {
                        if (box == null)
                            box = Undo.AddComponent<BoxCollider>(source.gameObject);
                        Undo.RecordObject(box, UndoName);
                        box.center = fit.center;
                        box.size = fit.size;
                        CopySettings(meshCollider, box);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(box);
                        if (meshCollider != null)
                            Undo.DestroyObjectImmediate(meshCollider);
                        changed++;
                    }
                }
            }

            Undo.CollapseUndoOperations(group);
            if (changed > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            bool saved = changed > 0 && !wasDirty && EditorSceneManager.SaveScene(scene);
            if (File.Exists(Request))
                File.Delete(Request);
            Debug.Log($"[StageOneRockColliders] Mesh: {meshCount}, Box: {boxCount}, changes: {changed}. Saved: {saved}.");
        }

        private static bool IsRockRoot(string name)
        {
            return Regex.IsMatch(name, @"^(?:SM_Rocks_0[1-6]|Small_Rock[1-5]|TSI_Rock_(?:Small|Medium|Large)_\d{2}[AB])(?: \(\d+\))?$");
        }

        private static bool TouchesTree(MeshCollider tree, Transform source, Bounds meshBounds)
        {
            var rock = BoundsInTransform(source, meshBounds, null);
            rock.Expand(0.5f);
            if (!tree.bounds.Intersects(rock))
                return false;

            // Query the tree surface across the rock volume in both directions.
            for (int axis = 0; axis < 3; axis++)
            {
                int u = (axis + 1) % 3;
                int v = (axis + 2) % 3;
                var direction = Vector3.zero;
                direction[axis] = 1f;
                for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                {
                    var start = rock.center;
                    start[u] += i * rock.extents[u] * 0.85f;
                    start[v] += j * rock.extents[v] * 0.85f;
                    start[axis] = rock.min[axis];
                    if (tree.Raycast(new Ray(start, direction), out _, rock.size[axis]))
                        return true;
                    start[axis] = rock.max[axis];
                    if (tree.Raycast(new Ray(start, -direction), out _, rock.size[axis]))
                        return true;
                }
            }

            return false;
        }

        private static Bounds BoundsInTransform(Transform source, Bounds bounds, Transform destination)
        {
            var result = new Bounds();
            for (int corner = 0; corner < 8; corner++)
            {
                var point = source.TransformPoint(new Vector3(
                    (corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (corner & 4) == 0 ? bounds.min.z : bounds.max.z));
                if (destination != null)
                    point = destination.InverseTransformPoint(point);
                if (corner == 0)
                    result = new Bounds(point, Vector3.zero);
                else
                    result.Encapsulate(point);
            }
            return result;
        }

        private static void CopySettings(Collider source, Collider destination)
        {
            if (source == null)
                return;
            destination.sharedMaterial = source.sharedMaterial;
            destination.contactOffset = source.contactOffset;
            destination.includeLayers = source.includeLayers;
            destination.excludeLayers = source.excludeLayers;
            destination.layerOverridePriority = source.layerOverridePriority;
            destination.providesContacts = source.providesContacts;
        }
    }
}