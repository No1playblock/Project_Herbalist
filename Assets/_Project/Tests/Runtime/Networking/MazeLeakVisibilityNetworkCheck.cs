#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Tests
{
    // Explicit development-only two-process check. Only positions/volume are fixtures;
    // controller assignment, leak detection, snapshots, camera masks and effects are production code.
    public static class MazeLeakVisibilityNetworkCheck
    {
        private const string MazePath = "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity";
        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static bool _running;
        private static string _folder, _peer;
        private static SapMazeBoard _fixture;
        private static Vector2 _waterPoint;
        private static T Read<T>(object obj, string field) => (T)obj.GetType().GetField(field, Flags).GetValue(obj);
        private static void Write(object obj, string field, object value) => obj.GetType().GetField(field, Flags).SetValue(obj, value);
        private static void Check(bool value, string text) { if (!value) throw new Exception(text); }
        private static void Log(string text) => Debug.Log("[MazeVisibility] " + _peer + " " + text);
        private static string Status(string name) { var p = Path.Combine(_folder, name + ".txt"); return File.Exists(p) ? File.ReadAllText(p) : ""; }
        private static void Set(string name, string value) => File.WriteAllText(Path.Combine(_folder, name + ".txt"), value);
        private static async Task Wait(Func<bool> predicate, string label, float seconds = 120)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline) await Task.Delay(50);
            Check(predicate(), "Timeout: " + label);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, "-herbalist-maze-visibility-check");
            if (i >= 0 && i + 4 < args.Length && !_running)
                Run(args[i + 1] == "host", args[i + 2], args[i + 3], int.Parse(args[i + 4]));
        }

        private static async Task Pump()
        {
            while (_running)
            {
                if (_fixture != null)
                {
                    var state = SapMazeSimulation.Initial(_fixture.Definition); state.Volume = 2;
                    Write(_fixture, "_state", state); Write(_fixture, "_surfacePosition", _waterPoint);
                    Write(_fixture, "_hasTarget", false);
                    _fixture.Fluid.Reset(_fixture.Surface.GridCell(_waterPoint), 2);
                    _fixture.Tick(.01f);
                }
                await Task.Delay(30);
            }
        }

        private static GameObject Splash(MazeLeakSocket socket) => Read<GameObject>(socket, "_approachSplash");
        private static GameObject Stream(MazeLeakSocket socket) => Read<GameObject>(socket, "_stream");

        private static Vector2 NearbyDryPoint(SapMazeBoard board, Vector2 hole)
        {
            var path = new List<Vector2>();
            Check(board.Surface.TryFindPath(hole, board.SurfaceStart, path), "nearby fixture route");
            for (int i = Math.Min(24, path.Count - 1); i > 0; i--)
            {
                board.Fluid.Reset(board.Surface.GridCell(path[i]), 2);
                float distance = board.Fluid.Distance(hole) * board.transform.TransformVector(Vector3.right).magnitude;
                if (!board.Fluid.Contains(hole) && distance > .1f && distance < 2.8f) return path[i];
            }
            throw new Exception("No nearby dry warning fixture");
        }

        // Render from the real local player camera; retain its actual per-peer culling mask.
        private static int CaptureDifference(Camera camera, GameObject effect, string file)
        {
            var target = new RenderTexture(800, 600, 24);
            var image = new Texture2D(800, 600, TextureFormat.RGB24, false);
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active; var oldRect = camera.rect;
            bool wasActive = effect.activeSelf;
            try
            {
                camera.rect = new Rect(0, 0, 1, 1); camera.targetTexture = target;
                camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); image.Apply();
                var on = image.GetPixels32(); File.WriteAllBytes(file, image.EncodeToPNG());
                effect.SetActive(false); camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); image.Apply(); var off = image.GetPixels32();
                int changed = 0;
                for (int i = 0; i < on.Length; i++)
                    if (Math.Abs(on[i].r - off[i].r) + Math.Abs(on[i].g - off[i].g) + Math.Abs(on[i].b - off[i].b) > 12) changed++;
                return changed;
            }
            finally
            {
                effect.SetActive(wasActive); camera.targetTexture = oldTarget; camera.rect = oldRect;
                RenderTexture.active = oldActive; Object.Destroy(image); target.Release(); Object.Destroy(target);
            }
        }

        private static void PointCamera(StageActor actor, MazeLeakSocket leak, Transform anchor, bool controller)
        {
            var effect = Stream(leak).transform;
            var focus = controller ? leak.transform.position : effect.position + effect.forward * 3 + Vector3.down * 2;
            anchor.position = focus + effect.forward * (controller ? -12 : 15) + effect.right * 3 + Vector3.up * 2;
            anchor.LookAt(focus);
            actor.GetComponent<Herbalist.Player.PlayerController>().View.SetCameraOverride(typeof(MazeLeakVisibilityNetworkCheck), anchor);
        }

        public static async void Run(bool host, string room, string folder, int hostSlot)
        {
            if (_running) return; _running = true; _folder = folder; _peer = host ? "host" : "client";
            Directory.CreateDirectory(folder); Application.runInBackground = true; Application.targetFrameRate = 45;
            var results = new List<object>();
            try
            {
                await Wait(() => FusionLobbySession.Instance != null, "menu");
                var session = FusionLobbySession.Instance;
                var settings = Object.Instantiate(Read<LobbySettings>(session, "settings"));
                settings.playScenePath = MazePath; Write(settings, "_grantPrototypeAbilitiesOnDirectStart", true);
                Write(session, "settings", settings); // Runtime copy: authored menu route is untouched.
                await session.ConnectAsync(room, host);
                Check(session.State != LobbyState.Error, session.Message);
                await Wait(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "two real peers");
                RoomControl.Instance.Select(host ? hostSlot : 1 - hostSlot);
                if (host) { await Wait(() => RoomControl.Instance.Ready, "role selection"); RoomControl.Instance.StartGame(); }
                await Wait(() => NetworkPlayer.Local != null && StageLevel.Instance != null && session.State == LobbyState.Playing &&
                    StageActor.All.Count(a => a.Available && a.Abilities.Unlocked) == 2, "Stage2 network actors");
                var local = StageActor.All.First(a => a.Local);
                bool controller = local.Abilities.Kind == PlayerAbilityKind.Sap;
                Check(local.Slot == (host ? hostSlot : 1 - hostSlot), "selected slot retained");
                Log("CONNECTED room=" + room + " slot=" + local.Slot + " controller=" + controller + " peers=" + session.ConnectedCount);
                var flow = Object.FindFirstObjectByType<DescendingMazeFlow>();
                var boards = Read<SapMazeBoard[]>(flow, "_mazes");
                await Wait(() => boards.All(b => b.Object != null && b.Object.IsValid && b.Fluid != null && b.Surface.Ready), "attached fluid boards");
                if (host)
                {
                    // Freeze automatic stage advancement, then drive ordinary authoritative board ticks.
                    session.Runner.SetIsSimulated(flow.Object, false);
                    _ = Pump();
                }
                var anchor = new GameObject("MazeVisibilityCameraAnchor").transform;
                for (int maze = 0; maze < boards.Length; maze++)
                {
                    var board = boards[maze];
                    if (host)
                    {
                        _fixture = null; Write(flow, "_current", maze); Write(flow, "_phase", 3);
                        flow.Tick(0);
                        var rafts = Read<Transform[]>(flow, "_rafts");
                        foreach (var actor in StageActor.All.Where(a => a.Available))
                        {
                            var point = actor.Abilities.Kind == PlayerAbilityKind.Sap ? board.ControlArea.PlayerArrivalPoint : rafts[1].position + Vector3.up;
                            actor.GetComponent<NetworkPlayer>().SetTransportAuthoritatively(true, point);
                        }
                        flow.Tick(0);
                    }
                    await Wait(() => flow.CurrentMaze == maze && board.Active && board.Controlling, "active maze and actual controller " + maze);
                    for (int hole = 0; hole < board.Leaks.Length; hole++)
                    {
                        var leak = board.Leaks[hole]; var center = Read<Vector2[]>(board, "_surfaceLeaks")[hole];
                        PointCamera(local, leak, anchor, controller);
                        foreach (var phase in new[] { "near", "wet", "dry" })
                        {
                            string token = maze + "-" + hole + "-" + phase;
                            if (host)
                            {
                                _fixture = null;
                                _waterPoint = phase == "near" ? NearbyDryPoint(board, center) : phase == "wet" ? center : board.SurfaceStart;
                                _fixture = board;
                                Set("phase", token);
                            }
                            else await Wait(() => Status("phase") == token, token);
                            await Wait(() => phase == "near" ? Splash(leak).activeInHierarchy && !Stream(leak).activeInHierarchy :
                                phase == "wet" ? Stream(leak).activeInHierarchy && !Splash(leak).activeInHierarchy :
                                !Stream(leak).activeInHierarchy && !Splash(leak).activeInHierarchy, "effect " + token, 30);
                            await Task.Delay(300);
                            int bit = 1 << 12;
                            Check(((local.Camera.cullingMask & bit) == 0) == controller, "camera visibility " + token);
                            int pixels = -1;
                            if (hole == 0 && phase != "dry")
                            {
                                var effect = phase == "near" ? Splash(leak) : Stream(leak);
                                if (phase == "near") foreach (var particle in effect.GetComponentsInChildren<ParticleSystem>()) particle.Simulate(.35f, false, false, true);
                                pixels = CaptureDifference(local.Camera, effect, Path.Combine(folder, _peer + "-maze" + maze + "-" + phase + ".png"));
                                Check(controller ? pixels < 20 : pixels > 20, "rendered pixels=" + pixels + " " + token);
                            }
                            if (phase == "wet")
                            {
                                var presentation = Stream(leak).GetComponent<MazeLeakStreamPresentation>();
                                var mesh = presentation.GetComponent<MeshFilter>().sharedMesh;
                                Check(mesh != null && mesh.vertexCount > 100 && mesh.bounds.size.sqrMagnitude > .01f, "water mesh " + token);
                                var path = Read<Vector3[]>(presentation, "_path");
                                Check(path.Last().y < path.First().y && Vector3.Dot(path.Last() - path.First(), presentation.transform.forward) > 1, "falling arc " + token);
                            }
                            results.Add(new { maze, hole, phase, controller, pixels, cameraMask = local.Camera.cullingMask, leakMask = board.AnyLeak });
                            Log("PASS " + token + " pixels=" + pixels);
                            Set(_peer, token);
                            if (host) await Wait(() => Status("client") == token, "client acknowledgement " + token);
                            else await Wait(() => Status("phase") != token || Status("host") == "PASS", "next phase " + token);
                        }
                    }
                }
                _fixture = null;
                File.WriteAllText(Path.Combine(folder, _peer + "-result.json"), Newtonsoft.Json.JsonConvert.SerializeObject(results, Newtonsoft.Json.Formatting.Indented));
                Set(_peer, "PASS");
                if (host) await Wait(() => Status("client") == "PASS", "client final");
                Log("ALL PASS cases=" + results.Count);
                _running = false; Application.Quit(0);
            }
            catch (Exception exception)
            {
                _running = false; _fixture = null;
                Debug.LogException(exception); Set(_peer, "FAIL " + exception.Message);
                File.WriteAllText(Path.Combine(folder, _peer + "-failure.txt"), exception.ToString());
                Application.Quit(1);
            }
        }
    }
}
#endif
