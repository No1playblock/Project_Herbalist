using System.Collections.Generic;
using Herbalist.GameUI;
using UnityEngine;

namespace Herbalist.Audio
{
    // All channels are authored in prefabs/scenes. Playback never creates UI or audio objects.
    [DefaultExecutionOrder(-900)]
    public sealed class SfxEmitter : MonoBehaviour
    {
        [SerializeField] private SfxCatalog _catalog;
        [SerializeField] private AudioSource[] _channels;
        [SerializeField] private bool _scenePool;
        private readonly Dictionary<int, float> _next = new();
        private readonly Dictionary<int, int> _loops = new();
        private int[] _playing;
        private bool[] _paused;
        public static SfxEmitter ScenePool { get; private set; }
        public SfxCatalog Catalog => _catalog;
        public int PlayedCount { get; private set; }
        public int LastPlayedId { get; private set; }
        public int ActiveLoopCount => _loops.Count;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPool() => ScenePool = null;
        private void Awake() { _playing = new int[_channels.Length]; _paused = new bool[_channels.Length]; }
        private void OnEnable() { if (_scenePool) ScenePool = this; }
        public bool Play(int id, float gain = 1, bool light = false) => PlayAt(id, transform.position, gain, light);
        public bool PlayAt(int id, Vector3 position, float gain = 1, bool light = false, int ownerKey = 0)
        {
            if (!_scenePool && ScenePool != null && ScenePool != this)
                return ScenePool.PlayAt(id, position, gain, light, GetEntityId().GetHashCode());
            var sound = _catalog != null ? _catalog.Find(id) : null;
            if (!CanPlay(sound) || GameplayPause.IsPaused && sound.pauseWithGameplay) return false;
            int cooldownKey = unchecked(ownerKey * 397 ^ id);
            if (_next.TryGetValue(cooldownKey, out float next) && Time.unscaledTime < next) return false;
            int slot = FreeChannel();
            if (slot < 0) return false; // Preserve active loops and important tails; never allocate voices.
            AudioClip clip = light && sound.lightClip != null ? sound.lightClip : Pick(sound);
            if (clip == null) return false;
            StartChannel(slot, sound, clip, position, gain, false);
            _next[cooldownKey] = Time.unscaledTime + sound.cooldown;
            return true;
        }
        public void SetLoop(int id, bool active, float gain = 1, Vector3? position = null)
        {
            if (!active)
            {
                if (_loops.TryGetValue(id, out int slot)) { _channels[slot].Stop(); _playing[slot] = 0; _paused[slot] = false; _loops.Remove(id); }
                return;
            }
            var sound = _catalog != null ? _catalog.Find(id) : null;
            if (!CanPlay(sound)) return;
            if (_loops.TryGetValue(id, out int existing))
            {
                _channels[existing].volume = sound.volume * Mathf.Clamp01(gain);
                _channels[existing].transform.position = position ?? transform.position;
                return;
            }
            if (GameplayPause.IsPaused && sound.pauseWithGameplay) return;
            int channel = FreeChannel();
            if (channel < 0) return;
            var clip = Pick(sound);
            if (clip == null) return;
            StartChannel(channel, sound, clip, position ?? transform.position, gain, sound.loop);
            _loops[id] = channel;
        }
        public void StopLoops()
        {
            foreach (var slot in _loops.Values) { _channels[slot].Stop(); _playing[slot] = 0; _paused[slot] = false; }
            _loops.Clear();
        }
        private static bool CanPlay(SfxDefinition s) => s != null && s.clips != null && s.clips.Length > 0;
        private static AudioClip Pick(SfxDefinition s) => s.clips[Random.Range(0, s.clips.Length)];
        private int FreeChannel()
        {
            for (int i = 0; i < _channels.Length; i++)
                if (_channels[i] != null && !_channels[i].isPlaying && !_paused[i] && !_loops.ContainsValue(i)) return i;
            return -1;
        }
        private void StartChannel(int slot, SfxDefinition sound, AudioClip clip, Vector3 position, float gain, bool loop)
        {
            var channel = _channels[slot];
            channel.transform.position = position;
            channel.playOnAwake = false; channel.clip = clip; channel.loop = loop;
            channel.volume = sound.volume * Mathf.Clamp01(gain);
            channel.pitch = Random.Range(sound.pitchRange.x, sound.pitchRange.y);
            channel.spatialBlend = sound.spatialBlend;
            channel.minDistance = sound.minDistance; channel.maxDistance = sound.maxDistance;
            channel.rolloffMode = AudioRolloffMode.Linear; channel.dopplerLevel = 0;
            _playing[slot] = sound.id; _paused[slot] = false; channel.Play();
            PlayedCount++; LastPlayedId = sound.id;
        }
        private void Update()
        {
            if (_playing == null || _catalog == null) return;
            for (int i = 0; i < _channels.Length; i++)
            {
                if (_playing[i] == 0 || _channels[i] == null) continue;
                var sound = _catalog.Find(_playing[i]);
                bool pause = sound != null && sound.pauseWithGameplay && GameplayPause.IsPaused;
                if (pause && !_paused[i]) { _channels[i].Pause(); _paused[i] = true; }
                else if (!pause && _paused[i]) { _channels[i].UnPause(); _paused[i] = false; }
            }
        }
        private void OnDisable()
        {
            StopLoops();
            if (_channels != null) foreach (var channel in _channels) if (channel != null) channel.Stop();
            if (ScenePool == this) ScenePool = null;
        }
    }
}
