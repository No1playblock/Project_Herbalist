#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Threading.Tasks;
using UnityEngine;
using Herbalist.Networking;
namespace Herbalist.GameUI
{
    public static class GameUiNetworkCheck
    {
        private static async Task Wait(Func<bool> condition,string name,int seconds=40)
        {
            var end=DateTime.UtcNow.AddSeconds(seconds);
            while(!condition()&&DateTime.UtcNow<end)await Task.Delay(100);
            if(!condition())throw new Exception(name);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Run()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-herbalist-ui-check");
            if(index<0||index+2>=args.Length)return;
            bool host=args[index+1]=="host"; Application.runInBackground=true;
            try
            {
                var session=FusionLobbySession.Instance;
                await session.ConnectAsync(args[index+2],host);
                await Wait(()=>session.ConnectedCount==2&&session.State==LobbyState.Waiting,"two in lobby");
                var room=RoomControl.Instance;
                if(host)
                {
                    room.Select(1);
                    await Task.Delay(2500);
                    if(session.State!=LobbyState.Waiting)throw new Exception("Unexpected auto-start");
                    await Wait(()=>room.Ready,"both choices");
                    room.StartGame();
                }
                else
                {
                    await Wait(()=>room.Occupant(1)!=0,"host choice replicated");
                    room.Select(1); await Task.Delay(500);
                    if(room.LocalChoice==1)throw new Exception("Duplicate choice accepted");
                    room.Select(0);
                }
                await Wait(()=>NetworkPlayer.Local!=null&&session.State==LobbyState.Playing,"scene start");
                if(NetworkPlayer.Local.Slot!=(host?1:0))throw new Exception("Role slot not preserved");
                await Task.Delay(500);
                if(host)
                {
                    room.RequestPause(true); await Wait(()=>room.Paused,"host pause");
                    Vector3 before=NetworkPlayer.Local.transform.position;
                    await Task.Delay(2500);
                    if(!room.Paused)throw new Exception("Non-owner resumed");
                    if(Vector3.Distance(before,NetworkPlayer.Local.transform.position)>.01f)throw new Exception("Player moved during pause");
                    room.RequestPause(false);
                }
                else
                {
                    await Wait(()=>room.Paused,"pause replicated");
                    if(room.OwnsPause)throw new Exception("Wrong pause owner");
                    room.RequestPause(false); await Task.Delay(500);
                    if(!room.Paused)throw new Exception("Unauthorized resume");
                }
                await Wait(()=>!room.Paused,"resume replicated");
                Debug.Log("[GameUICheck] PASS "+(host?"HOST":"CLIENT")+" choice/start/role/pause/owner-resume");
            }
            catch(Exception e){Debug.LogError("[GameUICheck] FAIL "+(host?"HOST":"CLIENT")+" "+e);}
        }
    }
}
#endif
