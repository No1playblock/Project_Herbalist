#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Herbalist.Networking;
using Herbalist.GameUI;
using Herbalist.Abilities;
using Herbalist.Levels;
namespace Herbalist.Tests
{
    public static class StageFourNetworkCheck
    {
        private static async Task Wait(Func<bool> predicate,string label,float seconds=90)
        {
            float end=Time.realtimeSinceStartup+seconds;
            while(!predicate()&&Time.realtimeSinceStartup<end)await Task.Delay(40);
            Check(predicate(),label);
        }
        private static void Check(bool value,string label){if(!value)throw new Exception(label);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var args=Environment.GetCommandLineArgs();int n=Array.IndexOf(args,"-herbalist-stage-four-check");
            if(n>=0&&n+2<args.Length)Run(args[n+1]=="host",args[n+2]);
        }
        private static async void Run(bool host,string room)
        {
            Keyboard keyboard=null;Application.runInBackground=true;
            try
            {
                await Wait(()=>FusionLobbySession.Instance!=null,"lobby");
                var session=FusionLobbySession.Instance;
                var field=typeof(FusionLobbySession).GetField("settings",BindingFlags.NonPublic|BindingFlags.Instance);
                var settings=UnityEngine.Object.Instantiate((LobbySettings)field.GetValue(session));
                settings.playScenePath="Assets/_Project/Scenes/Stages/Stage_04/Stage_04_Arrival.unity";field.SetValue(session,settings);
                await session.ConnectAsync(room,host);
                await Wait(()=>session.ConnectedCount==2&&session.State==LobbyState.Waiting,"pair");
                RoomControl.Instance.Select(host?1:0);
                if(host){await Wait(()=>RoomControl.Instance.Ready,"choices");RoomControl.Instance.StartGame();}
                await Wait(()=>UnityEngine.Object.FindFirstObjectByType<StageFourFlow>()!=null&&NetworkPlayer.Local!=null&&session.State==LobbyState.Playing,"stage four");
                await Task.Delay(700);
                // Headless processes have no focused/captured window. Remove only the pointer-focus gate in this explicit test.
                if(Application.isBatchMode)
                {
                    var cursor=UnityEngine.Object.FindFirstObjectByType<Herbalist.Presentation.GameplayCursor>();
                    if(cursor!=null)cursor.enabled=false;
                }
                var flow=UnityEngine.Object.FindFirstObjectByType<StageFourFlow>();
                Check(!flow.OfflineTestActive && flow.CurrentMaze==-1,"solo override disabled in multiplayer");
                var players=UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
                var sap=players.First(p=>p.GetComponent<PlayerAbilityController>().Kind==PlayerAbilityKind.Sap);
                var leaf=players.First(p=>p.GetComponent<PlayerAbilityController>().Kind==PlayerAbilityKind.Leaf);
                Check(host?leaf.HasInputAuthority:sap.HasInputAuthority,"remote sap input path");
                foreach(var board in flow.Mazes)
                {
                    // Accelerated transport/input smoke test only; do not modify the serialized assets.
                    board.Definition.speed=2;board.Definition.initialVolume=120;
                }
                if(!host){keyboard=InputSystem.AddDevice<Keyboard>();InputSystem.EnableDevice(keyboard);}
                for(int stage=0;stage<4;stage++)
                {
                    var lift=flow.Lifts[stage];
                    if(host)
                    {
                        sap.TeleportAuthoritatively(lift.Boarding[0].PlayerArrivalPoint+Vector3.up*.05f);
                        leaf.TeleportAuthoritatively(lift.Boarding[1].PlayerArrivalPoint+Vector3.up*.05f);
                    }
                    await Wait(()=>flow.CurrentMaze>=stage,"replicated lift "+stage);
                    Check(Mathf.Abs(sap.SimulationPosition.y-lift.Upper[0].position.y)<.4f &&
                        Mathf.Abs(leaf.SimulationPosition.y-lift.Upper[1].position.y)<.4f,"paired lift positions");
                    if(stage==3)break;
                    var maze=flow.Mazes[stage];
                    await Wait(()=>maze.Active,"maze active");
                    await Wait(()=>maze.State.Phase==MazePhase.Running,"preloaded sap");
                    Check(!maze.Controlling,"awaits R");
                    if(!host)
                    {
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R));await Task.Delay(150);
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    }
                    await Wait(()=>maze.Controlling,"remote R enters control");
                    if(!host)
                    {
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R));await Task.Delay(150);
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    }
                    await Wait(()=>!maze.Controlling,"remote R exits control");
                    Check(!sap.GetComponent<Herbalist.Player.PlayerController>().Motor.HasMovementLockExcept(sap),"control lock released");
                    if(!host)
                    {
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R));await Task.Delay(150);
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    }
                    await Wait(()=>maze.Controlling,"remote R re-enters control");
                    float end=Time.realtimeSinceStartup+360;
                    while(maze.State.Phase==MazePhase.Running&&Time.realtimeSinceStartup<end)
                    {
                        if(host)
                        {
                            for(int i=0;i<maze.Leaks.Length;i++)
                            {
                                var socket=maze.Leaks[i];
                                if(!SapMazeSimulation.AtLeak(maze.Definition,maze.State,i)||socket.Blocked)continue;
                                var aim=new Ray(socket.transform.position+Vector3.forward*2,Vector3.back);
                                leaf.GetComponent<PlayerAbilityController>().Leaf.TryThrow(LeafMode.Platform,aim);
                                break;
                            }
                        }
                        else
                        {
                            var state=maze.State;Vector2 direction=Vector2.zero;
                            if(!maze.AnyLeak)
                            {
                                var route=Route(maze.Definition,state.From,maze.Definition.goalNode);
                                if(route.Count>1)
                                {
                                    direction=state.From!=state.To&&route[1]!=state.To?
                                        maze.Definition.nodes[state.From]-maze.Definition.Position(state):
                                        maze.Definition.nodes[route[1]]-maze.Definition.Position(state);
                                }
                            }
                            Key key=direction.sqrMagnitude<.0001f?Key.None:Mathf.Abs(direction.x)>Mathf.Abs(direction.y)?
                                direction.x>0?Key.D:Key.A:direction.y>0?Key.W:Key.S;
                            InputSystem.QueueStateEvent(keyboard,key==Key.None?new KeyboardState():new KeyboardState(key));
                        }
                        await Task.Delay(20);
                    }
                    if(keyboard!=null)InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    Check(maze.State.Phase==MazePhase.Solved,"solved "+stage+" phase="+maze.State.Phase+" from="+maze.State.From+" to="+maze.State.To+" progress="+maze.State.Progress+" leak="+maze.AnyLeak);
                    Debug.Log("[StageFourNetworkCheck] "+(host?"HOST":"CLIENT")+" MAZE "+(stage+1)+" PASS");
                    await Task.Delay(host?1200:100);
                }
                if(host)
                {
                    var zone=GameObject.Find("StageFiveExit").GetComponent<StageArrivalZone>();
                    sap.TeleportAuthoritatively(zone.PlayerArrivalPoint+Vector3.left);
                    await Task.Delay(400);leaf.TeleportAuthoritatively(zone.PlayerArrivalPoint+Vector3.right);
                }
                await Wait(()=>SceneManager.GetActiveScene().name=="Stage_05_Arrival"&&NetworkPlayer.Local!=null,"stage five");
                await Task.Delay(500);
                Check(NetworkPlayer.Local.GetComponent<PlayerAbilityController>().Kind==(host?PlayerAbilityKind.Leaf:PlayerAbilityKind.Sap),"ability retained");
                Debug.Log("[StageFourNetworkCheck] PASS "+(host?"HOST":"CLIENT")+" preloaded sap, remote R/WASD, platform leaf seals, four descents and Stage Five");
            }
            catch(Exception e){Debug.LogError("[StageFourNetworkCheck] FAIL "+(host?"HOST":"CLIENT")+" "+e);}
            finally{if(keyboard!=null)InputSystem.RemoveDevice(keyboard);}
        }
        private static List<int> Route(SapMazeDefinition d,int start,int goal)
        {
            var parent=Enumerable.Repeat(-1,d.nodes.Length).ToArray();var queue=new Queue<int>();parent[start]=start;queue.Enqueue(start);
            while(queue.Count>0)
            {
                int n=queue.Dequeue();if(n==goal)break;
                foreach(var e in d.edges){int next=e.x==n?e.y:e.y==n?e.x:-1;if(next>=0&&parent[next]<0){parent[next]=n;queue.Enqueue(next);}}
            }
            var result=new List<int>();for(int n=goal;;n=parent[n]){result.Add(n);if(n==start)break;}result.Reverse();return result;
        }
    }
}
#endif