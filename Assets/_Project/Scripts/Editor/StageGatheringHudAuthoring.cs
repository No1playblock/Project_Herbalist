using System;
using Herbalist.GameUI;
using Herbalist.StageOne;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Herbalist.Editor
{
    internal static class StageGatheringHudAuthoring
    {
        private const string Project = "Assets/_Project/";
        private const string Art = Project + "Art/UI/Textures/Gathering/T_Gather_";
        private const string HudPath = Project + "Prefabs/UI/PF_PlayerHud.prefab";
        private const string CodexData = Project + "Data/UI/Gathering";
        private static readonly Color Ink = new(.16f, .20f, .13f);
        private static readonly Color SoftInk = new(.27f, .29f, .20f);
        private static readonly Color Rule = new(.38f, .34f, .22f, .35f);
        private static readonly Color Gold = new(.94f, .75f, .40f);
        private static readonly Color Paper = new(1f, .96f, .83f);
        private static readonly Color Dark = new(.045f, .083f, .055f, .94f);
        private static TMP_FontAsset _heading;
        private static TMP_FontAsset _body;

        [MenuItem("Herbalist/Authoring/Apply Stage One Gathering HUD")]
        public static void Run()
        {
            _heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Project + "Fonts/Diphylleia/DiphylleiaSdf.asset");
            _body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Project + "Fonts/Obangju/ObangjuDynamicSdf.asset");
            if (_heading == null || _body == null) throw new InvalidOperationException("Gathering HUD fonts are missing.");
            var copy = AssetDatabase.LoadAssetAtPath<StageMinigameUiText>(Project + "Data/UI/SO_StageMinigameUiText.asset");
            copy.gatherDuyeongInputHint = "약초의 생김새를 소담에게 설명하고, 들은 순서대로 W · A · S · D를 누르세요.";
            copy.gatherCompleteText = "채집을 마쳤습니다.";
            copy.gatherSodamIntro = "두영의 설명을 듣고 일치하는 약초를 고르세요.";
            EditorUtility.SetDirty(copy);
            if (!AssetDatabase.IsValidFolder(CodexData)) AssetDatabase.CreateFolder(Project + "Data/UI", "Gathering");

            var entries = new[]
            {
                CodexHerb("DewFern", "이슬고사리", "깃털 같은 잎 끝에 이슬이 맺히고 어린순은 둥글게 말린다.",
                    "바깥쪽 잎부터 번갈아 젖혀 가운데 순을 남기세요.", "AWDSAWDSAW"),
                PlayableHerb("LeafHerb", "가늘게 갈라진 줄기에 길고 뾰족한 초록 잎이 난다.",
                    "잎 끝부터 줄기 쪽으로 흙을 고르게 풀고 뿌리를 들어 올리세요."),
                CodexHerb("MoonBell", "달빛방울꽃", "옅은 푸른 종 모양 꽃이 아래를 향해 달린다.",
                    "꽃이 흔들리지 않도록 줄기 아래를 받치고 좌우 흙을 풀어 주세요.", "DWAASDWASD"),
                CodexHerb("CrimsonKnot", "붉은매듭풀", "자주색 줄기 끝에 붉은 열매가 촘촘히 맺힌다.",
                    "열매는 건드리지 말고 매듭진 뿌리 주변의 흙을 걷어 내세요.", "SAWDDSAWSD"),
                PlayableHerb("SapHerb", "둥근 잎 사이에 노란 꽃이 피고 줄기에 황금빛 수액이 맺힌다.",
                    "줄기 밑동을 잡고 오른쪽부터 둘레의 흙을 풀어 주세요."),
                CodexHerb("SilverMoss", "은별이끼", "작은 은회색 별 모양 잎이 낮고 둥근 덩어리를 이룬다.",
                    "가장자리부터 흙째 얇게 떠서 잎이 흩어지지 않게 들어 올리세요.", "SDAWASDAWS")
            };

            var prefab = PrefabUtility.LoadPrefabContents(HudPath);
            try
            {
                var hud = prefab.GetComponent<StageMinigameHud>();
                var data = new SerializedObject(hud);
                var view = prefab.GetComponent<StageGatheringView>();
                if (view != null)
                {
                    var previous = new SerializedObject(view);
                    foreach (var field in new[] { "_duyeongRoot", "_codexRoot" })
                        if (previous.FindProperty(field).objectReferenceValue is GameObject root)
                            UnityEngine.Object.DestroyImmediate(root);
                }
                else view = prefab.AddComponent<StageGatheringView>();

                BuildPull(prefab.transform, view, copy);
                BuildCodex(prefab.transform, view, entries, copy);
                data.FindProperty("_gatheringView").objectReferenceValue = view;
                data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefab, HudPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            AssetDatabase.SaveAssetIfDirty(copy);
            foreach (var entry in entries) AssetDatabase.SaveAssetIfDirty(entry);
            Debug.Log("Stage One gathering HUD authored with 6 codex entries and 2 × 10 pull frames.");
        }

        private static StageItemDefinition PlayableHerb(string name, string description, string method)
        {
            var herb = AssetDatabase.LoadAssetAtPath<StageItemDefinition>(Project + "Data/Items/SO_" + name + ".asset");
            herb.codexImage = Sprite(Art + name + ".png", 512);
            herb.appearance = description;
            herb.harvestingMethod = method;
            herb.plantedImage = Sprite(Art + name + "_00.png", 512);
            herb.gatheringFrames = new Sprite[herb.harvestingSteps.Length];
            for (int i = 0; i < herb.gatheringFrames.Length; i++)
                herb.gatheringFrames[i] = Sprite(Art + name + "_" + (i + 1).ToString("00") + ".png", 512);
            EditorUtility.SetDirty(herb);
            return herb;
        }

        private static StageItemDefinition CodexHerb(string id, string name, string description, string method, string sequence)
        {
            var path = CodexData + "/SO_Codex" + id + ".asset";
            var herb = AssetDatabase.LoadAssetAtPath<StageItemDefinition>(path);
            if (herb == null)
            {
                herb = ScriptableObject.CreateInstance<StageItemDefinition>();
                AssetDatabase.CreateAsset(herb, path);
            }
            herb.contentId = "CODEX_" + id.ToUpperInvariant();
            herb.displayName = name;
            herb.codexImage = Sprite(Art + id + ".png", 512);
            herb.appearance = description;
            herb.harvestingMethod = method;
            herb.harvestingSteps = Array.ConvertAll(sequence.ToCharArray(), key => (StageDirection)"WASD".IndexOf(key));
            EditorUtility.SetDirty(herb);
            return herb;
        }

        private static void BuildPull(Transform parent, StageGatheringView view, StageMinigameUiText copy)
        {
            var root = Rect(parent, "DuyeongGatheringHud", 0, 0, 1, 1);
            Image(root, "Backdrop", 0, 0, 1, 1, Dark);
            var stage = Fit(root, "ForestCloseUp");
            Image(stage, "ForestFloor", 0, 0, 1, 1, Color.white, Sprite(Art + "GatherBackground.png", 2048));
            var plant = Image(stage, "HerbPullFrame", .255f, .122f, .745f, .992f, Color.white);
            plant.preserveAspect = true;
            Image(stage, "TitlePlate", .35f, .875f, .65f, .976f, Dark);
            Image(stage, "TitleRule", .385f, .889f, .615f, .892f, Gold);
            Label(stage, "Title", .36f, .897f, .64f, .967f, copy.gatherDuyeongTitle, 46, Paper, true);
            var strip = Image(stage, "ProgressPlate", .19f, .028f, .81f, .24f, Dark).rectTransform;
            Image(strip, "UpperRule", .02f, .975f, .98f, .985f, Gold);
            var progress = Label(strip, "Progress", .05f, .72f, .95f, .94f,
                string.Format(copy.gatherProgressFormat, 0), 31, Gold, true);
            var hint = Label(strip, "InputHint", .025f, .065f, .975f, .30f, copy.gatherDuyeongInputHint, 27, Paper);
            var fills = new GameObject[10];
            var labels = new TMP_Text[10];
            var dot = AssetDatabase.LoadAssetAtPath<Sprite>(Project + "Art/UI/Textures/T_LeafStatusDot.png")
                ?? Array.Find(AssetDatabase.LoadAllAssetsAtPath(Project + "Art/UI/Textures/T_LeafStatusDot.png"), asset => asset is Sprite) as Sprite;
            if (dot == null) throw new InvalidOperationException("Gathering progress dot sprite is missing.");
            for (int i = 0; i < fills.Length; i++)
            {
                float x = .106f + .0875f * i;
                var step = Rect(strip, $"Step{i + 1:00}", x - .025f, .37f, x + .025f, .68f);
                Image(step, "EmptyDot", 0, 0, 1, 1, new Color(.94f, .86f, .70f, .44f), dot).preserveAspect = true;
                var fill = Image(step, "CompletedDot", 0, 0, 1, 1, Gold, dot);
                fill.preserveAspect = true;
                labels[i] = Label(fill.rectTransform, "AcceptedKey", 0, 0, 1, 1, "", 27, Ink, false, TextAlignmentOptions.Center);
                fills[i] = fill.gameObject;
                fills[i].SetActive(false);
            }
            var so = new SerializedObject(view);
            Ref(so, "_duyeongRoot", root.gameObject); Ref(so, "_plant", plant);
            Ref(so, "_progress", progress); Ref(so, "_inputHint", hint);
            Refs(so, "_stepFills", fills); Refs(so, "_stepLabels", labels);
            so.ApplyModifiedPropertiesWithoutUndo();
            root.gameObject.SetActive(false);
        }

        private static void BuildCodex(Transform parent, StageGatheringView view, StageItemDefinition[] entries, StageMinigameUiText copy)
        {
            var root = Rect(parent, "SodamHerbCodex", 0, 0, 1, 1);
            root.gameObject.AddComponent<Canvas>();
            root.gameObject.AddComponent<GraphicRaycaster>();
            Image(root, "Backdrop", 0, 0, 1, 1, new Color(.04f, .075f, .05f, .99f)).raycastTarget = true;
            var fit = Fit(root, "BookFit");
            var book = Image(fit, "CodexBackgroundPanel", .045f, .045f, .955f, .955f, Color.white,
                Sprite(Art + "CodexPanel.png", 2048)).rectTransform;
            Label(book, "Title", .09f, .859f, .459f, .944f, copy.gatherSodamTitle, 54, Ink, true, TextAlignmentOptions.MidlineLeft);
            Label(book, "Instruction", .09f, .804f, .463f, .859f, copy.gatherSodamIntro, 31, SoftInk, false, TextAlignmentOptions.MidlineLeft);
            Image(book, "IndexRule", .064f, .786f, .465f, .79f, Rule);
            var so = new SerializedObject(view);
            Ref(so, "_codexRoot", root.gameObject);
            var rows = so.FindProperty("_rows");
            rows.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                float top = .776f - i * .108f;
                var background = Image(book, $"Entry{i + 1:00}", .065f, top - .098f, .465f, top, new Color(.44f, .47f, .28f, .075f));
                background.raycastTarget = true;
                var row = background.rectTransform;
                var button = row.gameObject.AddComponent<Button>();
                button.targetGraphic = background;
                var colors = button.colors;
                colors.normalColor = Color.white; colors.highlightedColor = new Color(.74f, .77f, .45f);
                colors.selectedColor = colors.highlightedColor; colors.pressedColor = new Color(.62f, .65f, .34f);
                button.colors = colors;
                var selected = Image(row, "SelectedEntry", 0, 0, 1, 1, new Color(.43f, .51f, .29f, .20f));
                Image(selected.rectTransform, "IndexRibbon", 0, 0, .012f, 1, new Color(.32f, .42f, .21f));
                selected.gameObject.SetActive(false);
                Image(row, "SpecimenThumbnail", .016f, .035f, .178f, .965f, Color.white, entry.codexImage).preserveAspect = true;
                Label(row, "Name", .20f, .51f, .963f, .96f, entry.displayName, 37, Ink, true, TextAlignmentOptions.MidlineLeft);
                Label(row, "Description", .20f, .04f, .963f, .51f, entry.appearance, 25, SoftInk, false, TextAlignmentOptions.MidlineLeft);
                var serializedRow = rows.GetArrayElementAtIndex(i);
                serializedRow.FindPropertyRelative("herb").objectReferenceValue = entry;
                serializedRow.FindPropertyRelative("button").objectReferenceValue = button;
                serializedRow.FindPropertyRelative("selection").objectReferenceValue = selected.gameObject;
            }
            Label(book, "Folio", .065f, .065f, .465f, .112f, "01 — 06", 23, SoftInk);
            var portrait = Image(book, "SpecimenIllustration", .583f, .521f, .909f, .927f, Color.white);
            portrait.preserveAspect = true;
            var name = Label(book, "SpecimenName", .55f, .45f, .94f, .526f, "", 48, Ink, true);
            var description = Label(book, "SpecimenDescription", .551f, .389f, .94f, .452f, "", 29, SoftInk);
            Image(book, "MethodRule", .558f, .378f, .934f, .382f, Rule);
            Label(book, "MethodTitle", .56f, .312f, .932f, .371f, "채집 방법", 36, Ink, true);
            var method = Label(book, "MethodDescription", .551f, .258f, .941f, .317f, "", 28, SoftInk);
            var keys = new TMP_Text[10];
            for (int i = 0; i < keys.Length; i++)
            {
                float x = .589f + i % 5 * .069f;
                float y = i < 5 ? .183f : .103f;
                var key = Image(book, $"Direction{i + 1:00}", x, y, x + .04f, y + .059f, Dark);
                keys[i] = Label(key.rectTransform, "Key", 0, 0, 1, 1, "", 33, Paper);
                if (i % 5 != 4) Label(book, "Next", x + .043f, y, x + .066f, y + .059f, "›", 31, SoftInk);
            }
            Label(book, "RelayInstruction", .55f, .051f, .94f, .098f, copy.gatherSodamProgress, 24, SoftInk);
            Ref(so, "_codexPortrait", portrait); Ref(so, "_codexName", name);
            Ref(so, "_codexDescription", description); Ref(so, "_codexMethod", method); Refs(so, "_codexKeys", keys);
            so.ApplyModifiedPropertiesWithoutUndo();
            root.gameObject.SetActive(false);
        }

        private static RectTransform Rect(Transform parent, string name, float x0, float y0, float x1, float y1)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.gameObject.layer = parent.gameObject.layer;
            rect.anchorMin = new Vector2(x0, y0);
            rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }
        private static RectTransform Fit(Transform parent, string name)
        {
            var rect = Rect(parent, name, 0, 0, 1, 1);
            var fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 16f / 9f;
            return rect;
        }
        private static UnityEngine.UI.Image Image(Transform parent, string name, float x0, float y0, float x1, float y1, Color color, Sprite sprite = null)
        {
            var image = Rect(parent, name, x0, y0, x1, y1).gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
        private static TextMeshProUGUI Label(Transform parent, string name, float x0, float y0, float x1, float y1,
            string value, float size, Color color, bool heading = false, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var label = Rect(parent, name, x0, y0, x1, y1).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = heading ? _heading : _body;
            label.text = value;
            label.fontSize = label.fontSizeMax = size;
            label.fontSizeMin = size * .45f;
            label.enableAutoSizing = true;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }
        private static Sprite Sprite(string path, int size)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing gathering artwork: " + path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
                importer.maxTextureSize != size || importer.mipmapEnabled || !importer.alphaIsTransparency)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.maxTextureSize = size;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? throw new InvalidOperationException("Sprite import failed: " + path);
        }
        private static void Ref(SerializedObject so, string field, UnityEngine.Object value) =>
            so.FindProperty(field).objectReferenceValue = value;
        private static void Refs(SerializedObject so, string field, UnityEngine.Object[] values)
        {
            var p = so.FindProperty(field);
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
