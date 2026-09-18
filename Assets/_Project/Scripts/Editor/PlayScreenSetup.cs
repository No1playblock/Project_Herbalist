using System.Linq;
using Herbalist.Player;
using Herbalist.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Herbalist.Editor
{
    public static class PlayScreenSetup
    {
        public static string Apply()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Run in edit mode.");

            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/PlayerControls.inputactions");
            var action = input.FindAction("Presentation/ToggleScreen");
            if (action == null)
            {
                var map = input.FindActionMap("Presentation") ?? input.AddActionMap("Presentation");
                map.AddAction("ToggleScreen", InputActionType.Button, "<Keyboard>/tab");
                System.IO.File.WriteAllText("Assets/_Project/Settings/Input/PlayerControls.inputactions", input.ToJson());
                AssetDatabase.ImportAsset("Assets/_Project/Settings/Input/PlayerControls.inputactions");
                input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/PlayerControls.inputactions");
                action = input.FindAction("Presentation/ToggleScreen", true);
            }
            var reference = AssetDatabase.LoadAssetAtPath<InputActionReference>("Assets/_Project/Settings/Input/References/Player/ToggleScreen.asset");
            if (reference == null) { reference = InputActionReference.Create(action); AssetDatabase.CreateAsset(reference, "Assets/_Project/Settings/Input/References/Player/ToggleScreen.asset"); }
            string path = "Assets/_Project/Scenes/Development/Sandbox_Abilities.unity";
            var scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            if (scene.GetRootGameObjects().Any(g => g.GetComponent<PlayScreenController>() != null))
                throw new System.InvalidOperationException("Play screen already authored; edit its serialized settings instead.");
            var root = new GameObject("PlayScreen", typeof(PlayScreenController));
            SceneManager.MoveGameObjectToScene(root, scene);
            var canvasRoot = new GameObject("PlayHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasRoot.transform.SetParent(root.transform, false);
            canvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            var common = Region("CommonHUD", canvasRoot.transform);
            var left = Region("DuyeongHUD", canvasRoot.transform);
            var right = Region("SodamHUD", canvasRoot.transform);
            var fonts = AssetDatabase.FindAssets("Roboto t:Font");
            var font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(fonts[0]));
            Label(left, "DUYEONG", font, new Color(0.65f, 0.9f, 0.69f));
            Label(right, "SODAM", font, new Color(1f, 0.73f, 0.43f));
            var divider = Region("SplitDivider", common);
            divider.anchorMin = new Vector2(0.5f, 0); divider.anchorMax = new Vector2(0.5f, 1);
            divider.sizeDelta = new Vector2(3, 0);
            var image = divider.gameObject.AddComponent<Image>(); image.color = new Color(0.04f, 0.08f, 0.06f); image.raycastTarget = false;
            common.SetAsLastSibling();
            var so = new SerializedObject(root.GetComponent<PlayScreenController>());
            so.FindProperty("toggleAction").objectReferenceValue = reference;
            so.FindProperty("divider").objectReferenceValue = divider.gameObject;
            so.FindProperty("offlinePlayer").objectReferenceValue = scene.GetRootGameObjects().Select(g => g.GetComponent<PlayerController>()).First(p => p != null);
            var regions = so.FindProperty("regions"); regions.arraySize = 2;
            for (int i = 0; i < 2; i++)
            {
                var entry = regions.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("role").enumValueIndex = i;
                entry.FindPropertyRelative("spawnSlot").intValue = i;
                entry.FindPropertyRelative("viewport").rectValue = new Rect(i * 0.5f, 0, 0.5f, 1);
                entry.FindPropertyRelative("hudRoot").objectReferenceValue = i == 0 ? left : right;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            left.anchorMax = new Vector2(0.5f, 1); right.anchorMin = new Vector2(0.5f, 0);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            if (opened) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            return "Authored PlayScreen, CommonHUD, DuyeongHUD, SodamHUD, divider and ToggleScreen input.";
        }
        private static RectTransform Region(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static void Label(RectTransform parent, string title, Font font, Color color)
        {
            var rect = Region("CharacterName", parent);
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1); rect.sizeDelta = new Vector2(-32, 40); rect.anchoredPosition = new Vector2(0, -16);
            var label = rect.gameObject.AddComponent<Text>(); label.font = font; label.text = title;
            label.fontSize = 20; label.color = color; label.alignment = TextAnchor.UpperCenter; label.raycastTarget = false;
        }
    }
}
