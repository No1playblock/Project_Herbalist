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
        public uint JumpSequence;
        public uint AbilityCycle;
        public uint AbilityUse;
        public NetworkBool AbilityHeld;
    }

    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Material[] slotMaterials;
        private PlayScreenController screen;
        private PlayerCharacterPresentation characterPresentation;
        public static NetworkPlayer Local { get; private set; }
        [Networked] public Vector3 SimulationPosition { get; set; }
        [Networked] public Vector3 SimulationVelocity { get; set; }
        [Networked] public NetworkBool Grounded { get; set; }
        [Networked] public float BodyYaw { get; set; }
        [Networked] public Vector2 LookAngles { get; set; }
        [Networked] public uint LastJumpSequence { get; set; }
        [Networked] public NetworkBool MovementBlocked { get; set; }
        [Networked] public int Slot { get; set; }
        [Networked] public uint EnteredGateMask { get; set; }
        public PlayerController Player => player;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Local = null;

        public override void Spawned()
        {
            player.SetNetworkControl(HasInputAuthority);
            characterPresentation = GetComponent<PlayerCharacterPresentation>();
            if (characterPresentation != null) characterPresentation.SetNetworkSlot(Slot);
            if (HasInputAuthority) Local = this;
            if (HasStateAuthority)
            {
                SimulationPosition = transform.position;
                SimulationVelocity = Vector3.zero;
                Grounded = false;
                BodyYaw = transform.eulerAngles.y;
                LookAngles = new Vector2(BodyYaw, player.Tuning.initialPitch);
            }
            screen = FindFirstObjectByType<PlayScreenController>();
            if (screen != null) screen.Register(player.View, Slot, HasInputAuthority);
            // Only state authority and the predicting owner run the physical capsule.
            GetComponent<CharacterController>().enabled = !IsProxy;
            player.View.SetLookAngles(LookAngles.x, LookAngles.y);
            if (slotMaterials.Length > 0) bodyRenderer.sharedMaterial = slotMaterials[Mathf.Clamp(Slot, 0, slotMaterials.Length - 1)];
        }
        public PlayerNetworkInput ReadLocalInput() => new PlayerNetworkInput
        {
            Move = player.Input.Move,
            LookAngles = new Vector2(player.View.Yaw, player.View.Pitch),
            JumpSequence = player.Input.JumpSequence,
            AbilityCycle = GetComponent<Herbalist.Abilities.AbilityInputReader>().CycleSequence,
            AbilityUse = GetComponent<Herbalist.Abilities.AbilityInputReader>().UseSequence,
            AbilityHeld = GetComponent<Herbalist.Abilities.AbilityInputReader>().UseHeld
        };
        public override void FixedUpdateNetwork()
        {
            if (IsProxy) return;
            if (Herbalist.GameUI.GameplayPause.IsPaused) { if (GetInput(out PlayerNetworkInput pausedInput)) LastJumpSequence = pausedInput.JumpSequence; return; }
            // All values needed for re-simulation come from Fusion's restored tick state.
            if (HasStateAuthority) MovementBlocked = player.Motor.HasMovementLockExcept(this);
            player.Motor.SetMovementLock(this, MovementBlocked);
            player.Motor.RestoreState(new MotorState { Position = SimulationPosition, Velocity = SimulationVelocity, Grounded = Grounded, EnteredGateMask = EnteredGateMask });
            Vector3 direction = Vector3.zero;
            if (GetInput(out PlayerNetworkInput input))
            {
                if (Finite(input.Move.x) && Finite(input.Move.y) && Finite(input.LookAngles.x) && Finite(input.LookAngles.y))
                {
                    var movement = Vector2.ClampMagnitude(input.Move, 1);
                    LookAngles = new Vector2(Mathf.Repeat(input.LookAngles.x, 360), Mathf.Clamp(input.LookAngles.y, player.Tuning.pitchLimits.x, player.Tuning.pitchLimits.y));
                    direction = Quaternion.Euler(0, LookAngles.x, 0) * new Vector3(movement.x, 0, movement.y);
                }
                if (LastJumpSequence != input.JumpSequence)
                {
                    LastJumpSequence = input.JumpSequence;
                    player.Motor.TryJump();
                }
            }
            player.Motor.Simulate(direction * player.Tuning.moveSpeed, Runner.DeltaTime);
            BodyYaw = player.Tuning.ResolveBodyYaw(BodyYaw, LookAngles.x, direction, player.Motor.MovementLocked, Runner.DeltaTime);
            var state = player.Motor.CaptureState();
            SimulationPosition = state.Position; SimulationVelocity = state.Velocity; Grounded = state.Grounded;
            EnteredGateMask = state.EnteredGateMask;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public override void Render()
        {
            if (characterPresentation != null) characterPresentation.SetNetworkGrounded(Grounded);
            if (!HasInputAuthority) player.View.SetLookAngles(LookAngles.x, LookAngles.y);
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
