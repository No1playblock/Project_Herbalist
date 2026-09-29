using Fusion;
using UnityEngine;
using Herbalist.StageOne;
using Herbalist.Player;
using Herbalist.Networking;
using Herbalist.GameUI;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class StagePulleyLift : NetworkBehaviour
    {
        [SerializeField] private Transform[] _platforms;
        [Tooltip("Departure anchors; may be higher than the destination. Serialized name retained for scene compatibility.")]
        [SerializeField] private Transform[] _lower;
        [Tooltip("Destination anchors; can be below the departure for descending stages.")]
        [SerializeField] private Transform[] _upper;
        [SerializeField] private StageArrivalZone[] _boarding;
        [SerializeField] private Transform _counterweight;
        [SerializeField] private LineRenderer[] _ropes;
        [SerializeField] private Transform[] _pulleys;
        [SerializeField] private Vector3 _counterweightTravel=Vector3.up*2;
        [SerializeField,Min(.1f)] private float _duration=3;
        [Networked] private float Progress {get;set;}
        [Networked] private int Phase {get;set;}
        private float _progress;
        private int _phase;
        private Vector3 _boardRest;
        private readonly StageActor[] _riders=new StageActor[2];
        private readonly Vector3[] _offsets=new Vector3[2];
        private bool Online=>Object!=null&&Object.IsValid;
        public bool Moving=>(Online?Phase:_phase)==1;
        public bool Complete=>(Online?Phase:_phase)==2;
        public StageArrivalZone[] Boarding=>_boarding;
        public Transform[] Upper=>_upper;
        public Transform[] Platforms=>_platforms;
        private void Awake(){if(_counterweight!=null)_boardRest=_counterweight.localPosition;}
        public bool TryBegin(StageActor sap,StageActor leaf)
        {
            if((Online&&!HasStateAuthority)||_phase!=0 || sap==null||leaf==null||sap==leaf||sap.Slot==leaf.Slot)return false;
            var actors=new[]{sap,leaf};
            for(int i=0;i<actors.Length;i++)
                if(!actors[i].GetComponent<PlayerController>().Motor.IsGrounded ||
                   !_boarding[i].Contains(actors[i].transform.position+Vector3.up))return false;
            for(int i=0;i<actors.Length;i++)
            {
                _riders[i]=actors[i];_offsets[i]=actors[i].transform.position-_platforms[i].position;
                actors[i].GetComponent<Herbalist.Abilities.SapControlAbility>()?.Cancel();
                actors[i].GetComponent<PlayerController>().Motor.SetMovementLock(this,true);
            }
            _phase=1;Publish();return true;
        }
        private bool OfflineTestAllowed=>!Online && (FusionLobbySession.Instance==null||!FusionLobbySession.Instance.HasNetworkSession);
        public bool TryBeginOfflineForTesting(StageActor actor)
        {
            if(!OfflineTestAllowed || _phase!=0 || actor==null || !actor.Available)return false;
            var player=actor.GetComponent<PlayerController>();
            if(!player.Motor.IsGrounded)return false;
            int side=-1;
            for(int i=0;i<_boarding.Length;i++)if(_boarding[i].Contains(actor.transform.position+Vector3.up)){side=i;break;}
            if(side<0)return false;
            _riders[side]=actor;_offsets[side]=actor.transform.position-_platforms[side].position;
            actor.GetComponent<Herbalist.Abilities.SapControlAbility>()?.Cancel();
            player.Motor.SetMovementLock(this,true);_phase=1;return true;
        }
        public bool CompleteOfflineForTesting()
        {
            if(!OfflineTestAllowed || _phase!=0)return false;
            _progress=1;_phase=2;Present(1);return true;
        }
        public void ResetOfflineForTesting()
        {
            if(!OfflineTestAllowed)return;
            Release();
            for(int i=0;i<_riders.Length;i++)_riders[i]=null;
            _progress=0;_phase=0;Present(0);
        }
        public void Tick(float dt)
        {
            if((Online&&!HasStateAuthority)||GameplayPause.IsPaused||_phase!=1)return;
            _progress=Mathf.Min(1,_progress+dt/_duration);Present(_progress);
            for(int i=0;i<_riders.Length;i++)
            {
                if(_riders[i]==null)continue;
                var player=_riders[i].GetComponent<PlayerController>();
                var network=_riders[i].GetComponent<NetworkPlayer>();
                var point=_platforms[i].position+_offsets[i];
                if(network!=null&&network.Object!=null&&network.Object.IsValid)network.SetTransportAuthoritatively(true,point);
                else {player.SetTransport(this,point);player.Motor.Teleport(point);}
            }
            if(_progress>=1){_phase=2;Release();}
            Publish();
        }
        private void Publish(){if(Online){Progress=_progress;Phase=_phase;}}
        private void Present(float t)
        {
            t=t*t*(3-2*t);
            for(int i=0;i<_platforms.Length;i++)_platforms[i].position=Vector3.Lerp(_lower[i].position,_upper[i].position,t);
            if(_counterweight!=null)_counterweight.localPosition=_boardRest+_counterweightTravel*t;
            if(_ropes!=null&&_pulleys!=null)
                for(int i=0;i<_ropes.Length;i++)
                {
                    _ropes[i].SetPosition(0,_platforms[i].position);
                    _ropes[i].SetPosition(1,_pulleys[i].position);
                }
        }
        private void Update(){if(_platforms!=null&&_lower!=null&&_upper!=null)Present(Online?Progress:_progress);}
        private void Release()
        {
            foreach(var actor in _riders)
            {
                if(actor==null)continue;
                var player=actor.GetComponent<PlayerController>();player.ClearTransport(this);player.Motor.SetMovementLock(this,false);
                var n=actor.GetComponent<NetworkPlayer>();
                if(n!=null&&n.Object!=null&&n.Object.IsValid&&n.HasStateAuthority)n.SetTransportAuthoritatively(false,actor.transform.position);
            }
        }
        private void OnDisable(){if(!Online||HasStateAuthority)Release();}
    }
}