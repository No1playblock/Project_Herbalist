using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Herbalist.Abilities
{
    // Authored targets opt into installation. Decorative geometry never accepts leaves.
    public sealed class LeafInstallTarget : MonoBehaviour
    {
        [SerializeField] private int targetId;
        [SerializeField] private bool acceptsPlatform = true;
        [SerializeField] private bool acceptsPin;
        [SerializeField] private Transform socket;
        [SerializeField] private SapBindingSource sap;
        [SerializeField] private UnityEvent onPinned = new UnityEvent();
        [SerializeField] private UnityEvent onReleased = new UnityEvent();
        private readonly HashSet<LeafProjectile> occupants = new();
        private static readonly Dictionary<int, LeafInstallTarget> targets = new();
        public int Id => targetId;
        public bool Pinned { get { foreach (var leaf in occupants) if (leaf != null && leaf.Mode == LeafMode.Pin) return true; return false; } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() => targets.Clear();
        private void OnEnable()
        {
            if (targetId <= 0 || (targets.TryGetValue(targetId, out var other) && other != this)) { Debug.LogError("Leaf targets require unique positive IDs.", this); return; }
            targets[targetId] = this;
        }
        private void OnDisable() { if (targets.TryGetValue(targetId, out var current) && current == this) targets.Remove(targetId); }
        public static LeafInstallTarget Find(int id) => targets.TryGetValue(id, out var target) ? target : null;
        public bool Accepts(LeafMode mode) => isActiveAndEnabled && (mode == LeafMode.Platform ? acceptsPlatform : mode == LeafMode.Pin && acceptsPin);
        public Vector3 Position(Vector3 hit, Vector3 normal, float offset) => socket != null ? socket.position : hit + normal * offset;
        public Quaternion Rotation(LeafMode mode, Vector3 normal, Vector3 heading)
        {
            if (socket != null && mode != LeafMode.Pin) return socket.rotation;
            Vector3 forward = Vector3.ProjectOnPlane(socket != null ? socket.forward : (mode == LeafMode.Pin ? -normal : heading), Vector3.up);
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            Vector3 up = Mathf.Abs(Vector3.Dot(forward.normalized, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
            return Quaternion.LookRotation(forward, up);
        }
        public bool TryBind(LeafProjectile leaf) => SapDeposit.TryBind(leaf) || (GetComponent<SapReceiver>() == null && sap != null && sap.TryBind(leaf));
        public void Attach(LeafProjectile leaf, bool bound)
        {
            bool before = Pinned; occupants.Add(leaf);
            if (bound && GetComponent<SapReceiver>() == null && sap != null) sap.ShowBound(leaf);
            if (!before && Pinned) onPinned.Invoke();
        }
        public void Detach(LeafProjectile leaf)
        {
            bool before = Pinned; occupants.Remove(leaf);
            SapDeposit.Release(leaf);
            if (sap != null) sap.Release(leaf);
            if (before && !Pinned) onReleased.Invoke();
        }
    }
}
