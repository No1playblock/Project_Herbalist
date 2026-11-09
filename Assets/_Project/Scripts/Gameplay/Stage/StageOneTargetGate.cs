using UnityEngine;

namespace Herbalist.Levels
{
    // Connects an authored leaf target to its stage mechanism.
    public sealed class StageOneTargetGate : MonoBehaviour
    {
        [SerializeField] private StageOneDocumentMechanism _mechanism;
        [SerializeField] private bool _requiresCompleted = true;
        public bool AllowsInstallation => _mechanism != null && _mechanism.Kind != StageOneDocumentMechanismKind.LowerBranch &&
            (_requiresCompleted ? _mechanism.Solved : _mechanism.Ready);
        public bool TryHitBranch() => _mechanism != null && _mechanism.TryHitBranch();
    }
}
