using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Player
{
    public struct MotorState { public Vector3 Position; public Vector3 Velocity; public bool Grounded; }
    /// <summary>Only the backend moves the player root. Abilities depend on this contract.</summary>
    public abstract class PlayerMotor : MonoBehaviour
    {
        private readonly HashSet<object> movementLocks = new HashSet<object>();
        public bool MovementLocked => movementLocks.Count > 0;
        public bool HasMovementLockExcept(object owner) { foreach (var item in movementLocks) if (!ReferenceEquals(item, owner)) return true; return false; }
        public abstract bool IsGrounded { get; }
        public abstract Vector3 Velocity { get; }
        // Owner tokens prevent one ability from releasing another ability's lock.
        public void SetMovementLock(object owner, bool locked)
        {
            if (owner == null) throw new System.ArgumentNullException(nameof(owner));
            if (locked) movementLocks.Add(owner); else movementLocks.Remove(owner);
        }
        public abstract void Simulate(Vector3 desiredWorldVelocity, float deltaTime);
        public abstract void Teleport(Vector3 position);
        public abstract void ResetMotion();
        public abstract bool TryJump();
        public abstract MotorState CaptureState();
        public abstract void RestoreState(MotorState state);
    }
}
