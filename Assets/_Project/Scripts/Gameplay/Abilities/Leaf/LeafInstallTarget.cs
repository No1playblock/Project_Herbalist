using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
namespace Herbalist.Abilities
{
    // Authored targets opt into installation. Decorative geometry never accepts leaves.
    public sealed class LeafInstallTarget : MonoBehaviour
    {
        [SerializeField] private int targetId;
        [SerializeField] private Transform socket;
        [SerializeField] private bool _singleOccupant;
        [FormerlySerializedAs("onPinned"), SerializeField] private UnityEvent onInstalled = new UnityEvent();
        [SerializeField] private UnityEvent onReleased = new UnityEvent();
        private readonly HashSet<LeafProjectile> occupants = new();
        private static readonly Dictionary<int, LeafInstallTarget> targets = new();
        public int Id => targetId;
        public bool HasSocket => socket != null;
        public Vector3 SocketPosition => socket != null ? socket.position : transform.position;
        public Quaternion SocketRotation => socket != null ? socket.rotation : transform.rotation;
        public bool ExactSocketPlacement => socket != null;
        public LeafProjectile InstalledLeaf { get { foreach(var leaf in occupants) if(leaf != null && leaf.Installed) return leaf; return null; } }
        public bool HasLeaf => InstalledLeaf != null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() => targets.Clear();
        private void OnEnable()
        {
            if (targetId <= 0 || (targets.TryGetValue(targetId, out var other) && other != this)) { Debug.LogError("Leaf targets require unique positive IDs.", this); return; }
            targets[targetId] = this;
        }
        private void OnDisable() { if (targets.TryGetValue(targetId, out var current) && current == this) targets.Remove(targetId); }
        public static LeafInstallTarget Find(int id) => targets.TryGetValue(id, out var target) ? target : null;
        public bool CanInstall => isActiveAndEnabled && (!_singleOccupant || !HasLeaf);
        public Vector3 Position(Vector3 hit, Vector3 normal, float offset) => socket != null ? socket.position : hit + normal * offset;
        public Quaternion Rotation(Vector3 normal, Vector3 heading)
        {
            if (socket != null) return socket.rotation;
            Vector3 forward = Vector3.ProjectOnPlane(heading, Vector3.up);
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            Vector3 up = Mathf.Abs(Vector3.Dot(forward.normalized, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
            return Quaternion.LookRotation(forward, up);
        }
        public void Attach(LeafProjectile leaf)
        {
            bool before = HasLeaf; occupants.Add(leaf);
            if (!before && HasLeaf) onInstalled.Invoke();
        }
        public void Detach(LeafProjectile leaf)
        {
            bool before = HasLeaf; occupants.Remove(leaf);
            if (before && !HasLeaf) onReleased.Invoke();
        }
    }
}
