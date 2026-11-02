#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
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
    // Only enabled by the explicit command line flag in a two-process development check.
    public static class StageOneExitClothNetworkCheck
    {
        private static string _folder, _peer;
        private static void Check(bool value, string label) { if (!value) throw new Exception(label); }
        private static void Log(string label) => Debug.Log("[ExitClothCheck] " + label);
        private static void Set(string value) => File.WriteAllText(Path.Combine(_folder, _peer + "-status.txt"), value);
        private static string Status(string peer) { var p = Path.Combine(_folder, peer + "-status.txt"); return File.Exists(p) ? File.ReadAllText(p) : ""; }
        private static async Task Wait(Func<bool> predicate, string label, float seconds = 180)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline) await Task.Delay(100);
            Check(predicate(), "Timeout: " + label);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, "-herbalist-exit-cloth-check");
            if (i >= 0 && i + 4 < args.Length) Run(args[i + 1] == "host", args[i + 2], args[i + 3], int.Parse(args[i + 4]));
        }
        private static async void Run(bool host, string room, string folder, int firstSlot)
        {
            _folder = folder; _peer = host ? "host" : "client"; Directory.CreateDirectory(folder);
            Application.runInBackground = true;
            try
            {
                await Wait(() => FusionLobbySession.Instance != null, "menu");
                var session = FusionLobbySession.Instance;
                await session.ConnectAsync(room, host);
                await Wait(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "two peers");
                RoomControl.Instance.Select(host ? 1 : 0);
                if (host) { await Wait(() => RoomControl.Instance.Ready, "roles"); RoomControl.Instance.StartGame(); }
                await Wait(() => StageOneFlow.Instance != null && NetworkPlayer.Local != null && session.State == LobbyState.Playing, "Stage1");
                Log("REAL PHOTON " + _peer + " slot=" + NetworkPlayer.Local.Slot + " firstSlot=" + firstSlot);
                await Task.Delay(1500);
                var cloths = Object.FindObjectsByType<Cloth>();
                Check(cloths.Length == 9, "Nine active Cloths on " + _peer);
                Check(cloths.All(c => c.GetComponent<SkinnedMeshRenderer>().sharedMesh.isReadable), "Cloth meshes retain CPU data in Player build");
                Check(Object.FindObjectsByType<HangingCloth>().All(c => c.Object != null && c.Object.IsValid), "Network cloth objects attached");
                var pins = cloths.Select(c => c.vertices).ToArray();
                var frame = Time.frameCount;
                await Task.Delay(1500);
                Check(Time.frameCount > frame + 3, "Player frames advancing");
                for (int i = 0; i < cloths.Length; i++)
                {
                    var c = cloths[i]; var vertices = c.vertices; var coefficients = c.coefficients;
                    Check(vertices.All(v => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z)), "Finite cloth mesh");
                    int fixedCount = 0, moving = 0;
                    for (int n = 0; n < vertices.Length; n++)
                    {
                        float d = Vector3.Distance(pins[i][n], vertices[n]);
                        if (coefficients[n].maxDistance == 0) { fixedCount++; Check(d < .001f, "Cloth fixed point moved"); }
                        else if (d > .0001f) moving++;
                    }
                    Log("MOTION " + _peer + " " + c.transform.parent.parent.name + " fixed=" + fixedCount + " moving=" + moving + " enabled=" + c.enabled + " vertices=" + vertices.Length);
                    Check(fixedCount == 64 && moving > 100, "Cloth motion and pinned anchors: " + c.transform.parent.parent.name + " fixed=" + fixedCount + " moving=" + moving);
                }
                CaptureCloths(cloths, NetworkPlayer.Local.Player.View.Camera, host);
                Log("CLOTH PASS 9/9 visible, simulated, pinned on " + _peer);
                Set("cloth-ready");
                if (host)
                {
                    await Wait(() => Status("client") == "cloth-ready", "client cloth verified");
                    var flow = StageOneFlow.Instance; flow.RefreshActors();
                    Check(flow.Actors.All(a => a != null && a.Available), "Two authoritative actors");
                    flow.Progress.Crafted = flow.settings.RequiredMask;
                    foreach (var actor in flow.Actors)
                    {
                        var kind = actor.Slot == 0 ? PlayerAbilityKind.Sap : PlayerAbilityKind.Leaf;
                        var potion = flow.settings.items.First(x => x.type == StageItemType.Potion && x.ability == kind);
                        flow.Progress.Held[actor.Slot] = flow.settings.ItemId(potion);
                        Check(flow.Execute(actor, StageCommand.Drink) == flow.settings.successMessage, "Ordinary potion consumption");
                    }
                    Check(flow.GateOpen, "Actual scene exit prerequisites");
                    var box = flow.interior;
                    Vector3 Position(int slot, bool touching)
                    {
                        var actor = flow.Actors[slot]; var cc = actor.GetComponent<CharacterController>();
                        float radius = cc.radius * Mathf.Max(Mathf.Abs(actor.transform.lossyScale.x), Mathf.Abs(actor.transform.lossyScale.z));
                        var center = box.transform.TransformPoint(box.center);
                        float half = box.size.x * Mathf.Abs(box.transform.lossyScale.x) * .5f;
                        // Hold test bodies at the edge via the existing network transport state,
                        // so gravity/input cannot move the waiting actor out during observation.
                        var p = center + box.transform.right * (half + radius * (touching ? .5f : 4f)) + box.transform.forward * (slot == 0 ? -1 : 1);
                        return p - actor.transform.TransformVector(cc.center);
                    }
                    void Place(int slot, bool touching) => flow.Actors[slot].GetComponent<NetworkPlayer>().SetTransportAuthoritatively(true, Position(slot, touching));
                    Place(0, false); Place(1, false);
                    await Wait(() => flow.InsideCount == 0, "zero contacts");
                    Place(firstSlot, true);
                    await Wait(() => flow.InsideCount == 1, "first player contacts");
                    Set("first-waiting");
                    await Wait(() => Status("client") == "first-observed", "client observes one occupant");
                    await Task.Delay(3000);
                    Check(StageOneFlow.Instance == flow && flow.InsideCount == 1 && !flow.Progress.Cleared, "First waits 3 seconds without transition");
                    Log("WAIT PASS firstSlot=" + firstSlot + " one contact for 3 seconds");
                    // Leaving must not retain an old contact.
                    Place(firstSlot, false); Place(1 - firstSlot, true);
                    await Wait(() => flow.InsideCount == 1, "other player alone");
                    await Task.Delay(1500);
                    Check(!flow.Progress.Cleared, "Historical contact cannot clear");
                    Log("DEPARTURE PASS old contact not retained");
                    Place(1 - firstSlot, false);
                    await Wait(() => flow.InsideCount == 0, "reset both contacts");
                    Place(firstSlot, true);
                    await Wait(() => flow.InsideCount == 1, "first actor waits again");
                    await Task.Delay(3000);
                    Check(!flow.Progress.Cleared, "First actor still waits before second entry");
                    Log("SEQUENTIAL ENTRY firstSlot=" + firstSlot + " secondSlot=" + (1 - firstSlot));
                    Place(1 - firstSlot, true);
                    Log("SECOND CONTACT now; expecting real Fusion scene load");
                }
                else
                {
                    await Wait(() => Status("host") == "first-waiting", "first contact signal");
                    await Wait(() => StageOneFlow.Instance.InsideCount == 1, "replicated first occupancy");
                    Check(!StageOneFlow.Instance.Progress.Cleared, "Client remains in Stage1 with one player");
                    Log("CLIENT WAIT PASS replicated single occupant");
                    Set("first-observed");
                }
                await Wait(() => Object.FindAnyObjectByType<DescendingMazeFlow>() != null && NetworkPlayer.Local != null && session.State == LobbyState.Playing, "actual Stage2 load on " + _peer);
                Check(session.ConnectedCount == 2, "Both peers retained");
                Check(NetworkPlayer.Local.GetComponent<PlayerAbilityController>().Unlocked, "Potion ability retained");
                Log("ALL PASS " + _peer + " firstSlot=" + firstSlot + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
                File.WriteAllText(Path.Combine(folder, _peer + "-result.txt"), "PASS");
            }
            catch (Exception e)
            {
                Log("FAIL " + _peer + " " + e);
                File.WriteAllText(Path.Combine(folder, _peer + "-result.txt"), e.ToString());
            }
        }
        private static void CaptureCloths(Cloth[] cloths, Camera camera, bool host)
        {
            var all = Object.FindObjectsByType<Renderer>();
            var forceOff = all.Select(r => r.forceRenderingOff).ToArray();
            var terrains = Terrain.activeTerrains;
            var drawTerrain = terrains.Select(t => t.drawHeightmap).ToArray();
            var drawTrees = terrains.Select(t => t.drawTreesAndFoliage).ToArray();
            var position = camera.transform.position; var rotation = camera.transform.rotation;
            var target = camera.targetTexture; var flags = camera.clearFlags; var background = camera.backgroundColor;
            var rect = camera.rect; var fov = camera.fieldOfView; var previous = RenderTexture.active;
            var rt = new RenderTexture(640, 360, 24); var tex = new Texture2D(640, 360, TextureFormat.RGB24, false);
            try
            {
                camera.rect = new Rect(0, 0, 1, 1); camera.fieldOfView = 50; camera.targetTexture = rt;
                for (int i = 0; i < cloths.Length; i++)
                {
                    var c = cloths[i]; var renderer = c.GetComponent<SkinnedMeshRenderer>();
                    Check(renderer.enabled && renderer.updateWhenOffscreen && !renderer.forceRenderingOff, "Cloth renderer active");
                    Check((camera.cullingMask & (1 << c.gameObject.layer)) != 0, "Local player camera includes cloth layer");
                    var bounds = renderer.bounds;
                    camera.transform.position = bounds.center + (host ? Vector3.forward : Vector3.back) * bounds.size.y * 1.3f;
                    camera.transform.LookAt(bounds.center);
                    // First save the real scene through this peer's local gameplay camera.
                    camera.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 640, 360), 0, 0); tex.Apply();
                    File.WriteAllBytes(Path.Combine(_folder, _peer + "-cloth-" + i + ".png"), tex.EncodeToPNG());
                    // Then isolate the selected cloth fabric to prove pixels are from it.
                    for (int n = 0; n < all.Length; n++) all[n].forceRenderingOff = all[n] != renderer;
                    for (int n = 0; n < terrains.Length; n++) { terrains[n].drawHeightmap = false; terrains[n].drawTreesAndFoliage = false; }
                    camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
                    camera.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, 640, 360), 0, 0); tex.Apply();
                    int pixels = tex.GetPixels32().Count(p => p.r > 10 || p.g > 10 || p.b > 10);
                    Check(pixels > 100, "Cloth invisible from gameplay camera: " + c.transform.parent.parent.name);
                    File.WriteAllBytes(Path.Combine(_folder, _peer + "-cloth-" + i + "-isolated.png"), tex.EncodeToPNG());
                    Log("VISIBLE " + _peer + " " + c.transform.parent.parent.name + " fabricPixels=" + pixels);
                    for (int n = 0; n < all.Length; n++) all[n].forceRenderingOff = forceOff[n];
                    for (int n = 0; n < terrains.Length; n++) { terrains[n].drawHeightmap = drawTerrain[n]; terrains[n].drawTreesAndFoliage = drawTrees[n]; }
                    camera.clearFlags = flags; camera.backgroundColor = background;
                }
            }
            finally
            {
                for (int n = 0; n < all.Length; n++) if (all[n] != null) all[n].forceRenderingOff = forceOff[n];
                for (int n = 0; n < terrains.Length; n++) { terrains[n].drawHeightmap = drawTerrain[n]; terrains[n].drawTreesAndFoliage = drawTrees[n]; }
                camera.transform.SetPositionAndRotation(position, rotation); camera.targetTexture = target; camera.rect = rect; camera.fieldOfView = fov;
                camera.clearFlags = flags; camera.backgroundColor = background; RenderTexture.active = previous;
                Object.Destroy(rt); Object.Destroy(tex);
            }
        }
    }
}
#endif
