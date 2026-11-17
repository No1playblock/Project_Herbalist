using System;
using TMPro;
using UnityEngine;

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
        }

        [SerializeField] private KeySlot[] _keys;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private string _previewBinding;
        [SerializeField] private string _previewDescription;

        private void Update()
        {
            if (!Application.isPlaying) Present(_previewBinding, _previewDescription);
        }

        public void Present(string binding, string description)
        {
            if (_description != null) _description.text = description;
            var parts = (binding ?? string.Empty).Split('/');
            bool overflow = _keys != null && parts.Length > _keys.Length;
            for (int i = 0; _keys != null && i < _keys.Length; i++)
            {
                var slot = _keys[i];
                bool visible = i < parts.Length && (!overflow || i == 0) && !string.IsNullOrWhiteSpace(binding);
                slot.root.SetActive(visible);
                if (!visible) continue;
                string key = overflow ? binding : parts[i].Trim();
                bool showImage = !IsControlKey(key) && slot.image.sprite != null && SameKey(key, slot.binding);
                slot.image.gameObject.SetActive(showImage);
                slot.fallback.gameObject.SetActive(!showImage);
                slot.fallback.text = FormatKeyLabel(key);
            }
        }

        private static string FormatKeyLabel(string key)
        {
            return IsControlKey(key) ? "CTRL" : key;
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
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
