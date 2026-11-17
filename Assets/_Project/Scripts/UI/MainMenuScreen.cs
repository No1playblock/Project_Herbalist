using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Herbalist.Networking;
namespace Herbalist.GameUI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Herbalist.GameUI", sourceAssembly: "Assembly-CSharp", sourceClassName: "MainMenuUI")]
    public sealed class MainMenuScreen : MonoBehaviour
    {
        public GameUiSettings settings;
        public Sprite joinPanelSprite;
        public GameObject home, entry, joinEntry, room, loading, options;
        public Button create, join, confirm, entryBack, roomBack, start, copy, solo, quit, openOptions;
        public Button[] characterButtons;
        [SerializeField] private GameObject[] _availableRoleArrows;
        [SerializeField] private GameObject[] _occupiedRoleArrows;
        public TMP_Text entryTitle, message, roomCode, readiness, loadingText;
        public TMP_Text[] characterPlayers, characterLabels;
        public TMP_InputField code;
        public TMP_Text participantCount, unassignedPlayers;
        private bool creating, entering, soloLoading;
        private TMP_InputField joinCode;
        private Button joinConfirm, joinBack;
        private TMP_Text joinEntryTitle, joinMessage;
        private FusionLobbySession Session => FusionLobbySession.Instance;
        private void Awake()
        {
            if(joinEntry==null)return;
            var root=joinEntry.transform;
            joinCode=root.Find("RoomCode")?.GetComponent<TMP_InputField>();
            joinConfirm=root.Find("Confirm")?.GetComponent<Button>();
            joinBack=root.Find("Back")?.GetComponent<Button>();
            joinEntryTitle=root.Find("Title")?.GetComponent<TMP_Text>();
            joinMessage=root.Find("Message")?.GetComponent<TMP_Text>();
        }
        private void Start()
        {
            AudioListener.volume=UserOptions.Volume;
            create.onClick.AddListener(()=>OpenEntry(true)); join.onClick.AddListener(()=>OpenEntry(false));
            entryBack.onClick.AddListener(CloseEntry);
            if(joinBack!=null)joinBack.onClick.AddListener(CloseEntry);
            confirm.onClick.AddListener(Connect); if(joinConfirm!=null)joinConfirm.onClick.AddListener(Connect);
            roomBack.onClick.AddListener(Leave);
            start.onClick.AddListener(()=>RoomControl.Instance?.StartGame());
            copy.onClick.AddListener(()=>GUIUtility.systemCopyBuffer=Session.RoomName);
            solo.onClick.AddListener(Solo); quit.onClick.AddListener(Application.Quit);
            openOptions.onClick.AddListener(()=>options.SetActive(true));
            for(int i=0;i<characterButtons.Length;i++) { int role=i; characterButtons[i].onClick.AddListener(()=>RoomControl.Instance.Select(RoomControl.Instance.LocalChoice==role?-1:role)); }
            Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
        }
        private void OpenEntry(bool make)
        {
            creating=make; entering=true; Session.ClearError();
            if(make)code.text=System.Guid.NewGuid().ToString("N").Substring(0,6);
            else if(joinCode!=null)joinCode.text="";
            if(!make)SetJoinLabels();
        }
        private void CloseEntry() { entering=false; Session.ClearError(); TitleButtonHover.ClearSelection(); }
        private void SetJoinLabels()
        {
            if(joinEntryTitle!=null)joinEntryTitle.text=settings.joinText;
            SetJoinText("Hint","초대 코드를 입력하세요");
            SetJoinText("RoomCode/TextArea/Placeholder","초대 코드 입력");
            SetJoinText("Confirm/Label","참가하기");
            SetJoinText("Back/Label","← 뒤로");
        }
        private void SetJoinText(string path,string value)
        {
            var target=joinEntry!=null?joinEntry.transform.Find(path)?.GetComponent<TMP_Text>():null;
            if(target!=null)target.text=value;
        }
        private async void Connect()
        {
            var activeCode=creating?code:joinCode;
            if(activeCode==null)return;
            await Session.ConnectAsync(activeCode.text,creating);
        }
        private async void Leave() { entering=false; TitleButtonHover.ClearSelection(); await Session.LeaveAsync(); }
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
            bool entryVisible=!busy&&entering&&(state==LobbyState.Idle||state==LobbyState.Error);
            home.SetActive(!busy&&!entering&&(state==LobbyState.Idle||state==LobbyState.Error));
            entry.SetActive(entryVisible&&creating);
            if(joinEntry!=null)joinEntry.SetActive(entryVisible&&!creating);
            room.SetActive(state==LobbyState.Waiting&&!busy);
            if(entryTitle!=null)entryTitle.text=settings.createText;
            if(joinEntryTitle!=null)joinEntryTitle.text=settings.joinText;
            if(message!=null)message.text=state==LobbyState.Error&&creating?Session.Message:"";
            if(joinMessage!=null)joinMessage.text=state==LobbyState.Error&&!creating?Session.Message:"";
            var activeCode=creating?code:joinCode;
            bool canConfirm=!busy&&activeCode!=null&&!string.IsNullOrWhiteSpace(activeCode.text);
            if(confirm!=null)confirm.interactable=creating&&canConfirm;
            if(joinConfirm!=null)joinConfirm.interactable=!creating&&canConfirm;
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
                if (_availableRoleArrows != null && i < _availableRoleArrows.Length && _availableRoleArrows[i] != null)
                    _availableRoleArrows[i].SetActive(owner == 0);
                if (_occupiedRoleArrows != null && i < _occupiedRoleArrows.Length && _occupiedRoleArrows[i] != null)
                    _occupiedRoleArrows[i].SetActive(owner != 0);
                characterButtons[i].interactable=owner==0||owner==control.LocalId;
                characterLabels[i].text=owner==0?settings.selectText:owner==control.LocalId?settings.selectedText:settings.occupiedText;
                characterPlayers[i].text=owner==0?settings.vacantText:string.Format(settings.playerFormat,owner)+(owner==control.LocalId?settings.mineSuffix:"");
            }
        }
    }
}
