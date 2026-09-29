using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;
using Herbalist.Levels;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.GameUI;
using Object=UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class StageThreeBuilder
    {
        public const string ScenePath="Assets/_Project/Scenes/Stages/Stage_03/Stage_03_Prototype.unity";
        public const string NextPath="Assets/_Project/Scenes/Stages/Stage_04/Stage_04_Arrival.unity";
        private const string PadPrefab="Assets/_Project/Prefabs/InteractiveObjects/Stages/PF_SapJumpPad.prefab";
        private static Material _wood,_deck,_sap,_leaf;
        private static TMP_FontAsset _font;
        public static string Build()
        {
            if(Application.isPlaying || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Stop Play and save scene changes first.");
            var scene=EditorSceneManager.OpenScene(ScenePath);
            if(GameObject.Find("StageThree_Layout")!=null)throw new InvalidOperationException("Stage Three already authored. Edit scene and prefab instead of regenerating.");
            EnsureFolder("Assets/_Project/Scenes/Stages/Stage_04");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(NextPath)!=null)throw new InvalidOperationException("Stage Four arrival already exists.");
            AssetDatabase.CopyAsset(ScenePath,NextPath);
            BuildArrival();
            scene=EditorSceneManager.OpenScene(ScenePath);
            _wood=Mat("MAT_StageThreeWood",new Color(.23f,.15f,.08f));
            _deck=Mat("MAT_StageThreeDeck",new Color(.36f,.32f,.22f));
            _sap=Mat("MAT_StageThreeInlet",new Color(.12f,.65f,.75f));
            _leaf=Mat("MAT_StageThreeSocket",new Color(.32f,.64f,.16f));
            _font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NanumGothic/NanumGothicSdf.asset");
            Object.DestroyImmediate(GameObject.Find("StageThree_TestSpace"));
            var root=new GameObject("StageThree_Layout");
            var level=root.AddComponent<StageLevel>();level.nextScenePath=NextPath;
            var config=ScriptableObject.CreateInstance<SapJumpPadSettings>();
            EnsureFolder("Assets/_Project/Data/Stages/Stage_03");
            AssetDatabase.CreateAsset(config,"Assets/_Project/Data/Stages/Stage_03/SO_SapJumpPadSettings.asset");
            var starts=new[]{new Vector3(0,0,0),new Vector3(10,9,8),new Vector3(-2,18,16)};
            var ends=new[]{new Vector3(0,6,8),new Vector3(10,15,16),new Vector3(-2,24,24)};
            var pads=new List<SapJumpPad>();
            var firstLanding=default(StageArrivalZone);
            var gaps=new List<StageArrivalZone>();
            var route=new List<Transform>();
            for(int i=0;i<starts.Length;i++)
            {
                Deck(root.transform,"PadDeck_"+(i+1),starts[i],new Vector3(8,.6f,7));
                var pad=i==0 ? CreatePad(config) : ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PadPrefab))).GetComponent<SapJumpPad>();
                pad.name="SapJumpPad_"+(i+1);
                if(i==0)
                {
                    EnsureFolder("Assets/_Project/Prefabs/InteractiveObjects/Stages");
                    PrefabUtility.SaveAsPrefabAsset(pad.gameObject,PadPrefab);
                    Object.DestroyImmediate(pad.gameObject);
                    pad=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PadPrefab))).GetComponent<SapJumpPad>();
                }
                pad.transform.SetParent(root.transform);pad.transform.position=starts[i];pad.name="SapJumpPad_"+(i+1);
                SetInt(pad.LeafSlot,"targetId",301+i);
                SetInt(pad.Inlet.GetComponent<SapReceiver>(),"receiverId",401+i);
                var landing=Point(root.transform,"Landing_"+(i+1),ends[i]);
                Deck(root.transform,"LandingDeck_"+(i+1),ends[i],new Vector3(7,.6f,6));
                var a=Point(landing,"Slot_0",ends[i]+new Vector3(-.9f,.04f,0));
                var b=Point(landing,"Slot_1",ends[i]+new Vector3(.9f,.04f,0));
                SetArray(pad,"_landings",new Object[]{a,b});
                var zone=Zone(root.transform,"LandingZone_"+(i+1),ends[i]+Vector3.up,new Vector3(6,3,5));
                if(i==0)firstLanding=zone;
                pad.gameObject.SetActive(true);
                pads.Add(pad);route.Add(pad.transform);route.Add(landing);
                if(i<starts.Length-1)
                {
                    // A raised landing cannot be reached by a normal jump; build leaves on these pillars.
                    var middle=Vector3.Lerp(ends[i],starts[i+1],.55f);
                    var pillar=Cube(root.transform,"LeafClimbPillar_"+(i+1),new Vector3(middle.x,starts[i+1].y-3,middle.z+2),new Vector3(1.3f,12,1.3f),_wood);
                    pillar.SetActive(false);
                    var target=pillar.AddComponent<LeafInstallTarget>();SetInt(target,"targetId",311+i);SetBool(target,"acceptsPin",true);
                    var receiver=pillar.AddComponent<SapReceiver>();SetInt(receiver,"receiverId",411+i);Set(receiver,"leafTarget",target);
                    var supply=pillar.AddComponent<SapSource>();Set(supply,"interactionVolume",pillar.GetComponent<Collider>());pillar.SetActive(true);
                    gaps.Add(Zone(root.transform,"LeafRouteHint_"+(i+1),ends[i]+Vector3.up,new Vector3(8,3,7)));
                }
            }
            var exitZone=Zone(root.transform,"BothPlayers_StageFourExit",ends[2]+new Vector3(0,1,2),new Vector3(5,3,1.6f));
            var exit=exitZone.gameObject.AddComponent<CooperativeStageExit>();
            Set(exit,"_arrival",exitZone);Set(exit,"_level",level);SetString(exit,"_destinationEntryId","FromStage03");
            Cube(root.transform,"ExitMarker",ends[2]+new Vector3(0,.02f,2),new Vector3(4,.04f,.8f),_sap);
            var spawns=SetSpawns(new Vector3(0,.04f,-2.5f),"FromStage02");
            var fall=root.AddComponent<StageFallRecovery>();
            Set(fall,"_fallThreshold",Point(root.transform,"FallThreshold",Vector3.down*5));
            SetArray(fall,"_respawns",spawns.Cast<Object>().ToArray());
            Deck(root.transform,"LowerCatchFloor",Vector3.down*8,new Vector3(55,1,55));
            var hud=Object.FindFirstObjectByType<GameOverlayHud>();
            hud.title="서낭당 · 3스테이지";hud.defaultObjective="수액 점프대를 이용하세요.";
            var progress=root.AddComponent<StageThreeProgress>();
            Set(progress,"_firstPad",pads[0]);Set(progress,"_firstLanding",firstLanding);
            Set(progress,"_discovery",Zone(root.transform,"FirstPadDiscovery",new Vector3(0,1,-1),new Vector3(9,4,9)));
            SetArray(progress,"_leafGaps",gaps.Cast<Object>().ToArray());Set(progress,"_exit",exit);Set(progress,"_exitZone",exitZone);Set(progress,"_hud",hud);
            var guide=root.AddComponent<LevelRouteGuide>();guide.expectedRoute=route.ToArray();
            // Slender trunk columns outline the vertical space without obstructing ballistic paths.
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4;
                Cube(root.transform,"TrunkColumn_"+i,new Vector3(4+Mathf.Cos(a)*21,12,12+Mathf.Sin(a)*24),new Vector3(2,40,2),_wood);
            }
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var builds=EditorBuildSettings.scenes.ToList();
            if(!builds.Any(s=>s.path==NextPath))builds.Add(new EditorBuildSettingsScene(NextPath,true));
            EditorBuildSettings.scenes=builds.ToArray();
            AssetDatabase.SaveAssets();
            return "Stage Three: 3 cooperative pads, 2 leaf climbs, individual recovery, tutorial UGUI, Stage Four arrival.";
        }
        private static SapJumpPad CreatePad(SapJumpPadSettings settings)
        {
            var go=new GameObject("SapJumpPad");go.SetActive(false);
            var pad=go.AddComponent<SapJumpPad>();Set(pad,"_settings",settings);
            var slot=Cube(go.transform,"LeafSocket",new Vector3(0,.04f,0),new Vector3(1.2f,.08f,1.2f),_leaf);
            var leaf=slot.AddComponent<LeafInstallTarget>();SetInt(leaf,"targetId",301);SetBool(leaf,"_singleOccupant",true);
            Set(leaf,"socket",Point(go.transform,"LeafCenter",new Vector3(0,.16f,0)));
            var inlet=Cube(go.transform,"SapInlet",new Vector3(3,1,0),new Vector3(.8f,.8f,.8f),_sap);
            var port=inlet.AddComponent<SapInjectionPort>();
            var receiver=inlet.AddComponent<SapReceiver>();SetInt(receiver,"receiverId",401);
            var source=Cube(go.transform,"SapSupply",new Vector3(0,.65f,2.8f),new Vector3(1,1.3f,.8f),_wood);
            Set(source.AddComponent<SapSource>(),"interactionVolume",source.GetComponent<Collider>());
            Cube(go.transform,"ConduitBottom",new Vector3(1.5f,-.35f,0),new Vector3(3,.4f,.4f),_wood);
            Cube(go.transform,"ConduitUpright",new Vector3(3,.2f,0),new Vector3(.4f,1.3f,.4f),_wood);
            var volume=Point(go.transform,"Riders",Vector3.up).gameObject.AddComponent<BoxCollider>();volume.isTrigger=true;volume.size=new Vector3(6,2,5.2f);
            var pulse=GameObject.CreatePrimitive(PrimitiveType.Sphere);pulse.name="LeafLaunchVisual";pulse.transform.SetParent(go.transform);pulse.transform.localPosition=new Vector3(0,.16f,0);pulse.transform.localScale=new Vector3(6,.16f,5.2f);pulse.GetComponent<Renderer>().sharedMaterial=_leaf;Object.DestroyImmediate(pulse.GetComponent<Collider>());pulse.SetActive(false);
            var ready=Cube(go.transform,"ReadyIndicator",new Vector3(3,1.6f,0),new Vector3(.3f,.3f,.3f),_leaf);Object.DestroyImmediate(ready.GetComponent<Collider>());ready.SetActive(false);
            Set(pad,"_leafSlot",leaf);Set(pad,"_inlet",port);Set(pad,"_riderVolume",volume);Set(pad,"_launchVisual",pulse.transform);Set(pad,"_readyVisual",ready);
            var canvasGo=new GameObject("PadStatusCanvas",typeof(RectTransform),typeof(Canvas));canvasGo.transform.SetParent(go.transform);canvasGo.transform.localPosition=new Vector3(0,2.9f,.8f);canvasGo.transform.localScale=Vector3.one*.007f;
            canvasGo.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            ((RectTransform)canvasGo.transform).sizeDelta=new Vector2(800,120);
            var labelGo=new GameObject("Status",typeof(RectTransform),typeof(TextMeshProUGUI));labelGo.transform.SetParent(canvasGo.transform,false);
            var rect=(RectTransform)labelGo.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var label=labelGo.GetComponent<TextMeshProUGUI>();label.font=_font;label.fontSize=32;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;label.text="발판 나뭇잎을 구멍에 설치하세요";
            var status=canvasGo.AddComponent<SapJumpPadStatus>();Set(status,"_pad",pad);Set(status,"_label",label);
            return pad;
        }
        private static void BuildArrival()
        {
            var scene=EditorSceneManager.OpenScene(NextPath);
            GameObject.Find("StageThree_Runtime").name="StageFour_Runtime";
            GameObject.Find("StageThree_TestSpace").name="StageFour_ArrivalSpace";
            var hud=Object.FindFirstObjectByType<GameOverlayHud>();hud.title="서낭당 · 4스테이지 도착";hud.defaultObjective="3스테이지 완료 · 다음 구간 준비 중";
            SetSpawns(new Vector3(0,.04f,-2),"FromStage03");
            var level=Object.FindFirstObjectByType<StageLevel>();if(level!=null)level.nextScenePath="";
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        private static Transform[] SetSpawns(Vector3 center,string entry)
        {
            var layout=Object.FindFirstObjectByType<PlayerSpawnLayout>();
            SetString(layout,"_entryId",entry);
            var so=new SerializedObject(layout);var points=so.FindProperty("spawnPoints");
            var result=new Transform[2];
            for(int i=0;i<2;i++){result[i]=(Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;result[i].position=center+Vector3.right*(i==0?-1.25f:1.25f);result[i].rotation=Quaternion.identity;}
            var player=(GameObject)so.FindProperty("offlinePlayer").objectReferenceValue;player.transform.position=result[0].position;player.transform.rotation=Quaternion.identity;
            return result;
        }
        private static StageArrivalZone Zone(Transform parent,string name,Vector3 position,Vector3 size)
        {
            var go=Point(parent,name,position).gameObject;
            var box=go.AddComponent<BoxCollider>();box.isTrigger=true;box.size=size;
            var zone=go.AddComponent<StageArrivalZone>();Set(zone,"_volume",box);return zone;
        }
        private static void Deck(Transform parent,string name,Vector3 top,Vector3 size) => Cube(parent,name,top-Vector3.up*size.y*.5f,size,_deck);
        private static GameObject Cube(Transform parent,string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;
            if(material!=null)go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }
        private static Transform Point(Transform parent,string name,Vector3 position){var go=new GameObject(name);go.transform.SetParent(parent);go.transform.position=position;return go.transform;}
        private static Material Mat(string name,Color color)
        {
            var path="Assets/_Project/Art/Materials/Environments/Stage_03/"+name+".mat";
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=color;AssetDatabase.CreateAsset(mat,path);return mat;
        }
        private static void EnsureFolder(string path){if(AssetDatabase.IsValidFolder(path))return;var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');EnsureFolder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
        public static void Set(Object obj,string field,Object value){var s=new SerializedObject(obj);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        public static void SetInt(Object obj,string field,int value){var s=new SerializedObject(obj);s.FindProperty(field).intValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        public static void SetBool(Object obj,string field,bool value){var s=new SerializedObject(obj);s.FindProperty(field).boolValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        public static void SetString(Object obj,string field,string value){var s=new SerializedObject(obj);s.FindProperty(field).stringValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        public static void SetArray(Object obj,string field,Object[] values){var s=new SerializedObject(obj);var a=s.FindProperty(field);a.arraySize=values.Length;for(int i=0;i<values.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=values[i];s.ApplyModifiedPropertiesWithoutUndo();}
    }
}