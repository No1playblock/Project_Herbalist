using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Herbalist.Audio
{
    [RequireComponent(typeof(Button))]
    public sealed class SfxUiFeedback : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] private int _clickId = 1;
        private Button _button;
        private void Awake() { _button = GetComponent<Button>(); _button.onClick.AddListener(Clicked); }
        private void Clicked() { if (_button.IsInteractable()) SfxEmitter.ScenePool?.Play(_clickId); }
        public void OnPointerEnter(PointerEventData data) { if (GetComponent<Button>().IsInteractable()) SfxEmitter.ScenePool?.Play(2); }
        public void OnSelect(BaseEventData data) { if (GetComponent<Button>().IsInteractable()) SfxEmitter.ScenePool?.Play(2); }
        private void OnDestroy() { if (_button != null) _button.onClick.RemoveListener(Clicked); }
    }
}
