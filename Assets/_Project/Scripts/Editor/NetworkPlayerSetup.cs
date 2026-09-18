using Fusion;
using Herbalist.Networking;
using Herbalist.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Herbalist.Editor
{
    public static class NetworkPlayerSetup
    {
        public static string Apply()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Run in edit mode.");

            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/PlayerControls.inputactions");
            var jump = input.FindAction("Player/Jump");
            if (jump == null)
            {
                jump = input.FindActionMap("Player", true).AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
                System.IO.File.WriteAllText("Assets/_Project/Settings/Input/PlayerControls.inputactions", input.ToJson());
                AssetDatabase.ImportAsset("Assets/_Project/Settings/Input/PlayerControls.inputactions");
                input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/PlayerControls.inputactions");
                jump = input.FindAction("Player/Jump", true);
            }
            var jumpRef = AssetDatabase.LoadAssetAtPath<InputActionReference>("Assets/_Project/Settings/Input/References/Player/Jump.asset");
            if (jumpRef == null) { jumpRef = InputActionReference.Create(jump); AssetDatabase.CreateAsset(jumpRef, "Assets/_Project/Settings/Input/References/Player/Jump.asset"); }
            var offline = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Characters/PF_Player.prefab");
            Set(offline.GetComponent<PlayerInputReader>(), "jump", jumpRef);
            PrefabUtility.SaveAsPrefabAsset(offline, "Assets/_Project/Prefabs/Characters/PF_Player.prefab");
            PrefabUtility.UnloadPrefabContents(offline);
            var root = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Characters/PF_Player.prefab");
            var obj = root.AddComponent<NetworkObject>();
            root.AddComponent<NetworkTransform>();
            var net = root.AddComponent<NetworkPlayer>();
            Set(net, "player", root.GetComponent<PlayerController>());
            Set(net, "bodyRenderer", root.transform.Find("BodyRoot/Body").GetComponent<Renderer>());
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_PartnerBody.mat");
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_Body.mat"));
                material.color = new Color(0.68f, 0.36f, 0.16f);
                AssetDatabase.CreateAsset(material, "Assets/_Project/Art/Materials/Development/MAT_PartnerBody.mat");
            }
            var serialized = new SerializedObject(net);
            var mats = serialized.FindProperty("slotMaterials"); mats.arraySize = 2;
            mats.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_Body.mat");
            mats.GetArrayElementAtIndex(1).objectReferenceValue = material;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var pc = new SerializedObject(root.GetComponent<PlayerController>()); pc.FindProperty("locallyControlled").boolValue = false; pc.ApplyModifiedPropertiesWithoutUndo();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("layers"); int layer = LayerMask.NameToLayer("Player");
            if (layer < 0)
            {
                for (int i = 8; i < layers.arraySize; i++) if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) { layer = i; break; }
                if (layer < 0) throw new System.InvalidOperationException("No free Player layer.");
                layers.GetArrayElementAtIndex(layer).stringValue = "Player"; tags.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = layer;
            root.GetComponent<CharacterController>().excludeLayers = 1 << layer;
            foreach (var c in root.GetComponentsInChildren<Camera>(true)) c.enabled = false;
            foreach (var a in root.GetComponentsInChildren<AudioListener>(true)) a.enabled = false;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/Characters/PF_NetworkPlayer.prefab");
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.ImportAsset("Assets/_Project/Prefabs/Characters/PF_NetworkPlayer.prefab");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Development/Sandbox_Abilities.unity", OpenSceneMode.Additive);
            var layout = Object.FindFirstObjectByType<PlayerSpawnLayout>();
            if (layout == null)
            {
                var go = new GameObject("PlayerSpawnLayout"); SceneManager.MoveGameObjectToScene(go, scene); layout = go.AddComponent<PlayerSpawnLayout>();
                var points = new Transform[2];
                for (int i=0;i<points.Length;i++) { points[i] = new GameObject("PlayerSpawn_" + (i+1)).transform; points[i].SetParent(go.transform); points[i].position = new Vector3(i == 0 ? -1.5f : 1.5f, 0.1f, 0); }
                var so = new SerializedObject(layout); var array = so.FindProperty("spawnPoints"); array.arraySize = points.Length;
                for(int i=0;i<points.Length;i++) array.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            Set(layout, "playerPrefab", prefab.GetComponent<NetworkObject>());
            foreach (var go in scene.GetRootGameObjects()) if (go.GetComponent<PlayerController>() != null) Set(layout, "offlinePlayer", go);
            var tuning = AssetDatabase.LoadAssetAtPath<PlayerTuning>("Assets/_Project/Data/Characters/SO_PlayerTuning.asset"); EditorUtility.SetDirty(tuning);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets();
            Fusion.Editor.NetworkProjectConfigUtilities.RebuildPrefabTable();
            return "Jump binding, network prefab and two spawn points configured.";
        }
        private static void Set(Object target, string name, Object value)
        { var so = new SerializedObject(target); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
