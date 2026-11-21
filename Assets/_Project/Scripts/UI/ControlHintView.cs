using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    // Every slot and graphic is authored in the prefab/scene, including text fallbacks.
    [ExecuteAlways]
    public sealed class ControlHintView : MonoBehaviour
    {
        [Serializable]
        public sealed class KeySlot
        {
            public string binding;
            public GameObject root;
            public UnityEngine.UI.Image image;
            public TMP_Text fallback;
            [NonSerialized] internal Sprite originalSprite;
            [NonSerialized] internal bool hasOriginalSprite;
            [NonSerialized] internal float originalPreferredWidth;
            [NonSerialized] internal bool hasOriginalPreferredWidth;
        }

        [SerializeField] private KeySlot[] _keys;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private string _previewBinding;
        [SerializeField] private string _previewDescription;
        [SerializeField] private bool _useIllustratedKeys;
        [SerializeField] private Sprite _squareKeySprite;
        [SerializeField] private Sprite _wideKeySprite;
        [SerializeField] private Sprite _spaceKeySprite;
        [SerializeField] private Sprite _leftMouseSprite;
        [SerializeField] private Sprite _rightMouseSprite;
        [SerializeField, Min(0.1f)] private float _mouseIconScale = 2f;
        [SerializeField] private bool _centerPromptRow;
        [SerializeField, Min(0f)] private float _promptRowGap = 9f;
        [SerializeField] private float _promptRowYOffset = 30f;

        private void Update()
        {
            if (!Application.isPlaying) Present(_previewBinding, _previewDescription);
        }

        public void Present(string binding, string description)
        {
            if (_description != null) _description.text = description;
            var parts = (binding ?? string.Empty).Split('/');
            bool overflow = _keys != null && parts.Length > _keys.Length;
            string singleKey = parts.Length == 1 ? parts[0].Trim() : string.Empty;
            bool mappedSingleKey = !string.IsNullOrWhiteSpace(singleKey) && _keys != null &&
                Array.Exists(_keys, candidate => candidate != null && SameKey(singleKey, candidate.binding));
            for (int i = 0; _keys != null && i < _keys.Length; i++)
            {
                var slot = _keys[i];
                if (slot == null || slot.root == null || slot.image == null || slot.fallback == null) continue;
                bool visible = mappedSingleKey
                    ? SameKey(singleKey, slot.binding)
                    : i < parts.Length && (!overflow || i == 0) && !string.IsNullOrWhiteSpace(binding);
                slot.root.SetActive(visible);
                if (!visible) continue;
                string key = mappedSingleKey ? singleKey : overflow ? binding : parts[i].Trim();
                CacheKeySlotDefaults(slot);
                if (!IsSpaceKey(key) && slot.hasOriginalSprite && slot.image.sprite == _spaceKeySprite)
                    slot.image.sprite = slot.originalSprite;
                SetKeySlotWidth(slot, key);
                if (IsSpaceKey(key) && _spaceKeySprite != null)
                {
                    slot.image.sprite = _spaceKeySprite;
                    slot.image.preserveAspect = true;
                    slot.image.rectTransform.localScale = Vector3.one;
                    slot.image.gameObject.SetActive(true);
                    slot.fallback.gameObject.SetActive(false);
                    continue;
                }
                if (_useIllustratedKeys && slot.image != null && slot.fallback != null)
                {
                    bool leftMouse = IsMouseButton(key, false);
                    bool rightMouse = IsMouseButton(key, true);
                    Sprite artwork = leftMouse ? _leftMouseSprite : rightMouse ? _rightMouseSprite
                        : IsWideKey(key) ? _wideKeySprite : _squareKeySprite;
                    if (artwork != null)
                    {
                        slot.image.sprite = artwork;
                        slot.image.preserveAspect = true;
                        slot.image.rectTransform.localScale = leftMouse || rightMouse
                            ? new Vector3(_mouseIconScale, _mouseIconScale, 1f) : Vector3.one;
                        slot.image.gameObject.SetActive(true);
                        slot.fallback.gameObject.SetActive(!leftMouse && !rightMouse);
                        slot.fallback.text = FormatKeyLabel(key);
                        continue;
                    }
                }
                bool showImage = !IsControlKey(key) && slot.image.sprite != null && SameKey(key, slot.binding);
                slot.image.gameObject.SetActive(showImage);
                slot.fallback.gameObject.SetActive(!showImage);
                slot.fallback.text = FormatKeyLabel(key);
            }

            // Space is rendered with a much wider artwork than a letter key.
            // Always recalculate that row, even for older scene instances that
            // do not yet have the centering flag serialized.
            if (_centerPromptRow || IsSpaceKey(binding)) CenterPromptRow(binding);
        }

        private static void CacheKeySlotDefaults(KeySlot slot)
        {
            if (!slot.hasOriginalSprite)
            {
                slot.originalSprite = slot.image.sprite;
                slot.hasOriginalSprite = true;
            }
            if (!slot.hasOriginalPreferredWidth)
            {
                LayoutElement layout = slot.root.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    slot.originalPreferredWidth = layout.preferredWidth;
                    slot.hasOriginalPreferredWidth = true;
                }
            }
        }

        private static void SetKeySlotWidth(KeySlot slot, string key)
        {
            if (!slot.hasOriginalPreferredWidth) return;
            LayoutElement layout = slot.root.GetComponent<LayoutElement>();
            if (layout == null) return;
            layout.preferredWidth = IsWideKey(key)
                ? Mathf.Max(72f, slot.originalPreferredWidth)
                : slot.originalPreferredWidth;
        }

        private void CenterPromptRow(string binding)
        {
            if (_description == null || _keys == null || _keys.Length == 0 || _keys[0]?.root == null) return;
            RectTransform rowRect = transform as RectTransform;
            RectTransform keycapsRect = _keys[0].root.transform.parent as RectTransform;
            RectTransform descriptionRect = _description.rectTransform;
            if (rowRect == null || keycapsRect == null || descriptionRect == null) return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(keycapsRect);
            float keyWidth = MeasureVisibleKeyWidth(rowRect);
            if (keyWidth <= 0.1f) keyWidth = Mathf.Max(24f, LayoutUtility.GetPreferredWidth(keycapsRect));

            float availableWidth = Mathf.Max(48f, rowRect.rect.width - 28f);
            // The illustrated Space key is much wider than a letter key. Keep a
            // visible minimum gap so the following Korean prompt never touches it.
            float gap = IsSpaceKey(binding) ? Mathf.Max(14f, _promptRowGap) : _promptRowGap;
            float preferredTextWidth = Mathf.Max(1f, _description.GetPreferredValues(_description.text).x);
            float textWidth = Mathf.Min(preferredTextWidth, Mathf.Max(32f, availableWidth - keyWidth - gap));
            float totalWidth = Mathf.Min(availableWidth, keyWidth + gap + textWidth);

            keycapsRect.anchorMin = new Vector2(0.5f, keycapsRect.anchorMin.y);
            keycapsRect.anchorMax = new Vector2(0.5f, keycapsRect.anchorMax.y);
            keycapsRect.pivot = new Vector2(0.5f, keycapsRect.pivot.y);
            keycapsRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, keyWidth);
            Vector2 keyPosition = keycapsRect.anchoredPosition;
            keyPosition.x = -totalWidth * 0.5f + keyWidth * 0.5f;
            keyPosition.y = _promptRowYOffset;
            keycapsRect.anchoredPosition = keyPosition;
            LayoutRebuilder.ForceRebuildLayoutImmediate(keycapsRect);

            // Use the rendered keycap's right edge after layout (including sprite
            // scale and the active slot) so the description can never sit under it.
            float renderedKeyRight = MeasureVisibleKeyRight(rowRect);

            descriptionRect.anchorMin = new Vector2(0.5f, descriptionRect.anchorMin.y);
            descriptionRect.anchorMax = new Vector2(0.5f, descriptionRect.anchorMax.y);
            descriptionRect.pivot = new Vector2(0f, descriptionRect.pivot.y);
            descriptionRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
            Vector2 descriptionPosition = descriptionRect.anchoredPosition;
            descriptionPosition.x = Mathf.Max(-totalWidth * 0.5f + keyWidth + gap, renderedKeyRight + gap);
            descriptionPosition.y = _promptRowYOffset;
            descriptionRect.anchoredPosition = descriptionPosition;
            _description.alignment = TextAlignmentOptions.Left;

            InteractionTargetOrnament ornament = FindNearbyOrnament();
            if (ornament != null) ornament.SetContentWidth(totalWidth);
        }

        private InteractionTargetOrnament FindNearbyOrnament()
        {
            Transform ancestor = transform;
            while (ancestor != null)
            {
                InteractionTargetOrnament ornament = ancestor.GetComponentInChildren<InteractionTargetOrnament>(true);
                if (ornament != null) return ornament;
                ancestor = ancestor.parent;
            }
            return null;
        }

        private float MeasureVisibleKeyWidth(RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            bool found = false;

            for (int i = 0; i < _keys.Length; i++)
            {
                KeySlot slot = _keys[i];
                if (slot == null || slot.root == null || !slot.root.activeSelf) continue;
                RectTransform visual = null;
                if (slot.image != null && slot.image.gameObject.activeSelf)
                    visual = slot.image.rectTransform;
                else if (slot.fallback != null && slot.fallback.gameObject.activeSelf)
                    visual = slot.fallback.rectTransform;
                else
                    visual = slot.root.transform as RectTransform;
                if (visual == null) continue;

                visual.GetWorldCorners(corners);
                for (int corner = 0; corner < corners.Length; corner++)
                {
                    float x = relativeTo.InverseTransformPoint(corners[corner]).x;
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                }
                found = true;
            }

            return found ? maxX - minX : 0f;
        }

        private float MeasureVisibleKeyRight(RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            float maxX = float.NegativeInfinity;
            bool found = false;

            for (int i = 0; i < _keys.Length; i++)
            {
                KeySlot slot = _keys[i];
                if (slot == null || slot.root == null || !slot.root.activeSelf) continue;
                RectTransform visual = null;
                if (slot.image != null && slot.image.gameObject.activeSelf)
                    visual = slot.image.rectTransform;
                else if (slot.fallback != null && slot.fallback.gameObject.activeSelf)
                    visual = slot.fallback.rectTransform;
                else
                    visual = slot.root.transform as RectTransform;
                if (visual == null) continue;

                visual.GetWorldCorners(corners);
                for (int corner = 0; corner < corners.Length; corner++)
                    maxX = Mathf.Max(maxX, relativeTo.InverseTransformPoint(corners[corner]).x);
                found = true;
            }

            return found ? maxX : 0f;
        }

        private static string FormatKeyLabel(string key)
        {
            return IsControlKey(key) ? "CTRL" : key;
        }

        private static bool IsSpaceKey(string key)
        {
            string normalized = (key ?? string.Empty).Trim().Replace(" ", string.Empty);
            return normalized.Equals("Space", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("SpaceBar", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWideKey(string key)
        {
            string normalized = (key ?? string.Empty).Trim().Replace(" ", string.Empty);
            return normalized.Equals("Space", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("SpaceBar", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Enter", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Return", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("Shift", StringComparison.OrdinalIgnoreCase)
                || IsControlKey(normalized);
        }

        private static bool IsMouseButton(string key, bool right)
        {
            string normalized = (key ?? string.Empty).Trim().Replace(" ", string.Empty);
            return right
                ? normalized.Equals("RMB", StringComparison.OrdinalIgnoreCase)
                    || normalized.Equals("RightButton", StringComparison.OrdinalIgnoreCase)
                    || normalized.Equals("RightMouseButton", StringComparison.OrdinalIgnoreCase)
                : normalized.Equals("LMB", StringComparison.OrdinalIgnoreCase)
                    || normalized.Equals("LeftButton", StringComparison.OrdinalIgnoreCase)
                    || normalized.Equals("LeftMouseButton", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsControlKey(string key)
        {
            string normalized = (key ?? string.Empty).Trim().Replace(" ", string.Empty);
            return string.Equals(normalized, "Ctrl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Control", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "LeftCtrl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "RightCtrl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "LeftControl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "RightControl", StringComparison.OrdinalIgnoreCase);
        }

        private static bool SameKey(string a, string b)
        {
            if (IsSpaceKey(a) && IsSpaceKey(b)) return true;
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
