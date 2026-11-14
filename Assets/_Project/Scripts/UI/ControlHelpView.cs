using UnityEngine;
using UnityEngine.InputSystem;

namespace Herbalist.GameUI
{
    public sealed class ControlHelpView : MonoBehaviour
    {
        [SerializeField] private ControlHintView[] _rows;
        public void Present(GameUiSettings settings, bool offlineTest)
        {
            for (int i = 0; i < _rows.Length; i++)
            {
                var hint = i < settings.controls.Length ? settings.controls[i] : null;
                bool visible = hint != null && hint.Action != null && (!hint.offlineTestOnly || offlineTest);
                _rows[i].gameObject.SetActive(visible);
                if (visible) _rows[i].Present(hint.Action.action.GetBindingDisplayString(), hint.Label);
            }
        }
    }
}
