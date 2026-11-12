using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Explicit editor validation: real authored scene and runtime ability objects, no shipping test hooks.
    public static class BranchRideChecks
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static void Check(bool value,string message) { if(!value)throw new Exception("BranchRide: "+message); }
        private static T Get<T>(object target,string field)=>(T)target.GetType().GetField(field,Private).GetValue(target);
        private static void Set(object target,string field,object value)=>target.GetType().GetField(field,Private).SetValue(target,value);
        private static void Reset(BranchRideFlow flow)=>flow.GetType().GetMethod("ResetRide",Private).Invoke(flow,new object[]{false});
        private static List<BranchRideTrack> Walk(BranchRideTrack start)
        {
            var path=new List<BranchRideTrack>();
            var current=start;
            while(current!=null)
            {
                Check(!path.Contains(current),"route graph has no loop");
                path.Add(current);
                current=current.Exits.FirstOrDefault(next=>next!=null&&Vector3.Distance(current.End,next.Start)<.01f);
            }
            return path;
        }
        private static void Write(string name,string text)
        {System.IO.Directory.CreateDirectory("Temp/BranchRideVerification");System.IO.File.WriteAllText("Temp/BranchRideVerification/"+name+".txt",text);Debug.Log(text);}
        [MenuItem("Herbalist/Stages/Validate Stage Three Blockout")]
        public static void AuthoredMenu()=>Write("authored",Authored());
        public static string Authored()
        {
            Check(!Application.isPlaying,"edit mode required");
            var flow=Object.FindFirstObjectByType<BranchRideFlow>();Check(flow!=null,"open BranchRide scene");
            var tracks=Get<BranchRideTrack[]>(flow,"_track");var switches=Get<BranchRideSwitch[]>(flow,"_switches");
            Check(tracks.All(t=>t!=null&&t.Valid),"all spans have valid nodes");
            Check(tracks.Count(t=>t.IsDrop)==2,"exactly two intentional drops");
            Check(Get<BranchRideCarrier>(flow,"_carrier")!=null,"separate carrier backend");
            var surfaces=Get<LeafInstallTarget[]>(flow,"_leafSurfaces");
            Check(surfaces.Length>0&&surfaces.All(t=>!t.HasSocket),"free branch-surface leaf installation");
            Check(surfaces.Select(s=>s.Id).Distinct().Count()==surfaces.Length,"unique leaf target IDs");
            var tongues=tracks.Where(t=>t.SwitchIndex>=0).ToArray();
            bool planned=flow.transform.Find("Stage03_Blockout/PlannedBranchRoute")!=null;
            if(planned)
            {
                Check(switches.Length==4&&switches.All(s=>s.BranchCount==2),"four targets rotate two branches each");
                Check(tongues.Length==8&&tongues.All(t=>t.Exits.Length==1),"each rotating branch leads to its own fixed route");
                Check(surfaces.Length==9,"one fixed collision surface and eight rotating surfaces");
                Check(!flow.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="CommonHeel"),"no placeholder heel geometry");
            }
            else
            {
                Check(switches.Select(s=>s.BranchCount).SequenceEqual(new[]{2,3,3,2}),"blockout target groups 2/3/3/2");
                Check(tongues.Length==10&&tongues.All(t=>t.Exits.Length==2),"ten blockout turnouts");
            }
            var pivots=switches.SelectMany(t=>Get<Transform[]>(t,"_pivots")).ToArray();
            var authoredRotations=pivots.Select(t=>t.localRotation).ToArray();
            try
            {
                if(planned)
                {
                    for(int i=0;i<switches.Length;i++)
                    {
                        for(int j=0;j<switches.Length;j++)switches[j].Present(i==j?0:1);
                        Check(Walk(tracks[0]).Last().name==$"T{i+1}_DeadEnd",$"target {i+1} selects its alternate route");
                        var before=pivots.Skip(i*2).Take(2).Select(t=>t.localRotation).ToArray();
                        switches[i].Present(1);
                        Check(pivots.Skip(i*2).Take(2).Select((t,j)=>Quaternion.Angle(t.localRotation,before[j])>5).All(x=>x),
                            $"target {i+1} rotates both branches");
                    }
                    Check(tongues.All(t=>Vector3.Distance(t.End,t.Exits[0].Start)<.01f),"all outgoing branch ends stay attached");
                }
                else
                {
                    var heels=tongues.Select(t=>t.Start).ToArray();
                    foreach(var toggle in switches)toggle.Present(0);
                    Check(tongues.Select((t,i)=>Vector3.Distance(t.Start,heels[i])<.001f &&
                        Vector3.Distance(t.End,t.Exits[0].Start)<.01f).All(x=>x),"blockout A connections");
                    Check(Walk(tracks[0]).Last().name=="RouteA_DeadEnd","blockout A route ends at the dead end");
                    foreach(var toggle in switches)toggle.Present(1);
                    Check(tongues.Select((t,i)=>Vector3.Distance(t.Start,heels[i])<.001f &&
                        Vector3.Distance(t.End,t.Exits[1].Start)<.01f).All(x=>x),"blockout B connections");
                }
                foreach(var toggle in switches)toggle.Present(1);
                var bWalk=Walk(tracks[0]);
                Check(bWalk.Last().IsFinish&&bWalk.Count(t=>t.IsDrop)==2,"B graph reaches finish through two drops");
                Physics.SyncTransforms();
                var carrier=Get<BranchRideCarrier>(flow,"_carrier");
                var surfaceColliders=surfaces.SelectMany(s=>s.GetComponentsInChildren<Collider>()).ToHashSet();
                foreach(var drop in tracks.Where(t=>t.IsDrop))
                {
                    Check(drop.Start.y>drop.End.y+1,"drop descends");
                    float distance=0;
                    while(true)
                    {
                        var step=carrier.Step(drop,distance,Quaternion.identity,.02f,0,
                            Get<float>(flow,"_dropGravity"),Get<float>(flow,"_carrierHeight"),0,
                            Array.Empty<LeafProjectile>(),Vector3.zero,Vector3.zero);
                        Check(!Physics.OverlapSphere(step.position,.12f,~0,QueryTriggerInteraction.Ignore)
                            .Any(surfaceColliders.Contains),"drop arc clears branch colliders");
                        if(step.atEnd)break;
                        distance=step.distance;
                    }
                }
            }
            finally {for(int i=0;i<pivots.Length;i++)pivots[i].localRotation=authoredRotations[i];}
            Check(Get<Transform[]>(flow,"_respawns").Length==2&&Get<Transform[]>(flow,"_dismounts").Length==2,"paired spawn and dismount poses");
            Check(Get<GameObject>(flow,"_endingPanel")!=null,"authored completion UGUI");
            foreach(var t in flow.transform.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"missing script "+t.name);
            var hazards=Get<BranchRideStone[]>(flow,"_hazards");Check(hazards.Length==(planned?7:2),"authored rolling hazard count");
            foreach(var hazard in hazards)
            {
                Check(hazard.RollLength>4,"stone has rolling path");
                bool active=false,hidden=false;
                for(float t=0;t<hazard.CycleDuration;t+=.05f)
                {
                    if(hazard.Sample(t,out _))active=true;
                    else hidden=true;
                }
                Check(active&&hidden,"stone has an active path and a respawn interval");
            }
            return $"PASS authored: {tracks.Length} spans, {tongues.Length} rotating branches, connected B route, two clear drop arcs, {hazards.Length} rolling stones, leaf surfaces, paired spawn/dismount and completion UI.";
        }
        [MenuItem("Herbalist/Stages/Run Stage Three Gameplay Checks")]
        public static void RuntimeMenu()
        {
            try {Write("runtime",Runtime());}
            catch(Exception error){Write("runtime","FAIL "+error);throw;}
        }
        public static string Runtime()
        {
            Check(Application.isPlaying,"Play mode required");
            var flow=Object.FindFirstObjectByType<BranchRideFlow>();Check(flow!=null,"flow exists");
            var actors=StageActor.All.Where(a=>a.Available).OrderBy(a=>a.Slot).ToArray();Check(actors.Length==2,"two real offline actors");
            var hazards=Get<BranchRideStone[]>(flow,"_hazards");var switches=Get<BranchRideSwitch[]>(flow,"_switches");
            var respawns=Get<Transform[]>(flow,"_respawns");var dismounts=Get<Transform[]>(flow,"_dismounts");
            var results=new List<string>();
            void Board()
            {
                for(int i=0;i<2;i++)actors[i].GetComponent<PlayerController>().Motor.Teleport(respawns[i].position);
                Check(BranchRideFlow.TryBoard(actors[0],out _),"first rider boards");
                Check(BranchRideFlow.TryBoard(actors[1],out _),"second rider boards");
                Check(flow.CurrentPhase==BranchRidePhase.Boarding,"paired riders wait for boarding animations");
                var before=flow.Position;
                float delay=Get<float[]>(flow,"_boardingRemaining").Max();
                float elapsed = 0f;
                foreach (var actor in actors.OrderBy(a => a.GetComponent<SodamAnimationPresentation>().Profile.Find(SodamPose.TalismanBoard).ContactTime))
                {
                    float contact = actor.GetComponent<SodamAnimationPresentation>().Profile.Find(SodamPose.TalismanBoard).ContactTime;
                    flow.Tick(contact - elapsed - .01f);
                    Check(!BranchRideFlow.IsRiding(actor) && Vector3.Distance(actor.transform.position, respawns[actor.Slot].position) < .001f,
                        "Boarding starts on the approach, without transporting either role before its foot contact.");
                    flow.Tick(.02f);
                    elapsed = contact + .01f;
                    Check(BranchRideFlow.IsRiding(actor) && BranchRideFlow.TryGetBoardingSeat(actor, out var seat, out _) &&
                        Vector3.Distance(actor.transform.position, seat) < .001f, "The corresponding role mounts on its contact cue.");
                }
                flow.Tick(Mathf.Max(0,delay-elapsed-.01f));
                Check(flow.CurrentPhase==BranchRidePhase.Boarding&&Vector3.Distance(before,flow.Position)<.001f,"no departure before the last boarding animation finishes");
                flow.Tick(.03f);
                Check(flow.CurrentPhase==BranchRidePhase.Riding,"departure follows boarding animations");
            }
            void OpenAll()
            {foreach(var toggle in switches)toggle.Port.Inject(.3f);flow.Tick(.8f);}
            LeafProjectile Install(LeafInstallTarget target,Vector3 point)
            {
                var owner=actors[1].Abilities;var leaf=Object.Instantiate(owner.Leaf.Settings.offlinePrefab);
                leaf.Initialize(owner.Leaf.Settings,owner.transform,point,point,_=>Object.Destroy(leaf.gameObject),true);
                typeof(LeafProjectile).GetMethod("Install",Private).Invoke(leaf,new object[]{target,point,Vector3.up,Vector3.right});return leaf;
            }
            Ray AimFromRide(StageActor actor, Vector3 point)
            {
                var rotation = Get<Quaternion>(flow, "_carrierRotation");
                var offsets = Get<Vector3[]>(flow, "_riderOffsets");
                foreach (var rider in actors)
                    rider.GetComponent<PlayerController>().Motor.Teleport(flow.Position + rotation * offsets[rider.Slot]);
                Physics.SyncTransforms();
                var view = actor.GetComponent<PlayerController>().View;
                var pivot = Get<Transform>(view, "cameraTarget");
                Vector3 angles = Quaternion.LookRotation(point - pivot.position).eulerAngles;
                return view.GetAimRay(angles.y, Mathf.DeltaAngle(0f, angles.x));
            }
            try
            {
                Reset(flow);
                for(int i=0;i<2;i++)actors[i].GetComponent<PlayerController>().Motor.Teleport(respawns[i].position);
                int priorFailures=flow.FailureCount;
                actors[0].GetComponent<PlayerController>().Motor.Teleport(respawns[0].position+Vector3.down*(Get<float>(flow,"_unmountedFallMargin")+1));
                flow.Tick(.02f);
                Check(Vector3.Distance(actors[0].transform.position,respawns[0].position)<.1f,"unmounted fall restores the fallen actor");
                Check(Vector3.Distance(actors[1].transform.position,respawns[1].position)<.1f&&flow.FailureCount==priorFailures,"unmounted fall preserves the partner and ride state");
                results.Add("unmounted fall recovery without resetting partner");
                Reset(flow);
                actors[0].GetComponent<PlayerController>().Motor.Teleport(respawns[0].position);
                Check(BranchRideFlow.TryBoard(actors[0],out _),"first rider accepted");var before=flow.Position;flow.Tick(1);
                Check(flow.CurrentPhase==BranchRidePhase.Boarding&&Vector3.Distance(before,flow.Position)<.001f,"one rider cannot depart");
                Check(BranchRideFlow.IsRiding(actors[0])&&!BranchRideFlow.IsRiding(actors[1]),"rider ownership");
                results.Add("single-rider wait and rider ownership");
                Reset(flow);Set(flow,"_hazards",Array.Empty<BranchRideStone>());Board();int initialFailures=flow.FailureCount;
                bool traversedA=false;
                for(int n=0;n<1800&&flow.FailureCount==initialFailures;n++)
                {
                    var span=flow.Track[flow.CurrentSegment];
                    traversedA|=span.name.EndsWith("_DeadEnd",StringComparison.Ordinal);
                    flow.Tick(.02f);
                }
                Check(traversedA&&flow.FailureCount==initialFailures+1&&flow.CurrentPhase==BranchRidePhase.Boarding,"A branch is traversed before its dead end falls and resets");
                Check(actors.All(a=>!BranchRideFlow.IsRiding(a)&&!a.GetComponent<PlayerController>().Motor.MovementLocked),"reset releases both riders");
                Check(actors.Select((a,i)=>Vector3.Distance(a.transform.position,respawns[i].position)<.1f).All(x=>x),"reset respawn positions");
                results.Add("A branch traversal, disconnected tip fall, full restart and transport release");
                Reset(flow);Board();var first=switches[0];first.Port.Inject(.3f);flow.Tick(.02f);Check((flow.PresentationBranchMask&1)!=0,"water activates T1");
                for(int n=0;n<10;n++){first.Port.Inject(.05f);flow.Tick(.05f);}Check((flow.PresentationBranchMask&1)!=0,"held spray does not toggle repeatedly");
                flow.Tick(.5f);first.Port.Inject(.3f);flow.Tick(.02f);Check((flow.PresentationBranchMask&1)==0,"release and re-hit toggles again");
                results.Add("authoritative injection, one activation per burst and re-arm");
                // Keep the throw position fixed when the configured carrier speed changes.
                Reset(flow);Board();
                flow.Tick(2.4f / Get<float>(flow, "_speed"));
                var surfaces=Get<LeafInstallTarget[]>(flow,"_leafSurfaces");
                var leafActor = actors.Single(a => a.Abilities.Kind == PlayerAbilityKind.Leaf);
                var abilities = leafActor.Abilities;
                var aim = AimFromRide(leafActor, flow.Track[0].Sample(6f, out _));
                uint cycle = unchecked(Get<uint>(abilities, "lastCycle") + 1);
                uint use = Get<uint>(abilities, "lastUse");
                abilities.Tick(cycle, use, aim, .02f);
                abilities.Tick(cycle, use, aim, abilities.Leaf.Settings.throwPreparationSeconds + .02f);
                var leaf = Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).SingleOrDefault(l => l.State == LeafState.Flying);
                Check(leaf != null, "normal ability input launches a leaf while riding");
                for (int i = 0; i < 100 && leaf.State == LeafState.Flying; i++) leaf.Tick(.02f);
                Check(leaf.Installed && surfaces.Contains(leaf.Target), "the thrown leaf installs on an authored branch collider");
                if (leaf.Target.PointUpOnInstall)
                {
                    leaf.Tick(.02f);
                    var tip = Get<Transform>(leaf, "_attachmentTip");
                    Check(Vector3.Dot(leaf.transform.forward, Vector3.up) > .999f && tip.position.y > leaf.ContactPoint.y,
                        "the installed leaf remains vertical with its pointed end above the branch");
                }
                var renderers=leaf.GetComponentsInChildren<Renderer>();
                Check(renderers.Any(r=>r.enabled&&r.gameObject.activeInHierarchy),"installed leaf has active visible renderer");
                Check(renderers.All(r=>r.bounds.size.magnitude<40),"installed leaf does not use obsolete hundredfold scale");
                for(int n=0;n<120;n++)flow.Tick(.02f);
                Check(flow.PresentationBlockedByLeaf,"path-installed leaf stops carrier");var stopped=flow.Position;
                var forward = Get<Quaternion>(flow, "_carrierRotation") * Vector3.forward;
                var carrier = Get<BranchRideCarrier>(flow, "_carrier");
                float frontReach = Get<Vector2>(carrier, "_leafStopFootprint").y;
                Check(Vector3.Dot(leaf.ContactPoint - stopped, forward) >= frontReach - .05f,
                    "the front of the paper stops before reaching the leaf");
                flow.Tick(.3f);Check(Vector3.Distance(stopped,flow.Position)<.001f,"stop persists while installed");
                abilities.Tick(cycle, unchecked(use + 1), aim, .02f);
                Check(leaf.State == LeafState.Returning, "normal recall input releases the blocker");
                flow.Tick(.2f);Check(Vector3.Distance(stopped,flow.Position)>.1f,"recall resumes travel");
                results.Add("rider aim and ability input install a leaf, paper stops in front, recall resumes");
                Reset(flow);Board();OpenAll();int failures=flow.FailureCount;var drops=new HashSet<int>();
                for(int n=0;n<5000&&flow.CurrentPhase!=BranchRidePhase.Complete;n++)
                {if(flow.Track[flow.CurrentSegment].IsDrop)drops.Add(flow.CurrentSegment);flow.Tick(.02f);Check(flow.FailureCount==failures,"connected course stays valid");}
                Check(flow.CurrentPhase==BranchRidePhase.Complete&&drops.Count==2,"full route and exactly two drops complete");
                Check(actors.All(a=>!BranchRideFlow.IsRiding(a)&&!a.GetComponent<PlayerController>().Motor.MovementLocked),"finish releases transport");
                Check(actors.Select((a,i)=>Vector3.Distance(a.transform.position,dismounts[i].position)<.1f).All(x=>x),"both dismount at authored finish");
                results.Add("entire connected course, two falls, completion and paired dismount");
                // Real swept stone collision and leaf removal, with no protective shielding.
                Reset(flow);Board();OpenAll();Set(flow,"_hazards",hazards);
                int stoneSpan=Enumerable.Range(0,flow.Track.Count).First(i=>flow.Track[i].name=="MiddleBend");
                var runway=flow.Track[stoneSpan];
                var hazard=hazards.OrderBy(h=>Enumerable.Range(0,21)
                    .Min(i=>Vector3.Distance(runway.Sample(runway.Length*i/20,out _),h.transform.position))).First();
                var stonePoint=Get<Transform[]>(hazard,"_rollPoints")[0].position;
                float stoneDistance=Enumerable.Range(0,201).Select(i=>runway.Length*i/200)
                    .OrderBy(d=>Vector3.Distance(runway.Sample(d,out _),stonePoint)).First();
                Set(flow,"_segment",stoneSpan);Set(flow,"_distance",stoneDistance);
                float landingTime=Mathf.Repeat(Get<float>(hazard,"_fallDuration")-Get<float>(hazard,"_phaseOffset"),hazard.CycleDuration);
                Set(flow,"_carrierPosition",stonePoint+Vector3.up*.5f);Set(flow,"_stoneClock",landingTime);
                var nearSurface=surfaces.OrderBy(s=>Vector3.Distance(s.transform.position,stonePoint)).First();var hitLeaf=Install(nearSurface,stonePoint);
                flow.Tick(.02f);Check(!hitLeaf.Installed,"stone removes contacted leaf");Check(flow.CurrentPhase==BranchRidePhase.Falling,"stone still hits carrier after removing leaf");
                flow.Tick(1.2f);Check(flow.CurrentPhase==BranchRidePhase.Boarding&&flow.PresentationBranchMask==0,"stone failure resets target states");Object.Destroy(hitLeaf.gameObject);
                results.Add("stone sweeps remove leaves without shielding, collide with carrier and reset");
                // Pause freezes switches, carrier and stone clock.
                Reset(flow);Board();OpenAll();var paused=flow.Position;var clock=flow.PresentationStoneClock;
                Herbalist.GameUI.GameplayPause.SetOffline(true);flow.Tick(.5f);
                Check(Vector3.Distance(paused,flow.Position)<.001f&&clock==flow.PresentationStoneClock,"pause freezes simulation");
                Herbalist.GameUI.GameplayPause.SetOffline(false);results.Add("pause freezes motion and hazards");
                return "PASS runtime: "+string.Join("; ",results)+".";
            }
            finally {Herbalist.GameUI.GameplayPause.SetOffline(false);Set(flow,"_hazards",hazards);Reset(flow);}
        }
    }
}

