using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Herbalist.GameUI
{
    public sealed class StageCinematicView : MonoBehaviour
    {
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private Camera _backgroundCamera;
        [SerializeField] private VideoClip _duyeongSapClip, _sodamSapClip, _openingClip;
        [SerializeField] private RawImage _video;
        [SerializeField] private AspectRatioFitter _aspect;
        [SerializeField] private GameObject _skipPrompt;
        [SerializeField] private TMP_Text _skipLabel, _waitingLabel;
        [SerializeField] private Image _holdFill;
        [SerializeField] private InputActionReference _skipAction;
        [SerializeField, Min(0.1f)] private float _skipHoldSeconds = 2;
        [SerializeField, Min(1)] private float _prepareTimeoutSeconds = 15;
        [SerializeField, Min(1)] private float _playbackGraceSeconds = 20;

        private InputAction _input;
        private Action _onPrepared, _onFinished;
        private Action<bool> _onHoldChanged;
        private double _prepareDeadline, _playbackDeadline;
        private double? _holdStart;
        private bool _preparing, _playing, _finished, _videoFailed, _needsRelease, _voted, _loading;

        internal float SkipHoldSeconds => _skipHoldSeconds;
        internal double PlaybackLimitSeconds => (_videoPlayer.clip != null ? _videoPlayer.clip.length : 0d) + _playbackGraceSeconds;

        internal void Prepare(bool opening, int sapSlot, Action prepared, Action<bool> holdChanged, Action finished)
        {
            _onPrepared = prepared;
            _onHoldChanged = holdChanged;
            _onFinished = finished;
            _videoPlayer.prepareCompleted += VideoPrepared;
            _videoPlayer.errorReceived += VideoError;
            _videoPlayer.loopPointReached += VideoEnded;
            _videoPlayer.clip = opening ? _openingClip : sapSlot == 0 ? _duyeongSapClip : _sodamSapClip;
            _preparing = true;
            _prepareDeadline = Time.realtimeSinceStartupAsDouble + _prepareTimeoutSeconds;
            if (_videoPlayer.clip != null) _videoPlayer.Prepare();
            else VideoError(_videoPlayer, "Missing cinematic clip.");
        }

        internal void Play()
        {
            _playing = true;
            _playbackDeadline = Time.realtimeSinceStartupAsDouble + PlaybackLimitSeconds;
            if (_skipAction != null && _skipAction.action != null)
            {
                _input = _skipAction.action.Clone();
                _input.Enable();
                _needsRelease = true;
                _skipLabel.text = $"<b>{_input.GetBindingDisplayString()}</b> 꾹 누르기   SKIP";
                _skipPrompt.SetActive(true);
            }
            else Debug.LogError("Stage cinematic is missing its skip action.", this);
            if (_videoFailed) { FinishVideo(); return; }
            if (_videoPlayer.audioTrackCount > 0) _videoPlayer.SetDirectAudioVolume(0, UserOptions.Volume);
            _videoPlayer.Play();
        }

        private void Update()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (_preparing && now >= _prepareDeadline) VideoError(_videoPlayer, "Preparation timed out.");
            if (!_playing) return;

            if (!_finished && !_loading)
            {
                if (_videoPlayer.texture != null && _video.texture != _videoPlayer.texture)
                {
                    _video.texture = _videoPlayer.texture;
                    _video.enabled = true;
                }
                if (now >= _playbackDeadline) VideoError(_videoPlayer, "Playback timed out.");
            }

            if (_input == null || _voted || _loading) return;
            bool pressed = _input.IsPressed();
            if (!pressed) _needsRelease = false;
            bool held = Application.isFocused && pressed && !_needsRelease;
            if (!Application.isFocused) _needsRelease = true;
            if (held != _holdStart.HasValue)
            {
                _holdStart = held ? now : null;
                _onHoldChanged?.Invoke(held);
            }
            float progress = _holdStart is double since ? Mathf.Clamp01((float)(now - since) / _skipHoldSeconds) : 0f;
            _holdFill.fillAmount = progress;
            if (progress >= 1f)
            {
                _voted = true;
                _skipLabel.text = "스킵 동의 완료";
#if UNITY_EDITOR
                if (_onHoldChanged == null) FinishVideo();
#endif
            }
        }

        internal void ShowVotes(int votes, int total)
        {
            _waitingLabel.text = $"스킵 동의 {votes}/{total}";
            _waitingLabel.gameObject.SetActive(true);
            if (total > 0 && votes == total) ShowLoading();
        }

        internal void ShowLoading()
        {
            if (_loading) return;
            _loading = true;
            if (!_finished) _videoPlayer.Pause();
            _skipPrompt.SetActive(true);
            _skipLabel.text = "다음 스테이지 로딩 중…";
            _holdFill.fillAmount = 0;
        }

        internal void Hide()
        {
            Stop();
            if (_backgroundCamera != null) _backgroundCamera.enabled = false;
            gameObject.SetActive(false);
        }

        private void VideoPrepared(VideoPlayer player)
        {
            if (!_preparing) return;
            if (player.height > 0) _aspect.aspectRatio = (float)player.width / player.height;
            _preparing = false;
            _onPrepared?.Invoke();
        }

        private void VideoError(VideoPlayer player, string message)
        {
            if (_videoFailed || _finished) return;
            Debug.LogWarning($"Stage cinematic: {message}", this);
            _videoFailed = true;
            player.Stop();
            _video.enabled = false;
            if (_preparing)
            {
                _preparing = false;
                _onPrepared?.Invoke();
            }
            if (_playing) FinishVideo();
        }

        private void VideoEnded(VideoPlayer player) => FinishVideo();

        private void FinishVideo()
        {
            if (_finished) return;
            _finished = true;
            ShowLoading();
            _onFinished?.Invoke();
        }

        internal void Stop()
        {
            _preparing = _playing = false;
            if (_videoPlayer != null)
            {
                _videoPlayer.prepareCompleted -= VideoPrepared;
                _videoPlayer.errorReceived -= VideoError;
                _videoPlayer.loopPointReached -= VideoEnded;
                _videoPlayer.Stop();
            }
            _input?.Dispose();
            _input = null;
        }

        private void OnDestroy() => Stop();
    }
}
