using UnityEngine;

namespace Herbalist.Player
{
    public enum PlayerFacingMode { CameraAligned, MovementDirection }

    [CreateAssetMenu(menuName = "Herbalist/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
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
    }
}
