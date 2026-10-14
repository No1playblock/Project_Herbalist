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
        private Vector3 _baseLocalScale;
        private float _appliedScaleMultiplier;
        private uint lastJump;
        private IPlayerMovementReceiver _movementReceiver;
        private object _transportOwner;
        private Vector3 _transportPosition;
        public void SetMovementReceiver(IPlayerMovementReceiver receiver) => _movementReceiver=receiver;
        public void ClearMovementReceiver(IPlayerMovementReceiver receiver) { if(ReferenceEquals(_movementReceiver,receiver))_movementReceiver=null; }
        public bool RouteMovement(Vector2 move) { if(_movementReceiver==null)return false;_movementReceiver.ReceiveMovement(this,move);return true; }
        public void SetTransport(object owner,Vector3 position) { _transportOwner=owner;_transportPosition=position;motor.SetMovementLock(owner,true); }
        public void ClearTransport(object owner) { if(!ReferenceEquals(_transportOwner,owner))return;_transportOwner=null;motor.SetMovementLock(owner,false); }
        public PlayerInputReader Input => input;
        public PlayerView View => view;
        public PlayerTuning Tuning => tuning;
        public void SetNetworkControl(bool local) { networkDriven = true; SetLocalControl(local); }
        public PlayerMotor Motor => motor;
        public bool LocallyControlled => locallyControlled;

        private void Awake()
        {
            _baseLocalScale = transform.localScale;
            ApplyCharacterScale();
            ready = motor != null && input != null && view != null && tuning != null;
            if (ready) ready = input.Initialize() && view.Initialize();
            if (!ready) { Debug.LogError("Player prefab references are incomplete.", this); enabled = false; }
        }
        private void OnEnable() { ApplyCharacterScale(); if (ready) SetLocalControl(locallyControlled); }
        private void ApplyCharacterScale()
        {
            if (tuning == null) return;
            float multiplier = Mathf.Max(0.1f, tuning.characterScaleMultiplier);
            if (Mathf.Approximately(_appliedScaleMultiplier, multiplier)) return;
            transform.localScale = _baseLocalScale * multiplier;
            _appliedScaleMultiplier = multiplier;
        }
        public void SetLocalControl(bool value, bool preserveMotion = false)
        {
            locallyControlled = value;
            if (!ready) return;
            input.SetInputActive(value && isActiveAndEnabled);
            view.SetLocalView(value && isActiveAndEnabled);
            lastJump = input.JumpSequence;
            if (!preserveMotion) motor.ResetMotion();
        }
        private void Update()
        {
            ApplyCharacterScale();
            if (!ready || Herbalist.GameUI.GameplayPause.IsPaused) return;
            if (!locallyControlled)
            {
                // An unpossessed offline actor still falls and lands normally.
                if (!networkDriven)
                {
                    if (_transportOwner != null) motor.Teleport(_transportPosition);
                    else motor.Simulate(Vector3.zero, Time.deltaTime);
                }
                return;
            }
            view.ApplyLook(input.Look);
            view.ApplyZoom(input.Zoom);
            if (networkDriven) return;
            if(_transportOwner!=null){RouteMovement(input.Move);lastJump=input.JumpSequence;motor.Teleport(_transportPosition);return;}
            Vector2 movement = Vector2.ClampMagnitude(input.Move, 1);
            if(RouteMovement(movement))movement=Vector2.zero;
            Vector3 direction = view.PlanarRotation * new Vector3(movement.x, 0, movement.y);
            view.SetBodyYaw(tuning.ResolveBodyYaw(view.BodyYaw, view.Yaw, direction, motor.MovementLocked, Time.deltaTime));
            if (lastJump != input.JumpSequence) { lastJump = input.JumpSequence; TryJump(); }
            motor.Simulate(direction * tuning.moveSpeed, Time.deltaTime);
        }
        public bool TryJump() => motor.TryJump();
        private void LateUpdate() { if (ready) view.Present(Time.deltaTime, locallyControlled); }
        private void OnDisable()
        {
            if (input != null) input.SetInputActive(false);
            if (view != null) view.SetLocalView(false);
        }
    }
}
