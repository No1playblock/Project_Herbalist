using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Herbalist.Networking;
using Herbalist.Presentation;
namespace Herbalist.GameUI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Herbalist.GameUI", sourceAssembly: "Assembly-CSharp", sourceClassName: "GameOverlayUI")]
    public sealed class GameOverlayHud : MonoBehaviour
    {
        public GameUiSettings settings;
        public GameObject pauseRoot, ownMenu, options, loading;
        public TMP_Text pauseMessage, stageName, objective, subtitle;
        public Button resume, optionButton, mainMenu;
        public string title, defaultObjective;
        private InputAction pause;
        private bool lastPaused;
        private int pauseFrame;
        private float subtitleRemaining;
        private void OnEnable()
        {
            pause=settings.pauseAction.action.Clone(); pause.performed+=_=>TogglePause(); pause.Enable();
            AudioListener.volume=UserOptions.Volume;
        }
        private void Start()
        {
            resume.onClick.AddListener(()=>SetPause(false)); optionButton.onClick.AddListener(()=>options.SetActive(true));
            mainMenu.onClick.AddListener(ReturnToMenu); stageName.text=title;
        }
        private void TogglePause()
        {
            if(options.activeSelf) { options.SetActive(false); return; }
            if(!GameplayPause.IsPaused)SetPause(true);
            else if(RoomControl.Instance==null||!FusionLobbySession.Instance.HasNetworkSession||RoomControl.Instance.OwnsPause)SetPause(false);
        }
        private void SetPause(bool value)
        {
            if(FusionLobbySession.Instance!=null&&FusionLobbySession.Instance.HasNetworkSession)RoomControl.Instance?.RequestPause(value);
            else GameplayPause.SetOffline(value);
        }
        private async void ReturnToMenu()
        {
            if(FusionLobbySession.Instance!=null)await FusionLobbySession.Instance.LeaveAsync();
            else { GameplayPause.SetOffline(false); UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(settings.menuScene); }
        }
        public void ShowSubtitle(string text) { subtitle.text=text; subtitleRemaining=settings.feedbackDuration; }
        private void Update()
        {
            bool paused=GameplayPause.IsPaused;
            if(paused&&!lastPaused)pauseFrame=Time.frameCount;
            pauseRoot.SetActive(paused&&Time.frameCount>pauseFrame);
            bool mine=RoomControl.Instance==null||FusionLobbySession.Instance==null||!FusionLobbySession.Instance.HasNetworkSession||RoomControl.Instance.OwnsPause;
            ownMenu.SetActive(paused&&mine);
            pauseMessage.text=mine?settings.pausedText:settings.partnerPausedText;
            if(!paused)options.SetActive(false);
            if(GameplayCursor.Instance!=null) { GameplayCursor.Instance.SetUiMode(this,paused); if(lastPaused&&!paused)GameplayCursor.Instance.Capture(); }
            lastPaused=paused;
            var session=FusionLobbySession.Instance;
            loading.SetActive(session!=null&&(session.State==LobbyState.Loading||session.State==LobbyState.Leaving));
            var flow=Herbalist.StageOne.StageOneFlow.Instance;
            if(flow!=null)objective.text=flow.Objective;
            else if(FindFirstObjectByType<Herbalist.Levels.StageTwoGoal>()==null)objective.text=defaultObjective;
            if(!paused)subtitleRemaining-=Time.unscaledDeltaTime;
            subtitle.gameObject.SetActive(subtitleRemaining>0&&!string.IsNullOrEmpty(subtitle.text));
        }
        private void OnDisable()
        {
            pause?.Dispose();
            GameplayCursor.Instance?.SetUiMode(this,false);
            GameplayPause.SetOffline(false);
        }
    }
}
