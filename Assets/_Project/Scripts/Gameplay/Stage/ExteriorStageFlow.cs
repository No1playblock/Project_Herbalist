using Fusion;
using UnityEngine;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.Abilities;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class ExteriorStageFlow : NetworkBehaviour, IStageExitCondition
    {
        [SerializeField] private StageArrivalZone _top;
        [SerializeField] private SapInjectionPort _inlet;
        [SerializeField] private LandingCheckpoint[] _checkpoints;
        [SerializeField] private Transform[] _starts;
        [SerializeField] private Transform _fallThreshold;
        [SerializeField, Min(.1f)] private float _fallMargin = 5;
        [SerializeField, Min(.01f)] private float _injectionRequired = 3;
        [SerializeField] private GameObject _treeGlow;
        [SerializeField] private string _climbText = "일반 발판·점프대·천을 이용해 무당나무 상단으로 올라가세요.";
        [SerializeField] private string _injectText = "두 명이 상단에 도착했습니다 · 중앙 수액 통로에 R로 수액을 주입하세요.";
        [Networked] private uint Arrived { get; set; }
        [Networked] private float Injection { get; set; }
        [Networked] private NetworkBool Open { get; set; }
        private uint _arrived;
        private float _injection;
        private bool _open;
        private readonly Vector3[] _recovery = new Vector3[2];
        private bool Online => Object != null && Object.IsValid;
        private bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        public bool CanExit => Online ? Open : _open;
        public string Objective => (Online ? Arrived : _arrived) == 3 ? _injectText : _climbText;
        private void Awake() { for (int i = 0; i < 2; i++) _recovery[i] = _starts[i].position; }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            foreach (var actor in StageActor.All)
            {
                if (!actor.Available || actor.Slot < 0 || actor.Slot > 1) continue;
                var player = actor.GetComponent<PlayerController>();
                foreach (var checkpoint in _checkpoints) if (checkpoint.Supports(player)) _recovery[actor.Slot] = checkpoint.Point(actor.Slot);
                if (_top.Contains(actor.transform.position + Vector3.up)) _arrived |= 1u << actor.Slot;
                if (actor.transform.position.y >= Mathf.Max(_fallThreshold.position.y, _recovery[actor.Slot].y - _fallMargin)) continue;
                actor.Abilities.Sap?.Cancel();
                var net = actor.GetComponent<NetworkPlayer>();
                if (net != null && net.Object != null && net.Object.IsValid) net.TeleportAuthoritatively(_recovery[actor.Slot]);
                else player.Motor.Teleport(_recovery[actor.Slot]);
            }
            float supplied = _inlet.Consume();
            var gathering = StageOneFlow.Instance;
            if (_arrived == 3 && gathering != null && gathering.Progress.GateOpen) _injection += supplied;
            if (_injection >= _injectionRequired) _open = true;
            if (Online) { Arrived = _arrived; Injection = _injection; Open = _open; }
        }
        private void Update() { if (!Online && Authority) Tick(Time.deltaTime); if (_treeGlow != null) _treeGlow.SetActive(CanExit); }
    }
}
