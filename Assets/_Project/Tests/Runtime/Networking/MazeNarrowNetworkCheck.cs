#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.Presentation;
using Herbalist.StageOne;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace Herbalist.Tests
{
    // Explicit two-process regression. Seed once per segment; all movement after that uses
    // synthetic local Mouse -> ordinary Input System -> Fusion input -> authoritative Tick.
    public static class MazeNarrowNetworkCheck
    {
        private const string MazePath = "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity";
        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static bool _running;
        private static string _folder, _peer;
        private static T Read<T>(object obj, string name) => (T)obj.GetType().GetField(name, Flags).GetValue(obj);
        private static void Write(object obj, string name, object value) => obj.GetType().GetField(name, Flags).SetValue(obj, value);
        private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Log(string text) => Debug.Log("[MazeNarrow] " + _peer + " " + text);
        private static string Status(string name) { string path = Path.Combine(_folder, name + ".txt"); return File.Exists(path) ? File.ReadAllText(path) : ""; }
        private static void Set(string name, string text) => File.WriteAllText(Path.Combine(_folder, name + ".txt"), text);
        private static async Task Wait(Func<bool> predicate, string message, float timeout = 120)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!predicate() && Time.realtimeSinceStartup < end) await Task.Delay(50);
            Check(predicate(), "Timeout " + message);
        }
        private static string Hash(SapMazeFluidField fluid)
        {
            var packed = Enumerable.Range(0, fluid.ActiveCells.Count).Select(fluid.Pack).OrderBy(x => x).ToArray();
            var bytes = new byte[packed.Length * 4]; Buffer.BlockCopy(packed, 0, bytes, 0, bytes.Length);
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, "-herbalist-maze-narrow-check");
            if (i >= 0 && i + 4 < args.Length && !_running) Run(args[i + 1] == "host", args[i + 2], args[i + 3], int.Parse(args[i + 4]));
        }

        private static async Task<int> Drive(StageActor actor, SapMazeBoard board, List<Vector2> route,
            int from, int to, Mouse mouse, Transform anchor)
        {
            int direction = Math.Sign(to - from), index = from;
            int packets = 0; float deadline = Time.realtimeSinceStartup + 45;
            var center = board.transform.TransformPoint(new Vector3(route[(from + to) / 2].x,
                route[(from + to) / 2].y, board.Surface.SurfaceZ(route[(from + to) / 2], 0)));
            anchor.position = center - board.transform.forward * 25;
            actor.GetComponent<Herbalist.Player.PlayerController>().View.SetCameraOverride(typeof(MazeNarrowNetworkCheck), anchor);
            while (true)
            {
                index = direction > 0 ? Math.Min(index + 8, to) : Math.Max(index - 8, to);
                var point = route[index];
                anchor.LookAt(board.transform.TransformPoint(new Vector3(point.x, point.y, board.Surface.SurfaceZ(point, 0))));
                InputSystem.QueueStateEvent(mouse, new MouseState { position = actor.Camera.pixelRect.center, buttons = 1 });
                await Task.Delay(100);
                var sampledInput = NetworkPlayer.Local.ReadLocalInput();
                if (sampledInput.AbilityHeld && sampledInput.MazePointerValid &&
                    Vector2.Distance(sampledInput.MazePointer, point) < board.Surface.CellSize * 2) packets++;
                float waypointEnd = Time.realtimeSinceStartup + 8;
                while (board.Fluid.Distance(point) >= board.Surface.CellSize * 1.5f)
                {
                    var input = NetworkPlayer.Local.ReadLocalInput();
                    if (input.AbilityHeld && input.MazePointerValid && Vector2.Distance(input.MazePointer, point) < board.Surface.CellSize * 2) packets++;
                    Check(Time.realtimeSinceStartup < waypointEnd && Time.realtimeSinceStartup < deadline,
                        "stalled waypoint=" + index + " distance=" + board.Fluid.Distance(point) + " controlling=" + board.Controlling +
                        " held=" + input.AbilityHeld + " pointer=" + input.MazePointerValid + " aim=" + input.MazePointer + " phase=" + board.State.Phase);
                    await Task.Delay(30);
                }
                if (index == to) break;
            }
            InputSystem.QueueStateEvent(mouse, new MouseState { position = actor.Camera.pixelRect.center });
            await Task.Delay(200);
            Check(packets > 0, "actual held pointer inputs observed");
            Log("INPUT PASS " + from + " -> " + to + " packets=" + packets);
            return packets;
        }

        private static void Capture(Camera camera, string path)
        {
            var target = new RenderTexture(800, 600, 24); var image = new Texture2D(800, 600, TextureFormat.RGB24, false);
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active; var oldRect = camera.rect;
            try
            {
                camera.rect = new Rect(0, 0, 1, 1); camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); image.Apply(); File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { camera.targetTexture = oldTarget; camera.rect = oldRect; RenderTexture.active = oldActive; Object.Destroy(image); target.Release(); Object.Destroy(target); }
        }

        public static async void Run(bool host, string room, string folder, int hostSlot)
        {
            if (_running) return; _running = true; _folder = folder; _peer = host ? "host" : "client";
            Directory.CreateDirectory(folder); Application.runInBackground = true; Application.targetFrameRate = 45;
            Mouse mouse = null;
            try
            {
                await Wait(() => FusionLobbySession.Instance != null, "menu");
                var session = FusionLobbySession.Instance; var settings = Object.Instantiate(Read<LobbySettings>(session, "settings"));
                settings.playScenePath = MazePath; Write(settings, "_grantPrototypeAbilitiesOnDirectStart", true); Write(session, "settings", settings);
                await session.ConnectAsync(room, host); Check(session.State != LobbyState.Error, session.Message);
                await Wait(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "two peers");
                Herbalist.GameUI.RoomControl.Instance.Select(host ? hostSlot : 1 - hostSlot);
                if (host) { await Wait(() => Herbalist.GameUI.RoomControl.Instance.Ready, "roles"); Herbalist.GameUI.RoomControl.Instance.StartGame(); }
                await Wait(() => NetworkPlayer.Local != null && StageLevel.Instance != null && session.State == LobbyState.Playing &&
                    StageActor.All.Count(a => a.Available && a.Abilities.Unlocked) == 2, "Stage2 actors");
                var actor = StageActor.All.First(a => a.Local); bool controller = actor.Abilities.Kind == PlayerAbilityKind.Sap;
                var flow = Object.FindFirstObjectByType<DescendingMazeFlow>(); var board = Read<SapMazeBoard[]>(flow, "_mazes")[2];
                await Wait(() => board.Object != null && board.Object.IsValid && board.Fluid != null && board.Surface.Ready, "attached maze3");
                Log("CONNECTED slot=" + actor.Slot + " controller=" + controller + " peers=" + session.ConnectedCount);
                if (controller)
                {
                    // Hidden players have no OS focus. Preserve actions and Fusion input; bypass only the focus guard.
                    var cursor = Object.FindFirstObjectByType<GameplayCursor>(); if (cursor != null) cursor.enabled = false;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    mouse = InputSystem.AddDevice<Mouse>(); InputSystem.EnableDevice(mouse);
                }
                if (host)
                {
                    session.Runner.SetIsSimulated(flow.Object, false); Write(flow, "_current", 2); Write(flow, "_phase", 3); flow.Tick(0);
                    var rafts = Read<Transform[]>(flow, "_rafts");
                    foreach (var item in StageActor.All.Where(a => a.Available))
                        item.GetComponent<NetworkPlayer>().SetTransportAuthoritatively(true,
                            item.Abilities.Kind == PlayerAbilityKind.Sap ? board.ControlArea.PlayerArrivalPoint : rafts[1].position + Vector3.up);
                    flow.Tick(0);
                }
                await Wait(() => flow.CurrentMaze == 2 && board.Active && board.Controlling, "maze3 controller");
                var route = new List<Vector2>(); Check(board.Surface.TryFindPath(board.SurfaceStart, board.SurfaceGoal, route) && route.Count > 800, "maze route");
                var anchor = new GameObject("NarrowInputCameraAnchor").transform;
                var cases = new[] { new[] { 360, 540 }, new[] { 540, 360 }, new[] { 600, 760 }, new[] { 760, 600 } };
                var results = new List<object>();
                for (int c = 0; c < cases.Length; c++)
                {
                    int from = cases[c][0], to = cases[c][1]; string token = "case" + c;
                    if (host)
                    {
                        session.Runner.SetIsSimulated(flow.Object, false);
                        var state = SapMazeSimulation.Initial(board.Definition); Write(board, "_state", state);
                        Write(board, "_surfacePosition", route[from]); Write(board, "_hasTarget", false);
                        board.Fluid.Reset(board.Surface.GridCell(route[from]), board.Definition.initialVolume); board.Tick(0);
                        session.Runner.SetIsSimulated(flow.Object, true); Set("phase", token + "-start");
                    }
                    else await Wait(() => Status("phase") == token + "-start", token + " seeded");
                    await Task.Delay(200);
                    int inputPackets = 0;
                    if (controller)
                    {
                        inputPackets = await Drive(actor, board, route, from, to, mouse, anchor);
                        Set("driver", token + "-done");
                    }
                    if (host)
                    {
                        await Wait(() => Status("driver") == token + "-done", "pointer traversal " + token, 60);
                        session.Runner.SetIsSimulated(flow.Object, false); await Task.Delay(100);
                        Check(board.Fluid.Distance(route[to]) < board.Surface.CellSize * 1.5f, "authoritative arrival " + token);
                        File.WriteAllText(Path.Combine(folder, "expected.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new { hash = Hash(board.Fluid), count = board.Fluid.ActiveCells.Count }));
                        Set("phase", token + "-check");
                    }
                    else await Wait(() => Status("phase") == token + "-check", token + " snapshot");
                    var expected = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(folder, "expected.json")));
                    string expectedHash = (string)expected["hash"];
                    await Wait(() => Hash(board.Fluid) == expectedHash, "identical replicated cells " + token, 20);
                    Check(board.Fluid.ActiveCells.All(board.Surface.GridOpen), "no wall penetration " + token);
                    Check(board.Fluid.Distance(route[to]) < board.Surface.CellSize * 1.5f, "peer arrival " + token);
                    await Task.Delay(150);
                    if (controller) Capture(actor.Camera, Path.Combine(folder, _peer + "-" + token + ".png"));
                    var view = board.GetComponentInChildren<SapMazeTrailView>(true); var mesh = Read<Mesh>(view, "_mesh");
                    Check(mesh != null && mesh.vertexCount > 0, "water mesh " + token);
                    results.Add(new { c, from, to, controller, inputPackets, hash = expectedHash, cells = board.Fluid.ActiveCells.Count,
                        volume = board.RemainingVolume, distance = board.Fluid.Distance(route[to]), wallCells = 0, meshVertices = mesh.vertexCount });
                    Log("PASS " + token + " cells=" + board.Fluid.ActiveCells.Count + " hash=" + expectedHash); Set(_peer, token + "-pass");
                    if (host) await Wait(() => Status("client") == token + "-pass", "client checkpoint " + token);
                    else await Wait(() => Status("phase") != token + "-check" || Status("host") == "PASS", "next case");
                }
                File.WriteAllText(Path.Combine(folder, _peer + "-result.json"), Newtonsoft.Json.JsonConvert.SerializeObject(results, Newtonsoft.Json.Formatting.Indented));
                Set(_peer, "PASS"); if (host) await Wait(() => Status("client") == "PASS", "client final");
                Log("ALL PASS actual pointer/Fusion movement cases=" + results.Count); Application.Quit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error); Set(_peer, "FAIL " + error.Message);
                File.WriteAllText(Path.Combine(folder, _peer + "-failure.txt"), error.ToString()); Application.Quit(1);
            }
            finally { if (mouse != null) InputSystem.RemoveDevice(mouse); _running = false; }
        }
    }
}
#endif
