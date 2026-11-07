using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    // Authored outlet geometry, using the same flowing blue stream as the jump pads.
    [RequireComponent(typeof(MushroomWaterJetPresentation))]
    public sealed class StageOneFixedWaterStream : MonoBehaviour
    {
        [SerializeField] private SapJumpPadSettings _presentationSettings;
        [SerializeField] private Transform _source;
        [SerializeField] private Transform _end;
        [SerializeField] private CapsuleCollider _pushVolume;
        [SerializeField, Min(.01f)] private float _outletWidth = 1.1f;
        [SerializeField, Min(0)] private float _sprayWidth = 6f;
        [SerializeField, Min(0)] private float _arcRise = 1.6f;
        [SerializeField, Min(0)] private float _arcDrop = 2.2f;
        [SerializeField, Min(0)] private float _pushSpeed = 9f;
        [SerializeField, Min(0)] private float _liftSpeed = 3.5f;
        private MushroomWaterJetPresentation _presentation;
        private int Segments => Mathf.Max(6, _presentationSettings.waterStreamSegments);
        private float ArcRise => _pushVolume != null ? _arcRise : 0f;
        private float ArcDrop => _pushVolume != null ? _arcDrop : 0f;
        public Vector3 GetPathPoint(float t)
            => MushroomWaterJetPresentation.PathPoint(_source.position, _end.position, t, ArcRise, ArcDrop);

        private void Awake()
        {
            _presentation = GetComponent<MushroomWaterJetPresentation>();
            _presentation.Initialize(_presentationSettings, _pushVolume != null ? _outletWidth : 0f,
                _pushVolume != null ? _sprayWidth : 0f, conical: _pushVolume != null);
        }
        private void LateUpdate()
        {
            if (_source != null && _end != null) _presentation.Present(_source.position, _end.position, ArcRise, ArcDrop);
        }

        // Called by the owning outlet's authoritative tick, after leaf-plug state is applied.
        internal void PushOnContact()
        {
            if (!isActiveAndEnabled || _pushVolume == null || !_pushVolume.enabled ||
                _source == null || _end == null || _presentationSettings == null) return;
            var direction = Vector3.ProjectOnPlane(_end.position - _source.position, Vector3.up).normalized;
            foreach (var actor in StageActor.All)
            {
                if (!actor.Available || !Touches(actor.GetComponent<CharacterController>())) continue;
                var player = actor.GetComponent<PlayerController>();
                if (player.Motor.MovementLocked) continue;
                float verticalSpeed = player.Motor.IsGrounded ? _liftSpeed : player.Motor.Velocity.y;
                var velocity = direction * _pushSpeed + Vector3.up * verticalSpeed;
                var network = actor.GetComponent<NetworkPlayer>();
                if (network != null && network.Object != null && network.Object.IsValid)
                    network.LaunchAuthoritatively(velocity, player.Tuning.gravity, true);
                else player.Motor.Launch(velocity, player.Tuning.gravity, true);
            }
        }

        private bool Touches(CharacterController controller)
        {
            if (!controller.enabled) return false;
            var scale = controller.transform.lossyScale;
            float radius = controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float halfCore = Mathf.Max(0f, controller.height * Mathf.Abs(scale.y) * .5f - radius);
            var center = controller.transform.TransformPoint(controller.center);
            float combinedRadius = radius + _outletWidth * .5f;
            for (int i = 0; i <= Segments; i++)
            {
                var offset = GetPathPoint((float)i / Segments) - center;
                offset.y -= Mathf.Clamp(offset.y, -halfCore, halfCore);
                if (offset.sqrMagnitude <= combinedRadius * combinedRadius) return true;
            }
            return false;
        }
    }
}
