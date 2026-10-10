using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.Presentation;
using Herbalist.StageOne;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Herbalist.Abilities
{
    // Scene-authored solo adapter. Both actors stay present; only input and the personal view change owner.
    [DefaultExecutionOrder(200)]
    public sealed class OfflinePossessionController : MonoBehaviour
    {
        [SerializeField] private PlayerController _duyeong;
        [SerializeField] private PlayerController _sodam;
        [SerializeField] private PlayScreenController _screen;
        [SerializeField] private InputActionReference _possessAction;
        [SerializeField] private CharacterRole _startingRole = CharacterRole.Duyeong;

        private InputAction _action;
        private PlayerController _controlled;
        private StageActor _duyeongActor;
        private StageActor _sodamActor;

        public PlayerController Controlled => _controlled;

        private void Awake()
        {
            if (_duyeong == null || _sodam == null || _duyeong == _sodam || _screen == null || _possessAction == null)
            {
                Debug.LogError("Assign both distinct offline players, the play screen, and the possession action.", this);
                enabled = false;
                return;
            }
            _duyeongActor = _duyeong.GetComponent<StageActor>();
            _sodamActor = _sodam.GetComponent<StageActor>();
            if (_duyeongActor == null || _sodamActor == null || _duyeongActor.Slot == _sodamActor.Slot)
            {
                Debug.LogError("Offline possession requires Stage Actors in different slots.", this);
                enabled = false;
                return;
            }
            _action = _possessAction.action.Clone();
            _action.performed += OnPossess;
        }

        private void OnEnable() => _action?.Enable();

        private void Start()
        {
            if ((FusionLobbySession.Instance != null && FusionLobbySession.Instance.HasNetworkSession) ||
                !_duyeong.gameObject.activeInHierarchy || !_sodam.gameObject.activeInHierarchy)
            {
                enabled = false;
                return;
            }

            _duyeong.GetComponent<OfflineAbilitySwitcher>()?.SetOfflineRole(CharacterRole.Duyeong);
            _sodam.GetComponent<OfflineAbilitySwitcher>()?.SetOfflineRole(CharacterRole.Sodam);
            _controlled = _startingRole == CharacterRole.Sodam ? _sodam : _duyeong;
            ApplyControl();
            _screen.SetOverride(this, PlayScreenMode.Personal);
        }

        private void OnPossess(InputAction.CallbackContext context) => TryPossessOther();

        public bool TryPossessOther()
        {
            if (_controlled == null || Herbalist.GameUI.GameplayPause.IsPaused || !GameplayCursor.AllowsPointerInput)
                return false;

            // Releasing Sodam's input while fanning must also release the authoritative fan state.
            if (_controlled == _sodam && StageOneFlow.Instance != null &&
                StageOneFlow.Instance.Minigame == StageMinigameKind.Crafting)
                _sodamActor.Request(StageCommand.FanEnd);

            _controlled = _controlled == _duyeong ? _sodam : _duyeong;
            ApplyControl();
            return true;
        }

        private void ApplyControl()
        {
            _duyeong.SetLocalControl(_controlled == _duyeong, preserveMotion: true);
            _sodam.SetLocalControl(_controlled == _sodam, preserveMotion: true);
            _screen.Register(_duyeong.View, _duyeongActor.Slot, _controlled == _duyeong);
            _screen.Register(_sodam.View, _sodamActor.Slot, _controlled == _sodam);
        }

        private void OnDisable()
        {
            _action?.Disable();
            if (_screen != null) _screen.SetOverride(this, null);
        }

        private void OnDestroy()
        {
            if (_action == null) return;
            _action.performed -= OnPossess;
            _action.Dispose();
        }
    }
}
