using Herbalist.Presentation;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public sealed class BrewingPotRattle : MonoBehaviour
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField, Min(1f)] private float _framesPerSecond = 14f;
        [SerializeField, Min(.1f)] private float _rattleSeconds = .55f;
        private UnityEngine.UI.Image _image;
        private StageActor _actor;
        private int _lastStep;
        private bool _crafting;
        private float _remaining;
        private float _elapsed;

        private void Awake() => _image = GetComponent<UnityEngine.UI.Image>();

        public void Present(StageOneFlow flow, StageActor viewer, bool crafting)
        {
            var crafter = crafting && viewer != null && viewer.Local &&
                flow.RoleFor(viewer) == CharacterRole.Sodam ? viewer : null;
            if (_actor != crafter)
            {
                if (_actor != null) _actor.LocalCommandRequested -= OnCommand;
                _actor = crafter;
                if (_actor != null) _actor.LocalCommandRequested += OnCommand;
            }

            int step = crafting ? flow.MinigameSteps : 0;
            if (crafting && _crafting && _actor == null && step != _lastStep) Rattle();
            _lastStep = step;
            _crafting = crafting;
            if (!crafting) ResetPose();
        }

        private void OnCommand(StageCommand command)
        {
            if (_crafting && command == StageCommand.Craft) Rattle();
        }

        private void Rattle()
        {
            _remaining = _rattleSeconds;
            _elapsed = 0f;
        }

        private void LateUpdate()
        {
            if (_frames == null || _frames.Length == 0 || GameplayPause.IsPaused) return;
            if (_remaining > 0f && _crafting)
            {
                _elapsed += Time.deltaTime;
                _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);
                _image.sprite = _frames[Mathf.FloorToInt(_elapsed * _framesPerSecond) % _frames.Length];
            }
            else _image.sprite = _frames[0];
        }

        private void ResetPose()
        {
            _remaining = _elapsed = 0f;
            if (_image != null && _frames != null && _frames.Length > 0) _image.sprite = _frames[0];
        }

        private void OnDisable()
        {
            if (_actor != null) _actor.LocalCommandRequested -= OnCommand;
            _actor = null;
            _crafting = false;
            _lastStep = 0;
            ResetPose();
        }
    }
}
