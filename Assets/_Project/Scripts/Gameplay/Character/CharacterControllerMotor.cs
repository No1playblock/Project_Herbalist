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
        private bool _externalFlight;
        private float _externalGravity;
        private bool _externalAirControl;
        private float _liftSpeed;
        private CharacterController _launchCollisionPartner;
        private bool _ownsLaunchCollisionIgnore;
        private Herbalist.Levels.StageOneTraversal _treeTraversal;
        private bool _bareTreeContact;
        private bool _walkableContact;
        internal Collider WalkableGround { get; private set; }
        public override bool IsGrounded => grounded;
        public override Vector3 Velocity => velocity;
        private void Awake()
        {
            if (controller == null || tuning == null)
            { Debug.LogError("Assign controller and tuning on the player motor.", this); enabled = false; }
        }
        public override bool TryJump()
        {
            if (!isActiveAndEnabled || MovementLocked || _externalFlight || !grounded) return false;
            velocity.y = Mathf.Sqrt(2f * tuning.gravity * tuning.jumpHeight);
            grounded = false;
            return true;
        }

        public override void Simulate(Vector3 desiredWorldVelocity, float deltaTime)
            => Simulate(desiredWorldVelocity, deltaTime, Vector3.zero);

        public override void Simulate(Vector3 desiredWorldVelocity, float deltaTime, Vector3 rootMotionDisplacement)
        {
            if (!isActiveAndEnabled || !controller.enabled || deltaTime <= 0) return;
            if (grounded && velocity.y < 0) velocity.y = -tuning.groundStickSpeed;
            velocity.y = _liftSpeed > 0 ? _liftSpeed : _externalFlight ? velocity.y - _externalGravity * deltaTime : Mathf.Max(velocity.y - tuning.gravity * deltaTime, -tuning.terminalSpeed);
            Vector3 desired = Vector3.ProjectOnPlane(desiredWorldVelocity, Vector3.up);
            Vector3 horizontal = Vector3.ProjectOnPlane(velocity, Vector3.up);
            if (!(_externalFlight && !_externalAirControl))
            {
                if (MovementLocked) horizontal = Vector3.zero;
                else if (grounded) horizontal = desired;
                else if (_externalFlight && _externalAirControl)
                {
                    // Steering changes the launch direction without braking its inherited momentum when input is released.
                    if (desired.sqrMagnitude > .0001f)
                    {
                        float speed = Mathf.Max(horizontal.magnitude, desired.magnitude);
                        horizontal = Vector3.MoveTowards(horizontal, desired.normalized * speed, tuning.airAcceleration * deltaTime);
                    }
                }
                else horizontal = Vector3.MoveTowards(horizontal, desired, tuning.airAcceleration * deltaTime);
            }
            velocity.x = horizontal.x; velocity.z = horizontal.z;
            Vector3 displacement = velocity * deltaTime + rootMotionDisplacement;
            Vector3 previousCenter = transform.TransformPoint(controller.center);
            _treeTraversal = Herbalist.Levels.StageOneTraversal.Instance;
            if (_launchCollisionPartner != null) SetLaunchCollisionPartner(_launchCollisionPartner);
            CollisionFlags flags = Move(displacement, deltaTime);
            _enteredGateMask = Herbalist.Levels.StageEntranceGate.FinishMove(controller, previousCenter, _enteredGateMask);
            grounded = (flags & CollisionFlags.Below) != 0 && (!_bareTreeContact || _walkableContact);
            _treeTraversal = null;
            if (grounded && velocity.y <= 0) _externalFlight = false;
            if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0) velocity.y = 0;
            if (grounded && velocity.y < 0) velocity.y = -tuning.groundStickSpeed;
            UpdateLaunchCollisionPartner();
        }

        private void SetLaunchCollisionPartner(CharacterController partner)
        {
            if (!ReferenceEquals(_launchCollisionPartner, partner))
            {
                if (_ownsLaunchCollisionIgnore && _launchCollisionPartner != null)
                    Physics.IgnoreCollision(controller, _launchCollisionPartner, false);
                _launchCollisionPartner = partner;
                _ownsLaunchCollisionIgnore = false;
            }

            if (partner == null || !partner.enabled || !partner.gameObject.activeInHierarchy || !controller.enabled ||
                Physics.GetIgnoreCollision(controller, partner)) return;
            Physics.IgnoreCollision(controller, partner);
            _ownsLaunchCollisionIgnore = true;
        }

        private void UpdateLaunchCollisionPartner()
        {
            var partner = _launchCollisionPartner;
            if (partner == null || !partner.gameObject.activeInHierarchy)
            {
                SetLaunchCollisionPartner(null);
                return;
            }
            if (!partner.enabled)
            {
                if (velocity.y <= 0f) SetLaunchCollisionPartner(null);
                return;
            }

            var capsule = controller.bounds;
            var other = partner.bounds;
            float gap = other.min.y - capsule.max.y;
            // Keep the pair ignored through overlap, even if a ceiling interrupts the ascent.
            float skin = controller.skinWidth * Mathf.Abs(controller.transform.lossyScale.y) +
                partner.skinWidth * Mathf.Abs(partner.transform.lossyScale.y);
            bool stillApproaching = velocity.y > 0f && gap > 0f && gap <= skin &&
                capsule.min.x <= other.max.x && capsule.max.x >= other.min.x &&
                capsule.min.z <= other.max.z && capsule.max.z >= other.min.z;
            if (!capsule.Intersects(other) && !stillApproaching) SetLaunchCollisionPartner(null);
        }

        private CollisionFlags Move(Vector3 displacement, float deltaTime)
        {
            Vector3 start = transform.position;
            _bareTreeContact = _walkableContact = false;
            WalkableGround = null;
            Herbalist.Interaction.OneWayPlatform.PrepareMove(controller, displacement / deltaTime);
            Herbalist.Levels.StageEntranceGate.PrepareMove(controller, _enteredGateMask);
            CollisionFlags flags = controller.Move(displacement);
            float maximumY = start.y + Mathf.Max(0f, displacement.y);
            if (!_bareTreeContact || transform.position.y <= maximumY || Mathf.Approximately(transform.position.y, maximumY))
                return flags;

            // A sideways move into the tree can step or slide upward. Retry the vertical part
            // independently so a jump, drop or launch still has its intended motion.
            SetControllerPosition(start);
            _bareTreeContact = _walkableContact = false;
            WalkableGround = null;
            Vector3 vertical = Vector3.up * displacement.y;
            Herbalist.Interaction.OneWayPlatform.PrepareMove(controller, vertical / deltaTime);
            Herbalist.Levels.StageEntranceGate.PrepareMove(controller, _enteredGateMask);
            flags = controller.Move(vertical);
            if (transform.position.y <= maximumY || Mathf.Approximately(transform.position.y, maximumY)) return flags;

            SetControllerPosition(start);
            return CollisionFlags.Sides;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (_treeTraversal == null) return;
            if (_treeTraversal.IsBareTreeContact(hit.collider, hit.point)) _bareTreeContact = true;
            else if (hit.normal.y >= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad))
            {
                _walkableContact = true;
                WalkableGround = hit.collider;
            }
        }

        private void SetControllerPosition(Vector3 position)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = position;
            controller.enabled = wasEnabled;
        }
        public override MotorState CaptureState() => new MotorState
        {
            Position = transform.position, Velocity = velocity, Grounded = grounded, EnteredGateMask = _enteredGateMask,
            ExternalFlight = _externalFlight, ExternalGravity = _externalGravity, ExternalAirControl = _externalAirControl,
            LaunchCollisionPartner = _launchCollisionPartner
        };
        public override void RestoreState(MotorState state)
        {
            SetControllerPosition(state.Position);
            WalkableGround = null;
            _externalFlight = state.ExternalFlight; _externalGravity = state.ExternalGravity; _externalAirControl = state.ExternalAirControl;
            velocity = state.Velocity; grounded = state.Grounded; _enteredGateMask = state.EnteredGateMask;
            SetLaunchCollisionPartner(state.LaunchCollisionPartner);
        }
        public override void Launch(Vector3 launchVelocity, float gravity, bool allowAirControl = false,
            CharacterController collisionPartner = null)
        {
            _liftSpeed = 0; _externalAirControl = allowAirControl; velocity = launchVelocity; grounded = false;
            _externalFlight = true; _externalGravity = Mathf.Max(0, gravity);
            SetLaunchCollisionPartner(collisionPartner);
        }
        public override void SetLiftSpeed(float speed) => _liftSpeed = Mathf.Max(0, speed);
        public override void ResetMotion()
        {
            _liftSpeed = 0; velocity = Vector3.zero; grounded = false; _externalFlight = false; _externalGravity = 0;
            SetLaunchCollisionPartner(null);
        }
        private void OnDisable()
        {
            SetLaunchCollisionPartner(null);
            Herbalist.Interaction.OneWayPlatform.Release(controller);
            Herbalist.Levels.StageEntranceGate.Release(controller);
        }
        public override void Teleport(Vector3 position) => RestoreState(new MotorState { Position = position, EnteredGateMask = _enteredGateMask });
    }
}
