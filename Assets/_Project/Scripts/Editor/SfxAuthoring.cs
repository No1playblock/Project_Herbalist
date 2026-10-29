using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Herbalist.Audio;
using Herbalist.Abilities;
using Herbalist.Player;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class SfxAuthoring
    {
        public const string CatalogPath = "Assets/_Project/Data/Audio/SO_GameSfx.asset";
        private const string Imported = "Assets/_Project/Audio/SFX/Imported";
        private const string Derived = "Assets/_Project/Audio/SFX/Derived";
        private static SfxCatalog _catalog;
        private static int _changedPrefabs, _changedScenes;
        [MenuItem("Tools/Herbalist/Audio/Apply SFX Catalog And Presenters")]
        public static void ApplyMenu() => Debug.Log(Apply());
        public static void ApplyBatch()
        {
            try { Debug.Log(Apply(false)); }
            catch (Exception error) { File.WriteAllText("Temp/SfxIntegration/batch-failure.log", error.ToString()); Debug.LogException(error); EditorApplication.Exit(1); }
        }
        public static string Apply(bool rebuildCatalog = true)
        {
            if (EditorApplication.isPlaying) throw new Exception("Stop play before authoring SFX");
            if (SceneManager.GetActiveScene().isDirty) throw new Exception("Save the current scene before authoring SFX");
            string current = SceneManager.GetActiveScene().path;
            _changedPrefabs = _changedScenes = 0;
            try
            {
                Directory.CreateDirectory("Temp/SfxIntegration");
                File.WriteAllText("Temp/SfxIntegration/progress.log", "Building catalog\n");
                if (rebuildCatalog) BuildCatalog();
                else _catalog = AssetDatabase.LoadAssetAtPath<SfxCatalog>(CatalogPath);
                if (_catalog == null || _catalog.sounds.Length != 57) throw new Exception("Build the complete 57-ID SFX catalog first");
                File.AppendAllText("Temp/SfxIntegration/progress.log", "Catalog ready\n");
                foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (!Relevant(asset)) continue;
                    var root = PrefabUtility.LoadPrefabContents(path);
                    try { if (Wire(root)) { PrefabUtility.SaveAsPrefabAsset(root, path); _changedPrefabs++; File.AppendAllText("Temp/SfxIntegration/progress.log", "Prefab: " + path + "\n"); } }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
                foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    var before = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Collider>(true)).ToArray();
                    var oldSettings = before.ToDictionary(c => c, c => EditorJsonUtility.ToJson(c) + c.transform.localToWorldMatrix.ToString("R"));
                    var gameplay = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>(true))
                        .Where(c => c != null && c.GetType().Namespace != null && c.GetType().Namespace.StartsWith("Herbalist") && c.GetType().Namespace != "Herbalist.Audio")
                        .ToDictionary(c => c, c => EditorJsonUtility.ToJson(c));
                    foreach (var root in scene.GetRootGameObjects()) Wire(root);
                    var audio = scene.GetRootGameObjects().FirstOrDefault(r => r.name == "GameAudio");
                    if (audio == null) audio = new GameObject("GameAudio");
                    Emitter(audio, 24, true);
                    var state = audio.GetComponent<SceneSfxPresentation>();
                    if (state == null) state = audio.AddComponent<SceneSfxPresentation>();
                    Bool(state, "_forestAmbience", path.Contains("Stage_01/"));
                    foreach (var pair in oldSettings) if (pair.Key == null || EditorJsonUtility.ToJson(pair.Key) + pair.Key.transform.localToWorldMatrix.ToString("R") != pair.Value)
                        throw new Exception("SFX changed physics: " + path + "/" + pair.Key);
                    foreach (var pair in gameplay) if (pair.Key == null || EditorJsonUtility.ToJson(pair.Key) != pair.Value)
                        throw new Exception("SFX changed gameplay settings: " + path + "/" + pair.Key);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save " + path);
                    _changedScenes++;
                    File.AppendAllText("Temp/SfxIntegration/progress.log", "Scene: " + path + "\n");
                }
                AssetDatabase.SaveAssets();
                string report = "SFX catalog: " + _catalog.sounds.Length + " IDs (30 omitted), " + _changedPrefabs + " prefabs and " + _changedScenes + " scenes authored; original physics/gameplay settings preserved.";
                File.AppendAllText("Temp/SfxIntegration/progress.log", report + "\n");
                return report;
            }
            finally { if (!string.IsNullOrEmpty(current)) EditorSceneManager.OpenScene(current, OpenSceneMode.Single); }
        }
        private static bool Relevant(GameObject root) => root != null &&
            (root.GetComponentInChildren<PlayerController>(true) != null || root.GetComponentInChildren<LeafProjectile>(true) != null ||
            root.GetComponentInChildren<SapDeposit>(true) != null || root.GetComponentInChildren<Button>(true) != null ||
            root.GetComponentInChildren<ExteriorJumpPad>(true) != null || root.GetComponentInChildren<HangingCloth>(true) != null);
        private static bool Wire(GameObject root)
        {
            bool changed = false;
            foreach (var p in root.GetComponentsInChildren<PlayerController>(true))
            {
                changed |= Emitter(p.gameObject, 3, false);
                if (p.GetComponent<PlayerSfxPresentation>() == null) { p.gameObject.AddComponent<PlayerSfxPresentation>(); changed = true; }
            }
            changed |= Worlds<LeafProjectile>(root, SfxWorldKind.Leaf);
            changed |= Worlds<SapDeposit>(root, SfxWorldKind.Water);
            changed |= Worlds<ExteriorJumpPad>(root, SfxWorldKind.JumpPad);
            changed |= Worlds<HangingCloth>(root, SfxWorldKind.Cloth, "_knot");
            changed |= Worlds<SapMazeBoard>(root, SfxWorldKind.Maze, "_sapVisual");
            changed |= Worlds<DescendingMazeFlow>(root, SfxWorldKind.Descent, "_rafts");
            changed |= Worlds<BranchRideFlow>(root, SfxWorldKind.BranchRide, "_talisman");
            changed |= Worlds<AltarRing>(root, SfxWorldKind.AltarRing, "_stone");
            changed |= Worlds<RotatingAltar>(root, SfxWorldKind.Altar);
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                var audio = button.GetComponent<SfxUiFeedback>();
                if (audio == null) { audio = button.gameObject.AddComponent<SfxUiFeedback>(); changed = true; }
                string n = button.name.ToLowerInvariant();
                changed |= Int(audio, "_clickId", n.Contains("back") || n.Contains("cancel") || n.Contains("close") ? 3 : 1);
            }
            foreach (var c in root.GetComponentsInChildren<Collider>(true))
            {
                if (c.isTrigger || c is CharacterController || c.GetComponentInParent<PlayerController>(true) != null) continue;
                var surface = c.GetComponent<SfxSurface>();
                if (surface != null) continue;
                string n = AnimationUtility.CalculateTransformPath(c.transform, root.transform).ToLowerInvariant();
                SfxSurfaceKind kind = c is TerrainCollider || n.Contains("grass") ? SfxSurfaceKind.Grass :
                    n.Contains("mush") || n.Contains("musroom") || n.Contains("branch") || n.Contains("leaf") || n.Contains("raft") || n.Contains("deck") || n.Contains("platform") || n.Contains("wood") ? SfxSurfaceKind.Wood :
                    n.Contains("tree") || n.Contains("root") || n.Contains("seonang") ? SfxSurfaceKind.Bark : SfxSurfaceKind.Dirt;
                surface = c.gameObject.AddComponent<SfxSurface>(); Int(surface, "_kind", (int)kind); changed = true;
            }
            return changed;
        }
        private static bool Worlds<T>(GameObject root, SfxWorldKind kind, string motionField = null) where T : Component
        {
            bool changed = false;
            foreach (var subject in root.GetComponentsInChildren<T>(true))
            {
                changed |= Emitter(subject.gameObject, 3, false);
                var audio = subject.GetComponent<WorldSfxPresentation>();
                if (audio == null) { audio = subject.gameObject.AddComponent<WorldSfxPresentation>(); changed = true; }
                changed |= Int(audio, "_kind", (int)kind); changed |= Ref(audio, "_subject", subject);
                var source = new SerializedObject(subject);
                if (motionField != null)
                {
                    var p = source.FindProperty(motionField);
                    var value = p != null && p.isArray && p.arraySize > 0 ? p.GetArrayElementAtIndex(0).objectReferenceValue : p?.objectReferenceValue;
                    changed |= Ref(audio, "_motion", value);
                }
                if (subject is BranchRideFlow)
                {
                    var from = source.FindProperty("_stones"); var data = new SerializedObject(audio); var to = data.FindProperty("_stones");
                    bool edit = to.arraySize != from.arraySize; to.arraySize = from.arraySize;
                    for (int i = 0; i < from.arraySize; i++) { edit |= to.GetArrayElementAtIndex(i).objectReferenceValue != from.GetArrayElementAtIndex(i).objectReferenceValue; to.GetArrayElementAtIndex(i).objectReferenceValue = from.GetArrayElementAtIndex(i).objectReferenceValue; }
                    if (edit) { data.ApplyModifiedPropertiesWithoutUndo(); changed = true; }
                }
            }
            return changed;
        }
        private static bool Emitter(GameObject owner, int channels, bool pool)
        {
            bool changed = false;
            var emitter = owner.GetComponent<SfxEmitter>();
            if (emitter == null) { emitter = owner.AddComponent<SfxEmitter>(); changed = true; }
            changed |= Ref(emitter, "_catalog", _catalog); changed |= Bool(emitter, "_scenePool", pool);
            var data = new SerializedObject(emitter); var array = data.FindProperty("_channels");
            if (array.arraySize >= channels && Enumerable.Range(0, array.arraySize).All(i => array.GetArrayElementAtIndex(i).objectReferenceValue != null)) return changed;
            var container = owner.transform.Find("SfxChannels");
            if (container == null) { container = new GameObject("SfxChannels").transform; container.SetParent(owner.transform, false); }
            array.arraySize = channels;
            for (int i = 0; i < channels; i++)
            {
                var child = container.Find("Channel_" + i.ToString("00"));
                if (child == null) { child = new GameObject("Channel_" + i.ToString("00")).transform; child.SetParent(container, false); }
                var channel = child.GetComponent<AudioSource>();
                if (channel == null) channel = child.gameObject.AddComponent<AudioSource>();
                channel.playOnAwake = false; channel.dopplerLevel = 0; array.GetArrayElementAtIndex(i).objectReferenceValue = channel;
            }
            data.ApplyModifiedPropertiesWithoutUndo(); return true;
        }
        private static bool Ref(Object o, string name, Object value)
        { var s = new SerializedObject(o); var p = s.FindProperty(name); if (p.objectReferenceValue == value) return false; p.objectReferenceValue = value; s.ApplyModifiedPropertiesWithoutUndo(); return true; }
        private static bool Int(Object o, string name, int value)
        { var s = new SerializedObject(o); var p = s.FindProperty(name); if (p.intValue == value) return false; p.intValue = value; s.ApplyModifiedPropertiesWithoutUndo(); return true; }
        private static bool Bool(Object o, string name, bool value)
        { var s = new SerializedObject(o); var p = s.FindProperty(name); if (p.boolValue == value) return false; p.boolValue = value; s.ApplyModifiedPropertiesWithoutUndo(); return true; }
        private static AudioClip Clip(string filename)
        {
            string path = Directory.GetFiles(Imported, filename, SearchOption.AllDirectories).SingleOrDefault();
            if (path == null) throw new Exception("Missing SFX file " + filename);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path.Replace('\\', '/'));
        }
        public static void BuildCatalog()
        {
            Directory.CreateDirectory(Derived); Directory.CreateDirectory("Assets/_Project/Data/Audio");
            AssetDatabase.Refresh();
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Imported }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid); var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                var settings = importer.defaultSampleSettings;
                bool ambient = path.Contains("forest-birds");
                importer.forceToMono = !path.Contains("/UI/") && !ambient;
                settings.loadType = ambient ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = ambient ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
                settings.preloadAudioData = !ambient;
                importer.defaultSampleSettings = settings; importer.SaveAndReimport();
            }
            _catalog = AssetDatabase.LoadAssetAtPath<SfxCatalog>(CatalogPath);
            if (_catalog == null) { _catalog = ScriptableObject.CreateInstance<SfxCatalog>(); AssetDatabase.CreateAsset(_catalog, CatalogPath); }
            var sounds = new List<SfxDefinition>();
            var loopIds = new HashSet<int> { 24, 25, 31, 34, 37, 42, 49, 55 };
            foreach (string line in File.ReadAllLines("Assets/_Project/Documentation/Audio/SfxSourceMap.csv", Encoding.UTF8).Skip(1))
            {
                var values = line.Split(','); if (values.Length < 3 || !int.TryParse(values[0].Trim('\ufeff'), out int id)) continue;
                var sound = new SfxDefinition { id = id, label = values[1], clips = Array.Empty<AudioClip>(), loop = loopIds.Contains(id),
                    spatialBlend = id <= 8 || id >= 18 && id <= 28 || id == 37 || id == 43 ? 0 : 1,
                    pauseWithGameplay = id > 8, volume = id == 37 ? .16f : loopIds.Contains(id) ? .22f : .5f,
                    maxDistance = 100, minDistance = 5, pitchRange = id <= 9 || loopIds.Contains(id) ? Vector2.one : new Vector2(.97f, 1.03f), cooldown = .12f };
                if (!string.IsNullOrWhiteSpace(values[2])) sound.clips = new[] { Clip(values[2].Trim()) };
                if (id == 15 || id == 16) sound.lightClip = Clip(id == 15 ? "15_흙 착지_light_draft.wav" : "16_목재 착지_light_draft.wav");
                if (id == 38) sound.cooldown = 4.2f;
                if (id == 39) sound.cooldown = .8f;
                if (id == 44) sound.cooldown = 2.7f;
                if (id == 52) sound.cooldown = 1.7f;
                if (id == 43) sound.cooldown = .8f;
                if (id == 58) sound.cooldown = 1.5f;
                if (id >= 10 && id <= 12) sound.clips = Steps(sound.clips[0], id);
                if (id == 8) sound.clips = new[] { Excerpt(sound.clips[0], "SFX_08_HelpChime", 1.3f) };
                sounds.Add(sound);
            }
            foreach (var pair in new[] { (5, 4), (6, 1), (9, 4), (43, 3), (58, 8) })
            {
                var entry = sounds.Single(s => s.id == pair.Item1); entry.clips = sounds.Single(s => s.id == pair.Item2).clips;
                entry.temporaryFallback = true; entry.notes = "Source missing; temporary reuse of SFX " + pair.Item2 + ". Replace in this catalog.";
                entry.volume = pair.Item1 == 43 ? .15f : .35f;
            }
            sounds.Single(s => s.id == 39).notes = "Injection cue selected by title. liquid-bubble-3000 is retained as an alternate, not mixed.";
            sounds.Single(s => s.id == 40).notes = "Eruption/activation cue selected by title; source condition cells 39/40 were reversed.";
            sounds.Single(s => s.id == 11).notes = "Grass by filename/title; the source wood-condition cell was inconsistent.";
            sounds.Single(s => s.id == 46).notes = "One-shot tension cue; never loop.";
            _catalog.footstepSoundId = 10;
            _catalog.fallingWindEnabled = false;
            _catalog.sounds = sounds.ToArray(); EditorUtility.SetDirty(_catalog); AssetDatabase.SaveAssets();
        }
        private static float[] Samples(AudioClip clip)
        {
            clip.LoadAudioData(); var data = new float[clip.samples * clip.channels];
            if (!clip.GetData(data, 0)) throw new Exception("Cannot decode " + clip.name);
            return data;
        }
        private static AudioClip[] Steps(AudioClip clip, int id)
        {
            var data = Samples(clip); int window = Mathf.Max(1, clip.frequency / 50); var peaks = new List<(float energy, int sample)>();
            for (int sample = window; sample < clip.samples - window; sample += window)
            {
                float energy = 0; for (int i = sample * clip.channels; i < (sample + window) * clip.channels; i++) energy += data[i] * data[i];
                peaks.Add((energy, sample));
            }
            var selected = new List<int>();
            foreach (var peak in peaks.OrderByDescending(p => p.energy))
            {
                if (selected.All(s => Mathf.Abs(s - peak.sample) > clip.frequency * .55f)) selected.Add(peak.sample);
                if (selected.Count == 6) break;
            }
            return selected.OrderBy(s => s).Select((s, i) => SaveClip(clip, data, Mathf.Max(0, s - clip.frequency / 20), .36f, "SFX_" + id + "_Step_" + i)).ToArray();
        }
        private static AudioClip Excerpt(AudioClip clip, string name, float duration)
        {
            var data = Samples(clip); int strongest = 0; float max = 0;
            for (int i = 0; i < data.Length; i++) if (Mathf.Abs(data[i]) > max) { max = Mathf.Abs(data[i]); strongest = i / clip.channels; }
            return SaveClip(clip, data, Mathf.Max(0, strongest - clip.frequency / 12), duration, name);
        }
        private static AudioClip SaveClip(AudioClip original, float[] data, int start, float duration, string name)
        {
            int frames = Mathf.Min(Mathf.RoundToInt(duration * original.frequency), original.samples - start);
            string path = Derived + "/" + name + ".wav";
            using (var stream = new FileStream(path, FileMode.Create)) using (var writer = new BinaryWriter(stream))
            {
                int count = frames * original.channels; writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2); writer.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                writer.Write((short)1); writer.Write((short)original.channels); writer.Write(original.frequency); writer.Write(original.frequency * original.channels * 2); writer.Write((short)(original.channels * 2)); writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                for (int f = 0; f < frames; f++) for (int ch = 0; ch < original.channels; ch++)
                {
                    float fade = Mathf.Min(1, Mathf.Min(f / Mathf.Max(1f, original.frequency * .004f), (frames - 1 - f) / Mathf.Max(1f, original.frequency * .035f)));
                    writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(data[(start + f) * original.channels + ch] * fade, -1, 1) * 32767));
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
