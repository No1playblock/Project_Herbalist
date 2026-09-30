using Fusion;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.Networking;
using Herbalist.Presentation;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class DescendingMazeFlow : NetworkBehaviour
    {
        [SerializeField] private SapMazeBoard[] _mazes;
        [SerializeField] private Transform[] _rafts;
        [SerializeField] private Transform[] _stations;
        [SerializeField] private StageLevel _level;
        [SerializeField] private PlayScreenController _screen;
        [SerializeField] private Transform[] _introCameras;
        [SerializeField, Min(.1f)] private float _introDuration = 2;
        [SerializeField, Min(.1f)] private float _descentDuration = 3;
        [SerializeField, Min(.1f)] private float _fallMargin = 3;
        [SerializeField] private Vector3 _riderOffset = new(0, .05f, 0);
        [SerializeField] private int _frontLayer;
        [SerializeField] private int _backLayer;
        [SerializeField] private bool _allowSoloTesting = true;
        [SerializeField] private string _waitingText = "두 플레이어의 진입을 기다리는 중";
        [SerializeField] private string _introText = "무당나무 안으로 떨어집니다 · 각자의 뗏목에 착지";
        [SerializeField] private string _descentText = "미로 완료 · 같은 뗏목으로 아래 구간에 이동 중";
        [SerializeField] private string _mazeText = "앞: R로 수액 조종 · WASD 이동 / 뒤: 새는 구멍에 R로 나뭇잎 설치";
        [Networked] private int Current { get; set; }
        [Networked] private int Phase { get; set; }
        [Networked] private float Travel { get; set; }
        private int _current = -1, _phase;
        private float _travel;
        private bool _transition;
        private readonly StageActor[] _actors = new StageActor[2];
        private readonly Vector3[] _basePositions = new Vector3[2];
        private readonly System.Collections.Generic.Dictionary<Camera, int> _masks = new();
        private bool Online => Object != null && Object.IsValid;
        private bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        public int CurrentMaze => Online ? Current : _current;
        public bool Finished => CurrentMaze >= _mazes.Length;
        public string Objective => (Online ? Phase : _phase) == 0 ? _waitingText : (Online ? Phase : _phase) == 1 ? _introText : (Online ? Phase : _phase) == 2 ? _descentText : _mazeText;
        private bool Solo
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return _allowSoloTesting && !Online && (FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession);
#else
                return false;
#endif
            }
        }
        private void Awake() { for (int i = 0; i < 2; i++) _basePositions[i] = _rafts[i].position; }
        private void OnEnable() => _screen.SetOverride(this, PlayScreenMode.Personal);
        public override void Spawned()
        {
            // Fusion may disable scene objects while attaching them on a joining peer.
            _screen.SetOverride(this, PlayScreenMode.Personal);
            if (HasStateAuthority) { Current = -1; Phase = 0; }
        }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            _actors[0] = _actors[1] = null;
            foreach (var actor in StageActor.All)
                if (actor.Available && actor.Abilities.Unlocked) _actors[actor.Abilities.Kind == PlayerAbilityKind.Sap ? 0 : 1] = actor;
            bool ready = Solo ? _actors[0] != null || _actors[1] != null : _actors[0] != null && _actors[1] != null && _actors[0].Slot != _actors[1].Slot;
            if (_phase == 0 && ready) { _phase = 1; _travel = 0; }
            if (_phase == 1)
            {
                _travel = Mathf.Clamp01(_travel + dt / _introDuration);
                for (int i = 0; i < 2; i++) Carry(_actors[i], _rafts[i].position + _riderOffset + Vector3.up * Mathf.Lerp(_fallMargin, 0, _travel));
                if (_travel >= 1) { _phase = 2; _travel = 0; }
            }
            else if (_phase == 2)
            {
                _travel = Mathf.Clamp01(_travel + dt / _descentDuration); Present(_current, _travel, true);
                for (int i = 0; i < 2; i++) Carry(_actors[i], _rafts[i].position + _riderOffset);
                if (_travel >= 1)
                {
                    _current++; _phase = 3; _travel = 0;
                    foreach (var actor in _actors) Release(actor);
                }
            }
            else if (_phase == 3 && _current >= 0 && _current < _mazes.Length)
            {
                for (int i = 0; i < _mazes.Length; i++) { _mazes[i].SetPlayable(i == _current); _mazes[i].Tick(dt); }
                for (int i = 0; i < 2; i++)
                {
                    var actor = _actors[i]; if (actor == null) continue;
                    bool wrongSide = Solo && Mathf.Sign(actor.transform.position.z) != Mathf.Sign(_rafts[i].position.z);
                    if (actor.transform.position.y < _rafts[i].position.y - _fallMargin || wrongSide) Teleport(actor, _rafts[i].position + _riderOffset);
                }
                if (_mazes[_current].State.Phase == MazePhase.Solved)
                {
                    _mazes[_current].SetPlayable(false);
                    if (_current == _mazes.Length - 1) _current = _mazes.Length;
                    else { _phase = 2; _travel = 0; }
                }
            }
            if (Finished && !_transition) _transition = StageTransition.Advance(_level);
            if (Online) { Current = _current; Phase = _phase; Travel = _travel; }
        }
        private void Present(int current, float travel, bool moving)
        {
            int from = Mathf.Clamp(current + 1, 0, _stations.Length - 1);
            int to = Mathf.Clamp(current + 2, 0, _stations.Length - 1);
            float y = moving ? Mathf.Lerp(_stations[from].position.y, _stations[to].position.y, Mathf.SmoothStep(0, 1, travel)) : _stations[from].position.y;
            for (int i = 0; i < 2; i++) _rafts[i].position = new Vector3(_basePositions[i].x, y, _basePositions[i].z);
        }
        private void Carry(StageActor actor, Vector3 point)
        {
            if (actor == null) return;
            actor.Abilities.Sap?.Cancel(); actor.Abilities.SetInputLock(this, true);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid) net.SetTransportAuthoritatively(true, point);
            else actor.GetComponent<PlayerController>().SetTransport(this, point);
        }
        private void Release(StageActor actor)
        {
            if (actor == null) return;
            actor.Abilities.SetInputLock(this, false); actor.GetComponent<PlayerController>().ClearTransport(this);
            var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority) net.SetTransportAuthoritatively(false, actor.transform.position);
        }
        private static void Teleport(StageActor actor, Vector3 point)
        {
            actor.Abilities.Sap?.Cancel(); var net = actor.GetComponent<NetworkPlayer>();
            if (net != null && net.Object != null && net.Object.IsValid) net.TeleportAuthoritatively(point);
            else actor.GetComponent<PlayerController>().Motor.Teleport(point);
        }
        private void Update()
        {
            if (!Online && Authority) Tick(Time.deltaTime);
            int phase = Online ? Phase : _phase;
            Present(CurrentMaze, Online ? Travel : _travel, phase == 2);
            foreach (var actor in StageActor.All)
            {
                if (!actor.Local) continue;
                int side = actor.Abilities.Kind == PlayerAbilityKind.Sap ? 0 : 1;
                var camera = actor.Camera;
                if (!_masks.ContainsKey(camera)) _masks[camera] = camera.cullingMask;
                camera.cullingMask = _masks[camera] & ~(1 << (side == 0 ? _backLayer : _frontLayer));
                var view = actor.GetComponent<PlayerController>().View;
                if (phase == 1) view.SetCameraOverride(this, _introCameras[side]); else view.ClearCameraOverride(this);
            }
        }
        private void OnDisable()
        {
            if (_screen != null) _screen.SetOverride(this, null);
            foreach (var pair in _masks) if (pair.Key != null) pair.Key.cullingMask = pair.Value;
            foreach (var actor in _actors) { Release(actor); if (actor != null) actor.GetComponent<PlayerController>().View.ClearCameraOverride(this); }
        }
    }
}
