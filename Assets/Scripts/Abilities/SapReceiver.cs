using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Herbalist.Abilities
{
    public sealed class SapReceiver : MonoBehaviour
    {
        [SerializeField, Min(1)] private int receiverId;
        [SerializeField] private LeafInstallTarget leafTarget;
        [Tooltip("Optional device inlet. Empty means free placement on this collider surface.")]
        [SerializeField] private Transform inlet;
        [SerializeField, Min(0.01f)] private float inletRadius = 0.6f;
        [SerializeField] private UnityEvent onSupplied = new();
        [SerializeField] private UnityEvent onDrained = new();
        private readonly HashSet<SapDeposit> deposits = new();
        private static readonly Dictionary<int, SapReceiver> receivers = new();
        public int Id => receiverId;
        public LeafInstallTarget LeafTarget => leafTarget;
        public bool Supplied => deposits.Count > 0;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() => receivers.Clear();
        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            if (receiverId <= 0 || (receivers.TryGetValue(receiverId, out var other) && other != this))
            { Debug.LogError("Sap receivers require unique positive IDs.", this); return; }
            receivers[receiverId] = this;
        }
        private void OnDisable() { if (receivers.TryGetValue(receiverId, out var current) && current == this) receivers.Remove(receiverId); }
        public static SapReceiver Find(int id) => receivers.TryGetValue(id, out var receiver) ? receiver : null;
        public bool TryPlacement(RaycastHit hit, float offset, out Vector3 position, out Vector3 normal)
        {
            position = inlet != null ? inlet.position : hit.point + hit.normal * offset;
            normal = inlet != null ? inlet.forward : hit.normal;
            return isActiveAndEnabled && (inlet == null || Vector3.Distance(hit.point, inlet.position) <= inletRadius);
        }
        public void Attach(SapDeposit deposit)
        { bool before = Supplied; deposits.Add(deposit); if (!before && Supplied) onSupplied.Invoke(); }
        public void Detach(SapDeposit deposit)
        { bool before = Supplied; deposits.Remove(deposit); if (before && !Supplied) onDrained.Invoke(); }
    }
}
