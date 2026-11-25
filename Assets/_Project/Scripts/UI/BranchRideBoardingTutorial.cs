using System;
using System.Linq;
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
        private enum Trigger
        {
            BranchRideBoarding,
            FirstSapMaze,
            FirstStageOnePad
        }

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
        [SerializeField] private int _dismissSoundId = 1;
        [SerializeField] private Trigger _trigger;
        [SerializeField] private DescendingMazeFlow _mazeFlow;
        [SerializeField] private StageNarrativePresenter _stageOneNarrative;
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
        }

        private void Update()
        {
            if (_hasShown && !_panel.activeSelf) return;

            var actor = FindLocalActor();
            if (actor == null || !actor.Local || actor.Abilities == null || !actor.Abilities.Unlocked ||
                GameplayPause.IsPaused ||
                (_overlay != null && (_overlay.StageHeaderPlaying ||
                    (_overlay.options != null && _overlay.options.activeInHierarchy))) ||
                !CanShowAtCurrentStage(actor))
            {
                Hide();
                return;
            }

            if (_closeAt is double closeAt && Time.unscaledTimeAsDouble >= closeAt)
            {
                Hide();
                return;
            }

            if (_trigger == Trigger.FirstStageOnePad)
                FitLocalViewport(actor);

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

        private StageActor FindLocalActor()
        {
            if (_trigger == Trigger.FirstStageOnePad)
                return StageActor.All.FirstOrDefault(actor => actor.Local);

            var view = _ownerHud != null && _screen != null ? _screen.ViewForSlot(_ownerHud.slot) : null;
            return view != null ? view.GetComponent<StageActor>() : null;
        }

        private void FitLocalViewport(StageActor actor)
        {
            if (transform is not RectTransform root || actor.Camera == null) return;

            var viewport = actor.Camera.rect;
            if (root.anchorMin == viewport.min && root.anchorMax == viewport.max) return;

            root.anchorMin = viewport.min;
            root.anchorMax = viewport.max;
            root.offsetMin = root.offsetMax = Vector2.zero;
        }

        private bool CanShowAtCurrentStage(StageActor actor)
        {
            switch (_trigger)
            {
                case Trigger.FirstStageOnePad:
                    return _stageOneNarrative != null && _stageOneNarrative.IsAtFirstPad(actor) &&
                        (StageOneFlow.Instance?.Progress?.GateOpen == true || actor.Abilities.OfflineBothUnlocked);
                case Trigger.FirstSapMaze:
                    return _mazeFlow != null && _mazeFlow.CurrentMaze == 0 &&
                        _mazeFlow.CurrentBoard is SapMazeBoard board && board.Active &&
                        board.State.Phase == MazePhase.Running;
                default:
                    return BranchRideFlow.TryBoardHint(actor, out _);
            }
        }

        private void Show(Guide guide)
        {
            _hasShown = true;
            _title.text = guide.title;
            _instruction.text = guide.instruction;
            _clipImage.texture = guide.poster;
            _panel.SetActive(true);
            _closeButton.onClick.AddListener(Dismiss);
            GameplayCursor.Instance?.SetUiMode(this, true);
            _videoPlayer.clip = guide.clip;
            if (guide.clip != null) _videoPlayer.Play();
        }

        private void Dismiss()
        {
            if (_closeAt.HasValue) return;
            if (_closeButton.GetComponent<Herbalist.Audio.SfxUiFeedback>() == null)
                Herbalist.Audio.SfxEmitter.ScenePool?.Play(_dismissSoundId);
            _closeAt = Time.unscaledTimeAsDouble + _closeDelay;
        }

        private void Hide()
        {
            if (_panel == null || !_panel.activeSelf) return;
            _videoPlayer.Stop();
            _videoPlayer.clip = null;
            _clipImage.texture = null;
            _closeButton.onClick.RemoveListener(Dismiss);
            _panel.SetActive(false);
            GameplayCursor.Instance?.SetUiMode(this, false);
            GameplayCursor.Instance?.Capture();
        }

        private void OnDisable() => Hide();
    }
}
