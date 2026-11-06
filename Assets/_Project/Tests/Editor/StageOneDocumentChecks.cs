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
            Check(cloths.Length == 5 && cloths.Count(c => c.WaterDriven) == 2, "Five cloths, two water-driven.");
            foreach (var c in cloths)
            {
                Check(Read<Transform>(c, "_externalVisualRoot") != null && Read<bool>(c, "_singleUse") && Read<int>(c, "_riderCapacity") == 1, "Single-use source cloth binding.");
                Check(Read<Transform>(c, "_knot") != null && Read<HangingClothSettings>(c, "_settings") != null, "Cloth settings and grip.");
            }
            var loweredCloths = cloths.Where(c => Read<SapInjectionPort>(c, "_loweringPort") != null).ToArray();
            Check(loweredCloths.Select(c => c.name).OrderBy(n => n).SequenceEqual(new[] { "Coop_1_SingleUseCloth", "Coop_2_WaterCloth" }) &&
                loweredCloths.Select(c => Read<SapInjectionPort>(c, "_loweringPort")).Distinct().Count() == 2,
                "Only the two revised cloths use separate lowering inlets.");
            foreach (var number in new[] { 31, 32, 24, 25 })
                Check(targets.Any(t => t.name == $"LeafBranch_{number}" && t.HasSocket && t.GetComponent<StageOneLeafZone>() != null),
                    $"Branch {number} has an active leaf socket.");
            var pads = all.Select(t => t.GetComponent<ExteriorJumpPad>()).Where(t => t != null && t.isActiveAndEnabled).ToArray();
            Check(pads.Length > 0, "Jump pads exist.");
            foreach (var p in pads)
                Check(Read<SapInjectionPort[]>(p,"_ports").All(x => x != null && x.enabled) &&
                    Read<LeafInstallTarget[]>(p,"_sockets").All(x => x != null && x.HasSocket && x.enabled) &&
                    Read<GameObject[]>(p,"_jets").All(x => x != null), "Complete jump pad " + p.name);
            var mechanisms = all.Select(t => t.GetComponent<StageOneDocumentMechanism>()).Where(t => t != null && t.isActiveAndEnabled).ToArray();
            Check(mechanisms.Count(m => m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms) == 2 &&
                mechanisms.All(m => m.Kind != StageOneDocumentMechanismKind.EjectObstacle) &&
                mechanisms.Count(m => m.Kind == StageOneDocumentMechanismKind.LowerBranch) == 1 &&
                mechanisms.Count(m => m.Kind == StageOneDocumentMechanismKind.BlockJet) == 3, "Two growth ports, branch lowering and three mushroom-route jet plugs.");
            var traversal = all.Select(t => t.GetComponent<StageOneTraversal>()).First(t => t != null);
            var forestExit = Read<BoxCollider>(traversal, "_forestExitCheckpoint");
            var fork = Read<Transform>(traversal, "_forestExitRecoveryPoint");
            Check(forestExit != null && forestExit.isTrigger && fork != null && !forestExit.bounds.Contains(fork.position),
                "Forest exit activates a separate fork recovery point.");
            var checkpoints = Read<Collider[]>(traversal, "_checkpointSurfaces");
            var jetMushrooms = new[] { "04_Musroom (10)", "04_Musroom (11)", "04_Musroom (6)" };
            Check(!checkpoints.Any(c => c != null && jetMushrooms.Contains(c.name)),
                "Water-jet mushrooms cannot be saved as recovery points.");
            foreach (var growth in mechanisms.Where(m => m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms))
                Check(!checkpoints.Contains(Read<Transform[]>(growth, "_movingArt").Single().GetComponent<Collider>()),
                    "A growing mushroom cannot be saved as a recovery point.");
            var level = all.Select(t => t.GetComponent<StageLevel>()).First(x => x != null);
            Check(level.nextScenePath.EndsWith("Stage_02_Mazes.unity") && !level.abilityRules.leaf.permanentInstallation && level.abilityRules.leaf.lifetime == 5, "Stage transition and leaf lifetime.");
            return "PASS authored: original-map bindings, " + pads.Length + " pads, 5 single-use cloths, 3 active mechanism kinds, unique leaf IDs, no missing scripts, Stage 2 transition.";
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
                foreach (var growth in mechanisms.Where(m => m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms))
                {
                    var mushroom = Read<Transform[]>(growth, "_movingArt").Single();
                    var restScale = mushroom.localScale;
                    var restPosition = mushroom.position;
                    var growthOffset = Read<Vector3>(growth, "_growthWorldOffset");
                    var port = Read<SapInjectionPort>(growth, "_port");
                    port.Inject(1.1f); growth.Tick(1.1f);
                    Check(growth.Progress == 1 && Vector3.Distance(mushroom.localScale, Vector3.one * .5793636f) < .000001f,
                        "Water grows the mushroom to the authored local scale.");
                    Check(growthOffset.sqrMagnitude > 0 && Vector3.Distance(mushroom.position, restPosition + growthOffset) < .001f,
                        "The mushroom grows outward from the trunk.");
                    growth.Tick(4); Check(Mathf.Approximately(growth.Progress, .5f), "Mushroom is halfway restored after four seconds.");
                    growth.Tick(4); Check(growth.Progress == 0 && mushroom.localScale == restScale &&
                        Vector3.Distance(mushroom.position, restPosition) < .001f, "Mushroom restores its scale and position after eight seconds.");
                }
                var settings = StageLevel.Instance.abilityRules.leaf;
                var zones = UnityEngine.Object.FindObjectsByType<StageOneLeafUse>(FindObjectsSortMode.None);
                var zone = zones.First(z => z.GetComponent<StageOneTargetGate>() == null && !Read<bool>(z, "_recallAfterLeaving"));
                var target = zone.GetComponent<LeafInstallTarget>();
                leaf = UnityEngine.Object.Instantiate(settings.offlinePrefab); leaf.Initialize(settings,actors[0].transform,target.SocketPosition,target.SocketPosition,null,true);
                Check(leaf.TryInstallAtSocket(target), "Install at allowed wall socket."); Physics.SyncTransforms();
                zone.Tick(.1f); Check(leaf.Installed, "No footstep means no early recall.");
                var surface = Read<Collider>(leaf,"_surface"); var cc = actors[0].GetComponent<CharacterController>();
                var feet = new Vector3(surface.bounds.center.x,surface.bounds.max.y+.02f,surface.bounds.center.z);
                var offset=cc.transform.TransformVector(cc.center)-Vector3.up*cc.height*Mathf.Abs(cc.transform.lossyScale.y)*.5f;
                actors[0].GetComponent<PlayerController>().Motor.Teleport(feet-offset); Physics.SyncTransforms();
                Check(leaf.Supports(cc,.35f), "Actual player capsule stands on leaf."); zone.Tick(.07f); Check(leaf.State==LeafState.Returning, "First step recalls leaf.");
                UnityEngine.Object.DestroyImmediate(leaf.gameObject);
                zone = zones.Single(z => z.name == "LeafBranch_32");
                target = zone.GetComponent<LeafInstallTarget>();
                leaf = UnityEngine.Object.Instantiate(settings.offlinePrefab);
                leaf.Initialize(settings, actors[0].transform, target.SocketPosition, target.SocketPosition, null, true);
                Check(leaf.TryInstallAtSocket(target), "Install the cloth approach leaf."); Physics.SyncTransforms();
                surface = Read<Collider>(leaf, "_surface");
                feet = new Vector3(surface.bounds.center.x, surface.bounds.max.y + .02f, surface.bounds.center.z);
                var riderMotor = actors[0].GetComponent<PlayerController>().Motor;
                riderMotor.Teleport(feet - offset); Physics.SyncTransforms();
                Check(leaf.Supports(cc, .35f), "Player stands on the cloth approach leaf.");
                zone.Tick(.1f);
                leaf.Tick(settings.lifetime + .1f);
                Check(leaf.Installed, "Cloth approach leaf survives footstep and expiry while occupied.");
                riderMotor.Teleport(feet - offset + Vector3.up * 2); Physics.SyncTransforms();
                zone.Tick(.1f);
                leaf.Tick(.1f);
                Check(leaf.Installed, "A brief loss of support does not recall the cloth approach leaf.");
                zone.Tick(.1f);
                Check(leaf.State == LeafState.Returning, "Cloth approach leaf recalls after the player leaves.");
                var lower = mechanisms.First(m=>m.Kind==StageOneDocumentMechanismKind.LowerBranch); var button=Read<LeafInstallTarget>(lower,"_leafTarget");
                var firstCloth = Read<HangingCloth>(lower, "_requiredCloth");
                Check(firstCloth != null && !lower.Ready && !button.CanInstall,
                    "The branch waits for the first cloth to retract and cannot hold a leaf.");
                Write(firstCloth, "_retraction", 1f);
                Check(lower.Ready, "The branch accepts a hit once the first cloth has retracted.");
                var hitZone = button.GetComponent<SphereCollider>();
                var impactPoint = hitZone.bounds.center;
                var origins = new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up }
                    .Select(direction => impactPoint + direction * (hitZone.radius + settings.collisionRadius + 1));
                var origin = origins.First(point =>
                    StageOneLeafZone.Cast(new Ray(point, (impactPoint - point).normalized), settings.collisionRadius,
                        Vector3.Distance(point, impactPoint), settings.hitMask, out var hit) && hit.collider == hitZone);
                UnityEngine.Object.DestroyImmediate(leaf.gameObject);
                leaf = UnityEngine.Object.Instantiate(settings.offlinePrefab);
                leaf.Initialize(settings, actors[0].transform, origin, impactPoint, null, true);
                leaf.Tick(1 + Vector3.Distance(origin, impactPoint) / settings.flightSpeed);
                Check(lower.Solved && leaf.State == LeafState.Returning && !button.HasLeaf,
                    "A flying leaf hits the branch and bounces back without attaching.");
                var branch = Read<Transform[]>(lower, "_movingArt")[0];
                var restRotation = branch.rotation;
                lower.Tick(Read<float>(lower, "_moveSeconds"));
                Check(lower.Progress == 1 && Quaternion.Angle(branch.rotation, restRotation) > 1,
                    "The hit rotates the branch into its lowered pose.");
                lower.Tick(.1f);
                Check(lower.Solved && lower.Progress == 1, "The branch stays lowered after the leaf returns.");
                return "PASS Play: growth/recovery, first-footstep recall, leaf button and latched branch lowering.";
            }
            finally
            { if(leaf!=null) UnityEngine.Object.DestroyImmediate(leaf.gameObject); for(int i=0;i<actors.Length;i++){var motor=actors[i].GetComponent<PlayerController>().Motor;motor.Teleport(positions[i]);motor.ResetMotion();} }
        }
        public static string ClothAndJetPlay()
        {
            Check(Application.isPlaying && StageOneFlow.Instance.Authority,"Offline Play required.");
            var actors=StageActor.All.Where(a=>a.Available).OrderBy(a=>a.Slot).ToArray();
            var positions=actors.Select(a=>a.transform.position).ToArray();
            var cloths=UnityEngine.Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None).Where(c=>c.enabled)
                .OrderBy(c=>Read<StageOneDocumentMechanism>(c,"_deploymentMechanism") != null).ToArray();
            LeafProjectile leaf=null;
            try
            {
                foreach(var c in cloths)
                {
                    Write(c,"_used",false);Write(c,"_retraction",0f);Write(c,"_angles",Vector2.zero);Write(c,"_velocity",Vector2.zero);
                    var mechanism = Read<StageOneDocumentMechanism>(c, "_deploymentMechanism");
                    if (mechanism != null)
                    {
                        Check(mechanism.TryHitBranch(), "The first cloth retraction unlocks the replacement.");
                        mechanism.Tick(Read<float>(mechanism, "_moveSeconds"));
                        c.Tick(.02f);
                        Check(Vector3.Distance(Read<Transform>(c, "_knot").position,
                            Read<Transform>(c, "_deployedGrip").position) < .01f, "The replacement reaches the original boarding point.");
                    }
                    var loweringPort = Read<SapInjectionPort>(c, "_loweringPort");
                    if (loweringPort != null)
                    {
                        var grip = Read<Transform>(c, "_knot");
                        var raised = grip.position.y;
                        var raisedMeters = Read<float>(c, "_raisedMeters");
                        Check(!c.PushWithWater(Vector3.right, .1f), "Raised cloth waits for its inlet.");
                        loweringPort.Inject(.02f);
                        c.Tick(.25f);
                        Check(grip.position.y < raised && grip.position.y > raised - raisedMeters, "An injection starts lowering the cloth.");
                        c.Tick(1f);
                        var lowered = grip.position.y;
                        c.Tick(1f);
                        Check(Mathf.Abs((raised - lowered) - raisedMeters) < .05f && Mathf.Approximately(grip.position.y, lowered),
                            "The cloth finishes lowering and stays down without continued injection.");
                    }
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
                return "PASS Play: five cloths accept one rider once, roll up and reject reuse; the replacement reaches the first boarding point; leaf plug closes and reopens the jet.";
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
