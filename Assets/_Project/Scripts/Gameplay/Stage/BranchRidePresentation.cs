using Herbalist.GameUI;
using UnityEngine;

namespace Herbalist.Levels
{
    public sealed class BranchRidePresentation : MonoBehaviour
    {
        [SerializeField] private BranchRideFlow _flow;
        [SerializeField] private GameOverlayHud _hud;
        [SerializeField] private Transform _firstTarget, _firstStone;
        [SerializeField, Min(1)] private float _noticeRange = 16;
        [SerializeField, Min(.1f)] private float _subtitleDuration = 4;
        [SerializeField] private string _start = "부적을 타고 가지를 따라 이동하세요.";
        [SerializeField] private string _target = "소담: 과녁이 있어요. 왜 있는 걸까요?";
        [SerializeField] private string _turned = "두영: 가지가 움직였어. 과녁으로 길을 바꿀 수 있나 봐.";
        [SerializeField] private string _stone = "두영: 잠깐 멈출 수 없을까?  소담: 돌이 지나간 뒤에 움직이는 게 좋겠어요.";
        private int _seen, _failures;
        private float _remaining;
        private void Update()
        {
            if (_flow == null || _hud == null || GameplayPause.IsPaused) return;
            _remaining = Mathf.Max(0, _remaining - Time.deltaTime);
            if (_failures != _flow.FailureCount) { _failures = _flow.FailureCount; _seen = 0; }
            if (_flow.CurrentPhase != BranchRidePhase.Riding) return;
            if (_remaining > 0) return;
            if ((_seen & 1) == 0) Show(1, _start);
            else if ((_seen & 2) == 0 && _firstTarget != null && Vector3.Distance(_flow.Position, _firstTarget.position) < _noticeRange) Show(2, _target);
            else if ((_seen & 4) == 0 && (_flow.PresentationBranchMask & 1) != 0) Show(4, _turned);
            else if ((_seen & 8) == 0 && _firstStone != null && Vector3.Distance(_flow.Position, _firstStone.position) < _noticeRange) Show(8, _stone);
        }
        private void Show(int bit, string text) { _seen |= bit; _remaining = _subtitleDuration; _hud.ShowSubtitle(text, _subtitleDuration); }
    }
}
