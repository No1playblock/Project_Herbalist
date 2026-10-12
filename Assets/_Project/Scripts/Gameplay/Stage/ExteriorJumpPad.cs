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
        private readonly Vector3[] _jetOrigins = new Vector3[2];
        private readonly MushroomWaterJetPresentation[] _waterJets = new MushroomWaterJetPresentation[2];
        private float _flight, _cooldown;
        private int _output;
        private uint _riders;
        private readonly StageActor[] _carriedActors = new StageActor[2];
        private bool Online => Object != null && Object.IsValid;
        public bool Authority => Online ? HasStateAuthority : FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
        public float ChargeFraction => Mathf.Max(Online ? Charge0 : _charge[0], Online ? Charge1 : _charge[1]) / _settings.chargeUnits;
        private void Awake()
        {
            for (int i = 0; i < _leafCenters.Length; i++)
            {
                _rest[i] = _leafCenters[i].localPosition;
                _jetOrigins[i] = _jets[i].transform.position;
                _waterJets[i] = _jets[i].GetComponent<MushroomWaterJetPresentation>();
                if (_waterJets[i] != null) _waterJets[i].Initialize(_settings);
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
            float elapsed = Mathf.Clamp(_settings.flightDuration - flight, 0, _settings.flightDuration);
            float ascent = AscentDuration;
            if (elapsed <= ascent)
            {
                float progress = elapsed / ascent;
                return _settings.presentationRise * progress * (2f - progress);
            }
            float descent = Mathf.Clamp01((elapsed - ascent) / (_settings.flightDuration - ascent));
            return _settings.presentationRise * (1f - descent * descent);
        }
        private void Present(float flight, int output)
        {
            float rise = Rise(flight);
            for (int i = 0; i < _leafCenters.Length; i++)
            {
                bool erupting = flight > 0 && i == output;
                _leafCenters[i].position = LaunchPoint(i) + (erupting ? Vector3.up * rise : Vector3.zero);
                _jets[i].SetActive(erupting);
                if (erupting && _waterJets[i] != null)
                    _waterJets[i].Present(_jetOrigins[i], _leafCenters[i].position);
            }
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            float previousFlight = _flight;
            _flight = Mathf.Max(0, _flight - dt); _cooldown = Mathf.Max(0, _cooldown - dt);
            for (int i = 0; i < _ports.Length; i++)
            {
                float amount = _ports[i].Consume();
                if (_cooldown > 0) { _charge[i] = 0; continue; }
                if (amount > 0) { _charge[i] += amount; _grace[i] = _settings.injectionGrace; }
                else { _grace[i] -= dt; if (_grace[i] <= 0) _charge[i] = 0; }
                if (_charge[i] < _settings.chargeUnits) continue;
                // A single inlet erupts on its own side; paired pads retain opposite-side output.
                _output = _ports.Length == 1 ? 0 : 1 - i;
                _flight = _settings.flightDuration; _cooldown = _settings.cooldown;
                ReleaseRiders(false);
                _riders = 0;
                var leaf = _sockets[_output].InstalledLeaf;
                if (leaf != null)
                    foreach (var actor in StageActor.All)
                        if (actor.Available && actor.Slot >= 0 && actor.Slot < _carriedActors.Length && leaf.Supports(actor.GetComponent<CharacterController>(), _settings.standingTolerance))
                        {
                            _riders |= 1u << actor.Slot;
                            _carriedActors[actor.Slot] = actor;
                            actor.Abilities.Sap?.Cancel();
                            actor.Abilities.SetInputLock(this, true);
                        }
                _charge[0] = _charge[1] = 0;
                break;
            }
            bool atTop = _settings.flightDuration - _flight >= AscentDuration;
            float previousRise = previousFlight > 0 ? Rise(previousFlight) : 0;
            CarryRiders(Mathf.Max(0, Rise(_flight) - previousRise) / Mathf.Max(dt, Mathf.Epsilon));
            if (atTop && _settings.flightDuration - previousFlight >= AscentDuration) ReleaseRiders(true);
            if (Online) { Charge0 = _charge[0]; Charge1 = _charge[1]; Flight = _flight; Output = _output; Riders = _riders; }
        }
        private void CarryRiders(float liftSpeed)
        {
            for (int slot = 0; slot < _carriedActors.Length; slot++)
            {
                var actor = _carriedActors[slot];
                if (actor == null || !actor.Available) continue;
                var player = actor.GetComponent<PlayerController>();
                var net = actor.GetComponent<NetworkPlayer>();
                if (net != null && net.Object != null && net.Object.IsValid) net.SetLiftAuthoritatively(liftSpeed);
                else player.Motor.SetLiftSpeed(liftSpeed);
            }
        }
        private void ReleaseRiders(bool falling)
        {
            for (int slot = 0; slot < _carriedActors.Length; slot++)
            {
                var actor = _carriedActors[slot];
                if (actor == null) continue;
                var player = actor.GetComponent<PlayerController>();
                player.Motor.SetLiftSpeed(0);
                actor.Abilities.SetInputLock(this, false);
                var net = actor.GetComponent<NetworkPlayer>();
                if (net != null && net.Object != null && net.Object.IsValid && net.HasStateAuthority)
                {
                    net.SetLiftAuthoritatively(0);
                    if (falling) net.LaunchAuthoritatively(Vector3.zero, player.Tuning.gravity, true);
                }
                else if (!Online && falling) player.Motor.Launch(Vector3.zero, player.Tuning.gravity, true);
                _carriedActors[slot] = null;
            }
            _riders = 0;
        }
    }
}
