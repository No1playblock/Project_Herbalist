#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Tests
{
    // Called only by the explicit two-process traversal check. Positions/volume are test fixtures.
    public static class MazeLeakNetworkChecks
    {
        private static T Read<T>(object obj, string name) => (T)obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        private static void Write(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
        private static void Check(bool ok, string message) { if (!ok) throw new Exception("Maze leak: " + message); }
        private static async Task Wait(Func<bool> predicate, string label)
        {
            float end = Time.realtimeSinceStartup + 30;
            while (!predicate() && Time.realtimeSinceStartup < end) await Task.Delay(50);
            Check(predicate(), "timeout " + label);
        }
        private static string Status(string folder, string side) => File.Exists(Path.Combine(folder, side + ".txt")) ? File.ReadAllText(Path.Combine(folder, side + ".txt")) : "";
        private static bool Visible(MazeLeakSocket leak) => Read<GameObject>(leak, "_stream").activeInHierarchy;
        private static void WaterAt(SapMazeBoard board, Vector2 point)
        {
            var state = SapMazeSimulation.Initial(board.Definition);
            state.Volume = 1000;
            Write(board, "_state", state); Write(board, "_surfacePosition", point); Write(board, "_trailCount", 0);
            board.Tick(.01f);
        }
        private static void CheckMesh(MazeLeakSocket leak)
        {
            var effect = Read<GameObject>(leak, "_stream").GetComponent<MazeLeakStreamPresentation>();
            Check(effect != null && effect.gameObject.layer == 12, "back-side effect component/layer");
            var mesh = effect.GetComponent<MeshFilter>().sharedMesh;
            Check(mesh != null && mesh.vertexCount > 100 && mesh.bounds.size.sqrMagnitude > .01f, "generated water tube");
            var drops = effect.GetComponentInChildren<ParticleSystem>();
            Check(drops != null && drops.particleCount > 0, "ability droplets emitted");
        }
        private static void Capture(MazeLeakSocket leak, string file)
        {
            var effect = Read<GameObject>(leak, "_stream").transform;
            var go = new GameObject("LeakCheckCamera");
            var camera = go.AddComponent<Camera>(); camera.enabled = false;
            var target = new RenderTexture(800, 600, 24);
            var image = new Texture2D(800, 600, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                var focus = effect.position + effect.forward * 1.7f + Vector3.down * 1.1f;
                go.transform.position = focus + effect.forward * 8 + effect.right * 4 + Vector3.up * 1.5f;
                go.transform.LookAt(focus);
                camera.cullingMask = 1 << 12; camera.nearClipPlane = .05f; camera.farClipPlane = 150;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.07f, .09f, .1f);
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); image.Apply(); File.WriteAllBytes(file, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null;
                UnityEngine.Object.Destroy(go); UnityEngine.Object.Destroy(image); target.Release(); UnityEngine.Object.Destroy(target);
            }
        }
        public static async Task Run(bool host, SapMazeBoard board, int maze, string folder)
        {
            Check(board.Definition.limitPointerPathDistance && Mathf.Approximately(board.Definition.maxTargetPathDistance, 3), "3m limit is authored");
            for (int i = 0; i < board.Leaks.Length; i++)
            {
                var leak = board.Leaks[i]; string wet = "leak-wet-" + maze + "-" + i, dry = "leak-dry-" + maze + "-" + i;
                if (host)
                {
                    WaterAt(board, Read<Vector2[]>(board, "_surfaceLeaks")[i]);
                    await Wait(() => Visible(leak), wet); await Task.Delay(250); CheckMesh(leak);
                    File.WriteAllText(Path.Combine(folder, "host.txt"), wet);
                    await Wait(() => Status(folder, "client") == wet, "client " + wet);
                    if (i == 0) Capture(leak, Path.Combine(folder, "host-maze-" + maze + ".png"));
                    WaterAt(board, board.SurfaceStart);
                    await Wait(() => !board.AnyLeak, dry);
                    File.WriteAllText(Path.Combine(folder, "host.txt"), dry);
                    await Wait(() => Status(folder, "client") == dry, "client " + dry);
                }
                else
                {
                    await Wait(() => Status(folder, "host") == wet, wet);
                    await Wait(() => Visible(leak), "replicated " + wet); await Task.Delay(250); CheckMesh(leak);
                    if (i == 0) Capture(leak, Path.Combine(folder, "client-maze-" + maze + ".png"));
                    File.WriteAllText(Path.Combine(folder, "client.txt"), wet);
                    await Wait(() => Status(folder, "host") == dry, dry);
                    await Wait(() => !board.AnyLeak && board.Leaks.All(l => !Visible(l)), "replicated " + dry);
                    File.WriteAllText(Path.Combine(folder, "client.txt"), dry);
                }
            }
            string blocked = "leak-blocked-" + maze;
            if (host)
            {
                var actor = StageActor.All.First(a => a.Available && a.Abilities.Kind == PlayerAbilityKind.Leaf);
                var leak = board.Leaks[0];
                actor.GetComponent<NetworkPlayer>().TeleportAuthoritatively(leak.Target.SocketPosition + Vector3.up + Vector3.right);
                Check(actor.Abilities.Leaf.TryThrowAtSocket(leak.Target), "throw at leak");
                await Wait(() => actor.Abilities.Leaf.ActiveCount > 0, "leaf released");
                var leaf = Read<System.Collections.Generic.List<LeafProjectile>>(actor.Abilities.Leaf, "leaves").Last();
                if (!leaf.Installed) Check(leaf.TryInstallAtSocket(leak.Target), "install leak leaf");
                WaterAt(board, Read<Vector2[]>(board, "_surfaceLeaks")[0]);
                await Wait(() => leak.Blocked && !Visible(leak) && !board.AnyLeak, "leaf blocks stream");
                File.WriteAllText(Path.Combine(folder, "host.txt"), blocked);
                await Wait(() => Status(folder, "client") == blocked, "replicated blockage");
                leaf.BeginReturn();
                WaterAt(board, board.SurfaceStart);
                Write(board, "_state", SapMazeSimulation.Initial(board.Definition)); board.Tick(.01f);
                var sapActor = StageActor.All.First(a => a.Available && a.Abilities.Kind == PlayerAbilityKind.Sap);
                Check(board.ReceiveAbilityCycle(sapActor.Abilities, 1, false, default), "enter range fixture");
                Check(board.Controlling, "range fixture controls board");
                var route = new System.Collections.Generic.List<Vector2>();
                Check(board.Surface.TryFindPath(board.SapPosition, board.SurfaceGoal, route) && route.Count > 8, "range fixture route");
                var before = board.SapPosition;
                board.ReceiveAbilityPointer(sapActor.Abilities, true, true, route[route.Count - 1]); board.Tick(.1f);
                Check(Vector2.Distance(before, board.SapPosition) < .001f, "Host rejects target beyond 3m");
                board.ReceiveAbilityPointer(sapActor.Abilities, true, true, route[8]); board.Tick(.1f);
                Check(Vector2.Distance(before, board.SapPosition) > .002f, "Host accepts nearby target within 3m");
                board.ReceiveAbilityCycle(sapActor.Abilities, 1, false, default);
                WaterAt(board, board.SurfaceStart);
                Write(board, "_state", SapMazeSimulation.Initial(board.Definition)); board.Tick(.01f);
                Debug.Log("[MazeLeakCheck] Host range rejection/acceptance PASS maze=" + maze);
            }
            else
            {
                await Wait(() => Status(folder, "host") == blocked, blocked);
                await Wait(() => board.Leaks[0].Blocked && !Visible(board.Leaks[0]) && !board.AnyLeak, "leaf blockage replicated");
                File.WriteAllText(Path.Combine(folder, "client.txt"), blocked);
            }
            Debug.Log("[MazeLeakCheck] PASS " + (host ? "host" : "client") + " maze=" + maze + " holes=" + board.Leaks.Length);
        }
    }
}
#endif
