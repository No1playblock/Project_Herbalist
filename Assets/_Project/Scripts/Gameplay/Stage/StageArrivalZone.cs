using System.Collections.Generic;
using UnityEngine;
using Herbalist.StageOne;

namespace Herbalist.Levels
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class StageArrivalZone : MonoBehaviour
    {
        [SerializeField] private BoxCollider _volume;
        [SerializeField] private Vector3 _playerProbeOffset = Vector3.up;
        [SerializeField, Min(1)] private int _requiredPlayers = 2;
        private readonly HashSet<int> _slots = new();
        public int RequiredPlayers => _requiredPlayers;
        public Vector3 PlayerArrivalPoint => _volume.transform.TransformPoint(_volume.center) - _playerProbeOffset;
        public bool Contains(Vector3 position)
        {
            if (_volume == null || !_volume.enabled || !_volume.gameObject.activeInHierarchy) return false;
            var p = _volume.transform.InverseTransformPoint(position) - _volume.center;
            var half = _volume.size * .5f;
            return Mathf.Abs(p.x) <= half.x && Mathf.Abs(p.y) <= half.y && Mathf.Abs(p.z) <= half.z;
        }
        public int CountPresent()
        {
            _slots.Clear();
            foreach (var actor in StageActor.All)
                if (actor.Available && actor.Slot >= 0 && Contains(actor.transform.position + _playerProbeOffset))
                    _slots.Add(actor.Slot);
            return _slots.Count;
        }
        private void Reset() { _volume = GetComponent<BoxCollider>(); _volume.isTrigger = true; }
    }
}
