using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Herbalist.GameUI
{
    public sealed class ControlHelpView : MonoBehaviour
    {
        [SerializeField] private ControlHintView[] _rows;
        private bool _needsFit;

        private void OnEnable() => _needsFit = true;

        public void Present(GameUiSettings settings, RectTransform panel)
        {
            for (int i = 0; i < _rows.Length; i++)
            {
                var hint = i < settings.controls.Length ? settings.controls[i] : null;
                bool visible = settings.ShowsInControlHelp(hint);
                var row = _rows[i].gameObject;
                if (row.activeSelf != visible)
                {
                    row.SetActive(visible);
                    _needsFit = true;
                }
                if (visible) _rows[i].Present(hint.Action.action.GetBindingDisplayString(), hint.Label);
            }

            if (!_needsFit) return;
            _needsFit = false;
            var list = (RectTransform)transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(list);
            float verticalPadding = Mathf.Max(0f, panel.rect.height - list.rect.height);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutUtility.GetPreferredHeight(list) + verticalPadding);
        }
    }
}
