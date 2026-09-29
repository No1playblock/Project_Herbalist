using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using Herbalist.Levels;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.GameUI;
using Object=UnityEngine.Object;
namespace Herbalist.Editor
{
    public static class StageFourBuilder
    {
        public const string Path="Assets/_Project/Scenes/Stages/Stage_04/Stage_04_Arrival.unity";
        public const string Next="Assets/_Project/Scenes/Stages/Stage_05/Stage_05_Arrival.unity";
        private static Material _wood,_floor,_channel,_water,_end,_hole;
        private static TMP_FontAsset _font;
        private static readonly float[] Heights={39,36,24,12,0};
        public static string Build()
        {
            if(Application.isPlaying||SceneManager.GetActiveScene().isDirty)throw new Exception("Stop Play and save scene edits first.");
            var scene=EditorSceneManager.OpenScene(Path);
            if(GameObject.Find("StageFour_Layout")!=null)throw new Exception("Already authored; edit assets and scene instead of regenerating.");
            Folder("Assets/_Project/Scenes/Stages/Stage_05");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Next)!=null)throw new Exception("Stage Five placeholder already exists.");
            AssetDatabase.CopyAsset(Path,Next);
            var five=EditorSceneManager.OpenScene(Next);
            GameObject.Find("StageFour_Runtime").name="StageFive_Runtime";
            GameObject.Find("StageFour_ArrivalSpace").name="StageFive_ArrivalSpace";
            var hud=Object.FindFirstObjectByType<GameOverlayHud>();hud.title="서낭당 · 5스테이지 도착";hud.defaultObjective="회전 제단 구역 · 준비 중";
            SetEntry("FromStage04",new Vector3(0,.04f,-8));
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(five);EditorSceneManager.SaveScene(five);
            scene=EditorSceneManager.OpenScene(Path);
            Object.DestroyImmediate(GameObject.Find("StageFour_ArrivalSpace"));
            _font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NanumGothic/NanumGothicSdf.asset");
            _wood=Mat("Wood",new Color(.2f,.12f,.065f));_floor=Mat("Deck",new Color(.39f,.31f,.21f));
            _channel=Mat("Channel",new Color(.7f,.63f,.4f));_water=Mat("Sap",new Color(.1f,.7f,.9f));
            _end=Mat("Goal",new Color(.38f,.8f,.24f));_hole=Mat("Hole",new Color(.045f,.035f,.025f));
            var root=new GameObject("StageFour_Layout");var level=root.AddComponent<StageLevel>();level.nextScenePath=Next;
            var flow=root.AddComponent<StageFourFlow>();
            EntryDeck(root.transform);
            var boards=new SapMazeBoard[3];var boardRoots=new Transform[3];
            for(int i=0;i<3;i++)
            {
                float y=Heights[i+1];
                for(int side=0;side<2;side++)
                    Balcony(root.transform,"Station_"+i+"_"+side,y,side==0?-6:6);
                var definition=CreateDefinition(i);
                var board=CreateBoard(root.transform,definition,i,new Vector3(0,y,0));
                boards[i]=board;boardRoots[i]=board.transform;
            }
            Balcony(root.transform,"UpperFront",Heights[4],-6);Balcony(root.transform,"UpperBack",Heights[4],6);
            Deck(root.transform,"UpperArrivalBridge",new Vector3(0,Heights[4],0),new Vector3(18,.4f,5));
            var lifts=new StagePulleyLift[4];
            for(int i=0;i<lifts.Length;i++)
                lifts[i]=CreateLift(root.transform,i,Heights[i],Heights[i+1],i>0?boardRoots[i-1]:null);
            var zone=Zone(root.transform,"StageFiveExit",new Vector3(0,Heights[4]+1,0),new Vector3(5,3,4));
            Cube(root.transform,"FinalGoalMarker",new Vector3(0,Heights[4]+.025f,0),new Vector3(5,.05f,4),_end);
            var exit=zone.gameObject.AddComponent<CooperativeStageExit>();
            Set(exit,"_arrival",zone);Set(exit,"_level",level);Set(exit,"_condition",flow);Str(exit,"_destinationEntryId","FromStage04");
            hud=Object.FindFirstObjectByType<GameOverlayHud>();hud.title="서낭당 · 4스테이지";hud.defaultObjective="수액 미로를 함께 통과하세요.";
            Array(flow,"_mazes",boards.Cast<Object>().ToArray());Array(flow,"_lifts",lifts.Cast<Object>().ToArray());
            Set(flow,"_exitZone",zone);Set(flow,"_exit",exit);Set(flow,"_hud",hud);
            var solo=root.AddComponent<Herbalist.Development.StageFourSoloTest>();Set(solo,"_flow",flow);
            SetEntry("FromStage03",new Vector3(0,Heights[0]+.04f,-10));
            // Physical separation above the entry; the cameras remain on their own side.
            for(int side=0;side<2;side++)
                Cube(root.transform,"TrunkBoundary_"+side,new Vector3(side==0?-10:10,20,0),new Vector3(.5f,42,22),_wood);
            var route=root.AddComponent<LevelRouteGuide>();route.expectedRoute=lifts.Select(l=>l.Upper[0]).ToArray();
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var builds=EditorBuildSettings.scenes.ToList();if(!builds.Any(s=>s.path==Next))builds.Add(new EditorBuildSettingsScene(Next,true));EditorBuildSettings.scenes=builds.ToArray();
            for(int side=0;side<2;side++)
            {
                var light=new GameObject("MazeFillLight_"+side).AddComponent<Light>();light.type=LightType.Directional;light.intensity=.75f;light.shadows=LightShadows.None;light.transform.rotation=Quaternion.Euler(0,side*180,0);
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return "Stage Four authored: graph mazes with 3/4/5 platform-only leak sockets, four cooperative lifts and Stage Five arrival.";
        }
        // Explicit editor migration: update the existing authored scene without replacing its GUID or maze graph assets.
        public static string ApplyDescendingRevision()
        {
            if(Application.isPlaying || SceneManager.GetActiveScene().isDirty)throw new Exception("Stop Play and save scene edits first.");
            var scene=EditorSceneManager.OpenScene(Path);
            var root=GameObject.Find("StageFour_Layout").transform;
            var old=root.Find("EntryFloor");if(old!=null)Object.DestroyImmediate(old.gameObject);
            _floor=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Environments/Stage_04/MAT_MazeDeck.mat");
            foreach(var t in root.Cast<Transform>().ToArray())
                if(t.name.StartsWith("EntryFront")||t.name.StartsWith("EntryBack")||t.name.StartsWith("EntryDeck_"))Object.DestroyImmediate(t.gameObject);
            EntryDeck(root);
            var flow=root.GetComponent<StageFourFlow>();
            for(int i=0;i<3;i++)
            {
                var board=flow.Mazes[i];var delta=Heights[i+1]-board.transform.position.y;
                board.transform.position+=Vector3.up*delta;
                var area=board.ControlArea.transform;area.position=new Vector3(area.position.x,Heights[i+1]+1.5f,area.position.z);
                var camera=root.Find("MazeCamera_"+i);camera.position=new Vector3(camera.position.x,Heights[i+1]+5.5f,camera.position.z);
                foreach(string name in new[]{"FillInlet","SapSupply"})
                {var t=board.transform.Find(name);if(t!=null)Object.DestroyImmediate(t.gameObject);}
                board.ControlView=MazeControlView.KeepPlayerCamera;
                Str(board,"_readyText","R: 수액 조종 시작 · 잔량 {0:0}%");
                Str(board,"_moveText","WASD: 수액 이동 · R: 조종 종료 · 잔량 {0:0}%");
                Str(board,"_solvedText","미로 완료 · 양쪽 하강 발판에 탑승하세요");
                board.GetComponentInChildren<TMP_Text>().text="R: 수액 조종 시작";
                foreach(Transform t in root)
                    if(t.name.StartsWith("Station_"+i+"_"))t.position=new Vector3(t.position.x,Heights[i+1]-t.localScale.y*.5f,t.position.z);
                var data=board.Definition;data.initialVolume=12+i*4;EditorUtility.SetDirty(data);
            }
            foreach(Transform t in root)
            {
                if(t.name.StartsWith("UpperFront")||t.name.StartsWith("UpperBack")||t.name=="UpperArrivalBridge")
                    t.position=new Vector3(t.position.x,Heights[4]-t.localScale.y*.5f,t.position.z);
                if(t.name=="FinalGoalMarker")t.position=new Vector3(t.position.x,Heights[4]+.025f,t.position.z);
                if(t.name=="StageFiveExit")t.position=new Vector3(t.position.x,Heights[4]+1,t.position.z);
            }
            for(int i=0;i<flow.Lifts.Length;i++)
            {
                var lift=flow.Lifts[i];var so=new SerializedObject(lift);
                var starts=so.FindProperty("_lower");var ends=so.FindProperty("_upper");
                var pulleys=so.FindProperty("_pulleys");
                for(int side=0;side<2;side++)
                {
                    var start=(Transform)starts.GetArrayElementAtIndex(side).objectReferenceValue;
                    var end=(Transform)ends.GetArrayElementAtIndex(side).objectReferenceValue;
                    start.position=new Vector3(start.position.x,Heights[i],start.position.z);
                    end.position=new Vector3(end.position.x,Heights[i+1],end.position.z);
                    lift.Platforms[side].position=start.position;
                    var pulley=(Transform)pulleys.GetArrayElementAtIndex(side).objectReferenceValue;
                    pulley.position=new Vector3(pulley.position.x,Heights[i]+3,pulley.position.z);
                }
                so.FindProperty("_counterweightTravel").vector3Value=Vector3.up*2;so.ApplyModifiedPropertiesWithoutUndo();
            }
            var f=new SerializedObject(flow);var texts=f.FindProperty("_objectives");
            texts.GetArrayElementAtIndex(1).stringValue="R로 수액 조종을 시작하고 WASD로 도착 지점까지 이동시키세요.";
            texts.GetArrayElementAtIndex(3).stringValue="하강 발판을 이용해 아래로 이동하세요.";
            texts.GetArrayElementAtIndex(5).stringValue="하단 도착 지점에 함께 모이세요.";f.ApplyModifiedPropertiesWithoutUndo();
            SetEntry("FromStage03",new Vector3(0,Heights[0]+.04f,-10));
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            return "Descending stage authored: 39 -> 36 -> 24 -> 12 -> 0; preloaded 12/16/20 sap, Cycle control, player camera.";
        }
        private static void EntryDeck(Transform root)
        {
            // Outer approach paths keep the first maze's back-face shooting corridor free of overhead floors.
            Deck(root,"EntryDeck_Front",new Vector3(0,Heights[0],-9),new Vector3(18,.4f,2.6f));
            Deck(root,"EntryDeck_Far",new Vector3(0,Heights[0],9),new Vector3(18,.4f,2.6f));
            Deck(root,"EntryDeck_Back",new Vector3(0,Heights[0],-10),new Vector3(18,.4f,3));
            for(int side=0;side<2;side++)
                Deck(root,"EntryDeck_Side_"+side,new Vector3(side==0?-8:8,Heights[0],0),new Vector3(2,.4f,19));
        }
        private static SapMazeDefinition CreateDefinition(int index)
        {
            var d=ScriptableObject.CreateInstance<SapMazeDefinition>();
            d.initialVolume=12+index*4;d.speed=1.2f;
            StageFourMazeLayout.ApplyDefinition(d,index);
            Folder("Assets/_Project/Data/Stages/Stage_04");
            AssetDatabase.CreateAsset(d,"Assets/_Project/Data/Stages/Stage_04/SO_SapMaze_"+(index+1)+".asset");
            return d;
        }
        public static List<int> FindRoute(SapMazeDefinition d,int start,int goal)
        {
            var parent=Enumerable.Repeat(-1,d.nodes.Length).ToArray();var queue=new Queue<int>();queue.Enqueue(start);parent[start]=start;
            while(queue.Count>0)
            {
                int n=queue.Dequeue();if(n==goal)break;
                foreach(var edge in d.edges){int next=edge.x==n?edge.y:edge.y==n?edge.x:-1;if(next>=0&&parent[next]<0){parent[next]=n;queue.Enqueue(next);}}
            }
            if(parent[goal]<0)throw new Exception("Disconnected maze");
            var path=new List<int>();for(int n=goal;;n=parent[n]){path.Add(n);if(n==start)break;}path.Reverse();return path;
        }
        private static SapMazeBoard CreateBoard(Transform parent,SapMazeDefinition d,int index,Vector3 origin)
        {
            var go=new GameObject("SapMaze_"+(index+1));go.transform.SetParent(parent);go.transform.position=origin;
            var board=go.AddComponent<SapMazeBoard>();Set(board,"_definition",d);
            Cube(go.transform,"WoodDivider",origin+new Vector3(0,5.5f,0),new Vector3(14,11,.6f),_wood);
            foreach(var edge in d.edges)
            {
                Vector2 a=d.nodes[edge.x],b=d.nodes[edge.y];
                Cube(go.transform,"Channel_"+edge.x+"_"+edge.y,origin+new Vector3((a.x+b.x)*.5f,(a.y+b.y)*.5f,-.34f),
                    new Vector3(Mathf.Abs(a.x-b.x)+.18f,Mathf.Abs(a.y-b.y)+.18f,.04f),_channel,false);
            }
            var start=d.nodes[d.startNode];var end=d.nodes[d.goalNode];
            Cube(go.transform,"Start",origin+new Vector3(start.x,start.y,-.39f),new Vector3(.5f,.5f,.05f),_water,false);
            Cube(go.transform,"Goal",origin+new Vector3(end.x,end.y,-.39f),new Vector3(.5f,.5f,.05f),_end,false);
            var blob=GameObject.CreatePrimitive(PrimitiveType.Sphere);blob.name="MazeSap";blob.transform.SetParent(go.transform);blob.transform.localPosition=new Vector3(start.x,start.y,-.55f);blob.transform.localScale=Vector3.one*.42f;blob.GetComponent<Renderer>().sharedMaterial=_water;Object.DestroyImmediate(blob.GetComponent<Collider>());
            Set(board,"_sapVisual",blob.transform);
            var leaks=new MazeLeakSocket[d.leakNodes.Length];
            for(int i=0;i<leaks.Length;i++)
            {
                var p=d.nodes[d.leakNodes[i]];var hole=Cube(go.transform,"Leak_"+(i+1),origin+new Vector3(p.x,p.y,.45f),new Vector3(.65f,.65f,.4f),_hole);
                hole.SetActive(false);
                var target=hole.AddComponent<LeafInstallTarget>();Int(target,"targetId",501+index*10+i);Bool(target,"acceptsPlatform",true);Bool(target,"acceptsPin",false);Bool(target,"_singleOccupant",true);
                var socket=Point(go.transform,"LeafSocket_"+(i+1),origin+new Vector3(p.x,p.y,2.9f));socket.rotation=Quaternion.identity;Set(target,"socket",socket);
                var wet=Cube(hole.transform,"WetSurface",hole.transform.position+Vector3.forward*.23f,new Vector3(.7f,.7f,.05f),_water,false);wet.SetActive(false);
                var binding=hole.AddComponent<SapBindingSource>();Set(binding,"visual",wet);Set(target,"sap",binding);
                var stream=Cube(hole.transform,"LeakStream",hole.transform.position+new Vector3(0,-.7f,.3f),new Vector3(.15f,1.4f,.15f),_water,false);stream.SetActive(false);
                var leak=hole.AddComponent<MazeLeakSocket>();Set(leak,"_target",target);Set(leak,"_binding",binding);Set(leak,"_stream",stream);leaks[i]=leak;
                hole.SetActive(true);
            }
            Array(board,"_leaks",leaks.Cast<Object>().ToArray());
            var area=Zone(parent,"SapControlArea_"+index,origin+new Vector3(0,1.5f,-6),new Vector3(18,3,7));Set(board,"_controlArea",area);
            var camera=Point(parent,"MazeCamera_"+index,origin+new Vector3(0,5.5f,-13));camera.LookAt(origin+new Vector3(0,5.5f,0));Set(board,"_frontCamera",camera);
            var text=Label(go.transform,"MazeStatus",origin+new Vector3(0,10.6f,-.45f),Quaternion.identity,new Vector2(1400,80),.008f);
            text.text="R: 수액 조종 시작";Set(board,"_status",text);
            var back=Label(go.transform,"LeafInstructions",origin+new Vector3(0,10.6f,.45f),Quaternion.Euler(0,180,0),new Vector2(1400,80),.008f);
            back.text="새어 나오는 수액을 발판 나뭇잎으로 막으세요";
            StageFourMazeLayout.ApplyBoard(board,index);
            return board;
        }
        private static StagePulleyLift CreateLift(Transform parent,int index,float lowerY,float upperY,Transform weight)
        {
            var go=new GameObject("PulleyLift_"+index);go.transform.SetParent(parent);var lift=go.AddComponent<StagePulleyLift>();
            var platforms=new Transform[2];var lower=new Transform[2];var upper=new Transform[2];var zones=new StageArrivalZone[2];var ropes=new LineRenderer[2];var pulleys=new Transform[2];
            float x=index%2==0?-4:4;
            for(int i=0;i<2;i++)
            {
                float z=i==0?-6:6;var low=new Vector3(x,lowerY,z);var high=new Vector3(x,upperY,z);
                lower[i]=Point(go.transform,"Lower_"+i,low);upper[i]=Point(go.transform,"Upper_"+i,high);
                var platform=Point(go.transform,"Platform_"+i,low);platforms[i]=platform;
                Deck(platform,"Raft",low,new Vector3(3.6f,.3f,3.6f));
                zones[i]=Zone(platform,"Boarding",low+Vector3.up,new Vector3(3.5f,2.5f,3.5f));
                pulleys[i]=Point(go.transform,"Pulley_"+i,new Vector3(x,Mathf.Max(lowerY,upperY)+3,z));
                var rope=Point(go.transform,"Rope_"+i,Vector3.zero).gameObject.AddComponent<LineRenderer>();
                rope.sharedMaterial=_channel;rope.widthMultiplier=.055f;rope.positionCount=2;rope.SetPosition(0,low);rope.SetPosition(1,pulleys[i].position);ropes[i]=rope;
                var label=Label(platform,"BoardingLabel",low+Vector3.up*.025f,Quaternion.Euler(90,0,0),new Vector2(550,140),.005f);
                label.text=i==0?"수액 · 탑승":"나뭇잎 · 탑승";
            }
            Array(lift,"_platforms",platforms.Cast<Object>().ToArray());Array(lift,"_lower",lower.Cast<Object>().ToArray());Array(lift,"_upper",upper.Cast<Object>().ToArray());Array(lift,"_boarding",zones.Cast<Object>().ToArray());Array(lift,"_ropes",ropes.Cast<Object>().ToArray());Array(lift,"_pulleys",pulleys.Cast<Object>().ToArray());if(weight!=null)Set(lift,"_counterweight",weight);
            return lift;
        }
        private static TMP_Text Label(Transform parent,string name,Vector3 position,Quaternion rotation,Vector2 size,float scale)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Canvas));go.transform.SetParent(parent);go.transform.SetPositionAndRotation(position,rotation);go.transform.localScale=Vector3.one*scale;
            go.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;((RectTransform)go.transform).sizeDelta=size;
            var child=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));child.transform.SetParent(go.transform,false);
            var r=(RectTransform)child.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            var text=child.GetComponent<TextMeshProUGUI>();text.font=_font;text.fontSize=36;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;return text;
        }
        private static void SetEntry(string entry,Vector3 center)
        {
            var layout=Object.FindFirstObjectByType<PlayerSpawnLayout>();Str(layout,"_entryId",entry);
            var so=new SerializedObject(layout);var a=so.FindProperty("spawnPoints");
            for(int i=0;i<2;i++){var t=(Transform)a.GetArrayElementAtIndex(i).objectReferenceValue;t.position=center+Vector3.right*(i==0?-1.25f:1.25f);t.rotation=Quaternion.identity;}
            var player=(GameObject)so.FindProperty("offlinePlayer").objectReferenceValue;player.transform.position=center+Vector3.left*1.25f;player.transform.rotation=Quaternion.identity;
        }
        private static void Balcony(Transform parent,string name,float y,float z)
        {
            Deck(parent,name+"_Left",new Vector3(-7.5f,y,z),new Vector3(3,.4f,7));
            Deck(parent,name+"_Middle",new Vector3(0,y,z),new Vector3(4,.4f,7));
            Deck(parent,name+"_Right",new Vector3(7.5f,y,z),new Vector3(3,.4f,7));
            Deck(parent,name+"_Front",new Vector3(0,y,z-2.75f),new Vector3(18,.4f,1.5f));
            Deck(parent,name+"_Back",new Vector3(0,y,z+2.75f),new Vector3(18,.4f,1.5f));
        }
        private static GameObject Cube(Transform parent,string name,Vector3 position,Vector3 size,Material material,bool solid=true)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;
            if(!solid)Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        private static void Deck(Transform parent,string name,Vector3 top,Vector3 size)=>Cube(parent,name,top-Vector3.up*size.y*.5f,size,_floor);
        private static Transform Point(Transform parent,string name,Vector3 p){var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=p;return go.transform;}
        private static StageArrivalZone Zone(Transform p,string name,Vector3 pos,Vector3 size){var go=Point(p,name,pos).gameObject;var b=go.AddComponent<BoxCollider>();b.isTrigger=true;b.size=size;var z=go.AddComponent<StageArrivalZone>();Set(z,"_volume",b);return z;}
        private static Material Mat(string name,Color color){Folder("Assets/_Project/Art/Materials/Environments/Stage_04");var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;AssetDatabase.CreateAsset(m,"Assets/_Project/Art/Materials/Environments/Stage_04/MAT_Maze"+name+".mat");return m;}
        private static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
        private static void Set(Object o,string f,Object v)=>StageThreeBuilder.Set(o,f,v);
        private static void Int(Object o,string f,int v)=>StageThreeBuilder.SetInt(o,f,v);
        private static void Bool(Object o,string f,bool v)=>StageThreeBuilder.SetBool(o,f,v);
        private static void Str(Object o,string f,string v)=>StageThreeBuilder.SetString(o,f,v);
        private static void Array(Object o,string f,Object[] v)=>StageThreeBuilder.SetArray(o,f,v);
    }
}