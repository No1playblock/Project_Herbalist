using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.StageOne;

namespace Herbalist.Levels
{
    public enum SapJumpPadPhase { Empty, WaitingForRiders, Ready, Charging, Cooldown }
    [RequireComponent(typeof(NetworkObject))]
    public sealed class SapJumpPad : NetworkBehaviour
    {
        [SerializeField] private SapJumpPadSettings _settings;
        [SerializeField] private LeafInstallTarget _leafSlot;
        [SerializeField] private SapInjectionPort _inlet;
        [SerializeField] private BoxCollider _riderVolume;
        [SerializeField] private Transform[] _landings;
        [SerializeField] private Transform _launchVisual;
        [SerializeField] private GameObject _readyVisual;
        [Networked] private float Charge { get; set; }
        [Networked] private float Grace { get; set; }
        [Networked] private float Cooldown { get; set; }
        [Networked] private int Riders { get; set; }
        [Networked] private int Sequence { get; set; }
        [Networked] private int PhaseValue { get; set; }
        private float _charge, _grace, _cooldown, _visualTime;
        private int _riders, _sequence, _phase, _seenSequence;
        private Vector3 _visualRest;
        private readonly List<StageActor> _aboard = new();
        private readonly HashSet<int> _slots = new();
        private bool Networked => Object != null && Object.IsValid;
        public bool Authority => !Networked || HasStateAuthority;
        public SapJumpPadPhase Phase => (SapJumpPadPhase)(Networked ? PhaseValue : _phase);
        public float ChargeFraction => Mathf.Clamp01((Networked ? Charge : _charge) / _settings.chargeUnits);
        public int RiderCount => Networked ? Riders : _riders;
        public int LaunchSequence => Networked ? Sequence : _sequence;
        public LeafInstallTarget LeafSlot => _leafSlot;
        public SapInjectionPort Inlet => _inlet;
        public BoxCollider RiderVolume => _riderVolume;
        public Transform[] Landings => _landings;
        public SapJumpPadSettings Settings => _settings;
        private void Awake() { if (_launchVisual != null) _visualRest = _launchVisual.localPosition; }
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            _charge=Charge; _grace=Grace; _cooldown=Cooldown; _sequence=Sequence;
            Tick(Runner.DeltaTime);
            Charge=_charge; Grace=_grace; Cooldown=_cooldown; Riders=_riders; Sequence=_sequence; PhaseValue=_phase;
        }
        private void Update()
        {
            if (!Networked && (FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession)) Tick(Time.deltaTime);
            if (Herbalist.GameUI.GameplayPause.IsPaused) return;
            if (_seenSequence != LaunchSequence) { _seenSequence=LaunchSequence; _visualTime=_settings.presentationDuration; }
            _visualTime=Mathf.Max(0,_visualTime-Time.deltaTime);
            if (_launchVisual != null)
            {
                _launchVisual.gameObject.SetActive(_visualTime>0);
                float p=1-_visualTime/_settings.presentationDuration;
                _launchVisual.localPosition=_visualRest+Vector3.up*(Mathf.Sin(p*Mathf.PI)*_settings.presentationRise);
            }
            if (_readyVisual != null) _readyVisual.SetActive(Phase==SapJumpPadPhase.Ready || Phase==SapJumpPadPhase.Charging);
        }
        public void Tick(float dt)
        {
            if (!Authority || Herbalist.GameUI.GameplayPause.IsPaused || _settings==null || dt<=0) return;
            float supplied=_inlet.Consume();
            var leaf=_leafSlot.InstalledLeaf;
            CollectRiders(leaf);
            if (_cooldown>0) { _cooldown=Mathf.Max(0,_cooldown-dt); _phase=(int)SapJumpPadPhase.Cooldown; return; }
            if (leaf==null || _riders<_settings.requiredRiders)
            {
                _charge=0; _grace=0;
                _phase=(int)(leaf==null?SapJumpPadPhase.Empty:SapJumpPadPhase.WaitingForRiders);
                return;
            }
            if (supplied>0) { _grace=_settings.injectionGrace; _charge+=supplied; }
            else { _grace-=dt; if(_grace<=0)_charge=0; }
            _phase=(int)(_charge>0?SapJumpPadPhase.Charging:SapJumpPadPhase.Ready);
            if (_charge<_settings.chargeUnits) return;
            if (_landings.Length<_settings.requiredRiders) { _charge=0; return; }
            _aboard.Sort((a,b)=>a.Slot.CompareTo(b.Slot));
            for(int i=0;i<_settings.requiredRiders;i++)
            {
                var actor=_aboard[i]; var player=actor.GetComponent<PlayerController>();
                actor.GetComponent<SapControlAbility>()?.Cancel();
                var v=CalculateLaunchVelocity(actor.transform.position,_landings[i].position,_settings.flightDuration,_settings.gravity,dt);
                var network=actor.GetComponent<NetworkPlayer>();
                if(network!=null && network.Object!=null && network.Object.IsValid) network.LaunchAuthoritatively(v,_settings.gravity);
                else player.Motor.Launch(v,_settings.gravity);
            }
            leaf.BeginReturn(); // Free the installed leaf through the existing recall/capacity path.
            _charge=0; _grace=0; _cooldown=_settings.cooldown; _sequence++; _phase=(int)SapJumpPadPhase.Cooldown;
        }
        private void CollectRiders(LeafProjectile leaf)
        {
            _aboard.Clear(); _slots.Clear();
            if(leaf!=null)
            foreach(var actor in StageActor.All)
            {
                if(!actor.Available || _slots.Contains(actor.Slot))continue;
                Vector3 p=_riderVolume.transform.InverseTransformPoint(actor.transform.position)-_riderVolume.center;
                Vector3 half=_riderVolume.size*.5f;
                if(Mathf.Abs(p.x)>half.x || Mathf.Abs(p.y)>half.y || Mathf.Abs(p.z)>half.z)continue;
                var motor=actor.GetComponent<PlayerController>().Motor;
                if(!motor.IsGrounded || !leaf.Supports(actor.GetComponent<CharacterController>(),_settings.standingTolerance))continue;
                _slots.Add(actor.Slot);_aboard.Add(actor);
            }
            _riders=_aboard.Count;
        }
        public static Vector3 CalculateLaunchVelocity(Vector3 start,Vector3 target,float time,float gravity,float dt)
            => (target-start)/time + Vector3.up*(gravity*(time+dt)*.5f);
    }
}
