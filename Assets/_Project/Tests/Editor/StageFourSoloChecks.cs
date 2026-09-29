using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.Abilities;
using Herbalist.StageOne;
using Herbalist.Development;
using Object=UnityEngine.Object;
namespace Herbalist.Editor
{
    public static class StageFourSoloChecks
    {
        public static string Run()
        {
            if(!Application.isPlaying)throw new Exception("Run in fresh Stage Four solo Play.");
            var flow=Object.FindFirstObjectByType<StageFourFlow>();
            var test=Object.FindFirstObjectByType<StageFourSoloTest>();
            var actor=StageActor.All.Single(a=>a.Available);
            var player=actor.GetComponent<PlayerController>();var abilities=actor.Abilities;
            var cursor=Herbalist.Presentation.GameplayCursor.Instance;
            if(cursor!=null)cursor.enabled=false; // No focused game window while MCP runs these input events.
            var previousBackground=InputSystem.settings.backgroundBehavior;
            var previousRunInBackground=Application.runInBackground;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            Application.runInBackground=true;
            var previousInputBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                Check(test.Active && flow.CurrentMaze==0,"automatic first maze entry");
                Check(flow.Mazes[0].ControlArea.Contains(actor.transform.position+Vector3.up),"spawn inside control area");
                Check(abilities.Kind==PlayerAbilityKind.Sap,"sap start");
                Check(!flow.CanExit,"cannot bypass puzzles");
                var originals=flow.Mazes.Select(m=>AssetDatabase.LoadAssetAtPath<SapMazeDefinition>(
                    "Assets/_Project/Data/Stages/Stage_04/SO_SapMaze_"+(Array.IndexOf(flow.Mazes,m)+1)+".asset")).ToArray();
                for(int i=0;i<originals.Length;i++)Check(flow.Mazes[i].Definition!=originals[i],"test uses copied data");
                Press(keyboard,Key.R);TickInput(abilities,.02f);
                Check(flow.Mazes[0].Controlling,"actual R action starts control");
                Press(keyboard,Key.R);TickInput(abilities,.02f);
                Check(!flow.Mazes[0].Controlling&&!player.Motor.MovementLocked,"actual R action releases control");
                var messages=new List<string>();
                for(int level=0;level<3;level++)
                {
                    var maze=flow.Mazes[level];
                    player.Motor.Teleport(flow.Lifts[level].Upper[0].position+Vector3.up*.04f);
                    Settle(player.Motor);flow.Tick(.02f);
                    Press(keyboard,Key.R);TickInput(abilities,.02f);Check(maze.Controlling,"control entry "+level);
                    var route=StageFourBuilder.FindRoute(maze.Definition,maze.Definition.startNode,maze.Definition.goalNode);
                    int index=1,guard=0;
                    while(maze.State.Phase==MazePhase.Running && guard++<30000)
                    {
                        for(int i=0;i<maze.Leaks.Length;i++)
                        {
                            var socket=maze.Leaks[i];
                            if(!SapMazeSimulation.AtLeak(maze.Definition,maze.State,i)||socket.Blocked)continue;
                            Press(keyboard,Key.R);TickInput(abilities,.02f);
                            Press(keyboard,Key.Q);flow.Tick(.02f);
                            Check(abilities.Kind==PlayerAbilityKind.Leaf && !maze.Controlling && !player.Motor.MovementLocked,"Q switches and releases sap locks");
                            player.Motor.Teleport(flow.Lifts[level].Upper[1].position+Vector3.up*.04f);
                            Settle(player.Motor);flow.Tick(.02f);
                            Press(keyboard,Key.R);TickInput(abilities,.02f);
                            Press(keyboard,Key.R);TickInput(abilities,.02f);
                            Check(abilities.Mode==LeafMode.Platform,"real R selects platform");
                            for(int n=0;n<500&&!socket.Blocked;n++)
                            {
                                var aim=new Ray(socket.transform.position+Vector3.forward*2,Vector3.back);
                                abilities.Leaf.TryThrow(LeafMode.Platform,aim);
                                TickInput(abilities,.02f);
                                foreach(var leaf in Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None))leaf.Tick(.02f);
                                flow.Tick(.02f);
                            }
                            Check(socket.Blocked && socket.Target.InstalledLeaf.State==LeafState.Bound,"real solo leaf throw/binding");
                            Press(keyboard,Key.Q);Check(abilities.Kind==PlayerAbilityKind.Sap,"Q returns to sap");
                            player.Motor.Teleport(flow.Lifts[level].Upper[0].position+Vector3.up*.04f);
                            Settle(player.Motor);flow.Tick(.02f);
                            Press(keyboard,Key.R);TickInput(abilities,.02f);Check(maze.Controlling,"resume parked sap");
                        }
                        if(maze.State.Phase!=MazePhase.Running)break;
                        var state=maze.State;
                        if(state.From==route[index]&&state.To==state.From){index++;if(index>=route.Count)break;}
                        player.RouteMovement((maze.Definition.nodes[route[index]]-maze.Definition.Position(state)).normalized);
                        TickInput(abilities,.04f);flow.Tick(.04f);
                    }
                    Check(maze.State.Phase==MazePhase.Solved,"solo maze solved "+level);
                    var lift=flow.Lifts[level+1];
                    int side=level%2;player.Motor.Teleport(lift.Boarding[side].PlayerArrivalPoint+Vector3.up*.04f);
                    Settle(player.Motor);
                    Check(!lift.TryBegin(actor,null),"cooperative API still rejects single rider");
                    for(int n=0;n<170;n++)flow.Tick(.02f);
                    Check(lift.Complete && flow.CurrentMaze==level+1,"single-rider test descent "+level);
                    Check(Mathf.Abs(actor.transform.position.y-lift.Upper[side].position.y)<.2f,"single rider carried");
                    messages.Add("Solo maze "+(level+1)+": actual R/Q, real platform leaf binding, single-rider descent PASS");
                }
                for(int i=0;i<originals.Length;i++)Check(originals[i].initialVolume==12+i*4,"original data unchanged");
                Check(flow.CanExit && !test.TransitionRequested,"solo clear gate");
                Physics.SyncTransforms();
                player.Motor.Teleport(flow.ExitZone.PlayerArrivalPoint+Vector3.up*.04f);
                messages.Add("PASS solo first entry, all mazes, unchanged source assets and exit readiness");
                return string.Join("\n",messages);
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);
                InputSystem.settings.editorInputBehaviorInPlayMode=previousInputBehavior;
                InputSystem.settings.backgroundBehavior=previousBackground;
                Application.runInBackground=previousRunInBackground;
                if(cursor!=null)cursor.enabled=true;
            }
        }
        private static void TickInput(PlayerAbilityController a,float dt)=>a.Tick(a.Input.CycleSequence,a.Input.UseSequence,
            new Ray(a.transform.position,Vector3.forward),dt);
        private static void Press(Keyboard keyboard,Key key)
        {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(key));UpdateDynamicInput();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());UpdateDynamicInput();
        }
        // MCP invokes from the Editor update; explicitly process the game's dynamic input buffer.
        private static void UpdateDynamicInput()
        {
            typeof(InputSystem).GetMethod("Update",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic,
                null,new[]{typeof(InputUpdateType)},null).Invoke(null,new object[]{InputUpdateType.Dynamic});
        }
        private static void Settle(PlayerMotor motor)
        {
            Physics.SyncTransforms();for(int i=0;i<10;i++)motor.Simulate(Vector3.zero,.02f);Physics.SyncTransforms();
        }
        private static void Check(bool result,string text){if(!result)throw new Exception("StageFourSolo: "+text);}
    }
}
