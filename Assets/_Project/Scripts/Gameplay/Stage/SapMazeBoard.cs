using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Herbalist.Abilities;
using Herbalist.Player;
using Herbalist.StageOne;
using Herbalist.GameUI;
namespace Herbalist.Levels
{
    public enum MazeControlView { KeepPlayerCamera, MazeCamera }

    [RequireComponent(typeof(NetworkObject))]
    public sealed class SapMazeBoard : NetworkBehaviour,IPlayerMovementReceiver,IAbilityCycleReceiver
    {
        [SerializeField] private SapMazeDefinition _definition;
        [SerializeField] private MazeLeakSocket[] _leaks;
        [SerializeField] private StageArrivalZone _controlArea;
        [SerializeField] private Transform _sapVisual;
        [SerializeField] private SapMazeTrailView _trailView;
        [SerializeField, Min(1)] private float _dropElongation = 1;
        [SerializeField] private Transform _frontCamera;
        [SerializeField] private MazeControlView _controlView = MazeControlView.KeepPlayerCamera;
        [SerializeField] private TMPro.TMP_Text _status;
        [SerializeField] private string _inactiveText="아직 활성화되지 않은 미로 · 이전 구간을 완료하세요";
        [SerializeField] private string _readyText="R: 수액 조종 시작 · 잔량 {0:0}%";
        [SerializeField] private string _moveText="WASD: 수액 이동 · R: 조종 종료 · 잔량 {0:0}%";
        [SerializeField] private string _retryText="수액이 소진되었습니다 · 현재 미로 재시도";
        [SerializeField] private string _solvedText="미로 완료 · 양쪽 하강 발판에 탑승하세요";
        [Networked] private int From {get;set;}
        [Networked] private int To {get;set;}
        [Networked] private float Progress {get;set;}
        [Networked] private float Volume {get;set;}
        [Networked] private float Retry {get;set;}
        [Networked] private int PhaseValue {get;set;}
        [Networked] private int Attempt {get;set;}
        [Networked] private NetworkBool Playable {get;set;}
        [Networked] private int LeakMask {get;set;}
        [Networked] private int ControllerSlot {get;set;}
        private const int TrailCapacity = 128;
        [Networked] private int TrailFirst {get;set;}
        [Networked] private int TrailCount {get;set;}
        [Networked, Capacity(TrailCapacity)] private NetworkArray<Vector2> TrailPoints => default;
        private MazeRunState _state;
        private readonly Vector2[] _trailPoints = new Vector2[TrailCapacity];
        private readonly List<Vector2> _visibleTrail = new List<Vector2>(TrailCapacity);
        private int _trailFirst, _trailCount;
        private bool _playable;
        private int _leakMask;
        private int _controllerSlot=-1;
        private Vector2 _movement;
        private StageActor _candidate;
        private PlayerController _controller;
        private Vector3 _visualScale;
        private bool Online => Object!=null&&Object.IsValid;
        private bool Authority => !Online||HasStateAuthority;
        public SapMazeDefinition Definition=>_definition;
        public MazeLeakSocket[] Leaks=>_leaks;
        public StageArrivalZone ControlArea=>_controlArea;
        public MazeControlView ControlView {get=>_controlView;set=>_controlView=value;}
        public bool Active=>Online?Playable:_playable;
        public bool AnyLeak=>(Online?LeakMask:_leakMask)!=0;
        public bool Controlling=>(Online?ControllerSlot:_controllerSlot)>=0;
        public MazeRunState State=>Online?new MazeRunState{From=From,To=To,Progress=Progress,Volume=Volume,RetryRemaining=Retry,Phase=(MazePhase)PhaseValue,Attempt=Attempt}:_state;
        private void Awake()
        {
            if(_definition!=null)_state=SapMazeSimulation.Initial(_definition);
            if(_sapVisual!=null)_visualScale=_sapVisual.localScale;
            ResetTrail();
        }
        public bool ConfigureOfflineTestDefinition(SapMazeDefinition definition)
        {
            if(Online || definition==null || (Herbalist.Networking.FusionLobbySession.Instance!=null &&
                Herbalist.Networking.FusionLobbySession.Instance.HasNetworkSession))return false;
            StopControl();ReleaseController();_definition=definition;_state=SapMazeSimulation.Initial(definition);
            _movement=Vector2.zero;_leakMask=0;ResetTrail();return true;
        }
        public override void Spawned(){if(HasStateAuthority){ResetTrail();Publish();}}
        public void SetPlayable(bool value)
        {
            if(!Authority)return;
            _playable=value;
            if(!value)StopControl();
            Publish();RefreshAvailability();UpdateController();
        }
        private void RefreshAvailability()
        {
            foreach(var leak in _leaks)leak.gameObject.SetActive(Active || State.Phase==MazePhase.Solved);
        }
        public void Tick(float dt)
        {
            if(!Authority || GameplayPause.IsPaused)return;
            UpdateController();
            if(!_playable){_movement=Vector2.zero;return;}
            var before=_state.Phase;
            var attempt=_state.Attempt;
            SapMazeSimulation.Tick(_definition,ref _state,Controlling?_movement:Vector2.zero,dt,i=>_leaks[i].Blocked);
            _movement=Vector2.zero;
            if(_state.Attempt!=attempt)ResetTrail();
            else if(_state.Phase==MazePhase.Running||_state.Phase==MazePhase.Solved)
                AppendTrail(_definition.Position(_state));
            if(before!=MazePhase.Retrying && _state.Phase==MazePhase.Retrying)
                foreach(var leak in _leaks)leak.ResetSocket();
            if(_state.Phase!=MazePhase.Running)StopControl();
            _leakMask=0;
            for(int i=0;i<_leaks.Length;i++)
            {
                bool wet=SapMazeSimulation.AtLeak(_definition,_state,i);
                if(wet&&!_leaks[i].Blocked)_leakMask|=1<<i;
            }
            Publish();RefreshAvailability();UpdateController();
        }
        private void Publish()
        {
            if(!Online)return;
            From=_state.From;To=_state.To;Progress=_state.Progress;Volume=_state.Volume;
            Retry=_state.RetryRemaining;PhaseValue=(int)_state.Phase;Attempt=_state.Attempt;
            Playable=_playable;LeakMask=_leakMask;ControllerSlot=_controllerSlot;
            TrailFirst=_trailFirst;TrailCount=_trailCount;
        }
        private void ResetTrail()
        {
            _trailFirst=0;_trailCount=_definition==null?0:1;
            if(_trailCount==0)return;
            _trailPoints[0]=_definition.nodes[_definition.startNode];
            if(Online&&HasStateAuthority){var points=TrailPoints;points[0]=_trailPoints[0];TrailFirst=0;TrailCount=1;}
        }
        private void AppendTrail(Vector2 position)
        {
            if(_trailView==null||_trailView.Settings==null||_trailCount==0)return;
            var last=(_trailFirst+_trailCount-1)%TrailCapacity;
            if(Vector2.Distance(_trailPoints[last],position)<_trailView.Settings.sampleSpacing)return;
            var next=(_trailFirst+_trailCount)%TrailCapacity;
            if(_trailCount==TrailCapacity)_trailFirst=(_trailFirst+1)%TrailCapacity;
            else _trailCount++;
            _trailPoints[next]=position;
            if(Online&&HasStateAuthority){var points=TrailPoints;points[next]=position;}
        }
        private void ReadTrail()
        {
            _visibleTrail.Clear();
            var first=Online?TrailFirst:_trailFirst;
            var count=Online?TrailCount:_trailCount;
            for(var i=0;i<count&&i<TrailCapacity;i++)
                _visibleTrail.Add(Online?TrailPoints[(first+i)%TrailCapacity]:_trailPoints[(first+i)%TrailCapacity]);
        }
        public void ReceiveMovement(PlayerController player,Vector2 movement)
        {
            if(Authority && player==_controller && Active && State.Phase==MazePhase.Running)_movement=movement;
        }
        // Receives the existing rebindable Cycle action through the normal offline/Fusion ability input path.
        public bool ReceiveAbilityCycle(PlayerAbilityController player,uint steps)
        {
            if(!Authority || _candidate==null || _candidate.Abilities!=player || !Active)return false;
            if(State.Phase==MazePhase.Running && steps%2!=0)
            {
                if(Controlling)StopControl();
                else
                {
                    player.Sap.Cancel();
                    _controllerSlot=_candidate.Slot;
                }
                Publish();UpdateController();
            }
            // Suppress the ordinary hose ability in the active maze control area, including while not controlling.
            return true;
        }
        private void StopControl()
        {
            _controllerSlot=-1;_movement=Vector2.zero;
            if(Online&&HasStateAuthority)ControllerSlot=-1;
        }
        private void UpdateController()
        {
            StageActor candidate=null;
            if(Active && State.Phase!=MazePhase.Solved)
                foreach(var actor in StageActor.All)
                    if(actor.Available && actor.Abilities.Unlocked && actor.Abilities.Kind==PlayerAbilityKind.Sap &&
                       _controlArea.Contains(actor.transform.position+Vector3.up))
                    {candidate=actor;break;}
            if(candidate!=_candidate)
            {
                if(_candidate!=null)_candidate.Abilities.ClearCycleReceiver(this);
                _candidate=candidate;
                if(_candidate!=null && Authority)_candidate.Abilities.SetCycleReceiver(this);
            }
            int slot=Online?ControllerSlot:_controllerSlot;
            if(Authority && slot>=0 && (candidate==null||candidate.Slot!=slot))
            {StopControl();slot=-1;Publish();}
            var next=candidate!=null && candidate.Slot==slot?candidate.GetComponent<PlayerController>():null;
            if(next!=_controller)
            {
                ReleaseController();_controller=next;
                if(_controller!=null)
                {
                    _controller.GetComponent<PlayerAbilityController>().SetInputLock(this,true);
                    _controller.Motor.SetMovementLock(this,true);
                    if(Authority)_controller.SetMovementReceiver(this);
                }
            }
            if(_controller!=null)
            {
                if(_controlView==MazeControlView.MazeCamera && _frontCamera!=null)_controller.View.SetCameraOverride(this,_frontCamera);
                else _controller.View.ClearCameraOverride(this);
            }
        }
        private void ReleaseController()
        {
            if(_controller==null)return;
            _controller.ClearMovementReceiver(this);_controller.Motor.SetMovementLock(this,false);
            _controller.GetComponent<PlayerAbilityController>().SetInputLock(this,false);
            _controller.View.ClearCameraOverride(this);_controller=null;_movement=Vector2.zero;
        }
        private void Update()
        {
            UpdateController();
            if(_definition==null)return;
            var state=State;
            if(_sapVisual!=null)
            {
                var visible=Active && state.Phase==MazePhase.Running;
                _sapVisual.gameObject.SetActive(visible);
                if(!visible)_trailView?.Clear();
                var p=_definition.Position(state);
                _sapVisual.localPosition=new Vector3(p.x,p.y,_sapVisual.localPosition.z);
                var fraction=Mathf.Clamp01(state.Volume/_definition.initialVolume);
                var headScale=Mathf.Pow(fraction,1f/3f);
                if(visible&&_trailView!=null)
                {
                    ReadTrail();
                    headScale=_trailView.Present(_visibleTrail,p,fraction);
                }
                _sapVisual.localScale=_visualScale*headScale;
                var direction = _definition.nodes[state.To] - _definition.nodes[state.From];
                if (direction.sqrMagnitude > .000001f)
                {
                    _sapVisual.localRotation = Quaternion.FromToRotation(Vector3.up, new Vector3(direction.x, direction.y, 0));
                    _sapVisual.localScale = Vector3.Scale(_sapVisual.localScale, new Vector3(1, _dropElongation, 1));
                }
            }
            for(int i=0;i<_leaks.Length;i++)
            {
                _leaks[i].gameObject.SetActive(Active || state.Phase==MazePhase.Solved);
                _leaks[i].Present(((Online?LeakMask:_leakMask)&(1<<i))!=0);
            }
            if(_status!=null)
                _status.text=!Active && state.Phase!=MazePhase.Solved?_inactiveText:state.Phase==MazePhase.Running?string.Format(Controlling?_moveText:_readyText,state.Volume/_definition.initialVolume*100):
                    state.Phase==MazePhase.Retrying?_retryText:_solvedText;
        }
        private void OnDisable()
        {
            if(_candidate!=null)_candidate.Abilities.ClearCycleReceiver(this);
            _candidate=null;ReleaseController();
            if(Authority)StopControl();
        }
    }
}
