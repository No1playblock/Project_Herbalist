using Fusion;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    public enum StageOneDocumentMechanismKind { GrowingMushrooms, EjectObstacle, LowerBranch, BlockJet }

    // Scene-specific bindings. Original art stays at its authored rest pose in edit mode.
    [RequireComponent(typeof(NetworkObject))]
    public sealed class StageOneDocumentMechanism : NetworkBehaviour
    {
        [SerializeField] private StageOneDocumentMechanismKind _kind;
        [SerializeField] private SapInjectionPort _port;
        [SerializeField] private LeafInstallTarget _leafTarget;
        [SerializeField] private HangingCloth _requiredCloth;
        [SerializeField] private BoxCollider _activationArea;
        [SerializeField] private Transform[] _movingArt = System.Array.Empty<Transform>();
        [SerializeField] private Transform _motionPivot;
        [SerializeField] private Vector3 _offset = new(0, -12, 0);
        [SerializeField] private Vector3 _rotation = Vector3.zero;
        [SerializeField, Min(.01f)] private float _fillSeconds = 1f;
        [SerializeField, Min(.01f)] private float _restoreSeconds = 3f;
        [SerializeField, Min(.01f)] private float _moveSeconds = 1f;
        [SerializeField, Min(1)] private float _grownScale = 2.5f;
        [SerializeField, Min(0)] private float _grownLocalScale;
        [SerializeField] private Vector3 _growthWorldOffset;
        [SerializeField] private GameObject _jet;
        [SerializeField, Min(0)] private float _ejectionJetHoldSeconds = 1.2f;
        [SerializeField] private Collider[] _obstacleColliders = System.Array.Empty<Collider>();
        [Networked] private float Amount { get; set; }
        [Networked] private NetworkBool Completed { get; set; }
        private float _amount;
        private bool _completed;
        private Vector3[] _positions, _scales;
        private Quaternion[] _rotations;
        private Vector3 _pivot;
        private float _lastPresentedAmount, _ejectionJetRemaining;
        private StageOneFixedWaterStream _fixedStream;
        private bool Online => Object != null && Object.IsValid;
        public bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        public bool Solved => Online ? Completed : _completed;
        public float Progress => Online ? Amount : _amount;
        public StageOneDocumentMechanismKind Kind => _kind;
        public bool Ready => CanActivate();
        public bool TryHitBranch()
        {
            if (_kind != StageOneDocumentMechanismKind.LowerBranch || !Authority ||
                Herbalist.GameUI.GameplayPause.IsPaused || !CanActivate()) return false;
            _completed = true;
            if (Online) Completed = true;
            return true;
        }
        private void Awake()
        {
            if (_kind == StageOneDocumentMechanismKind.BlockJet && _jet != null)
                _fixedStream = _jet.GetComponent<StageOneFixedWaterStream>();
            _positions = new Vector3[_movingArt.Length]; _scales = new Vector3[_movingArt.Length]; _rotations = new Quaternion[_movingArt.Length];
            _pivot = _motionPivot != null ? _motionPivot.position : transform.position;
            for (int i = 0; i < _movingArt.Length; i++)
            { _positions[i] = _movingArt[i].position; _scales[i] = _movingArt[i].localScale; _rotations[i] = _movingArt[i].rotation; }
        }
        public override void Spawned() { if (HasStateAuthority) { Amount = 0; Completed = false; } }
        private bool CanActivate()
        {
            if (_requiredCloth != null && !_requiredCloth.WasUsed) return false;
            if (_activationArea == null) return true;
            foreach (var actor in StageActor.All)
                if (actor.Available &&
                    _activationArea.bounds.Contains(actor.GetComponent<CharacterController>().bounds.center)) return true;
            return false;
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused || dt <= 0) return;
            float injected = _port != null ? _port.Consume() : 0;
            switch (_kind)
            {
                case StageOneDocumentMechanismKind.GrowingMushrooms:
                    _amount = Mathf.MoveTowards(_amount, injected > 0 ? 1 : 0,
                        injected > 0 ? injected / _fillSeconds : dt / _restoreSeconds);
                    break;
                case StageOneDocumentMechanismKind.EjectObstacle:
                    if (!_completed)
                    {
                        if (CanActivate()) _amount = Mathf.Clamp01(_amount + injected / _fillSeconds);
                        if (_amount >= 1) _completed = true;
                    }
                    break;
                case StageOneDocumentMechanismKind.LowerBranch:
                    if (_completed) _amount = Mathf.MoveTowards(_amount, 1, dt / _moveSeconds);
                    break;
                case StageOneDocumentMechanismKind.BlockJet:
                    _completed = _leafTarget != null && _leafTarget.HasLeaf;
                    _amount = _completed ? 1 : 0;
                    break;
            }
            if (Online) { Amount = _amount; Completed = _completed; }
            Present(_amount, _completed, dt);
            if (!_completed && _fixedStream != null) _fixedStream.PushOnContact();
        }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        private void Update()
        {
            if (!Online) Tick(Time.deltaTime);
            else if (!HasStateAuthority) Present(Amount, Completed, Time.deltaTime);
        }
        private void Present(float amount, bool complete, float dt)
        {
            for (int i = 0; i < _movingArt.Length; i++)
            {
                var art = _movingArt[i];
                if (_kind == StageOneDocumentMechanismKind.GrowingMushrooms)
                {
                    var grown = _grownLocalScale > 0 ? Vector3.one * _grownLocalScale : _scales[i] * _grownScale;
                    art.localScale = Vector3.Lerp(_scales[i], grown, amount);
                    art.position = _positions[i] + _growthWorldOffset * amount;
                }
                else
                {
                    var rotation = Quaternion.Euler(_rotation * amount);
                    art.SetPositionAndRotation(_pivot + rotation * (_positions[i] - _pivot) + _offset * amount, rotation * _rotations[i]);
                }
            }
            if (_kind == StageOneDocumentMechanismKind.EjectObstacle)
            {
                if (amount > _lastPresentedAmount) _ejectionJetRemaining = _ejectionJetHoldSeconds;
                else if (!Herbalist.GameUI.GameplayPause.IsPaused) _ejectionJetRemaining = Mathf.Max(0, _ejectionJetRemaining - dt);
                _lastPresentedAmount = amount;
                bool visible = _ejectionJetRemaining > 0;
                if (_jet != null && _jet.activeSelf != visible) _jet.SetActive(visible);
            }
            else if (_jet != null && _jet.activeSelf == complete) _jet.SetActive(!complete);
            foreach (var c in _obstacleColliders) if (c != null) c.enabled = !complete;
        }
    }
}
