using System.Collections.Generic;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    // Stage-one authored tree hazards and safe moss areas are independent of leaf targets.
    public sealed class StageOneTraversal : MonoBehaviour
    {
        [SerializeField] private Collider[] _bareTrees;
        [SerializeField] private BoxCollider[] _mossAreas;
        [SerializeField] private Collider[] _checkpointSurfaces;
        [SerializeField] private Transform[] _starts;
        [SerializeField] private BoxCollider _forestExitCheckpoint;
        [SerializeField] private Transform _forestExitRecoveryPoint;
        [SerializeField] private bool _placeOfflineAtStarts;
        [SerializeField] private float _fallHeight = -15;
        [SerializeField, Min(0)] private float _contactPadding = .08f;
        [SerializeField, Min(0)] private float _recoveryGrace = .5f;
        [SerializeField, Min(0)] private float _checkpointProbeHeight = .3f;
        [SerializeField, Min(.01f)] private float _checkpointProbeDistance = .7f;
        private readonly Dictionary<StageActor, Vector3> _recovery = new();
        private readonly Dictionary<StageActor, float> _grace = new();
        private readonly HashSet<StageActor> _forestExitReached = new();
        private readonly Collider[] _overlaps = new Collider[128];
        public bool Authority => StageOneFlow.Instance != null && StageOneFlow.Instance.Authority;
        private void Start()
        {
            if (!_placeOfflineAtStarts || !Authority || FusionLobbySession.Instance is { HasNetworkSession: true }) return;
            foreach (var actor in StageActor.All)
                if (actor.Available && actor.Slot >= 0 && actor.Slot < _starts.Length && _starts[actor.Slot] != null)
                {
                    var motor = actor.GetComponent<PlayerController>().Motor;
                    motor.Teleport(_starts[actor.Slot].position); motor.ResetMotion();
                }
        }

        public bool IsMoss(Vector3 point)
        {
            foreach (var area in _mossAreas)
            {
                if (area == null || !area.enabled || !area.gameObject.activeInHierarchy) continue;
                Vector3 p = area.transform.InverseTransformPoint(point) - area.center;
                Vector3 half = area.size * .5f;
                if (Mathf.Abs(p.x) <= half.x && Mathf.Abs(p.y) <= half.y && Mathf.Abs(p.z) <= half.z) return true;
            }
            return false;
        }
        public bool TouchesBareTree(CharacterController cc)
        {
            float radius = cc.radius * Mathf.Max(Mathf.Abs(cc.transform.lossyScale.x), Mathf.Abs(cc.transform.lossyScale.z));
            float half = Mathf.Max(0, cc.height * Mathf.Abs(cc.transform.lossyScale.y) * .5f - radius);
            Vector3 center = cc.transform.TransformPoint(cc.center);
            int count = Physics.OverlapCapsuleNonAlloc(center + Vector3.up * half, center - Vector3.up * half,
                radius + _contactPadding + cc.skinWidth * Mathf.Abs(cc.transform.lossyScale.x), _overlaps, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (System.Array.IndexOf(_bareTrees, _overlaps[i]) < 0) continue;
                // Test the actual surface contact, so a safe platform does not exempt the whole capsule.
                Vector3 probe = _overlaps[i].bounds.ClosestPoint(center);
                if (Physics.ComputePenetration(cc, cc.transform.position, cc.transform.rotation,
                    _overlaps[i], _overlaps[i].transform.position, _overlaps[i].transform.rotation, out var direction, out _))
                    probe = center - direction * radius;
                Vector3 feet = new(center.x, cc.bounds.min.y + _contactPadding, center.z);
                if (!IsMoss(probe) && !IsMoss(feet)) return true;
            }
            return false;
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            foreach (var actor in StageActor.All)
            {
                if (!actor.Available || actor.Slot < 0 || actor.Slot >= _starts.Length) continue;
                var player = actor.GetComponent<PlayerController>();
                var cc = actor.GetComponent<CharacterController>();
                if (!_recovery.ContainsKey(actor)) _recovery[actor] = _starts[actor.Slot].position;
                _grace.TryGetValue(actor, out float grace);
                _grace[actor] = Mathf.Max(0, grace - dt);
                if (grace > 0) continue;
                bool ridingCloth = HangingCloth.IsRiding(actor);
                if (actor.transform.position.y < _fallHeight || (!ridingCloth && TouchesBareTree(cc)))
                {
                    HangingCloth.TryRelease(actor, out _);
                    actor.Abilities.Sap?.Cancel();
                    var net = actor.GetComponent<NetworkPlayer>();
                    if (net != null && net.Object != null && net.Object.IsValid) net.TeleportAuthoritatively(_recovery[actor]);
                    else { player.Motor.Teleport(_recovery[actor]); player.Motor.ResetMotion(); }
                    _grace[actor] = _recoveryGrace;
                    continue;
                }
                if (_forestExitRecoveryPoint != null && _forestExitCheckpoint != null &&
                    _forestExitCheckpoint.enabled && _forestExitCheckpoint.gameObject.activeInHierarchy &&
                    !_forestExitReached.Contains(actor) && Physics.ComputePenetration(_forestExitCheckpoint,
                        _forestExitCheckpoint.transform.position, _forestExitCheckpoint.transform.rotation,
                        cc, cc.transform.position, cc.transform.rotation, out _, out _))
                {
                    _forestExitReached.Add(actor);
                    _recovery[actor] = _forestExitRecoveryPoint.position;
                    continue;
                }
                if (!player.Motor.IsGrounded || ridingCloth) continue;
                Vector3 feet = cc.transform.TransformPoint(cc.center) - Vector3.up * cc.height * Mathf.Abs(cc.transform.lossyScale.y) * .5f;
                if (!Physics.Raycast(feet + Vector3.up * _checkpointProbeHeight, Vector3.down, out var ground, _checkpointProbeDistance, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (System.Array.IndexOf(_checkpointSurfaces, ground.collider) >= 0 ||
                    (System.Array.IndexOf(_bareTrees, ground.collider) >= 0 && IsMoss(ground.point)))
                    _recovery[actor] = actor.transform.position;
            }
        }
        private void Update() => Tick(Time.deltaTime);
    }
}
