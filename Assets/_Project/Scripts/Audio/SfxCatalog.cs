using System;
using UnityEngine;

namespace Herbalist.Audio
{
    // Numbers are the designer's stable IDs. 30 was intentionally deleted.
    [Serializable]
    public sealed class SfxDefinition
    {
        public int id;
        public string label;
        public AudioClip[] clips;
        public AudioClip lightClip;
        [Range(0, 1)] public float volume = .5f;
        [Range(0, 1)] public float spatialBlend = 1;
        [Min(.01f)] public float minDistance = 3;
        [Min(.1f)] public float maxDistance = 35;
        [Min(0)] public float cooldown = .1f;
        public Vector2 pitchRange = Vector2.one;
        public bool loop;
        public bool pauseWithGameplay = true;
        public bool temporaryFallback;
        [TextArea] public string notes;
    }

    [CreateAssetMenu(menuName = "Herbalist/Audio/SFX Catalog")]
    public sealed class SfxCatalog : ScriptableObject
    {
        public SfxDefinition[] sounds = Array.Empty<SfxDefinition>();
        [Header("Presentation tuning")]
        [Tooltip("Shared walking sound for every ground surface.")]
        public int footstepSoundId = 10;
        public bool fallingWindEnabled;
        [Min(.05f)] public float stepDistanceInHeights = .4f;
        [Min(.01f)] public float movementThreshold = .15f;
        [Min(.1f)] public float strongLandingSpeed = 8;
        [Min(.1f)] public float fallingWindSpeed = 8;
        [Min(1)] public float teleportDistanceInHeights = 4;
        [Min(.01f)] public float groundProbeHeight = 1;
        [Min(.01f)] public float groundProbeDistance = 2;
        public LayerMask groundMask = ~(1 << 8 | 1 << 10);
        public float boilingTemperature = 35;
        [Min(.01f)] public float clothMotionThreshold = .3f;
        [Min(.01f)] public float stoneContactDistance = .7f;
        public SfxDefinition Find(int id)
        {
            foreach (var sound in sounds) if (sound != null && sound.id == id) return sound;
            return null;
        }
    }
}
