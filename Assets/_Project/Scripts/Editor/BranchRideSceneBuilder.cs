using System;
using System.Linq;
using System.Collections.Generic;
using Fusion;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.Presentation;
using Herbalist.StageOne;
using Herbalist.GameUI;
using Herbalist.Audio;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Rebuild is editor-only. Every mesh, target, spawn and UGUI element is saved in the scene.
    public static class BranchRideSceneBuilder
    {
        public const string Path = CurrentStagePaths.BranchRide;
        private const string MatRoot = "Assets/_Project/Art/Materials/Environments/Stage_03/";
        private const string DataRoot = "Assets/_Project/Data/Stages/Stage_03/";
        private static Transform _root;
        private static Material _fixed, _rotating, _red, _white, _stone, _cyan, _green;
        private static readonly List<BranchRideTrack> Tracks = new();
        private static readonly List<BranchRideTrack> _mainTrack = new();
        private static readonly List<LeafInstallTarget> Surfaces = new();
        private static readonly List<Transform> Pivots = new();
        private static readonly List<BranchRideSwitch> Switches = new();
        private static readonly List<SapInjectionPort> Ports = new();
        private static readonly List<BranchRideStone> Hazards = new();
        private static int _targetId;
        [MenuItem("Herbalist/Stages/Open Branch Ride Stage")]
        public static void Create()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(Path);
        }
        [MenuItem("Herbalist/Stages/Rebuild Stage Three Blockout")]
        public static void Rebuild()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play first.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != Path)
            {
                if (scene.isDirty) throw new InvalidOperationException("Save the current scene before rebuilding Stage Three.");
                scene = EditorSceneManager.OpenScene(Path);
            }
            // Backup the last saved scene before replacing this stage's generated layout.
            System.IO.Directory.CreateDirectory("Temp/BranchRideBackup");
            System.IO.File.Copy(Path, "Temp/BranchRideBackup/Stage_03_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity", false);
            var flow = Object.FindFirstObjectByType<BranchRideFlow>();
            if (flow == null) throw new InvalidOperationException("Existing BranchRideFlow missing.");
            var level = Object.FindFirstObjectByType<StageLevel>();
            var children = flow.transform.Cast<Transform>().Where(t => t.name != "SfxChannels").ToArray();
            foreach (var child in children) Object.DestroyImmediate(child.gameObject);
            flow.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); flow.transform.localScale = Vector3.one;
            _root = New("Stage03_Blockout", flow.transform, Vector3.zero);
            Tracks.Clear(); _mainTrack.Clear(); Surfaces.Clear(); Pivots.Clear(); Switches.Clear(); Ports.Clear(); Hazards.Clear(); _targetId = 33000;
            _fixed = Material("MAT_BranchRideFixed", new Color(.34f,.22f,.12f));
            _rotating = Material("MAT_BranchRideRotating", new Color(.6f,.2f,.8f));
            _red = Material("MAT_BranchRideTarget", new Color(.85f,.08f,.07f));
            _white = Material("MAT_BranchRidePaper", new Color(.92f,.85f,.65f));
            _stone = Material("MAT_BranchRideStone", new Color(.35f,.39f,.42f));
            _cyan = Material("MAT_BranchRideDrop", new Color(.1f,.7f,.8f));
            _green = Material("MAT_BranchRideGoal", new Color(.2f,.65f,.3f));
            var baseRules = AssetDatabase.LoadAssetAtPath<StageAbilityRules>("Assets/_Project/Data/Stages/Revision_260928/SO_StageAbilityRules.asset");
            Folder(DataRoot.TrimEnd('/'));
            var rules = AssetDatabase.LoadAssetAtPath<StageAbilityRules>(DataRoot+"SO_BranchRideAbilityRules.asset");
            if (rules == null) { rules = Object.Instantiate(baseRules); AssetDatabase.CreateAsset(rules,DataRoot+"SO_BranchRideAbilityRules.asset"); }
            rules.leafInstructions = "R: 경로에 잎 설치 · 좌클릭: 회수";
            rules.sapInstructions = "R 유지: 과녁 작동";
            EditorUtility.SetDirty(rules); level.abilityRules = rules; level.requireEarnedAbilities = true; level.nextScenePath = "";
            Fixed("Entry", V(0,24,0), V(5,24,2), V(10,24,3));
            Junction(0, 2, V(10,24,3));
            Fixed("UpperExit", V(34,24,3), V(38,24,1), V(41,24,1));
            Drop("Drop1", V(41,24,1), V(43,16,1));
            Fixed("MiddleEntry", V(43,16,1), V(47,16,-2), V(52,16,-2));
            Junction(1, 3, V(52,16,-2));
            Fixed("MiddleBend", V(88,16,-2), V(93,16,2), V(98,16,6));
            Junction(2, 3, V(98,16,6));
            Fixed("StoneRun", V(134,16,6), V(139,16,2), V(143,16,-4), V(148,16,-4));
            Junction(3, 2, V(148,16,-4));
            Fixed("MiddleExit", V(172,16,-4), V(177,16,0), V(180,16,0));
            Drop("Drop2", V(180,16,0), V(182,8,0));
            Fixed("FinalApproach", V(182,8,0), V(188,8,0), V(194,8,0));
            SerializedEditorFields.SetBool(_mainTrack[_mainTrack.Count-1],"_isFinish",true);
            Deck("StartDeck",V(-2,23,0),new Vector3(10,2,10),_fixed);
            Deck("FinishDeck",V(197,7,0),new Vector3(12,2,10),_fixed);
            var talisman = New("SharedTalisman",_root,V(0,24.5f,0));
            var carrier = talisman.gameObject.AddComponent<BranchRideCarrier>();
            Deck("Paper",V(0,24.5f,0),new Vector3(6.2f,.3f,4.5f),_white,talisman);
            for(int i=0;i<4;i++) Deck("SealStroke"+i,V(-.7f+i*.45f,24.67f,0),new Vector3(.18f,.03f,2.3f-i*.25f),_red,talisman);
            // Two visible rolling cycles, opposite the carrier's approach, share the middle tier.
            Stone("StoneA",0,V(98,16,6),V(93,16,2),V(88,16,-2));
            Stone("StoneB",2.1f,V(148,16,-4),V(143,16,-4),V(139,16,2),V(134,16,6));
            var respawns = new[]{New("Respawn_Duyeong",_root,V(-3,24.08f,-2)),New("Respawn_Sodam",_root,V(-3,24.08f,2))};
            var exits = new[]{New("Dismount_Duyeong",_root,V(196,8.08f,-2)),New("Dismount_Sodam",_root,V(196,8.08f,2))};
            var spawnLayout = Object.FindFirstObjectByType<PlayerSpawnLayout>();
            SerializedEditorFields.SetArray(spawnLayout,"spawnPoints",respawns);
            var spawnData = new SerializedObject(spawnLayout);
            var offline = ((GameObject)spawnData.FindProperty("offlinePlayer").objectReferenceValue).GetComponent<PlayerController>();
            offline.transform.SetPositionAndRotation(respawns[0].position,Quaternion.Euler(0,90,0)); offline.transform.localScale=Vector3.one*2.7f;
            var offlineAbility = offline.GetComponent<PlayerAbilityController>();
            SerializedEditorFields.SetInt(offlineAbility,"prototypeOfflineAbility",(int)PlayerAbilityKind.Sap);
            SerializedEditorFields.SetBool(offlineAbility,"prototypeOfflineUnlock",true);
            offline.GetComponent<StageActor>().offlineSlot=0;
            // Reuse the project's existing two-actor offline possession adapter for local verification.
            var oldSecond = GameObject.Find("BranchRideOfflineSodam");
            if(oldSecond!=null) Object.DestroyImmediate(oldSecond);
            var second = Object.Instantiate(offline.gameObject);
            second.name="BranchRideOfflineSodam"; second.transform.SetParent(offline.transform.parent,true);
            second.transform.SetPositionAndRotation(respawns[1].position,Quaternion.Euler(0,90,0));second.transform.localScale=Vector3.one*2.7f;
            var secondPlayer=second.GetComponent<PlayerController>();
            SerializedEditorFields.SetBool(secondPlayer,"locallyControlled",false);
            second.GetComponent<StageActor>().offlineSlot=1;
            SerializedEditorFields.SetInt(second.GetComponent<PlayerAbilityController>(),"prototypeOfflineAbility",(int)PlayerAbilityKind.Leaf);
            SerializedEditorFields.SetArray(spawnLayout,"additionalOfflinePlayers",new Object[]{second});
            var screen=Object.FindFirstObjectByType<PlayScreenController>();
            var possession=spawnLayout.GetComponent<OfflinePossessionController>() ?? spawnLayout.gameObject.AddComponent<OfflinePossessionController>();
            SerializedEditorFields.Set(possession,"_duyeong",offline);SerializedEditorFields.Set(possession,"_sodam",secondPlayer);SerializedEditorFields.Set(possession,"_screen",screen);
            SerializedEditorFields.Set(possession,"_possessAction",AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionReference>("Assets/_Project/Settings/Input/References/Player/OfflinePossessOther.asset"));
            // Existing UGUI stays intact. Completion is an authored child, hidden until arrival.
            var hud=Object.FindFirstObjectByType<GameOverlayHud>();hud.title="3스테이지 · 가지 타기";
            var ending=Ending(hud);
            var presentation=flow.GetComponent<BranchRidePresentation>() ?? flow.gameObject.AddComponent<BranchRidePresentation>();
            SerializedEditorFields.Set(presentation,"_flow",flow);SerializedEditorFields.Set(presentation,"_hud",hud);
            SerializedEditorFields.Set(presentation,"_firstTarget",Ports[0].transform);SerializedEditorFields.Set(presentation,"_firstStone",Hazards[0].transform);
            SerializedEditorFields.Set(flow,"_level",level); SerializedEditorFields.Set(flow,"_talisman",talisman);
            SerializedEditorFields.Set(flow,"_carrier",carrier);
            SerializedEditorFields.SetArray(flow,"_track",Tracks.ToArray()); SerializedEditorFields.SetArray(flow,"_switches",Switches.ToArray());
            SerializedEditorFields.SetArray(flow,"_hazards",Hazards.ToArray()); SerializedEditorFields.SetArray(flow,"_leafSurfaces",Surfaces.ToArray());
            SerializedEditorFields.SetArray(flow,"_respawns",respawns); SerializedEditorFields.SetArray(flow,"_dismounts",exits);
            SerializedEditorFields.SetArray(flow,"_targets",Ports.ToArray()); SerializedEditorFields.SetArray(flow,"_branches",Pivots.ToArray());
            SerializedEditorFields.SetArray(flow,"_route",_mainTrack.Select(t=>t.transform).ToArray());
            SerializedEditorFields.SetArray(flow,"_stones",Hazards.Select(h=>h.Visual).ToArray());
            SerializedEditorFields.Set(flow,"_stopSocket",null);SerializedEditorFields.Set(flow,"_endingPanel",ending);
            Number(flow,"_boardRange",8);Number(flow,"_speed",3);VectorArray(flow,"_riderOffsets",new[]{new Vector3(-1.5f,.3f,0),new Vector3(1.5f,.3f,0)});
            Text(flow,"_ridingText","수액: 과녁 조준 + R · 잎: 경로 조준 + R / 좌클릭 회수 · 돌이 지나갈 때까지 정지");
            var audio=flow.GetComponent<WorldSfxPresentation>();
            if(audio!=null){SerializedEditorFields.Set(audio,"_motion",talisman);SerializedEditorFields.SetArray(audio,"_stones",Hazards.Select(h=>h.Visual).ToArray());}
            // Frame the whole course for editing, without adding a gameplay camera.
            foreach(var toggle in Switches)toggle.Present(0);
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.LookAt(new Vector3(97,16,0),Quaternion.Euler(32,-35,0),125);
            Debug.Log("Stage 3 authored: 4 targets / 10 rotating branches / 2 drops / 2 rolling stones / two offline actors. Saved "+Path);
        }
        private static Vector3 V(float x,float y,float z)=>new(x,y,z);
        private static Transform New(string name,Transform parent,Vector3 position)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=position;return go.transform;}
        private static Transform Point(string name,Transform parent,Vector3 position)=>New(name,parent,position);
        private static BranchRideTrack Track(string name,Transform parent,Vector3[] points,bool drop=false,int group=-1)
        {
            var root=New(name,parent,points[0]);var track=root.gameObject.AddComponent<BranchRideTrack>();
            var nodes=points.Select((p,i)=>Point("Node_"+i,root,p)).ToArray();
            SerializedEditorFields.SetArray(track,"_points",nodes);SerializedEditorFields.SetBool(track,"_drop",drop);SerializedEditorFields.SetInt(track,"_switchIndex",group);
            Tracks.Add(track);return track;
        }
        private static void Fixed(string name,params Vector3[] points)
        {var track=Track(name,_root,points);Main(track);for(int i=1;i<points.Length;i++)Branch("Branch_"+i,track.transform,points[i-1],points[i],_fixed);}
        private static void Main(BranchRideTrack track,bool linkPrevious=true)
        {
            if(linkPrevious && _mainTrack.Count>0)SerializedEditorFields.SetArray(_mainTrack[_mainTrack.Count-1],"_exits",new Object[]{track});
            _mainTrack.Add(track);
        }
        private static void Decoration(string name,params Vector3[] points)
        {var root=New(name,_root,points[0]);for(int i=1;i<points.Length;i++)Branch("SideArm_"+i,root,points[i-1],points[i],_fixed);}
        private static void Drop(string name,Vector3 from,Vector3 to)
        {
            Main(Track(name,_root,new[]{from,to},true));
            // Landing rim markers sit off the carrier lane; the gap contains no collider.
            for(int i=0;i<2;i++)Deck(name+"_LandingEdge_"+i,to+new Vector3(0,.03f,i==0?-1.7f:1.7f),new Vector3(3,.08f,.12f),_cyan);
        }
        private static void Junction(int index,int count,Vector3 origin)
        {
            var group=New("T"+(index+1)+"_BranchGroup",_root,origin);
            var pivots=new List<Transform>();
            for(int i=0;i<count;i++)
            {
                // Each module is a railway turnout: a fixed heel, a rotating tongue and two real exits.
                Vector3 heel=origin+Vector3.right*(i*12);
                float side=i%2==0?1:-1;
                var pivot=New("Turnout_"+(i+1)+"_Pivot",group,heel);pivots.Add(pivot);Pivots.Add(pivot);
                var tongue=Track("SwitchTongue",pivot,new[]{heel,heel+Vector3.right*6},false,index);
                Branch("PurpleTongue",tongue.transform,heel,heel+Vector3.right*6,_rotating);
                Main(tongue);
                Sphere("CommonHeel",group,heel-Vector3.up*1.6f,1.6f,_fixed,true);
                float reach=6*Mathf.Cos(30*Mathf.Deg2Rad);
                var aPoints=new[]{heel+new Vector3(reach,0,side*3),heel+new Vector3(8,0,side*5.5f),heel+new Vector3(11,0,side*7)};
                var bPoints=new[]{heel+new Vector3(reach,0,-side*3),heel+new Vector3(9,0,-side*3),heel+new Vector3(12,0,0)};
                var routeA=Track("RouteA_DeadEnd",group,aPoints);
                var routeB=Track("RouteB_Through",group,bPoints);
                for(int j=1;j<aPoints.Length;j++)Branch("BranchA_"+j,routeA.transform,aPoints[j-1],aPoints[j],_fixed);
                for(int j=1;j<bPoints.Length;j++)Branch("BranchB_"+j,routeB.transform,bPoints[j-1],bPoints[j],_fixed);
                SerializedEditorFields.SetArray(tongue,"_exits",new Object[]{routeA,routeB});
                Main(routeB,false);
                Label("A",routeA.transform,aPoints[0]+Vector3.up*2.2f);
                Label("B",routeB.transform,bPoints[0]+Vector3.up*2.2f);
            }
            var target=New("T"+(index+1)+"_Target",group,origin+new Vector3(-2,4,index%2==0?-4:4));
            // Sphere targets are hittable from every orbit direction; rings communicate the bullseye.
            Sphere("OuterTarget",target,target.position,1.05f,_red,true);
            Sphere("InnerTarget",target,target.position+Vector3.left*.85f,.7f,_white,false);
            Sphere("Bullseye",target,target.position+Vector3.left*1.38f,.32f,_red,false);
            var port=target.gameObject.AddComponent<SapInjectionPort>();Number(port,"_hintRange",.1f);Ports.Add(port);
            Label("T"+(index+1)+"  x"+count,target,target.position+Vector3.up*1.8f);
            var toggle=group.gameObject.AddComponent<BranchRideSwitch>();Switches.Add(toggle);
            SerializedEditorFields.Set(toggle,"_port",port);SerializedEditorFields.SetArray(toggle,"_pivots",pivots.ToArray());
            VectorArray(toggle,"_connectedEuler",Enumerable.Range(0,count).Select(i=>new Vector3(0,i%2==0?30:-30,0)).ToArray());
            VectorArray(toggle,"_disconnectedEuler",Enumerable.Range(0,count).Select(i=>new Vector3(0,i%2==0?-30:30,0)).ToArray());
        }
        private static void Stone(string name,float phase,params Vector3[] points)
        {
            var root=New(name,_root,points[0]);var hazard=root.gameObject.AddComponent<BranchRideStone>();
            var visual=Sphere("RollingStone",root,points[0]+Vector3.up*8,.85f,_stone,false);
            SerializedEditorFields.Set(hazard,"_visual",visual);SerializedEditorFields.SetArray(hazard,"_rollPoints",points.Select((p,i)=>Point("Roll_"+i,root,p)).ToArray());
            Number(hazard,"_phaseOffset",phase);Number(hazard,"_radius",.85f);Number(hazard,"_respawnDelay",3);Hazards.Add(hazard);hazard.Present(0,false);
        }
        private static void Branch(string name,Transform parent,Vector3 a,Vector3 b,Material material)
        {
            var mesh=ShapeGenerator.GenerateCylinder(PivotLocation.Center,10,1.6f,Vector3.Distance(a,b),0);
            var go=mesh.gameObject;go.name=name;go.transform.SetParent(parent,true);
            go.transform.SetPositionAndRotation((a+b)*.5f-Vector3.up*1.6f,Quaternion.FromToRotation(Vector3.up,b-a));
            go.GetComponent<Renderer>().sharedMaterial=material;
            if(go.GetComponent<MeshCollider>()==null)go.AddComponent<MeshCollider>();
            var target=go.AddComponent<LeafInstallTarget>();SerializedEditorFields.SetInt(target,"targetId",++_targetId);Surfaces.Add(target);
            var surface=go.AddComponent<SfxSurface>();SerializedEditorFields.SetInt(surface,"_kind",(int)SfxSurfaceKind.Wood);
        }
        private static Transform Deck(string name,Vector3 point,Vector3 size,Material material,Transform parent=null)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent??_root,true);go.transform.position=point;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
        }
        private static Transform Sphere(string name,Transform parent,Vector3 point,float radius,Material material,bool collision)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;go.transform.SetParent(parent,true);go.transform.position=point;go.transform.localScale=Vector3.one*radius*2;go.GetComponent<Renderer>().sharedMaterial=material;
            if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());return go.transform;
        }
        private static void Label(string text,Transform parent,Vector3 point)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Canvas));go.transform.SetParent(parent,false);go.transform.position=point;
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            var rect=(RectTransform)go.transform;rect.sizeDelta=new Vector2(240,60);rect.localScale=Vector3.one*.012f;
            var child=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));child.transform.SetParent(rect,false);
            var r=(RectTransform)child.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.sizeDelta=Vector2.zero;
            var tmp=child.GetComponent<TextMeshProUGUI>();tmp.text=text;tmp.fontSize=38;tmp.alignment=TextAlignmentOptions.Center;tmp.color=Color.white;tmp.raycastTarget=false;
        }
        private static GameObject Ending(GameOverlayHud hud)
        {
            var old=hud.transform.Find("BranchRideComplete");if(old!=null)Object.DestroyImmediate(old.gameObject);
            var root=new GameObject("BranchRideComplete",typeof(RectTransform),typeof(UnityEngine.UI.Image));root.transform.SetParent(hud.transform,false);
            var rect=(RectTransform)root.transform;rect.anchorMin=new Vector2(.25f,.4f);rect.anchorMax=new Vector2(.75f,.65f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            root.GetComponent<UnityEngine.UI.Image>().color=new Color(.025f,.06f,.045f,.94f);root.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var child=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));child.transform.SetParent(rect,false);
            var r=(RectTransform)child.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(20,10);r.offsetMax=new Vector2(-20,-10);
            var tmp=child.GetComponent<TextMeshProUGUI>();tmp.font=hud.objective.font;tmp.text="서낭당 최하단 도착\n3스테이지 완료";tmp.fontSize=30;tmp.alignment=TextAlignmentOptions.Center;tmp.raycastTarget=false;
            root.SetActive(false);return root;
        }
        private static Material Material(string name,Color color)
        {
            Folder(MatRoot.TrimEnd('/'));var path=MatRoot+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){var template=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Stages/M_Stage03_Branch.mat");mat=new Material(template);AssetDatabase.CreateAsset(mat,path);}
            mat.SetColor("_BaseColor",color);mat.color=color;EditorUtility.SetDirty(mat);return mat;
        }
        private static void Folder(string path)
        {if(AssetDatabase.IsValidFolder(path))return;Folder(System.IO.Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(path).Replace('\\','/'),System.IO.Path.GetFileName(path));}
        private static void Number(Object obj,string field,float value){var so=new SerializedObject(obj);so.FindProperty(field).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void Text(Object obj,string field,string value){var so=new SerializedObject(obj);so.FindProperty(field).stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void VectorArray(Object obj,string field,Vector3[] values)
        {var so=new SerializedObject(obj);var array=so.FindProperty(field);array.arraySize=values.Length;for(int i=0;i<values.Length;i++)array.GetArrayElementAtIndex(i).vector3Value=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
    }
}

