using UnityEngine;
namespace Herbalist.Abilities
{
    // Only accepted authoritative hose hits feed this port; ordinary sap marks are unchanged.
    public sealed class SapInjectionPort : MonoBehaviour
    {
        [SerializeField, Min(.01f)] private float _unitsPerSecond = 1;
        [SerializeField, Min(.01f)] private float _placementUnits = 1;
        private float _pending;
        public bool Inject(float seconds)
        {
            if (!isActiveAndEnabled || seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return false;
            _pending += seconds * _unitsPerSecond;
            return true;
        }
        public void InjectPlacement() { if (isActiveAndEnabled) _pending += _placementUnits; }
        public float Consume() { float result = _pending; _pending = 0; return result; }
        private void OnDisable() => _pending = 0;
    }
}
