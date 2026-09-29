using UnityEngine;
using UnityEngine.SceneManagement;
using Herbalist.Levels;
using Herbalist.StageOne;
using Herbalist.Player;
using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.GameUI;
namespace Herbalist.Development
{
    // Scene-authored development adapter. Never active in a Photon session or a non-development player.
    [DisallowMultipleComponent]
    public sealed class StageFourSoloTest : MonoBehaviour
    {
        [SerializeField] private bool _enabledForOfflineTesting=true;
        [SerializeField] private StageFourFlow _flow;
        [SerializeField,Min(1)] private float _sapVolumeMultiplier=25;
        [SerializeField] private Vector3 _spawnOffset=Vector3.up*.05f;
        [SerializeField] private string _controlText="솔로 테스트 · R: 수액 조종 · Q: 능력 전환";
        [SerializeField] private string _liftText="솔로 테스트 · 한쪽 하강 발판에 탑승하세요";
        [SerializeField] private string _exitText="솔로 테스트 완료 · 하단 출구로 이동하세요";
        private bool _initialized,_transitionRequested;
        private StageActor _actor;
        private Vector3 _entryPosition;
        private SapMazeDefinition[] _originals,_copies;
        public bool EnabledForOfflineTesting {get=>_enabledForOfflineTesting;set=>_enabledForOfflineTesting=value;}
        public bool Active=>_initialized&&_flow!=null&&_flow.OfflineTestActive;
        public bool TransitionRequested=>_transitionRequested;
        private static bool Allowed
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return FusionLobbySession.Instance==null || !FusionLobbySession.Instance.HasNetworkSession;
#else
                return false;
#endif
            }
        }
        private void Update()
        {
            if(!_enabledForOfflineTesting || !Allowed || _flow==null)
            {
                if(_initialized)Restore();
                return;
            }
            if(GameplayPause.IsPaused)return;
            if(!_initialized)
            {
                StageActor only=null;int count=0;
                foreach(var actor in StageActor.All)if(actor.Available){only=actor;count++;}
                if(count!=1 || only==null || !only.Local || !only.Abilities.Unlocked)return;
                if(only.Abilities.Kind!=PlayerAbilityKind.Sap && !only.Abilities.TrySwitchOfflineAbility())return;
                _actor=only;_entryPosition=only.transform.position;
                _originals=new SapMazeDefinition[_flow.Mazes.Length];_copies=new SapMazeDefinition[_flow.Mazes.Length];
                for(int i=0;i<_copies.Length;i++)
                {
                    var board=_flow.Mazes[i];_originals[i]=board.Definition;
                    var copy=Instantiate(board.Definition);copy.name=board.Definition.name+" (Solo Test)";
                    copy.initialVolume*=_sapVolumeMultiplier;_copies[i]=copy;
                    board.ConfigureOfflineTestDefinition(copy);
                }
                if(!_flow.BeginOfflineTest(only,_spawnOffset,_controlText,_liftText,_exitText)){Restore();return;}
                _initialized=true;
            }
            if(!_transitionRequested && _flow.CanExit && _actor!=null &&
                _flow.ExitZone.Contains(_actor.transform.position+Vector3.up))
            {
                var level=_flow.GetComponent<StageLevel>();
                if(level==null || string.IsNullOrWhiteSpace(level.nextScenePath))return;
                _transitionRequested=true;
                SceneManager.LoadSceneAsync(level.nextScenePath);
            }
        }
        private void Restore()
        {
            if(_flow!=null && _initialized)
            {
                _flow.EndOfflineTest();
                if(_actor!=null)_actor.GetComponent<PlayerController>().Motor.Teleport(_entryPosition);
            }
            if(_copies!=null)
                for(int i=0;i<_copies.Length;i++)
                {
                    if(_flow!=null && _originals[i]!=null)_flow.Mazes[i].ConfigureOfflineTestDefinition(_originals[i]);
                    if(_copies[i]!=null)Destroy(_copies[i]);
                }
            _copies=null;_originals=null;_initialized=false;_transitionRequested=false;_actor=null;
        }
        private void OnDisable()=>Restore();
    }
}
