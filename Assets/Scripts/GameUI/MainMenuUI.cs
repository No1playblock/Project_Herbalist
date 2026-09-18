using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Herbalist.Networking;
namespace Herbalist.GameUI
{
    public sealed class MainMenuUI : MonoBehaviour
    {
        public GameUiSettings settings;
        public GameObject home, entry, room, loading, options;
        public Button create, join, confirm, entryBack, roomBack, start, copy, solo, quit, openOptions;
        public Button[] characterButtons;
        public TMP_Text entryTitle, message, roomCode, readiness, loadingText;
        public TMP_Text[] characterPlayers, characterLabels;
        public TMP_InputField code;
        public TMP_Text participantCount, unassignedPlayers;
        private bool creating, entering, soloLoading;
        private FusionLobbySession Session => FusionLobbySession.Instance;
        private void Start()
        {
            AudioListener.volume=UserOptions.Volume;
            create.onClick.AddListener(()=>OpenEntry(true)); join.onClick.AddListener(()=>OpenEntry(false));
            entryBack.onClick.AddListener(()=>{ entering=false; Session.ClearError(); });
            confirm.onClick.AddListener(Connect); roomBack.onClick.AddListener(Leave);
            start.onClick.AddListener(()=>RoomControl.Instance?.StartGame());
            copy.onClick.AddListener(()=>GUIUtility.systemCopyBuffer=Session.RoomName);
            solo.onClick.AddListener(Solo); quit.onClick.AddListener(Application.Quit);
            openOptions.onClick.AddListener(()=>options.SetActive(true));
            for(int i=0;i<characterButtons.Length;i++) { int role=i; characterButtons[i].onClick.AddListener(()=>RoomControl.Instance.Select(RoomControl.Instance.LocalChoice==role?-1:role)); }
            Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
        }
        private void OpenEntry(bool make)
        { creating=make; entering=true; Session.ClearError(); code.text=make?System.Guid.NewGuid().ToString("N").Substring(0,6):""; }
        private async void Connect() { await Session.ConnectAsync(code.text,creating); }
        private async void Leave() { entering=false; await Session.LeaveAsync(); }
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
            entry.SetActive(!busy&&entering&&(state==LobbyState.Idle||state==LobbyState.Error));
            room.SetActive(state==LobbyState.Waiting&&!busy);
            entryTitle.text=creating?settings.createText:settings.joinText;
            message.text=state==LobbyState.Error?Session.Message:"";
            confirm.interactable=!busy&&!string.IsNullOrWhiteSpace(code.text);
            roomCode.text=string.Format(settings.roomFormat,Session.RoomName);
            var control=RoomControl.Instance;
            if(control==null)return;
            if(participantCount!=null)participantCount.text=string.Format(settings.participantsFormat,control.Players.Length);
            if(unassignedPlayers!=null) {
                var list=new System.Text.StringBuilder();
                foreach(int id in control.Players)if(control.Choice(id)<0)list.AppendLine(string.Format(settings.playerFormat,id)+(id==control.LocalId?settings.mineSuffix:"" )+"\n"+settings.unassignedText+"\n");
                unassignedPlayers.text=list.ToString();
            }
            readiness.text=control.Ready?settings.readyText:settings.waitingText;
            start.interactable=control.IsHost&&control.Ready;
            for(int i=0;i<characterButtons.Length;i++)
            {
                int owner=control.Occupant(i);
                characterButtons[i].interactable=owner==0||owner==control.LocalId;
                characterLabels[i].text=owner==0?settings.selectText:owner==control.LocalId?settings.selectedText:settings.occupiedText;
                characterPlayers[i].text=owner==0?settings.vacantText:string.Format(settings.playerFormat,owner)+(owner==control.LocalId?settings.mineSuffix:"");
            }
        }
    }
}
