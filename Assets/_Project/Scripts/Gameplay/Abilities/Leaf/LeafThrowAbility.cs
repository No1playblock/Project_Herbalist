using System;
using System.Collections.Generic;
using UnityEngine;
namespace Herbalist.Abilities
{
    public sealed class LeafThrowAbility : MonoBehaviour
    {
        [SerializeField] private LeafAbilitySettings settings;
        [SerializeField] private Transform launchFrame;
        private readonly List<LeafProjectile> leaves = new();
        private LeafProjectile[] displaySlots;
        private Func<LeafProjectile> spawn;
        private Action<LeafProjectile> despawn;
        private bool network;
        private LeafProjectile pendingRecovery;
        private bool pendingThrow;
        private Ray pendingAim;
        private float cooldown;
        private bool _preparingThrow;
        private float _preparationRemaining;
        private Ray _preparedAim;
        private LeafInstallTarget _preparedSocket;
        private LeafInstallTarget _pendingSocket;
        private bool _socketThrow, _pendingSocketThrow;
        public uint ThrowSequence { get; private set; }
        public float ThrowElapsed { get; private set; }
        public bool PreparingThrow => _preparingThrow;
        public void ApplyThrowPresentation(uint sequence, float elapsed) { ThrowSequence = sequence; ThrowElapsed = elapsed; }
        public event Action<float> FacingRequested;
        public LeafAbilitySettings Settings => settings;
        public int ActiveCount => leaves.Count;
        // Two bits per authored HUD slot: 0 held, 1 flying/installed, 2 returning.
        public int PackedSlotStates
        {
            get
            {
                if (displaySlots == null) return 0;
                int packed = 0;
                for (int i = 0; i < Mathf.Min(3, displaySlots.Length); i++)
                {
                    var leaf = displaySlots[i];
                    int state = leaf == null || leaf.State == LeafState.Complete ? 0 :
                        leaf.State == LeafState.Returning ? 2 : 1;
                    packed |= state << (i * 2);
                }
                return packed;
            }
        }
        public bool Recovering { get; private set; }
        public uint FeedbackSequence { get; private set; }
        public uint CatchSequence { get; private set; }
        public void ApplyCatchSequence(uint sequence) => CatchSequence = sequence;
        private void OnCatchApproaching() => CatchSequence++;
        public bool RangeRejected { get; private set; }
        private void Awake()
        {
            var rules = Herbalist.Levels.StageLevel.Instance != null ? Herbalist.Levels.StageLevel.Instance.abilityRules : null;
            if (rules != null && rules.leaf != null) settings = rules.leaf;
            displaySlots = new LeafProjectile[Mathf.Max(1, settings != null ? settings.capacity : 3)];
        }
        public void ApplyFeedback(uint sequence, bool rangeRejected) { FeedbackSequence = sequence; RangeRejected = rangeRejected; }
        private bool Reject(bool range) { RangeRejected = range; FeedbackSequence++; return false; }
        public bool TryThrowAtTarget(Ray aim)
        {
            return TryThrow(aim);
        }
        public void Configure(Func<LeafProjectile> create, Action<LeafProjectile> remove, bool online)
        { spawn = create; despawn = remove; network = online; }
        public void Tick(float dt, Ray aim)
        {
            if (Herbalist.GameUI.GameplayPause.IsPaused) return;
            cooldown = Mathf.Max(0, cooldown - dt);
            ThrowElapsed += dt;
            if (pendingThrow) pendingAim = aim;
            if (_preparingThrow)
            {
                _preparedAim = aim;
                _preparationRemaining -= dt;
                if (_preparationRemaining <= 0f) { _preparingThrow = false; Launch(_preparedAim); }
            }
        }
        public bool TryThrow(Ray aim)
            => TryThrow(aim, null);
        public bool TryThrowAtSocket(LeafInstallTarget socket)
        {
            if (socket == null || !socket.HasSocket || !socket.CanInstall ||
                Vector3.Distance(transform.position, socket.SocketPosition) > settings.range) return false;
            var direction = socket.SocketPosition - transform.position;
            return TryThrow(new Ray(transform.position, direction.normalized), socket);
        }
        private bool TryThrow(Ray aim, LeafInstallTarget socket)
        {
            if (Recovering || cooldown > 0 || _preparingThrow) return false;
            if (ActiveCount >= settings.capacity)
            {
                LeafProjectile oldest = leaves[0];
                if (oldest.State == LeafState.Complete) return false;
                Recovering = true;
                pendingRecovery = oldest;
                pendingThrow = true;
                pendingAim = aim;
                _pendingSocket = socket;
                _pendingSocketThrow = socket != null;
                if (oldest.State != LeafState.Returning) oldest.BeginReturn();
                return true;
            }
            PrepareThrow(aim, socket, socket != null);
            return true;
        }
        private void PrepareThrow(Ray aim, LeafInstallTarget socket = null, bool socketThrow = false)
        {
            _preparedSocket = socket;
            _socketThrow = socketThrow;
            ThrowSequence++;
            ThrowElapsed = 0f;
            _preparedAim = aim;
            _preparationRemaining = settings.throwPreparationSeconds;
            _preparingThrow = _preparationRemaining > 0f;
            FaceAim(aim);
            if (!_preparingThrow) Launch(aim);
        }
        private void FaceAim(Ray aim)
        {
            if (!settings.faceAimOnThrow) return;
            var player = GetComponent<Herbalist.Player.PlayerController>();
            var planar = Vector3.ProjectOnPlane(aim.direction, Vector3.up);
            if (player == null || planar.sqrMagnitude <= 0.0001f) return;
            float yaw = Quaternion.LookRotation(planar).eulerAngles.y;
            player.View.SetBodyYaw(yaw);
            FacingRequested?.Invoke(yaw);
        }
        private void Launch(Ray aim)
        {
            var socket = _preparedSocket;
            bool installAtSocket = _socketThrow;
            _preparedSocket = null;
            _socketThrow = false;
            if (installAtSocket)
            {
                if (socket == null || !socket.CanInstall ||
                    Vector3.Distance(transform.position, socket.SocketPosition) > settings.range)
                { Reject(false); return; }
                aim = new Ray(transform.position, (socket.SocketPosition - transform.position).normalized);
            }
            FaceAim(aim);
            Transform frame = launchFrame != null ? launchFrame : transform;
            Vector3 origin = frame.TransformPoint(settings.launchOffset);
            float maxDistance = settings.range;
            float aimRayDistance = maxDistance + Vector3.Distance(aim.origin, origin);
            Vector3 aimPoint = aim.GetPoint(aimRayDistance);
            if (Physics.Raycast(aim, out var hit, Mathf.Infinity, settings.hitMask, QueryTriggerInteraction.Ignore))
                aimPoint = hit.point + aim.direction * settings.collisionRadius;
            Vector3 towardAim = aimPoint - origin;
            if (towardAim.sqrMagnitude < 0.000001f) towardAim = aim.direction;
            Vector3 endpoint = origin + Vector3.ClampMagnitude(towardAim, maxDistance);
            var projectile = spawn();
            projectile.CatchApproaching += OnCatchApproaching;
            leaves.Add(projectile);
            for (int i = 0; i < displaySlots.Length; i++)
                if (displaySlots[i] == null) { displaySlots[i] = projectile; break; }
            projectile.Initialize(settings, transform, origin, endpoint, OnReturned, network, frame);
            if (installAtSocket && !projectile.TryInstallAtSocket(socket)) projectile.BeginReturn();
            RangeRejected = false;
            cooldown = settings.cooldown;
        }
        private void OnReturned(LeafProjectile leaf)
        {
            leaf.CatchApproaching -= OnCatchApproaching;
            bool launchPending = pendingRecovery == leaf && pendingThrow;
            var pendingSocket = _pendingSocket;
            bool pendingSocketThrow = _pendingSocketThrow;
            leaves.Remove(leaf);
            for (int i = 0; i < displaySlots.Length; i++)
                if (displaySlots[i] == leaf) { displaySlots[i] = null; break; }
            if (pendingRecovery == leaf) { Recovering = false; pendingRecovery = null; pendingThrow = false; _pendingSocket = null; _pendingSocketThrow = false; }
            despawn(leaf);
            if (launchPending && isActiveAndEnabled && spawn != null) PrepareThrow(pendingAim, pendingSocket, pendingSocketThrow);
        }
        public void Clear()
        {
            _preparingThrow = false;
            _preparationRemaining = 0f;
            _preparedSocket = _pendingSocket = null;
            _socketThrow = _pendingSocketThrow = false;
            pendingThrow = false; pendingRecovery = null; Recovering = false;
            foreach (var leaf in leaves.ToArray()) if (leaf != null) leaf.Finish();
            leaves.Clear();
        }
    }
}
