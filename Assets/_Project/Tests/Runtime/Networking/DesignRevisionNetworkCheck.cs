#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Herbalist.Networking;
using Herbalist.GameUI;
using Herbalist.StageOne;
using Herbalist.Levels;
using Herbalist.Abilities;
namespace Herbalist.Tests
{
    // Explicit opt-in two-process smoke check. Never runs in ordinary play or release builds.
    public static class DesignRevisionNetworkCheck
    {
        private static T Read<T>(object obj, string name) => (T)obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
        private static void Write(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        private static void Check(bool ok, string label) { if (!ok) throw new Exception(label); }
        private static async Task Wait(Func<bool> predicate, string label, float seconds = 90)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < end) await Task.Delay(50);
            Check(predicate(), "Timeout " + label);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-herbalist-revision-check");
            if (index >= 0 && index + 2 < args.Length) Run(args[index + 1] == "host", args[index + 2]);
        }
        private static async void Run(bool host, string room)
        {
            Keyboard keyboard = null; Application.runInBackground = true;
            try
            {
                await Wait(() => FusionLobbySession.Instance != null, "menu");
                var session = FusionLobbySession.Instance;
                await session.ConnectAsync(room, host);
                await Wait(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "pair");
                RoomControl.Instance.Select(host ? 1 : 0);
                if (host) { await Wait(() => RoomControl.Instance.Ready, "role selection"); RoomControl.Instance.StartGame(); }
                await Wait(() => StageOneFlow.Instance != null && NetworkPlayer.Local != null && session.State == LobbyState.Playing, "exterior");
                await Task.Delay(500);
                if (host)
                {
                    var flow = StageOneFlow.Instance; flow.RefreshActors(); var gatherer = flow.Actors[0]; var crafter = flow.Actors[1];
                    Check(!gatherer.Abilities.Unlocked && !crafter.Abilities.Unlocked, "no prototype grant in exterior");
                    for (int i = 0; i < flow.sources.Length; i++)
                    {
                        var item = flow.sources[i].item;
                        Teleport(gatherer, flow.sources[i].Point - flow.settings.interactionOffset);
                        Teleport(crafter, gatherer.transform.position + Vector3.right);
                        Check(flow.Execute(gatherer, StageCommand.Interact) == flow.settings.successMessage, "gather");
                        Check(flow.Execute(gatherer, StageCommand.Interact) == flow.settings.successMessage, "give herb");
                        Check(flow.Execute(crafter, StageCommand.Craft) == flow.settings.successMessage, "craft");
                        var potion = flow.settings.Item(flow.Progress.Held[1]);
                        if (potion.ability == PlayerAbilityKind.Sap) Check(flow.Execute(crafter, StageCommand.Interact) == flow.settings.successMessage, "give potion");
                        Check(flow.Execute(potion.ability == PlayerAbilityKind.Sap ? gatherer : crafter, StageCommand.Drink) == flow.settings.successMessage, "drink");
                    }
                }
                var pad = UnityEngine.Object.FindObjectsByType<ExteriorJumpPad>(FindObjectsSortMode.None).OrderBy(x => x.name).First();
                var socket = Read<LeafInstallTarget[]>(pad, "_sockets")[0];
                if (host)
                {
                    var actor = StageOneFlow.Instance.Actors[1];
                    Teleport(actor, socket.SocketPosition + Vector3.back * 2);
                    Physics.SyncTransforms();
                    Check(actor.Abilities.Leaf.TryThrowAtTarget(new Ray(socket.SocketPosition + Vector3.up * 3, Vector3.down)), "single leaf authoritative throw");
                    var leaf = Read<System.Collections.Generic.List<LeafProjectile>>(actor.Abilities.Leaf, "leaves").Last();
                    typeof(LeafProjectile).GetMethod("Install", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(leaf, new object[] { socket, socket.SocketPosition, Vector3.up, Vector3.forward });
                }
                await Wait(() => UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).Any(x => x.Installed && x.Target == socket), "single leaf installation replicated");
                var installedLeaf = UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).First(x => x.Installed && x.Target == socket);
                await Wait(() => Vector3.Distance(installedLeaf.transform.position, socket.SocketPosition) < .05f && Quaternion.Angle(installedLeaf.transform.rotation, socket.SocketRotation) < 1, "socket pose replicated");
                if (host)
                {
                    await Task.Delay(2000);
                    Check(StageOneFlow.Instance.Actors[1].Abilities.Leaf.RecallOldest(), "single leaf recall");
                }
                await Wait(() => !socket.HasLeaf, "single leaf recall replicated");
                Debug.Log("[DesignRevisionNetworkCheck] SingleLeaf PASS " + (host ? "host" : "client"));
                if (host)
                {
                    var flow = StageOneFlow.Instance; var gatherer = flow.Actors[0]; var crafter = flow.Actors[1];
                    var exterior = UnityEngine.Object.FindFirstObjectByType<ExteriorStageFlow>();
                    var top = Read<StageArrivalZone>(exterior, "_top");
                    Teleport(gatherer, top.PlayerArrivalPoint); Teleport(crafter, top.PlayerArrivalPoint + Vector3.right);
                    await Wait(() => Read<uint>(exterior, "_arrived") == 3, "both summit arrivals");
                    Read<SapInjectionPort>(exterior, "_inlet").Inject(3.1f);
                    await Wait(() => exterior.CanExit && flow.GateOpen, "injected summit gate");
                    Vector3 inside = flow.interior.transform.TransformPoint(flow.interior.center) - flow.settings.interactionOffset;
                    Teleport(gatherer, inside); Teleport(crafter, inside + Vector3.right);
                }
                await Wait(() => UnityEngine.Object.FindFirstObjectByType<DescendingMazeFlow>() != null && session.State == LobbyState.Playing && NetworkPlayer.Local != null, "maze transition");
                await Task.Delay(500);
                Check(NetworkPlayer.Local.GetComponent<PlayerAbilityController>().Kind == (host ? PlayerAbilityKind.Leaf : PlayerAbilityKind.Sap), "ability preserved across scene transition");
                var mazeFlow = UnityEngine.Object.FindFirstObjectByType<DescendingMazeFlow>();
                await Wait(() => mazeFlow.CurrentMaze == 0, "first automatic descent");
                var screen = UnityEngine.Object.FindFirstObjectByType<Herbalist.Presentation.PlayScreenController>();
                Check(screen.IsLocked && !screen.TryToggle(), "personal screen locked on both peers");
                var boards = Read<SapMazeBoard[]>(mazeFlow, "_mazes");
                if (!host)
                {
                    var cursor = UnityEngine.Object.FindFirstObjectByType<Herbalist.Presentation.GameplayCursor>(); if (cursor != null) cursor.enabled = false;
                    keyboard = InputSystem.AddDevice<Keyboard>(); InputSystem.EnableDevice(keyboard);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); await Task.Delay(160);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                }
                await Wait(() => boards[0].Controlling, "remote R reaches Host and replicates");
                Debug.Log("[DesignRevisionNetworkCheck] Control " + (host ? "host" : "client") + " active=" + boards[0].Active + " position=" + NetworkPlayer.Local.transform.position);
                if (!host)
                {
                    var d = boards[0].Definition; var edge = d.edges.Where(x => x.x == d.startNode || x.y == d.startNode).OrderByDescending(x => Vector2.Distance(d.nodes[x.x], d.nodes[x.y])).First();
                    int next = edge.x == d.startNode ? edge.y : edge.x; var delta = d.nodes[next] - d.nodes[d.startNode];
                    var key = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? (delta.x > 0 ? Key.D : Key.A) : (delta.y > 0 ? Key.W : Key.S);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); await Task.Delay(500);
                    Debug.Log("[DesignRevisionNetworkCheck] WASD key=" + key + " input=" + NetworkPlayer.Local.ReadLocalInput().Move + " sap=" + d.Position(boards[0].State));
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                }
                await Wait(() => Vector2.Distance(boards[0].Definition.Position(boards[0].State), boards[0].Definition.nodes[boards[0].Definition.startNode]) > .03f, "remote WASD moves sap");
                await Task.Delay(600);
                if (!host) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R)); await Task.Delay(160); InputSystem.QueueStateEvent(keyboard, new KeyboardState()); }
                await Wait(() => !boards[0].Controlling, "remote R releases control");
                for (int i = 0; i < boards.Length; i++)
                {
                    await Wait(() => mazeFlow.CurrentMaze == i, "automatic raft station " + i);
                    if (host)
                    {
                        // Graph traversal is checked separately. This accelerates the transport/scene replication smoke check.
                        var state = boards[i].State; state.Phase = MazePhase.Solved; state.From = state.To = boards[i].Definition.goalNode; state.Progress = 0;
                        Write(boards[i], "_state", state);
                    }
                    if (i < boards.Length - 1) await Wait(() => mazeFlow.CurrentMaze > i, "same rafts descend " + i);
                }
                await Wait(() => UnityEngine.Object.FindFirstObjectByType<RotatingAltar>() != null && session.State == LobbyState.Playing && NetworkPlayer.Local != null, "altar transition");
                Check(NetworkPlayer.Local.GetComponent<PlayerAbilityController>().Kind == (host ? PlayerAbilityKind.Leaf : PlayerAbilityKind.Sap), "ability retained at altar");
                var altar = UnityEngine.Object.FindFirstObjectByType<RotatingAltar>(); var rings = Read<AltarRing[]>(altar, "_rings");
                if (host) Read<SapInjectionPort>(altar, "_inlet").Inject(1.1f);
                float initialAngle = rings[0].CurrentAngle;
                await Wait(() => Mathf.Abs(Mathf.DeltaAngle(initialAngle, rings[0].CurrentAngle)) > 2, "altar rotation replicated");
                Debug.Log("[DesignRevisionNetworkCheck] PASS " + (host ? "host" : "client") + ": exterior gathering/crafting/potions, injected entrance, ability-preserving scene transitions, remote R/WASD, persistent raft descent and altar rotation.");
                // Client remains connected until the Host has observed the final state.
                await Task.Delay(host ? 1000 : 2500);
                await session.LeaveAsync(); Application.Quit(0);
            }
            catch (Exception exception) { Debug.LogError("[DesignRevisionNetworkCheck] FAIL " + exception); Application.Quit(1); }
            finally { if (keyboard != null) InputSystem.RemoveDevice(keyboard); }
        }
        private static void Teleport(StageActor actor, Vector3 point) => actor.GetComponent<NetworkPlayer>().TeleportAuthoritatively(point);
    }
}
#endif
