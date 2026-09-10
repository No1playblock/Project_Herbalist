using UnityEngine;

namespace Herbalist.Player
{
    [CreateAssetMenu(menuName = "Herbalist/Player Tuning")]
    public sealed class PlayerTuning : ScriptableObject
    {
        [Header("Movement")]
        [Min(0)] public float moveSpeed = 4f;
        [Min(0)] public float gravity = 22f;
        [Min(0)] public float terminalSpeed = 35f;
        [Min(0)] public float groundStickSpeed = 2f;
        [Min(0)] public float bodyTurnSpeed = 540f;
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
