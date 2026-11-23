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
        [SerializeField] private Transform _visibleMouth;
        [SerializeField] private Collider _coveredMushroomCap;
        [SerializeField, Min(0)] private float _capPushHeight = 1.5f;
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
            PresentWater();
        }
        private void LateUpdate()
        {
            if (_presentation == null) RefreshPresentation();
            if (_presentation != null && _source != null && _end != null) PresentWater();
        }

        private void PresentWater() => _presentation.Present(_source.position, _end.position, ArcRise, ArcDrop,
            _visibleMouth != null ? _visibleMouth.position : (Vector3?)null);

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
            float feet = center.y - controller.height * Mathf.Abs(scale.y) * .5f;
            if (TouchesCoveredCap(center, feet, radius)) return true;

            bool pouring = _presentation != null && _presentation.HasPourShape;
            int segments = pouring ? Segments * 2 : Segments;
            Vector3? mouth = _visibleMouth != null ? _visibleMouth.position : (Vector3?)null;
            Vector3 Point(float t) => pouring
                ? _presentation.PourPathPoint(_source.position, _end.position, t, ArcRise, ArcDrop, mouth)
                : GetPathPoint(t);
            var a = Point(0f);
            for (int i = 0; i < segments; i++)
            {
                float t0 = (float)i / segments, t1 = (float)(i + 1) / segments;
                var b = Point(t1);
                float horizontal = Mathf.Lerp(_outletWidth, Mathf.Max(_outletWidth, _sprayWidth), t1) * .5f;
                float thickness = 1f;
                if (pouring)
                {
                    float spread = MushroomWaterJetPresentation.PourSpread(t1);
                    horizontal = MushroomWaterJetPresentation.PourRadius(_outletWidth, _sprayWidth, t1)
                        * MushroomWaterJetPresentation.PourRippleEnvelope(spread);
                    thickness = MushroomWaterJetPresentation.PourThickness(MushroomWaterJetPresentation.PourSpread(t0));
                }
                float padding = radius + _presentationSettings.waterStreamSway * 1.5f;
                float combinedRadius = horizontal + padding;
                float verticalScale = combinedRadius / (horizontal * thickness + padding);
                var start = a - center;
                var end = b - center;
                a = b;
                start.y *= verticalScale;
                end.y *= verticalScale;
                float verticalCore = halfCore * verticalScale;
                float low = 0f, high = 1f;
                // Squared distance to a vertical line segment is convex along each water segment.
                for (int j = 0; j < 18; j++)
                {
                    float left = Mathf.Lerp(low, high, 1f / 3f), right = Mathf.Lerp(low, high, 2f / 3f);
                    if (CapsuleDistanceSquared(Vector3.Lerp(start, end, left), verticalCore) <
                        CapsuleDistanceSquared(Vector3.Lerp(start, end, right), verticalCore)) high = right;
                    else low = left;
                }
                if (CapsuleDistanceSquared(Vector3.Lerp(start, end, (low + high) * .5f), verticalCore) <=
                    combinedRadius * combinedRadius) return true;
            }
            return false;
        }

        private bool TouchesCoveredCap(Vector3 center, float feet, float radius)
        {
            if (_coveredMushroomCap == null || !_coveredMushroomCap.enabled) return false;
            var bounds = _coveredMushroomCap.bounds;
            if (center.x < bounds.min.x - radius || center.x > bounds.max.x + radius ||
                center.z < bounds.min.z - radius || center.z > bounds.max.z + radius ||
                feet < bounds.min.y - radius || feet > bounds.max.y + _capPushHeight) return false;

            // Catch a foot landing on either flank, using the actual cap surface rather than its bounding box.
            var origin = new Vector3(center.x, bounds.max.y + radius, center.z);
            float length = bounds.size.y + radius * 2f;
            if (HitsCapTop(origin, length, feet, radius)) return true;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (HitsCapTop(origin + offset, length, feet, radius)) return true;
            }
            return false;
        }

        private bool HitsCapTop(Vector3 origin, float distance, float feet, float radius) =>
            _coveredMushroomCap.Raycast(new Ray(origin, Vector3.down), out var hit, distance) && hit.normal.y > .25f &&
            feet >= hit.point.y - radius && feet <= hit.point.y + _capPushHeight;

        private static float CapsuleDistanceSquared(Vector3 offset, float halfCore)
        {
            offset.y -= Mathf.Clamp(offset.y, -halfCore, halfCore);
            return offset.sqrMagnitude;
        }
    }
}
