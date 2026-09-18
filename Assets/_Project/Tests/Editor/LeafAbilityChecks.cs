using System;
using System.Linq;
using System.Threading.Tasks;
using Herbalist.Abilities;
using Herbalist.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Herbalist.Editor
{
    public static class LeafAbilityChecks
    {
        public static string Result { get; private set; }
        private static void Check(bool ok,string message) { if(!ok) throw new Exception(message); }
        public static async void Run()
        {
            Result="Running";
            Keyboard keys=null; Mouse mouse=null;
            var original=InputSystem.settings.backgroundBehavior;
            try
            {
                Check(Application.isPlaying,"Play mode required");
                var player=UnityEngine.Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).First(p=>p.LocallyControlled);
                var ability=player.GetComponent<PlayerAbilityController>();
                player.Motor.Teleport(new Vector3(6,0.1f,-2)); player.View.SetLookAngles(0,0);
                await Task.Delay(300);
                InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                keys=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();InputSystem.EnableDevice(keys);InputSystem.EnableDevice(mouse);
                Check(ability.Mode==LeafMode.Off,"Initial off");
                await Click(mouse);Check(ability.Leaf.ActiveCount==0,"Off blocks use");
                await Press(keys);Check(ability.Mode==LeafMode.Pin,"R pin");
                await Click(mouse);await Task.Delay(1600);Check(ability.Leaf.ActiveCount==0,"Wrong form returns");
                await Press(keys);Check(ability.Mode==LeafMode.Platform,"R platform");
                await Click(mouse);await Task.Delay(900);
                var leaf=UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).First();
                Check(leaf.State==LeafState.Installed && leaf.Mode==LeafMode.Platform,"Platform installed");
                await Press(keys);Check(ability.Mode==LeafMode.Off && leaf.Mode==LeafMode.Platform,"R off preserves thrown form");
                await Task.Delay(5600);Check(ability.Leaf.ActiveCount==0,"Expiry returns capacity");
                ability.RevokeLeaf();await Press(keys);await Click(mouse);Check(!ability.Unlocked && ability.Mode==LeafMode.Off && ability.Leaf.ActiveCount==0,"Locked input rejected");
                Check(ability.UnlockLeaf(),"Potion unlock hook");
                await Press(keys);await Press(keys);Check(ability.Mode==LeafMode.Platform,"Cycle after unlock");
                var target=LeafInstallTarget.Find(100);target.GetComponent<SapBindingSource>().Deposit();
                await Click(mouse);await Task.Delay(900);leaf=UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).First();
                Check(leaf.State==LeafState.Bound,"Sap first binds");
                await Task.Delay(5200);Check(leaf.State==LeafState.Bound,"Binding removes timer");
                leaf.BeginReturn();Check(!target.GetComponent<SapBindingSource>().Available,"Return removes bound sap");
                await Task.Delay(900);Check(ability.Leaf.ActiveCount==0,"Bound return");
                await Click(mouse);await Task.Delay(700);target.GetComponent<SapBindingSource>().Deposit();
                leaf=UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).First();Check(leaf.State==LeafState.Installed,"Late sap does not bind");
                ability.Leaf.Clear();await Task.Delay(100);
                for(int i=0;i<3;i++){await Click(mouse);await Task.Delay(600);}
                Check(ability.Leaf.ActiveCount==3,"Three leaves active");
                var oldest=UnityEngine.Object.FindObjectsByType<LeafProjectile>(FindObjectsSortMode.None).OrderBy(l=>l.InstalledAt).First();
                await Click(mouse);Check(ability.Leaf.ActiveCount<=3,"Never exceeds capacity");
                await Task.Delay(1100);Check(ability.Leaf.ActiveCount==2,"Fourth click recalls without auto throw");
                ability.Leaf.Clear();
                player.Motor.Teleport(new Vector3(-6,0.1f,-2)); player.View.SetLookAngles(0,0);
                await Press(keys);await Press(keys);Check(ability.Mode==LeafMode.Pin,"Pin selected");
                await Click(mouse);await Task.Delay(900);Check(LeafInstallTarget.Find(101).Pinned,"Pin target effect applied");
                ability.Leaf.Clear();Check(!LeafInstallTarget.Find(101).Pinned,"Recall releases target effect");
                Result="PASS: R/Off, click, wrong target, immutable form, expiry, unlock, sap ordering/binding/removal, capacity/recall, pin effects.";
                Debug.Log("[LeafChecks] "+Result);
            }
            catch(Exception e){Result="FAIL: "+e;Debug.LogError("[LeafChecks] "+Result);}
            finally{if(keys!=null)InputSystem.RemoveDevice(keys);if(mouse!=null)InputSystem.RemoveDevice(mouse);InputSystem.settings.backgroundBehavior=original;}
        }
        private static async Task Press(Keyboard keys){InputSystem.QueueStateEvent(keys,new KeyboardState(Key.R));await Task.Delay(150);InputSystem.QueueStateEvent(keys,new KeyboardState());await Task.Delay(150);}
        private static async Task Click(Mouse mouse){InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));await Task.Delay(120);InputSystem.QueueStateEvent(mouse,new MouseState());await Task.Delay(180);}
    }
}
