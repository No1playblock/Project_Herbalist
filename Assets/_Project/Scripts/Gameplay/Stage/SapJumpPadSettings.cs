using UnityEngine;
namespace Herbalist.Levels
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Sap Jump Pad")]
    public sealed class SapJumpPadSettings : ScriptableObject
    {
        [Min(1)] public int requiredRiders = 2;
        [Min(.05f)] public float chargeUnits = 1.0f;
        [Min(.01f)] public float injectionGrace = .2f;
        [Header("Launch warning")]
        [Min(1f)] public float launchDelaySeconds = 3f;
        [Range(0f, 1f)] public float voiceVolume = 1f;
        public AudioClip chargedVoice;
        public AudioClip twoSecondsVoice;
        public AudioClip oneSecondVoice;
        public AudioClip launchVoice;
        [Min(.05f)] public float flightDuration = 1.4f;
        [Min(.1f)] public float gravity = 18f;
        [Min(.05f)] public float cooldown = 1.5f;
        [Min(.01f)] public float standingTolerance = .3f;
        [Min(.05f)] public float presentationDuration = .65f;
        [Min(0)] public float presentationRise = 1.2f;
        [Min(.01f)] public float eruptionRiseDuration = .2f;
        [Min(0)] public float riderLaunchSpeed = 0f;
        [Min(.01f)] public float jumpWindow = .3f;
        [Min(.1f)] public float jumpReach = 5;
        [Min(.1f)] public float launchHeightMultiplier = 5;
        [Header("Water jet presentation")]
        [Min(.01f)] public float waterStreamWidth = .44f;
        [Range(6, 32)] public int waterStreamSegments = 22;
        [Min(0)] public float waterStreamSway = .018f;
        [Min(0)] public float waterSprayDropsPerMeterPerSecond = 1.8f;
        public Vector2 waterSprayDropSize = new Vector2(.035f, .085f);
        public Vector2 waterSpraySideSpeed = new Vector2(.45f, 1.25f);
        [Min(0)] public float waterSprayForwardSpeed = .6f;
        [Range(1, 12)] public int waterSprayMaxDropsPerFrame = 6;
    }
}
