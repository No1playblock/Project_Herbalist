using UnityEngine;

namespace Herbalist.Player
{
    public enum PlayerFacingMode { CameraAligned, MovementDirection }

    [CreateAssetMenu(menuName = "Herbalist/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [Header("Character Size")]
        [Tooltip("Uniform multiplier for the player prefab, including its model, collision capsule, camera target and ability launch points. Set before spawning players; use the same asset on Host and Client.")]
        [Min(0.1f)] public float characterScaleMultiplier = 1f;

        [Header("Movement")]
        [Min(0)] public float moveSpeed = 4f;
        [Min(0)] public float gravity = 22f;
        [Min(0)] public float terminalSpeed = 35f;
        [Min(0)] public float groundStickSpeed = 2f;
        [Min(0)] public float bodyTurnSpeed = 540f;
        public PlayerFacingMode facingMode = PlayerFacingMode.CameraAligned;

        public float ResolveBodyYaw(float currentYaw, float cameraYaw, Vector3 movement, bool movementLocked, float deltaTime)
        {
            if (facingMode == PlayerFacingMode.CameraAligned) return cameraYaw;
            if (movementLocked || movement.sqrMagnitude < 0.0001f) return currentYaw;
            float target = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            return Mathf.MoveTowardsAngle(currentYaw, target, bodyTurnSpeed * deltaTime);
        }
        [Header("Jump")]
        [Min(0)] public float jumpHeight = 1.5f;
        [Min(0)] public float airAcceleration = 16f;
        [Header("Orbit camera")]
        [Min(0.1f)] public float cameraDistance = 6f;
        [Min(0.1f)] public float cameraZoomMinMultiplier = 0.5f;
        [Min(0.1f)] public float cameraZoomMaxMultiplier = 1.5f;
        [Min(0.01f)] public float cameraZoomStep = 0.1f;
        public float initialPitch = 35f;
        public Vector2 pitchLimits = new Vector2(15f, 65f);
        [Min(0)] public float mouseSensitivity = 0.15f;
        [Min(0.01f)] public float cameraProbeRadius = 0.2f;
        [Min(0)] public float cameraWallPadding = 0.1f;
        public LayerMask cameraObstacles = 1;
        [Header("Head relative to body")]
        [Range(0, 180)] public float headYawLimit = 75f;
        [Range(0, 90)] public float headPitchLimit = 40f;
        [Min(0)] public float headTurnSpeed = 240f;
        [Tooltip("Body-relative view yaw where head tracking fades out, then returns to neutral.")]
        public Vector2 headTrackingFadeAngles = new Vector2(75f, 90f);

        public Quaternion ResolveHeadLook(float bodyYaw, float viewYaw, float viewPitch)
        {
            float yaw = Mathf.DeltaAngle(bodyYaw, viewYaw);
            float weight = 1f - Mathf.InverseLerp(headTrackingFadeAngles.x, headTrackingFadeAngles.y, Mathf.Abs(yaw));
            return Quaternion.Euler(Mathf.Clamp(viewPitch, -headPitchLimit, headPitchLimit) * weight,
                Mathf.Clamp(yaw, -headYawLimit, headYawLimit) * weight, 0);
        }

        public bool CanAimWithHead(float bodyYaw, Vector3 aimDirection)
        {
            if (aimDirection.sqrMagnitude < 0.000001f) return false;
            float viewYaw = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
            float viewPitch = Mathf.Atan2(-aimDirection.y,
                new Vector2(aimDirection.x, aimDirection.z).magnitude) * Mathf.Rad2Deg;
            float fullTrackingYaw = Mathf.Min(headYawLimit, headTrackingFadeAngles.x);
            return Mathf.Abs(Mathf.DeltaAngle(bodyYaw, viewYaw)) <= fullTrackingYaw &&
                Mathf.Abs(viewPitch) <= headPitchLimit;
        }
    }
}
