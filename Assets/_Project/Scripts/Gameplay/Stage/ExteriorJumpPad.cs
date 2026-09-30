using Fusion;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class ExteriorJumpPad : NetworkBehaviour
    {
        [SerializeField] private SapJumpPadSettings _settings;
        [SerializeField] private SapInjectionPort[] _ports;
        [SerializeField] private LeafInstallTarget[] _sockets;
        [SerializeField] private Transform[] _leafCenters;
        [SerializeField] private GameObject[] _jets;
        [Networked] private float Charge0 { get; set; }
        [Networked] private float Charge1 { get; set; }
        [Networked] private float Flight { get; set; }
        [Networked] private int Output { get; set; }
        [Networked] private uint Riders { get; set; }
        private readonly float[] _charge = new float[2];
        private readonly float[] _grace = new float[2];
        private readonly Vector3[] _rest = new Vector3[2];
        private readonly Vector3[] _jetBottoms = new Vector3[2];
        private readonly Vector3[] _jetScales = new Vector3[2];
        private readonly float[] _jetHeights = new float[2];
        private float _flight, _cooldown;
        private int _output;
        private uint _riders;
        private readonly StageActor[] _carriedActors = new StageActor[2];
        private readonly Vector3[] _riderOffsets = new Vector3[2];
        private bool Online => Object != null && Object.IsValid;
        public bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        public float ChargeFraction => Mathf.Max(Online ? Charge0 : _charge[0], Online ? Charge1 : _charge[1]) / _settings.chargeUnits;
        private void Awake()
        {
            for (int i = 0; i < 2; i++)
            {
                _rest[i] = _leafCenters[i].localPosition;
                _jetScales[i] = _jets[i].transform.localScale;
                _jetHeights[i] = _jets[i].GetComponent<Renderer>().bounds.size.y;
                _jetBottoms[i] = _jets[i].transform.parent.InverseTransformPoint(_jets[i].transform.position - Vector3.up * _jetHeights[i] * .5f);
            }
        }
        private void OnDisable() { if (Authority) ReleaseRiders(false); }
        public override void FixedUpdateNetwork() { if (HasStateAuthority) Tick(Runner.DeltaTime); }
        private void Update()
        {
            if (!Online && Authority) Tick(Time.deltaTime);
            Present(Online ? Flight : _flight, Online ? Output : _output);
        }
        private float AscentDuration => Mathf.Min(_settings.eruptionRiseDuration, _settings.flightDuration * .5f);
        private Vector3 LaunchPoint(int side) => _leafCenters[side].parent.TransformPoint(_rest[side]);
        private float Rise(float flight)
        {
            float elapsed = _settings.flightDuration - flight;
            float fraction = elapsed <= AscentDuration ? elapsed / AscentDuration : 1 - Mathf.Clamp01((elapsed - AscentDuration) / (_settings.flightDuration - AscentDuration));
            return Mathf.Max(0, _settings.presentationRise * fraction);
        }
        private void Present(float flight, int output)
        {
            float rise = Rise(flight);
            for (int i = 0; i < 2; i++)
            {
                bool erupting = flight > 0 && i == output;
                _leafCenters[i].position = LaunchPoint(i) + (erupting ? Vector3.up * rise : Vector3.zero);
                if (erupting)
                {
                    _jets[i].transform.position = _jets[i].transform.parent.TransformPoint(_jetBottoms[i]) + Vector3.up * rise * .5f;
                    var scale = _jetScales[i];
                    scale.y *= rise / Mathf.Max(_jetHeights[i], Mathf.Epsilon);
                    _jets[i].transform.localScale = scale;
                }
                _jets[i].SetActive(erupting);
            }
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            _flight = Mathf.Max(0, _flight - dt); _cooldown = Mathf.Max(0, _cooldown - dt);
            for (int i = 0; i < 2; i++)
            {
                float amount = _ports[i].Consume();
                if (_cooldown > 0) { _charge[i] = 0; continue; }
                if (amount > 0) { _charge[i] += amount; _grace[i] = _settings.injectionGrace; }
                else { _grace[i] -= dt; if (_grace[i] <= 0) _charge[i] = 0; }
                if (_charge[i] < _settings.chargeUnits) continue;
                _output = 1 - i; _flight = _settings.flightDuration; _cooldown = _settings.cooldown;
                ReleaseRiders(false);
                _riders = 0;
                var leaf = _sockets[_output].InstalledLeaf;
                if (leaf != null)
                    foreach (var actor in StageActor.All)
                        if (actor.Available && actor.Slot >= 0 && actor.Slot < _carriedActors.Length && leaf.Supports(actor.GetComponent<CharacterController>(), _settings.standingTolerance))
                        {
                            _riders |= 1u << actor.Slot;
                            _carriedActors[actor.Slot] = actor;
                            _riderOffsets[actor.Slot] = actor.transform.position - LaunchPoint(_output);
                            actor.Abilities.Sap?.Cancel();
                            actor.Abilities.SetInputLock(this, true);
                        }
                _charge[0] = _charge[1] = 0;
                break;
            }
            Present(_flight, _output);
            bool atTop = _settings.flightDuration - _flight >= AscentDuration;
            CarryRiders(atTop ? _settings.presentationRise : Rise(_flight));
            if (atTop) ReleaseRiders(true);
            if (Online) { Charge0 = _charge[0]; Charge1 = _charge[1]; Flight = _flight; Output = _output; Riders = _riders; }
        }
        private void CarryRiders(float rise)
        {
            for (int slot = 0; slot < _carriedActors.Length; slot++)
            {
                var actor = _carriedActors[slot];
                if (actor == null || !actor.Available) continue;
                Vector3 point = LaunchPoint(_output) + Vector3.up * rise + _riderOffsets[slot];
                var player = actor.GetComponent<PlayerController>();
                player.Motor.SetMovementLock(this, true);
                var net = actor.GetComponent<NetworkPlayer>();
                if (net != null && net.Object != null && net.Object.IsValid) net.SetTransportAuthoritatively(true, point);
                else { player.SetTransport(this, point); player.Motor.Teleport(point); }
            }
        }
        private void ReleaseRiders(bool falling)
        {
            for (int slot = 0; slot < _carriedActors.Length; slot++)
            {
                var actor = _carriedActors[slot];
                if (actor == null) continue;
                var player = actor.GetComponent<PlayerController>();
                player.ClearTransport(this);
                player.Motor.SetMovementLock(this, false);
                actor.Abilities.SetInputLock(this, false);
                var net = actor.GetComponent<NetworkPlayer>();
                if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority)
                {
                    net.SetTransportAuthoritatively(false, actor.transform.position);
                    if (falling) net.LaunchAuthoritatively(Vector3.zero, player.Tuning.gravity, true);
                }
                else if (!Online && falling) player.Motor.Launch(Vector3.zero, player.Tuning.gravity, true);
                _carriedActors[slot] = null;
            }
            _riders = 0;
        }
    }
}
