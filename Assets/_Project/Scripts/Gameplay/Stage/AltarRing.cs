using Fusion;
using UnityEngine;
using Herbalist.Abilities;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class AltarRing : NetworkBehaviour
    {
        [SerializeField] private LeafInstallTarget _leafTarget;
        [SerializeField] private Transform _stone;
        [SerializeField] private Transform _stairDestination;
        [SerializeField] private float _initialAngle;
        [SerializeField] private float _degreesPerSecond = 25;
        [SerializeField] private float _targetAngle;
        [SerializeField, Range(.1f, 20)] private float _tolerance = 5;
        [Networked] private float Angle { get; set; }
        [Networked] private float StairProgress { get; set; }
        private float _angle, _stairProgress;
        private Vector3 _rest;
        private Quaternion _rotation;
        private bool Online => Object != null && Object.IsValid;
        public bool Aligned => _leafTarget.InstalledLeaf != null && Mathf.Abs(Mathf.DeltaAngle(Online ? Angle : _angle, _targetAngle)) <= _tolerance;
        public bool Frozen => _leafTarget.InstalledLeaf != null;
        public float CurrentAngle => Online ? Angle : _angle;
        private void Awake() { _rest = _stone.position; _rotation = _stone.rotation; _angle = _initialAngle; }
        public override void Spawned() { if (HasStateAuthority) Angle = _initialAngle; }
        public void Tick(bool rotating, float stairProgress, float dt)
        {
            if (Online && !HasStateAuthority) return;
            if (rotating && !Frozen) _angle = Mathf.Repeat(_angle + _degreesPerSecond * dt, 360);
            _stairProgress = stairProgress;
            if (Online) { Angle = _angle; StairProgress = _stairProgress; }
            Present();
        }
        private void Present()
        {
            float progress = Online ? StairProgress : _stairProgress;
            _stone.position = Vector3.Lerp(_rest, _stairDestination.position, Mathf.SmoothStep(0, 1, progress));
            _stone.rotation = _rotation * Quaternion.Euler(0, Online ? Angle : _angle, 0);
        }
        private void Update() => Present();
    }
}
