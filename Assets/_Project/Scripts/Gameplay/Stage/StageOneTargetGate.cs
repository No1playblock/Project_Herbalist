using UnityEngine;

namespace Herbalist.Levels
{
    // Only the authored bridge sockets behind the cooperative obstacle are gated.
    public sealed class StageOneTargetGate : MonoBehaviour
    {
        [SerializeField] private StageOneDocumentMechanism _mechanism;
        [SerializeField] private bool _requiresCompleted = true;
        public bool AllowsInstallation => _mechanism != null && (_requiresCompleted ? _mechanism.Solved : _mechanism.Ready);
    }
}
