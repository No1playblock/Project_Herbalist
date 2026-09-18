using System.Linq;
using Fusion;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.Presentation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Herbalist.Editor
{
    public static class LeafAbilitySetup
    {

        public static string Apply()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Edit mode only");
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/PlayerControls.inputactions");
            var map = input.FindActionMap("Ability") ?? input.AddActionMap("Ability");
            if (map.FindAction("Cycle") == null) map.AddAction("Cycle", InputActionType.Button, "<Keyboard>/r");
            if (map.FindAction("Use") == null) map.AddAction("Use", InputActionType.Button, "<Mouse>/leftButton");
            System.IO.File.WriteAllText("Assets/_Project/Settings/Input/PlayerControls.inputactions", input.ToJson());
            AssetDatabase.ImportAsset("Assets/_Project/Settings/Input/PlayerControls.inputactions");
            input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/PlayerControls.inputactions");
            var cycle = Reference(input.FindAction("Ability/Cycle"), "AbilityCycle.asset");
            var use = Reference(input.FindAction("Ability/Use"), "AbilityUse.asset");
            var settings = AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>("Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset");
            if (settings == null) { settings = ScriptableObject.CreateInstance<LeafAbilitySettings>(); AssetDatabase.CreateAsset(settings, "Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset"); }
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Abilities/MAT_Leaf.mat");
            if (mat == null) { mat = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_Body.mat")); mat.color = new Color(0.3f,0.7f,0.15f); AssetDatabase.CreateAsset(mat, "Assets/_Project/Art/Materials/Abilities/MAT_Leaf.mat"); }
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("layers"); int leafLayer = LayerMask.NameToLayer("Leaf");
            if (leafLayer < 0)
            {
                for (int i=8;i<layers.arraySize;i++) if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) { leafLayer=i; break; }
                if (leafLayer < 0) throw new System.InvalidOperationException("No free Leaf layer");
                layers.GetArrayElementAtIndex(leafLayer).stringValue="Leaf";tags.ApplyModifiedPropertiesWithoutUndo();
            }
            var leaf = new GameObject("LeafProjectile", typeof(LeafProjectile));
            var platform = Shape("Platform", leaf.transform, new Vector3(2.5f,0.16f,1.3f), mat);
            var pin = Shape("Pin", leaf.transform, new Vector3(1.3f,0.16f,2.5f), mat);
            var collider = platform.AddComponent<BoxCollider>(); collider.enabled = false;
            var pinCollider = pin.AddComponent<BoxCollider>(); pinCollider.enabled = false;
            Set(leaf.GetComponent<LeafProjectile>(), "platformVisual", platform);
            Set(leaf.GetComponent<LeafProjectile>(), "pinVisual", pin);
            Set(leaf.GetComponent<LeafProjectile>(), "platformCollider", collider);
            Set(leaf.GetComponent<LeafProjectile>(), "pinCollider", pinCollider);
            platform.SetActive(false); pin.SetActive(false);
            foreach(Transform child in leaf.GetComponentsInChildren<Transform>(true)) child.gameObject.layer=leafLayer;
            var offlinePrefab = PrefabUtility.SaveAsPrefabAsset(leaf, "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_LeafProjectile.prefab");
            settings.offlinePrefab = offlinePrefab.GetComponent<LeafProjectile>(); EditorUtility.SetDirty(settings);
            leaf.AddComponent<NetworkObject>(); leaf.AddComponent<NetworkTransform>(); leaf.AddComponent<NetworkLeafProjectile>();
            var netPrefab = PrefabUtility.SaveAsPrefabAsset(leaf, "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_NetworkLeafProjectile.prefab");
            Object.DestroyImmediate(leaf);
            foreach (string name in new[] { "Player.prefab", "NetworkPlayer.prefab" })
            {
                var root = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Characters/PF_" + name);
                var reader = root.GetComponent<AbilityInputReader>() ?? root.AddComponent<AbilityInputReader>();
                Set(reader,"cycle",cycle); Set(reader,"use",use);
                var ability = root.GetComponent<LeafThrowAbility>() ?? root.AddComponent<LeafThrowAbility>(); Set(ability,"settings",settings); Set(ability,"launchFrame",root.transform.Find("BodyRoot"));
                if (root.GetComponent<PlayerAbilityController>() == null) root.AddComponent<PlayerAbilityController>();
                if (name == "NetworkPlayer.prefab")
                {
                    var adapter = root.GetComponent<NetworkLeafAbility>() ?? root.AddComponent<NetworkLeafAbility>(); Set(adapter,"leafPrefab",netPrefab.GetComponent<NetworkObject>());
                }
                PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/Characters/PF_" + name); PrefabUtility.UnloadPrefabContents(root);
            }
            var scene = SceneManager.GetSceneByPath("Assets/_Project/Scenes/Development/Sandbox_Abilities.unity");
            bool opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Development/Sandbox_Abilities.unity",OpenSceneMode.Additive);
            var screen = scene.GetRootGameObjects().Select(g=>g.GetComponent<PlayScreenController>()).First(c=>c!=null);
            if (!scene.GetRootGameObjects().Any(g=>g.name=="LeafAbilityTestTargets"))
            {
                var group = new GameObject("LeafAbilityTestTargets"); SceneManager.MoveGameObjectToScene(group,scene);
                for(int i=0;i<2;i++)
                {
                    var board = GameObject.CreatePrimitive(PrimitiveType.Cube); board.name = i==0?"PlatformTarget":"PinTarget";
                    board.transform.SetParent(group.transform); board.transform.position = new Vector3(i==0?6:-6,3,6); board.transform.localScale=new Vector3(3,6,0.5f);
                    board.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_Obstacles.mat");
                    var target=board.AddComponent<LeafInstallTarget>(); var so=new SerializedObject(target);
                    so.FindProperty("targetId").intValue=100+i;
                    so.FindProperty("acceptsPlatform").boolValue=i==0; so.FindProperty("acceptsPin").boolValue=i==1; so.ApplyModifiedPropertiesWithoutUndo();
                    var sap=board.AddComponent<SapBindingSource>(); Set(target,"sap",sap);
                    var marker=GameObject.CreatePrimitive(PrimitiveType.Sphere); marker.name="PinnedIndicator"; marker.transform.SetParent(group.transform);
                    marker.transform.position=board.transform.position+Vector3.up*3.5f; marker.transform.localScale=Vector3.one*0.5f;
                    Object.DestroyImmediate(marker.GetComponent<Collider>()); marker.GetComponent<Renderer>().sharedMaterial=mat; marker.SetActive(false);
                    var eventSO=new SerializedObject(target); // Persistent events stay in the authored scene.
                    var pinned=(UnityEngine.Events.UnityEvent)typeof(LeafInstallTarget).GetField("onPinned",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(target);
                    var released=(UnityEngine.Events.UnityEvent)typeof(LeafInstallTarget).GetField("onReleased",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(target);
                    UnityEventTools.AddBoolPersistentListener(pinned,marker.SetActive,true); UnityEventTools.AddBoolPersistentListener(released,marker.SetActive,false); EditorUtility.SetDirty(target);
                }
            }
            var font=AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("Roboto t:Font")[0]));
            for(int i=0;i<2;i++)
            {
                var parent=screen.transform.Find("PlayHUD/"+(i==0?"DuyeongHUD":"SodamHUD"));
                if(parent.Find("LeafHUD")!=null) continue;
                var root=new GameObject("LeafHUD",typeof(RectTransform),typeof(LeafAbilityHud)); root.transform.SetParent(parent,false);
                var rect=root.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                var status=Text("Status",root.transform,font,"LEAF OFF",new Vector2(0.5f,0),new Vector2(0,30),new Vector2(560,32),18);
                var reticle=Text("Reticle",root.transform,font,"+",new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(48,48),24);
                var hud=root.GetComponent<LeafAbilityHud>();Set(hud,"screen",screen);Set(hud,"status",status);Set(hud,"reticle",reticle.gameObject);
                var so=new SerializedObject(hud);so.FindProperty("slot").intValue=i;so.ApplyModifiedPropertiesWithoutUndo();reticle.gameObject.SetActive(false);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            if(opened) EditorSceneManager.CloseScene(scene,true);
            AssetDatabase.SaveAssets();Fusion.Editor.NetworkProjectConfigUtilities.RebuildPrefabTable();
            return "Leaf prefabs, settings, R/LMB actions, UGUI and two test targets authored.";
        }
        private static GameObject Shape(string name,Transform parent,Vector3 scale,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(parent,false);go.transform.localScale=scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=mat;return go;
        }
        private static Text Text(string name,Transform parent,Font font,string value,Vector2 anchor,Vector2 position,Vector2 size,int fontSize)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;r.sizeDelta=size;
            var text=go.GetComponent<Text>();text.font=font;text.fontSize=fontSize;text.text=value;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;return text;
        }
        private static InputActionReference Reference(InputAction action,string name)
        {
            var reference=AssetDatabase.LoadAssetAtPath<InputActionReference>("Assets/_Project/Settings/Input/References/Player/" + name);
            if(reference==null){reference=InputActionReference.Create(action);AssetDatabase.CreateAsset(reference,"Assets/_Project/Settings/Input/References/Player/" + name);}return reference;
        }
        private static void Set(Object target,string name,Object value)
        {var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
