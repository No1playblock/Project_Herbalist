using Fusion;
using UnityEngine;
using Herbalist.Networking;

namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class CooperativeStageExit : NetworkBehaviour
    {
        [SerializeField] private StageArrivalZone _arrival;
        [SerializeField] private StageLevel _level;
        [SerializeField] private string _destinationEntryId;
        [SerializeField] private string _approachText = "출구에 함께 모여주세요 · {0}/{1}";
        [SerializeField] private string _waitingText = "다른 플레이어를 기다리는 중 · {0}/{1}";
        [SerializeField] private string _loadingText = "다음 스테이지로 이동 중";
        [Networked] public int PresentPlayers { get; private set; }
        [Networked] public NetworkBool TransitionRequested { get; private set; }
        public string Objective
        {
            get
            {
                bool networked = Object != null && Object.IsValid;
                int count = networked ? PresentPlayers : (_arrival != null ? _arrival.CountPresent() : 0);
                return networked && TransitionRequested ? _loadingText :
                    string.Format(count > 0 ? _waitingText : _approachText, count, _arrival != null ? _arrival.RequiredPlayers : 0);
            }
        }
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || TransitionRequested || Herbalist.GameUI.GameplayPause.IsPaused || _arrival == null) return;
            PresentPlayers = _arrival.CountPresent();
            if (PresentPlayers < _arrival.RequiredPlayers || _level == null) return;
            var session = FusionLobbySession.Instance;
            if (session == null || session.State != LobbyState.Playing) return;
            session.AdvanceStage(_level.nextScenePath, _destinationEntryId);
            // Latch only an accepted request. Session also guards concurrent scene loads.
            TransitionRequested = session.State == LobbyState.Loading;
        }
    }
}
