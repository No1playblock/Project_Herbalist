using UnityEngine;
using UnityEngine.EventSystems;

namespace Herbalist.GameUI
{
    public sealed class OptionsButtonScaleFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField, Min(1f)] private float hoverScale = 1.06f;
        [SerializeField, Min(1f)] private float clickScale = 1.12f;
        [SerializeField, Min(.01f)] private float transitionDuration = .12f;
        [SerializeField, Min(.01f)] private float clickPulseDuration = .2f;

        private RectTransform target;
        private Vector3 restScale;
        private bool hovered;
        private bool pulseActive;
        private float hoverProgress;
        private float pulseElapsed;

        private void Awake()
        {
            target = GetComponent<RectTransform>();
            if (target != null) restScale = target.localScale;
        }

        public void Initialize(RectTransform visual)
        {
            if (visual == null) return;
            target = visual;
            restScale = target.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;
            pulseElapsed = 0f;
            pulseActive = true;
        }

        private void Update()
        {
            if (target == null) return;

            float duration = Mathf.Max(.01f, transitionDuration);
            hoverProgress = Mathf.MoveTowards(hoverProgress, hovered ? 1f : 0f,
                Time.unscaledDeltaTime / duration);

            float hoverMultiplier = Mathf.Lerp(1f, hoverScale, Mathf.SmoothStep(0f, 1f, hoverProgress));
            float clickMultiplier = 1f;
            if (pulseActive)
            {
                pulseElapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(pulseElapsed / Mathf.Max(.01f, clickPulseDuration));
                clickMultiplier = Mathf.Lerp(1f, clickScale, Mathf.Sin(normalized * Mathf.PI));
                if (normalized >= 1f) pulseActive = false;
            }

            target.localScale = restScale * (hoverMultiplier * clickMultiplier);
        }

        private void OnDisable()
        {
            hovered = false;
            pulseActive = false;
            hoverProgress = 0f;
            if (target != null) target.localScale = restScale;
        }
    }
}
