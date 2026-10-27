using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Herbalist.Abilities;
using Herbalist.Player;
using Herbalist.Presentation;
using Herbalist.StageOne;
using Herbalist.GameUI;
namespace Herbalist.Levels
{
    public enum MazeControlView { KeepPlayerCamera, MazeCamera }

    [RequireComponent(typeof(NetworkObject))]
    public sealed class SapMazeBoard : NetworkBehaviour,IAbilityCycleReceiver,IAbilityPointerReceiver
    {
        [SerializeField] private SapMazeDefinition _definition;
        [SerializeField] private SapMazeSurface _surface;
        [SerializeField] private bool _requireSurface;
        [SerializeField] private Transform _startMarker;
        [SerializeField] private Transform _goalMarker;
        [SerializeField, Min(0)] private float _placementWarningDistance = .3f;
        [SerializeField] private MazeLeakSocket[] _leaks;
        [SerializeField] private Transform[] _refillMarkers;
        [SerializeField] private StageArrivalZone _controlArea;
        [SerializeField] private Transform _sapVisual;
        [SerializeField] private SapMazeTrailView _trailView;
        [SerializeField] private SapMazeVolumeDisplay _volumeDisplay;
        [SerializeField, Min(1)] private float _dropElongation = 1;
        [SerializeField] private Transform _frontCamera;
        [SerializeField] private MazeControlView _controlView = MazeControlView.MazeCamera;
        [SerializeField] private TMPro.TMP_Text _status;
        [SerializeField] private string _inactiveText="아직 활성화되지 않은 미로 · 이전 구간을 완료하세요";
        [SerializeField] private string _readyText="R: 미로 화면 열기";
        [SerializeField] private string _moveText="좌클릭 드래그: 수액 이동 · R: 미로 화면 닫기";
        [SerializeField] private string _retryText="수액이 소진되었습니다 · 현재 미로 재시도";
        [SerializeField] private string _solvedText="미로 완료 · 양쪽 하강 발판에 탑승하세요";
        [SerializeField] private string _needMoreVolumeText="도착에 필요한 수액이 부족합니다";
        [Networked] private int From {get;set;}
        [Networked] private int To {get;set;}
        [Networked] private float Progress {get;set;}
        [Networked] private float Volume {get;set;}
        [Networked] private int CollectedRefillMask {get;set;}
        [Networked] private float Retry {get;set;}
        [Networked] private int PhaseValue {get;set;}
        [Networked] private int Attempt {get;set;}
        [Networked] private NetworkBool Playable {get;set;}
        [Networked] private int LeakMask {get;set;}
        [Networked] private int ControllerSlot {get;set;}
        [Networked] private Vector2 SurfacePosition {get;set;}
        [Networked] private Vector2 SurfaceHeading {get;set;}
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
        private Vector2 _targetPoint;
        private bool _hasTarget;
        private readonly List<Vector2> _targetPath = new List<Vector2>();
        private Vector2 _surfacePosition, _surfaceHeading, _surfaceStart, _surfaceGoal;
        private Vector2[] _surfaceLeaks;
        private Vector2[] _surfaceRefills;
        private StageActor _candidate;
        private PlayerController _controller;
        private Vector3 _visualScale;
        private bool Online => Object!=null&&Object.IsValid;
        private bool Authority => !Online||HasStateAuthority;
        public SapMazeDefinition Definition=>_definition;
        public bool UsesRefillVolumeRules=>_definition!=null&&_definition.useRefillVolumeRules;
        public float RemainingVolume=>State.Volume;
        public float RequiredVolume=>_definition!=null?_definition.requiredVolume:0;
        public MazeLeakSocket[] Leaks=>_leaks;
        public StageArrivalZone ControlArea=>_controlArea;
        public MazeControlView ControlView {get=>_controlView;set=>_controlView=value;}
        public SapMazeSurface Surface => _surface;
        public Vector2 SurfaceStart => _surfaceStart;
        public Vector2 SurfaceGoal => _surfaceGoal;
        public Transform StartMarker => _startMarker;
        public Transform GoalMarker => _goalMarker;
        public Vector2 SapPosition => CurrentPosition();
        public bool Active=>Online?Playable:_playable;
        public bool AnyLeak=>(Online?LeakMask:_leakMask)!=0;
        public bool Controlling=>(Online?ControllerSlot:_controllerSlot)>=0;
        public static SapMazeBoard LocalMazeView
        {
            get
            {
                var flow = StageLevel.Instance != null ? StageLevel.Instance.GetComponent<DescendingMazeFlow>() : null;
                var board = flow != null ? flow.CurrentBoard : null;
                return board != null && board.Active && board.Controlling && board._controller != null &&
                    board._controller.LocallyControlled && board._controlView == MazeControlView.MazeCamera &&
                    board._frontCamera != null ? board : null;
            }
        }
        public bool IsViewedBy(PlayerView view) => view != null && _controller != null &&
            _controller.View == view && LocalMazeView == this;
        public MazeRunState State=>Online?new MazeRunState{From=From,To=To,Progress=Progress,Volume=Volume,
            CollectedRefillMask=CollectedRefillMask,RetryRemaining=Retry,
            Phase=(MazePhase)PhaseValue,Attempt=Attempt}:_state;
        private void Awake()
        {
            if(_definition!=null)_state=SapMazeSimulation.Initial(_definition);
            InitializeSurface();
            if(_sapVisual!=null)_visualScale=_sapVisual.localScale;
            ResetTrail();
        }
        private void InitializeSurface()
        {
            if(_surface==null || _definition==null)return;
            if(!_surface.Build(transform))
            {
                Debug.LogError("Maze surface could not be built for " + name, this);
                return;
            }
            _surfaceStart=ResolvePlacement(_startMarker,_definition.nodes[_definition.startNode],"start");
            _surfaceGoal=ResolvePlacement(_goalMarker,_definition.nodes[_definition.goalNode],"goal");
            _surfacePosition=_surfaceStart;
            _surfaceHeading=Vector2.zero;
            _surfaceLeaks=new Vector2[_leaks.Length];
            for(int i=0;i<_leaks.Length;i++)
            {
                var marker=_leaks[i]!=null?_leaks[i].transform:null;
                var fallback=_definition.leakNodes!=null && i<_definition.leakNodes.Length ?
                    _definition.nodes[_definition.leakNodes[i]] : Vector2.zero;
                _surfaceLeaks[i]=ResolvePlacement(marker,fallback,"leak " + (i+1));
                if(_leaks[i]!=null && _leaks[i].Target!=null)
                {
                    var target=_leaks[i].Target.transform;
                    var socket=_leaks[i].Target.Socket;
                    var socketOffset=socket!=null ? socket.position-target.position : Vector3.zero;
                    var local=transform.InverseTransformPoint(target.position);
                    target.position=transform.TransformPoint(new Vector3(_surfaceLeaks[i].x,_surfaceLeaks[i].y,local.z));
                    if(socket!=null)
                    {
                        socket.position=target.position+socketOffset;
                    }
                }
            }
            _surfaceRefills=new Vector2[_refillMarkers!=null?_refillMarkers.Length:0];
            for(int i=0;i<_surfaceRefills.Length;i++)
            {
                var marker=_refillMarkers[i];
                if(marker!=null)_surfaceRefills[i]=ResolvePlacement(marker,Vector2.zero,"refill " + (i+1));
            }
            var route=new List<Vector2>();
            if(!_surface.TryFindPath(_surfaceStart,_surfaceGoal,route))
                Debug.LogError("Maze surface has no route from start to goal: " + name, this);
        }
        private Vector2 ResolvePlacement(Transform marker,Vector2 fallback,string label)
        {
            if(marker==null)return _surface.Snap(fallback);
            var local=transform.InverseTransformPoint(marker.position);
            var placed=new Vector2(local.x,local.y);
            var snapped=_surface.Snap(placed);
            if(Vector2.Distance(placed,snapped)>_placementWarningDistance)
                Debug.LogWarning("Maze " + name + " " + label + " is outside its channel by " +
                    Vector2.Distance(placed,snapped).ToString("0.00") + " local units",marker);
            // Keep the designer's authored transform; only the simulation uses the corrected point.
            return snapped;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(Application.isPlaying || _surface==null || _definition==null)return;
            if(!_surface.Ready && !_surface.Build(transform))return;
            DrawPlacementWarning(_startMarker,"Start");
            DrawPlacementWarning(_goalMarker,"Goal");
            if(_leaks==null)return;
            for(int i=0;i<_leaks.Length;i++)
                DrawPlacementWarning(_leaks[i]!=null?_leaks[i].transform:null,"Leak " + (i+1));
            if(_refillMarkers!=null)
                for(int i=0;i<_refillMarkers.Length;i++)
                    DrawPlacementWarning(_refillMarkers[i],"Refill " + (i+1));
        }
        private void DrawPlacementWarning(Transform marker,string label)
        {
            if(marker==null)return;
            var local=transform.InverseTransformPoint(marker.position);
            var placed=new Vector2(local.x,local.y);
            var snapped=_surface.Snap(placed);
            var distance=Vector2.Distance(placed,snapped);
            if(distance<=_placementWarningDistance)return;
            var corrected=transform.TransformPoint(new Vector3(snapped.x,snapped.y,local.z));
            Gizmos.color=Color.yellow;
            Gizmos.DrawLine(marker.position,corrected);
            Gizmos.DrawWireSphere(corrected,.12f);
            UnityEditor.Handles.color=Color.yellow;
            UnityEditor.Handles.Label(marker.position,label + " outside channel: " + distance.ToString("0.00"));
        }
#endif
        public bool ConfigureOfflineTestDefinition(SapMazeDefinition definition)
        {
            if(Online || definition==null || (Herbalist.Networking.FusionLobbySession.Instance!=null &&
                Herbalist.Networking.FusionLobbySession.Instance.HasNetworkSession))return false;
            StopControl();ReleaseController();_definition=definition;_state=SapMazeSimulation.Initial(definition);
            InitializeSurface();
            _hasTarget=false;_leakMask=0;ResetTrail();return true;
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
            if((_requireSurface || _definition.useRefillVolumeRules) && (_surface==null || !_surface.Ready))return;
            UpdateController();
            if(!_playable){_hasTarget=false;return;}
            var before=_state.Phase;
            var attempt=_state.Attempt;
            if(_surface!=null && _surface.Ready)TickSurface(Controlling && _hasTarget,dt);
            else
            {
                var delta=_targetPoint-_definition.Position(_state);
                var worldDistance=transform.TransformVector(new Vector3(delta.x,delta.y,0)).magnitude;
                var input=Controlling && _hasTarget && (!_definition.limitPointerPathDistance || worldDistance<=_definition.maxTargetPathDistance) ? delta.normalized : Vector2.zero;
                SapMazeSimulation.Tick(_definition,ref _state,input,dt,i=>_leaks[i].Blocked);
            }
            if(_state.Attempt!=attempt){_surfacePosition=_surfaceStart;_surfaceHeading=Vector2.zero;ResetTrail();}
            else if(!_definition.useRefillVolumeRules && (_state.Phase==MazePhase.Running||_state.Phase==MazePhase.Solved))
                AppendTrail(CurrentPosition());
            if(before!=MazePhase.Retrying && _state.Phase==MazePhase.Retrying)
                foreach(var leak in _leaks)leak.ResetSocket();
            if(_state.Phase!=MazePhase.Running)StopControl();
            _leakMask=0;
            for(int i=0;i<_leaks.Length;i++)
            {
                bool wet=AtLeak(i);
                if(wet&&!_leaks[i].Blocked)_leakMask|=1<<i;
            }
            Publish();RefreshAvailability();UpdateController();
        }
        private Vector2 CurrentPosition() => _surface!=null && _surface.Ready ? Online?SurfacePosition:_surfacePosition : _definition.Position(State);
        private bool AtLeak(int i)
        {
            if(_surface==null || !_surface.Ready)return SapMazeSimulation.AtLeak(_definition,_state,i);
            if(_state.Phase!=MazePhase.Running)return false;
            var leak=_surfaceLeaks[i];
            var radius=_definition.leakRadius;
            var cursor=_surfacePosition;
            if(Vector2.Distance(cursor,leak)<=radius)return true;
            if(_trailView==null || _trailView.Settings==null)return false;

            // Use the same length budget as the visible LineRenderer. A leak keeps
            // draining after the head passes, but stops once the shrinking tail
            // no longer covers its socket.
            float remaining=_trailView.Settings.VisibleLength(VisualFraction(_state.Volume));
            for(int n=0;n<_trailCount && remaining>0;n++)
            {
                int index=(_trailFirst+_trailCount-1-n+TrailCapacity)%TrailCapacity;
                var previous=_trailPoints[index];
                float length=Vector2.Distance(cursor,previous);
                if(length<.0001f)continue;
                var end=length>remaining?Vector2.Lerp(cursor,previous,remaining/length):previous;
                var segment=end-cursor;
                if(segment.sqrMagnitude<.00000001f)break;
                float t=Mathf.Clamp01(Vector2.Dot(leak-cursor,segment)/segment.sqrMagnitude);
                if(Vector2.Distance(leak,cursor+segment*t)<=radius)return true;
                remaining-=length;
                cursor=previous;
            }
            return false;
        }
        private void TickSurface(bool moving,float dt)
        {
            if(dt<=0 || float.IsNaN(dt) || float.IsInfinity(dt) || _state.Phase==MazePhase.Solved)return;
            if(_state.Phase==MazePhase.Retrying)
            {
                _state.RetryRemaining-=dt;
                if(_state.RetryRemaining<=0)_state=SapMazeSimulation.Initial(_definition,_state.Attempt+1);
                return;
            }
            int waypoint=0;
            if(moving)
            {
                if(!_surface.TryFindPath(_surfacePosition,_targetPoint,_targetPath))moving=false;
                else
                {
                    // The first point is the centre of the cell already occupied by the sap.
                    // Chasing it on every tick can make the head oscillate at the cell edge.
                    if(_targetPath.Count>1)waypoint=1;
                    var first=_targetPath[0]-_surfacePosition;
                    float pathLength=transform.TransformVector(new Vector3(first.x,first.y,0)).magnitude;
                    for(int i=1;i<_targetPath.Count;i++)
                    {
                        var segment=_targetPath[i]-_targetPath[i-1];
                        pathLength+=transform.TransformVector(new Vector3(segment.x,segment.y,0)).magnitude;
                    }
                    if(_definition.limitPointerPathDistance && pathLength>_definition.maxTargetPathDistance) moving=false;
                }
            }
            int steps=Mathf.Max(1,Mathf.CeilToInt(dt*_definition.speed/Mathf.Max(.05f,_definition.leakRadius*.5f)));
            float stepTime=dt/steps;
            for(int step=0;step<steps && _state.Phase==MazePhase.Running;step++)
            {
                if(!_definition.useRefillVolumeRules)
                {
                    for(int i=0;i<_leaks.Length;i++)if(AtLeak(i) && !_leaks[i].Blocked)
                        _state.Volume=Mathf.Max(0,_state.Volume-_definition.lossPerSecond*stepTime);
                    if(_state.Volume<=0){_state.Phase=MazePhase.Retrying;_state.RetryRemaining=_definition.retryDelay;break;}
                }
                if(moving)
                {
                    while(waypoint<_targetPath.Count && Vector2.Distance(_surfacePosition,_targetPath[waypoint])<=_surface.CellSize*.35f)
                        waypoint++;
                    if(waypoint<_targetPath.Count)
                    {
                        var previous=_surfacePosition;
                        var delta=_targetPath[waypoint]-_surfacePosition;
                        _surfacePosition=_surface.Move(_surfacePosition,Vector2.ClampMagnitude(delta,_definition.speed*stepTime));
                        if((_surfacePosition-previous).sqrMagnitude>.000001f)_surfaceHeading=_surfacePosition-previous;
                        if(_definition.useRefillVolumeRules)AppendTrail(_surfacePosition);
                        else if(Vector2.Distance(_surfacePosition,_surfaceGoal)<=Mathf.Max(.12f,_definition.speed*stepTime))
                            _state.Phase=MazePhase.Solved;
                    }
                }
                if(_definition.useRefillVolumeRules)
                {
                    TickRefillVolume(stepTime);
                    if(_state.Phase!=MazePhase.Running)break;
                    if(Vector2.Distance(_surfacePosition,_surfaceGoal)<=Mathf.Max(.12f,_definition.speed*stepTime))
                    {
                        if(_state.Volume>=_definition.requiredVolume)_state.Phase=MazePhase.Solved;
                        else if(AllRefillsCollected())BeginRetry();
                    }
                }
            }
        }
        private void TickRefillVolume(float dt)
        {
            for(int i=0;i<_surfaceRefills.Length && i<32;i++)
            {
                int bit=1<<i;
                if(_refillMarkers[i]==null || (_state.CollectedRefillMask&bit)!=0)continue;
                if(Vector2.Distance(_surfacePosition,_surfaceRefills[i])>_definition.refillRadius)continue;
                _state.CollectedRefillMask|=bit;
                _state.Volume+=_definition.refillAmount;
            }
            // A leak drains continuously while any part of the visible stream covers it.
            // The authored layouts do not overlap leak regions, so charge at most one leak.
            for(int i=0;i<_leaks.Length;i++)
                if(!_leaks[i].Blocked && AtLeak(i))
                {
                    _state.Volume=Mathf.Max(0,_state.Volume-_definition.lossPerSecond*dt);
                    break;
                }
            if(_state.Volume<=0)BeginRetry();
        }
        private bool AllRefillsCollected()
        {
            for(int i=0;i<_refillMarkers.Length && i<32;i++)
                if(_refillMarkers[i]!=null && (_state.CollectedRefillMask&(1<<i))==0)return false;
            return true;
        }
        private void BeginRetry()
        {
            _state.Phase=MazePhase.Retrying;
            _state.RetryRemaining=_definition.retryDelay;
        }
        private float VisualFraction(float volume)
        {
            float capacity=_definition.initialVolume;
            if(_definition.useRefillVolumeRules && _refillMarkers!=null)
                foreach(var marker in _refillMarkers)if(marker!=null)capacity+=_definition.refillAmount;
            return Mathf.Clamp01(volume/Mathf.Max(.01f,capacity));
        }
        private void Publish()
        {
            if(!Online)return;
            From=_state.From;To=_state.To;Progress=_state.Progress;Volume=_state.Volume;
            CollectedRefillMask=_state.CollectedRefillMask;
            SurfacePosition=_surfacePosition;SurfaceHeading=_surfaceHeading;
            Retry=_state.RetryRemaining;PhaseValue=(int)_state.Phase;Attempt=_state.Attempt;
            Playable=_playable;LeakMask=_leakMask;ControllerSlot=_controllerSlot;
            TrailFirst=_trailFirst;TrailCount=_trailCount;
        }
        private void ResetTrail()
        {
            _trailFirst=0;_trailCount=_definition==null?0:1;
            if(_trailCount==0)return;
            _trailPoints[0]=_surface!=null && _surface.Ready?_surfaceStart:_definition.nodes[_definition.startNode];
            if(Online&&HasStateAuthority){var points=TrailPoints;points[0]=_trailPoints[0];TrailFirst=0;TrailCount=1;}
        }
        private void AppendTrail(Vector2 position)
        {
            if(_trailCount==0)return;
            var last=(_trailFirst+_trailCount-1)%TrailCapacity;
            float spacing=_trailView!=null && _trailView.Settings!=null ? _trailView.Settings.sampleSpacing : .1f;
            if(Vector2.Distance(_trailPoints[last],position)<spacing)return;
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
        public static bool TryReadLocalPointer(PlayerView view, out Vector2 boardPoint)
        {
            boardPoint = default;
            var flow = StageLevel.Instance != null ? StageLevel.Instance.GetComponent<DescendingMazeFlow>() : null;
            var board = flow != null ? flow.CurrentBoard : null;
            var camera = view != null ? view.Camera : null;
            if (board == null || !board.Active || !board.Controlling || board._controller == null ||
                board._controller.View != view || camera == null ||
                Mouse.current == null || !GameplayCursor.AllowsPointerInput) return false;
            return board.TryProjectPointer(camera, Mouse.current.position.ReadValue(), out boardPoint);
        }
        public bool TryProjectPointer(Camera camera, Vector2 screenPoint, out Vector2 boardPoint)
        {
            boardPoint = default;
            if (camera == null || !float.IsFinite(screenPoint.x) || !float.IsFinite(screenPoint.y)) return false;
            if (!camera.pixelRect.Contains(screenPoint)) return false;
            var ray = camera.ScreenPointToRay(screenPoint);
            Vector3 point;
            if (_surface != null && _surface.Ready && _surface.MazeCollider != null &&
                _surface.MazeCollider.Raycast(ray, out var hit, float.PositiveInfinity)) point = hit.point;
            else
            {
                var plane = new Plane(transform.forward, transform.position);
                if (!plane.Raycast(ray, out float distance) || distance < 0) return false;
                point = ray.GetPoint(distance);
            }
            var local = transform.InverseTransformPoint(point);
            boardPoint = new Vector2(local.x, local.y);
            return !float.IsNaN(boardPoint.x) && !float.IsNaN(boardPoint.y) &&
                   !float.IsInfinity(boardPoint.x) && !float.IsInfinity(boardPoint.y);
        }

        // R toggles the view. The pointer target is supplied separately each tick.
        public bool ReceiveAbilityCycle(PlayerAbilityController player,uint steps,bool held,Ray aim)
        {
            if(!Authority || _candidate==null || _candidate.Abilities!=player || !Active)return false;
            if(State.Phase==MazePhase.Running && steps%2!=0)
            {
                if(Controlling) StopControl();
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
        public void ReceiveAbilityPointer(PlayerAbilityController player, bool held, bool valid, Vector2 boardPoint)
        {
            if (!Authority || _candidate == null || _candidate.Abilities != player || !Controlling) return;
            _hasTarget = held && valid && State.Phase == MazePhase.Running &&
                !float.IsNaN(boardPoint.x) && !float.IsNaN(boardPoint.y) &&
                !float.IsInfinity(boardPoint.x) && !float.IsInfinity(boardPoint.y);
            if (_hasTarget) _targetPoint = _surface != null && _surface.Ready ?
                _surface.Snap(boardPoint) : boardPoint;
        }
        private void StopControl()
        {
            _controllerSlot=-1;_hasTarget=false;
            if(Online&&HasStateAuthority)ControllerSlot=-1;
        }
        private void UpdateController()
        {
            StageActor candidate=null;
            if(Active && State.Phase!=MazePhase.Solved)
                foreach(var actor in StageActor.All)
                    if(actor.Available && actor.Abilities.Unlocked && actor.Abilities.Kind==PlayerAbilityKind.Sap &&
                       _controlArea.ContainsPlayer(actor.transform.position))
                    {candidate=actor;break;}
            if(candidate!=_candidate)
            {
                if(_candidate!=null)
                {
                    _candidate.Abilities.ClearCycleReceiver(this);
                }
                _candidate=candidate;
                if(_candidate!=null)
                {
                    if(Authority)_candidate.Abilities.SetCycleReceiver(this);
                }
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
                }
            }
            if(_controller!=null)
            {
                if(_controlView==MazeControlView.MazeCamera && _frontCamera!=null)_controller.View.SetCameraOverride(this,_frontCamera);
                else _controller.View.ClearCameraOverride(this);
                if (_controller.LocallyControlled && GameplayCursor.Instance != null)
                    GameplayCursor.Instance.SetWorldPointerMode(this, _controlView == MazeControlView.MazeCamera);
            }
        }
        private void ReleaseController()
        {
            if(_controller==null)return;
            _controller.Motor.SetMovementLock(this,false);
            _controller.GetComponent<PlayerAbilityController>().SetInputLock(this,false);
            if (_controller.LocallyControlled && GameplayCursor.Instance != null)
                GameplayCursor.Instance.SetWorldPointerMode(this, false);
            _controller.View.ClearCameraOverride(this);_controller=null;_hasTarget=false;
        }
        private void Update()
        {
            UpdateController();
            if(_definition==null)return;
            var state=State;
            if(_volumeDisplay!=null)
                _volumeDisplay.Present(state.Volume,_definition.requiredVolume,
                    _definition.useRefillVolumeRules && Active && state.Phase==MazePhase.Running);
            if(_refillMarkers!=null)
                for(int i=0;i<_refillMarkers.Length;i++)
                    if(_refillMarkers[i]!=null)_refillMarkers[i].gameObject.SetActive(
                        _definition.useRefillVolumeRules && Active && state.Phase==MazePhase.Running &&
                        i<32 && (state.CollectedRefillMask&(1<<i))==0);
            if(_sapVisual!=null)
            {
                var visible=Active && state.Phase==MazePhase.Running;
                _sapVisual.gameObject.SetActive(visible);
                if(!visible)_trailView?.Clear();
                var p=CurrentPosition();
                _sapVisual.localPosition=new Vector3(p.x,p.y,_surface!=null && _surface.Ready ?
                    _surface.SurfaceZ(p,_sapVisual.localPosition.z) : _sapVisual.localPosition.z);
                var fraction=VisualFraction(state.Volume);
                var headScale=Mathf.Pow(fraction,1f/3f);
                if(visible&&_trailView!=null)
                {
                    ReadTrail();
                    headScale=_trailView.Present(_visibleTrail,p,fraction,_surface);
                    if (_surface != null && _surface.Ready)
                    {
                        var center = _trailView.HeadCenter;
                        _sapVisual.localPosition = new Vector3(center.x, center.y,
                            _surface.SurfaceZ(center, _sapVisual.localPosition.z));
                    }
                }
                _sapVisual.localScale=_visualScale*headScale;
                if (visible && _trailView != null && _trailView.HeadWidthWorld > 0 && _sapVisual.parent != null)
                {
                    var worldPerLocal = _sapVisual.parent.TransformVector(Vector3.right).magnitude;
                    if (worldPerLocal > .0001f)
                        _sapVisual.localScale = new Vector3(_trailView.HeadWidthWorld / worldPerLocal,
                            _sapVisual.localScale.y, _sapVisual.localScale.z);
                }
                var direction = _surface!=null && _surface.Ready ? Online?SurfaceHeading:_surfaceHeading :
                    _definition.nodes[state.To] - _definition.nodes[state.From];
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
            {
                if(!Active && state.Phase!=MazePhase.Solved)_status.text=_inactiveText;
                else if(state.Phase==MazePhase.Retrying)_status.text=_retryText;
                else if(state.Phase==MazePhase.Solved)_status.text=_solvedText;
                else if(_definition.useRefillVolumeRules &&
                    Vector2.Distance(CurrentPosition(),_surfaceGoal)<=Mathf.Max(.12f,_definition.refillRadius) &&
                    state.Volume<_definition.requiredVolume)_status.text=_needMoreVolumeText;
                else _status.text=Controlling?_moveText:_readyText;
            }
        }
        private void OnDisable()
        {
            if(_candidate!=null)
            {
                _candidate.Abilities.ClearCycleReceiver(this);
            }
            _candidate=null;ReleaseController();
            if (GameplayCursor.Instance != null) GameplayCursor.Instance.SetWorldPointerMode(this, false);
            if(Authority)StopControl();
        }
    }
}
