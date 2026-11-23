using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Levels;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class PrototypeRevision1117Authoring
    {
        private static Transform[] _transforms;
        private static Scene _scene;
        private static void Open(string path)
        {
            var active = SceneManager.GetActiveScene();
            if (active.isDirty)
                throw new InvalidOperationException("Save the open scene before applying the revision.");
            System.IO.Directory.CreateDirectory("Temp/PrototypeRevision1117/Backup");
            string backup = "Temp/PrototypeRevision1117/Backup/" + System.IO.Path.GetFileName(path);
            if (!System.IO.File.Exists(backup))
                System.IO.File.Copy(path, backup);
            _scene = active.path == path ? active : EditorSceneManager.OpenScene(path);
            _transforms = _scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        }

        private static void Save()
        {
            EditorSceneManager.MarkSceneDirty(_scene);
            if (!EditorSceneManager.SaveScene(_scene))
                throw new InvalidOperationException("Scene save failed.");
            AssetDatabase.SaveAssets();
        }

        private static Transform Named(string name) => _transforms.Single(t => t.name == name);
        private static T Read<T>(Object target, string field)
            where T : Object => new SerializedObject(target).FindProperty(field).objectReferenceValue as T;
        private static void Set(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Number(Object target, string field, float value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Flag(Object target, string field, bool value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Vector(Object target, string field, Vector3 value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).vector3Value = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Array(Object target, string field, IEnumerable<Object> values)
        {
            var serialized = new SerializedObject(target);
            var array = serialized.FindProperty(field);
            var objects = values.ToArray();
            array.arraySize = objects.Length;
            for (int i = 0; i < objects.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static string StageOne()
        {
            Open(CurrentStagePaths.StageOne);
            var branch31 = Named("branch1_low (31)");
            branch31.localPosition = new Vector3(-31.31f, -108.8f, 46.42f);
            branch31.localEulerAngles = new Vector3(3.104f, 53.59f, 35.99f);
            branch31.localScale = Vector3.one * .002f;
            var branch32 = Named("branch1_low (32)");
            branch32.localPosition = new Vector3(2.570f, -96.43f, 46f);
            branch32.localEulerAngles = new Vector3(-.421f, -80.8f, 21.527f);
            branch32.localScale = Vector3.one * .002f;
            var branch24 = Named("branch1_low (24)");
            branch24.localPosition = new Vector3(-50.2f, -48.88f, 36.32f);
            branch24.localEulerAngles = new Vector3(-5.474f, -242.2f, 36.64f);
            branch24.localScale = Vector3.one * .002f;
            var branch25 = Named("branch1_low (25)");
            branch25.localPosition = new Vector3(-61.62f, -43.51f, 31.30f);
            branch25.localEulerAngles = branch24.localEulerAngles;
            branch25.localScale = branch24.localScale;
            LeafPose("LeafBranch_31", new Vector3(277.52f, 49.17f, 293.68f), new Vector3(-81.92f, 0, -101.13f), 8.062f);
            LeafPose("LeafBranch_32", new Vector3(328.76f, 62.046f, 289.68f), new Vector3(-81.92f, 0, -235.6f), 8.062f);
            LeafPose("LeafBranch_24", new Vector3(260.59f, 107.41f, 295.43f), new Vector3(-81.92f, 0, -396.5f), 10.556f);
            LeafPose("LeafBranch_25", new Vector3(249.17f, 112.71f, 290.41f), new Vector3(-81.92f, 0, -396.5f), 10.556f);
            foreach (var mechanism in _transforms.Select(t => t.GetComponent<StageOneDocumentMechanism>()).Where(m => m != null))
                if (mechanism.Kind == StageOneDocumentMechanismKind.FallingBranch)
                {
                    Number(mechanism, "_branchResetSeconds", 5f);
                    var target=Read<LeafInstallTarget>(mechanism,"_leafTarget");
                    var field=typeof(LeafInstallTarget).GetField("onReleased",
                        System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                    var released=(UnityEngine.Events.UnityEvent)field.GetValue(target);
                    bool connected=Enumerable.Range(0,released.GetPersistentEventCount()).Any(i=>
                        released.GetPersistentTarget(i)==mechanism&&released.GetPersistentMethodName(i)=="DropAfterLeafRecall");
                    if(!connected)UnityEditor.Events.UnityEventTools.AddPersistentListener(released,mechanism.DropAfterLeafRecall);
                    EditorUtility.SetDirty(target);
                }
            var pads = _transforms.Select(t => t.GetComponent<ExteriorJumpPad>()).Where(p => p != null && p.enabled && p.gameObject.activeInHierarchy).ToArray();
            foreach (var pad in pads)
                Array(pad, "_readyVisuals", pad.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("CuredBlueSap")).Select(t => t.gameObject));
            var plugs = _transforms.Select(t => t.GetComponent<StageOneFixedWaterStream>()).Where(s => s != null && s.isActiveAndEnabled && Read<Collider>(s, "_coveredMushroomCap") != null).OrderBy(s => s.transform.position.y).ToArray();
            Vector3[] plugPositions = {new(294.3f, 57.91f, 323.36f), new(302.28f, 61.02f, 324.93f), new(313.03f, 64.34f, 325.99f)};
            Vector3[] plugRotations = {new(-10.25f, -19.05f, -539.3f), new(-1.619f, -19.81f, -526.9f), new(-4.818f, 9.512f, -528.4f)};
            for (int i = 0; i < Mathf.Min(3, plugs.Length); i++)
            {
                var mechanism = plugs[i].GetComponentInParent<StageOneDocumentMechanism>();
                var target = Read<LeafInstallTarget>(mechanism, "_leafTarget");
                SetLeafPose(target, plugPositions[i], plugRotations[i], 5.6582f);
            }

            var clothBranch = Named("branch1_low (26)");
            clothBranch.localPosition = new Vector3(-61.51f, -14.72f, 43.42f);
            clothBranch.localEulerAngles = new Vector3(-16.78f, 44.48f, -96.2f);
            clothBranch.localScale = Vector3.one * .008f;
            var waterCloth = _transforms.Select(t => t.GetComponent<HangingCloth>()).First(c => c != null && c.name == "Coop_5_WaterCloth");
            var clothArt = Read<Transform>(waterCloth, "_externalVisualRoot");
            var knot = clothArt.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "knot_low");
            var ring = clothArt.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ring_low");
            if (knot != null)
                knot.localPosition = new Vector3(-.011f, -.372f, -.022f);
            if (ring != null)
            {
                ring.localPosition = new Vector3(.014f, -.542f, -.003f);
                ring.localScale = Vector3.one * 1.1945f;
            }

            var deck = Named("04_Musroom (31)");
            deck.localPosition = new Vector3(253.2f, 115.54f, 253.2f);
            deck.localEulerAngles = new Vector3(-14.02f, 589.58f, -25.05f);
            deck.localScale = Vector3.one * .705f;
            var shiftedMushroom = Named("03_Mushroom (11)");
            shiftedMushroom.localPosition = new Vector3(320.8f, 25.21f, 271.86f);
            shiftedMushroom.localEulerAngles = new Vector3(.728f, 16.311f, 12.223f);
            shiftedMushroom.localScale = Vector3.one * .8514f;
            Named("03_Mushroom (10)").gameObject.SetActive(false);
            foreach (var root in _scene.GetRootGameObjects().Where(r => r.name.StartsWith("TSI_") && r.name.Contains("Tree") && !r.name.Contains("Branch")))
            {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                    continue;
                var bounds = renderers[0].bounds;
                foreach (var r in renderers)
                    bounds.Encapsulate(r.bounds);
                foreach (var old in root.GetComponentsInChildren<MeshCollider>(true))
                    old.enabled = false;
                var capsule = root.GetComponent<CapsuleCollider>();
                if (capsule == null)
                    capsule = root.AddComponent<CapsuleCollider>();
                var size = root.transform.InverseTransformVector(bounds.size);
                capsule.center = root.transform.InverseTransformPoint(new Vector3(root.transform.position.x, bounds.min.y + bounds.size.y * .35f, root.transform.position.z));
                capsule.height = Mathf.Abs(size.y) * .7f;
                capsule.radius = Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.z)) * .12f;
            }

            DropletMarkers();
            Narrative(pads.OrderBy(p => p.transform.position.y).First());
            Save();
            return "Stage One poses, cooldowns, pad availability, background tree trunks and narrative authored.";
        }

        private static void LeafPose(string name, Vector3 position, Vector3 rotation, float scale)
        {
            var target = Named(name).GetComponent<LeafInstallTarget>();
            SetLeafPose(target, position, rotation, scale);
            Number(target, "_reuseDelayAfterRelease", 5f);
        }

        private static void DropletMarkers()
        {
            const string meshPath = "Assets/_Project/Art/Abilities/Sap/Meshes/MESH_ReadySapDroplet.asset";
            System.IO.Directory.CreateDirectory("Assets/_Project/Art/Abilities/Sap/Meshes");
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = new Mesh{name = "ReadySapDroplet"};
                var vertices = new List<Vector3>();
                var triangles = new List<int>();
                const int rings = 14, sides = 16;
                for (int ring = 0; ring <= rings; ring++)
                {
                    float t = ring / (float)rings;
                    float radius = Mathf.Sin(t * Mathf.PI) * (1f - .65f * t) * .65f;
                    for (int side = 0; side <= sides; side++)
                    {
                        float angle = side * Mathf.PI * 2f / sides;
                        vertices.Add(new Vector3(Mathf.Cos(angle) * radius, t - .25f, Mathf.Sin(angle) * radius));
                        if (ring == rings || side == sides)
                            continue;
                        int a = ring * (sides + 1) + side, b = a + sides + 1;
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(a + 1);
                        triangles.Add(a + 1);
                        triangles.Add(b);
                        triangles.Add(b + 1);
                    }
                }

                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh, meshPath);
            }

            foreach (var marker in _transforms.Where(t => t.name.StartsWith("CuredBlueSap") && t.gameObject.activeInHierarchy))
            {
                var target = marker.GetComponentInParent<LeafInstallTarget>();
                var filter = marker.GetComponent<MeshFilter>();
                if (target == null || filter == null)
                    continue;
                filter.sharedMesh = mesh;
                marker.rotation = Quaternion.identity;
                marker.position = target.SocketPosition + Vector3.up * .15f;
                var parentScale = marker.parent.lossyScale;
                marker.localScale = new Vector3(.9f / Mathf.Abs(parentScale.x), 1.1f / Mathf.Abs(parentScale.y), .9f / Mathf.Abs(parentScale.z));
                Set(target, "_readyMarker", marker.gameObject);
                var effect = target.transform.Find("SapPop1117");
                if (effect == null)
                {
                    var go = new GameObject("SapPop1117", typeof(ParticleSystem));
                    go.transform.SetParent(target.transform, false);
                    go.transform.position = target.SocketPosition;
                    var particles = go.GetComponent<ParticleSystem>();
                    var main = particles.main;
                    main.playOnAwake = false;
                    main.loop = false;
                    main.duration = .3f;
                    main.startLifetime = .35f;
                    main.startSize = .12f;
                    main.startSpeed = 2.5f;
                    main.startColor = new Color(.1f, .65f, 1f, .8f);
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    var emission = particles.emission;
                    emission.rateOverTime = 0;
                    emission.SetBursts(new[]{new ParticleSystem.Burst(0, 12)});
                    var shape = particles.shape;
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = .2f;
                    particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = marker.GetComponent<Renderer>().sharedMaterial;
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    Set(target, "_installSplash", particles);
                }
            }
        }

        private static void SetLeafPose(LeafInstallTarget target, Vector3 position, Vector3 euler, float scale)
        {
            var level = _transforms.Select(t => t.GetComponent<StageLevel>()).First(l => l != null);
            var prefab = level.abilityRules.leaf.offlinePrefab;
            var mesh = prefab.GetComponentInChildren<MeshFilter>(true).transform;
            var visual = Read<GameObject>(prefab, "_visual").transform;
            var visualScale = new Vector3(scale / mesh.localScale.x, scale / mesh.localScale.z, scale / mesh.localScale.y);
            var rotation = Quaternion.Euler(euler) * Quaternion.Inverse(mesh.localRotation);
            var socketPosition = position - rotation * Vector3.Scale(visualScale, mesh.localPosition);
            target.transform.position = socketPosition;
            target.Socket.SetPositionAndRotation(socketPosition, rotation);
            Vector(target, "_installedVisualScale", visualScale);
        }

        private static TextMeshProUGUI Text(Transform parent, string name, TMP_Text template, Vector2 low, Vector2 high)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = low;
            rect.anchorMax = high;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = template.font;
            label.fontSharedMaterial = template.fontSharedMaterial;
            label.fontSize = 24;
            label.color = template.color;
            label.alignment = TextAlignmentOptions.TopRight;
            label.raycastTarget = false;
            return label;
        }

        private static void Narrative(ExteriorJumpPad firstPad = null)
        {
            var overlay = _transforms.Select(t => t.GetComponent<GameOverlayHud>()).First(o => o != null);
            var hud = overlay.GetComponent<StagePlaytestHud>();
            if (hud == null)
            {
                var reference = EditorSceneManager.OpenPreviewScene(CurrentStagePaths.StageOne);
                try
                {
                    var source = reference.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<StagePlaytestHud>(true)).First();
                    hud = overlay.gameObject.AddComponent<StagePlaytestHud>();
                    EditorUtility.CopySerialized(source, hud);
                    var feedback = new Dictionary<string, GameObject>();
                    foreach (string name in new[]{"RecoveryFade", "AbilitySpotlight", "DialogueHistory"})
                    {
                        var child = source.transform.Find(name);
                        var copy = Object.Instantiate(child.gameObject, overlay.transform, false);
                        copy.name = name;
                        feedback[name] = copy;
                    }

                    Set(hud, "_overlay", overlay);
                    Set(hud, "_recoveryFade", feedback["RecoveryFade"].GetComponent<CanvasGroup>());
                    Set(hud, "_spotlight", feedback["AbilitySpotlight"].transform);
                    Set(hud, "_spotlightOverlay", feedback["AbilitySpotlight"].GetComponentInChildren<RawImage>(true));
                    Set(hud, "_guideArrow", feedback["AbilitySpotlight"].transform.Find("Arrow"));
                    Set(hud, "_guideText", feedback["AbilitySpotlight"].GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "GuideText"));
                    Set(hud, "_historyPanel", feedback["DialogueHistory"]);
                    Set(hud, "_historyText", feedback["DialogueHistory"].GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Entries"));
                    Set(hud, "_historyScroll", feedback["DialogueHistory"].GetComponentInChildren<ScrollRect>(true));
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(reference);
                }

                Set(overlay, "_playtestHud", hud);
            }

            var presenter = overlay.GetComponent<StageNarrativePresenter>();
            if (presenter == null)
                presenter = overlay.gameObject.AddComponent<StageNarrativePresenter>();
            Set(presenter, "_text", AssetDatabase.LoadAssetAtPath<StageNarrativeText>("Assets/_Project/Data/UI/SO_StageNarrativeText.asset"));
            Set(presenter, "_hud", hud);
            var root = overlay.transform.Find("QuestTracker1117");
            if (root == null)
            {
                var go = new GameObject("QuestTracker1117", typeof(RectTransform));
                go.transform.SetParent(overlay.transform, false);
                root = go.transform;
                var rect = (RectTransform)root;
                rect.anchorMin = new Vector2(.57f, .79f);
                rect.anchorMax = new Vector2(.97f, .95f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                Set(presenter, "_mainQuest", Text(root, "MainQuest", overlay.objective, new Vector2(0, .5f), Vector2.one));
                var detail = Text(root, "SubQuest", overlay.objective, Vector2.zero, new Vector2(1, .5f));
                detail.fontSize = 20;
                Set(presenter, "_subQuest", detail);
            }

            var questRect = (RectTransform)root;
            questRect.anchorMin = new Vector2(.52f, 1);
            questRect.anchorMax = Vector2.one;
            questRect.pivot = Vector2.one;
            questRect.anchoredPosition = new Vector2(-28, -28);
            questRect.sizeDelta = new Vector2(-28, 146);
            questRect.localScale = Vector3.one;

            var authoredQuest = _transforms.FirstOrDefault(t => t.name == "QuestHudPanelVisual");
            if (authoredQuest != null && root.Find("QuestArt1117") == null)
            {
                var originalImage = authoredQuest.GetComponent<Image>();
                var go = new GameObject("QuestArt1117", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(root, false);
                go.transform.SetAsFirstSibling();
                var rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(0, .5f);
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = go.GetComponent<Image>();
                image.sprite = originalImage.sprite;
                image.color = originalImage.color;
                image.raycastTarget = false;
            }

            foreach (var old in _transforms.Where(t => t.name == "QuestHudPanelVisual" ||
                t.name == "QuestLogIcon" && !t.IsChildOf(root) ||
                System.Text.RegularExpressions.Regex.IsMatch(t.name, "^Quest[1-6]$")))
                old.gameObject.SetActive(false);
            foreach (var label in root.GetComponentsInChildren<TMP_Text>(true))
                label.color = new Color(.96f, .94f, .87f);
            var mainQuest=Read<TMP_Text>(presenter,"_mainQuest");
            mainQuest.rectTransform.anchorMin = new Vector2(0, .52f);
            mainQuest.rectTransform.anchorMax = Vector2.one;
            mainQuest.rectTransform.offsetMin = new Vector2(24, 4);
            mainQuest.rectTransform.offsetMax = new Vector2(-62, -4);
            mainQuest.alignment = TextAlignmentOptions.MidlineLeft;
            mainQuest.enableAutoSizing = true;
            mainQuest.fontSizeMin = 20;
            mainQuest.fontSizeMax = 26;
            mainQuest.margin = Vector4.zero;
            var subQuest = Read<TMP_Text>(presenter, "_subQuest");
            subQuest.rectTransform.anchorMin = Vector2.zero;
            subQuest.rectTransform.anchorMax = new Vector2(1, .5f);
            subQuest.rectTransform.offsetMin = new Vector2(16, 2);
            subQuest.rectTransform.offsetMax = new Vector2(-16, -6);
            subQuest.alignment = TextAlignmentOptions.TopRight;
            subQuest.enableAutoSizing = true;
            subQuest.fontSizeMin = 18;
            subQuest.fontSizeMax = 22;
            subQuest.margin = Vector4.zero;
            var historyPanel = Read<GameObject>(hud, "_historyPanel");
            root.SetSiblingIndex(historyPanel.transform.GetSiblingIndex());
            historyPanel.SetActive(false);
            if (firstPad != null)
            {
                Set(presenter, "_firstPad", firstPad);
                Set(presenter, "_firstPadLeaf", firstPad.GetComponentInChildren<LeafInstallTarget>(true));
                var ports = new SerializedObject(firstPad).FindProperty("_ports");
                Set(presenter, "_firstPadInlet", ports.GetArrayElementAtIndex(0).objectReferenceValue);
            }

            var arrow = Read<RectTransform>(hud, "_guideArrow");
            if (arrow != null)
            {
                foreach (var image in arrow.GetComponentsInChildren<Image>(true))
                    image.enabled = false;
                foreach (var text in arrow.GetComponentsInChildren<TMP_Text>(true))
                    text.enabled = false;
            }

            var guideText = Read<TMP_Text>(hud, "_guideText");
            var caption = (RectTransform)guideText.transform.parent;
            caption.anchorMin = new Vector2(.035f, .64f);
            caption.anchorMax = new Vector2(.42f, .80f);
            caption.offsetMin = caption.offsetMax = Vector2.zero;
            guideText.rectTransform.anchorMin = Vector2.zero;
            guideText.rectTransform.anchorMax = Vector2.one;
            guideText.rectTransform.offsetMin = new Vector2(12, 8);
            guideText.rectTransform.offsetMax = new Vector2(-12, -8);
            guideText.alignment = TextAlignmentOptions.TopLeft;
            guideText.color = new Color(.96f, .94f, .87f);
            var spotlight = Read<RectTransform>(hud, "_spotlight");
            var connector = spotlight.Find("GuideConnector1117");
            if (connector == null)
            {
                var go = new GameObject("GuideConnector1117", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(spotlight, false);
                connector = go.transform;
                Set(hud, "_guideConnector", connector);
            }
            var pointerImage = connector.GetComponent<Image>();
            if (pointerImage != null)
            {
                pointerImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Textures/ArtistDrop/UI/UI/GuideUI/GuideUI_Arrow.png");
                pointerImage.color = Color.white;
                pointerImage.preserveAspect = true;
                pointerImage.raycastTarget = false;
            }
        }

        public static string StageTwo()
        {
            Open(CurrentStagePaths.Maze);
            foreach (var board in _transforms.Select(t => t.GetComponent<SapMazeBoard>()).Where(b => b != null))
            {
                board.Definition.speed = 7.7f;
                EditorUtility.SetDirty(board.Definition);
                foreach (var leak in board.Leaks)
                    if (leak != null && leak.Target != null)
                        Flag(leak.Target, "_replaceOnInstall", true);
            }

            foreach (var stream in _transforms.Select(t => t.GetComponent<MazeLeakStreamPresentation>()).Where(s => s != null))
            {
                var so = new SerializedObject(stream);
                var speed = so.FindProperty("_launchSpeed");
                if (so.FindProperty("_dropsPerMeterPerSecond").floatValue == 1.8f)
                {
                    speed.floatValue *= 1.3f;
                    so.FindProperty("_diameter").floatValue *= 1.3f;
                    so.FindProperty("_dropsPerMeterPerSecond").floatValue = 2.34f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            Narrative();
            Save();
            return "Stage Two refill/retry presentation, speed and leak installation authored.";
        }

        public static string StageThree()
        {
            Open(CurrentStagePaths.BranchRide);
            var flow = _transforms.Select(t => t.GetComponent<BranchRideFlow>()).First(f => f != null);
            Flag(flow, "_leafStopsCarrier", false);
            var rules = Read<StageLevel>(flow, "_level").abilityRules;
            rules.leafInstructions = "굴러오는 돌에 나뭇잎을 던져 파괴하세요.";
            EditorUtility.SetDirty(rules);
            foreach (var target in _transforms.Select(t => t.GetComponent<LeafInstallTarget>()).Where(t => t != null))
                target.enabled = false;
            var hazards = flow.Hazards.ToList();
            foreach (var hazard in hazards)
            {
                var so = new SerializedObject(hazard);
                float previousSpeed = so.FindProperty("_rollSpeed").floatValue;
                float previousRadius = so.FindProperty("_radius").floatValue;
                if (previousSpeed < 21f)
                {
                    so.FindProperty("_rollSpeed").floatValue = previousSpeed * 3f;
                    so.FindProperty("_radius").floatValue = previousRadius * .5f;
                    so.FindProperty("_fallDuration").floatValue /= 3f;
                    so.FindProperty("_exitDuration").floatValue /= 3f;
                    so.FindProperty("_respawnDelay").floatValue /= 3f;
                    so.FindProperty("_phaseOffset").floatValue /= 3f;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    hazard.Visual.localScale *= .5f;
                }
            }

            foreach (var stone in hazards)
            {
                var collider = stone.Visual.GetComponent<SphereCollider>();
                if (collider == null)
                    collider = stone.Visual.gameObject.AddComponent<SphereCollider>();
                float scale = Mathf.Max(Mathf.Abs(stone.Visual.lossyScale.x), Mathf.Abs(stone.Visual.lossyScale.y), Mathf.Abs(stone.Visual.lossyScale.z));
                collider.center = Vector3.zero;
                collider.radius = stone.Radius / scale;
                var effect = stone.transform.Find("StoneBreak");
                if (effect == null)
                {
                    var go = new GameObject("StoneBreak");
                    go.transform.SetParent(stone.transform, false);
                    var ps = go.AddComponent<ParticleSystem>();
                    var main = ps.main;
                    main.playOnAwake = false;
                    main.loop = false;
                    main.duration = .5f;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(.3f, .8f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
                    main.startSize = new ParticleSystem.MinMaxCurve(.08f, .3f);
                    main.startColor = new Color(.35f, .3f, .23f);
                    main.gravityModifier = 1f;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    var emission = ps.emission;
                    emission.rateOverTime = 0;
                    emission.SetBursts(new[]{new ParticleSystem.Burst(0, 18)});
                    var shape = ps.shape;
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = stone.Radius * .35f;
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    renderer.renderMode = ParticleSystemRenderMode.Mesh;
                    renderer.mesh = stone.Visual.GetComponent<MeshFilter>().sharedMesh;
                    renderer.sharedMaterial = stone.Visual.GetComponent<Renderer>().sharedMaterial;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    Set(stone, "_breakParticles", ps);
                }
            }

            Array(flow, "_hazards", hazards);
            var presentation = flow.GetComponent<BranchRidePresentation>();
            Flag(presentation, "_narrativeControlled", true);
            var overlay = _transforms.Select(t => t.GetComponent<GameOverlayHud>()).First(h => h != null);
            var preview = overlay.transform.Find("BranchRotationPreview1117");
            const string texturePath = "Assets/_Project/Settings/Rendering/StageThreeBranchPreview.renderTexture";
            var texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(texturePath);
            if (texture == null)
            {
                texture = new RenderTexture(480, 270, 16);
                AssetDatabase.CreateAsset(texture, texturePath);
            }

            if (preview == null)
            {
                var go = new GameObject("BranchRotationPreview1117", typeof(RectTransform), typeof(RawImage));
                go.transform.SetParent(overlay.transform, false);
                preview = go.transform;
                var rect = (RectTransform)preview;
                rect.anchorMin = new Vector2(.72f, .04f);
                rect.anchorMax = new Vector2(.97f, .29f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = go.GetComponent<RawImage>();
                image.texture = texture;
                image.raycastTarget = false;
                Set(presentation, "_branchPreview", image);
                go.SetActive(false);
            }

            var cameras = new List<Camera>();
            for (int i = 0; i < flow.Switches.Count; i++)
            {
                var toggle = flow.Switches[i];
                var serialized = new SerializedObject(toggle);
                var pivots = serialized.FindProperty("_pivots");
                var bounds = new Bounds(toggle.Port.InteractionPoint, Vector3.one);
                for (int j = 0; j < pivots.arraySize; j++)
                    foreach (var renderer in ((Transform)pivots.GetArrayElementAtIndex(j).objectReferenceValue).GetComponentsInChildren<Renderer>(true))
                        bounds.Encapsulate(renderer.bounds);
                var cameraTransform = flow.transform.Find("BranchPreviewCamera_" + i);
                if (cameraTransform == null)
                {
                    cameraTransform = new GameObject("BranchPreviewCamera_" + i, typeof(Camera)).transform;
                    cameraTransform.SetParent(flow.transform, false);
                }

                var camera = cameraTransform.GetComponent<Camera>();
                camera.enabled = false;
                camera.targetTexture = texture;
                camera.nearClipPlane = .1f;
                camera.farClipPlane = 500f;
                camera.fieldOfView = 48f;
                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(bounds.size.z * .65f, bounds.size.x * .65f / camera.aspect);
                cameraTransform.position = new Vector3(bounds.center.x, bounds.max.y + 2f, bounds.center.z);
                cameraTransform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                camera.farClipPlane = Mathf.Max(12f, bounds.size.y + 4f);
                cameras.Add(camera);
                var glow = toggle.transform.Find("SapGlow1117");
                if (glow == null)
                {
                    glow = new GameObject("SapGlow1117", typeof(Light)).transform;
                    glow.SetParent(toggle.transform, false);
                    glow.position = toggle.Port.InteractionPoint;
                    var light = glow.GetComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(.12f, .7f, 1f);
                    light.range = Mathf.Max(5f, bounds.size.magnitude);
                    light.intensity = 3f;
                    Set(toggle, "_sapGlow", light);
                }
            }

            Array(presentation, "_branchPreviewCameras", cameras);
            const string glowPath = "Assets/_Project/Materials/Stages/M_Stage03_SapGlow.mat";
            var glowMaterial = AssetDatabase.LoadAssetAtPath<Material>(glowPath);
            if (glowMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (shader == null)
                    throw new InvalidOperationException("URP particle unlit shader is unavailable.");
                glowMaterial = new Material(shader);
                glowMaterial.SetColor("_BaseColor", new Color(.03f, .8f, 1f, 1f) * 2f);
                AssetDatabase.CreateAsset(glowMaterial, glowPath);
            }

            foreach (var track in flow.Track.Where(t => t.SwitchIndex >= 0))
            {
                var lineRoot = track.transform.Find("SapGlowLine1117");
                if (lineRoot == null)
                {
                    lineRoot = new GameObject("SapGlowLine1117", typeof(LineRenderer)).transform;
                    lineRoot.SetParent(track.transform, false);
                }

                var points = new SerializedObject(track).FindProperty("_points");
                var line = lineRoot.GetComponent<LineRenderer>();
                line.useWorldSpace = false;
                line.positionCount = points.arraySize;
                for (int i = 0; i < points.arraySize; i++)
                {
                    var point = (Transform)points.GetArrayElementAtIndex(i).objectReferenceValue;
                    line.SetPosition(i, lineRoot.InverseTransformPoint(point.position + Vector3.up * .2f));
                }

                line.widthMultiplier = .12f;
                line.sharedMaterial = glowMaterial;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
            }

            Narrative();
            Save();
            return "Stage Three authored with " + hazards.Count + " destructible stones, branch glows and personal-view preview.";
        }

        public static string MenusAndAudio()
        {
            const string menuPath = "Assets/_Project/Prefabs/UI/PF_MainMenuScreen.prefab";
            var prefab = PrefabUtility.LoadPrefabContents(menuPath);
            try
            {
                var menu = prefab.GetComponent<MainMenuScreen>();
                menu.entry.transform.localScale = Vector3.one * .92f;
                PrefabUtility.SaveAsPrefabAsset(prefab, menuPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }

            Open("Assets/ui_select_chracter.unity");
            var canvas = _transforms.First(t => t.name == "PF_CharacterSelectVisual").GetComponent<Canvas>();
            var container = canvas.transform.Find("ScaledSelectionUi1117");
            if (container != null && container.childCount == 0)
            {
                Object.DestroyImmediate(container.gameObject);
                container = null;
            }
            // Keep prefab membership and serialized links intact; scale each root around canvas center.
            foreach (RectTransform child in canvas.transform)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(child);
                if (source == null) continue;
                Vector3 desiredScale = source.localScale * .92f;
                if ((child.localScale - desiredScale).sqrMagnitude < .000001f) continue;
                child.anchoredPosition *= .92f;
                child.localScale = desiredScale;
            }
            Save();
            var catalog = AssetDatabase.LoadAssetAtPath<Herbalist.Audio.SfxCatalog>("Assets/_Project/Data/Audio/SO_GameSfx.asset");
            var sound = catalog.Find(59);
            if (sound == null)
            {
                sound = new Herbalist.Audio.SfxDefinition{id = 59, label = "무당방울 리스폰"};
                catalog.sounds = catalog.sounds.Concat(new[]{sound}).ToArray();
            }

            sound.clips = new[]{AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Stage01/ShamanBell_Respawn.wav")};
            sound.volume = .65f;
            sound.spatialBlend = 0;
            sound.cooldown = .5f;
            sound.temporaryFallback = true;
            sound.notes = "11/17 수정사항의 R 리스폰 피드백용 합성 방울 소리.";
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return "Room-entry/character-selection UI scaled to 92 percent, respawn bell assigned.";
        }

        public static string GrowCircledMushroom()
        {
            Open(CurrentStagePaths.StageOne);
            var mushroom = Named("03_Mushroom (6)");
            var existing = _transforms.FirstOrDefault(t => t.name is "Sap_GrowingCircledMushroom1117" or "MushroomGrowth_03_CircledMushroom");
            if (existing != null)
                return "Circled mushroom already bound.";
            var source = _transforms.Select(t => t.GetComponent<StageOneDocumentMechanism>()).First(m => m != null && m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms);
            var root = new GameObject("Sap_GrowingCircledMushroom1117");
            root.transform.SetParent(Named("Stage1_DocumentGimmicks"), false);
            var mechanism = root.AddComponent<StageOneDocumentMechanism>();
            var inlet = Object.Instantiate(Read<SapInjectionPort>(source, "_port"), root.transform);
            inlet.name = "CircledMushroomWaterInlet";
            inlet.transform.position = mushroom.position + new Vector3(-2f, 1f, 1f);
            inlet.transform.rotation = Quaternion.LookRotation(new Vector3(-1f, 0, -.2f));
            Set(mechanism, "_port", inlet);
            Array(mechanism, "_movingArt", new[]{mushroom});
            Number(mechanism, "_restScaleMultiplier", .4f);
            Number(mechanism, "_grownScale", 2.5f);
            Number(mechanism, "_fillSeconds", 1f);
            Number(mechanism, "_restoreSeconds", 3f);
            Vector(mechanism, "_growthWorldOffset", Vector3.zero);
            var recovery = new GameObject("CircledMushroomRecovery1117").transform;
            recovery.SetParent(mushroom, false);
            var collider = mushroom.GetComponent<Collider>();
            var bounds = collider.bounds;
            Vector3 point = new Vector3(bounds.center.x, bounds.max.y + .5f, bounds.center.z);
            if (collider.Raycast(new Ray(point, Vector3.down), out var hit, bounds.size.y + 1f))
                point = hit.point + Vector3.up * .15f;
            recovery.position = point;
            var traversal = _transforms.Select(t => t.GetComponent<StageOneTraversal>()).First(t => t != null);
            var serialized = new SerializedObject(traversal);
            var checkpoints = serialized.FindProperty("_sharedCheckpoints");
            for (int i = 0; i < checkpoints.arraySize; i++)
            {
                var group = checkpoints.GetArrayElementAtIndex(i);
                var surfaces = group.FindPropertyRelative("surfaces");
                for (int j = 0; j < surfaces.arraySize; j++)
                    if (surfaces.GetArrayElementAtIndex(j).objectReferenceValue == collider)
                        group.FindPropertyRelative("recoveryPoint").objectReferenceValue = recovery;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            Save();
            return "03_Mushroom (6) grows from 40 percent to its original full size with sap.";
        }
    }
}
