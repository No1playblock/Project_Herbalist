using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.Abilities;
using Object=UnityEngine.Object;
namespace Herbalist.Editor
{
    public static class StageThreeChecks
    {
        public static string Run()
        {
            if(!Application.isPlaying)throw new Exception("Open Stage_03_Prototype and enter Play mode.");
            var original=Object.FindFirstObjectByType<StageActor>();
            var originalState=original.GetComponent<PlayerController>().Motor.CaptureState();
            var clone=Object.Instantiate(original.gameObject);clone.name="StageThreeCheck_Partner";
            var partner=clone.GetComponent<StageActor>();partner.offlineSlot=1;
            var actors=new[]{original,partner};
            var motors=actors.Select(x=>x.GetComponent<PlayerController>().Motor).ToArray();
            var leaves=new List<LeafProjectile>();var results=new List<string>();
            var leafSettings=AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>("Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset");
            var sapSettings=AssetDatabase.LoadAssetAtPath<SapAbilitySettings>("Assets/_Project/Data/Abilities/Sap/SO_SapAbilitySettings.asset");
            try
            {
                var pads=Object.FindObjectsByType<SapJumpPad>(FindObjectsSortMode.None).OrderBy(x=>x.name).ToArray();
                Require(pads.Length==3,"Three pads authored");
                for(int padIndex=0;padIndex<pads.Length;padIndex++)
                {
                    var pad=pads[padIndex];
                    var leaf=ThrowAt(pad,original.transform,leafSettings);leaves.Add(leaf);
                    Require(!pad.LeafSlot.Accepts(LeafMode.Platform),"Occupied socket rejects another leaf");
                    if(padIndex==0)
                    {
                        for(int i=0;i<2;i++)motors[i].Teleport(pad.transform.position+new Vector3(i==0?-.9f:.9f,.03f,0));
                        Settle(motors);pad.Tick(.02f);
                        Require(pad.RiderCount==0,"Standing below the one-way leaf is not riding it");
                    }
                    for(int i=0;i<2;i++)motors[i].Teleport(leaf.transform.position+new Vector3(i==0?-.9f:.9f,.15f,0));
                    Settle(motors);
                    pad.Tick(.02f);
                    Require(pad.RiderCount==2,"Both feet stand on leaf "+pad.name+" riders="+pad.RiderCount);
                    if(padIndex==0)
                    {
                        partner.offlineSlot=0;pad.Tick(.02f);Require(pad.RiderCount==1,"Duplicate slots excluded");partner.offlineSlot=1;
                        motors[1].Teleport(pad.transform.position+new Vector3(0,.03f,3));
                        Supply(pad,sapSettings,.5f);Require(pad.LaunchSequence==0 && pad.ChargeFraction==0,"One rider cannot charge");
                        motors[1].Teleport(leaf.transform.position+new Vector3(.9f,.15f,0));Settle(motors);
                        Supply(pad,sapSettings,.25f);Require(pad.ChargeFraction>0,"Actual hose hit charges inlet");
                        pad.Tick(.3f);Require(pad.ChargeFraction==0,"Stopped injection clears charge after grace");
                        Supply(pad,sapSettings,.25f);leaf.BeginReturn();pad.Tick(.02f);
                        Require(pad.ChargeFraction==0 && pad.Phase==SapJumpPadPhase.Empty,"Recall cancels charge");
                        leaf=ThrowAt(pad,original.transform,leafSettings);leaves.Add(leaf);
                        for(int i=0;i<2;i++)motors[i].Teleport(leaf.transform.position+new Vector3(i==0?-.9f:.9f,.15f,0));
                        Settle(motors);
                    }
                    for(int step=0;step<55 && pad.LaunchSequence==0;step++)Supply(pad,sapSettings,.02f);
                    Require(pad.LaunchSequence==1,"Launch exactly once "+pad.name);
                    Require(leaf.State==LeafState.Returning,"Launched leaf returns through capacity path");
                    Require(motors.All(x=>x.CaptureState().ExternalFlight),"Both motors launched");
                    Require(motors.All(x=>!x.TryJump()),"Normal jump cannot interrupt launch");
                    // Restore each step to exercise prediction state; movement input cannot steer the pad launch.
                    for(int step=0;step<90;step++)
                    {
                        foreach(var motor in motors)
                        {
                            motor.RestoreState(motor.CaptureState());
                            motor.Simulate(motor.CaptureState().ExternalFlight?Vector3.left*8:Vector3.zero,.02f);
                        }
                        Physics.SyncTransforms();
                    }
                    for(int i=0;i<2;i++)
                    {
                        Require(motors[i].IsGrounded && !motors[i].CaptureState().ExternalFlight,"Grounded landing "+pad.name);
                        Require(Vector3.Distance(motors[i].transform.position,pad.Landings[i].position)<.3f,"Correct landing "+pad.name+" pos="+motors[i].transform.position);
                    }
                    pad.Tick(pad.Settings.cooldown+.1f);pad.Tick(.02f);Require(pad.Phase==SapJumpPadPhase.Empty,"Reusable after cooldown");
                    results.Add(pad.name+": real leaf install, hose injection, two-rider ballistic landing and state restore PASS");
                }
                var recovery=Object.FindFirstObjectByType<StageFallRecovery>();
                var retained=ThrowAt(pads[0],original.transform,leafSettings);leaves.Add(retained);
                var otherPosition=motors[1].transform.position;
                motors[0].Teleport(new Vector3(0,-6,0));recovery.Recover();
                Require(motors[0].transform.position.y>0,"Fallen player returns");
                Require(motors[1].transform.position==otherPosition,"Partner position preserved");
                Require(retained.Installed,"Placed leaf preserved on fall");
                var exit=GameObject.Find("BothPlayers_StageFourExit").GetComponent<StageArrivalZone>();
                motors[0].Teleport(exit.PlayerArrivalPoint+Vector3.left);
                motors[1].Teleport(exit.PlayerArrivalPoint+Vector3.right);
                Require(exit.CountPresent()==2,"Upper exit counts both");
                motors[1].Teleport(otherPosition+Vector3.back*10);Require(exit.CountPresent()==1,"Exit waiting counts update");
                results.Add("Missing/duplicate rider, charge interruption, recall, cooldown, individual recovery and shared exit PASS");
                return string.Join("\n",results);
            }
            finally
            {
                foreach(var leaf in leaves)if(leaf!=null)Object.DestroyImmediate(leaf.gameObject);
                Object.DestroyImmediate(clone);
                original.GetComponent<PlayerController>().Motor.RestoreState(originalState);
                Physics.SyncTransforms();
            }
        }
        private static LeafProjectile ThrowAt(SapJumpPad pad,Transform owner,LeafAbilitySettings settings)
        {
            var leaf=Object.Instantiate(settings.offlinePrefab);
            Vector3 origin=pad.LeafSlot.transform.position+Vector3.up*2;
            leaf.Initialize(settings,owner,LeafMode.Platform,origin,pad.LeafSlot.transform.position-Vector3.up,delegate{},true);
            Physics.SyncTransforms();
            for(int i=0;i<30&&!leaf.Installed;i++)leaf.Tick(.02f);
            Require(leaf.Installed && leaf.Target==pad.LeafSlot,"Real projectile hits socket "+pad.name);
            Physics.SyncTransforms();return leaf;
        }
        private static void Settle(PlayerMotor[] motors)
        {
            Physics.SyncTransforms();
            for(int step=0;step<10;step++){foreach(var m in motors)m.Simulate(Vector3.zero,.02f);Physics.SyncTransforms();}
        }
        private static void Supply(SapJumpPad pad,SapAbilitySettings settings,float dt)
        {
            RaycastHit hit;var collider=pad.Inlet.GetComponent<Collider>();
            Require(collider.Raycast(new Ray(collider.bounds.center+Vector3.up*2,Vector3.down),out hit,3),"Ray hits inlet");
            SapDeposit.ApplyHoseHit(hit,settings,dt,()=>throw new Exception("Inlet should not allocate normal sap marks"));
            pad.Tick(dt);
        }
        private static void Require(bool condition,string message){if(!condition)throw new Exception("Stage Three check failed: "+message);}
    }
}