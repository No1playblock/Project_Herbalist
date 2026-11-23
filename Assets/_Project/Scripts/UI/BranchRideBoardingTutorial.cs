using System;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Presentation;
using Herbalist.StageOne;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Herbalist.GameUI
{
    [DisallowMultipleComponent]
    public sealed class BranchRideBoardingTutorial : MonoBehaviour
    {
        [Serializable]
        private struct Guide
        {
            public VideoClip clip;
            public Texture2D poster;
            public string title;
            public string instruction;
        }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _instruction;
        [SerializeField] private RawImage _clipImage;
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private Button _closeButton;
        [SerializeField, Min(0)] private float _closeDelay = .2f;
        [SerializeField] private Guide _sapGuide;
        [SerializeField] private Guide _leafGuide;

        private PlayerHud _ownerHud;
        private PlayScreenController _screen;
        private GameOverlayHud _overlay;
        private bool _hasShown;
        private double? _closeAt;

        private void Awake()
        {
            _ownerHud = GetComponentInParent<PlayerHud>();
            _screen = FindAnyObjectByType<PlayScreenController>();
            _overlay = FindAnyObjectByType<GameOverlayHud>();
            _panel.SetActive(false);
            _videoPlayer.isLooping = true;
            _closeButton.onClick.AddListener(Dismiss);
        }

        private void Update()
        {
            if (_hasShown && !_panel.activeSelf) return;

            var view = _ownerHud != null && _screen != null ? _screen.ViewForSlot(_ownerHud.slot) : null;
            var actor = view != null ? view.GetComponent<StageActor>() : null;
            if (actor == null || !actor.Local || actor.Abilities == null || !actor.Abilities.Unlocked ||
                GameplayPause.IsPaused ||
                (_overlay != null && (_overlay.StageHeaderPlaying ||
                    (_overlay.options != null && _overlay.options.activeInHierarchy))) ||
                !BranchRideFlow.TryBoardHint(actor, out _))
            {
                Hide();
                return;
            }

            if (_closeAt is double closeAt && Time.unscaledTimeAsDouble >= closeAt)
            {
                Hide();
                return;
            }

            if (!_hasShown)
            {
                switch (actor.Abilities.Kind)
                {
                    case PlayerAbilityKind.Sap:
                        Show(_sapGuide);
                        break;
                    case PlayerAbilityKind.Leaf:
                        Show(_leafGuide);
                        break;
                }
            }
            else if (_videoPlayer.frame >= 0 && _videoPlayer.texture != null &&
                _clipImage.texture != _videoPlayer.texture)
            {
                _clipImage.texture = _videoPlayer.texture;
            }
        }

        private void Show(Guide guide)
        {
            _hasShown = true;
            _title.text = guide.title;
            _instruction.text = guide.instruction;
            _clipImage.texture = guide.poster;
            _panel.SetActive(true);
            GameplayCursor.Instance?.SetUiMode(this, true);
            _videoPlayer.clip = guide.clip;
            if (guide.clip != null) _videoPlayer.Play();
        }

        private void Dismiss()
        {
            _closeAt ??= Time.unscaledTimeAsDouble + _closeDelay;
        }

        private void Hide()
        {
            if (_panel == null || !_panel.activeSelf) return;
            _videoPlayer.Stop();
            _videoPlayer.clip = null;
            _clipImage.texture = null;
            _panel.SetActive(false);
            GameplayCursor.Instance?.SetUiMode(this, false);
            GameplayCursor.Instance?.Capture();
        }

        private void OnDisable() => Hide();
        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Dismiss);
        }
    }
}
