using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.Editor
{
    // One-time, explicit authoring operation. Never runs on import or during play.
    public static class StageOneGimmickRevisionAuthoring
    {
        private const string ScenePath = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch.unity";
        private const string DataPath = "Assets/_Project/Data/Stages/Stage_01";
        private const string MeshPath = "Assets/_Project/Art/Environments/Stage_01/GimmickRevisionMeshes";
        private const string MaterialPath = "Assets/_Project/Art/Materials/Environments/Stage_01Gimmicks";
        private static Transform _root;
        private static Material _moss, _marker;
        private static readonly List<BoxCollider> _mossAreas = new();
        private static readonly List<Collider> _checkpoints = new();
        private static int _id;

        public static string Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop play before authoring.");
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var gameplay = all.First(t => t.name == "Stage1_Gameplay");
            if (gameplay.Find("GimmickRevision_20261103") != null) throw new InvalidOperationException("Revision already authored. Edit its scene objects directly.");
            _root = New("GimmickRevision_20261103", gameplay, Vector3.zero);
            _mossAreas.Clear(); _checkpoints.Clear();
            EnsureFolder(MeshPath); EnsureFolder(MaterialPath);
            _id = all.Select(t => t.GetComponent<LeafInstallTarget>()).Where(t => t != null).Select(t => t.Id).DefaultIfEmpty(0).Max() + 100;
            _moss = MaterialAsset("M_StageOneMoss", new Color(.18f, .34f, .065f));
            _marker = MaterialAsset("M_StageOneLeafZone", new Color(.32f, .85f, .22f));

            var level = all.Select(t => t.GetComponent<StageLevel>()).First(t => t != null);
            var rules = UnityEngine.Object.Instantiate(level.abilityRules);
            rules.name = "SO_StageOneGimmickAbilityRules";
            var leaf = UnityEngine.Object.Instantiate(rules.leaf);
            leaf.name = "SO_StageOneTimedLeaf";
            leaf.permanentInstallation = false;
            leaf.lifetime = 5;
            AssetDatabase.CreateAsset(leaf, DataPath + "/" + leaf.name + ".asset");
            rules.leaf = leaf;
            rules.leafInstructions = "R: 지정된 초록 표식에 잎 설치 · 5초 후 회수 · 좌클릭: 오래된 잎 회수";
            rules.sapInstructions = "R 유지: 수액 발사 · 천을 맞혀 밀기 · 천 탑승 중 발사 불가";
            AssetDatabase.CreateAsset(rules, DataPath + "/" + rules.name + ".asset");
            level.abilityRules = rules;
            EditorUtility.SetDirty(level);

            // Only the ordinary ascending mushroom steps are replaced. Injection pads remain.
            int removed = 0;
            var candidates = all.Where(t => t.gameObject.activeInHierarchy && t.GetComponent<MeshRenderer>() != null &&
                (t.parent != null && t.parent.name == "AuthoredStaticPlatforms" || t.name.StartsWith("MushroomVisual")) &&
                t.position.y >= 40 && t.position.y <= 130).ToArray();
            foreach (var step in candidates)
            {
                var bounds = step.GetComponent<Renderer>().bounds;
                Zone("LeafStep_" + (++removed).ToString("00") + "_" + step.name,
                    new Vector3(bounds.center.x, bounds.max.y + .15f, bounds.center.z), _root);
                step.gameObject.SetActive(false);
            }

            var treeRoot = all.First(t => t.name == "Tree_playobject");
            var bare = treeRoot.GetComponentsInChildren<Collider>(true).Where(c => c.gameObject.activeInHierarchy && !c.isTrigger &&
                (c.name.ToLowerInvariant().Contains("branch") || c.name == "seonag_tree")).ToList();
            var branchTemplate = treeRoot.Find("branch1_low (27)").GetComponent<MeshFilter>();
            // Reconnect gameplay to three actual authored hanging fabrics, preserving their poses.
            var fabrics = new[] { all.First(t => t.parent != null && t.parent.name == "movefabric (2)" && t.name == "fabric"),
                treeRoot.Find("movefabric (1)/fabric"), treeRoot.Find("movefabric (4)/fabric") };
            var cloths = all.Select(t => t.GetComponent<HangingCloth>()).Where(c => c != null && c.gameObject.activeInHierarchy).OrderBy(c => c.transform.position.y).ToArray();
            int[] gimmicks = { 1, 2, 5 };
            for (int i = 0; i < cloths.Length; i++)
            {
                var cloth = cloths[i]; var visual = fabrics[i];
                var bounds = visual.GetComponent<Renderer>().bounds;
                var so = new SerializedObject(cloth);
                var knot = (Transform)so.FindProperty("_knot").objectReferenceValue;
                cloth.transform.position = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                cloth.transform.rotation = Quaternion.identity; cloth.transform.localScale = Vector3.one;
                visual = CloneFabric(visual, cloth.transform);
                knot.position = new Vector3(bounds.center.x, bounds.min.y + 1, bounds.center.z);
                so.Update();
                so.FindProperty("_waterDriven").boolValue = true;
                so.FindProperty("_authoredVisual").objectReferenceValue = visual;
                so.FindProperty("_grabText").stringValue = "천을 잡았습니다 · 동료가 수액으로 밀어줍니다 · 상호작용/점프: 놓기";
                so.ApplyModifiedPropertiesWithoutUndo();
                var hit = visual.GetComponent<BoxCollider>();
                if (hit == null) hit = visual.gameObject.AddComponent<BoxCollider>();
                var local = visual.GetComponent<MeshFilter>().sharedMesh.bounds;
                hit.center = local.center;
                hit.size = new Vector3(Mathf.Max(.01f, local.size.x), Mathf.Max(.01f, local.size.y), Mathf.Max(.01f, local.size.z));
                hit.isTrigger = false;
                var group = New("Coop_" + gimmicks[i] + "_WaterClothAndLeaves", _root, Vector3.zero);
                Vector3 start = knot.position + Vector3.down * 2;
                Vector3 direction = i == 1 ? Vector3.forward : Vector3.right;
                float length = bounds.size.y - 1;
                float travel = length * .46f;
                Vector3 end = start + direction * travel + Vector3.up * (length * (1 - Mathf.Sqrt(1 - .46f * .46f)));
                Branch("Start_MossBranch", branchTemplate, start, group);
                Branch("Arrival_MossBranch", branchTemplate, end, group);
                int count = Mathf.Max(1, Mathf.CeilToInt(travel / 10) - 1);
                for (int j = 1; j <= count; j++) Zone("BridgeLeaf_" + j, Vector3.Lerp(start, end, (float)j / (count + 1)) + Vector3.up * .3f, group);
                // Clear authored metadata makes the role order inspectable without a runtime UI.
                group.gameObject.AddComponent<StageOneCoopRoute>().Configure(gimmicks[i], cloth,
                    i == 0 ? "잎: 천으로 도착 후 귀환 잎 경로 설치 / 수액: 천 밀고 잎 경로로 합류" :
                    "잎: 잎 경로 설치 후 천 탑승 / 수액: 잎 경로로 사격 위치 이동 후 천 밀기");
            }

            foreach (var pad in all.Select(t => t.GetComponent<ExteriorJumpPad>()).Where(p => p != null && p.gameObject.activeInHierarchy))
            {
                var so = new SerializedObject(pad);
                var sockets = so.FindProperty("_sockets");
                for (int i = 0; i < sockets.arraySize; i++)
                {
                    var target = (LeafInstallTarget)sockets.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (target == null) continue;
                    var area = New("Moss_JumpPad_" + pad.name + "_" + i, _root, target.SocketPosition);
                    var box = area.gameObject.AddComponent<BoxCollider>(); box.isTrigger = true; box.size = new Vector3(9, 10, 9); box.center = Vector3.up * 2;
                    _mossAreas.Add(box);
                    PaintMoss(bare.First(c => c.name == "seonag_tree"), box.bounds, area);
                }
                // Branch launch surfaces are safe independently of their leaf socket.
                if (!pad.name.Contains("Branch")) continue;
                foreach (var model in pad.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (model.sharedMesh == null || !model.name.ToLowerInvariant().Contains("branch")) continue;
                    var collider = model.GetComponent<MeshCollider>();
                    if (collider == null) collider = model.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh = model.sharedMesh;
                    SafeSurface(collider, "Moss_" + pad.name);
                }
                if (pad.name == "JumpPad_17_Branch")
                {
                    var target = (LeafInstallTarget)sockets.GetArrayElementAtIndex(0).objectReferenceValue;
                    Branch("Final_MossJumpPadBranch", branchTemplate, target.SocketPosition - Vector3.up * .3f, _root);
                }
            }
            // Existing permanent decks remain checkpoints; temporary leaf platforms never are.
            foreach (var c in gameplay.Find("Platforms").GetComponentsInChildren<Collider>())
                if (!c.isTrigger && c.enabled) _checkpoints.Add(c);
            var traversal = _root.gameObject.AddComponent<StageOneTraversal>();
            var config = new SerializedObject(traversal);
            SetArray(config, "_bareTrees", bare.ToArray()); SetArray(config, "_mossAreas", _mossAreas.ToArray());
            SetArray(config, "_checkpointSurfaces", _checkpoints.ToArray());
            var spawns = all.Select(t => t.GetComponent<Herbalist.Networking.PlayerSpawnLayout>()).First(x => x != null);
            var spawnConfig = new SerializedObject(spawns).FindProperty("spawnPoints");
            var starts = new Transform[spawnConfig.arraySize];
            for (int i = 0; i < starts.Length; i++) starts[i] = (Transform)spawnConfig.GetArrayElementAtIndex(i).objectReferenceValue;
            SetArray(config, "_starts", starts); config.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Replaced mushroom steps=" + removed + ", water cloths=" + cloths.Length + ", moss areas=" + _mossAreas.Count + ", bare colliders=" + bare.Count + ". Coop 4 replacement deferred.";
        }

        private static Transform New(string name, Transform parent, Vector3 position)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.position = position; return t;
        }
        public static Transform CloneFabric(Transform original, Transform parent)
        {
            // Imported model prefab children cannot be reparented. Keep their authored source
            // and place an independent mesh instance under the networked gameplay root.
            var visual = UnityEngine.Object.Instantiate(original.gameObject, parent, true).transform;
            visual.name = "WaterDrivenFabric";
            visual.GetComponent<Renderer>().enabled = true;
            original.GetComponent<Renderer>().enabled = false;
            var simulation = original.GetComponent<Cloth>();
            if (simulation != null) simulation.enabled = false;
            EnsureReadableCloth(visual);
            return visual;
        }
        public static void EnsureReadableCloth(Transform visual)
        {
            var renderer = visual.GetComponent<SkinnedMeshRenderer>();
            if (visual.GetComponent<Cloth>() == null || renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.isReadable) return;
            EnsureFolder(MeshPath);
            var mesh = UnityEngine.Object.Instantiate(renderer.sharedMesh);
            mesh.name = "MESH_" + visual.parent.name + "_WaterFabric";
            var data = new SerializedObject(mesh); data.FindProperty("m_IsReadable").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(mesh, MeshPath + "/" + mesh.name + ".asset");
            renderer.sharedMesh = mesh;
            var filter = visual.GetComponent<MeshFilter>(); if (filter != null) filter.sharedMesh = mesh;
        }
        private static Material MaterialAsset(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.name = name;
            material.color = color; material.SetFloat("_Smoothness", .1f);
            AssetDatabase.CreateAsset(material, MaterialPath + "/" + name.Replace("M_", "MAT_") + ".mat"); return material;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/'); string parent = path.Substring(0, slash);
            EnsureFolder(parent); AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
        private static void SetArray(SerializedObject so, string name, UnityEngine.Object[] values)
        {
            var p = so.FindProperty(name); p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        private static void Zone(string name, Vector3 position, Transform parent)
        {
            var t = New(name, parent, position); var target = t.gameObject.AddComponent<LeafInstallTarget>();
            var socket = New("LeafSocket", t, position);
            var so = new SerializedObject(target); so.FindProperty("targetId").intValue = ++_id;
            so.FindProperty("socket").objectReferenceValue = socket;
            so.FindProperty("_singleOccupant").boolValue = true; so.ApplyModifiedPropertiesWithoutUndo();
            var trigger = t.gameObject.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 1.4f;
            t.gameObject.AddComponent<StageOneLeafZone>();
            var ring = New("LeafInstallMarker_NoFloor", t, position);
            var line = ring.gameObject.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.loop = true;
            line.sharedMaterial = _marker; line.widthMultiplier = .1f; line.positionCount = 40;
            for (int i = 0; i < 40; i++) { float a = i * Mathf.PI * 2 / 40; line.SetPosition(i, new Vector3(Mathf.Cos(a) * 1.3f, Mathf.Sin(a) * 1.3f, 0)); }
        }
        public static void Branch(string name, MeshFilter source, Vector3 top, Transform parent)
        {
            var t = New(name, parent, top); var b = source.sharedMesh.bounds;
            t.localScale = new Vector3(6 / b.size.x, 2 / b.size.y, 6 / b.size.z);
            t.position = top - Vector3.Scale(new Vector3(b.center.x, b.max.y, b.center.z), t.localScale);
            t.gameObject.AddComponent<MeshFilter>().sharedMesh = source.sharedMesh;
            t.gameObject.AddComponent<MeshRenderer>().sharedMaterials = source.GetComponent<MeshRenderer>().sharedMaterials;
            var c = t.gameObject.AddComponent<MeshCollider>(); c.sharedMesh = source.sharedMesh;
            SafeSurface(c, name + "_Moss");
        }
        private static void SafeSurface(Collider collider, string name)
        {
            Physics.SyncTransforms(); var b = collider.bounds;
            var t = New(name, _root, b.center); var box = t.gameObject.AddComponent<BoxCollider>();
            box.size = b.size + new Vector3(.4f, 8, .4f); box.center = Vector3.up * 3; box.isTrigger = true; _mossAreas.Add(box); _checkpoints.Add(collider);
            PaintMoss(collider, b, t);
        }
        public static void PaintMoss(Collider collider, Bounds b, Transform t)
        {
            // A fitted moss mesh follows the existing collision surface; no extra solid deck.
            const int side = 13; var vertices = new Vector3[side * side]; var valid = new bool[vertices.Length];
            var uv = new Vector2[vertices.Length]; var triangles = new List<int>();
            for (int z = 0; z < side; z++) for (int x = 0; x < side; x++)
            {
                int i = z * side + x; var origin = new Vector3(Mathf.Lerp(b.min.x, b.max.x, x / 12f), b.max.y + 1, Mathf.Lerp(b.min.z, b.max.z, z / 12f));
                if (collider.Raycast(new Ray(origin, Vector3.down), out var hit, b.size.y + 2))
                { valid[i] = true; vertices[i] = hit.point + hit.normal * .035f - t.position; }
                uv[i] = new Vector2(x / 3f, z / 3f);
            }
            for (int z = 0; z < side - 1; z++) for (int x = 0; x < side - 1; x++)
            {
                int a = z * side + x, d = a + side;
                if (valid[a] && valid[d] && valid[a + 1]) triangles.AddRange(new[] { a, d, a + 1 });
                if (valid[a + 1] && valid[d] && valid[d + 1]) triangles.AddRange(new[] { a + 1, d, d + 1 });
            }
            if (triangles.Count == 0) return;
            var mesh = new Mesh { name = t.name + "_Surface", vertices = vertices, uv = uv, triangles = triangles.ToArray() }; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, MeshPath + "/" + mesh.name + "_" + _mossAreas.Count + ".asset");
            t.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh; t.gameObject.AddComponent<MeshRenderer>().sharedMaterial = _moss;
        }
        public static string CompleteMossAuthoring()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            _root = all.First(t => t.name == "GimmickRevision_20261103");
            var traversal = _root.GetComponent<StageOneTraversal>(); var so = new SerializedObject(traversal);
            EnsureFolder(MeshPath); EnsureFolder(MaterialPath);
            _moss = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath + "/MAT_StageOneMoss.mat");
            _mossAreas.Clear(); _checkpoints.Clear();
            var areas = so.FindProperty("_mossAreas");
            for (int i = 0; i < areas.arraySize; i++) _mossAreas.Add((BoxCollider)areas.GetArrayElementAtIndex(i).objectReferenceValue);
            var checkpoints = so.FindProperty("_checkpointSurfaces");
            for (int i = 0; i < checkpoints.arraySize; i++) _checkpoints.Add((Collider)checkpoints.GetArrayElementAtIndex(i).objectReferenceValue);
            var treeRoot = all.First(t => t.name == "Tree_playobject");
            var bare = treeRoot.GetComponentsInChildren<Collider>(true).Where(c => c.enabled && c.gameObject.activeInHierarchy && !c.isTrigger &&
                (c.name.ToLowerInvariant().Contains("branch") || c.name == "seonag_tree")).ToArray();
            var tree = bare.First(c => c.name == "seonag_tree"); int painted = 0;
            foreach (var area in _mossAreas.ToArray())
            {
                if (!area.name.StartsWith("Moss_JumpPad_") || area.GetComponent<MeshFilter>() != null) continue;
                PaintMoss(tree, area.bounds, area.transform);
                if (area.GetComponent<MeshFilter>() != null) painted++;
            }
            if (_root.Find("Final_MossJumpPadBranch") == null)
            {
                var pad = all.First(t => t.name == "JumpPad_17_Branch").GetComponent<ExteriorJumpPad>();
                var socket = (LeafInstallTarget)new SerializedObject(pad).FindProperty("_sockets").GetArrayElementAtIndex(0).objectReferenceValue;
                Branch("Final_MossJumpPadBranch", treeRoot.Find("branch1_low (27)").GetComponent<MeshFilter>(), socket.SocketPosition - Vector3.up * .3f, _root);
            }
            so.Update(); SetArray(so, "_bareTrees", bare); SetArray(so, "_mossAreas", _mossAreas.ToArray()); SetArray(so, "_checkpointSurfaces", _checkpoints.ToArray());
            so.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Fitted moss patches=" + painted + "; final branch safe landing authored; bare colliders=" + bare.Length;
        }
    }
}
