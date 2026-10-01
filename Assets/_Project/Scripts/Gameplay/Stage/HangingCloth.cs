using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class HangingCloth : NetworkBehaviour, IPlayerMovementReceiver
    {
        [SerializeField] private Transform _knot;
        [SerializeField] private Transform _cloth;
        [SerializeField] private Vector3[] _gripOffsets = { new(-.45f, -1, 0), new(.45f, -1, 0) };
        [SerializeField, Min(.1f)] private float _length = 7;
        [SerializeField, Range(0, 60)] private float _maxAngle = 30;
        [SerializeField, Min(.1f)] private float _grabRange = 2.5f;
        [SerializeField, Min(0)] private float _driveAcceleration = 65;
        [SerializeField, Min(0)] private float _spring = 4;
        [SerializeField, Min(0)] private float _damping = 1.2f;
        [SerializeField, Min(0)] private float _releaseSpeed = 1;
        [SerializeField] private LayerMask _obstructionMask = 1;
        [SerializeField] private string _grabText = "천의 매듭을 잡았습니다 · 먼저 잡은 플레이어가 WASD로 흔들기 · E: 놓기";
        [SerializeField] private string _releaseText = "천을 놓았습니다.";
        [SerializeField] private string _fullText = "매듭에 두 명이 매달려 있습니다.";
        [Networked] private Vector2 Angles { get; set; }
        [Networked] private int Rider0 { get; set; }
        [Networked] private int Rider1 { get; set; }
        private Vector2 _angles, _velocity, _drive;
        private readonly StageActor[] _riders = new StageActor[2];
        private readonly Vector3[] _lastPoints = new Vector3[2];
        private static readonly HashSet<HangingCloth> Cloths = new();
        private bool Online => Object != null && Object.IsValid;
        public bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        private void OnEnable() => Cloths.Add(this);
        public override void Spawned() { if (HasStateAuthority) { Rider0 = Rider1 = -1; Angles = Vector2.zero; } }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        public static bool TryInteract(StageActor actor, out string feedback)
        {
            feedback = null;
            HangingCloth nearest = null; float distance = float.PositiveInfinity;
            foreach (var cloth in Cloths)
            {
                if (!cloth.Authority) continue;
                for (int i = 0; i < 2; i++) if (cloth._riders[i] == actor) { cloth.Detach(i); feedback = cloth._releaseText; return true; }
                float d = Vector3.Distance(actor.transform.position + Vector3.up, cloth._knot.position);
                if (d <= cloth._grabRange && d < distance) { nearest = cloth; distance = d; }
            }
            if (nearest == null) return false;
            var player = actor.GetComponent<PlayerController>();
            if (player.Motor.MovementLocked) return false;
            for (int i = 0; i < 2; i++)
                if (nearest._riders[i] == null)
                {
                    nearest._riders[i] = actor;
                    nearest._lastPoints[i] = actor.transform.position;
                    player.SetMovementReceiver(nearest); player.Motor.SetMovementLock(nearest, true);
                    actor.Abilities.Sap?.Cancel(); actor.Abilities.SetInputLock(nearest, true);
                    nearest.Publish(); feedback = nearest._grabText; return true;
                }
            feedback = nearest._fullText; return true;
        }
        public void ReceiveMovement(PlayerController player, Vector2 move)
        {
            if (!Authority || _riders[0] == null || _riders[0].GetComponent<PlayerController>() != player) return;
            Vector3 world = Quaternion.Euler(0, player.View.Yaw, 0) * new Vector3(move.x, 0, move.y);
            // Positive X rotation moves a hanging point toward -Z; positive Z toward +X.
            _drive = Vector2.ClampMagnitude(new Vector2(-world.z, world.x), 1);
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused || dt <= 0) return;
            for (int i = 0; i < 2; i++) if (_riders[i] != null && !_riders[i].Available) Detach(i);
            Vector2 velocity = _velocity + (_drive * _driveAcceleration - _angles * _spring - _velocity * _damping) * dt;
            Vector2 next = Vector2.ClampMagnitude(_angles + velocity * dt, _maxAngle);
            _drive = Vector2.zero;
            bool blocked = false;
            for (int i = 0; i < 2; i++)
            {
                if (_riders[i] == null) continue;
                Vector3 from = Position(_angles, i), to = Position(next, i);
                var cc = _riders[i].GetComponent<CharacterController>();
                Vector3 delta = to - from;
                Vector3 probe = from + cc.center;
                if (delta.sqrMagnitude > .000001f)
                    foreach (var hit in Physics.SphereCastAll(probe, cc.radius, delta.normalized, delta.magnitude, _obstructionMask, QueryTriggerInteraction.Ignore))
                        if (hit.collider.GetComponentInParent<StageActor>() == null && !hit.collider.transform.IsChildOf(transform)) { blocked = true; break; }
            }
            if (blocked) _velocity = Vector2.zero;
            else { _angles = next; _velocity = velocity; }
            Present(_angles);
            for (int i = 0; i < 2; i++)
            {
                var actor = _riders[i]; if (actor == null) continue;
                Vector3 point = Position(_angles, i); _lastPoints[i] = point;
                var net = actor.GetComponent<NetworkPlayer>();
                if (net != null && net.Object != null && net.Object.IsValid) net.SetTransportAuthoritatively(true, point);
                else actor.GetComponent<PlayerController>().SetTransport(this, point);
            }
            Publish();
        }
        private Vector3 Position(Vector2 angles, int side) => transform.position + Quaternion.Euler(angles.x, 0, angles.y) * (Vector3.down * _length + _gripOffsets[side]);
        private void Present(Vector2 angles)
        {
            Quaternion rotation = Quaternion.Euler(angles.x, 0, angles.y);
            _knot.position = transform.position + rotation * Vector3.down * _length;
            _cloth.rotation = rotation; _cloth.position = Vector3.Lerp(transform.position, _knot.position, .5f);
        }
        private void Publish() { if (Online) { Angles = _angles; Rider0 = _riders[0] != null ? _riders[0].Slot : -1; Rider1 = _riders[1] != null ? _riders[1].Slot : -1; } }
        private void Detach(int side)
        {
            var actor = _riders[side]; if (actor == null) return;
            var player = actor.GetComponent<PlayerController>();
            var velocity = (Position(_angles + _velocity * .01f, side) - Position(_angles, side)) / .01f * _releaseSpeed;
            player.ClearMovementReceiver(this); player.ClearTransport(this); player.Motor.SetMovementLock(this, false); actor.Abilities.SetInputLock(this, false);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority) { net.SetTransportAuthoritatively(false, actor.transform.position); net.LaunchAuthoritatively(velocity, player.Tuning.gravity, true); }
            else if (!Online) player.Motor.Launch(velocity, player.Tuning.gravity, true);
            _riders[side] = null;
            if (side == 0 && _riders[1] != null) { _riders[0] = _riders[1]; _riders[1] = null; }
            Publish();
        }
        private void Update() { if (!Online && Authority) Tick(Time.deltaTime); else if (Online && !HasStateAuthority) Present(Angles); }
        private void OnDisable() { Cloths.Remove(this); if (Authority) { Detach(1); Detach(0); } }
    }
}
