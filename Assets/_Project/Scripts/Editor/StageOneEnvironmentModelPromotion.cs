using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class StageOneEnvironmentModelPromotion
    {
        public const string Main = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference.unity";
        public const string Reference = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_20261108_Reference.unity";
        public const string RootName = "Stage1_EnvironmentModels_20261108";
        private const string ModelFolder = "Assets/_Project/Art/Environments/Stage_01/DevelopStage1Import_20261108/Models/";
        private static readonly string[] OldRoots = {
            "out_stone/TSI_Cliff_04A (1)", "out_stone/TSI_Background_Cliff_05A",
            "water_fall/TSI_Cliff_01A", "water_fall/TSI_Background_Cliff_04A",
            "water_fall/TSI_Background_Cliff_06A", "water_fall/TSI_Background_Cliff_06A (1)",
            "water_fall/TSI_Background_Cliff_05A", "cave/TSI_Cliff_04A",
            "cave_rock", "water_fall/waterfall_rock1", "water_fall/waterfall_rock2", "stair"
        };

        private static IEnumerable<T> All<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true));
        private static string Path(Transform t) => AnimationUtility.CalculateTransformPath(t, null);
        private static string Key(Object value)
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(value);
            return id.targetPrefabId + ":" + id.targetObjectId;
        }
        private static void Dirty(Component component)
        {
            EditorUtility.SetDirty(component);
            if (PrefabUtility.IsPartOfPrefabInstance(component))
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }

        public static string Apply()
        {
            var main = SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || main.path != Main || main.isDirty)
                throw new InvalidOperationException("Open the saved main Stage One scene outside Play mode first.");
            if (main.GetRootGameObjects().Any(g => g.name == RootName))
                throw new InvalidOperationException("New environment models are already installed.");

            var oldTransforms = All<Transform>(main).ToDictionary(t => t, t => new {
                parent = t.parent, position = t.localPosition, rotation = t.localRotation,
                scale = t.localScale, active = t.gameObject.activeSelf
            });
            var scripts = All<MonoBehaviour>(main).Where(m => m != null)
                .ToDictionary(m => m, m => EditorJsonUtility.ToJson(m));
            var triggers = All<Collider>(main).Where(c => c.isTrigger)
                .ToDictionary(c => c, c => EditorJsonUtility.ToJson(c));
            var preview = EditorSceneManager.OpenPreviewScene(Reference);
            var created = new List<GameObject>();
            try
            {
                var roots = OldRoots.ToDictionary(p => p,
                    p => oldTransforms.Keys.Single(t => Path(t) == p));
                if (roots.Values.Any(t => t.GetComponents<MonoBehaviour>().Any(m => m != null)))
                    throw new Exception("Unexpected gameplay script on an old model root.");
                var sourceFilters = All<MeshFilter>(preview).Where(f => f.sharedMesh != null &&
                    AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(ModelFolder) &&
                    !AssetDatabase.GetAssetPath(f.sharedMesh).Contains("/SeonangTree/")).ToArray();
                if (sourceFilters.Length != 26 || sourceFilters.Any(f => f.transform.childCount != 0 ||
                    f.GetComponents<MonoBehaviour>().Any(m => m != null)))
                    throw new Exception("Reference environment model layout differs from the reviewed 26 static pieces.");

                var referenceKeys = new HashSet<string>(All<MeshFilter>(preview).Select(Key));
                var retiring = roots.Values.SelectMany(t => t.GetComponentsInChildren<MeshFilter>(true)
                    .Where(f => f.transform == t || f.name.StartsWith(t.name.Replace(" (1)", "") + "_LOD")))
                    .Where(f => f.sharedMesh != null && !referenceKeys.Contains(Key(f)))
                    .Select(f => f.GetComponent<Renderer>()).Where(r => r != null).Distinct().ToArray();
                // Props attached below a cliff keep their own renderer and collider. Only the
                // cliff's own collision mesh and root LODGroup are retired.
                var retiringColliders = roots.Values.SelectMany(t => {
                    string name = t.name.Replace(" (1)", "");
                    return t.GetComponentsInChildren<Collider>(true).Where(c => !c.isTrigger &&
                        (c.transform == t || c.name == name + "_MeshCollider"));
                }).Distinct().ToArray();
                var lods = roots.Values.Select(t => t.GetComponent<LODGroup>()).Where(l => l != null).ToArray();
                var oldRendererState = retiring.ToDictionary(r => r, r => r.enabled);
                var oldColliderState = retiringColliders.ToDictionary(c => c, c => c.enabled);
                var oldLodState = lods.ToDictionary(l => l, l => l.enabled);
                foreach (var script in scripts.Keys)
                {
                    var property = new SerializedObject(script).GetIterator();
                    while (property.Next(true))
                        if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            property.objectReferenceValue is Collider referenced && retiringColliders.Contains(referenced))
                            throw new Exception("Rebind this gameplay collider before promotion: " + script.name + "." + property.propertyPath);
                }
                System.IO.File.Copy(Main, "Temp/Stage1BeforeEnvironmentModelPromotion.unity", true);

                var root = new GameObject(RootName); created.Add(root);
                SceneManager.MoveGameObjectToScene(root, main);
                var groups = new[] { "Cliffs", "WaterfallRocks", "CaveRock", "Stairs" }
                    .ToDictionary(n => n, n => { var g = new GameObject(n); g.transform.SetParent(root.transform, false); return g.transform; });
                try
                {
                    foreach (var source in sourceFilters)
                    {
                        string group = source.name.StartsWith("wood_stair") ? "Stairs" :
                            source.name.StartsWith("waterfall") ? "WaterfallRocks" :
                            source.name.StartsWith("cave") ? "CaveRock" : "Cliffs";
                        var copy = Object.Instantiate(source.gameObject, groups[group], false);
                        copy.name = source.name;
                        copy.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
                        copy.transform.localScale = source.transform.lossyScale;
                        copy.SetActive(true);
                        string layerRoot = group == "Stairs" ? "stair" : group == "CaveRock" ? "cave_rock" :
                            source.name == "waterfall_rock1_low" ? "water_fall/waterfall_rock1" :
                            source.name == "waterfall_rock_low" ? "water_fall/waterfall_rock2" :
                            source.name == "cliff3_low" ? "out_stone/TSI_Background_Cliff_05A" :
                            source.name == "cliff2_low (1)" ? "cave/TSI_Cliff_04A" : "water_fall/TSI_Cliff_01A";
                        copy.layer = roots[layerRoot].gameObject.layer;
                        var renderer = copy.GetComponent<MeshRenderer>();
                        renderer.enabled = true;
                        if (renderer.sharedMaterials.Any(m => m == null)) throw new Exception("Missing material: " + source.name);
                        var collider = copy.GetComponent<MeshCollider>();
                        if (collider == null) collider = copy.AddComponent<MeshCollider>();
                        collider.convex = false; collider.isTrigger = false; collider.enabled = true;
                        collider.sharedMesh = copy.GetComponent<MeshFilter>().sharedMesh;
                    }
                    foreach (var lod in lods) { lod.enabled = false; Dirty(lod); }
                    foreach (var renderer in retiring) { renderer.enabled = false; Dirty(renderer); }
                    foreach (var collider in retiringColliders) { collider.enabled = false; Dirty(collider); }
                    if (scripts.Any(p => p.Key == null || EditorJsonUtility.ToJson(p.Key) != p.Value) ||
                        triggers.Any(p => p.Key == null || EditorJsonUtility.ToJson(p.Key) != p.Value))
                        throw new Exception("Existing gameplay or trigger data changed.");
                    if (oldTransforms.Any(p => p.Key == null || p.Key.parent != p.Value.parent ||
                        p.Key.localPosition != p.Value.position || p.Key.localRotation != p.Value.rotation ||
                        p.Key.localScale != p.Value.scale || p.Key.gameObject.activeSelf != p.Value.active))
                        throw new Exception("Existing object placement or activation changed.");
                    Physics.SyncTransforms();
                    EditorSceneManager.MarkSceneDirty(main); EditorSceneManager.SaveScene(main);
                    string report = "Applied 26 models (4 cliffs, 2 waterfall rocks, 1 cave rock, 19 stair pieces); " +
                        retiring.Length + " old renderers, " + retiringColliders.Length + " old solid colliders and " +
                        lods.Length + " LODGroups retired; preserved " + scripts.Count + " gameplay components, " +
                        triggers.Count + " triggers and " + oldTransforms.Count + " existing transforms.";
                    System.IO.File.WriteAllText("Temp/Stage1EnvironmentModelPromotion.txt", report);
                    return report;
                }
                catch
                {
                    foreach (var p in oldRendererState) { p.Key.enabled = p.Value; Dirty(p.Key); }
                    foreach (var p in oldColliderState) { p.Key.enabled = p.Value; Dirty(p.Key); }
                    foreach (var p in oldLodState) { p.Key.enabled = p.Value; Dirty(p.Key); }
                    foreach (var g in created) if (g != null) Object.DestroyImmediate(g);
                    throw;
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(preview); SceneManager.SetActiveScene(main); }
        }
    }
}
