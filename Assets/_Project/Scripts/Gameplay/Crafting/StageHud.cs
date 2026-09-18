using UnityEngine;
using TMPro;
namespace Herbalist.StageOne
{
    public sealed class StageHud : MonoBehaviour
    {
        public TMP_Text objective, pocket, controls, feedback;
        public GameObject clearPanel;
        public TMP_Text clearText;
        private void Update()
        {
            var flow = StageOneFlow.Instance; if (flow == null) return;
            objective.text = flow.Objective;
            StageActor local = null;
            foreach (var actor in flow.Actors) if (actor != null && actor.Local) { local = actor; break; }
            if (local != null)
            {
                var item = flow.settings.Item(flow.Progress.Held[local.Slot]);
                var consumed = flow.settings.Item(flow.Progress.Consumed[local.Slot]);
                pocket.text = string.Format(flow.settings.pocketFormat, flow.settings.slotRoles[local.Slot], item != null ? item.displayName : flow.settings.emptyText, consumed != null ? consumed.displayName : flow.settings.lockedText);
                controls.text = string.Format(flow.settings.controlsFormat, local.Binding(0), local.Binding(1), local.Binding(2));
                feedback.text = local.Feedback ?? string.Empty;
            }
            else { pocket.text = flow.settings.waitingMessage; controls.text = feedback.text = string.Empty; }
            clearPanel.SetActive(flow.Progress.Cleared);
            clearText.text = flow.settings.clearMessage;
        }
    }
}
