using System.Linq;
using Fusion;
using Herbalist.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Herbalist.Editor
{
    public static class LobbySceneBuilder
    {
        public const string Main = "Assets/_Project/Scenes/01_Title/MainMenu.unity";

        private static Font font;
        private static readonly Color Ink = new Color(0.89f, 0.93f, 0.89f);
        private static readonly Color Muted = new Color(0.56f, 0.68f, 0.64f);
        private static readonly Color Green = new Color(0.29f, 0.61f, 0.45f);

        public static string Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Main) != null) throw new System.InvalidOperationException("MainMenu already exists.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Photon/Fusion/Runtime/RuntimeAssets/Roboto-Regular.ttf");
            var settings = ScriptableObject.CreateInstance<LobbySettings>();
            settings.mainScenePath = Main;
            settings.playScenePath = "Assets/_Project/Scenes/Development/Sandbox_Abilities.unity";
            AssetDatabase.CreateAsset(settings, "Assets/_Project/Data/Networking/SO_LobbySettings.asset");
            var transport = new GameObject("FusionRunner");
            transport.AddComponent<NetworkRunner>();
            transport.AddComponent<NetworkSceneManagerDefault>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(transport, "Assets/_Project/Prefabs/Networking/PF_FusionRunner.prefab");
            Object.DestroyImmediate(transport);
            var service = new GameObject("FusionLobbySession").AddComponent<FusionLobbySession>();
            Set(service, "settings", settings); Set(service, "runnerPrefab", prefab.GetComponent<NetworkRunner>());

            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.075f, 0.065f);
            var light = new GameObject("Directional Light", typeof(Light)).GetComponent<Light>(); light.type = LightType.Directional;
            var canvas = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            var background = Box("Background", canvas.transform, Vector2.zero, new Vector2(1280, 720), new Color(0.035f, 0.075f, 0.065f));
            var bgRect = background.GetComponent<RectTransform>(); bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;
            var card = Box("LobbyCard", canvas.transform, Vector2.zero, new Vector2(640, 600), new Color(0.07f, 0.125f, 0.105f));
            Box("Accent", card.transform, new Vector2(0, 296), new Vector2(640, 5), Green);
            Label("Eyebrow", card.transform, "HERBALIST  /  CO-OP", new Vector2(0, 246), new Vector2(540, 30), 18, Green);
            Label("Title", card.transform, "A journey for two", new Vector2(0, 188), new Vector2(540, 65), 40, Ink);
            Label("Subtitle", card.transform, "Meet your partner before entering the grove.", new Vector2(0, 131), new Vector2(540, 40), 18, Muted);
            var entry = Panel("EntryPanel", card.transform);
            Label("RoomLabel", entry.transform, "ROOM NAME", new Vector2(0, 64), new Vector2(500, 25), 14, Muted);
            var field = Box("RoomNameInput", entry.transform, new Vector2(0, 10), new Vector2(500, 60), new Color(0.11f, 0.18f, 0.15f));
            var input = field.AddComponent<InputField>(); input.targetGraphic = field.GetComponent<Image>();
            var value = Label("Text", field.transform, "", Vector2.zero, new Vector2(462, 50), 24, Ink);
            var placeholder = Label("Placeholder", field.transform, "e.g. forest-01", Vector2.zero, new Vector2(462, 50), 22, Muted);
            input.textComponent = value; input.placeholder = placeholder; input.lineType = InputField.LineType.SingleLine;
            var create = Button("CreateRoom", entry.transform, "CREATE ROOM", new Vector2(-130, -85), new Vector2(240, 58), Green);
            var join = Button("JoinRoom", entry.transform, "JOIN ROOM", new Vector2(130, -85), new Vector2(240, 58), new Color(0.16f, 0.26f, 0.21f));
            Label("Hint", entry.transform, "Use the same room name on both computers.", new Vector2(0, -143), new Vector2(520, 40), 16, Muted);

            var progress = Panel("ProgressPanel", card.transform);
            var room = Label("RoomName", progress.transform, "", new Vector2(0, 35), new Vector2(530, 50), 28, Ink);
            var status = Label("Status", progress.transform, settings.connectingMessage, new Vector2(0, -36), new Vector2(530, 80), 22, Ink);
            var leave = Button("LeaveRoom", progress.transform, "LEAVE ROOM", new Vector2(0, -135), new Vector2(280, 54), new Color(0.16f, 0.26f, 0.21f));
            var error = Panel("ErrorPanel", card.transform);
            var errorText = Label("ErrorMessage", error.transform, "", new Vector2(0, -5), new Vector2(520, 150), 22, new Color(1f, 0.69f, 0.56f));
            var back = Button("Back", error.transform, "BACK", new Vector2(0, -135), new Vector2(280, 54), Green);
            Label("Footer", card.transform, "2 PLAYERS  ·  STARTS WHEN BOTH CONNECT", new Vector2(0, -252), new Vector2(550, 35), 15, Muted);
            progress.SetActive(false); error.SetActive(false);
            var screen = canvas.AddComponent<LobbyScreen>();
            Set(screen, "settings", settings); Set(screen, "roomName", input); Set(screen, "createButton", create); Set(screen, "joinButton", join);
            Set(screen, "leaveButton", leave); Set(screen, "backButton", back); Set(screen, "entryPanel", entry); Set(screen, "progressPanel", progress);
            Set(screen, "errorPanel", error); Set(screen, "progressText", status); Set(screen, "errorText", errorText); Set(screen, "roomText", room);
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var existing = EditorBuildSettings.scenes.Where(s => s.path != Main && s.path != settings.playScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Main, true), new EditorBuildSettingsScene(settings.playScenePath, true) }.Concat(existing).ToArray();
            EditorSceneManager.SaveScene(scene, Main);
            AssetDatabase.SaveAssets();
            return Main;
        }
        private static GameObject Panel(string name, Transform parent) => Rect(name, parent, Vector2.zero, new Vector2(640, 400));
        private static GameObject Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var r = obj.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f); r.sizeDelta = size; r.anchoredPosition = position; return obj;
        }
        private static GameObject Box(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        { var obj = Rect(name, parent, position, size); obj.AddComponent<Image>().color = color; return obj; }
        private static Text Label(string name, Transform parent, string text, Vector2 position, Vector2 size, int sizeFont, Color color)
        {
            var label = Rect(name, parent, position, size).AddComponent<Text>(); label.font = font; label.text = text;
            label.fontSize = sizeFont; label.color = color; label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; return label;
        }
        private static Button Button(string name, Transform parent, string text, Vector2 position, Vector2 size, Color color)
        { var obj = Box(name, parent, position, size, color); var b = obj.AddComponent<Button>(); b.targetGraphic = obj.GetComponent<Image>(); Label("Label", obj.transform, text, Vector2.zero, size, 18, Ink); return b; }
        private static void Set(Object target, string name, Object value)
        { var so = new SerializedObject(target); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
