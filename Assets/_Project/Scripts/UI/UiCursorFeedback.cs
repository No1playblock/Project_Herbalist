using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Herbalist.GameUI
{
    public sealed class UiCursorFeedback : MonoBehaviour
    {
        [SerializeField] private Texture2D _normalCursor;
        [SerializeField] private Texture2D _hoverCursor;
        [SerializeField] private Vector2 _hotspot;
        [SerializeField] private UnityEngine.UI.RawImage _clickRing;
        [SerializeField, Min(0.01f)] private float _clickDuration = 0.32f;
        [SerializeField, Min(0f)] private float _clickStartScale = 0.65f;
        [SerializeField, Min(0f)] private float _clickEndScale = 1.15f;

        private bool _hovering;
        private float _clickStartedAt;

        private void LateUpdate()
        {
            var mouse = Mouse.current;
            if (!Application.isFocused || mouse == null)
            {
                ResetFeedback();
                return;
            }

            bool cursorAvailable = Cursor.visible && Cursor.lockState == CursorLockMode.None;
            SetHover(cursorAvailable && IsOverInteractiveUi(mouse.deviceId));

            if (cursorAvailable && mouse.leftButton.wasPressedThisFrame)
                ShowClickRing(mouse.position.ReadValue());

            if (_clickRing != null && _clickRing.gameObject.activeSelf)
                AnimateClickRing();
        }

        private static bool IsOverInteractiveUi(int mouseDeviceId)
        {
            if (EventSystem.current == null ||
                EventSystem.current.currentInputModule is not InputSystemUIInputModule module)
                return false;

            var hit = module.GetLastRaycastResult(mouseDeviceId);
            if (hit.module is not UnityEngine.UI.GraphicRaycaster || hit.gameObject == null)
                return false;

            var control = hit.gameObject.GetComponentInParent<UnityEngine.UI.Selectable>();
            return control != null && control.isActiveAndEnabled && control.IsInteractable();
        }

        private void SetHover(bool hovering)
        {
            if (_hovering == hovering) return;

            _hovering = hovering;
            var texture = hovering ? _hoverCursor : _normalCursor;
            Cursor.SetCursor(texture, texture != null ? _hotspot : Vector2.zero, CursorMode.Auto);
        }

        private void ShowClickRing(Vector2 screenPosition)
        {
            var overlayRect = (RectTransform)transform;
            if (_clickRing == null || !RectTransformUtility.RectangleContainsScreenPoint(overlayRect, screenPosition) ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, screenPosition, null, out var localPoint))
                return;

            _clickRing.rectTransform.anchoredPosition = localPoint;
            _clickStartedAt = Time.unscaledTime;
            _clickRing.gameObject.SetActive(true);
        }

        private void AnimateClickRing()
        {
            float progress = Mathf.Clamp01((Time.unscaledTime - _clickStartedAt) / _clickDuration);
            if (progress >= 1f)
            {
                _clickRing.gameObject.SetActive(false);
                return;
            }

            _clickRing.rectTransform.localScale = Vector3.one * Mathf.Lerp(_clickStartScale, _clickEndScale, progress);
            var color = _clickRing.color;
            color.a = 1f - progress * progress;
            _clickRing.color = color;
        }

        private void OnDisable() => ResetFeedback();

        private void ResetFeedback()
        {
            SetHover(false);
            if (_clickRing != null && _clickRing.gameObject.activeSelf)
                _clickRing.gameObject.SetActive(false);
        }
    }
}
