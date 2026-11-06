using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Applies only authored presentation. Gameplay nodes, collision proxies and configuration stay intact.
    public static class EnvironmentModelingSceneSync
    {
        private const string Import = "Assets/_Project/Art/Environments/EnvironmentModelingImport_20261103/";
        private const string Stage2 = "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity";
        private const string Stage3 = CurrentStagePaths.BranchRideBlockout;
        private const string Reference2 = "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_EnvironmentModeling_Reference.unity";
        private const string Reference3 = "Assets/_Project/Scenes/Stages/Stage_03/Stage_03_EnvironmentModeling_Reference.unity";

        private static Mesh Persistent(Mesh mesh, string key)
        {
            if (mesh == null || EditorUtility.IsPersistent(mesh)) return mesh;
            if (!AssetDatabase.IsValidFolder(Import + "BakedMeshes"))
                AssetDatabase.CreateFolder(Import.TrimEnd('/'), "BakedMeshes");
            string path = Import + "BakedMeshes/M_" + Hash128.Compute(key).ToString() + ".asset";
            var baked = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (baked != null) return baked;
            baked = Object.Instantiate(mesh);
            baked.name = mesh.name + "_Baked";
            AssetDatabase.CreateAsset(baked, path);
            return baked;
        }

        private static string Relative(Transform transform, Transform root)
        {
            string name = transform.name;
            while (transform.parent != null && transform.parent != root)
            { transform = transform.parent; name = transform.name + "/" + name; }
            return name;
        }

        private static void RemoveDeformers(GameObject root)
        {
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && component.GetType().Name.StartsWith("Lattice"))
                    Object.DestroyImmediate(component);
        }

        private static GameObject CloneVisual(GameObject source, Transform parent, string key)
        {
            var filters = source.GetComponentsInChildren<MeshFilter>(true);
            var meshes = filters.Select(f => Persistent(f.sharedMesh, key + "/" + Relative(f.transform, source.transform))).ToArray();
            var copy = Object.Instantiate(source);
            copy.name = source.name;
            copy.transform.SetParent(parent, false);
            copy.transform.localPosition = source.transform.position;
            copy.transform.localRotation = source.transform.rotation;
            copy.transform.localScale = source.transform.lossyScale;
            RemoveDeformers(copy);
            var copies = copy.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < copies.Length; i++) copies[i].sharedMesh = meshes[i];
            foreach (var collider in copy.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            return copy;
        }

        [MenuItem("Herbalist/Stages/Apply Environment Modeling Stage 2")]
        public static void ApplyStage2()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit mode required.");
            var scene = EditorSceneManager.OpenScene(Stage2, OpenSceneMode.Single);
            var environment = GameObject.Find("Stage2_EnvironmentModeling").transform;
            var mechanisms = GameObject.Find("Stage2_OriginalMechanisms").transform;
            if (environment.Find("EnvironmentModeling_20261103") != null)
                throw new InvalidOperationException("Stage 2 import already applied.");
            var reference = EditorSceneManager.OpenPreviewScene(Reference2);
            try
            {
                var roots = reference.GetRootGameObjects();
                var pack = roots.First(r => r.name == "Stage_2_Assets_Pack");
                var sources = pack.GetComponentsInChildren<MeshFilter>(true)
                    .GroupBy(f => Relative(f.transform, pack.transform) + "|" + f.sharedMesh?.name)
                    .ToDictionary(g => g.Key, g => g.First());
                int meshes = 0, materials = 0;
                foreach (var target in mechanisms.GetComponentsInChildren<MeshFilter>(true))
                {
                    string key = Relative(target.transform, mechanisms) + "|" + target.sharedMesh?.name;
                    if (!sources.TryGetValue(key, out var source)) continue;
                    // Shape equality was checked before replacing collision-adjacent visuals.
                    if (target.sharedMesh != null && source.sharedMesh != null &&
                        target.sharedMesh.vertexCount == source.sharedMesh.vertexCount &&
                        Vector3.Distance(target.sharedMesh.bounds.size, source.sharedMesh.bounds.size) < .01f)
                    { target.sharedMesh = Persistent(source.sharedMesh, "Stage2/Mechanisms/" + key); meshes++; }
                    var renderer = target.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    { renderer.sharedMaterials = source.GetComponent<MeshRenderer>().sharedMaterials; materials++; }
                }
                foreach (var renderer in environment.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                var imported = new GameObject("EnvironmentModeling_20261103").transform;
                imported.SetParent(environment, false);
                foreach (var source in roots.Where(r => r.name.StartsWith("Root_Big") || r.name == "GameObject" ||
                    r.name.StartsWith("Mushroom") || r.name.StartsWith("Quad") || r.name.StartsWith("Point Light") ||
                    r.name == "Spot Light" || r.name == "GlobalVolume"))
                {
                    var copy = CloneVisual(source, imported, "Stage2/" + source.name);
                    // Preserve the original effect for inspection without obscuring the gameplay cameras.
                    if (source.name.StartsWith("Quad") || source.name == "GlobalVolume") copy.SetActive(false);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Stage 2 environment applied: {meshes} compatible mesh replacements, {materials} material sets. Gameplay/colliders preserved.");
            }
            finally { EditorSceneManager.ClosePreviewScene(reference); }
        }

        private static void FitSkin(MeshFilter target, Mesh mesh, Material[] materials, Quaternion rotation, string name)
        {
            if (target.transform.Find(name) != null) throw new InvalidOperationException("Skin already exists: " + target.name);
            Bounds destination = target.sharedMesh.bounds;
            Vector3 rotatedSize = rotation * mesh.bounds.size;
            rotatedSize = new Vector3(Mathf.Abs(rotatedSize.x), Mathf.Abs(rotatedSize.y), Mathf.Abs(rotatedSize.z));
            Vector3 scaleInTarget = new Vector3(destination.size.x / rotatedSize.x,
                destination.size.y / rotatedSize.y, destination.size.z / rotatedSize.z);
            Vector3 scale = Quaternion.Inverse(rotation) * scaleInTarget;
            scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            var skin = new GameObject(name).transform;
            skin.SetParent(target.transform, false);
            skin.localRotation = rotation;
            skin.localScale = scale;
            skin.localPosition = destination.center - rotation * Vector3.Scale(mesh.bounds.center, scale);
            skin.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = skin.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            target.GetComponent<MeshRenderer>().enabled = false;
        }

        [MenuItem("Herbalist/Stages/Apply Environment Modeling Stage 3")]
        public static void ApplyStage3()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit mode required.");
            var scene = EditorSceneManager.OpenScene(Stage3, OpenSceneMode.Single);
            var layout = GameObject.Find("StageThree_BranchRideLayout").transform;
            if (layout.Find("EnvironmentModeling_20261103") != null)
                throw new InvalidOperationException("Stage 3 import already applied.");
            var branchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Import + "Models/1027_Branch_Fbx_4a5376d5.fbx");
            var branchMesh = branchPrefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            var bark = AssetDatabase.LoadAssetAtPath<Material>(Import + "Materials/Main_Branch_a1538192.mat");
            int count = 0;
            foreach (var track in layout.GetComponentsInChildren<BranchRideTrack>(true))
            {
                if (track.IsDrop) continue;
                foreach (var filter in track.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.GetComponent<MeshRenderer>() == null || filter.sharedMesh == null) continue;
                    FitSkin(filter, branchMesh, new[] { bark }, Quaternion.Euler(0, 0, 90), "ModeledBranchVisual");
                    count++;
                }
            }
            var reference = EditorSceneManager.OpenPreviewScene(Reference3);
            try
            {
                var roots = reference.GetRootGameObjects();
                var fabric = roots.First(r => r.name == "Fabric_unwarp").GetComponentInChildren<MeshFilter>();
                var talisman = layout.GetComponentsInChildren<Transform>(true).First(t => t.name == "SharedTalisman");
                var paper = talisman.Find("Paper").GetComponent<MeshFilter>();
                FitSkin(paper, fabric.sharedMesh, fabric.GetComponent<MeshRenderer>().sharedMaterials,
                    Quaternion.identity, "ModeledFabricVisual");
                foreach (var renderer in talisman.GetComponentsInChildren<MeshRenderer>(true))
                    if (renderer.name.StartsWith("SealStroke")) renderer.enabled = false;
                var imported = new GameObject("EnvironmentModeling_20261103").transform;
                imported.SetParent(layout, false);
                // The reference art is a Z-axis tunnel. Existing gameplay follows +X.
                var turn = Quaternion.Euler(0, 90, 0);
                var sourceCenter = new Vector3(47.8f, 14.3f, 51.62f);
                for (int section = 0; section < 4; section++)
                {
                    var frame = new GameObject("CaveSection_" + (section + 1)).transform;
                    frame.SetParent(imported, false);
                    frame.localRotation = turn;
                    frame.localScale = Vector3.one * 2.7f;
                    float height = section == 0 ? 24f : section == 3 ? 10f : 16f;
                    frame.localPosition = new Vector3(section * 45f, height, 0) - turn * (sourceCenter * 2.7f);
                    foreach (var source in roots.Where(r => r.name.StartsWith("Tree_Cave_Un") || r.name.StartsWith("Tree_Cave_Root")))
                        CloneVisual(source, frame, "Stage3/" + source.name);
                    var lightRoot = new GameObject("CaveFillLight_" + (section + 1)).transform;
                    lightRoot.SetParent(imported, false);
                    lightRoot.localPosition = new Vector3(section * 45f + 15f, height + 10f, 0f);
                    var light = lightRoot.gameObject.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(.9f, .95f, 1f);
                    light.range = 65f;
                    light.intensity = 800f;
                    light.shadows = LightShadows.None;
                }
                var trunk = roots.First(r => r.name == "Root_Big1");
                var background = new GameObject("BackgroundTrunk").transform;
                background.SetParent(imported, false);
                background.localPosition = new Vector3(-45, -70, 40) - trunk.transform.position;
                CloneVisual(trunk, background, "Stage3/Root_Big1");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"Stage 3 environment applied: {count} branch skins, modeled fabric and collider-free cave sections. Track nodes and gameplay remain unchanged.");
            }
            finally { EditorSceneManager.ClosePreviewScene(reference); }
        }
    }
}
