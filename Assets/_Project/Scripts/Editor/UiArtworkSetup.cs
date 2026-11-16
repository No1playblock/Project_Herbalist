using System;
using System.IO;
using System.Linq;
using Herbalist.GameUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Authoring only. All graphics created here are saved before entering Play mode.
    public static class UiArtworkSetup
    {
        private const string Prefabs = "Assets/_Project/Prefabs/UI/";
        private const string Placeholders = "Assets/_Project/Art/UI/Textures/ArtworkPlaceholders/";
        public const string DropFolder = "Assets/_Project/Art/UI/Textures/ArtistDrop/";
        private static readonly string[] Names = { "PF_OptionsPanel", "PF_LoadingPanel", "PF_MainMenuScreen", "PF_GameOverlayHud", "PF_PlayerHud" };

        [MenuItem("Herbalist/UI Art/Prepare Artwork Slots")]
        public static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play mode before preparing UI artwork.");
            Directory.CreateDirectory(Placeholders);
            Directory.CreateDirectory(DropFolder);
            AssetDatabase.Refresh();
            foreach (var name in Names)
            {
                string path = Prefabs + name + ".prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Configure(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            // These stages contain inline UI, rather than the common prefab instances.
            ConfigureScene("Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity");
            ConfigureScene(CurrentStagePaths.BranchRide);
            Debug.Log("UI artwork slots prepared. Open Herbalist/UI Art for the shared prefabs.");
        }

        private static void ConfigureScene(string path)
        {
            var scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (!opened && scene.isDirty)
                throw new InvalidOperationException("Save or close this scene first: " + path);
            var active = SceneManager.GetActiveScene();
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var hud in root.GetComponentsInChildren<PlayerHud>(true)) Configure(hud.gameObject);
                    foreach (var overlay in root.GetComponentsInChildren<GameOverlayHud>(true)) Configure(overlay.gameObject);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (active.IsValid()) SceneManager.SetActiveScene(active);
                if (opened) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static bool IsNested(Transform item, Transform root)
        {
            var nearest = PrefabUtility.GetNearestPrefabInstanceRoot(item.gameObject);
            return nearest != null && nearest != root.gameObject;
        }

        private static void Configure(GameObject root)
        {
            foreach (var image in root.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (IsNested(image.transform, root.transform)) continue;
                if (image.name == "Art_Reticle" && image.sprite == null) image.sprite = Circle();
                if (image.type == UnityEngine.UI.Image.Type.Filled && image.sprite == null)
                {
                    image.sprite = Solid(image.color);
                    image.color = Color.white;
                }
            }
            if (root.GetComponent<MainMenuScreen>() != null)
            {
                Portrait(root.transform.Find("CharacterRoom/DuyeongCard"), "Duyeong");
                Portrait(root.transform.Find("CharacterRoom/SodamCard"), "Sodam");
            }
            foreach (var original in root.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (IsNested(original.transform, root.transform) || original.name.StartsWith("Art_") ||
                    !original.enabled || original.transform.GetComponentsInParent<Transform>(true).Any(t => t.name.StartsWith("Art_"))) continue;
                if (!original.gameObject.activeSelf && original.transform.parent != null &&
                    (original.transform.parent.Find("Art_DuyeongPortrait") != null || original.transform.parent.Find("Art_SodamPortrait") != null)) continue;
                // Invisible hit surfaces/containers are not artwork slots.
                if (original.color.a <= 0) continue;
                bool stateTint = original.name.StartsWith("LeafSlot") && original.name.EndsWith("Dot");
                // Keep state-driven Image references and filled gauges on their original component.
                if (stateTint || original.type == UnityEngine.UI.Image.Type.Filled)
                {
                    original.name = "Art_" + (stateTint ? "StateTint_" : "Fill_") + original.name;
                    continue;
                }
                var art = NewArt(original.transform, "Art_" + original.name);
                art.sprite = original.sprite != null ? original.sprite : Solid(original.color);
                art.color = original.sprite != null ? original.color : Color.white;
                art.type = original.type;
                art.preserveAspect = original.preserveAspect;
                art.fillCenter = original.fillCenter;
                art.pixelsPerUnitMultiplier = original.pixelsPerUnitMultiplier;
                art.material = original.material;
                art.raycastTarget = original.raycastTarget;
                art.maskable = original.maskable;
                // Preserve click/slider focus feedback on the graphic the artist will replace.
                foreach (var selectable in root.GetComponentsInChildren<UnityEngine.UI.Selectable>(true))
                    if (selectable.targetGraphic == original) selectable.targetGraphic = art;
                original.enabled = false;
            }
            foreach (var raw in root.GetComponentsInChildren<UnityEngine.UI.RawImage>(true))
                if (!IsNested(raw.transform, root.transform) && raw.texture != null && !raw.name.StartsWith("Art_"))
                    raw.name = "Art_Texture_" + raw.name;
            foreach (var hud in root.GetComponentsInChildren<PlayerHud>(true))
            {
                if (hud.reticle == null || hud.reticle.transform.Find("Art_Reticle") != null) continue;
                var art = NewArt(hud.reticle.transform, "Art_Reticle");
                art.rectTransform.anchorMin = art.rectTransform.anchorMax = new Vector2(.5f, .5f);
                art.rectTransform.sizeDelta = new Vector2(4, 4);
                art.sprite = Circle();
                art.preserveAspect = true;
                var text = hud.reticle.GetComponent<TMPro.TMP_Text>();
                if (text != null) text.enabled = false;
            }
        }

        private static UnityEngine.UI.Image NewArt(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<UnityEngine.UI.Image>();
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.SetAsFirstSibling();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private static Sprite Solid(Color color)
        {
            string name = "T_UIPlaceholder_" + ColorUtility.ToHtmlStringRGBA(color);
            return SaveSprite(name, 2, 2, (x, y) => color);
        }

        private static Sprite Circle() => SaveSprite("T_UIPlaceholder_Reticle", 32, 32,
            (x, y) => Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(16, 16)) <= 15 ? Color.white : Color.clear);

        private static void Portrait(Transform card, string role)
        {
            if (card == null || card.Find("Art_" + role + "Portrait") != null) return;
            var parts = new[] { "Head", "Body", "LeftArm", "RightArm", "LeftLeg", "RightLeg" }
                .Select(n => card.Find(n)).Where(t => t != null).ToArray();
            if (parts.Length == 0) return;
            var art = NewArt(card, "Art_" + role + "Portrait");
            art.rectTransform.anchorMin = art.rectTransform.anchorMax = new Vector2(.5f, .5f);
            art.rectTransform.sizeDelta = new Vector2(160, 220);
            art.preserveAspect = true;
            art.sprite = SaveSprite("T_UIPlaceholder_" + role + "Portrait", 320, 440, (x, y) =>
            {
                var point = new Vector2((x + .5f) / 2 - 80, (y + .5f) / 2 - 110);
                Color color = Color.clear;
                foreach (var part in parts)
                {
                    var rect = (RectTransform)part;
                    var p = point - rect.anchoredPosition;
                    var size = rect.sizeDelta;
                    bool inside = Mathf.Abs(p.x) <= size.x / 2 && Mathf.Abs(p.y) <= size.y / 2;
                    if (part.name == "Head") inside = p.sqrMagnitude <= size.x * size.x / 4;
                    if (inside) color = part.GetComponent<UnityEngine.UI.Image>().color;
                }
                return color;
            });
            foreach (var part in parts) part.gameObject.SetActive(false);
        }

        private static Sprite SaveSprite(string name, int width, int height, Func<int, int, Color> pixel)
        {
            string path = Placeholders + name + ".png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                var colors = new Color[width * height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++) colors[y * width + x] = pixel(x, y);
                texture.SetPixels(colors);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.sRGBTexture = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            if (sprite == null) throw new InvalidOperationException("Sprite import failed: " + path);
            return sprite;
        }

        [MenuItem("Herbalist/UI Art/01 Main Menu")]
        public static void OpenMenu() => Open("PF_MainMenuScreen");
        [MenuItem("Herbalist/UI Art/02 Player HUD")]
        public static void OpenHud() => Open("PF_PlayerHud");
        [MenuItem("Herbalist/UI Art/03 Pause and Stage Header")]
        public static void OpenOverlay() => Open("PF_GameOverlayHud");
        [MenuItem("Herbalist/UI Art/04 Options")]
        public static void OpenOptions() => Open("PF_OptionsPanel");
        [MenuItem("Herbalist/UI Art/05 Loading")]
        public static void OpenLoading() => Open("PF_LoadingPanel");
        private static void Open(string name) => AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab"));
    }

    // New files in the artist drop folder are immediately usable in Source Image.
    public sealed class UiArtworkImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UiArtworkSetup.DropFolder, StringComparison.Ordinal) || !assetImporter.importSettingsMissing) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
        }
    }
}
