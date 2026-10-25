using Herbalist.Abilities;
using UnityEngine;

namespace Herbalist.Levels
{
    // Marks a complete jump-pad prefab and shows its placement points in the Scene view.
    [RequireComponent(typeof(ExteriorJumpPad))]
    public sealed class StageOneJumpPadAuthoring : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var inlet = GetComponentInChildren<SapInjectionPort>(true);
            var target = GetComponentInChildren<LeafInstallTarget>(true);
            if (inlet == null || target == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(inlet.transform.position, 0.6f);
            UnityEditor.Handles.Label(inlet.transform.position + Vector3.up, "Water inlet");

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.SocketPosition, 0.6f);
            UnityEditor.Handles.Label(target.SocketPosition + Vector3.up, "Leaf / water outlet");
        }
#endif
    }
}
