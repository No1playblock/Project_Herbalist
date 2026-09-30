using System.Collections.Generic;
using UnityEngine;

namespace Herbalist.Levels
{
    // Forward points into the stage. Passage is evaluated in the motor tick, including Fusion resimulation.
    public sealed class StageEntranceGate : MonoBehaviour
    {
        [SerializeField, Range(0, 31)] private int _gateIndex;
        [SerializeField] private BoxCollider _entryVolume;
        [SerializeField] private BoxCollider _returnBarrier;
        private static readonly HashSet<StageEntranceGate> Gates = new HashSet<StageEntranceGate>();
        private readonly HashSet<CharacterController> _ignored = new HashSet<CharacterController>();
        public BoxCollider EntryVolume => _entryVolume;
        public BoxCollider ReturnBarrier => _returnBarrier;
        public uint Mask => 1u << _gateIndex;
        private void OnEnable() => Gates.Add(this);
        public static void PrepareMove(CharacterController player, uint passed)
        {
            foreach (var gate in Gates)
            {
                if (gate._returnBarrier == null) continue;
                bool allowEntry = (passed & gate.Mask) == 0;
                Physics.IgnoreCollision(player, gate._returnBarrier, allowEntry);
                if (allowEntry) gate._ignored.Add(player); else gate._ignored.Remove(player);
            }
        }
        public uint EvaluatePassage(Vector3 previousCenter, Vector3 currentCenter, float radius, uint passed)
        {
            if ((passed & Mask) != 0 || _entryVolume == null || _returnBarrier == null) return passed;
            var frame = _entryVolume.transform;
            Vector3 from = frame.InverseTransformPoint(previousCenter) - _entryVolume.center;
            Vector3 to = frame.InverseTransformPoint(currentCenter) - _entryVolume.center;
            Vector3 half = _entryVolume.size * .5f;
            var barrier = _returnBarrier;
            Vector3 barrierFront = barrier.transform.TransformPoint(barrier.center + Vector3.forward * barrier.size.z * .5f);
            float clearance = frame.InverseTransformPoint(barrierFront).z - _entryVolume.center.z
                + radius / Mathf.Abs(frame.lossyScale.z);
            if (to.z < clearance || to.z <= from.z) return passed;
            float t = from.z < clearance ? Mathf.Clamp01((clearance - from.z) / (to.z - from.z)) : 1f;
            Vector3 crossing = Vector3.Lerp(from, to, t);
            if (Mathf.Abs(crossing.x) <= half.x && Mathf.Abs(crossing.y) <= half.y && Mathf.Abs(crossing.z) <= half.z)
                return passed | Mask;
            return passed;
        }
        public static uint FinishMove(CharacterController player, Vector3 previousCenter, uint passed)
        {
            Vector3 center = player.transform.TransformPoint(player.center);
            float radius = player.radius * Mathf.Max(Mathf.Abs(player.transform.lossyScale.x), Mathf.Abs(player.transform.lossyScale.z)) + player.skinWidth;
            foreach (var gate in Gates) passed = gate.EvaluatePassage(previousCenter, center, radius, passed);
            return passed;
        }
        public static void Release(CharacterController player)
        {
            if (player == null) return;
            foreach (var gate in Gates)
                if (gate._ignored.Remove(player) && gate._returnBarrier != null)
                    Physics.IgnoreCollision(player, gate._returnBarrier, false);
        }
        private void OnDisable()
        {
            Gates.Remove(this);
            foreach (var player in _ignored)
                if (player != null && _returnBarrier != null) Physics.IgnoreCollision(player, _returnBarrier, false);
            _ignored.Clear();
        }
        private void OnDrawGizmosSelected()
        {
            if (_entryVolume == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.matrix = _entryVolume.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_entryVolume.center, _entryVolume.size);
            Gizmos.DrawLine(_entryVolume.center, _entryVolume.center + Vector3.forward * _entryVolume.size.z);
        }
    }
}
