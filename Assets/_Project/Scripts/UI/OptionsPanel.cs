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
        private bool actionPending;

        private void Awake()
        {
            volume.onValueChanged.AddListener(ChangeVolume);
            sensitivity.onValueChanged.AddListener(ChangeLook);
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

        private void CloseOptionsImmediately()
        {
            actionPending = false;
            gameObject.SetActive(false);
            TitleButtonHover.ClearSelection();
        }

        private void OnEnable() { actionPending = false; volume.SetValueWithoutNotify(UserOptions.Volume); sensitivity.SetValueWithoutNotify(UserOptions.LookMultiplier); Refresh(); }
        private void ChangeVolume(float v) { UserOptions.SetVolume(v); Refresh(); }
        private void ChangeLook(float v) { UserOptions.SetLook(v); Refresh(); }
        private void Refresh() { volumeValue.text=Mathf.RoundToInt(volume.value*100)+"%"; sensitivityValue.text=sensitivity.value.ToString("0.00")+"x"; }
    }
}
