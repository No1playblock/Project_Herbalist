using Herbalist.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Herbalist.Abilities
{
    // Authored on the offline prefab only. Remove/disable this component after testing.
    [RequireComponent(typeof(PlayerAbilityController))]
    public sealed class OfflineAbilitySwitcher : MonoBehaviour
    {
        [SerializeField] private InputActionReference switchAbility;
        [SerializeField] private InputActionReference _unlockBoth;
        [SerializeField] private string _unlockMessage = "솔로 테스트: 나뭇잎·수액 해금 · {0}로 능력 전환";
        private InputAction action;
        private InputAction _unlockAction;
        private PlayerAbilityController abilities;
        private void Awake()
        {
            abilities = GetComponent<PlayerAbilityController>();
            if (switchAbility == null) { enabled = false; return; }
            action = switchAbility.action.Clone();
            action.performed += OnSwitch;
            if (_unlockBoth != null)
            {
                _unlockAction = _unlockBoth.action.Clone();
                _unlockAction.performed += OnUnlockBoth;
            }
        }
        private void OnEnable() { action?.Enable(); _unlockAction?.Enable(); }
        private void OnDisable() { action?.Disable(); _unlockAction?.Disable(); }
        private void OnUnlockBoth(InputAction.CallbackContext context)
        {
            if (GameplayCursor.AllowsPointerInput && abilities.TryUnlockBothOfflineAbilities())
                GetComponent<Herbalist.StageOne.StageActor>()?.SetFeedback(string.Format(_unlockMessage, action.GetBindingDisplayString()));
        }
        private void OnSwitch(InputAction.CallbackContext context)
        {
            if (GameplayCursor.AllowsPointerInput) abilities.TrySwitchOfflineAbility();
        }
        private void OnDestroy()
        {
            if (_unlockAction != null) { _unlockAction.performed -= OnUnlockBoth; _unlockAction.Dispose(); }
            if (action == null) return;
            action.performed -= OnSwitch;
            action.Dispose();
        }
    }
}
