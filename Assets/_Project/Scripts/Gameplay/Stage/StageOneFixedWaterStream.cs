using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    // Authored outlet geometry, using the same flowing blue stream as the jump pads.
    [RequireComponent(typeof(MushroomWaterJetPresentation))]
    [ExecuteAlways]
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

        private void Awake() => RefreshPresentation();
        private void OnEnable() => RefreshPresentation();
        private void OnValidate() => _presentation = null;
        public void RefreshPresentation()
        {
            if (_presentationSettings == null || _source == null || _end == null) return;
            _presentation = GetComponent<MushroomWaterJetPresentation>();
            // The editor preview uses the same authored mesh renderer as Play.
            if (!Application.isPlaying && _pushVolume != null && transform.Find("Conical Water Mesh") == null) return;
            _presentation.Initialize(_presentationSettings, _pushVolume != null ? _outletWidth : 0f,
                _pushVolume != null ? _sprayWidth : 0f, conical: _pushVolume != null);
            _presentation.Present(_source.position, _end.position, ArcRise, ArcDrop);
        }
        private void LateUpdate()
        {
            if (_presentation == null) RefreshPresentation();
            if (_presentation != null && _source != null && _end != null) _presentation.Present(_source.position, _end.position, ArcRise, ArcDrop);
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
            // The rendered cone widens along the complete arc. Use continuous
            // segments against the player's vertical capsule, rather than isolated bubbles.
            for (int i = 0; i < Segments; i++)
            {
                float t0 = (float)i / Segments, t1 = (float)(i + 1) / Segments;
                var a = GetPathPoint(t0);
                var b = GetPathPoint(t1);
                // Conservative enclosing radius includes visual sway and the segment's wide end.
                float width = Mathf.Lerp(_outletWidth, Mathf.Max(_outletWidth, _sprayWidth), t1);
                float combinedRadius = radius + width * .5f + _presentationSettings.waterStreamSway * 1.5f;
                float low = 0f, high = 1f;
                // Squared distance to a vertical line segment is convex along each water segment.
                for (int j = 0; j < 18; j++)
                {
                    float left = Mathf.Lerp(low, high, 1f / 3f), right = Mathf.Lerp(low, high, 2f / 3f);
                    if (CapsuleDistanceSquared(Vector3.Lerp(a, b, left) - center, halfCore) <
                        CapsuleDistanceSquared(Vector3.Lerp(a, b, right) - center, halfCore)) high = right;
                    else low = left;
                }
                if (CapsuleDistanceSquared(Vector3.Lerp(a, b, (low + high) * .5f) - center, halfCore) <=
                    combinedRadius * combinedRadius) return true;
            }
            return false;
        }
        private static float CapsuleDistanceSquared(Vector3 offset, float halfCore)
        {
            offset.y -= Mathf.Clamp(offset.y, -halfCore, halfCore);
            return offset.sqrMagnitude;
        }
    }
}
