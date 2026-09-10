#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Herbalist.Networking
{
    public static class NetworkMovementSmokeTest
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Run()
        {
            if (!Environment.GetCommandLineArgs().Contains("-herbalist-test-movement")) return;
            Keyboard keyboard = null;
            Mouse mouse = null;
            var settings = InputSystem.settings;
            var originalBackground = settings.backgroundBehavior;
            try
            {
                float deadline = Time.realtimeSinceStartup + 60;
                while (NetworkPlayer.Local == null && Time.realtimeSinceStartup < deadline) await Task.Delay(100);
                if (NetworkPlayer.Local == null) throw new Exception("Local network player not spawned");
                await Task.Delay(2000);
                var local = NetworkPlayer.Local;
                var origin = local.SimulationPosition;
                uint jumpStart = local.LastJumpSequence;
                settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                keyboard = InputSystem.AddDevice<Keyboard>();
                InputSystem.EnableDevice(keyboard);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                await Task.Delay(1000);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.Space));
                float peak = local.SimulationPosition.y;
                for (int i=0;i<10;i++) { await Task.Delay(50); peak = Mathf.Max(peak, local.SimulationPosition.y); }
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                for (int i=0;i<60;i++) { await Task.Delay(50); peak = Mathf.Max(peak, local.SimulationPosition.y); }
                bool landedWhileHeld = local.Grounded;
                uint jumpsHeld = local.LastJumpSequence - jumpStart;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                await Task.Delay(1000);
                mouse = InputSystem.AddDevice<Mouse>(); InputSystem.EnableDevice(mouse);
                InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(300, -40) });
                await Task.Delay(1000);
                var all = UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
                Debug.Log("[MovementTest] " + JsonUtility.ToJson(new Result {
                    players=all.Length,
                    cameras=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c=>c.enabled),
                    moved=Vector3.Distance(new Vector3(origin.x,0,origin.z),new Vector3(local.SimulationPosition.x,0,local.SimulationPosition.z)),
                    peak=peak-origin.y, landedWhileHeld=landedWhileHeld, jumpPresses=jumpsHeld,
                    localPosition=local.SimulationPosition, slot=local.Slot, lookAngles=local.LookAngles, bodyYaw=local.BodyYaw, renderPosition=local.transform.position,
                    remotePosition=all.First(p=>p!=local).SimulationPosition
                }));
            }
            catch(Exception e) { Debug.LogError("[MovementTest] " + e); }
            finally { if(keyboard!=null) InputSystem.RemoveDevice(keyboard); if(mouse!=null) InputSystem.RemoveDevice(mouse); settings.backgroundBehavior=originalBackground; }
        }
        [Serializable] private class Result
        {
            public int players,cameras,slot; public float moved,peak; public bool landedWhileHeld; public uint jumpPresses;
            public Vector3 localPosition,remotePosition,renderPosition; public Vector2 lookAngles; public float bodyYaw;
        }
    }
}
#endif
