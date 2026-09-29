using Fusion;
using UnityEngine;
using Herbalist.StageOne;
using Herbalist.Player;
using Herbalist.Networking;
using Herbalist.Abilities;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class StageFallRecovery : NetworkBehaviour
    {
        [SerializeField] private Transform _fallThreshold;
        [SerializeField] private Transform[] _respawns;
        private bool Networked => Object!=null&&Object.IsValid;
        public override void FixedUpdateNetwork() { if(HasStateAuthority) Recover(); }
        private void Update() { if(!Networked && (FusionLobbySession.Instance==null||!FusionLobbySession.Instance.HasNetworkSession)) Recover(); }
        public void Recover()
        {
            if(Herbalist.GameUI.GameplayPause.IsPaused || (Networked&&!HasStateAuthority))return;
            foreach(var actor in StageActor.All)
            {
                if(!actor.Available || actor.transform.position.y>=_fallThreshold.position.y || actor.Slot<0 || actor.Slot>=_respawns.Length)continue;
                actor.GetComponent<SapControlAbility>()?.Cancel();
                var network=actor.GetComponent<NetworkPlayer>();
                if(network!=null&&network.Object!=null&&network.Object.IsValid)network.TeleportAuthoritatively(_respawns[actor.Slot].position);
                else actor.GetComponent<PlayerController>().Motor.Teleport(_respawns[actor.Slot].position);
            }
        }
    }
}
