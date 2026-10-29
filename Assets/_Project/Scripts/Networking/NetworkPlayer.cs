using Fusion;
using Herbalist.Player;
using Herbalist.Presentation;
using UnityEngine;

namespace Herbalist.Networking
{
    public struct PlayerNetworkInput : INetworkInput
    {
        public Vector2 Move;
        public Vector2 LookAngles;
        public float ZoomMultiplier;
        public uint JumpSequence;
        public uint AbilityCycle;
        public uint AbilityUse;
        public NetworkBool AbilityHeld;
        public NetworkBool AbilityCycleHeld;
        public Vector2 MazePointer;
        public NetworkBool MazePointerValid;
    }

    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Material[] slotMaterials;
        private PlayScreenController screen;
        private PlayerCharacterPresentation characterPresentation;
        private SodamAnimationPresentation _sodamAnimation;
        public static NetworkPlayer Local { get; private set; }
        [Networked] public Vector3 SimulationPosition { get; set; }
        [Networked] public Vector3 SimulationVelocity { get; set; }
        [Networked] public NetworkBool Grounded { get; set; }
        [Networked] public float BodyYaw { get; set; }
        [Networked] public Vector2 LookAngles { get; set; }
        [Networked] public float CameraZoomMultiplier { get; set; }
        [Networked] public uint LastJumpSequence { get; set; }
        [Networked] public NetworkBool MovementBlocked { get; set; }
        [Networked] public int Slot { get; set; }
        [Networked] public uint EnteredGateMask { get; set; }
        [Networked] public NetworkBool ExternalFlight { get; set; }
        [Networked] public float ExternalGravity { get; set; }
        [Networked] public NetworkBool ExternalAirControl { get; set; }
        [Networked] public NetworkBool Transported { get; set; }
        [Networked] public Vector3 TransportPosition { get; set; }
        [Networked] public float LiftSpeed { get; set; }
        [Networked] public int RootMotionPose { get; set; }
        [Networked] public float RootMotionElapsed { get; set; }
        [Networked] public uint RootMotionSequence { get; set; }
        public void SetLiftAuthoritatively(float speed)
        {
            if (HasStateAuthority) LiftSpeed = Mathf.Max(0, speed);
        }
        public void SetTransportAuthoritatively(bool active,Vector3 position)
        {
            if(!HasStateAuthority)return;
            Transported=active;TransportPosition=position;
            if(active)TeleportAuthoritatively(position);
        }
        public PlayerController Player => player;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Local = null;

