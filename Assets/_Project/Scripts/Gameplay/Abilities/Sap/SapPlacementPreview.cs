using Herbalist.Player;
using UnityEngine;
namespace Herbalist.Abilities
{
    // Authored world-space mesh, not runtime UI. Only the owner sees a preview.
    [DefaultExecutionOrder(150)]
    public sealed class SapPlacementPreview : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerAbilityController abilities;
        [SerializeField] private GameObject preview;
        private void LateUpdate()
        {
            var sap = abilities.Sap;
            bool visible = player.LocallyControlled && abilities.Unlocked && abilities.Kind == PlayerAbilityKind.Sap &&
                sap != null && sap.Settings.controlMode == SapControlMode.Placement && sap.Controlling && sap.Ready;
            if (visible)
            {
                var aim = player.View.GetAimRay(player.View.Yaw, player.View.Pitch);
                var origin = sap.Held != null ? sap.Held.transform.position : sap.HoverPosition(aim);
                visible = sap.TryPlacement(aim, origin, out _, out var position, out var normal);
                if (visible)
                {
                    preview.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal));
                    preview.transform.localScale = sap.Settings.attachedScale;
                }
            }
            if (preview.activeSelf != visible) preview.SetActive(visible);
        }
        private void OnDisable() { if (preview != null) preview.SetActive(false); }
    }
}
