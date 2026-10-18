using System.Collections.Generic;
using Herbalist.StageOne;
using UnityEngine;
namespace Herbalist.Abilities
{
    // Only accepted authoritative hose hits feed this port; ordinary sap marks are unchanged.
    public sealed class SapInjectionPort : MonoBehaviour
    {
        private static readonly HashSet<SapInjectionPort> ActivePorts = new();
        [SerializeField, Min(.01f)] private float _unitsPerSecond = 1;
        [SerializeField, Min(.1f)] private float _hintRange = 4f;
        private float _pending;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPorts() => ActivePorts.Clear();
        private void OnEnable() => ActivePorts.Add(this);
        public static bool TryInteractionHint(StageActor actor, out Vector3 point)
        {
            point = default;
            var ability = actor != null ? actor.Abilities : null;
            var rules = Herbalist.Levels.StageLevel.Instance?.abilityRules;
            if (actor == null || !actor.Available || ability == null || !ability.Unlocked ||
                ability.Kind != PlayerAbilityKind.Sap || ability.Sap?.Settings == null ||
                rules == null || !rules.contextualUse) return false;
            float nearest = float.PositiveInfinity;
            bool found = false;
            foreach (var port in ActivePorts)
            {
                float range = Mathf.Min(port._hintRange, ability.Sap.Settings.controlRange);
                float distance = Vector3.Distance(actor.transform.position, port.transform.position);
                if (distance > range || distance >= nearest) continue;
                nearest = distance;
                point = port.transform.position;
                found = true;
            }
            return found;
        }
        public bool Inject(float seconds)
        {
            if (!isActiveAndEnabled || seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return false;
            _pending += seconds * _unitsPerSecond;
            return true;
        }
        public float Consume() { float result = _pending; _pending = 0; return result; }
        private void OnDisable() { ActivePorts.Remove(this); _pending = 0; }
    }
}
