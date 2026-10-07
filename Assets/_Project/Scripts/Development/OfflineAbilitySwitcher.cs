using Herbalist.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
namespace Herbalist.Abilities
{
    // Authored on the offline prefab only. Remove/disable this component after testing.
    [RequireComponent(typeof(PlayerAbilityController))]
    public sealed class OfflineAbilitySwitcher : MonoBehaviour
    {
        [SerializeField] private InputActionReference switchAbility;
        [FormerlySerializedAs("unlockBothAbilities")]
        [SerializeField] private InputActionReference _unlockBoth;
        [SerializeField] private string _unlockMessage = "솔로 테스트: 나뭇잎·수액 해금 · {0}로 능력 전환";
        private InputAction action;
        private InputAction _unlockAction;
        private InputAction _appearanceAction;
        private Herbalist.Player.PlayerCharacterPresentation _presentation;

        private PlayerAbilityController abilities;
private void Awake()
        {
            abilities = GetComponent<PlayerAbilityController>();
            _presentation = GetComponent<Herbalist.Player.PlayerCharacterPresentation>();
            if (switchAbility == null) { enabled = false; return; }

            action = switchAbility.action.Clone();
            action.performed += OnSwitch;

            var source = switchAbility.action.actionMap.asset.FindAction("Presentation/ToggleAppearance", false);
            if (source != null)
            {
                _appearanceAction = source.Clone();
                _appearanceAction.performed += OnToggleAppearance;
            }

            if (_unlockBoth != null)
            {
                _unlockAction = _unlockBoth.action.Clone();
                _unlockAction.performed += OnUnlockBoth;
            }
        }
private void OnEnable() { action?.Enable(); _unlockAction?.Enable(); _appearanceAction?.Enable(); }
private void OnDisable() { action?.Disable(); _unlockAction?.Disable(); _appearanceAction?.Disable(); }
        private void OnUnlockBoth(InputAction.CallbackContext context)
        {
            if (GameplayCursor.AllowsPointerInput && abilities.TryUnlockBothOfflineAbilities())
                GetComponent<Herbalist.StageOne.StageActor>()?.SetFeedback(string.Format(_unlockMessage, action.GetBindingDisplayString()));
        }
        private void OnSwitch(InputAction.CallbackContext context)
        {
            if (GameplayCursor.AllowsPointerInput) abilities.TrySwitchOfflineAbility();
        }

private void OnToggleAppearance(InputAction.CallbackContext context)
        {
            if (!Herbalist.GameUI.GameplayPause.IsPaused && GameplayCursor.AllowsPointerInput)
                _presentation?.TryToggleOfflineAppearance();
        }

        private void OnDestroy()
        {
            if (_unlockAction != null) { _unlockAction.performed -= OnUnlockBoth; _unlockAction.Dispose(); }
            if (_appearanceAction != null) { _appearanceAction.performed -= OnToggleAppearance; _appearanceAction.Dispose(); }

            if (action == null) return;
            action.performed -= OnSwitch;
            action.Dispose();
        }
    }
}
