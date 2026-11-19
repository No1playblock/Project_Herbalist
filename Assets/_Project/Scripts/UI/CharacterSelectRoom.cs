using System.Collections;
using Herbalist.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(CharacterSelectHover))]
    public sealed class CharacterSelectRoom : MonoBehaviour
    {
        [SerializeField] private Button _start, _startPlate, _copy, _options, _back;
        [SerializeField] private TMP_Text _roomCode, _participantCount, _startLabel;
        [SerializeField] private TMP_Text[] _statuses;
        [SerializeField] private Text[] _playerNames, _badges;
        [SerializeField, Range(1f, 1.2f)] private float _copyClickScale = 1.06f;

        private CharacterSelectHover _hover;
        private MainMenuScreen _menu;
        private bool _actionPending;
        private Coroutine _copyPulse;
        private Vector3 _copyRestScale;
        private bool InRoom => FusionLobbySession.Instance?.State == LobbyState.Waiting;
        internal bool InputBlocked => _actionPending || (_menu != null && (!InRoom || _menu.RoomOptionsOpen));
        internal bool Previewing => _menu == null && !InRoom;

        private void Awake()
        {
            _hover = GetComponent<CharacterSelectHover>();
            _copyRestScale = _copy.transform.localScale;
            _menu = FindAnyObjectByType<MainMenuScreen>();
            _start.onClick.AddListener(StartGame);
            _startPlate.onClick.AddListener(StartGame);
            _copy.onClick.AddListener(CopyRoomCode);
            _options.onClick.AddListener(OpenOptions);
            _back.onClick.AddListener(Back);
        }

        internal bool CanHoverRole(int role)
        {
            var room = RoomControl.Instance;
            return !InputBlocked && (_menu == null || (room != null &&
                (room.Occupant(role) == 0 || room.LocalChoice == role)));
        }

        internal void SelectRole(int role)
        {
            if (InRoom && CanHoverRole(role) && RoomControl.Instance is { } room)
                room.Select(room.LocalChoice == role ? -1 : role);
        }

        private void Update()
        {
            var room = InRoom ? RoomControl.Instance : null;
            var players = room?.Players;
            int count = players?.Length ?? 0;
            _roomCode.text = InRoom ? FusionLobbySession.Instance.RoomName : "-";
            _participantCount.text = $"({count}/2)";
            for (int i = 0; i < _statuses.Length; i++)
            {
                int player = i < count ? players[i] : 0;
                int role = player != 0 ? room.Choice(player) : -1;
                _playerNames[i].text = player != 0 ? $"Player{i + 1}" : string.Empty;
                _badges[i].text = player == 0 ? string.Empty : role == 0 ? "두영" : role == 1 ? "소담" : $"P{i + 1}";
                _statuses[i].text = player == 0 ? "대기 중" : role < 0 ? "캐릭터 선택 중" : "준비 완료";
            }

            bool ready = room?.IsHost == true && room.Ready;
            _start.interactable = ready && !InputBlocked;
            _startPlate.interactable = _start.interactable;
            _startLabel.text = ready ? "시작하기" : "대기 중";
            _copy.interactable = InRoom && !InputBlocked;
            _options.interactable = _menu != null && !InputBlocked;
            _back.interactable = _menu != null && !InputBlocked;
        }

        private void CopyRoomCode()
        {
            if (InRoom && !InputBlocked)
            {
                GUIUtility.systemCopyBuffer = FusionLobbySession.Instance.RoomName;
                if (_copyPulse != null) StopCoroutine(_copyPulse);
                _copyPulse = StartCoroutine(CopyFeedback());
            }
        }

        private IEnumerator CopyFeedback()
        {
            var rect = (RectTransform)_copy.transform;
            float elapsed = 0f;
            while (elapsed < _hover.ClickPulseDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / _hover.ClickPulseDuration);
                rect.localScale = _copyRestScale * Mathf.Lerp(1f, _copyClickScale, Mathf.Sin(progress * Mathf.PI));
                yield return null;
            }
            rect.localScale = _copyRestScale;
            _copyPulse = null;
        }

        private void OnDisable()
        {
            if (_copyPulse != null) StopCoroutine(_copyPulse);
            _copyPulse = null;
            _copy.transform.localScale = _copyRestScale;
        }

        private void OpenOptions()
        {
            if (InputBlocked || _menu == null) return;
            _hover.PlaySettingsPulse();
            _menu.OpenRoomOptions();
        }

        private void StartGame()
        {
            if (InputBlocked || !InRoom || RoomControl.Instance is not { IsHost: true, Ready: true }) return;
            _hover.PlayStartPulse();
            StartCoroutine(AfterPulse(false));
        }

        private void Back()
        {
            if (InputBlocked || !InRoom || _menu == null) return;
            _hover.PlayBackPulse();
            StartCoroutine(AfterPulse(true));
        }

        private IEnumerator AfterPulse(bool leave)
        {
            _actionPending = true;
            yield return new WaitForSecondsRealtime(_hover.ClickPulseDuration);
            _actionPending = false;
            if (!InRoom) yield break;
            if (leave) _menu.LeaveCharacterRoom();
            else RoomControl.Instance?.StartGame();
        }
    }
}
