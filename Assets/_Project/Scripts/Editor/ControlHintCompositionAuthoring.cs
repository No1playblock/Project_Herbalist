using System;
using System.Linq;
using Herbalist.GameUI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Herbalist.Editor
{
    public static class ControlHintCompositionAuthoring
    {
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play mode first.");
            var active = SceneManager.GetActiveScene();
            if (active.isDirty) throw new InvalidOperationException("Save the active scene first.");
            string prefabPath = "Assets/_Project/Prefabs/UI/PF_PlayerHud.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try { Configure(prefab.GetComponent<PlayerHud>()); PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath); }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            foreach (string path in new[] { "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity", CurrentStagePaths.BranchRide })
            {
                var scene = SceneManager.GetSceneByPath(path);
                bool opened = !scene.IsValid() || !scene.isLoaded;
                if (!opened && scene.isDirty) throw new InvalidOperationException("Save scene first: " + path);
                if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                        foreach (var hud in root.GetComponentsInChildren<PlayerHud>(true)) Configure(hud);
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

        private static void Configure(PlayerHud hud)
        {
            if (hud.helpPanel.GetComponentInChildren<ControlHelpView>(true) != null)
                throw new InvalidOperationException("This HUD already uses composed control hints: " + hud.name);
            foreach (var text in hud.GetComponentsInChildren<TMP_Text>(true))
                if (text.spriteAsset != null && text.spriteAsset.name == "TMP_ControlKeycaps") text.spriteAsset = null;
            var data = new SerializedObject(hud);
            var panelRect = (RectTransform)hud.helpPanel.transform;
            panelRect.sizeDelta = new Vector2(440, 480);
            var helpContainer = hud.helpText.rectTransform;
            Stretch(helpContainer);
            helpContainer.offsetMin = new Vector2(14, 14);
            helpContainer.offsetMax = new Vector2(-14, -14);
            hud.helpText.enabled = false;
            var list = NewRect("ControlRows", helpContainer);
            Stretch(list);
            var vertical = list.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vertical.spacing = 2;
            vertical.childControlWidth = vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;
            var help = list.gameObject.AddComponent<ControlHelpView>();
            var helpData = new SerializedObject(help);
            var rows = helpData.FindProperty("_rows");
            rows.arraySize = hud.settings.controls.Length;
            for (int i = 0; i < hud.settings.controls.Length; i++)
            {
                var hint = hud.settings.controls[i];
                string binding = hint.action != null ? hint.action.action.GetBindingDisplayString() : string.Empty;
                var row = NewRect("Control_" + i.ToString("00") + "_" + (hint.action != null ? hint.action.action.name : "Unassigned"), list);
                var layout = row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                layout.minHeight = layout.preferredHeight = 32;
                var view = Build(row, hud.helpText, binding, hint.label, 120, 14);
                rows.GetArrayElementAtIndex(i).objectReferenceValue = view;
            }
            helpData.ApplyModifiedPropertiesWithoutUndo();
            data.FindProperty("_helpControls").objectReferenceValue = help;
            data.FindProperty("_helpHintView").objectReferenceValue = Convert(hud.helpHint, hud.settings.helpAction.action.GetBindingDisplayString(), hud.settings.helpHint, 32);
            data.FindProperty("_interactionHintView").objectReferenceValue = Convert(hud.promptText, "E", hud.settings.gatherText, 30);
            data.FindProperty("_clothHintView").objectReferenceValue = Convert(hud.clothPromptText, "E/Space", hud.settings.clothReleaseText, 104);
            data.FindProperty("_sapHintView").objectReferenceValue = Convert(hud.sapPromptText, "R", hud.settings.sapInjectText, 30);
            data.ApplyModifiedPropertiesWithoutUndo();
            ResizePrompt(hud.prompt, hud.promptText, 240);
            ResizePrompt(hud.clothPrompt, hud.clothPromptText, 280);
            ResizePrompt(hud.sapPrompt, hud.sapPromptText, 320);
            var minigame = hud.GetComponent<StageMinigameHud>();
            if (minigame != null)
            {
                var miniData = new SerializedObject(minigame);
                var fanText = miniData.FindProperty("fanPromptText").objectReferenceValue as TMP_Text;
                if (fanText != null) miniData.FindProperty("_fanHint").objectReferenceValue = Convert(fanText, "Space", "부채질하기", 76);
                miniData.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(hud);
        }

        private static void ResizePrompt(GameObject prompt, TMP_Text text, float width)
        {
            if (prompt == null || text == null) return;
            var rect = (RectTransform)prompt.transform;
            rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(6, 1);
            text.rectTransform.offsetMax = new Vector2(-6, -1);
        }

        private static ControlHintView Convert(TMP_Text container, string keys, string description, float keyWidth)
        {
            if (container == null) return null;
            var row = NewRect("ControlHint", container.transform);
            Stretch(row);
            var view = Build(row, container, keys, description, keyWidth, container.fontSize);
            container.enabled = false;
            return view;
        }

        private static ControlHintView Build(RectTransform row, TMP_Text template, string binding, string description, float keyWidth, float fontSize)
        {
            var horizontal = row.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            horizontal.spacing = 8;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = horizontal.childForceExpandHeight = false;
            var keys = NewRect("Keycaps", row);
            var keyLayout = keys.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            keyLayout.minWidth = keyLayout.preferredWidth = keyWidth;
            keyLayout.preferredHeight = 28;
            var group = keys.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            group.spacing = 4;
            group.childAlignment = TextAnchor.MiddleLeft;
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = group.childForceExpandHeight = false;
            var desc = NewText("Description", row, template, fontSize);
            desc.text = description;
            desc.textWrappingMode = TextWrappingModes.Normal;
            var descLayout = desc.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            descLayout.flexibleWidth = 1;
            descLayout.minWidth = 0;
            descLayout.preferredHeight = 32;
            var view = row.gameObject.AddComponent<ControlHintView>();
            var data = new SerializedObject(view);
            data.FindProperty("_description").objectReferenceValue = desc;
            data.FindProperty("_previewBinding").stringValue = binding;
            data.FindProperty("_previewDescription").stringValue = description;
            var parts = binding.Split('/');
            var slots = data.FindProperty("_keys");
            slots.arraySize = parts.Length;
            for (int i = 0; i < parts.Length; i++)
            {
                string key = parts[i].Trim();
                var slot = NewRect("Key_" + key.Replace(' ', '_'), keys);
                var layout = slot.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                layout.minWidth = layout.preferredWidth = key == "Space" ? 72 : key.Length > 1 ? 40 : 24;
                layout.preferredHeight = 26;
                var imageRect = NewRect("Art_Keycap", slot);
                Stretch(imageRect);
                var image = imageRect.gameObject.AddComponent<UnityEngine.UI.Image>();
                image.raycastTarget = false;
                image.preserveAspect = true;
                var fallback = NewText("KeyTextFallback", slot, template, fontSize);
                Stretch(fallback.rectTransform);
                fallback.alignment = TextAlignmentOptions.Center;
                fallback.textWrappingMode = TextWrappingModes.NoWrap;
                fallback.text = key;
                if (parts.Length == 2 && i == 1)
                {
                    var separator = NewText("AlternativeSeparator", slot, template, fontSize);
                    separator.text = "/";
                    separator.alignment = TextAlignmentOptions.Center;
                    separator.rectTransform.anchorMin = separator.rectTransform.anchorMax = new Vector2(0, .5f);
                    separator.rectTransform.sizeDelta = new Vector2(8, 26);
                    separator.rectTransform.anchoredPosition = new Vector2(-2, 0);
                }
                var entry = slots.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("binding").stringValue = key;
                entry.FindPropertyRelative("root").objectReferenceValue = slot.gameObject;
                entry.FindPropertyRelative("image").objectReferenceValue = image;
                entry.FindPropertyRelative("fallback").objectReferenceValue = fallback;
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            view.Present(binding, description);
            return view;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static TMP_Text NewText(string name, Transform parent, TMP_Text template, float fontSize)
        {
            var rect = NewRect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = template.font;
            text.fontSharedMaterial = template.fontSharedMaterial;
            text.fontSize = fontSize;
            text.color = template.color;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
