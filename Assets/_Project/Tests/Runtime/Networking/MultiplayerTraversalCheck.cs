#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace Herbalist.Tests
{
    // Explicit two-process regression only. Checkpoints coordinate the local test processes;
    // gameplay state and input still travel through the real Photon Host/Client connection.
    public static class MultiplayerTraversalCheck
    {
        private static bool _running;
        private static string _folder;
        private static T Read<T>(object obj, string name) => (T)obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
        private static void Check(bool value, string label) { if (!value) throw new Exception(label); }
        private static void Log(string text) => Debug.Log("[TraversalCheck] " + text);
        private static async Task Wait(Func<bool> predicate, string label, float seconds = 120)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline) await Task.Delay(50);
            Check(predicate(), "Timeout: " + label);
        }
        private static string Status(string file) => File.Exists(Path.Combine(_folder, file)) ? File.ReadAllText(Path.Combine(_folder, file)) : "";
        private static void Set(string file, string value) => File.WriteAllText(Path.Combine(_folder, file), value);
        private static async Task Signal(string value)
        {
            Set("host.txt", value);
            await Wait(() => Status("client.txt") == value, "client acknowledgement " + value);
            Log("PAIR PASS " + value);
        }
        private static NetworkPlayer Net(StageActor actor) => actor.GetComponent<NetworkPlayer>();
        private static void Teleport(StageActor actor, Vector3 point) => Net(actor).TeleportAuthoritatively(point);
        private static async Task Press(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); await Task.Delay(250);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); await Task.Delay(250);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, "-herbalist-traversal-check");
            if (i >= 0 && i + 3 < args.Length) Run(args[i + 1] == "host", args[i + 2], args[i + 3]);
        }
        public static async void Run(bool host, string room, string folder)
        {
            if (_running) return;
            _running = true; _folder = folder; Directory.CreateDirectory(folder);
            Application.runInBackground = true;
            Keyboard keyboard = null; Mouse mouse = null;
            try
            {
                await Wait(() => FusionLobbySession.Instance != null, "menu");
                var session = FusionLobbySession.Instance;
                await session.ConnectAsync(room, host);
                await Wait(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "two peers");
                RoomControl.Instance.Select(host ? 1 : 0);
                if (host) { await Wait(() => RoomControl.Instance.Ready, "roles"); RoomControl.Instance.StartGame(); }
                await Wait(() => StageOneFlow.Instance != null && NetworkPlayer.Local != null && session.State == LobbyState.Playing, "Stage 1");
                Check(!Physics.GetIgnoreLayerCollision(8, 22), "network player/tree layer enabled");
                Log("Stage1 connected, local slot=" + NetworkPlayer.Local.Slot);
                if (host)
                {
                    var flow = StageOneFlow.Instance; flow.RefreshActors();
                    // Seed crafted potions only; consume through the ordinary authoritative command.
                    flow.Progress.Crafted = flow.settings.RequiredMask;
                    foreach (var actor in flow.Actors)
                    {
                        var kind = actor.Slot == 0 ? PlayerAbilityKind.Sap : PlayerAbilityKind.Leaf;
                        var potion = flow.settings.items.First(x => x.type == StageItemType.Potion && x.ability == kind);
                        flow.Progress.Held[actor.Slot] = flow.settings.ItemId(potion);
                        Check(flow.Execute(actor, StageCommand.Drink) == flow.settings.successMessage, "consume potion");
                    }
                    await Signal("abilities");
                    var tree = GameObject.Find("Seonang_Tripo").GetComponent<Collider>();
                    RaycastHit top = default; bool found = false;
                    for (int x = 1; x < 10 && !found; x++)
                    for (int z = 1; z < 10 && !found; z++)
                    {
                        var b = tree.bounds;
                        var p = new Vector3(Mathf.Lerp(b.min.x, b.max.x, x / 10f), b.max.y + 2, Mathf.Lerp(b.min.z, b.max.z, z / 10f));
                        found = tree.Raycast(new Ray(p, Vector3.down), out top, b.size.y + 4) && top.normal.y > .75f;
                    }
                    Check(found, "tree walkable surface");
                    foreach (var actor in flow.Actors)
                    {
                        Teleport(actor, top.point + Vector3.up * 2);
                        await Wait(() => Net(actor).Grounded, "tree landing slot " + actor.Slot, 10);
                        Check(actor.transform.position.y > top.point.y - 1, "tree does not fall through");
                        Log("Tree landing slot=" + actor.Slot + " at=" + actor.transform.position);
                        Teleport(actor, flow.sources[0].Point + Vector3.right * (actor.Slot * 3));
                    }
                    var cloths = Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None);
                    Check(cloths.Length > 0, "authored cloths");
                    foreach (var cloth in cloths)
                    {
                        Check(cloth.Object != null && cloth.Object.IsValid, "cloth attached to Fusion " + cloth.name);
                        var knot = Read<Transform>(cloth, "_knot"); Check(knot != null, "cloth grip");
                    }
                    var clothTest = cloths.First();
                    foreach (var actor in flow.Actors)
                    {
                        var cc = actor.GetComponent<CharacterController>();
                        Teleport(actor, Read<Transform>(clothTest, "_knot").position - actor.transform.TransformVector(cc.center));
                        Physics.SyncTransforms();
                        string feedback;
                        Check(HangingCloth.TryInteract(actor, out feedback) && HangingCloth.IsRiding(actor), "cloth grab slot " + actor.Slot);
                    }
                    await Signal("cloth-riders");
                    await Task.Delay(2000);
                    foreach (var actor in flow.Actors) { string feedback; Check(HangingCloth.TryRelease(actor, out feedback), "cloth release"); }
                    await Wait(() => flow.Actors.All(a => !Net(a).Transported), "cloth transport released");
                    Log("All cloths network attached=" + cloths.Length + "; paired grab/release PASS");
                    var pads = Object.FindObjectsByType<ExteriorJumpPad>(FindObjectsSortMode.None).Where(p => p.isActiveAndEnabled).ToArray();
                    foreach (var pad in pads)
                    {
                        Check(pad.Object != null && pad.Object.IsValid, "pad attached to Fusion " + pad.name);
                        Check(Read<SapInjectionPort[]>(pad, "_ports").All(p => p != null), "pad ports " + pad.name);
                        Check(Read<LeafInstallTarget[]>(pad, "_sockets").All(s => s != null && s.HasSocket), "pad sockets " + pad.name);
                    }
                    var padTest = pads.OrderBy(p => p.transform.position.y).First();
                    var ports = Read<SapInjectionPort[]>(padTest, "_ports");
                    int output = ports.Length == 1 ? 0 : 1;
                    var socket = Read<LeafInstallTarget[]>(padTest, "_sockets")[output];
                    var leafActor = flow.Actors[1];
                    Teleport(leafActor, socket.SocketPosition + Vector3.back * 2);
                    Physics.SyncTransforms();
                    Check(leafActor.Abilities.Leaf.TryThrowAtTarget(new Ray(socket.SocketPosition + Vector3.up * 3, Vector3.down)), "pad leaf throw");
                    await Wait(() => leafActor.Abilities.Leaf.ActiveCount > 0, "leaf released", 8);
                    var leaf = Read<System.Collections.Generic.List<LeafProjectile>>(leafActor.Abilities.Leaf, "leaves").Last();
                    if (!leaf.Installed) Check(leaf.TryInstallAtSocket(socket), "install pad leaf");
                    await Task.Delay(400);
                    var surface = Read<Collider>(leaf, "_surface");
                    var leafBounds = surface.bounds;
                    foreach (var actor in flow.Actors)
                    {
                        var origin = new Vector3(leafBounds.center.x + (actor.Slot == 0 ? -.6f : .6f), leafBounds.max.y + 3, leafBounds.center.z);
                        RaycastHit hit;
                        Check(surface.Raycast(new Ray(origin, Vector3.down), out hit, leafBounds.size.y + 6), "leaf support ray");
                        Teleport(actor, hit.point + Vector3.up * .5f);
                    }
                    var settings = Read<SapJumpPadSettings>(padTest, "_settings");
                    await Wait(() => flow.Actors.All(a => leaf.Supports(a.GetComponent<CharacterController>(), settings.standingTolerance)), "paired leaf riders", 12);
                    ports[0].Inject(settings.chargeUnits + .1f);
                    await Wait(() => flow.Actors.All(a => Net(a).ExternalFlight), "paired pad eruption", 12);
                    Check(flow.Actors.All(a => Net(a).SimulationVelocity.y > 0), "both pad riders launched upward");
                    Log("Pad network references=" + pads.Length + "; leaf and paired launch PASS");
                    leaf.BeginReturn();
                    foreach (var actor in flow.Actors) Teleport(actor, flow.sources[0].Point + Vector3.right * (actor.Slot * 3));
                    await Signal("stage1-checked");
                    // Exercise the actual summit/injection/paired-exit gate and Fusion scene loader.
                    var exterior = Object.FindFirstObjectByType<ExteriorStageFlow>();
                    if (exterior != null)
                    {
                        var arrival = Read<StageArrivalZone>(exterior, "_top");
                        foreach (var actor in flow.Actors) Teleport(actor, arrival.PlayerArrivalPoint + Vector3.right * actor.Slot);
                        await Wait(() => Read<uint>(exterior, "_arrived") == 3, "paired summit");
                        Read<SapInjectionPort>(exterior, "_inlet").Inject(3.1f);
                        await Wait(() => exterior.CanExit && flow.GateOpen, "entrance opens");
                    }
                    Check(flow.GateOpen, "current scene exit prerequisites");
                    var inside = flow.interior.transform.TransformPoint(flow.interior.center) - flow.settings.interactionOffset;
                    foreach (var actor in flow.Actors) Teleport(actor, inside + Vector3.right * actor.Slot);
                }
                else
                {
                    await Wait(() => Status("host.txt") == "abilities", "abilities checkpoint");
                    await Wait(() => StageActor.All.Count(a => a.Available && a.Abilities.Unlocked) == 2, "ability replication");
                    Set("client.txt", "abilities");
                    await Wait(() => Status("host.txt") == "cloth-riders", "cloth checkpoint");
                    await Wait(() => StageActor.All.Count(a => a.Available && HangingCloth.IsRiding(a)) == 2, "two cloth riders replicated");
                    Set("client.txt", "cloth-riders");
                    await Wait(() => Status("host.txt") == "stage1-checked", "stage1 checkpoint");
                    Set("client.txt", "stage1-checked");
                }
                await Wait(() => Object.FindFirstObjectByType<DescendingMazeFlow>() != null && NetworkPlayer.Local != null && session.State == LobbyState.Playing, "Stage2 transition");
                Check(NetworkPlayer.Local.GetComponent<PlayerAbilityController>().Kind == (host ? PlayerAbilityKind.Leaf : PlayerAbilityKind.Sap), "ability retained");
                var mazeFlow = Object.FindFirstObjectByType<DescendingMazeFlow>();
                var boards = Read<SapMazeBoard[]>(mazeFlow, "_mazes");
                Check(boards.Length == 3, "three mazes");
                if (!host)
                {
                    // A hidden automation client has no OS focus; bypass only its focus guard.
                    // Keep the real Input System actions, Fusion input and authoritative routing.
                    var cursor = Object.FindFirstObjectByType<Herbalist.Presentation.GameplayCursor>();
                    if (cursor != null) cursor.enabled = false;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
                    InputSystem.EnableDevice(keyboard); InputSystem.EnableDevice(mouse);
                }
                for (int i = 0; i < boards.Length; i++)
                {
                    await Wait(() => mazeFlow.CurrentBoard == boards[i], "maze station " + i);
                    var board = boards[i];
                    Check(board.Surface != null && board.Surface.Ready, "maze surface built " + i);
                    await MazeLeakNetworkChecks.Run(host, board, i, _folder);
                    if (host)
                    {
                        await Signal("maze-control-" + i);
                        await Wait(() => board.Controlling, "remote R " + i, 20);
                        await Wait(() => Status("client.txt") == "maze-far-" + i, "remote far target ready");
                        var parked = board.SapPosition;
                        Set("host.txt", "maze-far-ready-" + i);
                        await Wait(() => Status("client.txt") == "maze-far-done-" + i, "remote far target sent");
                        Check(Vector2.Distance(parked, board.SapPosition) < .001f, "remote target beyond 3m rejected by Host");
                        Set("host.txt", "maze-near-ready-" + i);
                        Debug.Log("[MazeLeakCheck] Remote 3m range rejection PASS maze=" + i);
                        var before = board.SapPosition;
                        await Wait(() => Vector2.Distance(before, board.SapPosition) > .025f, "remote pointer movement " + i, 25);
                        Set("host.txt", "maze-moved-" + i);
                        await Wait(() => Status("client.txt") == "maze-stopped-" + i && !board.Controlling, "remote control release " + i);
                        Log("Remote R, pointer movement, exit PASS maze=" + i);
                        // Accelerate solved status only, to isolate all raft and scene transitions.
                        var state = Read<MazeRunState>(board, "_state"); state.Phase = MazePhase.Solved;
                        board.GetType().GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(board, state);
                    }
                    else
                    {
                        await Wait(() => Status("host.txt") == "maze-control-" + i, "maze control checkpoint");
                        Set("client.txt", "maze-control-" + i);
                        await Press(keyboard, Key.R);
                        await Wait(() => board.Controlling, "local R " + i, 20);
                        var path = new System.Collections.Generic.List<Vector2>();
                        Check(board.Surface.TryFindPath(board.SapPosition, board.SurfaceGoal, path) && path.Count > 1, "maze path " + i);
                        var target = path.Count > 8 ? path[8] : Vector2.MoveTowards(board.SapPosition, path[path.Count - 1], .35f);
                        var camera = NetworkPlayer.Local.GetComponent<PlayerController>().View.Camera;
                        var goal = board.SurfaceGoal;
                        var farPixel = camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(goal.x, goal.y, board.Surface.SurfaceZ(goal, 0))));
                        Set("client.txt", "maze-far-" + i);
                        await Wait(() => Status("host.txt") == "maze-far-ready-" + i, "Host ready for distant click");
                        InputSystem.QueueStateEvent(mouse, new MouseState { position = farPixel, buttons = 1 });
                        await Task.Delay(750);
                        Set("client.txt", "maze-far-done-" + i);
                        await Wait(() => Status("host.txt") == "maze-near-ready-" + i, "distant click rejected");
                        var pixel = camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(target.x, target.y, board.Surface.SurfaceZ(target, 0))));
                        InputSystem.QueueStateEvent(mouse, new MouseState { position = pixel, buttons = 1 });
                        await Wait(() => Status("host.txt") == "maze-moved-" + i, "host observed pointer " + i, 30);
                        InputSystem.QueueStateEvent(mouse, new MouseState { position = pixel });
                        await Press(keyboard, Key.R);
                        await Wait(() => !board.Controlling, "local control released");
                        Set("client.txt", "maze-stopped-" + i);
                    }
                    Log("Maze PASS " + i + " volume=" + board.RemainingVolume);
                }
                await Wait(() => Object.FindFirstObjectByType<BranchRideFlow>() != null && NetworkPlayer.Local != null && session.State == LobbyState.Playing, "Stage3 branch ride transition");
                Check(NetworkPlayer.Local.GetComponent<PlayerAbilityController>().Kind == (host ? PlayerAbilityKind.Leaf : PlayerAbilityKind.Sap), "ability retained Stage3");
                Log("ALL PASS " + (host ? "host" : "client"));
                Set(host ? "host-result.txt" : "client-result.txt", "PASS");
            }
            catch (Exception e) { Log("FAIL " + (host ? "host " : "client ") + e); Set(host ? "host-result.txt" : "client-result.txt", e.ToString()); }
            finally
            {
                if (keyboard != null) InputSystem.RemoveDevice(keyboard);
                if (mouse != null) InputSystem.RemoveDevice(mouse);
                _running = false;
            }
        }
    }
}
#endif
