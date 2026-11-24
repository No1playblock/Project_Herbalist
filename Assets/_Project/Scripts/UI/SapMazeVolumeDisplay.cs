using TMPro;
using UnityEngine;

namespace Herbalist.GameUI
{
    public sealed class SapMazeVolumeDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private string _amountFormat =
            "<size=68%><color=#B9D1BF>수액 보유량</color></size>\n" +
            "<size=128%><b>{0:0.0}</b></size> <size=70%><color=#DFE8E0>/ 목표 {1:0}</color></size>";
        [SerializeField] private SapMazeShapeGraphic _droplet;
        [SerializeField, Range(0, 1)] private float _warningRatio = .7f;
        [SerializeField] private Color _lowColor = new Color(.06f, .30f, .52f, 1f);
        [SerializeField] private Color _warningColor = new Color(.08f, .55f, .80f, 1f);
        [SerializeField] private Color _readyColor = new Color(.36f, .82f, .98f, 1f);
        [SerializeField, Min(.1f)] private float _fillTransitionSpeed = 2.5f;

        private float _displayedRatio;
        private bool _fillInitialized;

        public void Present(float current, float target, bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
            if (!visible)
            {
                _fillInitialized = false;
                return;
            }

            if (_amount != null)
                _amount.text = string.Format(_amountFormat, current, target);

            var targetRatio = target > 0 ? Mathf.Clamp01(current / target) : 0f;
            if (!_fillInitialized)
            {
                _displayedRatio = 0f;
                _fillInitialized = true;
            }
            _displayedRatio = Mathf.MoveTowards(_displayedRatio, targetRatio,
                Time.deltaTime * _fillTransitionSpeed);

            if (_droplet == null) return;
            _droplet.SetFillAmount(_displayedRatio);
            _droplet.color = GetSapColor(_displayedRatio);
        }

        private Color GetSapColor(float ratio)
        {
            var transition = Mathf.Clamp01(_warningRatio);
            if (ratio < transition)
                return Color.Lerp(_lowColor, _warningColor,
                    transition > 0 ? ratio / transition : 1f);
            return Color.Lerp(_warningColor, _readyColor,
                transition < 1 ? (ratio - transition) / (1f - transition) : 1f);
        }
    }
}
