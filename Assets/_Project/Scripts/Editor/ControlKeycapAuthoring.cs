using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Herbalist.GameUI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore;

namespace Herbalist.Editor
{
    public static class ControlKeycapAuthoring
    {
        public const string LibraryPath = "Assets/_Project/Data/UI/SO_ControlKeycaps.asset";
        private const string SpritePath = "Assets/_Project/Data/UI/TMP_ControlKeycaps.asset";
        private const string AtlasPath = "Assets/_Project/Art/UI/Textures/ArtworkPlaceholders/T_ControlKeycaps.png";

        [MenuItem("Herbalist/UI Art/06 Control Keycaps")]
        public static void Open() { Selection.activeObject = AssetDatabase.LoadAssetAtPath<ControlKeycapLibrary>(LibraryPath); }

        public static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
            var library = AssetDatabase.LoadAssetAtPath<ControlKeycapLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<ControlKeycapLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
                var data = new SerializedObject(library);
                var keys = data.FindProperty("_keys");
                string[] names = { "W", "A", "S", "D", "Space", "E", "R", "F", "Tab", "H", "Esc", "Q", "G", "T", "Left Button" };
                keys.arraySize = names.Length;
                for (int i = 0; i < names.Length; i++)
                {
                    var item = keys.GetArrayElementAtIndex(i);
                    item.FindPropertyRelative("key").stringValue = names[i];
                    var aliases = item.FindPropertyRelative("aliases");
                    string[] values = names[i] == "Space" ? new[] { "SpaceBar", "Space Bar" } :
                        names[i] == "Esc" ? new[] { "Escape" } :
                        names[i] == "Left Button" ? new[] { "LMB", "Left Mouse Button" } : Array.Empty<string>();
                    aliases.arraySize = values.Length;
                    for (int j = 0; j < values.Length; j++) aliases.GetArrayElementAtIndex(j).stringValue = values[j];
                }
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            Rebuild(library);
            var settings = AssetDatabase.LoadAssetAtPath<GameUiSettings>("Assets/_Project/Data/UI/SO_GameUiSettings.asset");
            var serialized = new SerializedObject(settings);
            serialized.FindProperty("_keycaps").objectReferenceValue = library;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            string prefabPath = "Assets/_Project/Prefabs/UI/PF_PlayerHud.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try { Bind(prefab, library.SpriteAsset); PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            foreach (string path in new[] { "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity", "Assets/_Project/Scenes/Stages/Stage_03/Stage_03_BranchRide.unity" })
            {
                var scene = SceneManager.GetSceneByPath(path);
                bool opened = !scene.IsValid() || !scene.isLoaded;
                if (!opened && scene.isDirty) throw new InvalidOperationException("Save scene first: " + path);
                var active = SceneManager.GetActiveScene();
                if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                        foreach (var hud in root.GetComponentsInChildren<PlayerHud>(true)) Bind(hud.gameObject, library.SpriteAsset);
                    EditorSceneManager.SaveScene(scene);
                }
                finally
                {
                    if (active.IsValid()) SceneManager.SetActiveScene(active);
                    if (opened) EditorSceneManager.CloseScene(scene, true);
                }
            }
            AssetDatabase.SaveAssets();
        }

        private static void Bind(GameObject root, TMP_SpriteAsset sprites)
        {
            foreach (var hud in root.GetComponentsInChildren<PlayerHud>(true))
                foreach (var text in new[] { hud.helpText, hud.helpHint, hud.promptText, hud.clothPromptText, hud.sapPromptText, hud.abilityText })
                    if (text != null) { text.spriteAsset = sprites; EditorUtility.SetDirty(text); }
            foreach (var minigame in root.GetComponentsInChildren<StageMinigameHud>(true))
            {
                var data = new SerializedObject(minigame);
                foreach (string field in new[] { "body", "fanPromptText" })
                {
                    var text = data.FindProperty(field).objectReferenceValue as TMP_Text;
                    if (text != null) { text.spriteAsset = sprites; EditorUtility.SetDirty(text); }
                }
            }
        }

        public static void Rebuild(ControlKeycapLibrary library)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
            var entries = (library.Keys ?? Array.Empty<ControlKeycap>()).Where(k => k != null && k.image != null).ToArray();
            if (entries.Any(k => string.IsNullOrWhiteSpace(k.key) || k.key.IndexOfAny(new[] { '"', '<', '>' }) >= 0) ||
                entries.Select(k => k.key).Distinct(StringComparer.OrdinalIgnoreCase).Count() != entries.Length)
                throw new InvalidOperationException("Key names must be unique and cannot contain rich-text delimiters.");
            var textures = new List<Texture2D>();
            var atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Rect[] rects;
            try
            {
                foreach (var entry in entries) textures.Add(Copy(entry.image));
                if (textures.Count == 0) { atlas.SetPixels(new Color[4]); atlas.Apply(); rects = Array.Empty<Rect>(); }
                else rects = atlas.PackTextures(textures.ToArray(), 4, 4096, false);
                File.WriteAllBytes(AtlasPath, atlas.EncodeToPNG());
                AssetDatabase.ImportAsset(AtlasPath);
                var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 4096;
                importer.SaveAndReimport();
                var asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(SpritePath);
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                    asset.name = "TMP_ControlKeycaps";
                    AssetDatabase.CreateAsset(asset, SpritePath);
                    asset.material = new Material(Shader.Find("TextMeshPro/Sprite")) { name = "ControlKeycaps Material" };
                    AssetDatabase.AddObjectToAsset(asset.material, asset);
                }
                var spriteData = new SerializedObject(asset);
                spriteData.FindProperty("m_Version").stringValue = "1.1.0";
                spriteData.ApplyModifiedPropertiesWithoutUndo();
                asset.spriteSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
                asset.material.mainTexture = asset.spriteSheet;
                asset.spriteGlyphTable.Clear();
                asset.spriteCharacterTable.Clear();
                for (int i = 0; i < entries.Length; i++)
                {
                    var r = rects[i];
                    int x = Mathf.RoundToInt(r.x * atlas.width), y = Mathf.RoundToInt(r.y * atlas.height);
                    int w = Mathf.RoundToInt(r.width * atlas.width), h = Mathf.RoundToInt(r.height * atlas.height);
                    // A uniform height preserves the long Space/Tab keycap aspect ratio.
                    var glyph = new TMP_SpriteGlyph((uint)i, new GlyphMetrics(w, h, 0, h * .85f, w), new GlyphRect(x, y, w, h), 1, 0);
                    asset.spriteGlyphTable.Add(glyph);
                    asset.spriteCharacterTable.Add(new TMP_SpriteCharacter(0xFFFE, asset, glyph) { name = entries[i].key, scale = 1 });
                }
                asset.UpdateLookupTables();
                EditorUtility.SetDirty(asset);
                EditorUtility.SetDirty(asset.material);
                var data = new SerializedObject(library);
                data.FindProperty("_spriteAsset").objectReferenceValue = asset;
                data.ApplyModifiedPropertiesWithoutUndo();
                library.Invalidate();
                EditorUtility.SetDirty(library);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(atlas);
                foreach (var texture in textures) UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static Texture2D Copy(Sprite sprite)
        {
            if (sprite.packed) throw new InvalidOperationException("Use the original imported Sprite, not a packed atlas Sprite.");
            var texture = sprite.texture;
            var previous = RenderTexture.active;
            var buffer = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            try
            {
                Graphics.Blit(texture, buffer);
                RenderTexture.active = buffer;
                var rect = sprite.rect;
                var copy = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
                copy.ReadPixels(rect, 0, 0);
                copy.Apply();
                return copy;
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(buffer); }
        }
    }

    [CustomEditor(typeof(ControlKeycapLibrary))]
    public sealed class ControlKeycapLibraryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Assign individual keycap Sprites, then rebuild. Empty entries keep their text. No artwork is added automatically.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("Rebuild Keycap Images")) ControlKeycapAuthoring.Rebuild((ControlKeycapLibrary)target);
        }
    }
}
