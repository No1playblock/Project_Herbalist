using UnityEngine;

namespace Herbalist.Player
{
    // Presentation only: the motor/Fusion retain exclusive ownership of movement.
    [DefaultExecutionOrder(100)]
    public sealed class PlayerCharacterPresentation : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private Transform body;
        [SerializeField] private Transform lookPivot;
        [SerializeField] private GameObject characterRoot;
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer[] prototypeRenderers;
        [SerializeField] private bool showOffline = true;
        [SerializeField] private int[] visibleSlots;
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string groundedParameter = "Grounded";
        [SerializeField] private string playbackParameter = "PlaybackDirection";
        [SerializeField, Range(0, 1)] private float backwardThreshold = 0.1f;
        [SerializeField, Min(0)] private float speedDamping = 0.12f;
        [SerializeField, Min(0)] private float stoppedSpeedThreshold = 0.05f;
        [SerializeField, Min(0)] private float stopDamping = 0.04f;
        [SerializeField, Min(0.01f)] private float teleportDistance = 2f;
        private PlayerController player;
        private Transform head;
        private Vector3 previousPosition;
        private Quaternion animatedHead;
        private Quaternion lookRest;
        private bool headModified;
        private bool network;
        private bool grounded;
        private int speedId, groundedId, playbackId;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            speedId = Animator.StringToHash(speedParameter);
            groundedId = Animator.StringToHash(groundedParameter);
            playbackId = Animator.StringToHash(playbackParameter);
            lookRest = Quaternion.Inverse(body.rotation) * lookPivot.rotation;
            ApplyVisibility(showOffline);
        }
        private void OnEnable() => previousPosition = transform.position;
        public void SetNetworkSlot(int slot)
        {
            network = true;
            ApplyVisibility(System.Array.IndexOf(visibleSlots, slot) >= 0);
        }
        public void SetNetworkGrounded(bool value) => grounded = value;
        private void ApplyVisibility(bool visible)
        {
            characterRoot.SetActive(visible);
            foreach (var renderer in prototypeRenderers)
                if (renderer != null) renderer.enabled = !visible;
            if (visible)
            {
                animator.applyRootMotion = false;
                head = animator.GetBoneTransform(HumanBodyBones.Head);
            }
            previousPosition = transform.position;
        }
        private void Update()
        {
            // Remove our previous additive look before Animator evaluates this frame.
            if (headModified && head != null) head.localRotation = animatedHead;
            headModified = false;
        }
        private void LateUpdate()
        {
            animator.speed = Herbalist.GameUI.GameplayPause.IsPaused ? 0 : 1;
            Vector3 delta = transform.position - previousPosition;
            previousPosition = transform.position;
            if (!characterRoot.activeInHierarchy || Time.deltaTime <= 0) return;
            float speed = delta.magnitude > teleportDistance ? 0 : Vector3.ProjectOnPlane(delta, Vector3.up).magnitude / Time.deltaTime;
            // Use a shorter blend when stopping without snapping the current speed to zero.
            bool stopping = speed <= stoppedSpeedThreshold;
            animator.SetFloat(speedId, stopping ? 0 : speed, stopping ? stopDamping : speedDamping, Time.deltaTime);
            Vector3 planar = Vector3.ProjectOnPlane(delta, Vector3.up);
            bool backpedaling = player != null && player.Tuning.facingMode == PlayerFacingMode.CameraAligned
                && Vector3.Dot(planar.normalized, body.forward) < -backwardThreshold;
            animator.SetFloat(playbackId, backpedaling ? -1f : 1f);
            animator.SetBool(groundedId, network ? grounded : motor.IsGrounded);
            if (head == null) return;
            animatedHead = head.localRotation;
            Quaternion look = Quaternion.Inverse(body.rotation) * lookPivot.rotation * Quaternion.Inverse(lookRest);
            head.rotation = body.rotation * look * Quaternion.Inverse(body.rotation) * head.rotation;
            headModified = true;
        }
        private void OnDisable()
        {
            if (headModified && head != null) head.localRotation = animatedHead;
            headModified = false;
        }
    }
}
