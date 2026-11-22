using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Herbalist.GameUI
{
    public sealed class OptionsPanel : MonoBehaviour
    {
        public Slider volume, sensitivity;
        public TMP_Text volumeValue, sensitivityValue;
        public Button close;
        [SerializeField] private Button _returnToMenu;
        private void Awake()
        {
            volume.onValueChanged.AddListener(ChangeVolume);
            sensitivity.onValueChanged.AddListener(ChangeLook);
            close.onClick.AddListener(CloseOptions);
        }
        internal void BindReturnToMenu(UnityEngine.Events.UnityAction returnToMenu)
        {
            if (_returnToMenu != null)
                _returnToMenu.onClick.AddListener(returnToMenu);
        }

        internal void CloseOptions()
        {
            gameObject.SetActive(false);
            TitleButtonHover.ClearSelection();
        }

        private void OnEnable() { volume.SetValueWithoutNotify(UserOptions.Volume); sensitivity.SetValueWithoutNotify(UserOptions.LookMultiplier); Refresh(); }
        private void ChangeVolume(float v) { UserOptions.SetVolume(v); Refresh(); }
        private void ChangeLook(float v) { UserOptions.SetLook(v); Refresh(); }
        private void Refresh() { volumeValue.text=Mathf.RoundToInt(volume.value*100)+"%"; sensitivityValue.text=sensitivity.value.ToString("0.00")+"x"; }
    }
}
