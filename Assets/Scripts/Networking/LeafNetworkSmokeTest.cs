#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Threading.Tasks;
using Herbalist.Abilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Herbalist.Networking
{
    public static class LeafNetworkSmokeTest
    {
        private static void Check(bool value,string message) { if(!value) throw new Exception(message); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Run()
        {
            if(!Environment.GetCommandLineArgs().Contains("-herbalist-test-leaf")) return;
            Keyboard keys=null;Mouse mouse=null;var original=InputSystem.settings.backgroundBehavior;
            try
            {
                float deadline=Time.realtimeSinceStartup+60;
                while(NetworkPlayer.Local==null && Time.realtimeSinceStartup<deadline) await Task.Delay(100);
                Check(NetworkPlayer.Local!=null,"Local player spawn");await Task.Delay(1500);
                var local=NetworkPlayer.Local;var ability=local.GetComponent<PlayerAbilityController>();
                Check(ability.Unlocked && ability.Mode==LeafMode.Off,"Sodam grant and off");
                local.Player.View.SetLookAngles(0,0);
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                keys=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();InputSystem.EnableDevice(keys);InputSystem.EnableDevice(mouse);
                InputSystem.QueueStateEvent(keys,new KeyboardState(Key.D));await Task.Delay(1125);InputSystem.QueueStateEvent(keys,new KeyboardState());await Task.Delay(350);
                await Press(keys);Check(ability.Mode==LeafMode.Pin,"R pin replicated");
                await Press(keys);Check(ability.Mode==LeafMode.Platform,"R platform replicated");
                for(int i=0;i<3;i++){await Click(mouse);await Task.Delay(700);}
                var leaves=UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None);
                Check(ability.DisplayCount==3 && leaves.Length==3 && leaves.All(l=>l.State==LeafState.Installed && l.Mode==LeafMode.Platform),"Host spawned and replicated three platforms");
                Check(leaves.All(l=>l.GetComponentsInChildren<Collider>().Any(c=>c.enabled)),"Client platform colliders enabled");
                Debug.Log("[LeafNetworkTest] THREE_INSTALLED position="+local.SimulationPosition);
                await Click(mouse);await Task.Delay(1000);Check(ability.DisplayCount==2,"Oldest recall and no auto throw");
                await Press(keys);Check(ability.Mode==LeafMode.Off,"Off replicated");await Click(mouse);Check(ability.DisplayCount<=2,"Off blocks fire");
                await Task.Delay(6000);Check(ability.DisplayCount==0,"Network expiry reclaims capacity");
                Debug.Log("[LeafNetworkTest] PASS: unlock, R cycle, authoritative spawn, replicated platforms/colliders, capacity recall, off, expiry.");
            }
            catch(Exception e){Debug.LogError("[LeafNetworkTest] FAIL "+e);}
            finally{if(keys!=null)InputSystem.RemoveDevice(keys);if(mouse!=null)InputSystem.RemoveDevice(mouse);InputSystem.settings.backgroundBehavior=original;}
        }
        private static async Task Press(Keyboard keys){InputSystem.QueueStateEvent(keys,new KeyboardState(Key.R));await Task.Delay(180);InputSystem.QueueStateEvent(keys,new KeyboardState());await Task.Delay(300);}
        private static async Task Click(Mouse mouse){InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));await Task.Delay(140);InputSystem.QueueStateEvent(mouse,new MouseState());await Task.Delay(240);}
    }
}
#endif
