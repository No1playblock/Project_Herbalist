using System;
using System.Linq;
using System.Reflection;
using Herbalist.Audio;
using Herbalist.Abilities;
using Herbalist.GameUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class SfxIntegrationChecks
    {
        private static void Check(bool condition, string message)
        { if (!condition) throw new Exception("SFX: " + message); }
        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        private static void Tick(SfxEmitter emitter) => typeof(SfxEmitter).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(emitter, null);

        [MenuItem("Tools/Herbalist/Audio/Validate SFX Assets")]
        public static void ValidateMenu() => Debug.Log(ValidateAssets());
        public static string ValidateAssets()
        {
            Check(!EditorApplication.isPlaying, "asset validation requires Edit mode");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                Check(!UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty, "save open scenes before validation");
            var catalog = AssetDatabase.LoadAssetAtPath<SfxCatalog>(SfxAuthoring.CatalogPath);
            Check(catalog != null && catalog.sounds.Length == 57, "57 catalog entries");
            Check(catalog.sounds.Select(x => x.id).Distinct().Count() == 57 && catalog.Find(30) == null, "stable unique IDs, 30 omitted");
            Check(catalog.sounds.Where(x => x.temporaryFallback).Select(x => x.id).OrderBy(x => x).SequenceEqual(new[] { 5, 6, 9, 43, 58 }), "five marked fallbacks");
            foreach (var sound in catalog.sounds)
                Check(sound.clips.Length > 0 && sound.clips.All(c => c != null && c.length > 0), "valid clips: " + sound.id);
            Check(!catalog.Find(46).loop && !catalog.Find(17).loop, "descent and fall wind are one-shots");
            foreach (int id in new[] { 24, 25, 31, 34, 37, 42, 49, 55 }) Check(catalog.Find(id).loop, "loop " + id);
            foreach (int id in new[] { 10, 11, 12 })
                Check(catalog.Find(id).clips.Length == 6 && catalog.Find(id).clips.All(x => x.length <= .5f), "short footstep variants " + id);
            Check(catalog.Find(8).clips[0].length <= 1.5f, "short help chime");
            int emitters = 0, scenes = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" }))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var emitter in root.GetComponentsInChildren<SfxEmitter>(true)) { ValidateEmitter(emitter); emitters++; }
            }
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    var roots = scene.GetRootGameObjects();
                    var sources = roots.SelectMany(r => r.GetComponentsInChildren<SfxEmitter>(true)).ToArray();
                    Check(sources.Count(s => new SerializedObject(s).FindProperty("_scenePool").boolValue) == 1, path + " has one pool");
                    foreach (var emitter in sources) { ValidateEmitter(emitter); emitters++; }
                    foreach (var presenter in roots.SelectMany(r => r.GetComponentsInChildren<WorldSfxPresentation>(true)))
                        Check(new SerializedObject(presenter).FindProperty("_subject").objectReferenceValue != null, path + " world subject");
                    scenes++;
                }
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            return $"PASS: 57 catalog IDs, five explicit fallbacks, loop/clip rules, {scenes} scenes and {emitters} emitter references.";
        }
        private static void ValidateEmitter(SfxEmitter emitter)
        {
            var data = new SerializedObject(emitter);
            Check(data.FindProperty("_catalog").objectReferenceValue != null, emitter.name + " catalog");
            var channels = data.FindProperty("_channels");
            Check(channels.arraySize > 0, emitter.name + " channels");
            for (int i = 0; i < channels.arraySize; i++)
            {
                var source = channels.GetArrayElementAtIndex(i).objectReferenceValue as AudioSource;
                Check(source != null && !source.playOnAwake, emitter.name + " authored channel " + i);
            }
        }
        public static string ValidatePlayback()
        {
            Check(EditorApplication.isPlaying, "playback check requires Play mode");
            var catalog = AssetDatabase.LoadAssetAtPath<SfxCatalog>(SfxAuthoring.CatalogPath);
            var root = new GameObject("SfxPlaybackCheck"); root.SetActive(false);
            bool wasPaused = GameplayPause.IsPaused;
            var previousPool = SfxEmitter.ScenePool;
            try
            {
                var sources = new AudioSource[4];
                for (int i = 0; i < sources.Length; i++)
                {
                    var child = new GameObject("Channel"); child.transform.SetParent(root.transform);
                    sources[i] = child.AddComponent<AudioSource>(); sources[i].playOnAwake = false;
                }
                var emitter = root.AddComponent<SfxEmitter>();
                Field(emitter, "_catalog", catalog); Field(emitter, "_channels", sources); Field(emitter, "_scenePool", true);
                root.SetActive(true); GameplayPause.SetOffline(false);
                foreach (var sound in catalog.sounds)
                {
                    foreach (var source in sources) source.Stop();
                    Check(emitter.Play(sound.id), "play accepted " + sound.id);
                    Check(sources.Any(s => s.isPlaying && s.clip != null), "actual AudioSource playback " + sound.id);
                }
                foreach (var source in sources) source.Stop();
                emitter.SetLoop(34, true); int played = emitter.PlayedCount;
                emitter.SetLoop(34, true);
                Check(emitter.PlayedCount == played && emitter.ActiveLoopCount == 1 && sources.Any(s => s.loop && s.isPlaying), "idempotent sustained loop");
                GameplayPause.SetOffline(true); Tick(emitter);
                Check(!sources.Any(s => s.isPlaying), "gameplay loop pauses");
                Check(emitter.PlayAt(1, Vector3.zero, ownerKey: 80001), "UI remains available during pause");
                GameplayPause.SetOffline(false); Tick(emitter);
                Check(sources.Any(s => s.loop && s.isPlaying), "loop resumes");
                emitter.SetLoop(34, false);
                foreach (var source in sources) source.Stop();
                Check(emitter.PlayAt(33, Vector3.zero, ownerKey: 1001), "first owner cue");
                Check(!emitter.PlayAt(33, Vector3.zero, ownerKey: 1001), "same owner cooldown");
                Check(emitter.PlayAt(33, Vector3.zero, ownerKey: 1002), "second owner independent cooldown");
                emitter.SetLoop(37, true); root.SetActive(false);
                Check(emitter.ActiveLoopCount == 0 && sources.All(s => !s.isPlaying), "disable cleans up audio");
                root.SetActive(true);
                ((System.Collections.IDictionary)typeof(SfxEmitter).GetField("_next", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(emitter)).Clear();
                var leaf = root.AddComponent<LeafProjectile>();
                var presenter = root.AddComponent<WorldSfxPresentation>();
                Field(presenter, "_subject", leaf); Field(presenter, "_kind", SfxWorldKind.Leaf);
                var late = typeof(WorldSfxPresentation).GetMethod("LateUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
                leaf.ApplyReplica(LeafState.Flying, -1); late.Invoke(presenter, null);
                Check(emitter.ActiveLoopCount == 1 && sources.Any(s => s.clip == catalog.Find(33).clips[0]), "flying leaf starts launch and flight audio");
                leaf.ApplyReplica(LeafState.Installed, -1); late.Invoke(presenter, null);
                Check(emitter.ActiveLoopCount == 0 && emitter.LastPlayedId == 35, "installed leaf stops flight and plays impact");
                leaf.ApplyReplica(LeafState.Returning, -1); late.Invoke(presenter, null);
                Check(emitter.LastPlayedId == 36, "leaf recall cue");
                foreach (var source in sources) source.Stop();
                var water = root.AddComponent<SapDeposit>();
                Field(presenter, "_subject", water); Field(presenter, "_kind", SfxWorldKind.Water);
                water.SetHoseStream(Vector3.one, false, Vector3.up, 1, 0, true, Vector3.up); late.Invoke(presenter, null);
                Check(emitter.ActiveLoopCount == 1 && emitter.LastPlayedId == 32, "hose flow and impact cues");
                water.SetStream(Vector3.zero, false); late.Invoke(presenter, null);
                Check(emitter.ActiveLoopCount == 0, "hose stops without lingering loop");
                return "PASS: all 57 IDs actually play; loop idempotence, pause/resume, UI during pause, per-owner cooldown, disable cleanup; leaf flight/install/recall and water flow/impact/stop transitions.";
            }
            finally
            {
                GameplayPause.SetOffline(wasPaused); Object.DestroyImmediate(root);
                typeof(SfxEmitter).GetField("<ScenePool>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, previousPool);
            }
        }
    }
}
