using UnityEngine;
using Herbalist.Abilities;
namespace Herbalist.Levels
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Ability Rules")]
    public sealed class StageAbilityRules : ScriptableObject
    {
        public LeafAbilitySettings leaf;
        public SapAbilitySettings sap;
        public bool contextualUse = true;
        public string leafInstructions = "R: 조준한 곳에 나뭇잎 설치 · 좌클릭: 오래된 나뭇잎 회수";
        public string sapInstructions = "R 누르기: 수액 주입 · 미로에서는 R: 조종 전환";
        public string[] duyeongRangeMessages = { "조금 더 가까이 가야겠어.", "여기서는 너무 멀어." };
        public string[] sodamRangeMessages = { "조금 더 가까이 가야겠어요.", "여기서는 너무 멀어요." };
        public string invalidTargetMessage = "능력을 사용할 수 있는 곳을 조준하세요.";
    }
}
