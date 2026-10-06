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
        [SerializeField] private GameObject _duyeongRoot;
        [SerializeField] private Animator _duyeongAnimator;
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
        private GameObject _activeRoot;
        private Animator _activeAnimator;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            speedId = Animator.StringToHash(speedParameter);
            groundedId = Animator.StringToHash(groundedParameter);
            playbackId = Animator.StringToHash(playbackParameter);
            lookRest = Quaternion.Inverse(body.rotation) * lookPivot.rotation;
            ApplyRole(showOffline ? characterRoot : null, showOffline ? animator : null);
        }
        private void OnEnable() => previousPosition = transform.position;
        public void SetNetworkSlot(int slot)
        {
            network = true;
            bool isSodam = System.Array.IndexOf(visibleSlots, slot) >= 0;
            ApplyRole(isSodam ? characterRoot : _duyeongRoot, isSodam ? animator : _duyeongAnimator);
        }
        public void SetNetworkGrounded(bool value) => grounded = value;
        private void ApplyRole(GameObject root, Animator selectedAnimator)
        {
            if (headModified && head != null) head.localRotation = animatedHead;
            headModified = false;
            characterRoot.SetActive(root == characterRoot);
            if (_duyeongRoot != null) _duyeongRoot.SetActive(root == _duyeongRoot);
            _activeRoot = root;
            _activeAnimator = selectedAnimator;
            foreach (var renderer in prototypeRenderers)
                if (renderer != null) renderer.enabled = root == null;
            head = null;
            if (_activeAnimator != null)
            {
                _activeAnimator.applyRootMotion = false;
                head = _activeAnimator.GetBoneTransform(HumanBodyBones.Head);
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
            Vector3 delta = transform.position - previousPosition;
            previousPosition = transform.position;
            if (_activeRoot == null || _activeAnimator == null || !_activeRoot.activeInHierarchy || Time.deltaTime <= 0) return;
            _activeAnimator.speed = Herbalist.GameUI.GameplayPause.IsPaused ? 0 : 1;
            float speed = delta.magnitude > teleportDistance ? 0 : Vector3.ProjectOnPlane(delta, Vector3.up).magnitude / Time.deltaTime;
            // Use a shorter blend when stopping without snapping the current speed to zero.
            bool stopping = speed <= stoppedSpeedThreshold;
            _activeAnimator.SetFloat(speedId, stopping ? 0 : speed, stopping ? stopDamping : speedDamping, Time.deltaTime);
            Vector3 planar = Vector3.ProjectOnPlane(delta, Vector3.up);
            bool backpedaling = player != null && player.Tuning.facingMode == PlayerFacingMode.CameraAligned
                && Vector3.Dot(planar.normalized, body.forward) < -backwardThreshold;
            _activeAnimator.SetFloat(playbackId, backpedaling ? -1f : 1f);
            _activeAnimator.SetBool(groundedId, network ? grounded : motor.IsGrounded);
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
