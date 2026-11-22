using UnityEngine;
using UnityEngine.Audio;
namespace Herbalist.GameUI
{
    public static class UserOptions
    {
        private const string VolumeKey = "Herbalist.UI.Volume";
        private const string BgmKey = "Herbalist.UI.BgmVolume";
        private const string SfxKey = "Herbalist.UI.SfxVolume";
        private const string LookKey = "Herbalist.UI.LookMultiplier";
        private const string BgmParameter = "BGMVolume";
        private const string SfxParameter = "SFXVolume";
        private static AudioMixer _mixer;
        private static AudioMixerGroup _sfxOutput;

        public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1));
        public static float BgmVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(BgmKey, 1));
        public static float SfxVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, 1));
        public static float LookMultiplier => PlayerPrefs.GetFloat(LookKey, 1);

        private static AudioMixer Mixer => _mixer != null ? _mixer : _mixer = Resources.Load<AudioMixer>("Audio/HerbalistVolume");

        internal static AudioMixerGroup SfxOutput
        {
            get
            {
                if (_sfxOutput == null && Mixer != null)
                {
                    var groups = Mixer.FindMatchingGroups("SFX");
                    if (groups.Length > 0) _sfxOutput = groups[0];
                }
                return _sfxOutput;
            }
        }

        internal static void ApplyAudioVolumes()
        {
            AudioListener.volume = Volume;
            ApplyMixerVolume(BgmParameter, BgmVolume);
            ApplyMixerVolume(SfxParameter, SfxVolume);
        }

        public static void SetVolume(float value)
        {
            float volume = Mathf.Clamp01(value);
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat(VolumeKey, volume);
        }

        public static void SetBgmVolume(float value)
        {
            float volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(BgmKey, volume);
            ApplyMixerVolume(BgmParameter, volume);
        }

        public static void SetSfxVolume(float value)
        {
            float volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, volume);
            ApplyMixerVolume(SfxParameter, volume);
        }

        private static void ApplyMixerVolume(string parameter, float volume)
        {
            if (Mixer != null)
                Mixer.SetFloat(parameter, 20f * Mathf.Log10(Mathf.Max(volume, .0001f)));
        }

        public static void SetLook(float value) => PlayerPrefs.SetFloat(LookKey, value);
    }
}
