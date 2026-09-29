#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Herbalist.Networking;
using Herbalist.GameUI;
using Herbalist.Abilities;
using Herbalist.Levels;
namespace Herbalist.Tests
{
    // Explicit test flag only. Runtime-cloned settings keep the normal Stage Two lobby destination intact.
    public static class StageThreeNetworkCheck
    {
        private const string StageThree="Assets/_Project/Scenes/Stages/Stage_03/Stage_03_Prototype.unity";
        private static async Task Wait(Func<bool> test,string label,float timeout=60)
        {
            float end=Time.realtimeSinceStartup+timeout;
            while(!test() && Time.realtimeSinceStartup<end)await Task.Delay(50);
            Require(test(),label);
        }
        private static void Require(bool ok,string label){if(!ok)throw new Exception(label);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-herbalist-stage-three-check");
            if(i>=0 && i+2<args.Length)Run(args[i+1]=="host",args[i+2]);
        }
        public static async void Run(bool host,string roomName)
        {
            Application.runInBackground=true;
            try
            {
                await Wait(()=>FusionLobbySession.Instance!=null,"lobby available");
                var session=FusionLobbySession.Instance;
                var field=typeof(FusionLobbySession).GetField("settings",BindingFlags.Instance|BindingFlags.NonPublic);
                var settings=UnityEngine.Object.Instantiate((LobbySettings)field.GetValue(session));
                settings.playScenePath=StageThree;
                field.SetValue(session,settings);
                await session.ConnectAsync(roomName,host);
                await Wait(()=>session.ConnectedCount==2 && session.State==LobbyState.Waiting,"connected room");
                RoomControl.Instance.Select(host?0:1);
                if(host){await Wait(()=>RoomControl.Instance.Ready,"roles ready");RoomControl.Instance.StartGame();}
                await Wait(()=>SceneManager.GetActiveScene().name=="Stage_03_Prototype" && NetworkPlayer.Local!=null &&
                    UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Length==2,"stage three spawn");
                await Task.Delay(800);
                var players=UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).OrderBy(p=>p.Slot).ToArray();
                var kinds=players.Select(p=>p.GetComponent<PlayerAbilityController>().Kind).ToArray();
                Require(players.All(p=>p.GetComponent<PlayerAbilityController>().Unlocked),"direct test abilities");
                var pads=UnityEngine.Object.FindObjectsByType<SapJumpPad>(FindObjectsSortMode.None).OrderBy(p=>p.name).ToArray();
                foreach(var pad in pads)
                {
                    if(host)
                    {
                        for(int i=0;i<2;i++)players[i].TeleportAuthoritatively(pad.transform.position+new Vector3(i==0?-.9f:.9f,.04f,-1.2f));
                        await Task.Delay(250);
                        var leafPlayer=players.First(p=>p.GetComponent<PlayerAbilityController>().Kind==PlayerAbilityKind.Leaf);
                        var ability=leafPlayer.GetComponent<PlayerAbilityController>().Leaf;
                        Require(ability.TryThrow(LeafMode.Platform,new Ray(pad.LeafSlot.transform.position+Vector3.up*2,Vector3.down)),"leaf throw");
                        await Wait(()=>pad.LeafSlot.InstalledLeaf!=null,"network leaf installed");
                        var leaf=pad.LeafSlot.InstalledLeaf;
                        for(int i=0;i<2;i++)players[i].TeleportAuthoritatively(leaf.transform.position+new Vector3(i==0?-.9f:.9f,.15f,0));
                        await Wait(()=>pad.RiderCount==2,"both riders recognized");
                        var sapPlayer=players.First(p=>p.GetComponent<PlayerAbilityController>().Kind==PlayerAbilityKind.Sap);
                        var sap=sapPlayer.GetComponent<PlayerAbilityController>().Sap;
                        // Exercise the production hose hit receiver at Host; no client may supply charge directly.
                        var sapField=typeof(SapControlAbility).GetField("settings",BindingFlags.Instance|BindingFlags.NonPublic);
                        var config=(SapAbilitySettings)sapField.GetValue(sap);
                        float end=Time.realtimeSinceStartup+3;
                        while(pad.LaunchSequence==0 && Time.realtimeSinceStartup<end)
                        {
                            RaycastHit hit;var c=pad.Inlet.GetComponent<Collider>();
                            Require(c.Raycast(new Ray(c.bounds.center+Vector3.up*2,Vector3.down),out hit,3),"inlet ray");
                            SapDeposit.ApplyHoseHit(hit,config,.04f,()=>throw new Exception("Unexpected normal mark"));
                            await Task.Delay(40);
                        }
                    }
                    await Wait(()=>pad.LaunchSequence==1,"replicated launch "+pad.name,10);
                    await Wait(()=>players.All(p=>p.Grounded && !p.ExternalFlight) &&
                        players.All(p=>Vector3.Distance(p.SimulationPosition,pad.Landings[p.Slot].position)<.5f),"replicated landing "+pad.name,10);
                    Debug.Log("[StageThreeNetworkCheck] "+(host?"HOST":"CLIENT")+" "+pad.name+" LANDING PASS");
                    await Task.Delay(host?1600:100);
                }
                if(host)
                {
                    players[0].TeleportAuthoritatively(new Vector3(0,-6,0));
                    await Wait(()=>players[0].SimulationPosition.y>0 && players[0].SimulationPosition.z<0,"individual fall recovery");
                    await Task.Delay(1000);
                    var exit=GameObject.Find("BothPlayers_StageFourExit").GetComponent<StageArrivalZone>();
                    players[0].TeleportAuthoritatively(exit.PlayerArrivalPoint+Vector3.left);
                    await Task.Delay(300);
                    players[1].TeleportAuthoritatively(exit.PlayerArrivalPoint+Vector3.right);
                }
                await Wait(()=>SceneManager.GetActiveScene().name=="Stage_04_Arrival" && NetworkPlayer.Local!=null &&
                    UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).Length==2,"stage four transfer");
                await Task.Delay(500);
                players=UnityEngine.Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).OrderBy(p=>p.Slot).ToArray();
                Require(players.All(p=>p.GetComponent<PlayerAbilityController>().Unlocked) &&
                    players.Select(p=>p.GetComponent<PlayerAbilityController>().Kind).SequenceEqual(kinds),"abilities retained");
                Debug.Log("[StageThreeNetworkCheck] PASS "+(host?"HOST":"CLIENT")+" three launches, landing, Stage Four transfer, abilities retained");
            }
            catch(Exception e){Debug.LogError("[StageThreeNetworkCheck] FAIL "+(host?"HOST":"CLIENT")+" "+e);}
        }
    }
}
#endif