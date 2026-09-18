using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Herbalist.Abilities;
using Herbalist.StageOne;
using Herbalist.Presentation;
namespace Herbalist.GameUI
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Herbalist.GameUI", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayerHudUI")]
    public sealed class PlayerHud : MonoBehaviour
    {
        public GameUiSettings settings;
        public int slot;
        public GameObject helpPanel, reticle, prompt;
        public TMP_Text helpText, helpHint, pocket, abilityText, feedback, promptText;
        public RectTransform[] markers;
        public TMP_Text[] markerLabels;
        public Vector3 promptOffset=new Vector3(0,.3f,0);
        private PlayScreenController screen;
        private InputAction help;
        private bool expanded;
        private string previousFeedback;
        private float feedbackTime;
        private void Awake() { screen=FindFirstObjectByType<PlayScreenController>(); }
        private void OnEnable() { help=settings.helpAction.action.Clone(); help.performed+=_=>expanded=!expanded; help.Enable(); }
        private void Update()
        {
            var view=screen!=null?screen.ViewForSlot(slot):null;
            if(view==null)return;
            var player=view.GetComponent<Herbalist.Player.PlayerController>();
            bool local=player.LocallyControlled;
            helpHint.text=settings.helpAction.action.GetBindingDisplayString()+"  "+settings.helpHint;
            helpPanel.SetActive(local&&expanded);
            if(local&&expanded)
            {
                var lines=new System.Text.StringBuilder();
                foreach(var h in settings.controls) if(h.action!=null)lines.AppendLine(h.action.action.GetBindingDisplayString()+"  "+h.label);
                helpText.text=lines.ToString();
            }
            var ability=view.GetComponent<PlayerAbilityController>();
            reticle.SetActive(local&&ability!=null&&ability.Unlocked&&(ability.Kind==PlayerAbilityKind.Sap?ability.Sap.Controlling:ability.Mode!=LeafMode.Off));
            abilityText.text=ability!=null&&ability.Unlocked?(ability.Kind==PlayerAbilityKind.Sap?settings.sapText:settings.leafText+" · "+settings.leafModes[(int)ability.Mode]):"";
            pocket.text="";
            var flow=StageOneFlow.Instance;
            var actor=view.GetComponent<StageActor>();
            if(flow!=null&&actor!=null&&actor.Available)
            {
                var item=flow.settings.Item(flow.Progress.Held[actor.Slot]); pocket.text=item!=null?item.displayName:settings.emptyPocket;
                if(actor.Feedback!=previousFeedback) { previousFeedback=actor.Feedback; feedbackTime=settings.feedbackDuration; }
            }
            if(!GameplayPause.IsPaused)feedbackTime-=Time.unscaledDeltaTime;
            feedback.text=local&&feedbackTime>0?previousFeedback:"";
            prompt.SetActive(false);
            if(local&&flow!=null&&actor!=null&&flow.TryInteractionHint(actor,out var point,out bool transfer))
            {
                promptText.text=actor.Binding(0)+"  "+(transfer?settings.giveText:settings.gatherText);
                Position(prompt.GetComponent<RectTransform>(),view.Camera,point+promptOffset);
            }
            for(int i=0;i<markers.Length;i++)
            {
                var target=screen.ViewForSlot(i);
                var a=target!=null?target.GetComponent<PlayerAbilityController>():null;
                if(a==null||!a.Unlocked) { markers[i].gameObject.SetActive(false); continue; }
                markerLabels[i].text=a.Kind==PlayerAbilityKind.Leaf?settings.leafText:settings.sapText;
                Position(markers[i],view.Camera,target.transform.position+settings.markerOffset);
            }
        }
        private void Position(RectTransform rect,Camera camera,Vector3 point)
        {
            Vector3 pos=camera.WorldToScreenPoint(point);
            bool visible=pos.z>0&&camera.pixelRect.Contains(pos);
            rect.gameObject.SetActive(visible);
            if(visible&&RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,pos,null,out var local))
            {
                var bounds=((RectTransform)transform).rect; var half=rect.rect.size*.5f;
                local.x=Mathf.Clamp(local.x,bounds.xMin+half.x,bounds.xMax-half.x);
                local.y=Mathf.Clamp(local.y,bounds.yMin+half.y,bounds.yMax-half.y);
                rect.anchoredPosition=local;
            }
        }
        private void OnDisable() { help?.Dispose(); help=null; }
    }
}
