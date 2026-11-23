using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public sealed class CopyCodeIconFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Range(1f, 1.3f)] private float _hoverScale = 1.14f;
        [SerializeField, Range(1f, 1.3f)] private float _clickScale = 1.2f;
        [SerializeField, Min(0.01f)] private float _hoverSpeed = 18f;
        [SerializeField, Min(0.05f)] private float _clickDuration = 0.22f;
        [SerializeField, Range(0.1f, 0.6f)] private float _clickGrowFraction = 0.35f;

        private RectTransform _rect;
        private Vector3 _restScale;
        private bool _pointerInside;
        private float _hoverMultiplier = 1f;
        private float _clickMultiplier = 1f;
        private Coroutine _clickPulse;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            GetComponent<Image>().raycastTarget = true;
            _restScale = _rect.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData) => _pointerInside = true;
        public void OnPointerExit(PointerEventData eventData) => _pointerInside = false;

        private void Update()
        {
            float target = _pointerInside ? _hoverScale : 1f;
            float blend = 1f - Mathf.Exp(-_hoverSpeed * Time.unscaledDeltaTime);
            _hoverMultiplier = Mathf.Lerp(_hoverMultiplier, target, blend);
            if (Mathf.Abs(_hoverMultiplier - target) < 0.001f)
                _hoverMultiplier = target;

            ApplyScale();
        }

        public void PlayClickPulse()
        {
            if (!isActiveAndEnabled)
                return;

            if (_clickPulse != null)
                StopCoroutine(_clickPulse);
            _clickPulse = StartCoroutine(ClickPulse());
        }

        private IEnumerator ClickPulse()
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, _clickDuration);
            float growFraction = Mathf.Clamp(_clickGrowFraction, 0.1f, 0.6f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                if (progress < growFraction)
                {
                    float grow = Mathf.SmoothStep(0f, 1f, progress / growFraction);
                    _clickMultiplier = Mathf.Lerp(1f, _clickScale, grow);
                }
                else
                {
                    float shrink = Mathf.SmoothStep(0f, 1f,
                        (progress - growFraction) / (1f - growFraction));
                    _clickMultiplier = Mathf.Lerp(_clickScale, 1f, shrink);
                }

                ApplyScale();
                yield return null;
            }

            _clickMultiplier = 1f;
            _clickPulse = null;
            ApplyScale();
        }

        public void ResetVisual()
        {
            if (_clickPulse != null)
                StopCoroutine(_clickPulse);
            _clickPulse = null;
            _pointerInside = false;
            _hoverMultiplier = 1f;
            _clickMultiplier = 1f;
            ApplyScale();
        }

        private void ApplyScale()
        {
            if (_rect != null)
                _rect.localScale = _restScale * (_hoverMultiplier * _clickMultiplier);
        }

        private void OnDisable() => ResetVisual();
    }
}
