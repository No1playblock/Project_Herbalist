using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Herbalist.GameUI;
using Herbalist.Networking;
using Herbalist.Presentation;
using Herbalist.StageOne;
using Object=UnityEngine.Object;

public static class GameUiBuilder
{
    private const string Root="Assets/GameUI";
    private static TMP_FontAsset font;
    private static GameUiSettings settings;
    private static Color Ink=new Color(.10f,.19f,.15f), Paper=new Color(.96f,.94f,.87f), Gold=new Color(.65f,.49f,.24f), Muted=new Color(.37f,.43f,.38f);
    private static GameObject optionsPrefab, loadingPrefab, menuPrefab, overlayPrefab, hudPrefab;
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play mode first.");
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save current scene edits before UI authoring.");
        if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/MainMenuUI.prefab")!=null)throw new Exception("UI already authored; edit prefabs instead of rebuilding.");
        EnsureFont(); EnsureSettings();
        optionsPrefab=BuildOptions(); loadingPrefab=BuildLoading(); menuPrefab=BuildMenu(); overlayPrefab=BuildOverlay(); hudPrefab=BuildHud();
        ConfigureMain(); ConfigurePlay("Assets/PlayerPrototype/PlayerMovementPrototype.unity","능력 연습장","능력을 바꾸며 자유롭게 시험해 보세요");
        ConfigurePlay("Assets/StageOne/StageOnePrototype.unity","서낭당 · 숲의 입구","약초를 모아 두 약제를 완성하세요");
        ConfigurePlay("Assets/StageTwo/StageTwoPrototype.unity","서낭당 · 나무의 내부","함께 발판을 만들어 위쪽 가지로 올라가세요");
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        AssetDatabase.SaveAssets();
        return "Authored 5 shared UGUI prefabs and integrated MainMenu + 3 play scenes.";
    }
    private static void EnsureFont()
    {
        var path=Root+"/Fonts/NanumGothic SDF.asset";
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if(font!=null)return;
        font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(Root+"/Fonts/NanumGothic-Regular.ttf"));
        font.name="NanumGothic SDF"; font.atlasPopulationMode=AtlasPopulationMode.Dynamic; font.isMultiAtlasTexturesEnabled=true;
        AssetDatabase.CreateAsset(font,path);
        foreach(var texture in font.atlasTextures)if(texture!=null)AssetDatabase.AddObjectToAsset(texture,font);
        AssetDatabase.AddObjectToAsset(font.material,font);
        font.TryAddCharacters("초우록두영소담게임방만들기참가선택준비시작옵션종료음량마우스감도이어하기일시정지상대플레이어약초채집제조복용전달나뭇잎수액숲입구내부키확인빈손설정돌아가기테스트연결중위쪽가지",out string missing);
    }
    private static void EnsureSettings()
    {
        settings=ScriptableObject.CreateInstance<GameUiSettings>(); settings.menuScene="Assets/Scenes/MainMenu.unity";
        AssetDatabase.CreateAsset(settings,Root+"/GameUiSettings.asset");
        var asset=ScriptableObject.CreateInstance<InputActionAsset>(); asset.name="GameUIControls";
        var map=new InputActionMap("GameUI"); var pause=map.AddAction("Pause",InputActionType.Button,"<Keyboard>/escape");
        var help=map.AddAction("Help",InputActionType.Button,"<Keyboard>/h"); asset.AddActionMap(map);
        System.IO.File.WriteAllText(Root+"/GameUIControls.inputactions",asset.ToJson());
        Object.DestroyImmediate(asset);
        AssetDatabase.ImportAsset(Root+"/GameUIControls.inputactions",ImportAssetOptions.ForceSynchronousImport);
        var imported=AssetDatabase.LoadAssetAtPath<InputActionAsset>(Root+"/GameUIControls.inputactions");
        settings.pauseAction=Ref(imported.FindAction("Pause"),"Pause"); settings.helpAction=Ref(imported.FindAction("Help"),"Help");
        var player=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/PlayerPrototype/PlayerControls.inputactions");
        var stage=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/StageOne/StageControls.inputactions");
        settings.controls=new[]{
            Hint("일시정지",settings.pauseAction),Hint("키 안내",settings.helpAction),
            Hint("화면 전환",Ref(player.FindAction("ToggleScreen"),"ToggleScreen")),
            Hint("이동",Ref(player.FindAction("Move"),"Move")),Hint("점프",Ref(player.FindAction("Jump"),"Jump")),
            Hint("능력 전환",Ref(player.FindAction("Cycle"),"Cycle")),Hint("능력 사용",Ref(player.FindAction("Use"),"Use")),
            Hint("채집 / 전달",Ref(stage.FindAction("Interact"),"Interact")),Hint("약제 제조",Ref(stage.FindAction("Craft"),"Craft")),
            Hint("약제 복용",Ref(stage.FindAction("Drink"),"Drink"))};
        EditorUtility.SetDirty(settings);
    }
    private static ControlHint Hint(string label,InputActionReference action)=>new ControlHint{label=label,action=action};
    private static InputActionReference Ref(InputAction action,string name)
    {
        if(action==null)throw new Exception("Missing action: "+name);
        var r=InputActionReference.Create(action); AssetDatabase.CreateAsset(r,Root+"/"+name+".asset");return r;
    }
    private static GameObject Rect(string name,Transform parent,Vector2 pos,Vector2 size)
    {
        var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);
        var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=pos;r.sizeDelta=size;return g;
    }
    private static void Stretch(GameObject g) { var r=(RectTransform)g.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero; }
    private static GameObject Panel(string name,Transform parent,Color color)
    { var g=Rect(name,parent,Vector2.zero,Vector2.zero);Stretch(g);var image=g.AddComponent<Image>();image.color=color;image.raycastTarget=color.a>.1f;return g; }
    private static GameObject Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
    { var g=Rect(name,parent,pos,size);var im=g.AddComponent<Image>();im.color=color;im.raycastTarget=false;return g; }
    private static TMP_Text Label(string name,Transform p,string text,Vector2 pos,Vector2 size,int fontSize,Color color)
    {
        var g=Rect(name,p,pos,size);var t=g.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=fontSize;t.color=color;
        t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;
    }
    private static Button Button(string name,Transform p,string text,Vector2 pos,Vector2 size,bool primary=true)
    {
        var g=Box(name,p,pos,size,primary?Ink:Paper);g.GetComponent<Image>().raycastTarget=true;
        var b=g.AddComponent<Button>();b.targetGraphic=g.GetComponent<Image>();
        var c=b.colors;c.highlightedColor=new Color(.85f,.91f,.85f);c.pressedColor=new Color(.68f,.78f,.69f);c.disabledColor=new Color(.55f,.55f,.55f,.5f);b.colors=c;
        Label("Label",g.transform,text,Vector2.zero,size-new Vector2(14,6),19,primary?Paper:Ink);
        return b;
    }
    private static GameObject CanvasRoot(string name,int order)
    {
        var g=Rect(name,null,Vector2.zero,new Vector2(1280,720));var c=g.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=order;
        var sc=g.AddComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1280,720);sc.matchWidthOrHeight=.5f;
        g.AddComponent<GraphicRaycaster>();return g;
    }
    private static GameObject Save(GameObject g,string name)
    { var prefab=PrefabUtility.SaveAsPrefabAsset(g,Root+"/"+name+".prefab");Object.DestroyImmediate(g);return prefab; }
    private static GameObject Instance(GameObject prefab,Transform parent)
    { return (GameObject)PrefabUtility.InstantiatePrefab(prefab,parent); }
    private static void Background(Transform parent,string texture)
    {
        var g=Rect("Backdrop",parent,Vector2.zero,Vector2.zero);Stretch(g);
        var raw=g.AddComponent<RawImage>();raw.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/"+texture+".png");raw.raycastTarget=false;
    }
    private static Slider Slider(Transform p,string name,float y,float min,float max)
    {
        var g=Rect(name,p,new Vector2(50,y),new Vector2(330,30));var s=g.AddComponent<Slider>();s.minValue=min;s.maxValue=max;
        Box("Track",g.transform,Vector2.zero,new Vector2(330,4),new Color(.6f,.67f,.6f));
        var area=Rect("HandleArea",g.transform,Vector2.zero,new Vector2(312,30));
        var handle=Box("Handle",area.transform,Vector2.zero,new Vector2(18,28),Gold);
        s.handleRect=(RectTransform)handle.transform;s.targetGraphic=handle.GetComponent<Image>();s.direction=UnityEngine.UI.Slider.Direction.LeftToRight;handle.GetComponent<Image>().raycastTarget=true;
        return s;
    }
    private static GameObject BuildOptions()
    {
        var root=Panel("OptionsPanel",null,new Color(.03f,.07f,.05f,.85f));var c=root.AddComponent<OptionsPanel>();
        var card=Box("Card",root.transform,Vector2.zero,new Vector2(640,380),Paper);
        Label("Title",card.transform,"옵션",new Vector2(0,130),new Vector2(500,60),32,Ink);
        Label("VolumeLabel",card.transform,"전체 음량",new Vector2(-205,40),new Vector2(140,40),18,Ink);
        Label("LookLabel",card.transform,"마우스 감도",new Vector2(-205,-40),new Vector2(140,40),18,Ink);
        c.volume=Slider(card.transform,"Volume",40,0,1);c.sensitivity=Slider(card.transform,"Sensitivity",-40,.25f,2);
        c.volumeValue=Label("VolumeValue",card.transform,"100%",new Vector2(245,40),new Vector2(100,40),16,Muted);
        c.sensitivityValue=Label("LookValue",card.transform,"1.00x",new Vector2(245,-40),new Vector2(100,40),16,Muted);
        c.close=Button("Close",card.transform,"닫기",new Vector2(0,-130),new Vector2(220,46));return Save(root,"OptionsPanel");
    }
    private static GameObject BuildLoading()
    {
        var root=Panel("LoadingPanel",null,Ink);Background(root.transform,"LoadingBackground");
        Panel("Shade",root.transform,new Color(.02f,.04f,.02f,.2f));
        Box("ProgressCard",root.transform,new Vector2(410,-265),new Vector2(380,100),new Color(.05f,.12f,.08f,.9f));
        Label("LoadingLabel",root.transform,settings.loadingText,new Vector2(400,-260),new Vector2(340,50),23,Paper);
        Label("LoadingSub",root.transform,"잠시만 기다려 주세요",new Vector2(400,-291),new Vector2(340,25),14,new Color(.8f,.8f,.7f));
        return Save(root,"LoadingPanel");
    }
    private static GameObject BuildMenu()
    {
        var root=CanvasRoot("MainMenuUI",10);var ui=root.AddComponent<MainMenuUI>();ui.settings=settings;
        ui.home=Panel("Home",root.transform,Paper);Background(ui.home.transform,"HomeBackground");
        var side=Box("MenuPaper",ui.home.transform,new Vector2(-405,0),new Vector2(470,720),new Color(.96f,.95f,.90f,.97f));
        Label("Eyebrow",side.transform,"두 사람이 엮어 가는 숲의 이야기",new Vector2(0,270),new Vector2(420,40),17,Muted);
        Label("Title",side.transform,"초우록",new Vector2(0,190),new Vector2(410,100),70,Ink);
        Label("Subtitle",side.transform,"HERBALIST  ·  CO-OP ADVENTURE",new Vector2(0,125),new Vector2(400,35),13,Gold);
        ui.create=Button("Create",side.transform,"방 만들기",new Vector2(0,20),new Vector2(300,52));
        ui.join=Button("Join",side.transform,"방 참가하기",new Vector2(0,-48),new Vector2(300,52));
        ui.openOptions=Button("Options",side.transform,"옵션",new Vector2(0,-116),new Vector2(300,48),false);
        ui.quit=Button("Quit",side.transform,"종료",new Vector2(0,-177),new Vector2(300,48),false);
        ui.solo=Button("SoloTest",ui.home.transform,"솔로 테스트",new Vector2(510,-315),new Vector2(190,38),false);
        Label("Version",side.transform,"PROTOTYPE  /  2 PLAYERS",new Vector2(0,-320),new Vector2(400,30),12,Muted);

        ui.entry=Panel("RoomEntry",root.transform,Paper);
        Box("EntryAccent",ui.entry.transform,new Vector2(0,220),new Vector2(70,4),Gold);
        ui.entryTitle=Label("Title",ui.entry.transform,"방 만들기",new Vector2(0,150),new Vector2(600,80),42,Ink);
        Label("Hint",ui.entry.transform,"방 코드를 입력하거나 친구에게 알려 주세요",new Vector2(0,77),new Vector2(700,40),20,Muted);
        var field=Box("RoomCode",ui.entry.transform,new Vector2(0,0),new Vector2(470,66),Color.white);field.GetComponent<Image>().raycastTarget=true;
        ui.code=field.AddComponent<TMP_InputField>();ui.code.targetGraphic=field.GetComponent<Image>();
        var area=Rect("TextArea",field.transform,Vector2.zero,new Vector2(438,56));
        ui.code.textViewport=(RectTransform)area.transform;
        ui.code.textComponent=(TextMeshProUGUI)Label("Value",area.transform,"",Vector2.zero,new Vector2(430,56),26,Ink);
        ui.code.placeholder=Label("Placeholder",area.transform,"room-code",Vector2.zero,new Vector2(430,56),24,Muted);ui.code.characterLimit=32;
        ui.confirm=Button("Confirm",ui.entry.transform,"연결하기",new Vector2(0,-100),new Vector2(270,55));
        ui.entryBack=Button("Back",ui.entry.transform,"← 홈으로",new Vector2(-510,305),new Vector2(190,42),false);
        ui.message=Label("Message",ui.entry.transform,"",new Vector2(0,-200),new Vector2(980,100),18,new Color(.6f,.18f,.13f));

        ui.room=Panel("CharacterRoom",root.transform,Paper);
        ui.roomBack=Button("Leave",ui.room.transform,"← 메인으로",new Vector2(-510,310),new Vector2(190,40),false);
        ui.roomCode=Label("RoomCode",ui.room.transform,"방 코드",new Vector2(355,310),new Vector2(330,40),18,Ink);
        ui.copy=Button("Copy",ui.room.transform,"복사",new Vector2(570,310),new Vector2(100,38),false);
        Label("RoomTitle",ui.room.transform,"함께할 모습을 골라 주세요",new Vector2(0,250),new Vector2(800,55),29,Ink);
        ui.characterButtons=new Button[2];ui.characterPlayers=new TMP_Text[2];ui.characterLabels=new TMP_Text[2];
        for(int i=0;i<2;i++)
        {
            var card=Box(i==0?"DuyeongCard":"SodamCard",ui.room.transform,new Vector2(i==0?-260:260,10),new Vector2(380,400),new Color(.90f,.91f,.84f));
            Label("Role",card.transform,settings.roleNames[i],new Vector2(0,150),new Vector2(340,50),34,Ink);
            // Neutral silhouettes until both final character portraits are available.
            var head=Box("Head",card.transform,new Vector2(0,66),new Vector2(46,46),Ink);
            head.GetComponent<Image>().sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Box("Body",card.transform,new Vector2(0,-4),new Vector2(62,85),Ink);
            Box("LeftLeg",card.transform,new Vector2(-19,-66),new Vector2(22,66),Ink);
            Box("RightLeg",card.transform,new Vector2(19,-66),new Vector2(22,66),Ink);
            ui.characterPlayers[i]=Label("Player",card.transform,settings.vacantText,new Vector2(0,-115),new Vector2(350,35),18,Muted);
            ui.characterButtons[i]=Button("Choose",card.transform,settings.selectText,new Vector2(0,-165),new Vector2(280,45));
            ui.characterLabels[i]=ui.characterButtons[i].GetComponentInChildren<TMP_Text>();
            Label("RoleDescription",ui.room.transform,settings.roleDescriptions[i],new Vector2(i==0?-260:260,-215),new Vector2(450,36),15,Muted);
        }
        ui.readiness=Label("Ready",ui.room.transform,settings.waitingText,new Vector2(0,-264),new Vector2(950,30),17,Muted);
        ui.start=Button("Start",ui.room.transform,"시작하기",new Vector2(0,-314),new Vector2(270,48));
        ui.loading=Instance(loadingPrefab,root.transform);ui.loadingText=ui.loading.transform.Find("LoadingLabel").GetComponent<TMP_Text>();
        ui.options=Instance(optionsPrefab,root.transform);
        ui.entry.SetActive(false);ui.room.SetActive(false);ui.loading.SetActive(false);ui.options.SetActive(false);
        return Save(root,"MainMenuUI");
    }
    private static GameObject BuildOverlay()
    {
        var root=CanvasRoot("GameOverlayUI",30);var ui=root.AddComponent<GameOverlayUI>();ui.settings=settings;
        var header=Box("StageHeader",root.transform,new Vector2(0,285),new Vector2(560,88),new Color(.05f,.11f,.08f,.84f));
        ui.stageName=Label("StageName",header.transform,"서낭당",new Vector2(0,17),new Vector2(520,36),25,Paper);
        ui.objective=Label("Objective",header.transform,"함께 숲을 탐험하세요",new Vector2(0,-21),new Vector2(530,34),15,new Color(.88f,.81f,.60f));
        ui.subtitle=Label("Subtitle",root.transform,"",new Vector2(0,-255),new Vector2(920,80),24,Paper);ui.subtitle.gameObject.SetActive(false);
        ui.pauseRoot=Panel("Pause",root.transform,new Color(.02f,.07f,.04f,.78f));
        ui.pauseMessage=Label("Message",ui.pauseRoot.transform,settings.pausedText,new Vector2(0,150),new Vector2(1000,65),28,Paper);
        ui.ownMenu=Rect("OwnerMenu",ui.pauseRoot.transform,Vector2.zero,Vector2.zero);Stretch(ui.ownMenu);
        ui.resume=Button("Resume",ui.ownMenu.transform,"이어하기",new Vector2(0,20),new Vector2(300,55));
        ui.optionButton=Button("Options",ui.ownMenu.transform,"옵션",new Vector2(0,-60),new Vector2(240,46),false);
        ui.mainMenu=Button("MainMenu",ui.ownMenu.transform,"메인으로 돌아가기",new Vector2(0,-125),new Vector2(240,46),false);
        ui.options=Instance(optionsPrefab,root.transform);ui.loading=Instance(loadingPrefab,root.transform);
        ui.pauseRoot.SetActive(false);ui.options.SetActive(false);ui.loading.SetActive(false);
        return Save(root,"GameOverlayUI");
    }
    private static GameObject BuildHud()
    {
        var root=Rect("PlayerHudUI",null,Vector2.zero,new Vector2(640,720));var ui=root.AddComponent<PlayerHudUI>();ui.settings=settings;
        ui.helpHint=Label("HelpHint",root.transform,"H  키 확인",new Vector2(0,0),new Vector2(160,35),15,Paper);
        Corner((RectTransform)ui.helpHint.transform,new Vector2(0,0),new Vector2(24,20),new Vector2(0,0));
        ui.helpPanel=Box("HelpPanel",root.transform,new Vector2(0,0),new Vector2(300,320),new Color(.04f,.10f,.07f,.9f));
        Corner((RectTransform)ui.helpPanel.transform,Vector2.zero,new Vector2(24,66),Vector2.zero);
        ui.helpText=Label("Keys",ui.helpPanel.transform,"",Vector2.zero,new Vector2(268,290),16,Paper);ui.helpText.alignment=TextAlignmentOptions.MidlineLeft;
        ui.pocket=Label("Pocket",root.transform,"",Vector2.zero,new Vector2(250,34),16,Paper);
        Corner((RectTransform)ui.pocket.transform,new Vector2(1,0),new Vector2(-24,22),new Vector2(1,0));
        ui.abilityText=Label("Ability",root.transform,"",Vector2.zero,new Vector2(200,38),21,new Color(.94f,.83f,.43f));
        Corner((RectTransform)ui.abilityText.transform,new Vector2(1,0),new Vector2(-24,62),new Vector2(1,0));
        ui.feedback=Label("Feedback",root.transform,"",new Vector2(0,-220),new Vector2(540,62),18,Paper);
        ui.reticle=Label("Reticle",root.transform,"+",Vector2.zero,new Vector2(35,35),24,Paper).gameObject;
        ui.prompt=Box("Interaction",root.transform,Vector2.zero,new Vector2(200,42),new Color(.04f,.11f,.07f,.9f));
        ui.promptText=Label("Text",ui.prompt.transform,"E  채집",Vector2.zero,new Vector2(190,40),18,Paper);
        ui.markers=new RectTransform[2];ui.markerLabels=new TMP_Text[2];
        for(int i=0;i<2;i++)
        {
            var marker=Box("AbilityMarker"+i,root.transform,Vector2.zero,new Vector2(44,44),new Color(.10f,.30f,.20f,.9f));
            ui.markers[i]=(RectTransform)marker.transform;
            ui.markerLabels[i]=Label("Kind",marker.transform,"잎",Vector2.zero,new Vector2(42,40),21,Paper);
        }
        ui.helpPanel.SetActive(false);ui.prompt.SetActive(false);ui.reticle.SetActive(false);
        return Save(root,"PlayerHudUI");
    }
    private static void Corner(RectTransform r,Vector2 anchor,Vector2 pos,Vector2 pivot)
    { r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos; }
    private static void EventSystem()
    {
        if(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>()!=null)return;
        var e=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(InputSystemUIInputModule));
        e.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }
    private static void ConfigureMain()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        foreach(var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(c.transform.parent==null)c.gameObject.SetActive(false);
        foreach(var m in Object.FindObjectsByType<PrototypeModeMenu>(FindObjectsInactive.Include,FindObjectsSortMode.None))m.enabled=false;
        var session=Object.FindFirstObjectByType<FusionLobbySession>();
        if(session.GetComponent<RoomControl>()==null)session.gameObject.AddComponent<RoomControl>();
        Instance(menuPrefab,null);EventSystem();
        var lobby=AssetDatabase.LoadAssetAtPath<LobbySettings>("Assets/Lobby/LobbySettings.asset");
        lobby.manualCharacterSelection=true;lobby.loadingMessage="숲으로 향하는 중…";EditorUtility.SetDirty(lobby);
        EditorSceneManager.SaveScene(scene);
    }
    private static void ConfigurePlay(string path,string title,string objective)
    {
        var scene=EditorSceneManager.OpenScene(path);
        var screen=Object.FindFirstObjectByType<PlayScreenController>();var so=new SerializedObject(screen);var regions=so.FindProperty("regions");
        for(int i=0;i<regions.arraySize;i++)
        {
            var item=regions.GetArrayElementAtIndex(i);var parent=(RectTransform)item.FindPropertyRelative("hudRoot").objectReferenceValue;
            foreach(Transform child in parent)child.gameObject.SetActive(false);
            var hud=Instance(hudPrefab,parent);Stretch(hud);hud.GetComponent<PlayerHudUI>().slot=item.FindPropertyRelative("spawnSlot").intValue;
            PrefabUtility.RecordPrefabInstancePropertyModifications(hud.GetComponent<PlayerHudUI>());
        }
        foreach(var old in Object.FindObjectsByType<StageHud>(FindObjectsInactive.Include,FindObjectsSortMode.None))old.gameObject.SetActive(false);
        var oldStageTwo=GameObject.Find("StageTwoHUD");if(oldStageTwo!=null)oldStageTwo.SetActive(false);
        var overlay=Instance(overlayPrefab,null).GetComponent<GameOverlayUI>();overlay.title=title;overlay.defaultObjective=objective;
        PrefabUtility.RecordPrefabInstancePropertyModifications(overlay);
        var goal=Object.FindFirstObjectByType<Herbalist.Levels.StageTwoGoal>();
        if(goal!=null) { goal.objective=overlay.objective;goal.objectiveText=objective;goal.clearedText="두 사람이 위쪽 가지에 도착했습니다"; }
        EventSystem();EditorSceneManager.SaveScene(scene);
    }
}
