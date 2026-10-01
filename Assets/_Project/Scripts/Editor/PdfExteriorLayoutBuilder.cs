using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Herbalist.Levels;
using Herbalist.Abilities;
using Herbalist.StageOne;
using Herbalist.Networking;
using Object = UnityEngine.Object;
namespace Herbalist.Editor
{
    public static class PdfExteriorLayoutBuilder
    {
        public const string SettingsPath = "Assets/_Project/Data/Stages/Revision_260928/SO_ExteriorLayout_260928.asset";
        private static TMP_FontAsset _font;
        [MenuItem("Herbalist/Stages/Apply PDF Exterior Pages 20-23")]
        private static void ApplyMenu() => Debug.Log(Apply());
        public static string Apply()
        {
            if (Application.isPlaying) throw new Exception("Stop Play before authoring.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != CurrentStagePaths.Exterior) throw new Exception("Open the current exterior scene first.");
            var settings = AssetDatabase.LoadAssetAtPath<ExteriorLayoutSettings>(SettingsPath) ?? Seed();
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/NanumGothic/NanumGothicSdf.asset");
            var root = Object.FindFirstObjectByType<ExteriorStageFlow>().transform;
            foreach (var child in root.Cast<Transform>().ToArray())
                if (child.name.StartsWith("Branch_") || child.name.StartsWith("Route_") || child.name.StartsWith("TwinJumpPad_") || child.name.StartsWith("WhiteCloth_") || child.name.StartsWith("SwingLanding_") || child.name.StartsWith("SummitApproach_") || child.name == "PdfExteriorLayout") Object.DestroyImmediate(child.gameObject);
            var layout = new GameObject("PdfExteriorLayout").transform; layout.SetParent(root, false);
            var tree = GameObject.Find("Sacred_Tree_1.0");
            Vector3 center = tree.transform.position; center.y = 0;
            int targetId = 2200, padIndex = 0;
            var routes = new System.Collections.Generic.List<Transform>();
            foreach (var definition in settings.branches)
            {
                var direction = Direction(definition.angle);
                Vector3 top = center + direction * (definition.innerRadius + definition.length * .5f) + Vector3.up * definition.height;
                var branch = new GameObject(definition.name); branch.SetActive(false); branch.transform.SetParent(layout, false);
                branch.transform.SetPositionAndRotation(top, Quaternion.LookRotation(direction));
                var visual = Primitive(PrimitiveType.Cylinder, branch.transform, "BranchMesh", new Vector3(0, -definition.width * .5f, 0), new Vector3(definition.width, definition.length * .5f, definition.width), definition.kind == ExteriorBranchKind.JumpPad ? settings.jumpBranchMaterial : settings.normalBranchMaterial);
                visual.transform.localRotation = Quaternion.Euler(90, 0, 0);
                var collider = branch.AddComponent<BoxCollider>(); collider.center = new Vector3(0, -.2f, 0); collider.size = new Vector3(definition.width, .4f, definition.length);
                var target = branch.AddComponent<LeafInstallTarget>(); Target(target, ++targetId, false);
                Checkpoint(branch);
                routes.Add(branch.transform);
                if (definition.kind == ExteriorBranchKind.JumpPad) Pad(layout, top, definition.angle, padIndex++, settings);
                branch.SetActive(true);
            }
            foreach (var definition in settings.cloths) Cloth(layout, center, definition, settings);
            foreach (var definition in settings.leafBridges) Bridge(layout, center, definition, settings, ref targetId);
            var guide = root.GetComponent<LevelRouteGuide>(); guide.expectedRoute = routes.ToArray();
            var spawnLayout = Object.FindFirstObjectByType<PlayerSpawnLayout>(); var serializedSpawn = new SerializedObject(spawnLayout);
            var points = serializedSpawn.FindProperty("spawnPoints");
            for (int i = 0; i < points.arraySize; i++) ((Transform)points.GetArrayElementAtIndex(i).objectReferenceValue).position = center + settings.spawnOffsets[i];
            var offline = (GameObject)serializedSpawn.FindProperty("offlinePlayer").objectReferenceValue;
            offline.transform.position = center + settings.spawnOffsets[0];
            var potions = GameObject.Find("SoloTest_Potions"); if (potions != null) potions.transform.position = offline.transform.position;
            var flow = Object.FindFirstObjectByType<StageOneFlow>();
            for (int i = 0; i < flow.sources.Length; i++) flow.sources[i].transform.position = center + settings.herbOffsets[i];
            // Existing summit gate, role/ability flow and tree asset remain connected.
            var progress = root.GetComponent<ExteriorStageFlow>();
            Array(progress, "_checkpoints", root.GetComponentsInChildren<LandingCheckpoint>().Cast<Object>().ToArray());
            AddSummitConnections(layout, center, settings);
            Physics.SyncTransforms();
            Fusion.Editor.NetworkObjectPostprocessor.BakeScene(scene);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            return "PDF exterior authored: " + settings.branches.Count(x => x.kind == ExteriorBranchKind.JumpPad) + " yellow jump branches, " + settings.branches.Count(x => x.kind == ExteriorBranchKind.Normal) + " orange branches, " + settings.cloths.Length + " blue cloths, " + settings.leafBridges.Length + " leaf-built gaps.";
        }
        private static void Pad(Transform parent, Vector3 top, float angle, int index, ExteriorLayoutSettings settings)
        {
            var go = new GameObject("TwinJumpPad_" + index); go.SetActive(false); go.transform.SetParent(parent, false); go.transform.SetPositionAndRotation(top, Quaternion.Euler(0, angle - 90, 0));
            var pad = go.AddComponent<ExteriorJumpPad>(); Set(pad, "_settings", settings.jumpPadSettings);
            var ports = new SapInjectionPort[2]; var sockets = new LeafInstallTarget[2]; var centers = new Transform[2]; var jets = new GameObject[2];
            for (int i = 0; i < 2; i++)
            {
                var hole = Primitive(PrimitiveType.Cylinder, go.transform, "Hole_" + i, new Vector3(i == 0 ? -2 : 2, .05f, 0), new Vector3(1.2f, .05f, 1.2f), settings.portMaterial);
                var collider = hole.AddComponent<BoxCollider>(); collider.size = new Vector3(1, 2, 1);
                ports[i] = hole.AddComponent<SapInjectionPort>(); sockets[i] = hole.AddComponent<LeafInstallTarget>(); Target(sockets[i], 2500 + index * 2 + i, true);
                centers[i] = Point(hole.transform, "LeafCenter", new Vector3(0, 3, 0)); Set(sockets[i], "socket", centers[i]);
                jets[i] = Primitive(PrimitiveType.Cylinder, go.transform, "SapJet_" + i, new Vector3(i == 0 ? -2 : 2, 5, 0), new Vector3(.6f, 5, .6f), settings.jetMaterial != null ? settings.jetMaterial : settings.portMaterial);
                AddJetDroplets(jets[i], settings);
                jets[i].SetActive(false);
            }
            Array(pad, "_ports", ports); Array(pad, "_sockets", sockets); Array(pad, "_leafCenters", centers); Array(pad, "_jets", jets);
            Label(go.transform, new Vector3(0, 1.7f, -1.5f), settings.padInstructions);
            go.SetActive(true);
        }
        private static void Cloth(Transform parent, Vector3 center, ExteriorClothLayout definition, ExteriorLayoutSettings settings)
        {
            var root = new GameObject(definition.name); root.SetActive(false); root.transform.SetParent(parent, false); root.transform.position = center + Direction(definition.angle) * definition.radius + Vector3.up * definition.anchorHeight;
            var component = root.AddComponent<HangingCloth>(); Float(component, "_length", definition.length); Float(component, "_driveAcceleration", settings.clothDriveAcceleration);
            var cloth = Primitive(PrimitiveType.Cube, root.transform, "ClothMesh", Vector3.down * definition.length * .5f, new Vector3(.55f, definition.length, .08f), settings.clothMaterial);
            var knot = Primitive(PrimitiveType.Sphere, root.transform, "InteractableBottomKnot", Vector3.down * definition.length, Vector3.one * .45f, settings.clothMaterial);
            var beam = Primitive(PrimitiveType.Cylinder, root.transform, "ClothAnchorArm", -Direction(definition.angle) * definition.radius * .5f, new Vector3(.4f, definition.radius * .5f, .4f), settings.clothMaterial);
            beam.transform.rotation = Quaternion.FromToRotation(Vector3.up, Direction(definition.angle));
            Set(component, "_knot", knot.transform); Set(component, "_cloth", cloth.transform);
            Label(root.transform, Vector3.down * definition.length + Vector3.up * 1.2f, settings.clothInstructions);
            root.SetActive(true);
        }
        private static void Bridge(Transform parent, Vector3 center, ExteriorLeafBridgeLayout definition, ExteriorLayoutSettings settings, ref int targetId)
        {
            var root = new GameObject(definition.name).transform; root.SetParent(parent, false);
            for (int i = 0; i < definition.wallSections; i++)
            {
                float t = definition.wallSections == 1 ? .5f : i / (float)(definition.wallSections - 1);
                float angle = Mathf.LerpAngle(definition.fromAngle, definition.toAngle, t);
                float height = Mathf.Lerp(definition.fromHeight, definition.toHeight, t);
                var wall = Primitive(PrimitiveType.Cube, root, "LeafAttachmentWall_" + i, Vector3.zero, new Vector3(definition.width, 3, definition.thickness), settings.leafBridgeMaterial);
                wall.SetActive(false); wall.transform.position = center + Direction(angle) * definition.radius + Vector3.up * (height + .5f); wall.transform.rotation = Quaternion.LookRotation(Direction(angle));
                wall.AddComponent<BoxCollider>(); var target = wall.AddComponent<LeafInstallTarget>(); Target(target, ++targetId, false); wall.SetActive(true);
            }
            var guide = root.gameObject.AddComponent<LevelRouteGuide>(); guide.color = Color.green;
            guide.expectedRoute = new[] { Point(root, "LeafBridgeStart", center + Direction(definition.fromAngle) * (definition.radius + 1) + Vector3.up * definition.fromHeight), Point(root, "LeafBridgeEnd", center + Direction(definition.toAngle) * (definition.radius + 1) + Vector3.up * definition.toHeight) };
            // No horizontal collider or preinstalled leaf: the player builds this crossing.
            Label(root, guide.expectedRoute[0].position + Vector3.up * 2, settings.bridgeInstructions);
        }
        private static void AddSummitConnections(Transform parent, Vector3 center, ExteriorLayoutSettings settings)
        {
            var summit = parent.parent.Find("SummitDeck");
            summit.localScale = settings.summitDeckSize;
            // Keep the final launch column open; move the existing entrance to the east.
            if (parent.parent.Find("SummitEntranceCluster") == null)
            {
                var entrance = new GameObject("SummitEntranceCluster").transform;
                entrance.SetParent(parent.parent, false); entrance.position = center + Vector3.up * settings.summitHeight;
                foreach (var name in new[] { "SummitSapChannel", "SummitIrisSeal", "IrisLeft", "IrisRight", "IrisBack", "IrisRoof", "BothInsideIris", "SacredTreeAwakeGlow" })
                    parent.parent.Find(name).SetParent(entrance, true);
                entrance.rotation = Quaternion.Euler(0, 90, 0);
            }
            var corridor = Primitive(PrimitiveType.Cube, parent, "SummitEntranceFloor", center + Vector3.right * 4.5f + Vector3.up * (settings.summitHeight - .3f), new Vector3(9, .6f, 6), settings.normalBranchMaterial);
            corridor.AddComponent<BoxCollider>(); Checkpoint(corridor);
            var last = settings.branches.Last(x => x.kind == ExteriorBranchKind.JumpPad);
            var direction = Direction(last.angle); var tangent = Quaternion.Euler(0, 90, 0) * direction;
            var deck = Primitive(PrimitiveType.Cube, parent, "SummitSideLanding", center + direction * 6 + tangent * 4 + Vector3.up * (settings.summitHeight - .3f), new Vector3(4, .6f, 6), settings.normalBranchMaterial);
            deck.AddComponent<BoxCollider>(); Checkpoint(deck);
            Array(parent.parent.GetComponent<ExteriorStageFlow>(), "_checkpoints", parent.parent.GetComponentsInChildren<LandingCheckpoint>().Cast<Object>().ToArray());
        }
        private static ExteriorLayoutSettings Seed()
        {
            var s = ScriptableObject.CreateInstance<ExteriorLayoutSettings>();
            s.branches = new[] {
                Branch("NormalBranch_01", ExteriorBranchKind.Normal, 110, 6), Branch("JumpBranch_01", ExteriorBranchKind.JumpPad, 150, 8),
                Branch("JumpBranch_02", ExteriorBranchKind.JumpPad, 180, 16), Branch("NormalBranch_02", ExteriorBranchKind.Normal, 280, 21),
                Branch("JumpBranch_03", ExteriorBranchKind.JumpPad, 290, 25), Branch("NormalBranch_03", ExteriorBranchKind.Normal, 335, 30),
                Branch("JumpBranch_04", ExteriorBranchKind.JumpPad, 10, 35) };
            s.branches[2].innerRadius = 5; s.branches[2].length = 6;
            s.cloths = new[] { ClothDefinition("BlueCloth_01", 115, 31), ClothDefinition("BlueCloth_02", 210, 18), ClothDefinition("BlueCloth_03", 300, 8) };
            s.leafBridges = new[] {
                Gap("LeafBridge_01", 110, 110, 0, 6, 5), Gap("LeafBridge_02", 110, 150, 6, 8, 4),
                Gap("LeafBridge_03", 280, 290, 21, 25, 3), Gap("LeafBridge_04", 335, 10, 30, 35, 4) };
            s.spawnOffsets = new[] { new Vector3(12, .03f, -18), new Vector3(13.5f, .03f, -18) };
            s.herbOffsets = new[] { new Vector3(8, .7f, -6), new Vector3(-7, .7f, -3) };
            s.jumpPadSettings = AssetDatabase.LoadAssetAtPath<SapJumpPadSettings>("Assets/_Project/Data/Stages/Revision_260928/SO_ExteriorJumpPad.asset");
            s.jumpBranchMaterial = Material("YellowJumpBranch", new Color(.95f, .82f, .12f));
            s.normalBranchMaterial = Material("OrangeNormalBranch", new Color(.88f, .36f, .08f));
            s.clothMaterial = Material("BlueCloth", new Color(.12f, .4f, 1));
            s.leafBridgeMaterial = Material("GreenLeafAttachment", new Color(.13f, .5f, .22f));
            s.portMaterial = Material("SapPort", new Color(.08f, .4f, .95f));
            s.jetMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Abilities/MAT_SapStream.mat");
            AssetDatabase.CreateAsset(s, SettingsPath); AssetDatabase.SaveAssets(); return s;
        }
        private static ExteriorBranchLayout Branch(string name, ExteriorBranchKind kind, float angle, float height) => new ExteriorBranchLayout { name = name, kind = kind, angle = angle, height = height };
        private static ExteriorClothLayout ClothDefinition(string name, float angle, float length) => new ExteriorClothLayout { name = name, angle = angle, length = length };
        private static ExteriorLeafBridgeLayout Gap(string name, float from, float to, float bottom, float top, int sections) => new ExteriorLeafBridgeLayout { name = name, fromAngle = from, toAngle = to, fromHeight = bottom, toHeight = top, wallSections = sections };
        private static Vector3 Direction(float angle) => Quaternion.Euler(0, angle, 0) * Vector3.forward;
        private static Material Material(string name, Color color)
        {
            string folder = "Assets/_Project/Art/Materials/Environments/Revision_260928";
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = color; AssetDatabase.CreateAsset(m, folder + "/MAT_Pdf" + name + ".mat"); return m;
        }
        private static GameObject Primitive(PrimitiveType type, Transform parent, string name, Vector3 local, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = local; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        public static ParticleSystem AddJetDroplets(GameObject jet, ExteriorLayoutSettings settings)
        {
            var child = new GameObject("WaterDroplets"); child.transform.SetParent(jet.transform, false);
            var particles = child.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true; main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.startLifetime = new ParticleSystem.MinMaxCurve(settings.jetDropletLifetime.x, settings.jetDropletLifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(settings.jetDropletSpeed.x, settings.jetDropletSpeed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(settings.jetDropletSize.x, settings.jetDropletSize.y);
            main.gravityModifier = settings.jetDropletGravity;
            main.maxParticles = 256;
            var emission = particles.emission; emission.rateOverTime = settings.jetDropletRate;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
            shape.meshRenderer = jet.GetComponent<MeshRenderer>();
            shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shape.normalOffset = .02f;
            shape.randomDirectionAmount = .25f;
            var size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            var renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            renderer.sharedMaterial = settings.jetMaterial != null ? settings.jetMaterial : settings.portMaterial;
            return particles;
        }
        private static Transform Point(Transform parent, string name, Vector3 local) { var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = local; return go.transform; }
        private static void Checkpoint(GameObject go)
        {
            var c = go.AddComponent<LandingCheckpoint>(); Set(c, "_surface", go.GetComponent<Collider>());
            var top = go.GetComponent<Collider>().bounds.max.y;
            Array(c, "_respawns", new[] { Point(go.transform, "Checkpoint_0", Vector3.zero), Point(go.transform, "Checkpoint_1", Vector3.right) });
            foreach (var p in go.transform.Cast<Transform>().Where(x => x.name.StartsWith("Checkpoint_"))) p.position = new Vector3(p.position.x, top + .05f, p.position.z);
        }
        private static void Label(Transform parent, Vector3 local, string value)
        {
            var go = new GameObject("Instructions", typeof(RectTransform), typeof(Canvas)); go.transform.SetParent(parent, false); go.transform.localPosition = local; go.transform.localScale = Vector3.one * .004f; go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace; ((RectTransform)go.transform).sizeDelta = new Vector2(1100, 180);
            var child = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)); child.transform.SetParent(go.transform, false); var rect = (RectTransform)child.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = child.GetComponent<TextMeshProUGUI>(); text.font = _font; text.fontSize = 32; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false; text.text = value;
        }
        private static void Target(LeafInstallTarget t, int id, bool singleOccupant) { SerializedEditorFields.SetInt(t, "targetId", id); SerializedEditorFields.SetBool(t, "_singleOccupant", singleOccupant); }
        private static void Set(Object obj, string field, Object value) => SerializedEditorFields.Set(obj, field, value);
        private static void Array(Object obj, string field, Object[] values) => SerializedEditorFields.SetArray(obj, field, values);
        private static void Float(Object obj, string field, float value) { var so = new SerializedObject(obj); so.FindProperty(field).floatValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
