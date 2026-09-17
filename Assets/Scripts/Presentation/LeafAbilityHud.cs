using Herbalist.Abilities;
using UnityEngine;
using UnityEngine.UI;
namespace Herbalist.Presentation
{
    public sealed class LeafAbilityHud : MonoBehaviour
    {
        [SerializeField] private PlayScreenController screen;
        [SerializeField] private int slot;
        [SerializeField] private Text status;
        [SerializeField] private GameObject reticle;
        [SerializeField] private string lockedText = "Leaf ability locked";
        [SerializeField] private string[] modeLabels = { "OFF", "PIN", "PLATFORM" };
        [SerializeField] private string countFormat = "LEAF  {0}   |   {1}/{2}";
        [SerializeField] private string returningText = "  Returning...";
        [SerializeField] private string sapIdleText = "SAP  |  Near tree: R to extract";
        [SerializeField] private string sapControlText = "SAP  |  Aim to move  |  R: cancel";
        [SerializeField] private string sapPlaceText = "SAP  |  LMB: attach / inject  |  R: cancel";
        [SerializeField] private string sapHoseText = "SAP  |  Hold LMB: spray  |  R: cancel";
        private void Update()
        {
            var view = screen.ViewForSlot(slot);
            var ability = view != null ? view.GetComponent<PlayerAbilityController>() : null;
            if (ability != null && ability.Unlocked && ability.Kind == PlayerAbilityKind.Sap)
            {
                bool controlling = ability.Sap != null && ability.Sap.Controlling;
                reticle.SetActive(controlling);
                status.text = !controlling ? sapIdleText :
                    ability.Sap.Settings.controlMode == SapControlMode.Hose && ability.Sap.Ready ? sapHoseText :
                    ability.Sap.CanPlace ? sapPlaceText : sapControlText;
                return;
            }
            bool active = ability != null && ability.Unlocked && ability.Mode != LeafMode.Off;
            reticle.SetActive(active);
            status.text = ability == null || !ability.Unlocked ? lockedText :
                string.Format(countFormat, modeLabels[(int)ability.Mode], ability.DisplayCount, ability.Leaf.Settings.capacity) + (ability.DisplayRecovering ? returningText : "");
        }
    }
}
