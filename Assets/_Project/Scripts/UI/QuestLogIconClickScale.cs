using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public sealed class QuestLogIconClickScale : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField, Range(1f, 1.2f)] private float scaleMultiplier = 1.06f;
        [SerializeField, Min(0.05f)] private float duration = 0.24f;
        [SerializeField, Range(0.1f, 0.6f)] private float growFraction = 0.3f;

        private RectTransform rectTransform;
        private Vector3 baseScale;
        private Coroutine pulse;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            baseScale = rectTransform.localScale;
        }

        private void OnEnable()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
                baseScale = rectTransform.localScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (rectTransform == null)
                return;

            if (pulse != null)
                StopCoroutine(pulse);

            rectTransform.localScale = baseScale;
            pulse = StartCoroutine(PlayPulse());
        }

        private IEnumerator PlayPulse()
        {
            float safeDuration = Mathf.Max(0.05f, duration);
            float safeGrowFraction = Mathf.Clamp(growFraction, 0.1f, 0.6f);
            float elapsed = 0f;

            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / safeDuration);
                float multiplier;

                if (progress < safeGrowFraction)
                {
                    float grow = Mathf.SmoothStep(0f, 1f, progress / safeGrowFraction);
                    multiplier = Mathf.Lerp(1f, scaleMultiplier, grow);
                }
                else
                {
                    float returnProgress = Mathf.SmoothStep(0f, 1f,
                        (progress - safeGrowFraction) / (1f - safeGrowFraction));
                    multiplier = Mathf.Lerp(scaleMultiplier, 1f, returnProgress);
                }

                rectTransform.localScale = baseScale * multiplier;
                yield return null;
            }

            rectTransform.localScale = baseScale;
            pulse = null;
        }

        private void OnDisable()
        {
            if (pulse != null)
                StopCoroutine(pulse);
            pulse = null;

            if (rectTransform != null)
                rectTransform.localScale = baseScale;
        }
    }
}