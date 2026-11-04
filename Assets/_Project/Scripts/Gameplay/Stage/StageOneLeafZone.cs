using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Authored aim targets have no solid floor. Only leaf queries include these triggers.
    [RequireComponent(typeof(Herbalist.Abilities.LeafInstallTarget))]
    public sealed class StageOneLeafZone : MonoBehaviour
    {
        private static readonly HashSet<StageOneLeafZone> _zones = new();
        private static readonly RaycastHit[] _hits = new RaycastHit[128];
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _zones.Clear();
        private void OnEnable() => _zones.Add(this);
        private void OnDisable() => _zones.Remove(this);

        public static bool Cast(Ray ray, float radius, float distance, int mask, out RaycastHit hit)
        {
            if (_zones.Count == 0)
                return radius > 0 ? Physics.SphereCast(ray, radius, out hit, distance, mask, QueryTriggerInteraction.Ignore)
                    : Physics.Raycast(ray, out hit, distance, mask, QueryTriggerInteraction.Ignore);
            int count = radius > 0 ? Physics.SphereCastNonAlloc(ray, radius, _hits, distance, mask, QueryTriggerInteraction.Collide)
                : Physics.RaycastNonAlloc(ray, _hits, distance, mask, QueryTriggerInteraction.Collide);
            var hits = _hits;
            if (count == _hits.Length)
            {
                hits = radius > 0 ? Physics.SphereCastAll(ray, radius, distance, mask, QueryTriggerInteraction.Collide)
                    : Physics.RaycastAll(ray, distance, mask, QueryTriggerInteraction.Collide);
                count = hits.Length;
            }
            hit = default;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var candidate = hits[i];
                var zone = candidate.collider.GetComponent<StageOneLeafZone>();
                if (candidate.collider.isTrigger && (zone == null || !zone.isActiveAndEnabled)) continue;
                if (candidate.distance >= nearest) continue;
                nearest = candidate.distance;
                hit = candidate;
            }
            return nearest < float.PositiveInfinity;
        }
    }
}
