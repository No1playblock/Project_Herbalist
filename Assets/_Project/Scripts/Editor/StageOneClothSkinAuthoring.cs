using System;
using System.Linq;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class StageOneClothSkinAuthoring
    {
        private static T Read<T>(Object o, string field) where T : Object => (T)new SerializedObject(o).FindProperty(field).objectReferenceValue;
        private static void Set(Object o, string field, Object value)
        { var so = new SerializedObject(o); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Dirty(Object o)
        {
            EditorUtility.SetDirty(o);
            if (o is Component c && PrefabUtility.IsPartOfPrefabInstance(c)) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        }
        public static string Apply()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != StageOneDocumentAuthoring.ScenePath) throw new InvalidOperationException("Imported Stage One Edit mode required.");
            var document = GameObject.Find("Stage1_DocumentGimmicks").transform;
            if (document.Find("ClothSimulations") != null) throw new InvalidOperationException("Cloth skin binding already authored.");
            System.IO.Directory.CreateDirectory("Temp/ClothSkinBinding_20261109");
            EditorSceneManager.SaveScene(scene, "Temp/ClothSkinBinding_20261109/before.unity", true);
            var group = new GameObject("ClothSimulations").transform; group.SetParent(document, false);
            const string folder = "Assets/_Project/Data/Stages/Stage_01/ClothSkins";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_Project/Data/Stages/Stage_01", "ClothSkins");
            int count = 0;
            foreach (var driver in Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None).Where(c => c.enabled))
            {
                var art = Read<Transform>(driver, "_externalVisualRoot"); if (art == null) continue;
                var fabric = art.Find("fabric"); var filter = fabric.GetComponent<MeshFilter>(); var source = filter.sharedMesh;
                var knot = art.Find("knot_low").GetComponent<MeshFilter>();
                var bounds = source.bounds;
                Vector3 top = driver.transform.position;
                Vector3 end = fabric.TransformPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
                Vector3 axis = end - top;
                var points = source.vertices.Select(v => fabric.TransformPoint(v) - top).ToArray();
                var normals = source.normals.Select(n => fabric.TransformDirection(n).normalized).ToArray();
                var mesh = Object.Instantiate(source); mesh.name = driver.name + " two-end cloth";
                mesh.vertices = points; mesh.normals = normals;
                var weights = new BoneWeight[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    float t = Mathf.Clamp01(Vector3.Dot(points[i], axis) / axis.sqrMagnitude);
                    weights[i] = new BoneWeight { boneIndex0 = 0, boneIndex1 = 1, weight0 = 1 - t, weight1 = t };
                }
                mesh.boneWeights = weights;
                var originalCloth = fabric.GetComponent<Cloth>(); originalCloth.enabled = false;
                var originalSkin = fabric.GetComponent<SkinnedMeshRenderer>(); originalSkin.enabled = false;
                var originalRigid = fabric.GetComponent<MeshRenderer>(); if (originalRigid != null) { originalRigid.enabled = false; Dirty(originalRigid); }
                Dirty(originalCloth); Dirty(originalSkin);
                // Prefab child parenting is restored on Play. Use an authored standalone
                // skin with the original mesh/material/Cloth settings in the stable frame.
                fabric = new GameObject(driver.name + "_Fabric", typeof(MeshFilter), typeof(SkinnedMeshRenderer), typeof(Cloth)).transform;
                fabric.SetParent(group, false);
                fabric.SetPositionAndRotation(top, Quaternion.identity); fabric.localScale = Vector3.one;
                filter = fabric.GetComponent<MeshFilter>();
                var skin = fabric.GetComponent<SkinnedMeshRenderer>(); skin.sharedMaterials = originalSkin.sharedMaterials;
                var simulation = fabric.GetComponent<Cloth>(); EditorUtility.CopySerialized(originalCloth, simulation);
                var topBone = new GameObject("TopAttachment").transform; topBone.SetParent(fabric, false);
                var endBone = new GameObject("KnotAttachment").transform; endBone.SetParent(topBone, false); endBone.position = end;
                mesh.bindposes = new[] { topBone.worldToLocalMatrix * fabric.localToWorldMatrix, endBone.worldToLocalMatrix * fabric.localToWorldMatrix };
                mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, folder + "/" + driver.name + ".asset");
                filter.sharedMesh = mesh; skin.sharedMesh = mesh; skin.bones = new[] { topBone, endBone };
                skin.rootBone = topBone; skin.updateWhenOffscreen = true; skin.forceMatrixRecalculationPerRender = true;
                var rigid = fabric.GetComponent<MeshRenderer>(); if (rigid != null) { rigid.enabled = false; Dirty(rigid); }
                var endMarker = new GameObject("ClothEndAttachment").transform;
                endMarker.SetParent(knot.transform, false); endMarker.position = end;
                var binding = fabric.gameObject.AddComponent<ClothEndpointBinding>();
                Set(binding, "_driver", driver); Set(binding, "_end", endMarker); Set(binding, "_topBone", topBone);
                Set(binding, "_endBone", endBone); Set(binding, "_renderer", skin);
                var bs = new SerializedObject(binding); bs.FindProperty("_restAxis").vector3Value = axis;
                float width = points.Max(p => Vector3.ProjectOnPlane(p, axis).magnitude);
                bs.FindProperty("_boundsPadding").floatValue = Mathf.Max(1, width + .4f); bs.ApplyModifiedPropertiesWithoutUndo();
                // Cloth welds duplicated UV vertices. Paint coefficients in the welded particle order.
                var unique = points.Distinct().ToArray(); var coefficients = new ClothSkinningCoefficient[unique.Length];
                for (int i = 0; i < unique.Length; i++)
                {
                    float t = Mathf.Clamp01(Vector3.Dot(unique[i], axis) / axis.sqrMagnitude);
                    coefficients[i].maxDistance = t < .035f || t > .965f ? 0f : .2f * Mathf.Sin(t * Mathf.PI);
                    coefficients[i].collisionSphereDistance = 0;
                }
                simulation.coefficients = coefficients; simulation.useGravity = true; simulation.damping = .35f;
                // Length is deliberately skinned during lowering/roll-up. Native rest-length
                // constraints must not fight the prescribed endpoint positions.
                simulation.stretchingStiffness = 0; simulation.bendingStiffness = .5f; simulation.useTethers = false;
                simulation.sleepThreshold = 0;
                simulation.clothSolverFrequency = 120; simulation.externalAcceleration = Vector3.zero; simulation.randomAcceleration = Vector3.zero;
                skin.enabled = true; simulation.enabled = true;
                Set(driver, "_authoredVisual", null); // Gameplay anchors remain on the authoritative controller.
                foreach (var o in new Object[] { fabric, filter, skin, simulation, binding, driver }) Dirty(o);
                count++;
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return $"Bound {count} original Cloth renderers to two bones; top and knot bands pinned, middle remains simulated; authoritative gameplay unchanged.";
        }

        public static string RepairPrefabFrames()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit mode required.");
            var group = GameObject.Find("Stage1_DocumentGimmicks/ClothSimulations").transform;
            int count = 0;
            foreach (var old in Object.FindObjectsByType<ClothEndpointBinding>(FindObjectsSortMode.None))
            {
                if (old.transform.parent == group) continue;
                var os = new SerializedObject(old);
                var driver = Read<HangingCloth>(old, "_driver"); var end = Read<Transform>(old, "_end");
                var mesh = old.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                var fabric = new GameObject(driver.name + "_Fabric", typeof(MeshFilter), typeof(SkinnedMeshRenderer), typeof(Cloth)).transform;
                fabric.SetParent(group, false); fabric.position = driver.transform.position;
                var skin = fabric.GetComponent<SkinnedMeshRenderer>();
                skin.sharedMaterials = old.GetComponent<SkinnedMeshRenderer>().sharedMaterials;
                skin.sharedMesh = mesh; fabric.GetComponent<MeshFilter>().sharedMesh = mesh;
                var cloth = fabric.GetComponent<Cloth>(); EditorUtility.CopySerialized(old.GetComponent<Cloth>(), cloth); cloth.enabled = false;
                var topBone = new GameObject("TopAttachment").transform; topBone.SetParent(fabric, false);
                var endBone = new GameObject("KnotAttachment").transform; endBone.SetParent(fabric, false);
                endBone.localPosition = os.FindProperty("_restAxis").vector3Value;
                mesh.bindposes = new[] { topBone.worldToLocalMatrix * fabric.localToWorldMatrix, endBone.worldToLocalMatrix * fabric.localToWorldMatrix };
                Dirty(mesh); skin.bones = new[] { topBone, endBone }; skin.rootBone = topBone; skin.updateWhenOffscreen = true;
                var binding = fabric.gameObject.AddComponent<ClothEndpointBinding>();
                Set(binding, "_driver", driver); Set(binding, "_end", end); Set(binding, "_topBone", topBone); Set(binding, "_endBone", endBone); Set(binding, "_renderer", skin);
                var bs = new SerializedObject(binding); bs.FindProperty("_restAxis").vector3Value = os.FindProperty("_restAxis").vector3Value;
                bs.FindProperty("_boundsPadding").floatValue = os.FindProperty("_boundsPadding").floatValue; bs.ApplyModifiedPropertiesWithoutUndo();
                cloth.coefficients = old.GetComponent<Cloth>().coefficients; cloth.enabled = true;
                var oldSkin = old.GetComponent<SkinnedMeshRenderer>(); oldSkin.enabled = false;
                var oldCloth = old.GetComponent<Cloth>(); oldCloth.enabled = false;
                var oldFilter = old.GetComponent<MeshFilter>();
                var original = PrefabUtility.GetCorrespondingObjectFromOriginalSource(oldFilter);
                if (original != null) { oldFilter.sharedMesh = original.sharedMesh; oldSkin.sharedMesh = original.sharedMesh; Dirty(oldFilter); }
                var originalTop = Read<Transform>(old, "_topBone"); var originalEnd = Read<Transform>(old, "_endBone");
                Object.DestroyImmediate(originalTop.gameObject); Object.DestroyImmediate(originalEnd.gameObject);
                old.gameObject.name = "fabric"; Dirty(old.transform); Dirty(oldSkin); Dirty(oldCloth);
                Object.DestroyImmediate(old); count++;
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            return "Moved " + count + " skins into persistent unit-scale authored frames outside the model prefab hierarchy.";
        }

        public static string FinalizeSimulationSettings()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Edit mode required.");
            foreach (var binding in Object.FindObjectsByType<ClothEndpointBinding>(FindObjectsSortMode.None))
            {
                var cloth = binding.GetComponent<Cloth>();
                cloth.stretchingStiffness = 0; cloth.bendingStiffness = .5f; cloth.useTethers = false; cloth.sleepThreshold = 0;
                var skin = binding.GetComponent<SkinnedMeshRenderer>(); skin.forceMatrixRecalculationPerRender = true;
                skin.bones[1].SetParent(skin.bones[0], true);
                Dirty(cloth); Dirty(skin); Dirty(skin.bones[1]);
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            return "Saved rollout-compatible native Cloth constraints and complete bone hierarchies.";
        }
    }
}
