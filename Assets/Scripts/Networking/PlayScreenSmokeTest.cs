#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Threading.Tasks;
using Herbalist.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Herbalist.Networking
{
    public static class PlayScreenSmokeTest
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Run()
        {
            if (!Environment.GetCommandLineArgs().Contains("-herbalist-test-screen")) return;
            Keyboard keyboard = null;
            var original = InputSystem.settings.backgroundBehavior;
            try
            {
                float deadline = Time.realtimeSinceStartup + 60;
                while ((NetworkPlayer.Local == null || UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Length != 2) && Time.realtimeSinceStartup < deadline) await Task.Delay(100);
                await Task.Delay(2000);
                var screen = UnityEngine.Object.FindFirstObjectByType<PlayScreenController>();
                var players = UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
                Require(screen != null && players.Length == 2, "two players and screen");
                Require(screen.IsSplitVisible, "initial split");
                foreach (var p in players) Require(p.Player.View.Camera.enabled && Mathf.Approximately(p.Player.View.Camera.rect.x, p.Slot * 0.5f), "role viewport");
                Require(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(a => a.enabled) == 1, "one listener");
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                keyboard = InputSystem.AddDevice<Keyboard>(); InputSystem.EnableDevice(keyboard);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab)); await Task.Delay(350);
                Require(screen.Mode == PlayScreenMode.Personal, "Tab to personal");
                await Task.Delay(350); Require(screen.Mode == PlayScreenMode.Personal, "held Tab does not repeat");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); await Task.Delay(150);
                Require(players.Count(p => p.Player.View.Camera.enabled) == 1 && NetworkPlayer.Local.Player.View.Camera.rect == new Rect(0,0,1,1), "local fullscreen only");
                var first = new object(); var second = new object();
                screen.SetOverride(first, PlayScreenMode.Split); screen.SetOverride(second, PlayScreenMode.Personal);
                Require(!screen.TryToggle() && screen.Mode == PlayScreenMode.Personal, "nested lock blocks toggle");
                screen.SetOverride(second, null); Require(screen.Mode == PlayScreenMode.Split && screen.IsLocked, "previous lock survives");
                screen.SetOverride(first, null); Require(screen.Mode == PlayScreenMode.Personal && !screen.IsLocked, "restore preference");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab)); await Task.Delay(250);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); await Task.Delay(150);
                Require(screen.IsSplitVisible, "second Tab to split");
                var local = NetworkPlayer.Local;
                var remote = players.First(p => p != local);
                var hud = screen.GetComponentsInChildren<RectTransform>(true).First(r => r.name == (local.Slot == 0 ? "DuyeongHUD" : "SodamHUD"));
                Require(Mathf.Approximately(hud.anchorMax.x - hud.anchorMin.x, 0.5f), "split HUD width");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Tab)); await Task.Delay(200);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                Vector3 origin = local.SimulationPosition; await Task.Delay(500);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); await Task.Delay(300);
                Require(Vector3.Distance(origin, local.SimulationPosition) > 0.5f, "movement continues in personal mode");
                Require(hud.anchorMin == Vector2.zero && hud.anchorMax == Vector2.one, "personal HUD fills screen");
                Debug.Log("[ScreenTest] PASS: role viewports, Tab/hold, personal camera, listener, nested locks, HUD, movement. Final client=Personal slot=" + local.Slot);
            }
            catch(Exception e) { Debug.LogError("[ScreenTest] FAIL " + e); }
            finally { if(keyboard != null) InputSystem.RemoveDevice(keyboard); InputSystem.settings.backgroundBehavior = original; }
        }
        private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
    }
}
#endif
