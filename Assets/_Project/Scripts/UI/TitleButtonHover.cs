using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    [RequireComponent(typeof(Button))]
    public sealed class TitleButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
    {
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

        [SerializeField] private RectTransform _art;
        [SerializeField] private GameObject _hoverVisual;
        [SerializeField] private GameObject[] _additionalHoverVisuals;
        [SerializeField, Min(1f)] private float _hoverScale = 1.05f;
        [SerializeField, Min(.01f)] private float _duration = .14f;
        [SerializeField, Range(0f, 1f)] private float _hoverOutlineWidth = .18f;

        private static TitleButtonHover _selectedButton;

        private Button _button;
        private TMP_Text _label;
        private Material _labelMaterial;
        private Color _normalLabelColor;
        private float _normalOutlineWidth;
        private bool _hasOutlineWidth;
        private Vector3 _restScale;
        private bool _hovered;
        private bool _selectedByClick;
        private float _progress;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _label = transform.Find("Label")?.GetComponent<TMP_Text>();
            if (_label == null) _label = GetComponentInChildren<TMP_Text>(true);
            if (_label != null)
            {
                _normalLabelColor = _label.color;
                _labelMaterial = _label.fontMaterial;
                _hasOutlineWidth = _labelMaterial != null && _labelMaterial.HasProperty(OutlineWidthId);
                if (_hasOutlineWidth) _normalOutlineWidth = _labelMaterial.GetFloat(OutlineWidthId);
            }
            if (_art != null) _restScale = _art.localScale;
            RefreshHoverVisual();
        }

        private void OnEnable()
        {
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null) _button.onClick.AddListener(SelectThisButton);
            RefreshHoverVisual();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            RefreshHoverVisual();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            RefreshHoverVisual();
        }

        public static void ClearSelection()
        {
            TitleButtonHover selected = _selectedButton;
            _selectedButton = null;
            if (selected != null) selected.SetClickedSelected(false);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
            => RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, screenPoint, eventCamera);

        private void SelectThisButton()
        {
            if (_button == null || !_button.isActiveAndEnabled || !_button.IsInteractable()) return;

            if (_selectedButton != null && _selectedButton != this)
                _selectedButton.SetClickedSelected(false);

            _selectedButton = this;
            SetClickedSelected(true);
        }

        private void SetClickedSelected(bool selected)
        {
            _selectedByClick = selected;
            if (!selected && _selectedButton == this) _selectedButton = null;
            RefreshHoverVisual();
        }

        private void RefreshHoverVisual()
        {
            bool canHover = _hovered && _button != null && _button.isActiveAndEnabled && _button.IsInteractable();
            bool highlighted = _selectedByClick || canHover;

            if (_hoverVisual != null) _hoverVisual.SetActive(highlighted);
            SetAdditionalHoverVisuals(highlighted);
            if (_label != null) _label.color = highlighted ? Color.black : _normalLabelColor;
            if (_hasOutlineWidth) _labelMaterial.SetFloat(OutlineWidthId, highlighted ? _hoverOutlineWidth : _normalOutlineWidth);
        }

        private void SetAdditionalHoverVisuals(bool visible)
        {
            if (_additionalHoverVisuals == null) return;
            foreach (GameObject visual in _additionalHoverVisuals)
                if (visual != null) visual.SetActive(visible);
        }
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
            if (_button != null) _button.onClick.RemoveListener(SelectThisButton);
            _hovered = false;
            _progress = 0f;
            if (_art != null) _art.localScale = _restScale;
            if (_selectedButton == this) _selectedButton = null;
            _selectedByClick = false;
            if (_hoverVisual != null) _hoverVisual.SetActive(false);
            SetAdditionalHoverVisuals(false);
            if (_label != null) _label.color = _normalLabelColor;
            if (_hasOutlineWidth) _labelMaterial.SetFloat(OutlineWidthId, _normalOutlineWidth);
        }
    }
}