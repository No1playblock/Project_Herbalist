using UnityEngine;

namespace Herbalist.StageOne
{
    public enum StageFireStrength { Low, Medium, High }

    public static class StageBrewingRules
    {
        public static StageFireStrength FireStrength(int presses, StageOneSettings settings)
        {
            if (presses <= settings.lowFireMaxPresses) return StageFireStrength.Low;
            return presses <= settings.mediumFireMaxPresses ? StageFireStrength.Medium : StageFireStrength.High;
        }

        public static void Advance(StageOneSettings settings, StageRecipe recipe, int presses, float deltaTime,
            ref float temperature, ref float gauge)
        {
            switch (FireStrength(presses, settings))
            {
                case StageFireStrength.Low:
                    temperature -= settings.temperatureChangePerSecond * deltaTime;
                    break;
                case StageFireStrength.High:
                    temperature += settings.temperatureChangePerSecond * deltaTime;
                    break;
            }
            temperature = Mathf.Clamp(temperature, 0f, settings.maximumTemperature);
            bool target = temperature >= recipe.targetMin && temperature <= recipe.targetMax;
            gauge = Mathf.Clamp(gauge + (target ? settings.gaugeGainPerSecond : -settings.gaugeLossPerSecond) * deltaTime, 0f, 100f);
        }
    }
}
