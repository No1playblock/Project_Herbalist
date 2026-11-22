using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Herbalist.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
namespace Herbalist.GameUI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Herbalist.GameUI", sourceAssembly: "Assembly-CSharp", sourceClassName: "MainMenuUI")]
    public sealed class MainMenuScreen : MonoBehaviour
    {
        public GameUiSettings settings;
        public GameObject home, entry, room, loading, options;
        public Button create, join, confirm, entryBack, roomBack, start, copy, solo, quit, openOptions;
        public Button[] characterButtons;
        [SerializeField] private GameObject[] _availableRoleArrows;
        [SerializeField] private GameObject[] _occupiedRoleArrows;
        public TMP_Text entryTitle, message, roomCode, readiness, loadingText;
        public TMP_Text[] characterPlayers, characterLabels;
        public TMP_InputField code;
        [SerializeField] private GameObject _joinCodeHint;
        [SerializeField] private TMP_Text _entryActionLabel;
        public TMP_Text participantCount, unassignedPlayers;
        private bool creating, entering, soloLoading;
        private string _joinActionText;
        [SerializeField] private string _characterSelectScene;
        [SerializeField] private GameObject _titleBackdrop;
        private AsyncOperation _characterSelectOperation;
        private GameObject[] _menuBackdrop;
        private InputAction _closeOptionsAction;
        private FusionLobbySession Session => FusionLobbySession.Instance;
        private void OnEnable()
        {
            _closeOptionsAction = settings.pauseAction.action.Clone();
            _closeOptionsAction.performed += _ =>
            {
                if (options.activeSelf && options.TryGetComponent<OptionsPanel>(out var panel))
                    panel.CloseOptions();
            };
            _closeOptionsAction.Enable();
        }
        private void OnDisable()
        {
            _closeOptionsAction?.Dispose();
            _closeOptionsAction = null;
        }

        private void Start()
        {
            AudioListener.volume=UserOptions.Volume;
            _joinActionText = _entryActionLabel.text;
            create.onClick.AddListener(CreateRoom); join.onClick.AddListener(OpenJoin);
            entryBack.onClick.AddListener(()=>{ entering=false; Session.ClearError(); });
            confirm.onClick.AddListener(Connect); roomBack.onClick.AddListener(Leave);
            start.onClick.AddListener(()=>RoomControl.Instance?.StartGame());
            copy.onClick.AddListener(()=>GUIUtility.systemCopyBuffer=Session.RoomName);
            solo.onClick.AddListener(Solo); quit.onClick.AddListener(Application.Quit);
            openOptions.onClick.AddListener(()=>options.SetActive(true));
            for(int i=0;i<characterButtons.Length;i++) { int role=i; characterButtons[i].onClick.AddListener(()=>RoomControl.Instance.Select(RoomControl.Instance.LocalChoice==role?-1:role)); }
            Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
            _menuBackdrop = System.Array.FindAll(gameObject.scene.GetRootGameObjects(), root =>
                root.activeSelf && (root.GetComponent<MainMenuLiveBackdrop>() != null ||
                root.GetComponent<Light>() != null || root.name == "Stage01_MenuEnvironment" ||
                root.GetComponent<UnityEngine.EventSystems.EventSystem>() != null));
        }
        private void CreateRoom()
        {
            creating = true;
            entering = true;
            code.gameObject.SetActive(false);
            _joinCodeHint.SetActive(false);
            _entryActionLabel.text = settings.createText;
            Connect();
        }

        private void OpenJoin()
        {
            creating = false;
            entering = true;
            Session.ClearError();
            code.text = string.Empty;
            code.gameObject.SetActive(true);
            _joinCodeHint.SetActive(true);
            _entryActionLabel.text = _joinActionText;
        }

        private async void Connect()
        {
            string roomName = creating ? System.Guid.NewGuid().ToString("N").Substring(0, 6) : code.text;
            await Session.ConnectAsync(roomName, creating);
        }
        private async void Leave() { entering=false; await Session.LeaveAsync(); }
        internal void OpenRoomOptions() => options.SetActive(true);
        internal bool RoomOptionsOpen => options.activeSelf;
        internal void LeaveCharacterRoom()
        {
            options.SetActive(false);
            Leave();
        }
        private void Solo()
        {
            if(Session.HasNetworkSession || soloLoading)return;
            soloLoading=true; loading.SetActive(true);
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(settings.soloScene);
        }
        private void Update()
        {
            if(Session==null)return;
            var state=Session.State;
            bool busy=state==LobbyState.Connecting||state==LobbyState.Loading||state==LobbyState.Leaving||soloLoading;
            loading.SetActive(busy);
            loadingText.text=state==LobbyState.Connecting?settings.connectingText:settings.loadingText;
            home.SetActive(!busy&&!entering&&(state==LobbyState.Idle||state==LobbyState.Error));
            entry.SetActive(!busy && entering && (state == LobbyState.Error || (!creating && state == LobbyState.Idle)));
            bool showCharacterSelect = state == LobbyState.Waiting && !busy &&
                !string.IsNullOrEmpty(_characterSelectScene);
            if (_titleBackdrop != null) _titleBackdrop.SetActive(!showCharacterSelect && !busy);
            UpdateCharacterSelect(showCharacterSelect);
            room.SetActive(state==LobbyState.Waiting&&!busy&&!showCharacterSelect);
            entryTitle.text=creating?settings.createText:settings.joinText;
            message.text=state==LobbyState.Error?Session.Message:"";
            confirm.interactable = !busy && (creating || !string.IsNullOrWhiteSpace(code.text));
            roomCode.text=string.Format(settings.roomFormat,Session.RoomName);
            if (!room.activeSelf) return;
            var control=RoomControl.Instance;
            if(control==null)return;
            if(participantCount!=null)participantCount.text=string.Format(settings.participantsFormat,control.Players.Length);
            if(unassignedPlayers!=null) {
                var list=new System.Text.StringBuilder();
                foreach(int id in control.Players)if(control.Choice(id)<0)list.AppendLine(string.Format(settings.playerFormat,id)+(id==control.LocalId?settings.mineSuffix:"" )+"\n"+settings.unassignedText+"\n");
                unassignedPlayers.text=list.ToString();
            }
            readiness.text=control.Ready?settings.readyText:settings.waitingText;
            start.gameObject.SetActive(control.IsHost && control.Players.Length >= 2);
            start.interactable=control.IsHost&&control.Ready;
            for(int i=0;i<characterButtons.Length;i++)
            {
                int owner=control.Occupant(i);
                if (_availableRoleArrows != null && i < _availableRoleArrows.Length && _availableRoleArrows[i] != null)
                    _availableRoleArrows[i].SetActive(owner == 0);
                if (_occupiedRoleArrows != null && i < _occupiedRoleArrows.Length && _occupiedRoleArrows[i] != null)
                    _occupiedRoleArrows[i].SetActive(owner != 0);
                characterButtons[i].interactable=owner==0||owner==control.LocalId;
                characterLabels[i].text=owner==0?settings.selectText:owner==control.LocalId?settings.selectedText:settings.occupiedText;
                characterPlayers[i].text=owner==0?settings.vacantText:string.Format(settings.playerFormat,owner)+(owner==control.LocalId?settings.mineSuffix:"");
            }
        }

        private void UpdateCharacterSelect(bool show)
        {
            if (string.IsNullOrEmpty(_characterSelectScene) ||
                (_characterSelectOperation != null && !_characterSelectOperation.isDone))
                return;

            var selectionScene = SceneManager.GetSceneByPath(_characterSelectScene);
            if (show)
            {
                if (selectionScene.isLoaded)
                    return;

                foreach (var root in _menuBackdrop)
                    root.SetActive(false);
                _characterSelectOperation = SceneManager.LoadSceneAsync(_characterSelectScene, LoadSceneMode.Additive);
            }
            else if (selectionScene.isLoaded)
            {
                options.SetActive(false);
                // Fusion owns the single-scene load when the match starts.
                if (Session.State != LobbyState.Loading && Session.State != LobbyState.Playing)
                    _characterSelectOperation = SceneManager.UnloadSceneAsync(selectionScene);
            }
            else
            {
                foreach (var root in _menuBackdrop)
                    if (!root.activeSelf)
                        root.SetActive(true);
            }
        }
    }
}
