using UnityEngine;
namespace Herbalist.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControllerMotor : PlayerMotor
    {
        [SerializeField] private CharacterController controller;
        [SerializeField] private PlayerTuning tuning;
        private Vector3 velocity;
        private bool grounded;
        private uint _enteredGateMask;
        public override bool IsGrounded => grounded;
        public override Vector3 Velocity => velocity;
        private void Awake()
        {
            if (controller == null || tuning == null)
            { Debug.LogError("Assign controller and tuning on the player motor.", this); enabled = false; }
        }
        public override bool TryJump()
        {
            if (!isActiveAndEnabled || MovementLocked || !grounded) return false;
            velocity.y = Mathf.Sqrt(2f * tuning.gravity * tuning.jumpHeight);
            grounded = false;
            return true;
        }
        public override void Simulate(Vector3 desiredWorldVelocity, float deltaTime)
        {
            if (!isActiveAndEnabled || !controller.enabled || deltaTime <= 0) return;
            if (grounded && velocity.y < 0) velocity.y = -tuning.groundStickSpeed;
            velocity.y = Mathf.Max(velocity.y - tuning.gravity * deltaTime, -tuning.terminalSpeed);
            Vector3 desired = Vector3.ProjectOnPlane(desiredWorldVelocity, Vector3.up);
            Vector3 horizontal = Vector3.ProjectOnPlane(velocity, Vector3.up);
            horizontal = MovementLocked ? Vector3.zero : grounded ? desired : Vector3.MoveTowards(horizontal, desired, tuning.airAcceleration * deltaTime);
            velocity.x = horizontal.x; velocity.z = horizontal.z;
            Herbalist.Interaction.OneWayPlatform.PrepareMove(controller, velocity);
            Vector3 previousCenter = transform.TransformPoint(controller.center);
            Herbalist.Levels.StageEntranceGate.PrepareMove(controller, _enteredGateMask);
            CollisionFlags flags = controller.Move(velocity * deltaTime);
            _enteredGateMask = Herbalist.Levels.StageEntranceGate.FinishMove(controller, previousCenter, _enteredGateMask);
            grounded = (flags & CollisionFlags.Below) != 0;
            if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0) velocity.y = 0;
            if (grounded && velocity.y < 0) velocity.y = -tuning.groundStickSpeed;
        }
        public override MotorState CaptureState() => new MotorState { Position = transform.position, Velocity = velocity, Grounded = grounded, EnteredGateMask = _enteredGateMask };
        public override void RestoreState(MotorState state)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = state.Position;
            velocity = state.Velocity; grounded = state.Grounded; _enteredGateMask = state.EnteredGateMask;
            controller.enabled = wasEnabled;
        }
        public override void ResetMotion() { velocity = Vector3.zero; grounded = false; }
        private void OnDisable() { Herbalist.Interaction.OneWayPlatform.Release(controller); Herbalist.Levels.StageEntranceGate.Release(controller); }
        public override void Teleport(Vector3 position) => RestoreState(new MotorState { Position = position, EnteredGateMask = _enteredGateMask });
    }
}
