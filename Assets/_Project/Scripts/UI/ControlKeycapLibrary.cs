using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Herbalist.GameUI
{
    [Serializable]
    public sealed class ControlKeycap
    {
        public string key;
        public string[] aliases;
        public Sprite image;
    }

    // Images are packed into a TMP sprite asset in the Editor, never during play.
    [CreateAssetMenu(menuName = "Herbalist/UI/Control Keycaps")]
    public sealed class ControlKeycapLibrary : ScriptableObject
    {
        [SerializeField] private ControlKeycap[] _keys;
        [SerializeField, HideInInspector] private TMP_SpriteAsset _spriteAsset;
        [SerializeField, Min(.1f)] private float _imageScale = 1.25f;
        private Regex _tokens;
        private Dictionary<string, string> _names;
        public ControlKeycap[] Keys => _keys;
        public TMP_SpriteAsset SpriteAsset => _spriteAsset;

        private void OnEnable() { Invalidate(); }
        private void OnValidate() { Invalidate(); }
        public void Invalidate() { _tokens = null; _names = null; }

        public void Present(TMP_Text target, string text)
        {
            if (target == null) return;
            target.spriteAsset = _spriteAsset;
            target.text = Format(text);
        }

        public string Format(string text)
        {
            if (string.IsNullOrEmpty(text) || _spriteAsset == null) return text;
            if (_names == null) BuildLookup();
            if (_tokens == null) return text;
            // Rich-text tags remain intact. Latin word boundaries still allow Korean particles (E로).
            return _tokens.Replace(text, match =>
            {
                if (string.Equals(match.Value, "WASD", StringComparison.OrdinalIgnoreCase)) return Format("W A S D");
                string name;
                if (!_names.TryGetValue(match.Value, out name)) return match.Value;
                string tag = "<sprite name=\"" + name + "\" tint=0>";
                return "<size=" + (_imageScale * 100).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%>" + tag + "</size>";
            });
        }

        private void BuildLookup()
        {
            _names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in _keys ?? Array.Empty<ControlKeycap>())
            {
                if (entry == null || entry.image == null || string.IsNullOrEmpty(entry.key) ||
                    _spriteAsset.GetSpriteIndexFromName(entry.key) < 0) continue;
                _names[entry.key] = entry.key;
                foreach (string alias in entry.aliases ?? Array.Empty<string>())
                    if (!string.IsNullOrEmpty(alias)) _names[alias] = entry.key;
            }
            if (_names.Count == 0) return;
            string choices = "WASD|" + string.Join("|", _names.Keys.OrderByDescending(k => k.Length).Select(Regex.Escape));
            _tokens = new Regex("<[^>]*>|(?<![A-Za-z0-9_])(?:" + choices + ")(?![A-Za-z0-9_])", RegexOptions.IgnoreCase);
        }
    }
}
