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
        private void Awake()
        {
            volume.onValueChanged.AddListener(ChangeVolume);
            sensitivity.onValueChanged.AddListener(ChangeLook);
            close.onClick.AddListener(()=>gameObject.SetActive(false));
        }
        private void OnEnable() { volume.SetValueWithoutNotify(UserOptions.Volume); sensitivity.SetValueWithoutNotify(UserOptions.LookMultiplier); Refresh(); }
        private void ChangeVolume(float v) { UserOptions.SetVolume(v); Refresh(); }
        private void ChangeLook(float v) { UserOptions.SetLook(v); Refresh(); }
        private void Refresh() { volumeValue.text=Mathf.RoundToInt(volume.value*100)+"%"; sensitivityValue.text=sensitivity.value.ToString("0.00")+"x"; }
    }
}
