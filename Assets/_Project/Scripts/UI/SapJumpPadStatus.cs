using UnityEngine;
using TMPro;
using Herbalist.Levels;
namespace Herbalist.GameUI
{
    public sealed class SapJumpPadStatus : MonoBehaviour
    {
        [SerializeField] private SapJumpPad _pad;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private string[] _messages = {
            "발판 나뭇잎을 구멍에 설치하세요",
            "두 명이 나뭇잎 위에 올라가세요 · {0}/{1}",
            "연결된 주입구에 수액을 쏘세요",
            "수액 충전 · {2:0}%",
            "다음 사용을 준비하는 중"
        };
        private void Update()
        {
            if(_pad==null || _label==null || _pad.Settings==null) return;
            int index=(int)_pad.Phase;
            if(index>=0 && index<_messages.Length)
                _label.text=string.Format(_messages[index],_pad.RiderCount,_pad.Settings.requiredRiders,_pad.ChargeFraction*100);
        }
    }
}