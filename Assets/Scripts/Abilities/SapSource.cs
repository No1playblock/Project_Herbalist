using System.Collections.Generic;
using UnityEngine;
namespace Herbalist.Abilities
{
    // Supply and receiving surfaces are independent: one source may feed several devices.
    public sealed class SapSource : MonoBehaviour
    {
        [SerializeField] private Transform extractionPoint;
        [SerializeField] private Collider interactionVolume;
        [Tooltip("Zero means unlimited prototype supply.")]
        [SerializeField, Min(0)] private int supplyUnits;
        private int used;
        private static readonly HashSet<SapSource> sources = new();
        public Vector3 ExtractionPoint => extractionPoint != null ? extractionPoint.position : transform.position;
        public bool Available => isActiveAndEnabled && (supplyUnits == 0 || used < supplyUnits);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() => sources.Clear();
        private void OnEnable() => sources.Add(this);
        private void OnDisable() => sources.Remove(this);
        public bool TryExtract() { if (!Available) return false; used++; return true; }
        public void Refund() => used = Mathf.Max(0, used - 1);
        public static SapSource FindNearest(Vector3 position, float range)
        {
            SapSource best = null; float distance = range * range;
            foreach (var source in sources)
            {
                if (!source.Available) continue;
                Vector3 point = source.interactionVolume != null ? source.interactionVolume.ClosestPoint(position) : source.ExtractionPoint;
                float candidate = (point - position).sqrMagnitude;
                if (candidate <= distance) { distance = candidate; best = source; }
            }
            return best;
        }
    }
}
