using UnityEngine;
using UnityEngine.InputSystem;
namespace Herbalist.GameUI
{
    [System.Serializable] public sealed class ControlHint { public string label; public InputActionReference action; }
    [CreateAssetMenu(menuName="Herbalist/Game UI Settings")]
    public sealed class GameUiSettings : ScriptableObject
    {
        public string[] roleNames = { "두영", "소담" };
        public string[] roleDescriptions = { "약초를 발견하고 채집합니다", "전달받은 약초로 약제를 만듭니다" };
        public string selectText="선택하기", selectedText="선택 취소", occupiedText="상대가 선택함";
        public string playerFormat="여행자 {0}", mineSuffix=" · 나", vacantText="선택 대기 중";
        public string waitingText="두 명이 서로 다른 캐릭터를 선택해 주세요";
        public string readyText="준비 완료 · 방장이 시작할 수 있습니다";
        public string participantsFormat="{0} / 2 참가", unassignedText="선택 중";
        public string roomFormat="방 코드  {0}", connectingText="연결 중…", loadingText="숲으로 향하는 중…";
        public string createText="방 만들기", joinText="방 참가하기";
        public string pausedText="게임을 일시 정지하였습니다.", partnerPausedText="다른 플레이어가 게임을 일시 정지하였습니다.";
        public string leafText="잎", sapText="액", gatherText="채집", giveText="전달";
        public string pauseHint="일시정지", helpHint="키 확인", emptyPocket="빈손";
        public string[] leafModes={"꺼짐","고정","발판"};
        public InputActionReference pauseAction, helpAction;
        public ControlHint[] controls;
        public Vector3 markerOffset = new Vector3(-.65f, 1.9f, 0);
        [Min(.1f)] public float feedbackDuration=4;
        public string menuScene="Assets/Scenes/MainMenu.unity";
        public string soloScene="Assets/PlayerPrototype/PlayerMovementPrototype.unity";
    }
}
