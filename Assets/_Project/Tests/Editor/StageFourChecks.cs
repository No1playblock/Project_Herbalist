using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Herbalist.Levels;
using Herbalist.Abilities;
using Herbalist.Player;
using Herbalist.StageOne;
using Object=UnityEngine.Object;
namespace Herbalist.Editor
{
    public static class StageFourChecks
    {
        public static string Model()
        {
            var d=ScriptableObject.CreateInstance<SapMazeDefinition>();
            try
            {
                d.nodes=new[]{Vector2.zero,Vector2.right,Vector2.right*2};
                d.edges=new[]{new Vector2Int(0,1),new Vector2Int(1,2)};
                d.startNode=0;d.goalNode=2;d.leakNodes=new[]{1};d.speed=1;d.leakRadius=.25f;d.initialVolume=2;d.lossPerSecond=1;d.retryDelay=.1f;
                var s=SapMazeSimulation.Initial(d);
                SapMazeSimulation.Tick(d,ref s,Vector2.zero,.1f,_=>false);
                Check(s.Phase==MazePhase.Running&&s.Volume==d.initialVolume,"preloaded reservoir");
                SapMazeSimulation.Tick(d,ref s,Vector2.up,.5f,_=>false);
                Check(d.Position(s)==Vector2.zero && s.Volume==2,"wall and refill rejection");
                SapMazeSimulation.Tick(d,ref s,Vector2.right,1,_=>true);
                Check(Vector2.Distance(d.Position(s),Vector2.right)<.01f,"connected edge movement");
                SapMazeSimulation.Tick(d,ref s,Vector2.zero,.5f,_=>false);
                Check(Mathf.Abs(s.Volume-1.5f)<.01f,"standing leak loss");
                SapMazeSimulation.Tick(d,ref s,Vector2.zero,.5f,_=>true);
                Check(Mathf.Abs(s.Volume-1.5f)<.01f,"blocked leak holds volume");
                SapMazeSimulation.Tick(d,ref s,Vector2.left,.7f,_=>true);
                float volume=s.Volume;SapMazeSimulation.Tick(d,ref s,Vector2.zero,.4f,_=>false);
                Check(Mathf.Approximately(s.Volume,volume),"leaving leak stops loss");
                s.From=1;s.To=1;s.Progress=0;
                SapMazeSimulation.Tick(d,ref s,Vector2.zero,3,_=>false);
                Check(s.Phase==MazePhase.Retrying,"empty reservoir fails");
                SapMazeSimulation.Tick(d,ref s,Vector2.zero,.2f,_=>false);
                Check(s.Phase==MazePhase.Running&&s.From==0&&s.Attempt==1&&s.Volume==d.initialVolume,"only current attempt resets");
                s=SapMazeSimulation.Initial(d);SapMazeSimulation.Tick(d,ref s,Vector2.zero,.02f,_=>true);
                for(int i=0;i<30;i++)SapMazeSimulation.Tick(d,ref s,Vector2.right,.1f,_=>true);
                Check(s.Phase==MazePhase.Solved,"goal reached");
                float solvedVolume=s.Volume;SapMazeSimulation.Tick(d,ref s,Vector2.zero,10,_=>false);
                Check(s.Phase==MazePhase.Solved&&s.Volume==solvedVolume,"solved state retained");
                return "PASS model: graph walls, preloaded volume, leak/blocked/departure, retry and solved retention";
            }
            finally{Object.DestroyImmediate(d);}
        }
        public static string Scene()
        {
            if(!Application.isPlaying)throw new Exception("Run in fresh Stage Four Play mode.");
            var flow=Object.FindFirstObjectByType<StageFourFlow>();Check(flow!=null && flow.CurrentMaze==-1,"fresh stage");
            var original=StageActor.All.First(x=>x.Available);var clone=Object.Instantiate(original.gameObject);clone.name="StageFourCheckPartner";
            var partner=clone.GetComponent<StageActor>();partner.offlineSlot=1;
            var originalAbility=original.GetComponent<PlayerAbilityController>();var partnerAbility=partner.GetComponent<PlayerAbilityController>();
            partnerAbility.ConfigureNetwork(true);partnerAbility.UnlockLeaf();
            if(originalAbility.Kind!=PlayerAbilityKind.Sap)originalAbility.TrySwitchOfflineAbility();
            var actors=new[]{original,partner};var motors=actors.Select(a=>a.GetComponent<PlayerController>().Motor).ToArray();
            var leaves=new List<LeafProjectile>();var log=new List<string>();uint cycle=0;
            try
            {
                Check(!flow.CanExit,"exit cannot bypass unfinished mazes");
                for(int level=0;level<4;level++)
                {
                    var lift=flow.Lifts[level];
                    Check(lift.Upper[0].position.y<lift.Platforms[0].position.y,"destination below departure");
                    for(int side=0;side<2;side++)motors[side].Teleport(lift.Boarding[side].PlayerArrivalPoint+Vector3.up*.04f);
                    Settle(motors);
                    if(level==0)
                    {
                        motors[1].Teleport(new Vector3(0,lift.Boarding[0].PlayerArrivalPoint.y,-10));Settle(motors);flow.Tick(.02f);Check(!lift.Moving,"single rider rejected");
                        motors[1].Teleport(lift.Boarding[1].PlayerArrivalPoint+Vector3.up*.04f);Settle(motors);
                    }
                    for(int i=0;i<170;i++)flow.Tick(.02f);
                    Check(lift.Complete && flow.CurrentMaze==level,"cooperative descent "+level);
                    for(int side=0;side<2;side++)Check(Mathf.Abs(motors[side].transform.position.y-lift.Upper[side].position.y)<.2f,"rider carried "+side);
                    if(level==3)break;
                    var board=flow.Mazes[level];Check(board.Definition.IsValid()&&board.Leaks.Length==level+3,"authored data");
                    flow.Tick(.02f);Check(board.State.Phase==MazePhase.Running && !board.Controlling,"preloaded board awaits R");
                    Check(Mathf.Approximately(board.State.Volume,board.Definition.initialVolume),"per-maze preloaded volume");
                    Check(!motors[0].MovementLocked,"player starts free");
                    var control=original.GetComponent<PlayerController>();
                    var yaw=control.View.Yaw;control.View.ApplyLook(new Vector2(5,0));
                    Check(control.View.Yaw!=yaw,"current camera remains controllable");
                    Cycle(originalAbility,++cycle);Check(board.Controlling&&motors[0].MovementLocked,"R enters control");
                    Vector2 parked=board.Definition.Position(board.State);
                    Cycle(originalAbility,++cycle);flow.Tick(.1f);
                    Check(!board.Controlling&&!motors[0].MovementLocked&&board.Definition.Position(board.State)==parked,"R releases player preserving sap");
                    Cycle(originalAbility,++cycle);Check(board.Controlling,"R re-enters control");
                    board.ControlView=MazeControlView.MazeCamera;flow.Tick(.02f);
                    yaw=control.View.Yaw;control.View.ApplyLook(new Vector2(5,0));Check(control.View.Yaw==yaw,"optional maze camera override");
                    board.ControlView=MazeControlView.KeepPlayerCamera;flow.Tick(.02f);
                    yaw=control.View.Yaw;control.View.ApplyLook(new Vector2(5,0));Check(control.View.Yaw!=yaw,"camera option restores current view");
                    Check(original.GetComponent<PlayerController>().Motor.MovementLocked,"sap movement redirected");
                    if(level==1)
                    {
                        // Use a reachable socket; the reference second maze encloses leak 1.
                        int wetIndex=Enumerable.Range(0,board.Definition.leakNodes.Length)
                            .First(i=>StageFourReferenceChecks.Reachable(board.Definition,board.Definition.leakNodes[i]));
                        var approach=StageFourBuilder.FindRoute(board.Definition,board.Definition.startNode,board.Definition.leakNodes[wetIndex]);
                        int next=1,limit=0;
                        while(!SapMazeSimulation.AtLeak(board.Definition,board.State,wetIndex)&&limit++<10000)
                        {
                            if(board.State.From==approach[next]&&board.State.From==board.State.To)next++;
                            var move=(board.Definition.nodes[approach[next]]-board.Definition.Position(board.State)).normalized;
                            original.GetComponent<PlayerController>().RouteMovement(move);flow.Tick(.04f);
                        }
                        var dry=Install(board.Leaks[board.Leaks.Length-1],partner.transform);leaves.Add(dry);
                        Check(dry.State==LeafState.Installed,"preinstalled leaf is not retroactively bound");
                        Cycle(originalAbility,++cycle);
                        float remaining=board.State.Volume;flow.Tick(.1f);
                        Check(!board.Controlling&&!motors[0].MovementLocked&&board.State.Volume<remaining,"leak continues with control off");
                        for(int t=0;t<100&&board.State.Phase==MazePhase.Running;t++)flow.Tick(.1f);
                        Check(board.State.Phase==MazePhase.Retrying && board.Leaks.All(l=>l.Target.InstalledLeaf==null),"failure clears only this board leaves");
                        Check(flow.Mazes[0].State.Phase==MazePhase.Solved&&flow.Lifts[1].Complete,"completed board and lift retained on failure");
                        flow.Tick(board.Definition.retryDelay+.1f);
                        Check(board.State.Attempt==1&&flow.CurrentMaze==1,"retry remains on current floor");
                        Cycle(originalAbility,++cycle);flow.Tick(.02f);
                    }
                    var route=StageFourBuilder.FindRoute(board.Definition,board.Definition.startNode,board.Definition.goalNode);
                    int index=1;int guard=0;
                    while(board.State.Phase==MazePhase.Running && index<route.Count && guard++<30000)
                    {
                        for(int hole=0;hole<board.Leaks.Length;hole++)
                        {
                            if(!SapMazeSimulation.AtLeak(board.Definition,board.State,hole)||board.Leaks[hole].Blocked)continue;
                            if(leaves.Count>=3){Object.DestroyImmediate(leaves[0].gameObject);leaves.RemoveAt(0);}
                            var leaf=Install(board.Leaks[hole],partner.transform);leaves.Add(leaf);
                            Check(leaf.Mode==LeafMode.Platform&&leaf.State==LeafState.Bound,"wet platform leaf binding");
                            leaf.Tick(6);Check(leaf.Installed,"bound lifetime");
                        }
                        var state=board.State;
                        if(state.From==route[index]&&state.To==state.From){index++;if(index>=route.Count)break;}
                        var direction=(board.Definition.nodes[route[index]]-board.Definition.Position(board.State)).normalized;
                        original.GetComponent<PlayerController>().RouteMovement(direction);
                        flow.Tick(.04f);
                    }
                    Check(board.State.Phase==MazePhase.Solved,"maze solved "+level+" phase="+board.State.Phase);
                    Check(!original.GetComponent<PlayerController>().Motor.MovementLocked,"control lock released");
                    if(level>0)Check(flow.Mazes[level-1].State.Phase==MazePhase.Solved,"previous maze preserved");
                    log.Add("Maze "+(level+1)+": "+board.Leaks.Length+" real platform sockets, leak binding, WASD route and solved PASS");
                    foreach(var leaf in leaves)if(leaf!=null)Object.DestroyImmediate(leaf.gameObject);leaves.Clear();
                }
                Check(flow.CanExit,"all three mazes gate final exit");
                log.Add("PASS four descending paired lifts, transported riders, movement unlock and final clear gate");
                return string.Join("\n",log);
            }
            finally
            {
                foreach(var leaf in leaves)if(leaf!=null)Object.DestroyImmediate(leaf.gameObject);
                Object.DestroyImmediate(clone);
            }
        }
        private static void Cycle(PlayerAbilityController abilities,uint sequence)
        {
            abilities.Tick(sequence,0,new Ray(Vector3.zero,Vector3.forward),.02f,false);
        }
        private static LeafProjectile Install(MazeLeakSocket socket,Transform owner)
        {
            Check(!socket.Target.Accepts(LeafMode.Pin)&&socket.Target.Accepts(LeafMode.Platform),"only Platform accepted");
            var d=AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>("Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset");
            var leaf=Object.Instantiate(d.offlinePrefab);
            var point=socket.transform.position;
            leaf.Initialize(d,owner,LeafMode.Platform,point+Vector3.forward*2,point-Vector3.forward,l=>Object.Destroy(l.gameObject),true);
            Physics.SyncTransforms();
            for(int i=0;i<30&&!leaf.Installed;i++)leaf.Tick(.02f);
            Check(leaf.Target==socket.Target&&leaf.Installed,"physical socket collision");return leaf;
        }
        private static void Settle(PlayerMotor[] motors){Physics.SyncTransforms();for(int i=0;i<10;i++)foreach(var m in motors)m.Simulate(Vector3.zero,.02f);Physics.SyncTransforms();}
        private static void Check(bool ok,string message){if(!ok)throw new Exception("Stage Four: "+message);}
    }
}