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
        private LeafInstallTarget _target;
        private LeafProjectile _leaf;
        private float _remaining;
        private bool _stepped;
        private void Awake() => _target = GetComponent<LeafInstallTarget>();
        public void Tick(float dt)
        {
            if (StageOneFlow.Instance == null || !StageOneFlow.Instance.Authority || Herbalist.GameUI.GameplayPause.IsPaused) return;
            var leaf = _target.InstalledLeaf;
            if (leaf != _leaf) { _leaf = leaf; _stepped = false; _remaining = _returnDelay; }
            if (leaf == null) return;
            if (FusionLobbySession.Instance is { HasNetworkSession: true } && IsOccupied(leaf, leafAbilityOnly: true))
            {
                leaf.BeginReturn();
                return;
            }
            bool occupied = (!_stepped || _recallAfterLeaving) && IsOccupied(leaf);
            if (occupied)
            {
                _stepped = true;
                if (_recallAfterLeaving) { _remaining = _returnDelay; return; }
            }
            if (_stepped && (_remaining -= dt) <= 0) leaf.BeginReturn();
        }
        public bool DefersExpiry(LeafProjectile leaf) => isActiveAndEnabled && _recallAfterLeaving &&
            ((_leaf == leaf && _stepped) || IsOccupied(leaf));

        private bool IsOccupied(LeafProjectile leaf, bool leafAbilityOnly = false)
        {
            foreach (var actor in StageActor.All)
                if (actor.Available && !HangingCloth.IsRiding(actor) &&
                    (!leafAbilityOnly || actor.Abilities is { Unlocked: true, Kind: PlayerAbilityKind.Leaf }) &&
                    leaf.Supports(actor.GetComponent<CharacterController>(), _standingTolerance))
                    return true;
            return false;
        }
        private void Update() => Tick(Time.deltaTime);
    }
}
