using System;
using Herbalist.Presentation;
using Herbalist.StageOne;
using TMPro;
using UnityEngine;

namespace Herbalist.GameUI
{
    public sealed class StageGatheringView : MonoBehaviour
    {
        [Serializable]
        private struct CodexRow
        {
            public StageItemDefinition herb;
            public UnityEngine.UI.Button button;
            public GameObject selection;
        }

        [SerializeField] private GameObject _duyeongRoot, _codexRoot;
        [SerializeField] private UnityEngine.UI.Image _plant, _codexPortrait;
        [SerializeField] private TMP_Text _progress, _inputHint;
        [SerializeField] private GameObject[] _stepFills;
        [SerializeField] private TMP_Text[] _stepLabels;
        [SerializeField] private CodexRow[] _rows;
        [SerializeField] private TMP_Text _codexName, _codexDescription, _codexMethod;
        [SerializeField] private TMP_Text[] _codexKeys;
        [SerializeField, Min(0f)] private float _completionHoldSeconds = .6f;

        private StageItemDefinition _activeHerb;
        private uint _resultAtStart;
        private int _shownSteps = -1;
        private float _completeUntil;
        private bool _codexOpen;
        private StageOneFlow _duyeongFlow;

        private void Awake()
        {
            for (int i = 0; i < _rows.Length; i++)
            {
                int index = i;
                _rows[i].button.onClick.AddListener(() => SelectEntry(index));
            }
        }

        private StageMinigameUiText _copy;

        public bool Present(StageOneFlow flow, CharacterRole role, StageMinigameUiText copy)
        {
            if (flow == null || copy == null)
            {
                Hide();
                return false;
            }

            _copy = copy;
            bool gathering = flow.Minigame == StageMinigameKind.Gathering;
            SetCodexOpen(gathering && role == CharacterRole.Sodam);
            if (role != CharacterRole.Duyeong || flow.Minigame == StageMinigameKind.Crafting)
            {
                ResetPull();
                return _codexOpen;
            }

            if (gathering)
            {
                var herb = flow.settings.Item(flow.MinigameItemId);
                if (_duyeongFlow != flow || _activeHerb != herb || _resultAtStart != flow.MinigameResultSequence)
                {
                    _duyeongFlow = flow;
                    _activeHerb = herb;
                    _resultAtStart = flow.MinigameResultSequence;
                    _shownSteps = -1;
                }
                _completeUntil = 0;
                ShowPull(flow.MinigameSteps, false);
                return true;
            }

            // The successful tenth input and clearing the minigame can arrive in
            // the same Fusion snapshot. Show the final frame from the result.
            if (_duyeongFlow == flow && flow.MinigameSucceeded && flow.MinigameResultSequence != _resultAtStart)
            {
                _duyeongFlow = null;
                _completeUntil = Time.unscaledTime + _completionHoldSeconds;
            }
            if (Time.unscaledTime < _completeUntil)
            {
                ShowPull(_stepFills.Length, true);
                return true;
            }
            ResetPull();
            return false;
        }

        public void Hide()
        {
            SetCodexOpen(false);
            ResetPull();
        }

        private void ResetPull()
        {
            _duyeongRoot.SetActive(false);
            _duyeongFlow = null;
            _activeHerb = null;
            _shownSteps = -1;
            _completeUntil = 0;
        }

        private void ShowPull(int steps, bool complete)
        {
            _duyeongRoot.SetActive(true);
            steps = Mathf.Clamp(steps, 0, _stepFills.Length);
            if (steps == _shownSteps) return;
            _shownSteps = steps;
            var frames = _activeHerb != null ? _activeHerb.gatheringFrames : null;
            _plant.sprite = steps > 0 && frames != null && steps <= frames.Length
                ? frames[steps - 1]
                : _activeHerb != null ? (_activeHerb.plantedImage != null ? _activeHerb.plantedImage : _activeHerb.codexImage) : null;
            _plant.enabled = _plant.sprite != null;
            _progress.text = string.Format(_copy.gatherProgressFormat, steps);
            _inputHint.text = complete ? _copy.gatherCompleteText : _copy.gatherDuyeongInputHint;
            var directions = _activeHerb != null ? _activeHerb.harvestingSteps : null;
            for (int i = 0; i < _stepFills.Length; i++)
            {
                _stepFills[i].SetActive(i < steps);
                if (i < steps && directions != null && i < directions.Length)
                    _stepLabels[i].text = _copy.DirectionLabel(directions[i]);
            }
        }

        private void SetCodexOpen(bool open)
        {
            if (_codexOpen == open) return;
            _codexOpen = open;
            var cursor = GameplayCursor.Instance;
            cursor?.SetUiMode(this, open);
            _codexRoot.SetActive(open);
            if (open) SelectEntry(0);
            else if (!GameplayPause.IsPaused) cursor?.Capture();
        }

        private void SelectEntry(int index)
        {
            if (_copy == null || GameplayPause.IsPaused) return;
            var herb = _rows[index].herb;
            for (int i = 0; i < _rows.Length; i++) _rows[i].selection.SetActive(i == index);
            _codexPortrait.sprite = herb.codexImage;
            _codexName.text = herb.displayName;
            _codexDescription.text = herb.appearance;
            _codexMethod.text = herb.harvestingMethod;
            for (int i = 0; i < _codexKeys.Length; i++)
                _codexKeys[i].text = i < herb.harvestingSteps.Length
                    ? _copy.DirectionLabel(herb.harvestingSteps[i]) : string.Empty;
        }
    }
}
