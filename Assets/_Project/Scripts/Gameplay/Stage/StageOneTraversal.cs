using System.Collections.Generic;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Herbalist.Levels
{
    // Stage-one authored tree hazards and safe moss areas are independent of leaf targets.
    public sealed class StageOneTraversal : MonoBehaviour
    {
        [System.Serializable]
        private sealed class CheckpointGroup
        {
            public Collider[] surfaces = System.Array.Empty<Collider>();
            public Transform recoveryPoint;
            [System.NonSerialized] public int arrived;
            [System.NonSerialized] public readonly Vector3[] positions = new Vector3[2];
        }

        [SerializeField] private InputActionReference _recoverAction;
        [SerializeField] private CheckpointGroup[] _sharedCheckpoints = System.Array.Empty<CheckpointGroup>();
        [SerializeField] private Collider[] _bareTrees;
        [SerializeField] private BoxCollider[] _mossAreas;
        [SerializeField] private Collider[] _checkpointSurfaces;
        [SerializeField] private Collider[] _cameraPassThrough = System.Array.Empty<Collider>();
        [SerializeField] private Transform[] _starts;
        [SerializeField] private BoxCollider _forestExitCheckpoint;
        [SerializeField] private Transform _forestExitRecoveryPoint;
        [SerializeField] private HangingCloth[] _respawnCloths = System.Array.Empty<HangingCloth>();
        [SerializeField] private StageOneDocumentMechanism[] _respawnBranches = System.Array.Empty<StageOneDocumentMechanism>();
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
        private int _activeCheckpoint = -1;
        private static readonly HashSet<Collider> CameraPassThrough = new();
        public static StageOneTraversal Instance { get; private set; }
        public InputActionReference RecoverAction => _recoverAction;
        public static bool IgnoresCamera(Collider collider) => CameraPassThrough.Contains(collider);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            CameraPassThrough.Clear();
        }
        private void OnEnable()
        {
            if (_recoverAction != null) Instance = this;
            foreach (var collider in _cameraPassThrough)
                if (collider != null) CameraPassThrough.Add(collider);
        }
        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            foreach (var collider in _cameraPassThrough) CameraPassThrough.Remove(collider);
        }
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
        internal bool IsBareTreeContact(Collider surface, Vector3 point) =>
            System.Array.IndexOf(_bareTrees, surface) >= 0 && !IsMoss(point);

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
        public bool TryRecover(StageActor actor)
        {
            if (_recoverAction == null || !Authority || Herbalist.GameUI.GameplayPause.IsPaused ||
                StageOneFlow.Instance.Progress.Cleared || actor == null || !actor.Available || actor.Slot < 0 ||
                actor.Slot >= _starts.Length || _starts[actor.Slot] == null ||
                (_grace.TryGetValue(actor, out float grace) && grace > 0)) return false;

            Recover(actor, false);
            return true;
        }

        private void Recover(StageActor actor, bool fell)
        {
            var flow = StageOneFlow.Instance;
            if (_recoverAction != null && flow.CanCancelMinigame(actor))
                flow.Execute(actor, StageCommand.CancelMinigame);
            HangingCloth.TryRelease(actor, out _);
            if (_recoverAction != null) HangingCloth.ResetApproachAfterFall(actor);
            if (_recoverAction == null)
            {
                foreach (var cloth in _respawnCloths)
                    if (cloth != null) cloth.RestoreAfterRespawn();
                foreach (var branch in _respawnBranches)
                    if (branch != null) branch.RestoreFallenBranch();
            }
            actor.Abilities.Sap?.Cancel();
            var motor = actor.GetComponent<PlayerController>().Motor;
            motor.ResetMotion();
            var net = actor.GetComponent<NetworkPlayer>();
            Vector3 destination = _starts[actor.Slot].position;
            if (_activeCheckpoint >= 0) destination = _sharedCheckpoints[_activeCheckpoint].positions[actor.Slot];
            else if (_recovery.TryGetValue(actor, out var checkpoint)) destination = checkpoint;
            if (net != null && net.Object != null && net.Object.IsValid) net.TeleportAuthoritatively(destination);
            else motor.Teleport(destination);
            _grace[actor] = _recoveryGrace;
            flow.MarkRecovery(actor, fell);
        }

        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            foreach (var actor in StageActor.All)
            {
                if (!actor.Available || actor.Slot < 0 || actor.Slot >= _starts.Length) continue;
                var player = actor.GetComponent<PlayerController>();
                var cc = actor.GetComponent<CharacterController>();
                bool hasCheckpoint = _recovery.TryGetValue(actor, out var checkpoint);
                _grace.TryGetValue(actor, out float grace);
                _grace[actor] = Mathf.Max(0, grace - dt);
                if (grace > 0) continue;
                bool usingCloth = HangingCloth.IsRiding(actor) || HangingCloth.IsGrabbing(actor);
                if (actor.transform.position.y < _fallHeight ||
                    (_recoverAction == null && !usingCloth && TouchesBareTree(cc)))
                {
                    Recover(actor, actor.transform.position.y < _fallHeight);
                    continue;
                }
                StageOneFlow.Instance.TrackLeafPlatformFall(actor, player.Motor.IsGrounded);
                if (_sharedCheckpoints.Length > 0)
                {
                    if (!usingCloth && player.Motor.IsGrounded) UpdateSharedCheckpoint(actor, cc);
                    continue;
                }
                if (_forestExitRecoveryPoint != null && _forestExitCheckpoint != null &&
                    _forestExitCheckpoint.enabled && _forestExitCheckpoint.gameObject.activeInHierarchy &&
                    !_forestExitReached.Contains(actor) && Physics.ComputePenetration(_forestExitCheckpoint,
                        _forestExitCheckpoint.transform.position, _forestExitCheckpoint.transform.rotation,
                        cc, cc.transform.position, cc.transform.rotation, out _, out _))
                {
                    _forestExitReached.Add(actor);
                    Vector3 fork = _forestExitRecoveryPoint.position;
                    if (!hasCheckpoint || fork.y > checkpoint.y) _recovery[actor] = fork;
                    continue;
                }
                if (!player.Motor.IsGrounded || usingCloth) continue;
                Vector3 position = actor.transform.position;
                if (hasCheckpoint && position.y <= checkpoint.y) continue;
                Vector3 feet = cc.transform.TransformPoint(cc.center) - Vector3.up * cc.height * Mathf.Abs(cc.transform.lossyScale.y) * .5f;
                if (!Physics.Raycast(feet + Vector3.up * _checkpointProbeHeight, Vector3.down, out var ground, _checkpointProbeDistance, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (System.Array.IndexOf(_checkpointSurfaces, ground.collider) >= 0 ||
                    (System.Array.IndexOf(_bareTrees, ground.collider) >= 0 && IsMoss(ground.point)))
                    _recovery[actor] = position;
            }
        }

        private void UpdateSharedCheckpoint(StageActor actor, CharacterController character)
        {
            Vector3 feet = character.transform.TransformPoint(character.center) -
                Vector3.up * character.height * Mathf.Abs(character.transform.lossyScale.y) * .5f;
            Physics.Raycast(feet + Vector3.up * _checkpointProbeHeight, Vector3.down,
                out var ground, _checkpointProbeDistance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = _sharedCheckpoints.Length - 1; i > _activeCheckpoint; i--)
            {
                var group = _sharedCheckpoints[i];
                bool reached = false;
                foreach (var surface in group.surfaces)
                {
                    if (surface == null || !surface.enabled || !surface.gameObject.activeInHierarchy) continue;
                    if (surface == ground.collider || (surface.isTrigger && Physics.ComputePenetration(
                        surface, surface.transform.position, surface.transform.rotation,
                        character, character.transform.position, character.transform.rotation, out _, out _)))
                    {
                        reached = true;
                        break;
                    }
                }
                if (!reached) continue;
                int slot = actor.Slot;
                group.positions[slot] = group.recoveryPoint != null
                    ? group.recoveryPoint.position + _starts[slot].position - _starts[0].position
                    : actor.transform.position;
                group.arrived |= 1 << slot;
                if (group.arrived == 0b11) _activeCheckpoint = i;
                return;
            }
        }
        private void Update() => Tick(Time.deltaTime);
    }
}
