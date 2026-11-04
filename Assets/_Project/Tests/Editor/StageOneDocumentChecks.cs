using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class StageOneDocumentChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        private static T Read<T>(object target, string field) => (T)target.GetType().GetField(field, Private).GetValue(target);
        private static void Write(object target, string field, object value) => target.GetType().GetField(field, Private).SetValue(target, value);
        private static string Id(UnityEngine.Object obj) => obj != null ? GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString() : "null";
        public static void CaptureOriginalArt(string path)
        {
            var lines = new List<string>();
            foreach (var t in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
            {
                if (t.root.name == "Stage1_DocumentGimmicks") continue;
                var text = Id(t) + "|" + t.name + "|" + Id(t.parent) + "|" + t.localPosition.ToString("F7") + "|" + t.localRotation.ToString("F7") + "|" + t.localScale.ToString("F7") + "|" + t.gameObject.activeSelf;
                foreach (var r in t.GetComponents<Renderer>()) text += "|" + r.GetType().Name + ":" + r.enabled + ":" + string.Join(",", r.sharedMaterials.Select(Id));
                var mesh = t.GetComponent<MeshFilter>(); if (mesh != null) text += "|mesh=" + Id(mesh.sharedMesh);
                var skin = t.GetComponent<SkinnedMeshRenderer>(); if (skin != null) text += "|skin=" + Id(skin.sharedMesh);
                lines.Add(text);
            }
            System.IO.File.WriteAllLines(path, lines.OrderBy(x => x).ToArray());
        }
        public static string Authored()
        {
            Check(!Application.isPlaying, "Edit mode required.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Check(scene.path == StageOneDocumentAuthoring.ScenePath, "Imported scene required.");
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            foreach (var t in all) Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0, "Missing script " + t.name);
            var targets = all.Select(t => t.GetComponent<LeafInstallTarget>()).Where(t => t != null && t.isActiveAndEnabled).ToArray();
            Check(targets.All(t => t.Id > 0) && targets.Select(t => t.Id).Distinct().Count() == targets.Length, "Unique positive leaf IDs.");
            var cloths = all.Select(t => t.GetComponent<HangingCloth>()).Where(t => t != null && t.isActiveAndEnabled).ToArray();
            Check(cloths.Length == 4 && cloths.Count(c => c.WaterDriven) == 2, "Four cloths, two water-driven.");
            foreach (var c in cloths)
            {
                Check(Read<Transform>(c, "_externalVisualRoot") != null && Read<bool>(c, "_singleUse") && Read<int>(c, "_riderCapacity") == 1, "Single-use source cloth binding.");
                Check(Read<Transform>(c, "_knot") != null && Read<HangingClothSettings>(c, "_settings") != null, "Cloth settings and grip.");
            }
            var pads = all.Select(t => t.GetComponent<ExteriorJumpPad>()).Where(t => t != null && t.isActiveAndEnabled).ToArray();
            Check(pads.Length > 0, "Jump pads exist.");
            foreach (var p in pads)
                Check(Read<SapInjectionPort[]>(p,"_ports").All(x => x != null && x.enabled) &&
                    Read<LeafInstallTarget[]>(p,"_sockets").All(x => x != null && x.HasSocket && x.enabled) &&
                    Read<GameObject[]>(p,"_jets").All(x => x != null), "Complete jump pad " + p.name);
            var mechanisms = all.Select(t => t.GetComponent<StageOneDocumentMechanism>()).Where(t => t != null && t.isActiveAndEnabled).ToArray();
            Check(mechanisms.Count(m => m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms) == 2 &&
                mechanisms.Count(m => m.Kind == StageOneDocumentMechanismKind.EjectObstacle) == 1 &&
                mechanisms.Count(m => m.Kind == StageOneDocumentMechanismKind.LowerBranch) == 1 &&
                mechanisms.Count(m => m.Kind == StageOneDocumentMechanismKind.BlockJet) == 3, "Two growth ports, ejection, branch lowering and three mushroom-route jet plugs.");
            var level = all.Select(t => t.GetComponent<StageLevel>()).First(x => x != null);
            Check(level.nextScenePath.EndsWith("Stage_02_Mazes.unity") && !level.abilityRules.leaf.permanentInstallation && level.abilityRules.leaf.lifetime == 5, "Stage transition and leaf lifetime.");
            return "PASS authored: original-map bindings, " + pads.Length + " pads, 4 single-use cloths, 4 mechanism kinds, unique leaf IDs, no missing scripts, Stage 2 transition.";
        }
        public static string RunPlay()
        {
            Check(Application.isPlaying && StageOneFlow.Instance != null && StageOneFlow.Instance.Authority, "Offline imported Stage One Play required.");
            var actors = StageActor.All.Where(a => a.Available).OrderBy(a => a.Slot).ToArray(); Check(actors.Length == 2, "Two actors.");
            var positions = actors.Select(a => a.transform.position).ToArray();
            Check(actors[0].Abilities.UnlockSap() && actors[1].Abilities.UnlockLeaf(), "Grant the two test abilities through the authority hook.");
            LeafProjectile leaf = null;
            var mechanisms = UnityEngine.Object.FindObjectsByType<StageOneDocumentMechanism>(FindObjectsSortMode.None);
            try
            {
                var growth = mechanisms.First(m => m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms);
                var port = Read<SapInjectionPort>(growth,"_port"); port.Inject(1.1f); growth.Tick(1.1f); Check(growth.Progress > .99f, "Water grows mushrooms.");
                growth.Tick(1.5f); Check(growth.Progress > .4f && growth.Progress < .6f, "Mushrooms shrink gradually after water stops.");
                growth.Tick(2); Check(growth.Progress == 0, "Mushrooms restore.");
                var eject = mechanisms.First(m => m.Kind == StageOneDocumentMechanismKind.EjectObstacle);
                var gate = UnityEngine.Object.FindObjectsByType<StageOneTargetGate>(FindObjectsSortMode.None).First(g=>Read<StageOneDocumentMechanism>(g,"_mechanism")==eject); var gatedTarget = gate.GetComponent<LeafInstallTarget>();
                Check(!gatedTarget.CanInstall, "Bridge is initially locked.");
                Read<SapInjectionPort>(eject,"_port").Inject(2); eject.Tick(2); Check(!eject.Solved, "Obstacle cannot be bypassed before cloth use and arrival.");
                var cloth = Read<HangingCloth>(eject,"_requiredCloth"); Write(cloth,"_used",true);
                var area = Read<BoxCollider>(eject,"_activationArea");
                var sapActor = actors.First(a => a.Abilities.Kind == PlayerAbilityKind.Sap);
                sapActor.GetComponent<PlayerController>().Motor.Teleport(area.bounds.center); Physics.SyncTransforms();
                // Test the actual current stage role; offline ability switching remains separate from character identity.
                var unlocked = sapActor.Abilities.Unlocked;
                Check(unlocked, "Prototype actor must have Sap unlocked for mechanism check.");
                Read<SapInjectionPort>(eject,"_port").Inject(1.1f); eject.Tick(1.1f); Check(eject.Solved && gatedTarget.CanInstall, "Arrival injection ejects obstacle and unlocks bridge.");
                var settings = StageLevel.Instance.abilityRules.leaf;
                var zone = UnityEngine.Object.FindObjectsByType<StageOneLeafUse>(FindObjectsSortMode.None).First(z => z.GetComponent<StageOneTargetGate>() == null);
                var target = zone.GetComponent<LeafInstallTarget>();
                leaf = UnityEngine.Object.Instantiate(settings.offlinePrefab); leaf.Initialize(settings,actors[0].transform,target.SocketPosition,target.SocketPosition,null,true);
                Check(leaf.TryInstallAtSocket(target), "Install at allowed wall socket."); Physics.SyncTransforms();
                zone.Tick(.1f); Check(leaf.Installed, "No footstep means no early recall.");
                var surface = Read<Collider>(leaf,"_surface"); var cc = actors[0].GetComponent<CharacterController>();
                var feet = new Vector3(surface.bounds.center.x,surface.bounds.max.y+.02f,surface.bounds.center.z);
                var offset=cc.transform.TransformVector(cc.center)-Vector3.up*cc.height*Mathf.Abs(cc.transform.lossyScale.y)*.5f;
                actors[0].GetComponent<PlayerController>().Motor.Teleport(feet-offset); Physics.SyncTransforms();
                Check(leaf.Supports(cc,.35f), "Actual player capsule stands on leaf."); zone.Tick(.07f); Check(leaf.State==LeafState.Returning, "First step recalls leaf.");
                var lower = mechanisms.First(m=>m.Kind==StageOneDocumentMechanismKind.LowerBranch); var button=Read<LeafInstallTarget>(lower,"_leafTarget");
                Check(!button.CanInstall,"Branch button cannot skip the first cloth trip.");
                Write(Read<HangingCloth>(lower,"_requiredCloth"),"_used",true);
                UnityEngine.Object.DestroyImmediate(leaf.gameObject); leaf=UnityEngine.Object.Instantiate(settings.offlinePrefab);
                leaf.Initialize(settings,actors[0].transform,button.SocketPosition,button.SocketPosition,null,true); Check(leaf.TryInstallAtSocket(button),"Leaf hits branch button.");
                lower.Tick(2); Check(lower.Solved && lower.Progress==1,"Branch lowers and remains lowered.");
                leaf.BeginReturn(); lower.Tick(.1f); Check(lower.Solved,"Leaf recall does not raise branch again.");
                return "PASS Play: growth/recovery, gated obstacle prerequisites and arrival injection, first-footstep recall, leaf button and latched branch lowering.";
            }
            finally
            { if(leaf!=null) UnityEngine.Object.DestroyImmediate(leaf.gameObject); for(int i=0;i<actors.Length;i++){var motor=actors[i].GetComponent<PlayerController>().Motor;motor.Teleport(positions[i]);motor.ResetMotion();} }
        }
        public static string ClothAndJetPlay()
        {
            Check(Application.isPlaying && StageOneFlow.Instance.Authority,"Offline Play required.");
            var actors=StageActor.All.Where(a=>a.Available).OrderBy(a=>a.Slot).ToArray();
            var positions=actors.Select(a=>a.transform.position).ToArray();
            var cloths=UnityEngine.Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None).Where(c=>c.enabled).ToArray();
            LeafProjectile leaf=null;
            try
            {
                foreach(var c in cloths)
                {
                    Write(c,"_used",false);Write(c,"_retraction",0f);Write(c,"_angles",Vector2.zero);Write(c,"_velocity",Vector2.zero);
                    if(c.WaterDriven)
                    {
                        Check(c.PushWithWater(Vector3.right,.1f),"Water cloth accepts impact.");c.Tick(.02f);
                        Check(Read<Vector2>(c,"_angles").y>0,"Actual impact direction drives swing.");
                        Write(c,"_angles",Vector2.zero);Write(c,"_velocity",Vector2.zero);
                    }
                    else Check(!c.PushWithWater(Vector3.right,.1f),"Self-driven cloth does not accept water drive.");
                    var knot=Read<Transform>(c,"_knot");var cc=actors[0].GetComponent<CharacterController>();
                    actors[0].GetComponent<PlayerController>().Motor.Teleport(knot.position-cc.transform.TransformVector(cc.center));Physics.SyncTransforms();
                    Check(HangingCloth.TryInteract(actors[0],out var feedback),"Grab authored cloth: "+c.name+" "+feedback);
                    Check(Read<StageActor[]>(c,"_riders").Contains(actors[0])&&c.WasUsed,"First rider consumes cloth use.");
                    var partner=actors[1].GetComponent<PlayerController>();partner.Motor.Teleport(actors[0].transform.position+Vector3.right*.5f);
                    HangingCloth.TryInteract(actors[1],out _);Check(!Read<StageActor[]>(c,"_riders").Contains(actors[1]),"Only one rider can use each cloth.");HangingCloth.TryRelease(actors[1],out _);
                    Check(HangingCloth.TryRelease(actors[0],out _),"Release current rider.");c.Tick(2);
                    Check(Read<float>(c,"_retraction")==1,"Used cloth rolls up.");
                    HangingCloth.TryInteract(actors[0],out _);Check(!Read<StageActor[]>(c,"_riders").Contains(actors[0]),"Used cloth rejects boarding.");HangingCloth.TryRelease(actors[0],out _);
                }
                var jet=UnityEngine.Object.FindObjectsByType<StageOneDocumentMechanism>(FindObjectsSortMode.None).First(m=>m.Kind==StageOneDocumentMechanismKind.BlockJet);
                var target=Read<LeafInstallTarget>(jet,"_leafTarget");var stream=Read<GameObject>(jet,"_jet");
                Check(stream.activeSelf,"Jet begins open.");
                var settings=StageLevel.Instance.abilityRules.leaf;
                leaf=UnityEngine.Object.Instantiate(settings.offlinePrefab);leaf.Initialize(settings,actors[0].transform,target.SocketPosition,target.SocketPosition,null,true);
                Check(leaf.TryInstallAtSocket(target),"Install leaf plug.");jet.Tick(.02f);Check(!stream.activeSelf,"Leaf blocks water jet.");
                leaf.Tick(5.01f);jet.Tick(.02f);Check(stream.activeSelf&&leaf.State==LeafState.Returning,"Attachment expiry reopens jet.");
                return "PASS Play: four cloths accept one rider once, roll up, reject reuse; two water cloths respond to direction; leaf plug closes jet and timed recall reopens it.";
            }
            finally
            {
                foreach(var a in actors)HangingCloth.TryRelease(a,out _);
                if(leaf!=null)UnityEngine.Object.DestroyImmediate(leaf.gameObject);
                for(int i=0;i<actors.Length;i++){var m=actors[i].GetComponent<PlayerController>().Motor;m.Teleport(positions[i]);m.ResetMotion();}
            }
        }
        public static string HoseAndPadPlay()
        {
            Check(Application.isPlaying&&StageOneFlow.Instance.Authority,"Offline Play required.");
            var actor=StageActor.All.First(a=>a.Available);var player=actor.GetComponent<PlayerController>();var saved=actor.transform.position;
            var sap=StageLevel.Instance.abilityRules.sap;var leafSettings=StageLevel.Instance.abilityRules.leaf;
            var held=UnityEngine.Object.Instantiate(sap.offlinePrefab);LeafProjectile leaf=null;
            try
            {
                var growth=UnityEngine.Object.FindObjectsByType<StageOneDocumentMechanism>(FindObjectsSortMode.None).First(m=>m.Kind==StageOneDocumentMechanismKind.GrowingMushrooms);
                var port=Read<SapInjectionPort>(growth,"_port");
                var tree=GameObject.Find("Tree_playobject").GetComponentsInChildren<Collider>().First(c=>c.name=="seonag_tree");
                Vector3 outward=Vector3.ProjectOnPlane(port.transform.position-tree.bounds.center,Vector3.up).normalized;
                held.transform.position=port.transform.position+outward*8;var hose=new SapHoseSprayer();
                var aim=new Ray(held.transform.position,-outward);Physics.SyncTransforms();
                for(int i=0;i<90;i++){hose.Tick(held,aim,true,.02f,sap,()=>null);growth.Tick(.02f);Physics.SyncTransforms();}
                Check(growth.Progress>.8f,"Actual ballistic hose reaches the growth port.");
                growth.Tick(4);
                var plug=UnityEngine.Object.FindObjectsByType<StageOneDocumentMechanism>(FindObjectsSortMode.None).First(m=>m.Kind==StageOneDocumentMechanismKind.BlockJet);
                var plugTarget=Read<LeafInstallTarget>(plug,"_leafTarget");
                outward=Read<GameObject>(plug,"_jet").transform.forward;
                Check(StageOneLeafZone.Cast(new Ray(plugTarget.SocketPosition+outward*12,-outward),leafSettings.collisionRadius,15,leafSettings.hitMask,out var hit)&&hit.collider.GetComponentInParent<LeafInstallTarget>()==plugTarget,"Leaf query can reach plug through the flowing jet.");
                int count=0;
                foreach(var pad in UnityEngine.Object.FindObjectsByType<ExteriorJumpPad>(FindObjectsSortMode.None).Where(p=>p.enabled))
                {
                    var target=Read<LeafInstallTarget[]>(pad,"_sockets")[0];var portForPad=Read<SapInjectionPort[]>(pad,"_ports")[0];
                    leaf=UnityEngine.Object.Instantiate(leafSettings.offlinePrefab);leaf.Initialize(leafSettings,actor.transform,target.SocketPosition,target.SocketPosition,null,true);
                    Check(leaf.TryInstallAtSocket(target),"Install pad leaf: "+pad.name);Physics.SyncTransforms();
                    leaf.Tick(120f);Check(leaf.Installed,"Pad leaf has no time limit: "+pad.name);
                    var cc=actor.GetComponent<CharacterController>();var surface=Read<Collider>(leaf,"_surface");
                    var feet=new Vector3(surface.bounds.center.x,surface.bounds.max.y+.02f,surface.bounds.center.z);
                    player.Motor.Teleport(feet-cc.transform.TransformVector(cc.center)+Vector3.up*cc.height*Mathf.Abs(cc.transform.lossyScale.y)*.5f);Physics.SyncTransforms();
                    Check(leaf.Supports(cc,.64f),"Rider supported: "+pad.name);
                    portForPad.Inject(1.1f);pad.Tick(1.1f);Check(pad.ChargeFraction>=1,"Pad charged: "+pad.name);
                    pad.Tick(3.1f);Check(pad.PresentationFlight>0,"Pad erupts: "+pad.name);
                    Check(player.Motor.Velocity.y>0,"Rider launched: "+pad.name);
                    Check(leaf.State==LeafState.Returning&&!target.HasLeaf,"Used pad leaf recalled after rider launch: "+pad.name);
                    leaf.BeginReturn();UnityEngine.Object.DestroyImmediate(leaf.gameObject);leaf=null;player.Motor.ResetMotion();count++;
                }
                player.Motor.Teleport(saved);Physics.SyncTransforms();
                var emptyPad=UnityEngine.Object.FindObjectsByType<ExteriorJumpPad>(FindObjectsSortMode.None).First(p=>p.enabled);
                emptyPad.Tick(5f);var emptyTarget=Read<LeafInstallTarget[]>(emptyPad,"_sockets")[0];
                leaf=UnityEngine.Object.Instantiate(leafSettings.offlinePrefab);leaf.Initialize(leafSettings,actor.transform,emptyTarget.SocketPosition,emptyTarget.SocketPosition,null,true);
                Check(leaf.TryInstallAtSocket(emptyTarget),"Install leaf for empty eruption check.");
                Read<SapInjectionPort[]>(emptyPad,"_ports")[0].Inject(1.1f);emptyPad.Tick(1.1f);emptyPad.Tick(3.1f);
                Check(leaf.Installed,"Empty eruption must preserve the unused leaf.");
                return "PASS Play: actual hose and jet queries; all "+count+" pad leaves survive 120 seconds, launch actual riders and recall after use; empty eruption preserves leaf.";
            }
            finally{if(leaf!=null)UnityEngine.Object.DestroyImmediate(leaf.gameObject);UnityEngine.Object.DestroyImmediate(held.gameObject);player.Motor.Teleport(saved);player.Motor.ResetMotion();}
        }
    }
}
