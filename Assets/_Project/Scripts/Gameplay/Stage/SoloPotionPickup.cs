using System.Collections.Generic;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Levels
{
    // Removable, preauthored debug content. Uses the ordinary held item and drinking rules.
    public sealed class SoloPotionPickup : MonoBehaviour
    {
        [SerializeField] private StageItemDefinition _potion;
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private TMPro.TMP_Text _instructions;
        [SerializeField] private string _instructionsFormat = "솔로 테스트 · {0}\n{1}: 집기 / {2}: 복용";
        [SerializeField] private string _holdingMessage = "먼저 손에 든 아이템을 사용하세요.";
        private static readonly HashSet<SoloPotionPickup> Pickups = new();
        private bool _taken;
        private static bool Solo
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return FusionLobbySession.Instance == null || !FusionLobbySession.Instance.HasNetworkSession;
#else
                return false;
#endif
            }
        }
        private Vector3 Point => _interactionPoint != null ? _interactionPoint.position : transform.position;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Pickups.Clear();
        private void OnEnable() => Pickups.Add(this);
        private void OnDisable() => Pickups.Remove(this);
        private void Start()
        {
            if (!Solo) { gameObject.SetActive(false); return; }
            if (_instructions != null && _potion != null)
                foreach (var actor in StageActor.All)
                    if (actor.Local)
                    {
                        _instructions.text = string.Format(_instructionsFormat, _potion.displayName, actor.Binding((int)StageCommand.Interact), actor.Binding((int)StageCommand.Drink));
                        break;
                    }
        }
        private bool Reach(StageActor actor, StageOneFlow flow)
        {
            if (_taken || !isActiveAndEnabled || _potion == null || _potion.type != StageItemType.Potion || flow.settings.ItemId(_potion) <= 0) return false;
            Vector3 origin = actor.transform.position + flow.settings.interactionOffset;
            Vector3 delta = Point - origin;
            if (delta.sqrMagnitude > flow.settings.interactionRange * flow.settings.interactionRange) return false;
            foreach (var hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude, flow.settings.obstructionMask, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<StageActor>() == null && hit.collider.GetComponentInParent<SoloPotionPickup>() != this) return false;
            return true;
        }
        private static SoloPotionPickup Nearest(StageActor actor)
        {
            var flow = StageOneFlow.Instance;
            if (!Solo || flow == null || !flow.Authority || actor == null || !actor.Local || actor.Slot < 0 || actor.Slot >= flow.Progress.Held.Length) return null;
            SoloPotionPickup best = null; float distance = float.PositiveInfinity;
            foreach (var pickup in Pickups)
            {
                if (!pickup.Reach(actor, flow)) continue;
                float value = (actor.transform.position - pickup.Point).sqrMagnitude;
                if (value < distance) { distance = value; best = pickup; }
            }
            return best;
        }
        public static bool TryHint(StageActor actor, out Vector3 point)
        {
            point = default;
            var pickup = Nearest(actor);
            if (pickup == null || actor.Abilities.Unlocked || StageOneFlow.Instance.Progress.Held[actor.Slot] != 0) return false;
            point = pickup.Point; return true;
        }
        public static bool TryPickup(StageActor actor, out string message)
        {
            message = string.Empty;
            var pickup = Nearest(actor);
            if (pickup == null) return false;
            var flow = StageOneFlow.Instance;
            if (actor.Abilities.Unlocked) message = flow.settings.drinkBlockedMessage;
            else if (flow.Progress.Held[actor.Slot] != 0) message = pickup._holdingMessage;
            else
            {
                flow.Progress.Held[actor.Slot] = flow.settings.ItemId(pickup._potion);
                pickup._taken = true;
                pickup.gameObject.SetActive(false);
                message = pickup._potion.displayName + " · " + actor.Binding((int)StageCommand.Drink) + " 복용";
            }
            return true;
        }
    }
}
