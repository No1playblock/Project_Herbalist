using UnityEngine;

namespace Herbalist.Levels
{
    // Authored outlet geometry, using the same flowing blue stream as the jump pads.
    [RequireComponent(typeof(MushroomWaterJetPresentation))]
    public sealed class StageOneFixedWaterStream : MonoBehaviour
    {
        [SerializeField] private SapJumpPadSettings _presentationSettings;
        [SerializeField] private Transform _source;
        [SerializeField] private Transform _end;
        private MushroomWaterJetPresentation _presentation;

        private void Awake()
        {
            _presentation = GetComponent<MushroomWaterJetPresentation>();
            _presentation.Initialize(_presentationSettings);
        }
        private void LateUpdate()
        {
            if (_source != null && _end != null) _presentation.Present(_source.position, _end.position);
        }
    }
}
