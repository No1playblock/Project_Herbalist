using UnityEngine;
using UnityEngine.EventSystems;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public sealed class TitleButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
    {
        [SerializeField] private RectTransform _art;
        [SerializeField, Min(1f)] private float _hoverScale = 1.05f;
        [SerializeField, Min(.01f)] private float _duration = .14f;

        private UnityEngine.UI.Button _button;
        private Vector3 _restScale;
        private bool _hovered;
        private float _progress;

        private void Awake()
        {
            _button = GetComponent<UnityEngine.UI.Button>();
            if (_art != null) _restScale = _art.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData) => _hovered = false;

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
            => RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, screenPoint, eventCamera);

        private void Update()
        {
            if (_art == null) return;
            float target = _hovered && _button.isActiveAndEnabled && _button.IsInteractable() ? 1f : 0f;
            if (_progress == target) return;
            _progress = Mathf.MoveTowards(_progress, target, Time.unscaledDeltaTime / _duration);
            float scale = Mathf.SmoothStep(1f, _hoverScale, _progress);
            _art.localScale = new Vector3(_restScale.x * scale, _restScale.y * scale, _restScale.z);
        }

        private void OnDisable()
        {
            _hovered = false;
            _progress = 0f;
            if (_art != null) _art.localScale = _restScale;
        }
    }
}
