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
        private InputAction moveAction, lookAction, orbitAction, jumpAction, zoomAction;
        private Vector2 _previousMove;
        public event System.Action<Vector2> MovePerformed;
        public uint JumpSequence { get; private set; }
        private bool active;
        public Vector2 Move => active && !Herbalist.GameUI.GameplayPause.IsPaused ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => active && Herbalist.Presentation.GameplayCursor.AllowsPointerInput && (!requireOrbitButton || orbitAction.IsPressed()) ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        public float Zoom => active && !Herbalist.GameUI.GameplayPause.IsPaused && Herbalist.Presentation.GameplayCursor.AllowsPointerInput
            ? zoomAction.ReadValue<Vector2>().y : 0f;

        public bool Initialize()
        {
            if (moveAction != null) return true;
            if (move == null || look == null || orbit == null || jump == null || look.action.actionMap.FindAction("Zoom") == null)
            {
                Debug.LogError("Assign Move, Look, Orbit and Jump references, and add Zoom to the Player action map.", this);
                return false;
            }
            // Private actions: disabling a remote player must not disable the local player's input.
            moveAction = move.action.Clone();
            moveAction.performed += context =>
            {
                Vector2 current = context.ReadValue<Vector2>();
                if (_previousMove.sqrMagnitude < .25f && current.sqrMagnitude >= .25f) MovePerformed?.Invoke(current);
                _previousMove = current;
            };
            moveAction.canceled += _ => _previousMove = Vector2.zero;
            lookAction = look.action.Clone();
            orbitAction = orbit.action.Clone();
            jumpAction = jump.action.Clone();
            zoomAction = look.action.actionMap.FindAction("Zoom").Clone();
            jumpAction.performed += OnJump;
            return true;
        }

        public void SetInputActive(bool value)
        {
            if (!Initialize()) return;
            active = value;
            if (!active) _previousMove = Vector2.zero;
            if (active) { moveAction.Enable(); lookAction.Enable(); orbitAction.Enable(); jumpAction.Enable(); zoomAction.Enable(); }
            else { moveAction.Disable(); lookAction.Disable(); orbitAction.Disable(); jumpAction.Disable(); zoomAction.Disable(); }
        }
        private void OnJump(InputAction.CallbackContext context) { if (active && !Herbalist.GameUI.GameplayPause.IsPaused) JumpSequence++; }
        private void OnDisable() { if (moveAction != null) SetInputActive(false); }
        private void OnDestroy() { moveAction?.Dispose(); lookAction?.Dispose(); orbitAction?.Dispose(); jumpAction?.Dispose(); zoomAction?.Dispose(); }
    }
}
