using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Herbalist.StageOne;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.Abilities;
using Fusion;
namespace Herbalist.Editor
{
    public static class StageLevelLayout
    {
        public const string One="Assets/_Project/Scenes/Stages/Stage_01/Stage_01_Exterior.unity";
        public const string Two="Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Interior.unity";
        private static Material ground,stone;
        public static string Build(bool resumeUnmodifiedCopy = false)
        {
            if(Application.isPlaying)throw new Exception("Stop play first");
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save scene edits first");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Two)!=null && !resumeUnmodifiedCopy)throw new Exception("Stage Two exists. Do not regenerate over edits.");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Two)==null)AssetDatabase.CopyAsset(One,Two);
            ground=Material("ForestGround",new Color(.13f,.19f,.13f));stone=Material("PathStone",new Color(.3f,.32f,.27f));
            var scene=EditorSceneManager.OpenScene(One,OpenSceneMode.Single);
            var tree=GameObject.Find("Sacred_Tree_1.0"); AddMeshCollider(tree,false);
            foreach(var t in Roots())t.gameObject.SetActive(false);
            DisableTests();
            var terrain=new GameObject("StageOne_ExteriorLayout");
            Cube("ExteriorGround",terrain.transform,new Vector3(0,-.5f,0),new Vector3(160,1,160),ground);
            var level=terrain.AddComponent<StageLevel>();level.nextScenePath=Two;
            SetSpawns(new Vector3(-44,.03f,-40),Quaternion.Euler(0,0,0));
            var flow=UnityEngine.Object.FindAnyObjectByType<StageOneFlow>();
            var temporary=flow.entranceBlocker.transform.parent.gameObject; UnityEngine.Object.DestroyImmediate(temporary);
            var entry=new GameObject("SacredTree_Entrance");entry.transform.SetParent(tree.transform,true);entry.transform.position=new Vector3(1.3f,0,-13);
            // World-space child avoids inheriting the imported model's large scale.
            entry.transform.localScale=new Vector3(1/tree.transform.lossyScale.x,1/tree.transform.lossyScale.y,1/tree.transform.lossyScale.z);entry.transform.rotation=Quaternion.identity;
            flow.entranceBlocker=Cube("RootSeal_OpensAfterBothPotions",entry.transform,new Vector3(1.3f,2.8f,-14),new Vector3(12,5.6f,.6f),stone);
            var volume=new GameObject("StageTwo_EntryVolume");volume.transform.SetParent(entry.transform,true);volume.transform.position=new Vector3(1.3f,2,-9);
            flow.interior=volume.AddComponent<BoxCollider>();flow.interior.isTrigger=true;flow.interior.size=new Vector3(10,4,6);
            var exit=flow.gameObject.AddComponent<StageExit>();Set(exit,"level",level);
            Physics.SyncTransforms();
            var herbs=new[]{new Vector3(-41,0,20),new Vector3(40,0,-22)};
            for(int i=0;i<2;i++)
            {
                float y=Floor(herbs[i]);flow.sources[i].transform.position=herbs[i]+Vector3.up*(y+.65f);
                Cube("HerbRock_"+i,terrain.transform,herbs[i]+Vector3.up*(y+.2f),new Vector3(2.4f,.4f,2),stone);
                Cube("ApproachStone_"+i,terrain.transform,herbs[i]+new Vector3(-2,y+.15f,-1),new Vector3(1.6f,.3f,1.5f),stone);
            }
            Vector3[] path={new Vector3(-44,0,-40),new Vector3(-48,0,-20),new Vector3(-52.5f,0,20),herbs[0],new Vector3(-45,0,38),new Vector3(-25,0,48),new Vector3(10,0,50),new Vector3(42,0,34),new Vector3(49,0,5),herbs[1],new Vector3(30,0,-38),new Vector3(30,0,-50),new Vector3(1.3f,0,-50),new Vector3(1.3f,0,-20),new Vector3(1.3f,0,-9)};
            Guide(terrain.transform,"ExpectedExteriorRoute",path.Select(p=>new Vector3(p.x,Floor(p)+.1f,p.z)).ToArray());
            // Low stones define the intended exploration space without prescribing herb order.
            for(int i=0;i<path.Length-2;i++)
            {
                var p=path[i]+new Vector3(-3,0,2);p.y=Floor(p)+.3f;
                var rock=Cube("RouteBoundaryStone_"+i,terrain.transform,p,new Vector3(1.8f,.6f,1.3f),stone);rock.transform.rotation=Quaternion.Euler(0,i*37,0);
            }
            var herbRock=GameObject.Find("HerbRock_0");herbRock.transform.position=herbs[0]+Vector3.up*1.8f;herbRock.transform.localScale=new Vector3(2.4f,3.6f,2.4f);flow.sources[0].transform.position=herbs[0]+Vector3.up*4.25f;
            for(int i=0;i<4;i++){float h=.8f*(i+1);Cube("GatheringJumpRock_"+i,terrain.transform,herbs[0]+new Vector3(-8.8f+i*2.2f,h*.5f,0),new Vector3(2,h,2.4f),stone);}
            var exteriorGuide=GameObject.Find("ExpectedExteriorRoute").GetComponent<LevelRouteGuide>();
            exteriorGuide.expectedRoute[3].position=new Vector3(-41,3.7f,20);exteriorGuide.expectedRoute[9].position=new Vector3(40,.45f,-22);
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            scene=EditorSceneManager.OpenScene(Two,OpenSceneMode.Single);
            var roots=Roots().OrderBy(t=>t.name).ToArray();
            var sacred=GameObject.Find("Sacred_Tree_1.0");sacred.SetActive(false);
            DisableTests();var stage=GameObject.Find("StageOne");UnityEngine.Object.DestroyImmediate(stage);
            var insideRoot=new GameObject("StageTwo_InteriorLayout");insideRoot.AddComponent<StageLevel>();
            Vector3 center=new Vector3(48,0,-58);
            Cube("CatchFloor_NoRespawn",insideRoot.transform,center+Vector3.down*.5f,new Vector3(40,1,40),ground);
            for(int i=0;i<roots.Length;i++)
            {
                var t=roots[i];var r=t.GetComponent<Renderer>();float maxExtent=Mathf.Max(r.bounds.extents.x,r.bounds.extents.z);
                t.localScale*=4.9f/maxExtent;
                float a=i*Mathf.PI/3;var target=center+new Vector3(Mathf.Cos(a)*8.2f,0,Mathf.Sin(a)*8.2f);
                r=t.GetComponent<Renderer>();t.position+=new Vector3(target.x-r.bounds.center.x,-r.bounds.min.y,target.z-r.bounds.center.z);
                AddMeshCollider(t.gameObject,true);
                var leaf=t.gameObject.AddComponent<LeafInstallTarget>();SetInt(leaf,"targetId",201+i);SetBool(leaf,"acceptsPin",true);SetBool(leaf,"acceptsPlatform",true);
                var receiver=t.gameObject.AddComponent<SapReceiver>();SetInt(receiver,"receiverId",201+i);Set(receiver,"leafTarget",leaf);
                var source=t.gameObject.AddComponent<SapSource>();Set(source,"interactionVolume",t.GetComponent<MeshCollider>());
                t.SetParent(insideRoot.transform,true);
            }
            SetSpawns(center+new Vector3(-.8f,.03f,0),Quaternion.Euler(0,90,0));
            FinishInterior();
            return RefineInteriorRoute();
        }
        public static string FinishInterior()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var insideRoot=GameObject.Find("StageTwo_InteriorLayout");
            if(insideRoot==null||scene.path!=Two)throw new Exception("Interior layout is not open");
            stone=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Environments/Stage_02/MAT_PathStone.mat");
            var center=new Vector3(48,0,-58);Physics.SyncTransforms();var route=new List<Vector3>();route.Add(center);
            for(int i=0;i<17;i++)
            {
                float y=.8f+i*.75f;float baseAngle=i*13.75f;
                bool found=false;Vector3 point=Vector3.zero;
                for(int offset=0;offset<=18&&!found;offset++)for(int sign=-1;sign<=1;sign+=2)
                {
                    float a=(baseAngle+sign*offset*2.5f)*Mathf.Deg2Rad;Vector3 direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));RaycastHit hit;
                    if(!Physics.Raycast(center+Vector3.up*y,direction,out hit,25,1,QueryTriggerInteraction.Ignore)||hit.collider.GetComponent<LeafInstallTarget>()==null)continue;
                    point=hit.point-direction*1.8f;found=true;break;
                }
                if(!found)throw new Exception("No reachable tree surface at height "+y);
                route.Add(point);
            }
            Vector3 finish=route[route.Count-1]+Vector3.up*.75f;
            Cube("UpperArrivalBranch",insideRoot.transform,finish-Vector3.up*.2f,new Vector3(5,.4f,4),stone);
            var goalGO=new GameObject("StageTwoGoal",typeof(NetworkObject),typeof(StageTwoGoal));goalGO.transform.SetParent(insideRoot.transform);
            var goal=goalGO.GetComponent<StageTwoGoal>();var zone=new GameObject("BothPlayersArrival");zone.transform.SetParent(goalGO.transform);zone.transform.position=finish+Vector3.up*1.5f;goal.arrival=zone.AddComponent<BoxCollider>();goal.arrival.isTrigger=true;goal.arrival.size=new Vector3(5,3,4);
            route.Add(finish);Guide(insideRoot.transform,"SuggestedLeafRoute_NoAuthoredPlatforms",route.ToArray());Hud(goal,insideRoot.transform);
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==Two))scenes.Add(new EditorBuildSettingsScene(Two,true));EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            float maxGap=0;for(int i=2;i<route.Count;i++){var d=route[i]-route[i-1];d.y=0;maxGap=Mathf.Max(maxGap,d.magnitude);}
            return "Interior complete: route samples="+route.Count+", max leaf-center gap="+maxGap+", arrival="+finish;
        }
        public static string RefineInteriorRoute()
        {
            var center=new Vector3(48,0,-58);Physics.SyncTransforms();
            var guide=GameObject.Find("SuggestedLeafRoute_NoAuthoredPlatforms").GetComponent<LevelRouteGuide>();
            var previous=center;float maxGap=0;float lastAngle=0;
            for(int i=0;i<17;i++)
            {
                float y=.8f+i*.75f;float desired=i*13.75f;float score=float.MaxValue;Vector3 best=Vector3.zero;float bestAngle=0;
                for(int a=0;a<360;a+=2)
                {
                    if(i>0&&(a<lastAngle-6||a>lastAngle+30))continue;
                    var direction=new Vector3(Mathf.Cos(a*Mathf.Deg2Rad),0,Mathf.Sin(a*Mathf.Deg2Rad));RaycastHit hit;
                    if(!Physics.Raycast(center+Vector3.up*y,direction,out hit,18,1,QueryTriggerInteraction.Ignore)||hit.collider.GetComponent<LeafInstallTarget>()==null)continue;
                    var point=hit.point-direction*1.8f;var d=point-previous;d.y=0;
                    if(i>0&&d.magnitude>3.5f)continue;
                    if(i>0&&Physics.Linecast(previous+Vector3.up,point+Vector3.up,1,QueryTriggerInteraction.Ignore))continue;
                    float cost=d.sqrMagnitude+Mathf.Pow(Mathf.DeltaAngle(a,desired),2)*.02f;
                    if(cost<score){score=cost;best=point;bestAngle=a;}
                }
                if(score==float.MaxValue)throw new Exception("No safe adjacent leaf placement at step "+i);
                if(i>0){var d=best-previous;d.y=0;maxGap=Mathf.Max(maxGap,d.magnitude);}
                guide.expectedRoute[i+1].position=best;previous=best;lastAngle=bestAngle;
            }
            var finish=previous+Vector3.ProjectOnPlane(center-previous,Vector3.up).normalized*4+Vector3.up*.75f;guide.expectedRoute[18].position=finish;
            GameObject.Find("UpperArrivalBranch").transform.position=finish-Vector3.up*.2f;
            GameObject.Find("BothPlayersArrival").transform.position=finish+Vector3.up*1.5f;
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Route refined: max center gap="+maxGap+", final angle="+lastAngle+", goal="+finish;
        }
        private static Transform[] Roots()=>UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Where(t=>t.name.StartsWith("Root_Big")&&t.GetComponent<MeshFilter>()!=null).ToArray();
        private static void DisableTests(){foreach(var name in new[]{"MovementTestEnvironment","LeafAbilityTestTargets","SapAbilityTestObjects"}){var go=GameObject.Find(name);if(go!=null)go.SetActive(false);}}
        private static void AddMeshCollider(GameObject go,bool readable)
        {
            var mesh=go.GetComponent<MeshFilter>().sharedMesh;
            if(readable&&!mesh.isReadable){var path=AssetDatabase.GetAssetPath(mesh);var importer=AssetImporter.GetAtPath(path) as ModelImporter;if(importer!=null){importer.isReadable=true;importer.SaveAndReimport();mesh=go.GetComponent<MeshFilter>().sharedMesh;}}
            var c=go.GetComponent<MeshCollider>();if(c==null)c=go.AddComponent<MeshCollider>();c.sharedMesh=mesh;c.convex=false;
        }
        private static float Floor(Vector3 p){RaycastHit h;return Physics.Raycast(p+Vector3.up*150,Vector3.down,out h,200,1,QueryTriggerInteraction.Ignore)?h.point.y:0;}
        private static Material Material(string name,Color color){var path="Assets/_Project/Art/Materials/Environments/Stage_02/MAT_"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m!=null)return m;m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;AssetDatabase.CreateAsset(m,path);return m;}
        private static GameObject Cube(string name,Transform parent,Vector3 pos,Vector3 scale,Material mat){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,true);go.transform.position=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;return go;}
        private static void Set(UnityEngine.Object obj,string field,UnityEngine.Object value){var s=new SerializedObject(obj);s.FindProperty(field).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private static void SetInt(UnityEngine.Object obj,string field,int value){var s=new SerializedObject(obj);s.FindProperty(field).intValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private static void SetBool(UnityEngine.Object obj,string field,bool value){var s=new SerializedObject(obj);s.FindProperty(field).boolValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private static void SetSpawns(Vector3 start,Quaternion rotation)
        {
            var layout=UnityEngine.Object.FindAnyObjectByType<PlayerSpawnLayout>();var s=new SerializedObject(layout);var points=s.FindProperty("spawnPoints");
            for(int i=0;i<points.arraySize;i++){var t=(Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;t.SetPositionAndRotation(start+Vector3.right*(i*1.6f),rotation);}
            var offline=(GameObject)s.FindProperty("offlinePlayer").objectReferenceValue;offline.transform.SetPositionAndRotation(start,rotation);
        }
        private static void Guide(Transform parent,string name,Vector3[] points){var go=new GameObject(name);go.transform.SetParent(parent);var g=go.AddComponent<LevelRouteGuide>();g.expectedRoute=new Transform[points.Length];for(int i=0;i<points.Length;i++){var t=new GameObject("Route_"+i.ToString("00")).transform;t.SetParent(go.transform);t.position=points[i];g.expectedRoute[i]=t;}}
        private static void Hud(StageTwoGoal goal,Transform parent)
        {
            var go=new GameObject("StageTwoHUD",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));go.transform.SetParent(parent);go.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;go.GetComponent<Canvas>().sortingOrder=20;
            var scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var textGo=new GameObject("Objective",typeof(RectTransform),typeof(TMPro.TextMeshProUGUI));textGo.transform.SetParent(go.transform,false);var rect=(RectTransform)textGo.transform;rect.anchorMin=new Vector2(.1f,1);rect.anchorMax=new Vector2(.9f,1);rect.pivot=new Vector2(.5f,1);rect.anchoredPosition=new Vector2(0,-60);rect.sizeDelta=new Vector2(0,70);
            var text=textGo.GetComponent<TMPro.TextMeshProUGUI>();text.font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");text.fontSize=30;text.alignment=TMPro.TextAlignmentOptions.Center;text.raycastTarget=false;text.text=goal.objectiveText;goal.objective=text;
        }
    }
}
