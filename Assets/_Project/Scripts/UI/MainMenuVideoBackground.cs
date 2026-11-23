using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Herbalist.GameUI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    public sealed class MainMenuVideoBackground : MonoBehaviour
    {
        [SerializeField] private VideoClip _videoClip;
        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _fillScreen = true;

        private RawImage _rawImage;
        private VideoPlayer _videoPlayer;
        private Texture _fallbackTexture;
        private Rect _fallbackUvRect;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            if (_rawImage == null)
                return;

            _fallbackTexture = _rawImage.texture;
            _fallbackUvRect = _rawImage.uvRect;
            if (_videoClip == null)
                return;

            _videoPlayer = GetComponent<VideoPlayer>();
            if (_videoPlayer == null)
                _videoPlayer = gameObject.AddComponent<VideoPlayer>();

            _videoPlayer.playOnAwake = false;
            _videoPlayer.source = VideoSource.VideoClip;
            _videoPlayer.clip = _videoClip;
            _videoPlayer.renderMode = VideoRenderMode.APIOnly;
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            _videoPlayer.isLooping = _loop;
            _videoPlayer.waitForFirstFrame = true;
            _videoPlayer.skipOnDrop = true;
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.errorReceived += OnVideoError;
        }

        private void OnEnable()
        {
            if (_videoPlayer == null)
                return;

            if (_videoPlayer.isPrepared)
                _videoPlayer.Play();
            else
                _videoPlayer.Prepare();
        }

        private void Update()
        {
            if (_videoPlayer == null || !_videoPlayer.isPlaying || _rawImage == null)
                return;

            Texture videoTexture = _videoPlayer.texture;
            if (videoTexture == null)
                return;

            if (_rawImage.texture != videoTexture)
                _rawImage.texture = videoTexture;

            if (_fillScreen)
                ApplyFillCrop(videoTexture);
        }

        private void OnVideoPrepared(VideoPlayer player)
        {
            if (player == null || !isActiveAndEnabled)
                return;

            player.Play();
        }

        private void OnVideoError(VideoPlayer player, string message)
        {
            RestoreFallback();
            if (player != null)
                player.Stop();
        }

        private void OnDisable()
        {
            if (_videoPlayer != null)
                _videoPlayer.Pause();

            RestoreFallback();
        }

        private void OnDestroy()
        {
            if (_videoPlayer == null)
                return;

            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.errorReceived -= OnVideoError;
        }

        private void RestoreFallback()
        {
            if (_rawImage == null)
                return;

            _rawImage.texture = _fallbackTexture;
            _rawImage.uvRect = _fallbackUvRect;
        }

        private void ApplyFillCrop(Texture texture)
        {
            Rect rect = _rawImage.rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f || texture.height <= 0)
                return;

            float textureAspect = (float)texture.width / texture.height;
            float panelAspect = rect.width / rect.height;
            Rect uv;

            if (textureAspect > panelAspect)
            {
                float width = panelAspect / textureAspect;
                uv = new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
            else
            {
                float height = textureAspect / panelAspect;
                uv = new Rect(0f, (1f - height) * 0.5f, 1f, height);
            }

            if (_rawImage.uvRect != uv)
                _rawImage.uvRect = uv;
        }
    }
}