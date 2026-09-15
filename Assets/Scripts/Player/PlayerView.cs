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
        public Camera Camera => playerCamera;
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

        public void SetPresentation(bool visible, bool audible, Rect viewport)
        {
            playerCamera.rect = viewport;
            playerCamera.enabled = visible;
            audioListener.enabled = audible;
        }

        public void ApplyLook(Vector2 delta)
        {
            Yaw = Mathf.Repeat(Yaw + delta.x * tuning.mouseSensitivity, 360f);
            Pitch = Mathf.Clamp(Pitch - delta.y * tuning.mouseSensitivity, tuning.pitchLimits.x, tuning.pitchLimits.y);
            SetBodyYaw(Yaw);
        }

        // Network presentation can use this without reading local input.
        public void SetLookAngles(float yaw, float pitch)
        {
            Yaw = Mathf.Repeat(yaw, 360f);
            Pitch = Mathf.Clamp(pitch, tuning.pitchLimits.x, tuning.pitchLimits.y);
            SetBodyYaw(Yaw);
        }

        public void Present(float deltaTime, bool local)
        {
            // Keep the camera behind the body, including owner render frames between network ticks.
            SetBodyYaw(Yaw);
            float headYaw = Mathf.Clamp(Mathf.DeltaAngle(body.eulerAngles.y, Yaw), -tuning.headYawLimit, tuning.headYawLimit);
            float headPitch = Mathf.Clamp(Pitch, -tuning.headPitchLimit, tuning.headPitchLimit);
            Quaternion target = headRest * Quaternion.Euler(headPitch, headYaw, 0);
            headPivot.localRotation = Quaternion.RotateTowards(headPivot.localRotation, target, tuning.headTurnSpeed * deltaTime);
            if (!playerCamera.enabled) return;
            Quaternion rotation = Quaternion.Euler(Pitch, Yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            float distance = tuning.cameraDistance;
            if (Physics.SphereCast(cameraTarget.position, tuning.cameraProbeRadius, backward, out RaycastHit hit,
                distance, tuning.cameraObstacles, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0, hit.distance - tuning.cameraWallPadding);
            playerCamera.transform.SetPositionAndRotation(cameraTarget.position + backward * distance, rotation);
        }

        // Also used by the host for a hidden/remote camera and by split-screen aiming.
        public Ray GetAimRay(float yaw, float pitch)
        {
            pitch = Mathf.Clamp(pitch, tuning.pitchLimits.x, tuning.pitchLimits.y);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            float distance = tuning.cameraDistance;
            if (Physics.SphereCast(cameraTarget.position, tuning.cameraProbeRadius, backward, out RaycastHit hit,
                distance, tuning.cameraObstacles, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0, hit.distance - tuning.cameraWallPadding);
            return new Ray(cameraTarget.position + backward * distance, rotation * Vector3.forward);
        }

        private void OnDisable() => SetLocalView(false);
    }
}
