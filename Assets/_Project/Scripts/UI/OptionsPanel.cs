using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Herbalist.GameUI
{
    public sealed class OptionsPanel : MonoBehaviour
    {
        public Slider volume, sensitivity;
        public TMP_Text volumeValue, sensitivityValue;
        public Button close, mainMenu;
        [SerializeField] private Slider _bgmVolume, _sfxVolume;
        [SerializeField] private TMP_Text _bgmVolumeValue, _sfxVolumeValue;
        private bool actionPending;

        private void Awake()
        {
            volume.onValueChanged.AddListener(ChangeVolume);
            sensitivity.onValueChanged.AddListener(ChangeLook);
            if (_bgmVolume != null) _bgmVolume.onValueChanged.AddListener(ChangeBgmVolume);
            if (_sfxVolume != null) _sfxVolume.onValueChanged.AddListener(ChangeSfxVolume);
            if (close != null)
            {
                close.onClick.AddListener(CloseOptions);
                AddScaleFeedback(close);
            }
            if (mainMenu != null)
            {
                mainMenu.onClick.AddListener(ReturnToMainMenu);
                AddScaleFeedback(mainMenu);
            }
        }

        private static void AddScaleFeedback(Button button)
        {
            var feedback = button.GetComponent<OptionsButtonScaleFeedback>();
            if (feedback == null) feedback = button.gameObject.AddComponent<OptionsButtonScaleFeedback>();
            feedback.Initialize(button.transform as RectTransform);
        }

        private void CloseOptions()
        {
            if (actionPending) return;
            StartCoroutine(FinishAfterButtonFeedback(false));
        }

        private void ReturnToMainMenu()
        {
            if (actionPending) return;
            StartCoroutine(FinishAfterButtonFeedback(true));
        }

        private IEnumerator FinishAfterButtonFeedback(bool returnToMenu)
        {
            actionPending = true;
            yield return new WaitForSecondsRealtime(.22f);

            if (returnToMenu)
            {
                var overlay = GetComponentInParent<GameOverlayHud>();
                if (overlay != null) overlay.ReturnToMenuFromOptions();
                else CloseOptionsImmediately();
                yield break;
            }

            CloseOptionsImmediately();
        }

        internal void CloseOptionsImmediately()
        {
            actionPending = false;
            gameObject.SetActive(false);
            TitleButtonHover.ClearSelection();
        }

        private void OnEnable()
        {
            actionPending = false;
            volume.SetValueWithoutNotify(UserOptions.Volume);
            sensitivity.SetValueWithoutNotify(UserOptions.LookMultiplier);
            if (_bgmVolume != null) _bgmVolume.SetValueWithoutNotify(UserOptions.BgmVolume);
            if (_sfxVolume != null) _sfxVolume.SetValueWithoutNotify(UserOptions.SfxVolume);
            Refresh();
        }

        private void OnDisable() => PlayerPrefs.Save();
        private void ChangeVolume(float value) { UserOptions.SetVolume(value); Refresh(); }
        private void ChangeBgmVolume(float value) { UserOptions.SetBgmVolume(value); Refresh(); }
        private void ChangeSfxVolume(float value) { UserOptions.SetSfxVolume(value); Refresh(); }
        private void ChangeLook(float value) { UserOptions.SetLook(value); Refresh(); }

        private void Refresh()
        {
            volumeValue.text = FormatVolume(volume.value);
            sensitivityValue.text = sensitivity.value.ToString("0.00") + "x";
            if (_bgmVolume != null && _bgmVolumeValue != null) _bgmVolumeValue.text = FormatVolume(_bgmVolume.value);
            if (_sfxVolume != null && _sfxVolumeValue != null) _sfxVolumeValue.text = FormatVolume(_sfxVolume.value);
        }

        private static string FormatVolume(float value) => Mathf.RoundToInt(value * 100f) + "%";
    }
}
