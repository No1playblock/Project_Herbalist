using UnityEngine;
namespace Herbalist.GameUI
{
    public static class UserOptions
    {
        private const string VolumeKey = "Herbalist.UI.Volume";
        private const string LookKey = "Herbalist.UI.LookMultiplier";
        public static float Volume => PlayerPrefs.GetFloat(VolumeKey, 1);
        public static float LookMultiplier => PlayerPrefs.GetFloat(LookKey, 1);
        public static void SetVolume(float value) { AudioListener.volume = value; PlayerPrefs.SetFloat(VolumeKey, value); }
        public static void SetLook(float value) => PlayerPrefs.SetFloat(LookKey, value);
    }
}
