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
        [SerializeField] private InputActionReference unlockBothAbilities;
        private InputAction action, unlockAction;
        private PlayerAbilityController abilities;
        private void Awake()
        {
            abilities = GetComponent<PlayerAbilityController>();
            if (switchAbility == null) { enabled = false; return; }
            action = switchAbility.action.Clone();
            action.performed += OnSwitch;
            if (unlockBothAbilities != null)
            {
                unlockAction = unlockBothAbilities.action.Clone();
                unlockAction.performed += OnUnlock;
            }
        }
        private void OnEnable() { action?.Enable(); unlockAction?.Enable(); }
        private void OnDisable() { action?.Disable(); unlockAction?.Disable(); }
        private void OnSwitch(InputAction.CallbackContext context)
        {
            if (GameplayCursor.AllowsPointerInput) abilities.TrySwitchOfflineAbility();
        }
        private void OnUnlock(InputAction.CallbackContext context)
        {
            if (GameplayCursor.AllowsPointerInput) abilities.TryUnlockBothOfflineAbilities();
        }
        private void OnDestroy()
        {
            if (action == null) return;
            action.performed -= OnSwitch;
            action.Dispose();
            if (unlockAction == null) return;
            unlockAction.performed -= OnUnlock;
            unlockAction.Dispose();
        }
    }
}
