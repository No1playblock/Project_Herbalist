using System;
using System.Linq;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class TreeSapInletChecks
    {
        private static T Read<T>(object obj,string field) => (T)obj.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj);
        private static void Check(bool value,string message){if(!value)throw new Exception(message);}
        public static string Play()
        {
            Check(Application.isPlaying,"Offline Play required.");
            var pads=UnityEngine.Object.FindObjectsByType<ExteriorJumpPad>(FindObjectsSortMode.None).Where(p=>p.isActiveAndEnabled).ToArray();
            Check(pads.Length==6,"Exactly six selected mushroom pads.");
            var actor=StageActor.All.First(a=>a.Available);
            var player=actor.GetComponent<PlayerController>();
            var ability=actor.Abilities;
            var saved=actor.transform.position;
            ability.RevokeLeaf();Check(ability.UnlockSap(),"Unlock Sap for test.");
            var settings=StageLevel.Instance.abilityRules.sap;
            var leafSettings=StageLevel.Instance.abilityRules.leaf;
            var held=UnityEngine.Object.Instantiate(settings.offlinePrefab);
            LeafProjectile leaf=null;
            var report=new System.Text.StringBuilder();
            try
            {
                foreach(var pad in pads)
                {
                    var port=Read<SapInjectionPort[]>(pad,"_ports")[0];
                    var visual=port.transform.Find("TreeInletVisual");
                    Check(visual!=null,"Visible inlet: "+pad.name);
                    pad.Tick(10);
                    var origin=port.InteractionPoint+visual.forward*2.8f;
                    held.transform.position=origin;
                    var hose=new SapHoseSprayer();
                    Physics.SyncTransforms();
                    for(int i=0;i<160 && pad.ChargeFraction<1;i++)
                    {hose.Tick(held,new Ray(origin,-visual.forward),true,.02f,settings,()=>null);pad.Tick(.02f);}
                    Check(pad.ChargeFraction>=1,"Direct aimed hose charges "+pad.name);
                    pad.Tick(3.1f);Check(pad.PresentationFlight>0,"Direct hose erupts "+pad.name);
                    pad.Tick(10);
                    var target=Read<LeafInstallTarget[]>(pad,"_sockets")[0];
                    leaf=UnityEngine.Object.Instantiate(leafSettings.offlinePrefab);
                    leaf.Initialize(leafSettings,actor.transform,target.SocketPosition,target.SocketPosition,null,true);
                    Check(leaf.TryInstallAtSocket(target),"Install leaf: "+pad.name);
                    leaf.Tick(120);Check(leaf.Installed,"No leaf timeout.");
                    player.Motor.Teleport(port.InteractionPoint+visual.forward*2.4f-Vector3.up*.6f);
                    Physics.SyncTransforms();
                    Check(SapInjectionPort.TryInteractionHint(actor,out var hint)&&Vector3.Distance(hint,port.InteractionPoint)<.05f,"Nearby R selects this inlet: "+pad.name);
                    var aim=new Ray(actor.transform.position+Vector3.up*1.7f,visual.forward);
                    for(int i=0;i<180 && pad.ChargeFraction<1;i++)
                    {ability.Tick(1,0,aim,.02f,false,true);pad.Tick(.02f);Physics.SyncTransforms();}
                    Check(pad.ChargeFraction>=1,"Nearby R charges "+pad.name);
                    ability.Sap.Cancel();
                    var cc=actor.GetComponent<CharacterController>();
                    var surface=Read<Collider>(leaf,"_surface");
                    var feet=new Vector3(surface.bounds.center.x,surface.bounds.max.y+.02f,surface.bounds.center.z);
                    player.Motor.Teleport(feet-cc.transform.TransformVector(cc.center)+Vector3.up*cc.height*Mathf.Abs(cc.transform.lossyScale.y)*.5f);
                    Physics.SyncTransforms();
                    Check(leaf.Supports(cc,.64f),"Supported rider.");
                    pad.Tick(3.1f);
                    Check(pad.PresentationFlight>0&&player.Motor.Velocity.y>0,"R injection launches rider: "+pad.name);
                    Check(leaf.State==LeafState.Returning&&!target.HasLeaf,"Used leaf returns: "+pad.name);
                    UnityEngine.Object.DestroyImmediate(leaf.gameObject);leaf=null;
                    player.Motor.ResetMotion();
                    report.AppendLine("PASS "+pad.name+": aimed hose, nearby R while aiming away, 120s leaf lifetime, rider launch and leaf return.");
                }
                return report.ToString();
            }
            finally
            {
                ability.Sap.Clear();
                if(leaf!=null)UnityEngine.Object.DestroyImmediate(leaf.gameObject);
                UnityEngine.Object.DestroyImmediate(held.gameObject);
                player.Motor.Teleport(saved);player.Motor.ResetMotion();
            }
        }
    }
}
