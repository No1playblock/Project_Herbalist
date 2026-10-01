using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.Networking
{
    public sealed class LobbyScreen : MonoBehaviour
    {
        [SerializeField] private LobbySettings settings;
        [SerializeField] private InputField roomName;
        [SerializeField] private Button createButton, joinButton, leaveButton, backButton;
        [SerializeField] private GameObject entryPanel, progressPanel, errorPanel;
        [SerializeField] private Text progressText, errorText, roomText;
        private FusionLobbySession session;
        private void Start()
        {
            session = FusionLobbySession.Instance;
            roomName.characterLimit = settings.roomNameLimit;
            createButton.onClick.AddListener(Create);
            joinButton.onClick.AddListener(Join);
            leaveButton.onClick.AddListener(Leave);
            backButton.onClick.AddListener(Back);
            session.Changed += Render;
            Render();
        }
        private async void Create() => await session.ConnectAsync(roomName.text, true);
        private async void Join() => await session.ConnectAsync(roomName.text, false);
        private async void Leave() => await session.LeaveAsync();
        private void Back() => session.ClearError();
        private void Render()
        {
            var state = session.State;
            entryPanel.SetActive(state == LobbyState.Idle);
            errorPanel.SetActive(state == LobbyState.Error);
            progressPanel.SetActive(state != LobbyState.Idle && state != LobbyState.Error);
            progressText.text = session.Message;
            errorText.text = session.Message;
            roomText.text = session.RoomName;
            leaveButton.gameObject.SetActive(state == LobbyState.Waiting);
        }
        private void OnDestroy()
        {
            if (session != null) session.Changed -= Render;
            createButton.onClick.RemoveListener(Create); joinButton.onClick.RemoveListener(Join);
            leaveButton.onClick.RemoveListener(Leave); backButton.onClick.RemoveListener(Back);
        }
    }
}
