using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Herbalist.Networking
{
    // Temporary test entry. Removal instructions: Assets/_Project/Documentation/Player/PrototypeModeMenu.md.
    public sealed class PrototypeModeMenu : MonoBehaviour
    {
        [SerializeField] private GameObject selectionPanel, multiplayerPanel;
        [SerializeField] private UnityEngine.UI.Button singlePlayerButton, multiplayerButton, returnButton;
        [SerializeField] private UnityEngine.UI.Text status;
        [SerializeField] private string singlePlayerScenePath;
        [SerializeField] private string loadingMessage = "Loading...";
        [SerializeField] private string failureMessage = "Could not load the test scene.";
        private FusionLobbySession session;
        private bool loading;
        private void Start()
        {
            session = FusionLobbySession.Instance;
            singlePlayerButton.onClick.AddListener(StartSinglePlayer);
            multiplayerButton.onClick.AddListener(ShowMultiplayer);
            returnButton.onClick.AddListener(ShowSelection);
            if (session != null) session.Changed += OnSessionChanged;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            if (session != null && session.State != LobbyState.Idle) ShowMultiplayer();
            else ShowSelection();
        }
        private bool CanChoose => !loading && (session == null || (!session.HasNetworkSession &&
            (session.State == LobbyState.Idle || session.State == LobbyState.Error)));
        public void ShowMultiplayer()
        {
            if (loading) return;
            selectionPanel.SetActive(false); multiplayerPanel.SetActive(true);
            RefreshButtons();
        }
        public void ShowSelection()
        {
            if (!CanChoose) return;
            if (session != null) session.ClearError();
            selectionPanel.SetActive(true); multiplayerPanel.SetActive(false);
            status.text = string.Empty; RefreshButtons();
        }
        private void OnSessionChanged()
        {
            if (session.HasNetworkSession || session.State == LobbyState.Error) ShowMultiplayer();
            RefreshButtons();
        }
        private void RefreshButtons()
        {
            singlePlayerButton.interactable = CanChoose;
            multiplayerButton.interactable = CanChoose;
            returnButton.gameObject.SetActive(CanChoose);
        }
        public async void StartSinglePlayer()
        {
            if (!CanChoose) return;
            loading = true; RefreshButtons(); status.text = loadingMessage;
            try
            {
                if (string.IsNullOrWhiteSpace(singlePlayerScenePath) || !Application.CanStreamedLevelBeLoaded(singlePlayerScenePath))
                    throw new InvalidOperationException("Configured single-player scene is missing from Build Settings.");
                var operation = SceneManager.LoadSceneAsync(singlePlayerScenePath, LoadSceneMode.Single);
                while (operation != null && !operation.isDone) await System.Threading.Tasks.Task.Yield();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (this != null) { loading = false; status.text = failureMessage; RefreshButtons(); }
            }
        }
        private void OnDestroy()
        {
            if (session != null) session.Changed -= OnSessionChanged;
            singlePlayerButton.onClick.RemoveListener(StartSinglePlayer);
            multiplayerButton.onClick.RemoveListener(ShowMultiplayer);
            returnButton.onClick.RemoveListener(ShowSelection);
        }
    }
}
