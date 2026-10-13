using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Levels;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class BranchRideSceneBuilder
    {
        public const string Path = "Assets/_Project/Scenes/Stages/Stage_03/Stage_03_BranchRide.unity";
        [MenuItem("Herbalist/Stages/Create Branch Ride Stage")]
        public static void Create()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            var source = EditorSceneManager.OpenScene(CurrentStagePaths.Altar);
            if (!EditorSceneManager.SaveScene(source, Path, true)) throw new Exception("Could not copy altar scene.");
            var scene = EditorSceneManager.OpenScene(Path);
            var old = GameObject.Find("StageThree_AltarLayout");
            Object.DestroyImmediate(old);
            var level = Object.FindFirstObjectByType<StageLevel>();
            level.gameObject.name = "StageThree_BranchRideRuntime";
            level.nextScenePath = string.Empty;
            var spawn = GameObject.Find("PlayerSpawnLayout");
            foreach (Transform child in spawn.transform) child.position = new Vector3(child.name.EndsWith("1") ? -2 : 2, .6f, -4);
            GameObject.Find("Player").transform.position = new Vector3(-2, .6f, -4);
            var root = new GameObject("StageThree_BranchRideLayout");
            root.SetActive(false);
            var flow = root.AddComponent<BranchRideFlow>();
            var stoneMaterial = Material("Stone", new Color(.45f, .33f, .22f));
            var branchMaterial = Material("Branch", new Color(.44f, .25f, .12f));
            var talismanMaterial = Material("Talisman", new Color(.91f, .78f, .37f));
            var targetMaterial = Material("Target", new Color(.1f, .73f, .93f));
            Vector3[] positions = { new(0, 0, 0), new(0, 0, 10), new(0, -4, 20), new(0, -4, 30), new(0, -8, 40), new(0, -8, 50) };
            var route = new Transform[positions.Length];
            for (int i = 0; i < route.Length; i++)
            {
                var point = new GameObject("Route_" + i).transform;
                point.SetParent(root.transform); point.position = positions[i]; route[i] = point;
            }
            Primitive(PrimitiveType.Cube, root.transform, "StartDeck", new Vector3(0, -.4f, -4), new Vector3(9, .7f, 9), branchMaterial);
            Primitive(PrimitiveType.Cube, root.transform, "FinishDeck", new Vector3(0, -8.4f, 51), new Vector3(9, .7f, 8), branchMaterial);
            var talisman = Primitive(PrimitiveType.Cube, root.transform, "SharedTalisman", positions[0], new Vector3(3.5f, .22f, 2.6f), talismanMaterial);
            var stopSocket = talisman.AddComponent<LeafInstallTarget>();
            Set(stopSocket, "targetId", 3901); Set(stopSocket, "_singleOccupant", true);
            var socket = new GameObject("LeafStopSocket").transform;
            socket.SetParent(talisman.transform); socket.localPosition = new Vector3(0, .35f, 0);
            Set(stopSocket, "socket", socket);
            var branches = new Transform[4];
            var targets = new SapInjectionPort[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 from = positions[i], to = positions[i + 1];
                var branch = Primitive(PrimitiveType.Cube, root.transform, "RotatingBranch_" + (i + 1),
                    Vector3.Lerp(from, to, .5f) + Vector3.down * .45f, new Vector3(2.4f, .7f, (to - from).magnitude - 1), branchMaterial);
                branch.transform.rotation = Quaternion.LookRotation(to - from);
                branches[i] = branch.transform;
                var target = Primitive(PrimitiveType.Sphere, root.transform, "T" + (i + 1), from + new Vector3(4, 1.4f, 2), Vector3.one * 1.4f, targetMaterial);
                targets[i] = target.AddComponent<SapInjectionPort>();
                TargetLabel(target.transform, "T" + (i + 1));
            }
            var stones = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                var origin = new GameObject("RollingStoneOrigin_" + i).transform;
                origin.SetParent(root.transform); origin.position = new Vector3(0, i == 0 ? -2 : -6, i == 0 ? 15 : 35);
                var stone = Primitive(PrimitiveType.Sphere, origin, "RollingStone_" + i, origin.position, Vector3.one * 2, stoneMaterial);
                stones[i] = stone.transform;
            }
            Set(flow, "_level", level); Set(flow, "_talisman", talisman.transform);
            Array(flow, "_route", route); Array(flow, "_branches", branches); Array(flow, "_targets", targets);
            Set(flow, "_stopSocket", stopSocket); Array(flow, "_stones", stones);
            var panel = GameObject.Find("EndingPanel");
            if (panel != null) Set(flow, "_endingPanel", panel);
            foreach (var label in Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (label.name == "StageName") label.text = "서낭당 · 3스테이지 가지 타기";
                else if (label.name == "Objective") label.text = "E: 두 명이 부적 탑승 · R: T1~T4 수액 조준 · 나뭇잎: 정지/회수";
                else if (label.name == "ClearText") label.text = "서낭당 클리어\n두 여행자가 부적을 타고 숲을 통과했습니다.";
            }
            root.SetActive(true);
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            var maze = EditorSceneManager.OpenScene(CurrentStagePaths.Maze);
            Object.FindFirstObjectByType<StageLevel>().nextScenePath = Path;
            EditorSceneManager.MarkSceneDirty(maze); EditorSceneManager.SaveScene(maze);
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(x => x.path == Path)) scenes.Add(new EditorBuildSettingsScene(Path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorSceneManager.OpenScene(Path);
            AssetDatabase.SaveAssets();
        }
        private static GameObject Primitive(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name;
            go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
        private static Material Material(string name, Color color)
        {
            System.IO.Directory.CreateDirectory("Assets/_Project/Materials/Stages");
            string path = "Assets/_Project/Materials/Stages/M_Stage03_" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader) { name = "M_Stage03_" + name, color = color };
            AssetDatabase.CreateAsset(material, path); return material;
        }
        private static void TargetLabel(Transform target, string text)
        {
            var canvasObject = new GameObject("TargetLabel", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(target, false);
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(0, 1.1f, 0); rect.localRotation = Quaternion.Euler(0, 180, 0);
            rect.localScale = Vector3.one * .01f; rect.sizeDelta = new Vector2(160, 70);
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            textObject.transform.SetParent(canvasObject.transform, false);
            var labelRect = textObject.GetComponent<RectTransform>(); labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var label = textObject.GetComponent<TMPro.TextMeshProUGUI>();
            label.text = text; label.fontSize = 48; label.color = Color.white; label.alignment = TMPro.TextAlignmentOptions.Center;
        }
        private static void Set(Object target, string name, Object value)
        {
            var so = new SerializedObject(target); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Set(Object target, string name, int value)
        {
            var so = new SerializedObject(target); so.FindProperty(name).intValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Set(Object target, string name, bool value)
        {
            var so = new SerializedObject(target); so.FindProperty(name).boolValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Array<T>(Object target, string name, T[] values) where T : Object
        {
            var so = new SerializedObject(target); var property = so.FindProperty(name); property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
