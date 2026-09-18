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
        private InputAction action;
        private PlayerAbilityController abilities;
        private void Awake()
        {
            abilities = GetComponent<PlayerAbilityController>();
            if (switchAbility == null) { enabled = false; return; }
            action = switchAbility.action.Clone();
            action.performed += OnSwitch;
        }
        private void OnEnable() => action?.Enable();
        private void OnDisable() => action?.Disable();
        private void OnSwitch(InputAction.CallbackContext context)
        {
            if (GameplayCursor.AllowsPointerInput) abilities.TrySwitchOfflineAbility();
        }
        private void OnDestroy()
        {
            if (action == null) return;
            action.performed -= OnSwitch;
            action.Dispose();
        }
    }
}
