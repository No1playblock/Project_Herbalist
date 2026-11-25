using System;
using System.IO;
using System.Reflection;
using Herbalist.Audio;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.Editor
{
    [InitializeOnLoad]
    public static class SfxPlayback20261126
    {
        private const string Key = "Herbalist.SfxPlayback20261126";
        private static SfxEmitter _emitter;
        private static WorldSfxPresentation _world;
        private static DescendingMazeFlow _descent;
        private static AudioSource[] _channels;
        private static double _deadline, _finish;
        private static bool _started;
        static SfxPlayback20261126() { EditorApplication.playModeStateChanged += Changed; }
        public static void Run()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(Key, true);
            EditorApplication.EnterPlaymode();
        }
        private static void Set(object obj, string name, object value) => obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        private static void Call(string name, object subject) => typeof(WorldSfxPresentation).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_world, new[] { subject });
        private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); File.AppendAllText("Logs/SfxPlayback20261126.txt", "PASS " + message + "\n"); }
        private static void Changed(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Key, false)) return;
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            try
            {
                File.WriteAllText("Logs/SfxPlayback20261126.txt", "Runtime checks\n");
                var catalog = AssetDatabase.LoadAssetAtPath<SfxCatalog>(SfxAuthoring.CatalogPath);
                var root = new GameObject("SfxPlaybackProbe");root.SetActive(false);
                _channels = new AudioSource[3];
                for (int i = 0; i < _channels.Length; i++) { var child = new GameObject("Channel" + i); child.transform.SetParent(root.transform); _channels[i] = child.AddComponent<AudioSource>(); }
                _emitter = root.AddComponent<SfxEmitter>();Set(_emitter, "_catalog", catalog);Set(_emitter, "_channels", _channels);
                _world = root.AddComponent<WorldSfxPresentation>();_world.enabled = false;
                root.SetActive(true);
                foreach (var channel in _channels) { channel.playOnAwake = false; channel.Stop(); }
                Set(_world, "_emitter", _emitter);
                File.AppendAllText("Logs/SfxPlayback20261126.txt", "Before playback catalog=" + _emitter.Catalog.name + " clips=" + catalog.Find(45).clips.Length + " clip=" + catalog.Find(45).clips[0] + " free=" + typeof(SfxEmitter).GetMethod("FreeChannel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_emitter, null) + "\n");
                var inactive = new GameObject("InactiveGameplayProbe");inactive.SetActive(false);
                _descent = inactive.AddComponent<DescendingMazeFlow>();
                Set(_descent, "_phase", 2);Call("Descent", _descent);
                File.AppendAllText("Logs/SfxPlayback20261126.txt", "Diagnostic phase=" + _descent.PresentationPhase + " loops=" + _emitter.ActiveLoopCount + " paused=" + Herbalist.GameUI.GameplayPause.IsPaused + " catalogLoop=" + catalog.Find(45).loop + " channels=" + string.Join(",", Array.ConvertAll(_channels,c => c.clip == null ? "empty" : c.clip.name + ":" + c.loop)) + "\n");
                Check(_emitter.ActiveLoopCount == 1 && Array.Exists(_channels, c => c.clip == catalog.Find(45).clips[0] && c.loop), "Descent starts a looping source");
                _deadline = EditorApplication.timeSinceStartup + catalog.Find(45).clips[0].length * 2.1;
                _finish = _deadline + 20;
                _started = true;EditorApplication.update += Tick;
            }
            catch (Exception e) { Fail(e); }
        }
        private static void Tick()
        {
            if (!_started || !EditorApplication.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup > _finish) throw new Exception("Runtime check timed out");
                if (EditorApplication.timeSinceStartup < _deadline) return;
                Check(_emitter.ActiveLoopCount == 1 && Array.Exists(_channels, c => c.loop && c.isPlaying), "Descent still plays after multiple clip lengths");
                Set(_descent, "_phase", 3);Call("Descent", _descent);
                Check(_emitter.ActiveLoopCount == 0, "Descent stops at destination");
                _emitter.StopLoops();foreach (var c in _channels) c.Stop();
                var holder = new GameObject("InactiveMazeProbe");holder.SetActive(false);
                var board = holder.AddComponent<SapMazeBoard>();
                var definition = ScriptableObject.CreateInstance<SapMazeDefinition>();definition.nodes = new[] { Vector2.zero, Vector2.one };Set(board, "_definition", definition);
                Set(board, "_playable", true);Set(board, "_state", new MazeRunState { Phase = MazePhase.Running });Set(board, "_leakMask", 1);
                Call("Maze", board);Check(_emitter.ActiveLoopCount == 1, "Continuous leak starts");
                Set(board, "_leakMask", 0);Call("Maze", board);Check(_emitter.ActiveLoopCount == 0, "Plugged leak stops");
                var portObject = new GameObject("InletProbe");portObject.transform.position = new Vector3(100,100,100);
                portObject.AddComponent<BoxCollider>();portObject.AddComponent<SapInjectionPort>();
                var waterObject = new GameObject("WaterProbe");waterObject.SetActive(false);
                var water = waterObject.AddComponent<SapDeposit>();
                var settings = ScriptableObject.CreateInstance<SapAbilitySettings>();settings.collisionMask = ~0;Set(water, "settings", settings);
                water.SetHoseStream(portObject.transform.position, false, Vector3.forward, 1, 0, false, Vector3.up);
                Physics.SyncTransforms();Call("Water", water);
                var catalog = _emitter.Catalog;
                Check(_emitter.ActiveLoopCount == 1 && Array.Exists(_channels, c => c.isPlaying && c.clip == catalog.Find(39).clips[0]), "Tree inlet contact starts injection loop");
                water.SetHoseStream(portObject.transform.position, true, Vector3.forward, 1, 0, false, Vector3.up);
                Call("Water", water);Check(_emitter.ActiveLoopCount == 0, "Injection stops when release/retraction begins");
                var stage3 = SceneManager.CreateScene("Stage_03_EnvironmentModeling_FinalReference");
                SceneManager.SetActiveScene(stage3);
                _emitter.StopLoops();foreach (var c in _channels) c.Stop();
                Check(_emitter.Play(1), "Stage 3 one-shot starts");
                Check(Array.Exists(_channels, c => c.clip == catalog.Find(1).clips[0] && Mathf.Approximately(c.volume, catalog.Find(1).volume * .15f)), "Stage 3 one-shot is attenuated");
                foreach (var c in _channels) c.Stop();
                _emitter.SetLoop(55, true);
                Check(Array.Exists(_channels, c => c.clip == catalog.Find(55).clips[0] && Mathf.Approximately(c.volume, catalog.Find(55).volume * .15f)), "Stage 3 rolling loop is attenuated");
                _emitter.SetLoop(55, false);_emitter.SetLoop(49, true);
                Check(Array.Exists(_channels, c => c.clip == catalog.Find(49).clips[0] && Mathf.Approximately(c.volume, catalog.Find(49).volume)), "Talisman slide remains unchanged");
                _emitter.StopLoops();foreach (var c in _channels) c.Stop();
                var stage1 = SceneManager.CreateScene("Stage_01_AudioProbe");SceneManager.SetActiveScene(stage1);
                var padObject = new GameObject("InactiveCountdownProbe");padObject.SetActive(false);
                var pad = padObject.AddComponent<ExteriorJumpPad>();Set(pad, "_launchDelayRemaining", 2f);
                var pads = (System.Collections.Generic.HashSet<ExteriorJumpPad>)typeof(ExteriorJumpPad).GetField("ActivePads",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);pads.Add(pad);
                var sceneObject = new GameObject("SceneAudioProbe");sceneObject.SetActive(false);
                var presenter = sceneObject.AddComponent<SceneSfxPresentation>();Set(presenter,"_emitter",_emitter);
                var late = typeof(SceneSfxPresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
                late.Invoke(presenter,null);
                Check(_emitter.ActiveLoopCount == 1 && Array.Exists(_channels,c=>c.isPlaying&&c.clip==catalog.Find(65).clips[0]), "UI countdown starts former inlet sound");
                Set(pad,"_launchDelayRemaining",0f);late.Invoke(presenter,null);
                Check(_emitter.ActiveLoopCount == 0, "Countdown sound stops at zero");pads.Remove(pad);
                Set(_world,"_kind",SfxWorldKind.Ambient);Set(_world,"_ambientSoundId",62);Set(_world,"_subject",portObject.transform);
                var worldLate=typeof(WorldSfxPresentation).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
                worldLate.Invoke(_world,null);Check(_emitter.ActiveLoopCount==1,"Environment source starts");
                portObject.SetActive(false);worldLate.Invoke(_world,null);Check(_emitter.ActiveLoopCount==0,"Hidden stream environment source stops");
                _started = false;SessionState.SetBool(Key, false);EditorApplication.update -= Tick;
                Debug.Log("SFX_PLAYBACK_PASS");EditorApplication.Exit(0);
            }
            catch (Exception e) { Fail(e); }
        }
        private static void Fail(Exception e)
        {
            _started = false;SessionState.SetBool(Key, false);EditorApplication.update -= Tick;
            File.AppendAllText("Logs/SfxPlayback20261126.txt", "FAIL " + e);Debug.LogException(e);EditorApplication.Exit(1);
        }
    }
}
