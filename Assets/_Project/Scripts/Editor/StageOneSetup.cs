using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using Fusion;
using Herbalist.StageOne;
using Herbalist.Abilities;
using Herbalist.Player;
using Herbalist.Networking;
using Herbalist.Presentation;
using TMPro;
namespace Herbalist.Editor
{
    // Editor-only authoring. Runtime UI is never instantiated.
    public static class StageOneSetup
    {

        public const string ScenePath = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_Exterior.unity";
        public static string Create()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop play before authoring.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the active scene before setup.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) throw new InvalidOperationException("Stage scene already exists; edit it without regenerating.");

            var leafHerb = Item("LeafHerb", "Leaf herb", StageItemType.Herb, PlayerAbilityKind.Leaf, new Color(.22f,.85f,.3f));
            var sapHerb = Item("SapHerb", "Sap herb", StageItemType.Herb, PlayerAbilityKind.Sap, new Color(1,.65f,.12f));
            var leafPotion = Item("LeafPotion", "Leaf potion", StageItemType.Potion, PlayerAbilityKind.Leaf, leafHerb.color);
            var sapPotion = Item("SapPotion", "Sap potion", StageItemType.Potion, PlayerAbilityKind.Sap, sapHerb.color);
            var settings = ScriptableObject.CreateInstance<StageOneSettings>();
            settings.items = new[] { leafHerb, sapHerb, leafPotion, sapPotion };
            settings.recipes = new[] { Recipe("LeafRecipe", leafHerb, leafPotion), Recipe("SapRecipe", sapHerb, sapPotion) };
            AssetDatabase.CreateAsset(settings, "Assets/_Project/Data/Stages/Stage_01/SO_StageOneSettings.asset");
            var controls = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = controls.AddActionMap("StageInteraction");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            map.AddAction("Craft", InputActionType.Button, "<Keyboard>/f");
            map.AddAction("Drink", InputActionType.Button, "<Keyboard>/g");
            System.IO.File.WriteAllText("Assets/_Project/Settings/Input/StageControls.inputactions", controls.ToJson());
            UnityEngine.Object.DestroyImmediate(controls); AssetDatabase.ImportAsset("Assets/_Project/Settings/Input/StageControls.inputactions");
            controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/_Project/Settings/Input/StageControls.inputactions");
            var actions = new InputActionReference[3];
            string[] actionNames = { "Interact", "Craft", "Drink" };
            for (int i=0;i<3;i++) { actions[i] = InputActionReference.Create(controls.FindAction(actionNames[i], true)); AssetDatabase.CreateAsset(actions[i], "Assets/_Project/Settings/Input/References/Stage/" + actionNames[i] + ".asset"); }
            var carryMaterial = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Abilities/MAT_Sap.mat"));
            carryMaterial.name = "StageCarry"; AssetDatabase.CreateAsset(carryMaterial, "Assets/_Project/Art/Materials/Items/MAT_StageCarry.mat");
            var networkPrefab = PrefabUtility.LoadPrefabContents("Assets/_Project/Prefabs/Characters/PF_NetworkPlayer.prefab");
            AddActor(networkPrefab, actions, carryMaterial); networkPrefab.AddComponent<NetworkStageActor>();
            new Fusion.Editor.NetworkObjectBakerEditTime().Bake(networkPrefab);
            var saved = PrefabUtility.SaveAsPrefabAsset(networkPrefab, "Assets/_Project/Prefabs/Characters/PF_StageNetworkPlayer.prefab");
            PrefabUtility.UnloadPrefabContents(networkPrefab);
            AssetDatabase.CopyAsset("Assets/_Project/Scenes/Development/Sandbox_Abilities.unity", ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var layout = UnityEngine.Object.FindAnyObjectByType<PlayerSpawnLayout>();
            var layoutSO = new SerializedObject(layout);
            layoutSO.FindProperty("playerPrefab").objectReferenceValue = saved.GetComponent<NetworkObject>();
            var offline = (GameObject)layoutSO.FindProperty("offlinePlayer").objectReferenceValue;
            layoutSO.ApplyModifiedPropertiesWithoutUndo();
            AddActor(offline, actions, carryMaterial);
            var stage = new GameObject("StageOne");
            var flow = stage.AddComponent<StageOneFlow>(); flow.settings = settings;
            stage.AddComponent<NetworkStageState>();
            var origin = offline.transform.position;
            flow.sources = new[] {
                Source(stage.transform, "LeafHerb_Source", origin + new Vector3(-2, .5f, 2), leafHerb, settings.herbGlowLayer),
                Source(stage.transform, "SapHerb_Source", origin + new Vector3(4, .5f, 2), sapHerb, settings.herbGlowLayer)
            };
            var entrance = new GameObject("TestTreeEntrance_MoveToFinalEntrance"); entrance.transform.SetParent(stage.transform); entrance.transform.position = origin + new Vector3(0,0,-8);
            var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Development/MAT_Obstacles.mat");
            Primitive("LeftPost", entrance.transform, new Vector3(-2,1.5f,0), new Vector3(.4f,3,.5f), wood, true);
            Primitive("RightPost", entrance.transform, new Vector3(2,1.5f,0), new Vector3(.4f,3,.5f), wood, true);
            Primitive("Lintel", entrance.transform, new Vector3(0,3,0), new Vector3(4.4f,.4f,.5f), wood, true);
            flow.entranceBlocker = Primitive("LockedEntrance", entrance.transform, new Vector3(0,1.4f,0), new Vector3(3.7f,2.8f,.3f), wood, true);
            var inside = new GameObject("Interior_ClearVolume"); inside.transform.SetParent(entrance.transform); inside.transform.localPosition = new Vector3(0,1.5f,-2.5f);
            flow.interior = inside.AddComponent<BoxCollider>(); flow.interior.isTrigger = true; flow.interior.size = new Vector3(4,3,4);
            Primitive("InteriorFloorMarker", entrance.transform, new Vector3(0,.015f,-2.5f), new Vector3(4,.02f,4), carryMaterial, false);
            MakeHud(stage.transform);
            foreach (var hud in UnityEngine.Object.FindObjectsByType<LeafAbilityHud>(FindObjectsInactive.Include))
            { var so = new SerializedObject(hud); so.FindProperty("lockedText").stringValue = "Drink a potion to unlock an ability"; so.ApplyModifiedPropertiesWithoutUndo(); }
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            var scenes = EditorBuildSettings.scenes.ToList(); if (!scenes.Any(s => s.path == ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath,true)); EditorBuildSettings.scenes = scenes.ToArray();
            var lobby = AssetDatabase.LoadAssetAtPath<LobbySettings>("Assets/_Project/Data/Networking/SO_LobbySettings.asset");
            lobby.playScenePath = ScenePath; EditorUtility.SetDirty(lobby);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset("Assets/_Project/Prefabs/Characters/PF_StageNetworkPlayer.prefab", ImportAssetOptions.ForceUpdate);
            Fusion.Editor.NetworkProjectConfigImporter.RebuildPrefabHash();
            AssetDatabase.ImportAsset("Assets/Photon/Fusion/Resources/NetworkProjectConfig.fusion", ImportAssetOptions.ForceUpdate);
            Fusion.NetworkProjectConfig.UnloadGlobal();
            return "Authored StageOnePrototype, stage network prefab, items/recipes/input, UGUI and test entrance. Multiplayer now enters stage; solo menu retains ability prototype.";
        }
        private static StageItemDefinition Item(string name, string label, StageItemType type, PlayerAbilityKind ability, Color color)
        {
            var item = ScriptableObject.CreateInstance<StageItemDefinition>(); item.displayName = label; item.type = type; item.ability = ability; item.color = color;
            AssetDatabase.CreateAsset(item, "Assets/_Project/Data/Items/SO_" + name + ".asset"); return item;
        }
        private static StageRecipe Recipe(string name, StageItemDefinition herb, StageItemDefinition potion)
        { var r = ScriptableObject.CreateInstance<StageRecipe>(); r.ingredient = herb; r.result = potion; AssetDatabase.CreateAsset(r, "Assets/_Project/Data/Recipes/SO_" + name + ".asset"); return r; }
        private static void AddActor(GameObject go, InputActionReference[] actions, Material material)
        {
            var actor = go.AddComponent<StageActor>(); actor.interact=actions[0]; actor.craft=actions[1]; actor.drink=actions[2];
            var body = go.GetComponentsInChildren<Transform>(true).First(t => t.name == "BodyRoot");
            var carry = new GameObject("StageHeldItem"); carry.transform.SetParent(body,false); carry.transform.localPosition = new Vector3(.48f,1,.4f);
            var view=carry.AddComponent<StageCarryPresentation>(); view.actor=actor;
            view.herb=Primitive("HeldHerb",carry.transform,Vector3.zero,new Vector3(.12f,.35f,.12f),material,false);
            view.potion=GameObject.CreatePrimitive(PrimitiveType.Sphere); view.potion.name="HeldPotion"; view.potion.transform.SetParent(carry.transform,false); view.potion.transform.localScale=Vector3.one*.25f;
            UnityEngine.Object.DestroyImmediate(view.potion.GetComponent<Collider>()); view.potion.GetComponent<Renderer>().sharedMaterial=material;
            view.herbRenderer=view.herb.GetComponent<Renderer>(); view.potionRenderer=view.potion.GetComponent<Renderer>();
            view.herb.SetActive(false); view.potion.SetActive(false);
        }
        private static GameObject Primitive(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collider)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material; if(!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        private static HerbSource Source(Transform parent,string name,Vector3 position,StageItemDefinition item,int glowLayer)
        {
            var go=new GameObject(name); go.transform.SetParent(parent);go.transform.position=position;
            var source=go.AddComponent<HerbSource>();source.item=item;source.interactionPoint=go.transform;
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=item.color; AssetDatabase.CreateAsset(mat,"Assets/_Project/Art/Materials/Items/MAT_"+name+".mat");
            source.visual=Primitive("Herb",go.transform,Vector3.zero,new Vector3(.3f,.7f,.3f),mat,false);
            var glowMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));glowMaterial.color=item.color*1.7f; AssetDatabase.CreateAsset(glowMaterial,"Assets/_Project/Art/Materials/Items/MAT_"+name+"_Glow.mat");
            source.glow=GameObject.CreatePrimitive(PrimitiveType.Sphere);source.glow.name="DuyeongOnly_Glow";source.glow.transform.SetParent(go.transform,false);source.glow.transform.localPosition=new Vector3(0,.7f,0);source.glow.transform.localScale=Vector3.one*.22f;
            source.glow.layer=glowLayer;source.glow.GetComponent<Renderer>().sharedMaterial=glowMaterial;UnityEngine.Object.DestroyImmediate(source.glow.GetComponent<Collider>());
            return source;
        }
        private static void MakeHud(Transform parent)
        {
            var go=new GameObject("StageHUD",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));go.transform.SetParent(parent,false);
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
            var scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var panel=new GameObject("ProgressPanel",typeof(RectTransform),typeof(UnityEngine.UI.Image));panel.transform.SetParent(go.transform,false);
            var rect=(RectTransform)panel.transform;rect.anchorMin=new Vector2(.08f,1);rect.anchorMax=new Vector2(.92f,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,-55);rect.sizeDelta=new Vector2(0,165);
            var image=panel.GetComponent<UnityEngine.UI.Image>();image.color=new Color(.035f,.09f,.07f,.92f);image.raycastTarget=false;
            var hud=go.AddComponent<StageHud>();
            hud.objective=Text(panel.transform,"Objective",-18,30);hud.pocket=Text(panel.transform,"Pocket",-57,23);hud.controls=Text(panel.transform,"Controls",-93,22);hud.feedback=Text(panel.transform,"Feedback",-127,20);
            hud.objective.text="Find herbs. Duyeong gathers; Sodam crafts.";hud.controls.text="E: Gather / Give    F: Craft    G: Drink";
            var clear=new GameObject("ClearPanel",typeof(RectTransform),typeof(UnityEngine.UI.Image));clear.transform.SetParent(go.transform,false);
            rect=(RectTransform)clear.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(650,120);
            clear.GetComponent<UnityEngine.UI.Image>().color=new Color(.04f,.16f,.11f,.96f);clear.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            hud.clearPanel=clear;hud.clearText=Text(clear.transform,"ClearMessage",-45,38);hud.clearText.text="STAGE 1 CLEAR";clear.SetActive(false);
        }
        private static TMP_Text Text(Transform parent,string name,float y,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);
            var rect=(RectTransform)go.transform;rect.anchorMin=new Vector2(0,1);rect.anchorMax=new Vector2(1,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,y);rect.sizeDelta=new Vector2(-36,42);
            var text=go.GetComponent<TextMeshProUGUI>();text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");text.fontSize=size;text.color=new Color(.9f,1,.9f);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            return text;
        }
    }
}
