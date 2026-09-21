#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Herbalist.Networking;
namespace Herbalist.StageOne
{
    // Opt-in real two-process verification; never runs during normal play.
    public static class StageOneNetworkCheck
    {
        private static async Task Wait(Func<bool> condition, string label)
        {
            float end=Time.realtimeSinceStartup+60;
            while(!condition() && Time.realtimeSinceStartup<end) await Task.Delay(100);
            if(!condition()) throw new Exception("Timed out: "+label);
            await Task.Delay(250);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Run()
        {
            var args=Environment.GetCommandLineArgs(); int index=Array.IndexOf(args,"-herbalist-stage-test");
            bool levels=args.Contains("-herbalist-level-test"); bool swap=args.Contains("-swap-potions");
            if(index<0 || index+2>=args.Length) return;
            bool host=args[index+1]=="host";
            Application.runInBackground=true;
            try
            {
                await Wait(()=>FusionLobbySession.Instance!=null,"lobby");
                await FusionLobbySession.Instance.ConnectAsync(args[index+2],host);
                await Wait(()=>StageOneFlow.Instance!=null && NetworkPlayer.Local!=null && StageOneFlow.Instance.Actors.All(a=>a!=null),"two actors");
                var f=StageOneFlow.Instance; var local=NetworkPlayer.Local.GetComponent<NetworkStageActor>();
                if(f.Actors.Any(a=>a.Abilities.Unlocked)) throw new Exception("Prototype ability grant leaked into stage");
                if(host)
                {
                    if(NetworkPlayer.Local.Slot!=0) throw new Exception("Expected host test slot 0");
                    PlacePair(f,swap?1:0);
                    await Task.Delay(800);
                    local.RPC_Command(StageCommand.Interact);
                    await Wait(()=>f.Progress.Held[0]!=0,"first gather");
                    local.RPC_Command(StageCommand.Interact);
                    await Wait(()=>f.Progress.Consumed[1]!=0,"partner first potion");
                    PlacePair(f,swap?0:1); await Task.Delay(800);
                    local.RPC_Command(StageCommand.Interact);
                    await Wait(()=>f.Progress.Held[0]!=0,"second gather");
                    local.RPC_Command(StageCommand.Interact);
                    await Wait(()=>f.settings.Item(f.Progress.Held[0])?.type==StageItemType.Potion,"partner returned potion");
                    local.RPC_Command(StageCommand.Drink);
                    await Wait(()=>f.Progress.GateOpen,"gate open");
                    var inside=f.interior.transform.TransformPoint(f.interior.center)-f.settings.interactionOffset;
                    Teleport(f.Actors[0],inside+Vector3.left*.5f); await Task.Delay(700);
                    if(f.Progress.Cleared) throw new Exception("Cleared with only one inside");
                    Teleport(f.Actors[1],inside+Vector3.right*.5f);
                }
                else
                {
                    if(NetworkPlayer.Local.Slot!=1) throw new Exception("Expected client test slot 1");
                    for(int round=0;round<2;round++)
                    {
                        await Wait(()=>f.settings.Item(f.Progress.Held[1])?.type==StageItemType.Herb,"received herb");
                        local.RPC_Command(StageCommand.Craft);
                        await Wait(()=>f.settings.Item(f.Progress.Held[1])?.type==StageItemType.Potion,"crafted potion replica");
                        if(round==0)
                        {
                            local.RPC_Command(StageCommand.Drink);
                            await Wait(()=>f.Progress.Consumed[1]!=0,"own drink replica");
                        }
                        else
                        {
                            int potion=f.Progress.Held[1]; local.RPC_Command(StageCommand.Drink); await Task.Delay(700);
                            if(f.Progress.Held[1]!=potion) throw new Exception("Second potion was consumed");
                            local.RPC_Command(StageCommand.Interact);
                        }
                    }
                }
                if(levels)
                {
                    await Wait(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Stage_02_BGModel"&&NetworkPlayer.Local!=null&&StageActor.All.Count(a=>a.Available)==2,"stage two spawns");
                    await Task.Delay(1000);
                    var actors=StageActor.All.Where(a=>a.Available).OrderBy(a=>a.Slot).ToArray();
                    var expected0=swap?Herbalist.Abilities.PlayerAbilityKind.Leaf:Herbalist.Abilities.PlayerAbilityKind.Sap;
                    var expected1=swap?Herbalist.Abilities.PlayerAbilityKind.Sap:Herbalist.Abilities.PlayerAbilityKind.Leaf;
                    if(!actors.All(a=>a.Abilities.Unlocked)||actors[0].Abilities.Kind!=expected0||actors[1].Abilities.Kind!=expected1)throw new Exception("Stage transfer lost potion assignments");
                    var zone=UnityEngine.Object.FindAnyObjectByType<Herbalist.Levels.StageArrivalZone>();
                    var exit=UnityEngine.Object.FindAnyObjectByType<Herbalist.Levels.CooperativeStageExit>();
                    if(host)
                    {
                        Vector3 target=zone.PlayerArrivalPoint;
                        Teleport(actors[0],target+Vector3.forward*.6f);
                        await Wait(()=>exit.PresentPlayers==1,"first player waiting");
                        if(exit.TransitionRequested)throw new Exception("Exit transitioned with only one player");
                        Teleport(actors[0],target+Vector3.left*8);
                        await Wait(()=>exit.PresentPlayers==0,"waiting player left");
                        Teleport(actors[0],target+Vector3.forward*.6f);
                        Teleport(actors[1],target+Vector3.back*.6f);
                    }
                    await Wait(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Stage_03_Prototype"&&NetworkPlayer.Local!=null&&StageActor.All.Count(a=>a.Available)==2,"stage three arrival");
                    actors=StageActor.All.Where(a=>a.Available).OrderBy(a=>a.Slot).ToArray();
                    if(!actors.All(a=>a.Abilities.Unlocked)||actors[0].Abilities.Kind!=expected0||actors[1].Abilities.Kind!=expected1)throw new Exception("Stage three transfer lost potion assignments");
                    Debug.Log("[StageLevelsCheck] PASS "+(host?"HOST":"CLIENT")+" slot0="+actors[0].Abilities.Kind+" slot1="+actors[1].Abilities.Kind+" stage3arrival=true");
                    return;
                }
                await Wait(()=>f.Progress.Cleared,"stage clear replica");
                if(!f.Actors.All(a=>a.Abilities.Unlocked)) throw new Exception("Ability replication missing");
                int bit=1<<f.settings.herbGlowLayer;
                if((f.Actors[0].Camera.cullingMask&bit)==0 || (f.Actors[1].Camera.cullingMask&bit)!=0) throw new Exception("Glow camera isolation failed");
                Debug.Log("[StageOneNetworkCheck] PASS "+(host?"HOST":"CLIENT")+" crafted="+f.Progress.Crafted+" consumed="+f.Progress.ConsumedMask+" inside="+f.InsideCount+" clear="+f.Progress.Cleared);
            }
            catch(Exception e) { Debug.LogError("[StageOneNetworkCheck] FAIL "+e); }
        }
        private static void PlacePair(StageOneFlow f,int source)
        {
            Vector3 ground=f.sources[source].Point; ground.y=f.sources[source].Point.y-.25f;
            Teleport(f.Actors[0],ground+Vector3.back); Teleport(f.Actors[1],ground+Vector3.back+Vector3.right);
        }
        private static void Teleport(StageActor actor,Vector3 position)
        {
            var n=actor.GetComponent<NetworkPlayer>(); n.SimulationPosition=position;n.SimulationVelocity=Vector3.zero;actor.transform.position=position;
        }
    }
}
#endif
