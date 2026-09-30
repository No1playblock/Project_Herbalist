using Fusion;
using UnityEngine;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class FinalStageExit : NetworkBehaviour
    {
        [SerializeField] private StageArrivalZone _arrival;
        [SerializeField] private RotatingAltar _altar;
        [SerializeField] private GameObject _endingPanel;
        [Networked] private NetworkBool Cleared { get; set; }
        private bool _cleared;
        private bool Online => Object != null && Object.IsValid;
        public bool Complete => Online ? Cleared : _cleared;
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(); }
        public void Tick()
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused || (Online && !HasStateAuthority)) return;
            if (_altar.CanExit && _arrival.CountPresent() >= _arrival.RequiredPlayers) _cleared = true;
            if (Online) Cleared = _cleared;
        }
        private void Update()
        {
            if (!Online && (FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession)) Tick();
            _endingPanel.SetActive(Complete);
        }
    }
}
