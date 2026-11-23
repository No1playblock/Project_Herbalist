using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(Image))]
    public sealed class NorigeFrameLoop : MonoBehaviour
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField, Min(0.01f)] private float _frameDuration = 0.08f;

        private Image _image;
        private Coroutine _playback;
        private int _frameIndex;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_image == null || _frames == null || _frames.Length == 0) return;

            _frameIndex = 0;
            _image.sprite = _frames[0];
            _playback = StartCoroutine(PlayFrames());
        }

        private IEnumerator PlayFrames()
        {
            var wait = new WaitForSecondsRealtime(_frameDuration);
            while (_frames != null && _frames.Length > 0)
            {
                yield return wait;
                _frameIndex = (_frameIndex + 1) % _frames.Length;
                _image.sprite = _frames[_frameIndex];
            }
        }

        private void OnDisable()
        {
            if (_playback != null)
            {
                StopCoroutine(_playback);
                _playback = null;
            }

            if (_image != null && _frames != null && _frames.Length > 0)
            {
                _frameIndex = 0;
                _image.sprite = _frames[0];
            }
        }
    }
}