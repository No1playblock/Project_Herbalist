using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Herbalist.Abilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Explicit asset authoring only. Local +Z points out of the tree; the back sits on its surface.
    public static class TreeSapInletAuthoring
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/InteractiveObjects/PF_TreeSapInlet.prefab";
        private const string MeshPath = "Assets/_Project/Art/Environments/Stage_01/Models/TreeSapInlet";
        private const string Materials = "Assets/_Project/Art/Materials/Environments/TreeSapInlet";
        private const string Bark = "Assets/_Project/Art/Environments/Stage_01/Stage1_Assets/Materials/ImportedFromLatest/Environment/Seonang_Tree_2/seonang_tree1.mat";
        private const int Segments = 64;

        private static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = path.Substring(0, path.LastIndexOf('/'));
            Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static Material Material(string name, Material source, Color tint)
        {
            string path = Materials + "/MAT_" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(source) { name = "MAT_" + name };
            material.color = tint;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Mesh BuildMesh()
        {
            // The apparent recess is in front of the original tree, so opaque bark cannot fill it.
            float[] radius = { 1f, .88f, .70f, .59f, .54f, .43f, .28f, 0f };
            float[] depth = { .008f, .065f, .22f, .27f, .22f, .12f, .035f, .025f };
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            for (int ring = 0; ring < radius.Length; ring++)
                for (int i = 0; i <= Segments; i++)
                {
                    float a = i * Mathf.PI * 2 / Segments;
                    float irregular = 1 + .07f * Mathf.Sin(a * 3 + .8f) + .035f * Mathf.Sin(a * 7);
                    float r = radius[ring] * irregular;
                    var p = new Vector3(Mathf.Cos(a) * r * .8f, Mathf.Sin(a) * r * 1.17f,
                        depth[ring] + Mathf.Sin(a * 5) * .015f * radius[ring]);
                    vertices.Add(p);
                    uv.Add(new Vector2(.41f + p.x * .035f, .58f + p.y * .035f));
                }
            var mesh = new Mesh { name = "TreeSapInlet_Recess", subMeshCount = 5 };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            var triangles = new List<int>[5];
            for (int n = 0; n < triangles.Length; n++) triangles[n] = new List<int>();
            for (int ring = 0; ring < radius.Length - 1; ring++)
                for (int i = 0; i < Segments; i++)
                {
                    int a = ring * (Segments + 1) + i, b = a + Segments + 1;
                    int material = ring < 3 ? 0 : Mathf.Min(ring - 2, 4);
                    triangles[material].AddRange(new[] { a, a + 1, b, a + 1, b + 1, b });
                }
            for (int i = 0; i < triangles.Length; i++) mesh.SetTriangles(triangles[i], i);
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }

        [MenuItem("Herbalist/Art/Create Tree Sap Inlet Prefab")]
        public static string CreatePrefab()
        {
            Folder(MeshPath); Folder(Materials); Folder("Assets/_Project/Prefabs/InteractiveObjects");
            var source = AssetDatabase.LoadAssetAtPath<Material>(Bark);
            if (source == null) throw new InvalidOperationException("Stage One bark material missing.");
            var materials = new[] {
                Material("TreeInlet_Bark", source, Color.white),
                Material("TreeInlet_InnerLip", source, new Color(.55f,.48f,.40f,1)),
                Material("TreeInlet_InnerWall", source, new Color(.24f,.20f,.16f,1)),
                Material("TreeInlet_DeepWall", source, new Color(.07f,.065f,.055f,1)),
                Material("TreeInlet_DarkBack", source, new Color(.012f,.014f,.013f,1)) };
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath + "/TreeSapInlet.asset");
            if (mesh == null) { mesh = BuildMesh(); AssetDatabase.CreateAsset(mesh, MeshPath + "/TreeSapInlet.asset"); }
            var previous = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var root = new GameObject("TreeSapInlet");
            if (previous != null) root.transform.localScale = previous.transform.localScale;
            try
            {
                var port = root.AddComponent<SapInjectionPort>();
                var aim = new GameObject("InjectionAimPoint").transform;
                aim.SetParent(root.transform, false); aim.localPosition = new Vector3(0, 0, .15f);
                var portSo = new SerializedObject(port); portSo.FindProperty("_aimPoint").objectReferenceValue = aim; portSo.ApplyModifiedPropertiesWithoutUndo();
                var visual = new GameObject("BarkRim_InnerWall_DarkBack");
                visual.transform.SetParent(root.transform, false);
                visual.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = visual.AddComponent<MeshRenderer>(); renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var hit = root.AddComponent<BoxCollider>();
                hit.size = new Vector3(1.2f, 1.8f, .25f); hit.center = new Vector3(0, 0, .15f);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
            return PrefabPath;
        }

        public static string ApplySample()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != StageOneDocumentAuthoring.ScenePath || EditorApplication.isPlaying)
                throw new InvalidOperationException("Open the imported Stage One scene in Edit mode.");
            var existing = GameObject.Find("Arrival_LinkedInlet");
            if (existing == null) throw new InvalidOperationException("Linked inlet missing.");
            if (existing.transform.Find("TreeHoleVisual") != null) return "Sample already exists.";
            Physics.SyncTransforms();
            RaycastHit hit;
            if (!Physics.Raycast(existing.transform.position, Vector3.right, out hit, 9) || hit.collider.name != "seonag_tree")
                throw new InvalidOperationException("Expected original tree surface not found.");
            Undo.RecordObject(existing.transform, "Place linked inlet on tree surface");
            existing.transform.SetPositionAndRotation(hit.point + hit.normal * .025f,
                Quaternion.LookRotation(hit.normal, Vector3.up));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var sample = (GameObject)PrefabUtility.InstantiatePrefab(prefab, existing.transform);
            sample.name = "TreeHoleVisual";
            // Keep the existing authoritative receiver and its serialized mechanism connection.
            Object.DestroyImmediate(sample.GetComponent<SapInjectionPort>());
            Object.DestroyImmediate(sample.GetComponent<SphereCollider>());
            sample.transform.localPosition = Vector3.zero;
            sample.transform.localRotation = Quaternion.identity;
            sample.transform.localScale = Vector3.one;
            var filter = sample.GetComponentInChildren<MeshFilter>();
            var localMesh = Object.Instantiate(filter.sharedMesh);
            localMesh.name = "TreeSapInlet_SurfaceMatched";
            var uv = localMesh.uv;
            var vertices = localMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                RaycastHit surface;
                var world = sample.transform.TransformPoint(new Vector3(vertices[i].x, vertices[i].y, 0));
                if (hit.collider.Raycast(new Ray(world + hit.normal * 2, -hit.normal), out surface, 4))
                {
                    uv[i] = surface.textureCoord;
                    vertices[i] = sample.transform.InverseTransformPoint(surface.point + hit.normal * (vertices[i].z + .025f));
                }
            }
            localMesh.uv = uv;
            localMesh.vertices = vertices;
            localMesh.RecalculateNormals(); localMesh.RecalculateBounds(); localMesh.RecalculateTangents();
            AssetDatabase.CreateAsset(localMesh, MeshPath + "/TreeSapInlet_SurfaceMatched.asset");
            filter.sharedMesh = localMesh;
            var collider = existing.GetComponent<SphereCollider>();
            if (collider != null) { collider.radius = .68f; collider.center = new Vector3(0,0,.12f); }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = existing;
            SceneView.lastActiveSceneView?.Frame(new Bounds(existing.transform.position, Vector3.one * 5), false);
            return "Tree hole sample connected to Arrival_LinkedInlet at " + existing.transform.position;
        }
    }
}
