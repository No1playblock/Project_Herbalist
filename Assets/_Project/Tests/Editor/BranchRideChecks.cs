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
            Check(switches.Select(s=>s.BranchCount).SequenceEqual(new[]{2,3,3,2}),"target groups 2/3/3/2");
            Check(Get<BranchRideCarrier>(flow,"_carrier")!=null,"separate carrier backend");
            var surfaces=Get<LeafInstallTarget[]>(flow,"_leafSurfaces");
            Check(surfaces.Length>10&&surfaces.All(t=>!t.HasSocket),"free branch-surface leaf installation");
            Check(surfaces.Select(s=>s.Id).Distinct().Count()==surfaces.Length,"unique leaf target IDs");
            var tongues=tracks.Where(t=>t.SwitchIndex>=0).ToArray();
            Check(tongues.Length==10 && tongues.All(t=>t.Exits.Length==2),"ten A/B turnouts");
            var heels=tongues.Select(t=>t.Start).ToArray();
            try
            {
                foreach(var toggle in switches)toggle.Present(0);
                for(int i=0;i<tongues.Length;i++)
                {
                    Check(Vector3.Distance(tongues[i].Start,heels[i])<.001f,"fixed common entrance A");
                    Check(Vector3.Distance(tongues[i].End,tongues[i].Exits[0].Start)<.01f,"tip connects to physical A branch");
                    Check(Vector3.Distance(tongues[i].End,tongues[i].Exits[1].Start)>1,"A and B exits are distinct");
                }
                var aWalk=Walk(tracks[0]);
                Check(aWalk.Last().name=="RouteA_DeadEnd"&&!aWalk.Last().IsFinish,"A route follows an actual fixed branch to its end");
                foreach(var toggle in switches)toggle.Present(1);
                for(int i=0;i<tongues.Length;i++)
                {
                    Check(Vector3.Distance(tongues[i].Start,heels[i])<.001f,"fixed common entrance B");
                    Check(Vector3.Distance(tongues[i].End,tongues[i].Exits[1].Start)<.01f,"tip connects to physical B branch");
                }
                var bWalk=Walk(tracks[0]);
                Check(bWalk.Last().IsFinish&&bWalk.Count(t=>t.IsDrop)==2,"B graph reaches finish through two drops");
                foreach(var drop in tracks.Where(t=>t.IsDrop))
                {
                    Check(drop.Start.y>drop.End.y+1,"drop descends");
                    Vector3 middle=(drop.Start+drop.End)*.5f;
                    Check(!Physics.CheckSphere(middle,.3f,1,QueryTriggerInteraction.Ignore),"empty-air drop gap");
                }
            }
            finally {foreach(var toggle in switches)toggle.Present(0);}
            Check(Get<Transform[]>(flow,"_respawns").Length==2&&Get<Transform[]>(flow,"_dismounts").Length==2,"paired spawn and dismount poses");
            Check(Get<GameObject>(flow,"_endingPanel")!=null,"authored completion UGUI");
            foreach(var t in flow.transform.GetComponentsInChildren<Transform>(true))Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"missing script "+t.name);
            var hazards=Get<BranchRideStone[]>(flow,"_hazards");Check(hazards.Length==2,"two authored rolling hazards");
            foreach(var hazard in hazards)
            {
                Check(hazard.RollLength>4,"stone has rolling path");
                Check(hazard.Sample(0,out var a),"stone cycle starts active");
                bool hidden=false;for(float t=0;t<hazard.CycleDuration;t+=.05f)hidden|=!hazard.Sample(t,out _);
                Check(hidden,"stone has respawn interval");
            }
            return "PASS authored: "+tracks.Length+" authored spans, ten fixed-heel A/B turnouts, both physical exit connections, two open-air drops, T1~T4 = 2/3/3/2, unique free-install leaf surfaces, rolling stones, paired spawn/dismount, completion UGUI.";
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
            }
            void OpenAll()
            {foreach(var toggle in switches)toggle.Port.Inject(.3f);flow.Tick(.8f);}
            LeafProjectile Install(LeafInstallTarget target,Vector3 point)
            {
                var owner=actors[1].Abilities;var leaf=Object.Instantiate(owner.Leaf.Settings.offlinePrefab);
                leaf.Initialize(owner.Leaf.Settings,owner.transform,point,point,_=>Object.Destroy(leaf.gameObject),true);
                typeof(LeafProjectile).GetMethod("Install",Private).Invoke(leaf,new object[]{target,point,Vector3.up,Vector3.right});return leaf;
            }
            try
            {
                Reset(flow);
                actors[0].GetComponent<PlayerController>().Motor.Teleport(respawns[0].position);
                Check(BranchRideFlow.TryBoard(actors[0],out _),"first rider accepted");var before=flow.Position;flow.Tick(1);
                Check(flow.CurrentPhase==BranchRidePhase.Boarding&&Vector3.Distance(before,flow.Position)<.001f,"one rider cannot depart");
                Check(BranchRideFlow.IsRiding(actors[0])&&!BranchRideFlow.IsRiding(actors[1]),"rider ownership");
                results.Add("single-rider wait and rider ownership");
                Reset(flow);Set(flow,"_hazards",Array.Empty<BranchRideStone>());Board();int initialFailures=flow.FailureCount;
                bool traversedA=false;
                for(int n=0;n<1800&&flow.FailureCount==initialFailures;n++)
                {traversedA|=flow.Track[flow.CurrentSegment].name=="RouteA_DeadEnd";flow.Tick(.02f);}
                Check(traversedA&&flow.FailureCount==initialFailures+1&&flow.CurrentPhase==BranchRidePhase.Boarding,"A branch is traversed before its dead end falls and resets");
                Check(actors.All(a=>!BranchRideFlow.IsRiding(a)&&!a.GetComponent<PlayerController>().Motor.MovementLocked),"reset releases both riders");
                Check(actors.Select((a,i)=>Vector3.Distance(a.transform.position,respawns[i].position)<.1f).All(x=>x),"reset respawn positions");
                results.Add("physical A branch traversal, dead-end fall, full restart and transport release");
                Reset(flow);Board();var first=switches[0];first.Port.Inject(.3f);flow.Tick(.02f);Check((flow.PresentationBranchMask&1)!=0,"water activates T1");
                for(int n=0;n<10;n++){first.Port.Inject(.05f);flow.Tick(.05f);}Check((flow.PresentationBranchMask&1)!=0,"held spray does not toggle repeatedly");
                flow.Tick(.5f);first.Port.Inject(.3f);flow.Tick(.02f);Check((flow.PresentationBranchMask&1)==0,"release and re-hit toggles again");
                results.Add("authoritative injection, one activation per burst and re-arm");
                Reset(flow);Board();OpenAll();
                var surfaces=Get<LeafInstallTarget[]>(flow,"_leafSurfaces");
                var leafTarget=surfaces.First(s=>s.transform.position.x<6);
                var stopPoint=flow.Track[0].Sample(5,out _);var leaf=Install(leafTarget,stopPoint);
                for(int n=0;n<120;n++)flow.Tick(.02f);
                Check(flow.PresentationBlockedByLeaf,"path-installed leaf stops carrier");var stopped=flow.Position;
                flow.Tick(.3f);Check(Vector3.Distance(stopped,flow.Position)<.001f,"stop persists while installed");
                leaf.BeginReturn();flow.Tick(.2f);Check(Vector3.Distance(stopped,flow.Position)>.1f,"recall resumes travel");Object.Destroy(leaf.gameObject);
                results.Add("free-surface installed leaf stop, hold and recall resume");
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
                Set(flow,"_segment",stoneSpan);Set(flow,"_distance",flow.Track[stoneSpan].Length);
                var stonePoint=flow.Track[stoneSpan].End;
                Set(flow,"_carrierPosition",stonePoint+Vector3.up*.5f);Set(flow,"_stoneClock",.8f);
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

