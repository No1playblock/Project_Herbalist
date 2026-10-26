using TMPro;
using UnityEngine;

namespace Herbalist.GameUI
{
    public sealed class SapMazeVolumeDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text _amount;
        [SerializeField] private SapMazeShapeGraphic _droplet;
        [SerializeField, Range(0, 1)] private float _warningRatio = .7f;
        [SerializeField] private Color _lowColor = new Color(.90f, .22f, .20f);
        [SerializeField] private Color _warningColor = new Color(.25f, .80f, .29f);
        [SerializeField] private Color _readyColor = new Color(.18f, .67f, 1f);

        public void Present(float current, float target, bool visible)
        {
            if (gameObject.activeSelf != visible)gameObject.SetActive(visible);
            if (!visible)return;
            if (_amount != null)_amount.text = current.ToString("0.0") + " / " + target.ToString("0");
            if (_droplet != null)
            {
                var ratio = target > 0 ? current / target : 0;
                _droplet.color = ratio >= 1 ? _readyColor : ratio >= _warningRatio ? _warningColor : _lowColor;
            }
        }
    }
}
