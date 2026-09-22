using System;
using Herbalist.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Herbalist.Editor
{
    /// <summary>Editor-only authoring. No runtime object or UI creation.</summary>
    public static class PlayerPrototypeBuilder
    {

        public static string Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Build in edit mode.");
            if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PF_Player.prefab") != null)
                throw new InvalidOperationException("Prototype already exists; edit the existing assets.");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);

            var tuning = ScriptableObject.CreateInstance<PlayerTuning>();
            AssetDatabase.CreateAsset(tuning, "Assets/_Project/Data/Characters/SO_PlayerTuning.asset");
            var actions = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = actions.AddActionMap("Player");
            var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            var look = map.AddAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlLayout: "Vector2");
            var orbit = map.AddAction("Orbit", InputActionType.Button, "<Mouse>/rightButton");
            System.IO.File.WriteAllText("Assets/_Project/Settings/Input/PlayerControls.inputactions", actions.ToJson());
            UnityEngine.Object.DestroyImmediate(actions);
            AssetDatabase.ImportAsset("Assets/_Project/Settings/Input/PlayerControls.inputactions");
            actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/PlayerControls.inputactions");
            var moveRef = MakeReference(actions, "Player/Move", "Move");
            var lookRef = MakeReference(actions, "Player/Look", "Look");
            var orbitRef = MakeReference(actions, "Player/Orbit", "Orbit");

            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            var bodyMat = Material("Body", shader, new Color(0.18f, 0.45f, 0.37f));
            var headMat = Material("Head", shader, new Color(0.89f, 0.70f, 0.46f));
            var accentMat = Material("FacingMarker", shader, new Color(0.13f, 0.19f, 0.24f));
            var floorMat = Material("Ground", shader, new Color(0.33f, 0.39f, 0.43f));
            var obstacleMat = Material("Obstacles", shader, new Color(0.61f, 0.49f, 0.33f));

            var player = new GameObject("Player");
            player.SetActive(false);
            player.layer = 2; // Built-in Ignore Raycast excludes the player from the configured camera mask.
            var capsule = player.AddComponent<CharacterController>();
            capsule.height = 1.8f; capsule.radius = 0.3f; capsule.center = new Vector3(0, 0.9f, 0);
            capsule.stepOffset = 0.25f; capsule.slopeLimit = 45f; capsule.skinWidth = 0.03f;
            var body = Child("BodyRoot", player.transform, Vector3.zero);
            var torso = Primitive("Body", PrimitiveType.Capsule, body, new Vector3(0, 0.7f, 0), new Vector3(0.55f, 0.6f, 0.4f), bodyMat, false);
            Primitive("BodyForward", PrimitiveType.Cube, body, new Vector3(0, 0.85f, 0.23f), new Vector3(0.28f, 0.2f, 0.08f), accentMat, false);
            var head = Child("HeadPivot", body, new Vector3(0, 1.5f, 0));
            Primitive("Head", PrimitiveType.Sphere, head, Vector3.zero, Vector3.one * 0.5f, headMat, false);
            Primitive("Nose", PrimitiveType.Cube, head, new Vector3(0, 0, 0.27f), new Vector3(0.12f, 0.12f, 0.15f), accentMat, false);
            var target = Child("CameraTarget", player.transform, new Vector3(0, 1.3f, 0));
            var rig = Child("CameraRig", player.transform, Vector3.zero);
            var cameraObject = Child("PlayerCamera", rig, Vector3.zero).gameObject;
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f; camera.nearClipPlane = 0.1f; camera.farClipPlane = 150f;
            var listener = cameraObject.AddComponent<AudioListener>();
            var motor = player.AddComponent<CharacterControllerMotor>();
            var input = player.AddComponent<PlayerInputReader>();
            var view = player.AddComponent<PlayerView>();
            var driver = player.AddComponent<PlayerController>();
            Set(motor, "controller", capsule); Set(motor, "tuning", tuning);
            Set(input, "move", moveRef); Set(input, "look", lookRef); Set(input, "orbit", orbitRef);
            Set(view, "tuning", tuning); Set(view, "body", body); Set(view, "headPivot", head);
            Set(view, "cameraTarget", target); Set(view, "playerCamera", camera); Set(view, "audioListener", listener);
            Set(driver, "motor", motor); Set(driver, "input", input); Set(driver, "view", view); Set(driver, "tuning", tuning);
            foreach (Transform child in player.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 2;
            camera.transform.SetPositionAndRotation(target.position + Quaternion.Euler(tuning.initialPitch, 0, 0) * Vector3.back * tuning.cameraDistance, Quaternion.Euler(tuning.initialPitch, 0, 0));
            player.SetActive(true);
            PrefabUtility.SaveAsPrefabAssetAndConnect(player, "Assets/_Project/Prefabs/Characters/PF_Player.prefab", InteractionMode.AutomatedAction);

            var environment = new GameObject("MovementTestEnvironment");
            Primitive("Ground", PrimitiveType.Cube, environment.transform, new Vector3(0, -0.25f, 0), new Vector3(24, 0.5f, 24), floorMat, true);
            Primitive("Wall", PrimitiveType.Cube, environment.transform, new Vector3(0, 1, 5), new Vector3(6, 2, 0.5f), obstacleMat, true);
            for (int i = 0; i < 4; i++)
                Primitive("Step_" + i, PrimitiveType.Cube, environment.transform, new Vector3(-4, (i + 1) * 0.1f, 1 + i * 0.7f), new Vector3(2, (i + 1) * 0.2f, 0.7f), obstacleMat, true);
            var lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(50, -30, 0);
            var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.5f; light.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.48f, 0.52f);
            EditorSceneManager.SaveScene(scene, "Assets/_Project/Scenes/Development/Sandbox_Abilities.unity");
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = player;
            return "Created player prefab, input assets, tuning and additive prototype scene.";
        }

        private static InputActionReference MakeReference(InputActionAsset asset, string action, string name)
        {
            var reference = InputActionReference.Create(asset.FindAction(action, true));
            AssetDatabase.CreateAsset(reference, "Assets/_Project/Settings/Input/References/Player/" + name + ".asset");
            return reference;
        }
        private static Material Material(string name, Shader shader, Color color)
        {
            var material = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(material, "Assets/_Project/Art/Materials/Development/MAT_" + name + ".mat");
            return material;
        }
        private static Transform Child(string name, Transform parent, Vector3 position)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false); child.localPosition = position;
            return child;
        }
        private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, bool collision)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name;
            obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }
        private static void Set(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
