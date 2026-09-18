using System.Linq;
using Fusion;
using Herbalist.Abilities;
using Herbalist.Networking;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Herbalist.Editor
{
    public static class SapAbilitySetup
    {

        public static string Apply()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Edit mode only");
            var settings = AssetDatabase.LoadAssetAtPath<SapAbilitySettings>("Assets/_Project/Data/Abilities/Sap/SO_SapAbilitySettings.asset");
            if (settings == null) { settings = ScriptableObject.CreateInstance<SapAbilitySettings>(); AssetDatabase.CreateAsset(settings, "Assets/_Project/Data/Abilities/Sap/SO_SapAbilitySettings.asset"); }
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Abilities/MAT_Sap.mat");
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_Body.mat"));
                material.color = new Color(1f, 0.62f, 0.06f); if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.85f);
                AssetDatabase.CreateAsset(material, "Assets/_Project/Art/Materials/Abilities/MAT_Sap.mat");
            }
            var offline = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_SapDeposit.prefab");
            var network = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_NetworkSapDeposit.prefab");
            if (offline == null || network == null)
            {
                var root = new GameObject("SapDeposit", typeof(SapDeposit));
                var blob = GameObject.CreatePrimitive(PrimitiveType.Sphere); blob.name = "LiquidVisual"; blob.transform.SetParent(root.transform, false);
                Object.DestroyImmediate(blob.GetComponent<Collider>()); blob.GetComponent<Renderer>().sharedMaterial = material;
                blob.transform.localScale = settings.controlledScale;
                Set(root.GetComponent<SapDeposit>(), "visual", blob.transform); Set(root.GetComponent<SapDeposit>(), "settings", settings);
                offline = PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_SapDeposit.prefab");
                root.AddComponent<NetworkObject>(); root.AddComponent<NetworkTransform>(); root.AddComponent<NetworkSapDeposit>();
                network = PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_NetworkSapDeposit.prefab");
                Object.DestroyImmediate(root);
            }
            settings.offlinePrefab = offline.GetComponent<SapDeposit>(); EditorUtility.SetDirty(settings);
            foreach (var name in new[] { "Player.prefab", "NetworkPlayer.prefab" })
            {
                var root = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Characters/PF_" + name);
                try
                {
                    var sap = root.GetComponent<SapControlAbility>() ?? root.AddComponent<SapControlAbility>(); Set(sap, "settings", settings);
                    var manager = new SerializedObject(root.GetComponent<PlayerAbilityController>());
                    manager.FindProperty("prototypeOfflineAbility").enumValueIndex = (int)PlayerAbilityKind.Sap;
                    manager.ApplyModifiedPropertiesWithoutUndo();
                    if (name == "NetworkPlayer.prefab")
                    {
                        var adapter = root.GetComponent<NetworkLeafAbility>(); Set(adapter, "sapPrefab", network.GetComponent<NetworkObject>());
                        var so = new SerializedObject(adapter);
                        var leafSlots = so.FindProperty("prototypeUnlockSlots"); leafSlots.arraySize = 1; leafSlots.GetArrayElementAtIndex(0).intValue = 1;
                        var sapSlots = so.FindProperty("prototypeSapSlots"); sapSlots.arraySize = 1; sapSlots.GetArrayElementAtIndex(0).intValue = 0;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/Characters/PF_" + name);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            var scene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Development/Sandbox_Abilities.unity");
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Development/Sandbox_Abilities.unity", OpenSceneMode.Additive);
            foreach (var target in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LeafInstallTarget>(true)))
            {
                var receiver = target.GetComponent<SapReceiver>() ?? target.gameObject.AddComponent<SapReceiver>();
                Set(receiver, "leafTarget", target); var so = new SerializedObject(receiver); so.FindProperty("receiverId").intValue = target.Id; so.ApplyModifiedPropertiesWithoutUndo();
            }
            if (!scene.GetRootGameObjects().Any(g => g.name == "SapAbilityTestObjects"))
            {
                var group = new GameObject("SapAbilityTestObjects"); SceneManager.MoveGameObjectToScene(group, scene);
                var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder); tree.name = "SapExtractionTree"; tree.transform.SetParent(group.transform);
                tree.transform.position = new Vector3(-2, 1, 1.5f); tree.transform.localScale = new Vector3(0.8f, 1, 0.8f);
                tree.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_Obstacles.mat");
                var nozzle = GameObject.CreatePrimitive(PrimitiveType.Sphere); nozzle.name = "ExtractionPoint"; nozzle.transform.SetParent(group.transform);
                nozzle.transform.position = new Vector3(-1.3f, 1.3f, 0.8f); nozzle.transform.localScale = Vector3.one * 0.3f;
                Object.DestroyImmediate(nozzle.GetComponent<Collider>()); nozzle.GetComponent<Renderer>().sharedMaterial = material;
                var source = tree.AddComponent<SapSource>(); Set(source, "extractionPoint", nozzle.transform); Set(source, "interactionVolume", tree.GetComponent<Collider>());
                var device = GameObject.CreatePrimitive(PrimitiveType.Cube); device.name = "SapInletTest"; device.transform.SetParent(group.transform);
                device.transform.position = new Vector3(-3, 1, 2); device.transform.localScale = new Vector3(1, 2, 0.5f);
                var receiver = device.AddComponent<SapReceiver>(); var so = new SerializedObject(receiver); so.FindProperty("receiverId").intValue = 1000; so.ApplyModifiedPropertiesWithoutUndo();
                var inlet = new GameObject("Inlet"); inlet.transform.SetParent(group.transform); inlet.transform.position = new Vector3(-3, 1.3f, 1.725f); inlet.transform.forward = Vector3.back;
                Set(receiver, "inlet", inlet.transform);
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere); marker.name = "SuppliedIndicator"; marker.transform.SetParent(group.transform);
                marker.transform.position = new Vector3(-3, 2.5f, 2); marker.transform.localScale = Vector3.one * 0.4f;
                Object.DestroyImmediate(marker.GetComponent<Collider>()); marker.GetComponent<Renderer>().sharedMaterial = material; marker.SetActive(false);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                UnityEventTools.AddBoolPersistentListener((UnityEngine.Events.UnityEvent)typeof(SapReceiver).GetField("onSupplied", flags).GetValue(receiver), marker.SetActive, true);
                UnityEventTools.AddBoolPersistentListener((UnityEngine.Events.UnityEvent)typeof(SapReceiver).GetField("onDrained", flags).GetValue(receiver), marker.SetActive, false);
                EditorUtility.SetDirty(receiver);
                var offlinePlayer = scene.GetRootGameObjects().FirstOrDefault(g => g.GetComponent<Herbalist.Player.PlayerController>() != null);
                if (offlinePlayer != null) group.transform.position = new Vector3(offlinePlayer.transform.position.x, 0, offlinePlayer.transform.position.z);
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            if (opened) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.SaveAssets(); Fusion.Editor.NetworkProjectConfigUtilities.RebuildPrefabTable();
            return "Sap prefabs/settings authored. Offline and slot 0: Sap; slot 1: Leaf. Source, free surfaces and inlet test connected.";
        }
        private static void Set(Object target, string name, Object value)
        { var so = new SerializedObject(target); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
