using Herbalist.Abilities;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    // Opt-in per target: jump-pad sockets and jet plugs do not consume on a footstep.
    [RequireComponent(typeof(LeafInstallTarget))]
    public sealed class StageOneLeafUse : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _returnDelay = .06f;
        [SerializeField, Min(.01f)] private float _standingTolerance = .35f;
        [SerializeField] private bool _recallAfterLeaving;
        [SerializeField] private StageOneDocumentMechanism _dropBranchOnLeafFootstep;
        private LeafInstallTarget _target;
        private LeafProjectile _leaf;
        private float _remaining;
        private bool _stepped;
        private static bool Multiplayer => FusionLobbySession.Instance is { HasNetworkSession: true };
        private void Awake() => _target = GetComponent<LeafInstallTarget>();
        public void Tick(float dt)
        {
            if (StageOneFlow.Instance == null || !StageOneFlow.Instance.Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            var leaf = _target.InstalledLeaf;
            if (leaf != _leaf) { _leaf = leaf; _stepped = false; _remaining = _returnDelay; }
            if (_dropBranchOnLeafFootstep != null)
            {
                if (IsOccupied(leaf, PlayerAbilityKind.Leaf))
                {
                    if (leaf != null) leaf.BeginReturn();
                    _dropBranchOnLeafFootstep.DropAfterLeafRecall();
                }
                return;
            }
            if (leaf == null) return;
            bool multiplayer = Multiplayer;
            if (multiplayer && IsOccupied(leaf, PlayerAbilityKind.Leaf))
            {
                leaf.BeginReturn();
                return;
            }
            bool sapStanding = multiplayer && IsOccupied(leaf, PlayerAbilityKind.Sap);
            if (sapStanding || ((!_stepped || _recallAfterLeaving) && IsOccupied(leaf)))
            {
                _stepped = true;
                if (sapStanding || _recallAfterLeaving) { _remaining = _returnDelay; return; }
            }
            if (_stepped && (_remaining -= dt) <= 0) leaf.BeginReturn();
        }
        public bool DefersExpiry(LeafProjectile leaf) => isActiveAndEnabled &&
            (_dropBranchOnLeafFootstep != null ||
             (_recallAfterLeaving && ((_leaf == leaf && _stepped) || IsOccupied(leaf))) ||
             (Multiplayer && IsOccupied(leaf, PlayerAbilityKind.Sap)));

        private bool IsOccupied(LeafProjectile leaf, PlayerAbilityKind? ability = null)
        {
            foreach (var actor in StageActor.All)
            {
                if (!actor.Available || HangingCloth.IsRiding(actor) || HangingCloth.IsGrabbing(actor) ||
                    (ability.HasValue && !(actor.Abilities is { Unlocked: true } abilities && abilities.Kind == ability.Value))) continue;
                var character = actor.GetComponent<CharacterController>();
                if ((leaf != null && leaf.Supports(character, _standingTolerance)) ||
                    (_dropBranchOnLeafFootstep != null && _dropBranchOnLeafFootstep.SupportsBranch(character, _standingTolerance)))
                    return true;
            }
            return false;
        }
        private void Update() => Tick(Time.deltaTime);
    }
}
