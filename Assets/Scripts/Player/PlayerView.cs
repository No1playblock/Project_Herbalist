using UnityEngine;

namespace Herbalist.Player
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private PlayerTuning tuning;
        [SerializeField] private Transform body;
        [SerializeField] private Transform headPivot;
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;
        private Quaternion headRest;
        private bool initialized;
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public void SetBodyYaw(float yaw) => body.rotation = Quaternion.Euler(0, yaw, 0);
        public Quaternion PlanarRotation => Quaternion.Euler(0, Yaw, 0);

        public bool Initialize()
        {
            if (initialized) return true;
            if (tuning == null || body == null || headPivot == null || cameraTarget == null || playerCamera == null || audioListener == null)
            {
                Debug.LogError("Assign all PlayerView references.", this);
                return false;
            }
            headRest = headPivot.localRotation;
            Yaw = body.eulerAngles.y;
            Pitch = tuning.initialPitch;
            initialized = true;
            return true;
        }

        public void SetLocalView(bool active)
        {
            if (playerCamera != null) playerCamera.enabled = active;
            if (audioListener != null) audioListener.enabled = active;
        }

        public void ApplyLook(Vector2 delta)
        {
            Yaw = Mathf.Repeat(Yaw + delta.x * tuning.mouseSensitivity, 360f);
            Pitch = Mathf.Clamp(Pitch - delta.y * tuning.mouseSensitivity, tuning.pitchLimits.x, tuning.pitchLimits.y);
        }

        // Network presentation can use this without reading local input.
        public void SetLookAngles(float yaw, float pitch)
        {
            Yaw = Mathf.Repeat(yaw, 360f);
            Pitch = Mathf.Clamp(pitch, tuning.pitchLimits.x, tuning.pitchLimits.y);
        }

        public void FaceMovement(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon) return;
            body.rotation = Quaternion.RotateTowards(body.rotation, Quaternion.LookRotation(direction, Vector3.up), tuning.bodyTurnSpeed * deltaTime);
        }

        public void Present(float deltaTime, bool local)
        {
            float headYaw = Mathf.Clamp(Mathf.DeltaAngle(body.eulerAngles.y, Yaw), -tuning.headYawLimit, tuning.headYawLimit);
            float headPitch = Mathf.Clamp(Pitch, -tuning.headPitchLimit, tuning.headPitchLimit);
            Quaternion target = headRest * Quaternion.Euler(headPitch, headYaw, 0);
            headPivot.localRotation = Quaternion.RotateTowards(headPivot.localRotation, target, tuning.headTurnSpeed * deltaTime);
            if (!local) return;
            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            float distance = tuning.cameraDistance;
            if (Physics.SphereCast(cameraTarget.position, tuning.cameraProbeRadius, backward, out RaycastHit hit,
                distance, tuning.cameraObstacles, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0, hit.distance - tuning.cameraWallPadding);
            playerCamera.transform.SetPositionAndRotation(cameraTarget.position + backward * distance, rotation);
        }

        private void OnDisable() => SetLocalView(false);
    }
}
