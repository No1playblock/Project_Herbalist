using Fusion;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class RotatingAltar : NetworkBehaviour, IStageExitCondition
    {
        [SerializeField] private SapInjectionPort _inlet;
        [SerializeField] private AltarRing[] _rings;
        [SerializeField, Min(.01f)] private float _injectionRequired = 1;
        [SerializeField, Min(.01f)] private float _stairDuration = 3;
        [SerializeField] private GameObject _solvedGlow;
        [SerializeField] private string _startText = "제단 상단에 R로 수액을 주입해 석판을 회전시키세요.";
        [SerializeField] private string _rotateText = "동·서·남·북과 사신 문양을 맞추세요 · R: 나뭇잎으로 정지 · 좌클릭: 회수하여 다시 회전";
        [SerializeField] private string _exitText = "문양이 맞춰졌습니다 · 변형된 계단을 올라 두 명 모두 탈출하세요.";
        [Networked] private int Phase { get; set; }
        [Networked] private float Progress { get; set; }
        private int _phase;
        private float _injection, _progress;
        private bool Online => Object != null && Object.IsValid;
        private bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        public bool CanExit => (Online ? Progress : _progress) >= 1;
        public string Objective => (Online ? Phase : _phase) == 0 ? _startText : (Online ? Phase : _phase) == 1 ? _rotateText : _exitText;
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            _injection += _inlet.Consume();
            if (_phase == 0 && _injection >= _injectionRequired) _phase = 1;
            bool aligned = _rings.Length > 0;
            foreach (var ring in _rings) aligned &= ring.Aligned;
            if (_phase == 1 && aligned) _phase = 2;
            if (_phase == 2) _progress = Mathf.Clamp01(_progress + dt / _stairDuration);
            foreach (var ring in _rings) ring.Tick(_phase == 1, _progress, dt);
            if (Online) { Phase = _phase; Progress = _progress; }
        }
        private void Update() { if (!Online && Authority) Tick(Time.deltaTime); if (_solvedGlow != null) _solvedGlow.SetActive((Online ? Phase : _phase) >= 2); }
    }
}
