using UnityEngine;
using UnityEngine.InputSystem;

namespace Herbalist.Player
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference look;
        [SerializeField] private InputActionReference orbit;
        [SerializeField] private InputActionReference jump;
        [SerializeField] private bool requireOrbitButton = false;
        private InputAction moveAction, lookAction, orbitAction, jumpAction;
        public uint JumpSequence { get; private set; }
        private bool active;
        public Vector2 Move => active && !Herbalist.GameUI.GameplayPause.IsPaused ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => active && Herbalist.Presentation.GameplayCursor.AllowsPointerInput && (!requireOrbitButton || orbitAction.IsPressed()) ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        public bool Initialize()
        {
            if (moveAction != null) return true;
            if (move == null || look == null || orbit == null || jump == null)
            {
                Debug.LogError("Assign Move, Look, Orbit and Jump input action references.", this);
                return false;
            }
            // Private actions: disabling a remote player must not disable the local player's input.
            moveAction = move.action.Clone();
            lookAction = look.action.Clone();
            orbitAction = orbit.action.Clone();
            jumpAction = jump.action.Clone();
            jumpAction.performed += OnJump;
            return true;
        }

        public void SetInputActive(bool value)
        {
            if (!Initialize()) return;
            active = value;
            if (active) { moveAction.Enable(); lookAction.Enable(); orbitAction.Enable(); jumpAction.Enable(); }
            else { moveAction.Disable(); lookAction.Disable(); orbitAction.Disable(); jumpAction.Disable(); }
        }
        private void OnJump(InputAction.CallbackContext context) { if (active && !Herbalist.GameUI.GameplayPause.IsPaused) JumpSequence++; }
        private void OnDisable() { if (moveAction != null) SetInputActive(false); }
        private void OnDestroy() { moveAction?.Dispose(); lookAction?.Dispose(); orbitAction?.Dispose(); jumpAction?.Dispose(); }
    }
}
