using Fusion;
using UnityEngine;
using Herbalist.StageOne;
using Herbalist.Networking;
using Herbalist.Player;
using Herbalist.GameUI;

namespace Herbalist.Levels
{
    // Host latches tutorial milestones; each peer presents the authored lines once.
    [RequireComponent(typeof(NetworkObject))]
    public sealed class StageThreeProgress : NetworkBehaviour
    {
        [SerializeField] private SapJumpPad _firstPad;
        [SerializeField] private StageArrivalZone _discovery;
        [SerializeField] private StageArrivalZone _firstLanding;
        [SerializeField] private StageArrivalZone[] _leafGaps;
        [SerializeField] private CooperativeStageExit _exit;
        [SerializeField] private StageArrivalZone _exitZone;
        [SerializeField] private GameOverlayHud _hud;
        [SerializeField] private string _initialObjective = "수액 점프대를 이용하세요.";
        [SerializeField] private string _upperObjective = "상단의 도착 지점까지 이동하세요.";
        [SerializeField] private string[] _lines = {
            "두영: 이 구멍들, 서로 이어져 있는 것 같은데?",
            "소담: 여기 올라간 다음 수액을 넣어보면 어떨까요?",
            "두영: 이걸 타고 위쪽으로 올라가면 되겠네.",
            "소담: 여기는 나뭇잎을 이용해야 할 거 같아요."
        };
        [SerializeField, Min(.1f)] private float _lineDuration = 4;
        [Networked] private int Milestones { get; set; }
        private int _offlineMilestones, _shown;
        private float _remaining;
        private bool Networked => Object != null && Object.IsValid;
        private int Flags => Networked ? Milestones : _offlineMilestones;
        public string Objective => _exitZone != null && _exitZone.CountPresent() > 0 ? _exit.Objective :
            (Flags & 4) != 0 ? _upperObjective : _initialObjective;
        public override void FixedUpdateNetwork() { if(HasStateAuthority) Milestones=Observe(Milestones); }
        private int Observe(int flags)
        {
            if(GameplayPause.IsPaused || _firstPad==null) return flags;
            if(_discovery.CountPresent()>0) flags|=1;
            if(_firstPad.LeafSlot.InstalledLeaf!=null) flags|=2;
            if(_firstPad.LaunchSequence>0)
                foreach(var actor in StageActor.All)
                    if(actor.Available && actor.GetComponent<PlayerController>().Motor.IsGrounded &&
                       _firstLanding.Contains(actor.transform.position+Vector3.up)) flags|=4;
            foreach(var gap in _leafGaps) if(gap.CountPresent()>0) flags|=8;
            return flags;
        }
        private void Update()
        {
            if(!Networked && (FusionLobbySession.Instance==null || !FusionLobbySession.Instance.HasNetworkSession))
                _offlineMilestones=Observe(_offlineMilestones);
            if(GameplayPause.IsPaused || _hud==null) return;
            _remaining-=Time.deltaTime;
            if(_remaining>0)return;
            for(int i=0;i<_lines.Length;i++)
            {
                int bit=1<<i;
                if((Flags&bit)==0 || (_shown&bit)!=0)continue;
                _shown|=bit; _remaining=_lineDuration;
                _hud.ShowSubtitle(_lines[i],_lineDuration); break;
            }
        }
    }
}