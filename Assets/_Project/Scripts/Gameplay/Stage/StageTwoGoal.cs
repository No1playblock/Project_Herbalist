using UnityEngine;
using UnityEngine.Events;
using Fusion;
using Herbalist.StageOne;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class StageTwoGoal : NetworkBehaviour
    {
        public BoxCollider arrival;
        public Vector3 playerProbeOffset=new Vector3(0,1,0);
        public TMPro.TMP_Text objective;
        public GameObject clearPanel;
        public string objectiveText="Reach the upper branch together.";
        public string clearedText="STAGE 2 CLEAR";
        public UnityEvent onCleared=new UnityEvent();
        [Networked] public NetworkBool Complete { get; private set; }
        private bool notified;
        public override void FixedUpdateNetwork()
        {
            if(Herbalist.GameUI.GameplayPause.IsPaused || !HasStateAuthority || Complete) return;
            bool first=false,second=false;
            foreach(var a in StageActor.All)
            {
                if(!a.Available || !Inside(a.transform.position+playerProbeOffset))continue;
                if(a.Slot==0)first=true; if(a.Slot==1)second=true;
            }
            Complete=first&&second;
        }
        public bool Inside(Vector3 position)
        {
            var p=arrival.transform.InverseTransformPoint(position)-arrival.center; var half=arrival.size*.5f;
            return Mathf.Abs(p.x)<=half.x&&Mathf.Abs(p.y)<=half.y&&Mathf.Abs(p.z)<=half.z;
        }
        private void Update()
        {
            bool done=Object!=null&&Object.IsValid&&Complete;
            if(objective!=null)objective.text=done?clearedText:objectiveText;
            if(clearPanel!=null)clearPanel.SetActive(done);
            if(done&&!notified){notified=true;onCleared.Invoke();}
        }
    }
}
