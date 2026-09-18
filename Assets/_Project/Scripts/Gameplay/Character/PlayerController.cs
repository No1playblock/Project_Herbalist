using UnityEngine;

namespace Herbalist.Player
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerView view;
        [SerializeField] private PlayerTuning tuning;
        [Tooltip("Offline default. Photon adapter must set ownership before the first simulation tick.")]
        [SerializeField] private bool locallyControlled = true;
        private bool ready;
        private bool networkDriven;
        private uint lastJump;
        public PlayerInputReader Input => input;
        public PlayerView View => view;
        public PlayerTuning Tuning => tuning;
        public void SetNetworkControl(bool local) { networkDriven = true; SetLocalControl(local); }
        public PlayerMotor Motor => motor;
        public bool LocallyControlled => locallyControlled;

        private void Awake()
        {
            ready = motor != null && input != null && view != null && tuning != null;
            if (ready) ready = input.Initialize() && view.Initialize();
            if (!ready) { Debug.LogError("Player prefab references are incomplete.", this); enabled = false; }
        }
        private void OnEnable() { if (ready) SetLocalControl(locallyControlled); }
        public void SetLocalControl(bool value)
        {
            locallyControlled = value;
            if (!ready) return;
            input.SetInputActive(value && isActiveAndEnabled);
            view.SetLocalView(value && isActiveAndEnabled);
            lastJump = input.JumpSequence;
            motor.ResetMotion();
        }
        private void Update()
        {
            if (!ready || !locallyControlled || Herbalist.GameUI.GameplayPause.IsPaused) return;
            view.ApplyLook(input.Look);
            if (networkDriven) return;
            Vector2 movement = Vector2.ClampMagnitude(input.Move, 1);
            Vector3 direction = view.PlanarRotation * new Vector3(movement.x, 0, movement.y);
            view.SetBodyYaw(tuning.ResolveBodyYaw(view.BodyYaw, view.Yaw, direction, motor.MovementLocked, Time.deltaTime));
            if (lastJump != input.JumpSequence) { lastJump = input.JumpSequence; motor.TryJump(); }
            motor.Simulate(direction * tuning.moveSpeed, Time.deltaTime);
        }
        private void LateUpdate() { if (ready) view.Present(Time.deltaTime, locallyControlled); }
        private void OnDisable()
        {
            if (input != null) input.SetInputActive(false);
            if (view != null) view.SetLocalView(false);
        }
    }
}
