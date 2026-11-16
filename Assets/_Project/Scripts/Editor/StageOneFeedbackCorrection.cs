using System;
using System.Linq;
using Fusion;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class StageOneFeedbackCorrection
    {
        private const string PairName = "BranchHole_LeftLeaf_RightWater_20261109";
        private static T Read<T>(Object obj, string field) where T : Object =>
            (T)new SerializedObject(obj).FindProperty(field).objectReferenceValue;
        private static void Set(Object obj, string field, Object value)
        {
            var so = new SerializedObject(obj); so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo(); Dirty(obj);
        }
        private static void Array(Object obj, string field, Object value)
        {
            var so = new SerializedObject(obj); var p = so.FindProperty(field); p.arraySize = 1;
            p.GetArrayElementAtIndex(0).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); Dirty(obj);
        }
        private static void Dirty(Object obj)
        {
            EditorUtility.SetDirty(obj);
            if (obj is Component c && PrefabUtility.IsPartOfPrefabInstance(c)) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        }
        private static Vector3 End(MeshFilter f, bool top)
        {
            var b = f.sharedMesh.bounds;
            return f.transform.TransformPoint(new Vector3(b.center.x, top ? b.max.y : b.min.y, b.center.z));
        }
        public static string Apply()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != StageOneDocumentAuthoring.ScenePath) throw new InvalidOperationException("Imported Stage One Edit mode required.");
            var root = GameObject.Find("Stage1_DocumentGimmicks").transform;
            if (root.Find(PairName) != null) throw new InvalidOperationException("Feedback correction already applied.");
            System.IO.Directory.CreateDirectory("Temp/Stage1Feedback_20261109");
            EditorSceneManager.SaveScene(scene, "Temp/Stage1Feedback_20261109/before.unity", true);

            int clothCount = 0;
            foreach (var cloth in Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None).Where(c => c.enabled))
            {
                var art = Read<Transform>(cloth, "_externalVisualRoot"); if (art == null) continue;
                var fabric = art.Find("fabric").GetComponent<MeshFilter>();
                var simulation = fabric.GetComponent<Cloth>();
                if (simulation != null) { simulation.enabled = false; Dirty(simulation); }
                var skinned = fabric.GetComponent<SkinnedMeshRenderer>();
                var renderer = fabric.GetComponent<MeshRenderer>();
                if (renderer == null) renderer = fabric.gameObject.AddComponent<MeshRenderer>();
                if (skinned != null)
                {
                    renderer.sharedMaterials = skinned.sharedMaterials;
                    skinned.enabled = false; Dirty(skinned);
                }
                renderer.enabled = true; Dirty(renderer);
                if (cloth.name == "Coop_2_WaterCloth")
                {
                    Vector3 top = End(fabric, true);
                    float length = Vector3.Distance(top, End(fabric, false));
                    var scale = fabric.transform.localScale; scale.y *= (length - 1.5f) / length;
                    fabric.transform.localScale = scale;
                    fabric.transform.position += top - End(fabric, true); Dirty(fabric.transform);
                }
                var knot = art.Find("knot_low").GetComponent<MeshFilter>();
                knot.transform.position += End(fabric, false) + fabric.transform.TransformVector(Vector3.up).normalized * .08f - End(knot, true);
                Dirty(knot.transform);
                var grip = Read<Transform>(cloth, "_knot");
                grip.position = knot.transform.TransformPoint(knot.sharedMesh.bounds.center); Dirty(grip);
                clothCount++;
            }
            foreach (var m in Object.FindObjectsByType<StageOneDocumentMechanism>(FindObjectsSortMode.None))
            {
                if (m.Kind != StageOneDocumentMechanismKind.GrowingMushrooms) continue;
                var so = new SerializedObject(m);
                Vector3 outward = m.name.Contains("Mushroom_1_") ? new Vector3(-2.5827682f, 0, -4.4346256f) : new Vector3(-3.2607641f, 0, -3.1129107f);
                so.FindProperty("_growthWorldOffset").vector3Value = outward.normalized * 1.25f;
                so.ApplyModifiedPropertiesWithoutUndo(); Dirty(m);
            }

            const string materialPath = "Assets/_Project/Art/Materials/Abilities/MAT_StageOneSolidWater.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Abilities/MAT_SapStream.mat"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.SetFloat("_SolidWater", 1); material.SetFloat("_ZWrite", 1); material.SetFloat("_Cull", 0);
            material.renderQueue = 2450; material.SetOverrideTag("RenderType", "Opaque"); Dirty(material);
            int cones = 0;
            foreach (var stream in Object.FindObjectsByType<StageOneFixedWaterStream>(FindObjectsSortMode.None))
            {
                if (Read<CapsuleCollider>(stream, "_pushVolume") == null) continue;
                var child = stream.transform.Find("Conical Water Mesh");
                if (child == null)
                {
                    child = new GameObject("Conical Water Mesh", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                    child.SetParent(stream.transform, false); child.gameObject.layer = stream.gameObject.layer;
                }
                var renderer = child.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; Dirty(renderer);
                Set(stream.GetComponent<MushroomWaterJetPresentation>(), "_conicalSurface", child.GetComponent<MeshFilter>());
                stream.RefreshPresentation(); cones++;
            }

            var source = root.GetComponentsInChildren<ExteriorJumpPad>().Single(p => p.name == "Photo1_Branch_UpperLeaf_LowerWater");
            var sourceData = new SerializedObject(source);
            var pair = new GameObject(PairName); pair.transform.SetParent(root, false); pair.AddComponent<NetworkObject>();
            var pad = pair.AddComponent<ExteriorJumpPad>(); EditorUtility.CopySerialized(source, pad);
            var target = Object.Instantiate(((LeafInstallTarget)sourceData.FindProperty("_sockets").GetArrayElementAtIndex(0).objectReferenceValue).gameObject, pair.transform).GetComponent<LeafInstallTarget>();
            var inlet = Object.Instantiate(((SapInjectionPort)sourceData.FindProperty("_ports").GetArrayElementAtIndex(0).objectReferenceValue).gameObject, pair.transform).GetComponent<SapInjectionPort>();
            var jet = Object.Instantiate((GameObject)sourceData.FindProperty("_jets").GetArrayElementAtIndex(0).objectReferenceValue, pair.transform);
            target.name = "LeftHole_LeafSocket"; inlet.name = "RightHole_WaterInlet"; jet.name = "LeftHole_LaunchJet";
            var ts = new SerializedObject(target);
            ts.FindProperty("targetId").intValue = Object.FindObjectsByType<LeafInstallTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None).Max(t => t.Id) + 1;
            ts.ApplyModifiedPropertiesWithoutUndo();
            // Photo five: the high opening away from the trunk is left; the low opening by the trunk is right.
            Vector3 left = new Vector3(278.4957f, 87.0964f, 330.1857f);
            Vector3 right = new Vector3(287.4343f, 83.2532f, 322.0099f);
            pair.transform.position = left;
            target.transform.SetPositionAndRotation(left + Vector3.up * .12f, Quaternion.identity);
            target.Socket.SetPositionAndRotation(left + Vector3.up * .12f, Quaternion.identity);
            inlet.transform.SetPositionAndRotation(right, Quaternion.identity);
            Read<Transform>(inlet, "_aimPoint").position = right + Vector3.up * .35f;
            jet.transform.position = target.SocketPosition; jet.SetActive(false);
            Array(pad, "_sockets", target); Array(pad, "_ports", inlet); Array(pad, "_leafCenters", target.Socket); Array(pad, "_jets", jet);
            Dirty(target); Dirty(pad);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return $"{clothCount} cloth simulations disabled in favor of existing authored swing; blue grip raised 1.5m; mushrooms move out 1.25m; {cones} closed solid edit/play cones; additional branch_hole paired pad.";
        }
    }
}
