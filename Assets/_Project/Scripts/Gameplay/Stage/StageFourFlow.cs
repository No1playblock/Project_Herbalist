using Fusion;
using UnityEngine;
using Herbalist.StageOne;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.GameUI;
using Herbalist.Player;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class StageFourFlow : NetworkBehaviour,IStageExitCondition
    {
        [SerializeField] private SapMazeBoard[] _mazes;
        [SerializeField] private StagePulleyLift[] _lifts;
        [SerializeField] private StageArrivalZone _exitZone;
        [SerializeField] private CooperativeStageExit _exit;
        [SerializeField] private GameOverlayHud _hud;
        [SerializeField] private float _fallMargin=4;
        [SerializeField] private Vector3 _recoveryOffset=Vector3.up*.1f;
        [SerializeField] private string[] _objectives={"양쪽 발판에 각자 올라서세요 · 앞: 수액 / 뒤: 나뭇잎","R로 수액 조종을 시작하고 WASD로 도착 지점까지 이동시키세요.","새어 나오는 수액을 발판 나뭇잎으로 막으세요.","하강 발판을 이용해 아래로 이동하세요.","모든 미로의 수액을 도착 지점까지 이동시키세요.","하단 도착 지점에 함께 모이세요."};
        [Networked] private int Current {get;set;}
        [Networked] private int Tutorial {get;set;}
        private int _current=-1,_tutorial,_seen;
        private StageActor _offlineTestActor;
        private string _offlineControlText,_offlineLiftText,_offlineExitText;
        public StageArrivalZone ExitZone=>_exitZone;
        public bool OfflineTestActive=>_offlineTestActor!=null && !Online && (FusionLobbySession.Instance==null||!FusionLobbySession.Instance.HasNetworkSession);
        private bool Online=>Object!=null&&Object.IsValid;
        public int CurrentMaze=>Online?Current:_current;
        public SapMazeBoard[] Mazes=>_mazes;
        public StagePulleyLift[] Lifts=>_lifts;
        public bool CanExit=>Finished;
        public bool Finished=>CurrentMaze>=_mazes.Length;
        public string Objective
        {
            get
            {
                int current=CurrentMaze;
                if(OfflineTestActive)return Finished?_offlineExitText:
                    current>=0 && _mazes[current].State.Phase==MazePhase.Solved?_offlineLiftText:_offlineControlText;
                if(Finished)return _exitZone.CountPresent()>0?_exit.Objective:_objectives[5];
                if(current<0)return _objectives[0];
                var state=_mazes[current].State;
                return state.Phase==MazePhase.Solved?_objectives[3]:
                    _mazes[current].AnyLeak?_objectives[2]:current==0?_objectives[1]:_objectives[4];
            }
        }
        public bool BeginOfflineTest(StageActor actor,Vector3 spawnOffset,string controlText,string liftText,string exitText)
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return false;
#else
            if(Online || _current!=-1 || actor==null || !actor.Available ||
                (FusionLobbySession.Instance!=null&&FusionLobbySession.Instance.HasNetworkSession))return false;
            if(!_lifts[0].CompleteOfflineForTesting())return false;
            _offlineTestActor=actor;_offlineControlText=controlText;_offlineLiftText=liftText;_offlineExitText=exitText;
            _current=0;actor.GetComponent<PlayerController>().Motor.Teleport(_lifts[0].Upper[0].position+spawnOffset);
            for(int i=0;i<_mazes.Length;i++)_mazes[i].SetPlayable(i==0);
            return true;
#endif
        }
        public void EndOfflineTest()
        {
            if(!OfflineTestActive)return;
            foreach(var maze in _mazes)maze.SetPlayable(false);
            foreach(var lift in _lifts)lift.ResetOfflineForTesting();
            _offlineTestActor=null;_current=-1;_tutorial=0;_seen=0;
        }
        public override void Spawned(){if(HasStateAuthority)Current=-1;}
        public override void FixedUpdateNetwork(){if(HasStateAuthority)Tick(Runner.DeltaTime);}
        private void Update()
        {
            if(!Online && (FusionLobbySession.Instance==null||!FusionLobbySession.Instance.HasNetworkSession))Tick(Time.deltaTime);
            _exitZone.GetComponent<BoxCollider>().enabled=Finished;
            int flags=Online?Tutorial:_tutorial;
            if(_hud!=null && !GameplayPause.IsPaused && flags!=_seen)
            {
                int added=flags&~_seen;_seen=flags;
                _hud.ShowSubtitle((added&2)!=0?_objectives[3]:_objectives[2]);
            }
        }
        public void Tick(float dt)
        {
            if((Online&&!HasStateAuthority)||GameplayPause.IsPaused)return;
            StageActor sap=null,leaf=null;
            foreach(var actor in StageActor.All)
            {
                if(!actor.Available||!actor.Abilities.Unlocked)continue;
                if(actor.Abilities.Kind==PlayerAbilityKind.Sap)sap=actor;
                else if(actor.Abilities.Kind==PlayerAbilityKind.Leaf)leaf=actor;
            }
            for(int i=0;i<_mazes.Length;i++){_mazes[i].SetPlayable(i==_current);_mazes[i].Tick(dt);}
            if(_current<_mazes.Length)
            {
                int liftIndex=_current+1;
                var lift=_lifts[liftIndex];
                bool ready=_current<0 || _mazes[_current].State.Phase==MazePhase.Solved;
                if(ready&&!lift.Moving&&!lift.Complete)
                {
                    if(OfflineTestActive)lift.TryBeginOfflineForTesting(_offlineTestActor);
                    else lift.TryBegin(sap,leaf);
                }
                lift.Tick(dt);
                if(lift.Complete)_current++;
                if(_current>=0&&_current<_mazes.Length)
                {
                    if(_mazes[_current].AnyLeak)_tutorial|=1;
                    if(_mazes[_current].State.Phase==MazePhase.Solved)_tutorial|=2;
                }
            }
            Recover(sap,0);Recover(leaf,1);
            if(Online){Current=_current;Tutorial=_tutorial;}
        }
        private void Recover(StageActor actor,int side)
        {
            if(actor==null)return;
            int station=Mathf.Clamp(_current+1,0,_lifts.Length-1);
            var lift=_lifts[station];
            if(lift.Moving)return;
            Vector3 point=_current<0?_lifts[0].Platforms[side].position:
                _current>=_mazes.Length?_lifts[_lifts.Length-1].Upper[side].position:_lifts[_current].Upper[side].position;
            if(actor.transform.position.y>=point.y-_fallMargin)return;
            actor.GetComponent<SapControlAbility>()?.Cancel();
            var network=actor.GetComponent<NetworkPlayer>();
            if(network!=null&&network.Object!=null&&network.Object.IsValid)network.TeleportAuthoritatively(point+_recoveryOffset);
            else actor.GetComponent<PlayerController>().Motor.Teleport(point+_recoveryOffset);
        }
    }
}