        public override void Spawned()
        {
            player.SetNetworkControl(HasInputAuthority);
            characterPresentation = GetComponent<PlayerCharacterPresentation>();
            _sodamAnimation = GetComponent<SodamAnimationPresentation>();
            if (characterPresentation != null) characterPresentation.SetNetworkSlot(Slot);
            if (HasInputAuthority) Local = this;
            if (HasStateAuthority)
            {
                SimulationPosition = transform.position;
                SimulationVelocity = Vector3.zero;
                Grounded = false;
                BodyYaw = transform.eulerAngles.y;
                LookAngles = new Vector2(BodyYaw, player.Tuning.initialPitch);
                CameraZoomMultiplier = 1f;
                RootMotionPose = -1;
            }
            screen = FindFirstObjectByType<PlayScreenController>();
            if (screen != null) screen.Register(player.View, Slot, HasInputAuthority);
            // Only state authority and the predicting owner run the physical capsule.
            GetComponent<CharacterController>().enabled = !IsProxy;
            player.View.SetLookAngles(LookAngles.x, LookAngles.y);
            if (slotMaterials.Length > 0) bodyRenderer.sharedMaterial = slotMaterials[Mathf.Clamp(Slot, 0, slotMaterials.Length - 1)];
        }
        public PlayerNetworkInput ReadLocalInput()
        {
            bool pointerValid = Herbalist.Levels.SapMazeBoard.TryReadLocalPointer(player.View, out var pointerPosition);
            var abilityInput = GetComponent<Herbalist.Abilities.AbilityInputReader>();
            return new PlayerNetworkInput
            {
                Move = player.Input.Move,
                LookAngles = new Vector2(player.View.Yaw, player.View.Pitch),
                ZoomMultiplier = player.View.ZoomMultiplier,
                JumpSequence = player.Input.JumpSequence,
                AbilityCycle = abilityInput.CycleSequence,
                AbilityUse = abilityInput.UseSequence,
                AbilityHeld = abilityInput.UseHeld,
                AbilityCycleHeld = abilityInput.CycleHeld,
                MazePointer = pointerPosition,
                MazePointerValid = pointerValid
            };
        }
        public override void FixedUpdateNetwork()
        {
            if (IsProxy) return;
            if (Herbalist.GameUI.GameplayPause.IsPaused) { if (GetInput(out PlayerNetworkInput pausedInput)) LastJumpSequence = pausedInput.JumpSequence; return; }
            if(Transported)
            {
                if (HasStateAuthority) { RootMotionPose = -1; RootMotionElapsed = 0f; }
                if(GetInput(out PlayerNetworkInput ridingInput))
                {
                    LastJumpSequence=ridingInput.JumpSequence;
                    if(Finite(ridingInput.LookAngles.x) && Finite(ridingInput.LookAngles.y))
                        LookAngles=new Vector2(Mathf.Repeat(ridingInput.LookAngles.x,360),Mathf.Clamp(ridingInput.LookAngles.y,player.Tuning.pitchLimits.x,player.Tuning.pitchLimits.y));
                    if(HasStateAuthority && Finite(ridingInput.Move.x) && Finite(ridingInput.Move.y))
                        player.RouteMovement(ridingInput.Move);
                }
                player.Motor.Teleport(TransportPosition);SimulationPosition=TransportPosition;
                SimulationVelocity=Vector3.zero;Grounded=false;return;
            }
            // All values needed for re-simulation come from Fusion's restored tick state.
            if (HasStateAuthority) MovementBlocked = player.Motor.HasMovementLockExcept(this);
            player.Motor.SetMovementLock(this, MovementBlocked);
            player.Motor.RestoreState(new MotorState { Position = SimulationPosition, Velocity = SimulationVelocity, Grounded = Grounded, EnteredGateMask = EnteredGateMask, ExternalFlight = ExternalFlight, ExternalGravity = ExternalGravity, ExternalAirControl = ExternalAirControl });
            player.Motor.SetLiftSpeed(LiftSpeed);
            Vector3 direction = Vector3.zero;
            if (GetInput(out PlayerNetworkInput input))
            {
                if (Finite(input.Move.x) && Finite(input.Move.y) && Finite(input.LookAngles.x) && Finite(input.LookAngles.y))
                {
                    if (Finite(input.ZoomMultiplier) && input.ZoomMultiplier > 0f)
                    {
                        CameraZoomMultiplier = input.ZoomMultiplier;
                        if (!HasInputAuthority) player.View.SetRemoteZoom(input.ZoomMultiplier);
                    }
                    LookAngles = new Vector2(Mathf.Repeat(input.LookAngles.x, 360), Mathf.Clamp(input.LookAngles.y, player.Tuning.pitchLimits.x, player.Tuning.pitchLimits.y));
                    var movement = Vector2.ClampMagnitude(input.Move, 1);
                    if(HasStateAuthority && player.RouteMovement(movement))movement=Vector2.zero;
                    direction = Quaternion.Euler(0, LookAngles.x, 0) * new Vector3(movement.x, 0, movement.y);
                }
                if (LastJumpSequence != input.JumpSequence)
                {
                    LastJumpSequence = input.JumpSequence;
                    player.TryJump();
                }
            }
            if (HasStateAuthority && _sodamAnimation != null)
            {
                var pose = _sodamAnimation.CurrentPose;
                int nextPose = pose.HasValue ? (int)pose.Value : -1;
                if (RootMotionPose != nextPose || RootMotionSequence != _sodamAnimation.PoseSequence)
                {
                    RootMotionPose = nextPose;
                    RootMotionSequence = _sodamAnimation.PoseSequence;
                    RootMotionElapsed = 0f;
                }
            }
            Vector3 rootMotion = Vector3.zero;
            bool rootMotionActive = false;
            if (RootMotionPose >= 0 && _sodamAnimation != null && _sodamAnimation.Profile != null)
            {
                var pose = (SodamPose)RootMotionPose;
                var definition = _sodamAnimation.Profile.Find(pose);
                if (definition != null && definition.useRootMotion && definition.rootMotionPlaybackSpeed > 0f)
                {
                    rootMotionActive = definition.overrideMovementInput &&
                        RootMotionElapsed * definition.rootMotionPlaybackSpeed < definition.rootMotionClipLength;
                    Vector3 localDelta = _sodamAnimation.Profile.RootMotionDelta(pose, RootMotionElapsed, Runner.DeltaTime);
                    rootMotion = Quaternion.Euler(0f, BodyYaw, 0f) * (localDelta * _sodamAnimation.RootMotionScale);
                    RootMotionElapsed += Runner.DeltaTime;
                }
            }
            player.Motor.Simulate(rootMotionActive ? Vector3.zero : direction * player.Tuning.moveSpeed, Runner.DeltaTime, rootMotion);
            BodyYaw = player.Tuning.ResolveBodyYaw(BodyYaw, LookAngles.x, direction, player.Motor.MovementLocked, Runner.DeltaTime);
            var state = player.Motor.CaptureState();
            SimulationPosition = state.Position; SimulationVelocity = state.Velocity; Grounded = state.Grounded;
            EnteredGateMask = state.EnteredGateMask;
            ExternalFlight = state.ExternalFlight; ExternalGravity = state.ExternalGravity; ExternalAirControl = state.ExternalAirControl;
        }
        public void LaunchAuthoritatively(Vector3 velocity, float gravity, bool allowAirControl = false)
        {
            if (!HasStateAuthority) return;
            ExternalAirControl = allowAirControl; SimulationVelocity = velocity; Grounded = false; ExternalFlight = true; ExternalGravity = gravity;
            player.Motor.Launch(velocity, gravity, allowAirControl);
        }
        public void TeleportAuthoritatively(Vector3 position)
        {
            if (!HasStateAuthority) return;
            SimulationPosition = position; SimulationVelocity = Vector3.zero; Grounded = false;
            ExternalFlight = false; ExternalGravity = 0;
            player.Motor.Teleport(position);
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public override void Render()
        {
            if (characterPresentation != null) characterPresentation.SetNetworkGrounded(Grounded);
            if (!HasInputAuthority) player.View.SetLookAngles(LookAngles.x, LookAngles.y);
            if (!HasInputAuthority) player.View.SetRemoteZoom(CameraZoomMultiplier);
            player.View.SetBodyYaw(player.Tuning.facingMode == PlayerFacingMode.CameraAligned && HasInputAuthority
                ? player.View.Yaw : BodyYaw);
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (screen != null) screen.Unregister(player.View);
            if (Local == this) Local = null;
            player.SetLocalControl(false);
        }
    }
}
