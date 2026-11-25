using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Herbalist.Audio;
using Herbalist.Abilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Herbalist.Editor
{
    // Targeted repair for the 28-item listening review; never runs automatically.
    public static class SfxFeedback20261125
    {
        private static readonly List<string> Report = new();
        private static void Check(bool valid, string label)
        {
            if (!valid) throw new Exception(label);
            Report.Add("PASS " + label);
        }
        public static void ApplyAndValidate()
        {
            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                var catalog = AssetDatabase.LoadAssetAtPath<SfxCatalog>(SfxAuthoring.CatalogPath);
                Check(catalog != null && catalog.sounds.Select(s => s.id).Distinct().Count() == catalog.sounds.Length,
                    "Catalog has unique sound IDs");
                Check(catalog.Find(8).volume == 0 && catalog.Find(59).clips[0] == catalog.Find(8).clips[0], "Silent help, recovery reuses help chime");
                Check(Mathf.Approximately(catalog.Find(59).volume, .375f), "Recovery at 75 percent of former help volume");
                foreach (int id in new[] { 39, 44, 45, 49, 55 })
                    Check(catalog.Find(id).loop, "Continuous sound " + id);
                Check(!catalog.Find(46).loop && !catalog.Find(47).loop, "Descent start tension and stop stay one-shot");
                Check(catalog.Find(catalog.mushroomLandingSoundId) != null, "Dedicated mushroom landing");
                foreach (var sound in catalog.sounds)
                {
                    Check(sound.clips.Length > 0 && sound.clips.All(c => c != null && c.length > 0), "Valid clip " + sound.id);
                    Check(sound.volume >= 0 && sound.volume <= 1, "Valid gain " + sound.id);
                }
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Project/Audio/SFX/Derived" }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.Contains("20261125")) continue;
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    clip.LoadAudioData();
                    var samples = new float[clip.samples * clip.channels];
                    Check(clip.GetData(samples, 0) && samples.All(s => !float.IsNaN(s) && Mathf.Abs(s) < .99f), "Decoded, unclipped " + clip.name);
                }
                string scenePath = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference.unity";
                string backup = "CodexBackups/SfxFeedback20261125/" + scenePath;
                Directory.CreateDirectory(Path.GetDirectoryName(backup));
                if (!File.Exists(backup)) File.Copy(scenePath, backup);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var colliders = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Collider>(true)).ToArray();
                var before = colliders.ToDictionary(c => c, c => EditorJsonUtility.ToJson(c) + c.transform.localToWorldMatrix.ToString("R"));
                int changed = 0;
                foreach (var collider in colliders)
                {
                    if (collider.isTrigger || collider is CharacterController || collider.GetComponentInParent<SapInjectionPort>() != null) continue;
                    bool mushroom = false;
                    for (var t = collider.transform; t != null; t = t.parent)
                    {
                        string n = t.name.ToLowerInvariant();
                        if (n.Contains("mushroom") || n.Contains("musroom")) { mushroom = true; break; }
                    }
                    if (!mushroom) continue;
                    // Put the override on the collider, ahead of an inherited Wood component.
                    var surface = collider.GetComponent<SfxSurface>();
                    if (surface == null) surface = collider.gameObject.AddComponent<SfxSurface>();
                    var serialized = new SerializedObject(surface);
                    serialized.FindProperty("_kind").enumValueIndex = (int)SfxSurfaceKind.Mushroom;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.RecordPrefabInstancePropertyModifications(surface);
                    changed++;
                }
                Check(changed > 0, "Mushroom collider bindings: " + changed);
                Check(before.All(p => p.Key != null && p.Value == EditorJsonUtility.ToJson(p.Key) + p.Key.transform.localToWorldMatrix.ToString("R")),
                    "Original collider settings and placements unchanged");
                EditorSceneManager.MarkSceneDirty(scene);
                Check(EditorSceneManager.SaveScene(scene), "Saved targeted mushroom surface bindings");
                AssetDatabase.SaveAssets();
                Directory.CreateDirectory("Logs");
                File.WriteAllLines("Logs/SfxFeedback20261125.txt", Report);
                Debug.Log("SFX_FEEDBACK_PASS " + Report.Count + " checks");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/SfxFeedback20261125.txt", string.Join("\n", Report) + "\nFAIL " + error);
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }
    }
}
