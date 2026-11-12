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
        private static int _playerLayers;
        internal static int PlayerLayers => _playerLayers == 0
            ? _playerLayers = LayerMask.GetMask("Player", "character", "Ignore Raycast", "Default")
            : _playerLayers;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _zones.Clear();
        private void OnEnable() => _zones.Add(this);
        private void OnDisable() => _zones.Remove(this);
        internal static bool TryNearbyGuidePoint(Vector3 origin, float range, out Vector3 point)
        {
            point = default;
            float nearest = range * range;
            bool found = false;
            foreach (var zone in _zones)
            {
                var target = zone.GetComponent<Herbalist.Abilities.LeafInstallTarget>();
                if (target == null || !target.CanInstall) continue;
                Vector3 candidate = target.HasSocket ? target.SocketPosition : zone.transform.position;
                float distance = (candidate - origin).sqrMagnitude;
                if (distance > nearest) continue;
                nearest = distance;
                point = candidate;
                found = true;
            }
            return found;
        }

        public static bool Cast(Ray ray, float radius, float distance, int mask, out RaycastHit hit,
            Transform owner = null, bool includeLeafZones = true)
        {
            bool includeTriggers = includeLeafZones && _zones.Count != 0;
            if (!includeTriggers && owner == null)
                return radius > 0 ? Physics.SphereCast(ray, radius, out hit, distance, mask, QueryTriggerInteraction.Ignore)
                    : Physics.Raycast(ray, out hit, distance, mask, QueryTriggerInteraction.Ignore);
            int queryMask = owner == null ? mask : mask | PlayerLayers;
            var triggerQuery = includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            int count = radius > 0 ? Physics.SphereCastNonAlloc(ray, radius, _hits, distance, queryMask, triggerQuery)
                : Physics.RaycastNonAlloc(ray, _hits, distance, queryMask, triggerQuery);
            var hits = _hits;
            if (count == _hits.Length)
            {
                hits = radius > 0 ? Physics.SphereCastAll(ray, radius, distance, queryMask, triggerQuery)
                    : Physics.RaycastAll(ray, distance, queryMask, triggerQuery);
                count = hits.Length;
            }
            hit = default;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var candidate = hits[i];
                if (owner != null && candidate.collider.transform.IsChildOf(owner)) continue;
                if ((mask & (1 << candidate.collider.gameObject.layer)) == 0 &&
                    candidate.collider.GetComponentInParent<Herbalist.Player.PlayerController>() == null) continue;
                if (candidate.collider.isTrigger &&
                    (!candidate.collider.TryGetComponent<StageOneLeafZone>(out var zone) || !zone.isActiveAndEnabled)) continue;
                if (candidate.distance >= nearest) continue;
                nearest = candidate.distance;
                hit = candidate;
            }
            return nearest < float.PositiveInfinity;
        }
    }
}
