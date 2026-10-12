using UnityEngine;

namespace Herbalist.GameUI
{
    [CreateAssetMenu(menuName = "Herbalist/Game UI/Stage Minigame Text")]
    public sealed class StageMinigameUiText : ScriptableObject
    {
        public string gatherDuyeongTitle;
        [TextArea] public string gatherDuyeongBody;
        public string gatherProgressFormat;
        public string gatherSodamTitle;
        [TextArea] public string gatherSodamIntro;
        public string codexStepsLabel;
        public string gatherSodamProgress;
        public string craftSodamTitle;
        [TextArea] public string craftSodamBodyFormat;
        public string craftSodamProgressFormat;
        public string craftDuyeongTitle;
        [TextArea] public string craftDuyeongBodyFormat;
        public string craftDuyeongProgressFormat;
        public string fireStrong;
        public string fireMedium;
        public string fireWeak;
        [Header("Brewing HUD")]
        public string targetTemperatureTitle;
        public string currentTemperatureTitle;
        public string targetTemperatureFormat;
        public string currentTemperatureFormat;
        public string fireStrengthFormat;
        public string temperatureLow;
        public string temperatureInRange;
        public string temperatureHigh;
        public string brewingGaugeFormat;
        public string brewingTimeFormat;
        public string fanPrompt;
        public string stepUp, stepLeft, stepDown, stepRight;
    }
}
