using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Herbalist.Networking;
using Herbalist.Presentation;
using Herbalist.StageOne;
namespace Herbalist.GameUI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Herbalist.GameUI", sourceAssembly: "Assembly-CSharp", sourceClassName: "GameOverlayUI")]
    public sealed class GameOverlayHud : MonoBehaviour
    {
        public GameUiSettings settings;
        public GameObject pauseRoot, ownMenu, options, loading, stageHeader;
        public TMP_Text pauseMessage, stageName, objective, subtitle;
        public Button resume, optionButton, mainMenu;
        public string title, defaultObjective;
        private InputAction pause;
        private bool lastPaused;
        private bool recaptureAfterMinigame;
        private int pauseFrame;
        private float subtitleRemaining;
        [SerializeField] private CanvasGroup _stageHeaderGroup;
        private string _shownObjective;
        private float _objectiveElapsed;
        private void OnEnable()
        {
            _shownObjective = null;
            _objectiveElapsed = 0;
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
            if (!GameplayPause.IsPaused && TryCloseMinigame())
            {
                recaptureAfterMinigame = true;
                return;
            }
            if(!GameplayPause.IsPaused)SetPause(true);
            else if(RoomControl.Instance==null||!FusionLobbySession.Instance.HasNetworkSession||RoomControl.Instance.OwnsPause)SetPause(false);
        }
        private static bool TryCloseMinigame()
        {
            var flow = StageOneFlow.Instance;
            if (flow == null || flow.Minigame == StageMinigameKind.None) return false;
            foreach (var actor in StageActor.All)
            {
                if (!actor.Local || !flow.CanCancelMinigame(actor)) continue;
                actor.Request(StageCommand.CancelMinigame);
                return true;
            }
            return false;
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
        public void ShowSubtitle(string text) => ShowSubtitle(text,settings.feedbackDuration);
        public void ShowSubtitle(string text,float duration) { subtitle.text=text; subtitleRemaining=duration; }
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
            bool mazeView = Herbalist.Levels.SapMazeBoard.LocalMazeView != null;
            if(flow!=null)objective.text=flow.Objective;
            else
            {
                var maze = FindFirstObjectByType<Herbalist.Levels.DescendingMazeFlow>();
                var altar = FindFirstObjectByType<Herbalist.Levels.RotatingAltar>();
                var branchRide = FindFirstObjectByType<Herbalist.Levels.BranchRideFlow>();
                if (maze != null) objective.text = maze.Objective;
                else if (branchRide != null) objective.text = branchRide.Objective;
                else if (altar != null) objective.text = altar.Objective;
                else objective.text = defaultObjective;
            }
            UpdateStageHeader(!mazeView && (flow == null || flow.Minigame == StageMinigameKind.None), paused, Time.unscaledDeltaTime);
            if(!paused)subtitleRemaining-=Time.unscaledDeltaTime;
            subtitle.gameObject.SetActive(subtitleRemaining>0&&!string.IsNullOrEmpty(subtitle.text));
        }
        private void UpdateStageHeader(bool visible, bool paused, float deltaTime)
        {
            if (_shownObjective != objective.text)
            {
                _shownObjective = objective.text;
                _objectiveElapsed = 0;
            }
            else if (visible && !paused) _objectiveElapsed += deltaTime;
            float alpha = 1f - Mathf.Clamp01((_objectiveElapsed - settings.objectiveHoldDuration) /
                Mathf.Max(.01f, settings.objectiveFadeDuration));
            if (_stageHeaderGroup != null) _stageHeaderGroup.alpha = alpha;
            else
            {
                stageName.alpha = alpha;
                objective.alpha = alpha;
            }
            if (stageHeader != null) stageHeader.SetActive(visible && alpha > 0);
            else
            {
                stageName.gameObject.SetActive(visible && alpha > 0);
                objective.gameObject.SetActive(visible && alpha > 0);
            }
        }
        private void LateUpdate()
        {
            if (!recaptureAfterMinigame) return;
            if (GameplayPause.IsPaused) { recaptureAfterMinigame = false; return; }
            if (StageOneFlow.Instance != null && StageOneFlow.Instance.Minigame != StageMinigameKind.None) return;
            var cursor = GameplayCursor.Instance;
            if (cursor == null) { recaptureAfterMinigame = false; return; }
            cursor.Capture();
            if (Cursor.lockState == CursorLockMode.Locked) recaptureAfterMinigame = false;
        }
        private void OnDisable()
        {
            recaptureAfterMinigame = false;
            pause?.Dispose();
            GameplayCursor.Instance?.SetUiMode(this,false);
            GameplayPause.SetOffline(false);
        }
    }
}
