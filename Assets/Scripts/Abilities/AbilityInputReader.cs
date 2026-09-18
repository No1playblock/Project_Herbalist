using Herbalist.Player;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Herbalist.Abilities
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class AbilityInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference cycle;
        [SerializeField] private InputActionReference use;
        private InputAction cycleAction, useAction;
        private PlayerController player;
        private bool active;
        public uint CycleSequence { get; private set; }
        public uint UseSequence { get; private set; }
        public bool UseHeld => active && !Herbalist.GameUI.GameplayPause.IsPaused && useAction != null && useAction.IsPressed() && Herbalist.Presentation.GameplayCursor.AllowsPointerInput;
        private void Awake()
        {
            player = GetComponent<PlayerController>();
            if (cycle == null || use == null) { Debug.LogError("Assign ability action references.", this); enabled = false; return; }
            cycleAction = cycle.action.Clone(); useAction = use.action.Clone();
            cycleAction.performed += _ => { if (active && !Herbalist.GameUI.GameplayPause.IsPaused && Herbalist.Presentation.GameplayCursor.AllowsPointerInput) CycleSequence++; };
            useAction.performed += _ => { if (active && !Herbalist.GameUI.GameplayPause.IsPaused && Herbalist.Presentation.GameplayCursor.AllowsPointerInput) UseSequence++; };
        }
        private void Update()
        {
            bool next = player.LocallyControlled && player.isActiveAndEnabled;
            if (next == active) return;
            active = next;
            if (active) { cycleAction.Enable(); useAction.Enable(); }
            else { cycleAction.Disable(); useAction.Disable(); }
        }
        private void OnDisable() { active = false; cycleAction?.Disable(); useAction?.Disable(); }
        private void OnDestroy() { cycleAction?.Dispose(); useAction?.Dispose(); }
    }
}
