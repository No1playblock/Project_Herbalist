using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Herbalist.Editor
{
    // Explicit one-time binding of the imported map. Never replaces its decorative objects.
    public static class StageOneDocumentAuthoring
    {
        public const string ScenePath = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference.unity";
        private const string Data = "Assets/_Project/Data/Stages/Stage_01/";
        private static Transform _root;
        private static Transform[] _original;
        private static Material _marker, _water;
        private static int _id = 610000;
        private static readonly List<BoxCollider> Moss = new();
        private static readonly List<Collider> Safe = new();
        private static Collider _tree;

        private static Transform New(string name, Transform parent, Vector3 position)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); t.position = position; return t; }
        private static void Set(UnityEngine.Object obj, string property, UnityEngine.Object value)
        { var so = new SerializedObject(obj); so.FindProperty(property).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Set(UnityEngine.Object obj, string property, bool value)
        { var so = new SerializedObject(obj); so.FindProperty(property).boolValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Set(UnityEngine.Object obj, string property, float value)
        { var so = new SerializedObject(obj); so.FindProperty(property).floatValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Set(UnityEngine.Object obj, string property, Vector3 value)
        { var so = new SerializedObject(obj); so.FindProperty(property).vector3Value = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Array(UnityEngine.Object obj, string property, UnityEngine.Object[] values)
        {
            var so = new SerializedObject(obj); var p = so.FindProperty(property); p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static Transform At(string path) => _original.First(t => FullPath(t) == path);
        private static string FullPath(Transform t) { string p = t.name; while (t.parent != null) { t = t.parent; p = t.name + "/" + p; } return p; }
        private static Renderer Render(Transform t) => t.GetComponents<Renderer>().First(r => r.enabled);
        private static Vector3 Top(Renderer r)
        {
            var c = r.GetComponent<MeshCollider>();
            if (c == null) { c = r.gameObject.AddComponent<MeshCollider>(); c.sharedMesh = r.GetComponent<MeshFilter>().sharedMesh; }
            if (c.sharedMesh == null) c.sharedMesh = r.GetComponent<MeshFilter>().sharedMesh;
            c.enabled = true;
            Physics.SyncTransforms();
            var b = r.bounds; Vector3 result = new(b.center.x, b.max.y, b.center.z); float highest = float.NegativeInfinity;
            for (int x = -2; x <= 2; x++) for (int z = -2; z <= 2; z++)
            {
                var start = new Vector3(b.center.x + b.size.x * x * .1f, b.max.y + 1, b.center.z + b.size.z * z * .1f);
                if (c.Raycast(new Ray(start, Vector3.down), out var hit, b.size.y + 2) && hit.point.y > highest)
                { highest = hit.point.y; result = hit.point; }
            }
            return result + Vector3.up * .15f;
        }
        private static void SafeArea(string name, Vector3 position, Vector3 size)
        {
            var b = New(name, _root, position).gameObject.AddComponent<BoxCollider>(); b.isTrigger = true; b.size = size; Moss.Add(b);
            var line = New("MossSafeMarker_NoFloor", b.transform, position-Vector3.up*.94f).gameObject.AddComponent<LineRenderer>();
            line.sharedMaterial=_marker; line.useWorldSpace=false; line.loop=true; line.widthMultiplier=.1f;
            line.positionCount=4; float x=size.x*.5f,z=size.z*.5f;
            line.SetPositions(new[]{new Vector3(-x,0,-z),new Vector3(-x,0,z),new Vector3(x,0,z),new Vector3(x,0,-z)});
        }
        private static LeafInstallTarget Target(string name, Vector3 position, Transform parent, bool step, StageOneDocumentMechanism gate = null)
        {
            var t = New(name, parent, position); var target = t.gameObject.AddComponent<LeafInstallTarget>();
            var so = new SerializedObject(target); so.FindProperty("targetId").intValue = ++_id;
            so.FindProperty("socket").objectReferenceValue = New("LeafSocket", t, position);
            so.FindProperty("_singleOccupant").boolValue = true; so.ApplyModifiedPropertiesWithoutUndo();
            var trigger = t.gameObject.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 1.4f;
            t.gameObject.AddComponent<StageOneLeafZone>();
            if (step) t.gameObject.AddComponent<StageOneLeafUse>();
            if (gate != null) Set(t.gameObject.AddComponent<StageOneTargetGate>(), "_mechanism", gate);
            var line = New("LeafMarker_NoFloor", t, position).gameObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false; line.loop = true; line.sharedMaterial = _marker; line.widthMultiplier = .09f; line.positionCount = 32;
            for (int i = 0; i < 32; i++) { float a = i * Mathf.PI / 16; line.SetPosition(i, new Vector3(Mathf.Cos(a) * 1.3f, .04f, Mathf.Sin(a) * 1.3f)); }
            return target;
        }
        private static SapInjectionPort Port(string name, Vector3 point, Transform parent)
        {
            var t = New(name, parent, point); var c = t.gameObject.AddComponent<SphereCollider>(); c.radius = .65f;
            return t.gameObject.AddComponent<SapInjectionPort>();
        }
        private static SapInjectionPort PortAt(Transform art)
        {
            var port = art.GetComponent<SapInjectionPort>();
            if (port == null) port = art.gameObject.AddComponent<SapInjectionPort>();
            port.enabled = true;
            if (art.GetComponent<Collider>() == null)
            { var hit = art.gameObject.AddComponent<SphereCollider>(); hit.radius = .65f / Mathf.Max(.01f, Mathf.Max(art.lossyScale.x, art.lossyScale.z)); }
            return port;
        }
        private static StageOneDocumentMechanism Mechanism(string name, StageOneDocumentMechanismKind kind)
        {
            var go = New(name, _root, Vector3.zero).gameObject; go.AddComponent<NetworkObject>();
            var m = go.AddComponent<StageOneDocumentMechanism>(); var so = new SerializedObject(m);
            so.FindProperty("_kind").enumValueIndex = (int)kind; so.ApplyModifiedPropertiesWithoutUndo(); return m;
        }
        private static HangingCloth Cloth(string name, Transform art, bool waterDriven, Transform anchor = null)
        {
            var visual = art.Find("fabric"); var b = Render(visual).bounds;
            var top = new Vector3(b.center.x, b.max.y, b.center.z); var bottom = new Vector3(b.center.x, b.min.y + 1, b.center.z);
            var t = New(name, _root, top); t.gameObject.AddComponent<NetworkObject>(); var cloth = t.gameObject.AddComponent<HangingCloth>();
            Set(cloth, "_knot", New("GripPoint", t, bottom));
            Set(cloth, "_authoredVisual", visual); Set(cloth, "_externalVisualRoot", art); Set(cloth, "_externalAnchor", anchor);
            Set(cloth, "_singleUse", true); Set(cloth, "_waterDriven", waterDriven);
            Set(cloth, "_settings", AssetDatabase.LoadAssetAtPath<HangingClothSettings>(Data + "SO_HangingClothTuning.asset"));
            var so = new SerializedObject(cloth); so.FindProperty("_riderCapacity").intValue = 1;
            so.FindProperty("_grabText").stringValue = "1회용 천 · 이동 후 놓으면 말려 올라갑니다 · 탑승 중 능력 사용 불가"; so.ApplyModifiedPropertiesWithoutUndo();
            // Collision proxy follows the animated controller without duplicating the original mesh.
            var hit = New("WaterHitProxy", t, b.center).gameObject.AddComponent<BoxCollider>(); hit.size = new Vector3(Mathf.Max(1, b.size.x), b.size.y, Mathf.Max(1, b.size.z));
            return cloth;
        }
        private static Vector3 WallPoint(Vector3 desired)
        {
            Vector3 center = _tree.bounds.center; center.y = desired.y;
            Vector3 radial = Vector3.ProjectOnPlane(desired - center, Vector3.up).normalized;
            if (radial.sqrMagnitude < .1f) radial = Vector3.forward;
            if (_tree.Raycast(new Ray(center + radial * 170, -radial), out var hit, 340)) return hit.point + hit.normal * .5f;
            throw new InvalidOperationException("Cannot find authored tree surface for " + desired);
        }

        [MenuItem("Herbalist/Stages/Bind Imported Stage One Document Gimmicks")]
        private static void ApplyFromMenu() => Debug.Log(Apply());

        public static string Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit mode required.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath);
            if (scene.GetRootGameObjects().Any(r => r.name == "Stage1_DocumentGimmicks")) throw new InvalidOperationException("Already bound. Edit the authored bindings directly.");
            _original = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            _root = New("Stage1_DocumentGimmicks", null, Vector3.zero); _id = 610000; Moss.Clear(); Safe.Clear();
            _tree = At("Tree_playobject/seonag_tree").GetComponent<Collider>();
            if (_tree == null) throw new InvalidOperationException("Missing main tree collider.");
            _marker = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Environments/Stage_01Gimmicks/MAT_StageOneLeafZone.mat");
            _water = SapOrbitSetup.WaterMaterial();
            // Retire only outdated simulation components. Art activation, hierarchy and poses stay intact.
            foreach (var t in _original)
            {
                foreach (var c in t.GetComponents<ExteriorJumpPad>()) c.enabled = false;
                foreach (var c in t.GetComponents<HangingCloth>()) c.enabled = false;
                foreach (var c in t.GetComponents<LeafInstallTarget>()) c.enabled = false;
                foreach (var c in t.GetComponents<SapInjectionPort>()) c.enabled = false;
            }
            var level = _original.Select(t => t.GetComponent<StageLevel>()).First(x => x != null);
            var rules = AssetDatabase.LoadAssetAtPath<StageAbilityRules>(Data + "SO_ImportedStageOneRules.asset");
            if (rules == null) { rules = UnityEngine.Object.Instantiate(level.abilityRules); rules.name = "SO_ImportedStageOneRules"; AssetDatabase.CreateAsset(rules, Data + rules.name + ".asset"); }
            var leaf = AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>(Data + "SO_ImportedStageOneLeaf.asset");
            if (leaf == null) { leaf = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>(Data + "SO_StageOneTimedLeaf.asset")); leaf.name = "SO_ImportedStageOneLeaf"; AssetDatabase.CreateAsset(leaf, Data + leaf.name + ".asset"); }
            leaf.permanentInstallation = false; leaf.lifetime = 5;
            rules.leaf = leaf;
            rules.leafInstructions = "R: 초록 구역에 잎 설치 · 첫 탑승 시 회수 · 부착 제한 5초 · 좌클릭: 회수";
            rules.sapInstructions = "R 유지: 수액 발사 · 주입구/버섯/천에 사용 · 천 탑승 중 발사 불가";
            level.abilityRules = rules; EditorUtility.SetDirty(rules); EditorUtility.SetDirty(leaf);
            var cube = At("Tree_playobject/Cube");
            var c1 = Cloth("Coop_1_SingleUseCloth", At("movefabric (2)"), false);
            Cloth("Coop_2_WaterCloth", At("Tree_playobject/movefabric (1)"), false);
            Cloth("Coop_4_SingleUseCloth", At("Tree_playobject/movefabric (4)"), false, cube);
            Cloth("Coop_5_WaterCloth", At("Tree_playobject/movefabric (3)"), false);

            var mushrooms = _original.Select(t => t.GetComponent<MeshRenderer>()).Where(r => r != null && r.enabled && r.gameObject.activeInHierarchy &&
                (r.name.StartsWith("03_Mushroom") || r.name.StartsWith("04_Musroom") || r.name.StartsWith("MushroomVisual")) && r.GetComponent<MeshFilter>().sharedMesh != null).ToArray();
            foreach (var r in mushrooms) { Top(r); Safe.Add(r.GetComponent<Collider>()); }
            for (int i=0; i<2; i++)
            {
                var growth = Mechanism("Sap_1_GrowingMushroom_"+(i+1)+"_RoutePage6", StageOneDocumentMechanismKind.GrowingMushrooms);
                var mushroom = At("DesignStage1_Traversal/StaticPlatforms/04_Musroom ("+(15+i)+")");
                Safe.Remove(mushroom.GetComponent<Collider>());
                Array(growth, "_movingArt", new[] { mushroom }); Set(growth, "_grownScale", 6f);
                var sourceId = i==0 ? 1068282342UL : 1063571562UL;
                Set(growth,"_port",PortAt(_original.First(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId==sourceId)));
            }
            // Existing inlet/model identities, rather than nearest-model guessing. Positions remain authored.
            var links = new (ulong inlet, string model)[] {
                (1852367055,"03_Mushroom (11)"),(153831838,"04_Musroom (2)"),(1520852230,"04_Musroom (5)"),
                (322074202,"04_Musroom (14)"),(1427526811,"04_Musroom (18)"),(2043272759,"04_Musroom (19)"),
                (520556937,"branch_hole"),(1971396465,"branch_hole (1)"),(1727019246,"04_Musroom (21)"),
                (1713762518,"branch1_low (7)"),(92183412,"04_Musroom (32)"),(1664675313,"04_Musroom (25)"),
                (1561476787,"04_Musroom (26)"),(2026349226,"04_Musroom (35)"),(581031837,null)
            };
            var settings = AssetDatabase.LoadAssetAtPath<SapJumpPadSettings>(Data + "SO_ModelingMushroomJumpPad.asset");
            int pads = 0;
            foreach (var link in links)
            {
                var inlet = _original.First(t=>GlobalObjectId.GetGlobalObjectIdSlow(t).targetObjectId==link.inlet);
                var renderer = link.model == null
                    ? At("Tree_playobject").GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("branch") && r.GetComponent<MeshFilter>().sharedMesh!=null).OrderBy(r=>Vector3.Distance(r.bounds.center,inlet.position)).First()
                    : link.model.StartsWith("branch") ? Render(At("Tree_playobject/"+link.model)) : mushrooms.First(r=>r.name==link.model);
                var top = Top(renderer); var t = New("JumpPad_" + (++pads).ToString("00") + "_" + inlet.name, _root, top);
                t.gameObject.AddComponent<NetworkObject>(); var pad = t.gameObject.AddComponent<ExteriorJumpPad>(); t.gameObject.AddComponent<StageOneJumpPadAuthoring>();
                var port = PortAt(inlet);
                var target = Target("LeafOutlet", top, t, false);
                Set(target,"_keepLeafUntilUsed",true); Set(pad,"_recallLeafAfterLaunch",true);
                var jet = GameObject.CreatePrimitive(PrimitiveType.Cylinder); jet.name = "WaterJet"; jet.transform.SetParent(t, false);
                jet.transform.position = top; jet.transform.localScale = new Vector3(.3f, 1, .3f); UnityEngine.Object.DestroyImmediate(jet.GetComponent<Collider>());
                jet.GetComponent<Renderer>().sharedMaterial = _water; jet.SetActive(false);
                Set(pad, "_settings", settings); Array(pad, "_ports", new[] { port }); Array(pad, "_sockets", new[] { target });
                Array(pad, "_leafCenters", new[] { target.Socket }); Array(pad, "_jets", new[] { jet });
            }
            // Cooperative obstacle: it occupies only the new leaf installation path, never an original platform.
            var eject = Mechanism("Coop_1_LinkedObstacleEjection", StageOneDocumentMechanismKind.EjectObstacle);
            Set(eject, "_requiredCloth", c1);
            var b1 = Render(At("movefabric (2)/fabric")).bounds;
            Vector3 bridge = WallPoint(new Vector3(288, 54, 279));
            var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere); stone.name = "LeafPathObstacleStone"; stone.transform.SetParent(eject.transform, false); stone.transform.position = bridge;
            stone.transform.localScale = Vector3.one * 2.8f; stone.GetComponent<Renderer>().sharedMaterial = _original.Select(t => t.GetComponent<MeshRenderer>()).First(r => r != null && r.name.StartsWith("SM_Rocks")).sharedMaterial;
            Array(eject, "_movingArt", new[] { stone.transform }); Array(eject, "_obstacleColliders", new[] { stone.GetComponent<Collider>() }); Set(eject, "_offset", new Vector3(0, 5, -8));
            Vector3 arrival = new(280.276f, 55.1f, 314.911f);
            Set(eject, "_port", Port("Arrival_LinkedInlet", arrival + Vector3.up, eject.transform));
            var area = New("ArrivalActivationArea", eject.transform, arrival + Vector3.up * 2).gameObject.AddComponent<BoxCollider>(); area.isTrigger = true; area.size = new Vector3(22, 12, 22); Set(eject, "_activationArea", area);
            Target("Coop1_GatedLeafBridge", bridge, _root, true, eject);
            var lower = Mechanism("Coop_4_LeafButtonLowersBranch", StageOneDocumentMechanismKind.LowerBranch);
            var button = Target("LeafButton", new Vector3(299, 131.5f, 317.4f), lower.transform, false);
            Set(lower, "_leafTarget", button); Array(lower, "_movingArt", new[] { cube }); Set(lower, "_motionPivot", cube);
            Set(lower, "_offset", new Vector3(0, -4, 0)); Set(lower, "_rotation", new Vector3(0, 0, 70)); Set(lower, "_moveSeconds", 1.2f);

            var zonePositions = new[] { new Vector3(316,61,289),new Vector3(307,65,287),new Vector3(298,69,288),
                new Vector3(327,65,289),new Vector3(329,70,291),new Vector3(259,119,284),new Vector3(255,121,285),new Vector3(299,126,319) };
            for(int i=0;i<zonePositions.Length;i++) Target("LeafWall_"+(i+1), i==7 ? zonePositions[i] : WallPoint(zonePositions[i]), _root, true);
            var jetBlock = Mechanism("Leaf_1_BlockingEruption", StageOneDocumentMechanismKind.BlockJet);
            var jetPoint = WallPoint(new Vector3(277, 82, 331));
            var plug = Target("JetPlug", jetPoint, jetBlock.transform, false); Set(jetBlock, "_leafTarget", plug);
            var stream = GameObject.CreatePrimitive(PrimitiveType.Cylinder); stream.name = "BlockingWaterJet"; stream.transform.SetParent(jetBlock.transform, false);
            Vector3 direction = Vector3.ProjectOnPlane(jetPoint - _tree.bounds.center, Vector3.up).normalized;
            stream.transform.position = jetPoint + direction * 4; stream.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
            stream.transform.localScale = new Vector3(1, 4, 1); stream.GetComponent<Renderer>().sharedMaterial = _water;
            stream.layer=LayerMask.NameToLayer("Ignore Raycast"); // Solid for players, transparent to the leaf query reaching the plug.
            Set(jetBlock, "_jet", stream);
            var traversal = _root.gameObject.AddComponent<StageOneTraversal>();
            foreach (var filter in At("Tree_playobject").GetComponentsInChildren<MeshFilter>(true))
            {
                var collision = filter.GetComponent<MeshCollider>();
                if (filter.sharedMesh != null && collision != null && collision.sharedMesh == null) collision.sharedMesh = filter.sharedMesh;
            }
            var bare = At("Tree_playobject").GetComponentsInChildren<Collider>(true).Where(c => c.enabled && !c.isTrigger &&
                (c.name == "seonag_tree" || c.name.ToLowerInvariant().Contains("branch"))).ToList(); bare.Add(stream.GetComponent<Collider>());
            // Existing authored branches explicitly marked safe in the route drawing.
            foreach (var name in new[] { "Tree_playobject/branch_hole", "Tree_playobject/branch1_low (27)", "Tree_playobject/branch1_low (24)", "Tree_playobject/branch1_low (8)" })
            {
                var r = Render(At(name)); var surface = Top(r); var collider = r.GetComponent<Collider>();
                bare.Remove(collider); Safe.Add(collider);
                SafeArea("MossSafe_"+r.name, surface + Vector3.up, new Vector3(Mathf.Min(8,r.bounds.size.x),4,Mathf.Min(8,r.bounds.size.z)));
            }
            RepairHerbBindings();
            CompleteEntryAndExit();
            UpgradeWaterPresentation();
            ConfigureBlockingJet();
            var spawns = _original.Select(t => t.GetComponent<PlayerSpawnLayout>()).First(x => x != null);
            var sp = new SerializedObject(spawns).FindProperty("spawnPoints"); var starts = new Transform[sp.arraySize];
            for (int i=0;i<starts.Length;i++) starts[i]=(Transform)sp.GetArrayElementAtIndex(i).objectReferenceValue;
            Array(traversal,"_bareTrees",bare.ToArray()); Array(traversal,"_mossAreas",Moss.ToArray()); Array(traversal,"_checkpointSurfaces",Safe.ToArray()); Array(traversal,"_starts",starts); Set(traversal,"_placeOfflineAtStarts",true);
            ConfigureMovingBranchSurface();
            EditorUtility.SetDirty(level); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Imported map bound: "+pads+" jump pads, 4 single-use cloths, 2 growing mushrooms, linked obstacle, leaf jet plug and branch-lowering button. Original art untouched.";
        }
        public static void RepairHerbBindings()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var root=scene.GetRootGameObjects().First(x=>x.name=="Stage1_DocumentGimmicks").transform;
            foreach (var source in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Herbalist.StageOne.HerbSource>(true)))
            {
                var art=scene.GetRootGameObjects().First(x=>x.name==(source.name.StartsWith("Leaf") ? "herb" : "flower"));
                var nearest=art.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled).OrderBy(r=>Vector3.Distance(r.bounds.center,art.transform.position)).First();
                var lod=nearest.GetComponentInParent<LODGroup>();
                source.visual=lod!=null?lod.gameObject:nearest.gameObject;
                var renderers = source.visual.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled).ToArray();
                var b = renderers[0].bounds; foreach(var r in renderers.Skip(1)) b.Encapsulate(r.bounds);
                var point=root.Find("HerbInteraction_"+source.name); if(point==null) point=New("HerbInteraction_"+source.name,root,b.center); else point.position=b.center;
                source.interactionPoint=point;
                EditorUtility.SetDirty(source);
            }
        }
        public static void UpgradeWaterPresentation()
        {
            var root=GameObject.Find("Stage1_DocumentGimmicks");
            foreach(var pad in root.GetComponentsInChildren<ExteriorJumpPad>(true))
            {
                var so=new SerializedObject(pad); var jets=so.FindProperty("_jets");
                for(int i=0;i<jets.arraySize;i++)
                {
                    var jet=(GameObject)jets.GetArrayElementAtIndex(i).objectReferenceValue;
                    if(jet.GetComponent<MushroomWaterJetPresentation>()!=null)continue;
                    UnityEngine.Object.DestroyImmediate(jet.GetComponent<MeshRenderer>()); UnityEngine.Object.DestroyImmediate(jet.GetComponent<MeshFilter>());
                    var line=jet.AddComponent<LineRenderer>();line.sharedMaterial=SapOrbitSetup.WaterMaterial();line.useWorldSpace=true;
                    line.positionCount=2;line.SetPositions(new[]{jet.transform.position,jet.transform.position+Vector3.up*.2f});
                    Set(jet.AddComponent<MushroomWaterJetPresentation>(),"_stream",line);
                }
            }
            var stream=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="BlockingWaterJet");stream.gameObject.layer=LayerMask.NameToLayer("Ignore Raycast");
        }
        public static void CompleteEntryAndExit()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var root=scene.GetRootGameObjects().First(x=>x.name=="Stage1_DocumentGimmicks").transform;
            var settings=AssetDatabase.LoadAssetAtPath<SapJumpPadSettings>(Data+"SO_ModelingMushroomJumpPad.asset");
            _id=Mathf.Max(_id,root.GetComponentsInChildren<LeafInstallTarget>(true).Select(x=>x.Id).DefaultIfEmpty(610000).Max());
            foreach(var name in new[]{"MushroomVisual (1)","MushroomVisual (2)"})
            {
                if(root.Find("EntryPad_"+name)!=null)continue;
                var renderer=scene.GetRootGameObjects().Where(r=>r.transform!=root).SelectMany(r=>r.GetComponentsInChildren<MeshRenderer>(true)).First(r=>r.name==name);
                var top=Top(renderer);var t=New("EntryPad_"+name,root,top);t.gameObject.AddComponent<NetworkObject>();var pad=t.gameObject.AddComponent<ExteriorJumpPad>();t.gameObject.AddComponent<StageOneJumpPadAuthoring>();
                var port=Port("EntryWaterInlet",top+Vector3.left*(renderer.bounds.size.x*.4f),t);
                var ring=port.gameObject.AddComponent<LineRenderer>();ring.sharedMaterial=SapOrbitSetup.WaterMaterial();ring.loop=true;ring.useWorldSpace=false;ring.widthMultiplier=.1f;ring.positionCount=32;
                for(int i=0;i<32;i++){float a=i*Mathf.PI/16;ring.SetPosition(i,new Vector3(Mathf.Cos(a)*.65f,Mathf.Sin(a)*.65f,0));}
                _marker=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Environments/Stage_01Gimmicks/MAT_StageOneLeafZone.mat");
                var target=Target("EntryLeafOutlet",top,t,false);
                Set(target,"_keepLeafUntilUsed",true);Set(pad,"_recallLeafAfterLaunch",true);
                var jet=New("WaterJet",t,top).gameObject;jet.AddComponent<LineRenderer>().sharedMaterial=SapOrbitSetup.WaterMaterial();jet.SetActive(false);
                Set(jet.AddComponent<MushroomWaterJetPresentation>(),"_stream",jet.GetComponent<LineRenderer>());
                Set(pad,"_settings",settings);Array(pad,"_ports",new[]{port});Array(pad,"_sockets",new[]{target});Array(pad,"_leafCenters",new[]{target.Socket});Array(pad,"_jets",new[]{jet});
            }
            var goal=root.Find("PairedStage2Arrival_RoutePage11");
            if(goal==null)
            {
                var final=scene.GetRootGameObjects().Where(r=>r.transform!=root).SelectMany(r=>r.GetComponentsInChildren<MeshRenderer>(true)).First(r=>r.name=="MushroomVisual (3)");
                goal=New("PairedStage2Arrival_RoutePage11",root,Top(final)+Vector3.up*3);
                var c=goal.gameObject.AddComponent<BoxCollider>();c.isTrigger=true;c.size=new Vector3(8,8,8);
            }
            var flow=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Herbalist.StageOne.StageOneFlow>(true)).First();flow.interior=goal.GetComponent<BoxCollider>();EditorUtility.SetDirty(flow);
        }
        public static void ConfigureBlockingJet()
        {
            var branch=GameObject.Find("Tree_playobject").transform.Find("branch_hole").GetComponent<MeshCollider>();
            Vector3 radial=Vector3.ProjectOnPlane(new Vector3(278.51f,86.71f,330.29f)-branch.bounds.center,Vector3.up).normalized;
            var center=branch.bounds.center;center.y+=branch.bounds.extents.y*.65f;
            if(!branch.Raycast(new Ray(center+radial*100,-radial),out var hit,200))throw new InvalidOperationException("Jet outlet requires an actual branch surface.");
            var root=GameObject.Find("Stage1_DocumentGimmicks");var mechanism=root.GetComponentsInChildren<StageOneDocumentMechanism>().First(m=>m.Kind==StageOneDocumentMechanismKind.BlockJet);
            var so=new SerializedObject(mechanism);var target=(LeafInstallTarget)so.FindProperty("_leafTarget").objectReferenceValue;var jet=(GameObject)so.FindProperty("_jet").objectReferenceValue;
            target.transform.position=hit.point+hit.normal*.8f;jet.transform.position=target.SocketPosition+hit.normal*4;
            jet.transform.rotation=Quaternion.FromToRotation(Vector3.up,hit.normal);jet.layer=LayerMask.NameToLayer("Ignore Raycast");
        }
        public static void ConfigureMovingBranchSurface()
        {
            var root=GameObject.Find("Stage1_DocumentGimmicks");
            var branch=GameObject.Find("Tree_playobject").transform.Find("Cube/branch1_low (11)");
            var renderer=branch.GetComponent<MeshRenderer>();var surface=Top(renderer);
            var collider=branch.GetComponent<MeshCollider>();
            var traversal=root.GetComponent<StageOneTraversal>();var so=new SerializedObject(traversal);var checkpoints=so.FindProperty("_checkpointSurfaces");
            bool present=false;for(int i=0;i<checkpoints.arraySize;i++)present|=checkpoints.GetArrayElementAtIndex(i).objectReferenceValue==collider;
            if(!present){checkpoints.InsertArrayElementAtIndex(checkpoints.arraySize);checkpoints.GetArrayElementAtIndex(checkpoints.arraySize-1).objectReferenceValue=collider;so.ApplyModifiedPropertiesWithoutUndo();}
            var marker=root.transform.Find("MovingBranchSafeMarker");
            if(marker==null)
            {
                marker=New("MovingBranchSafeMarker",root.transform,surface+Vector3.up*.04f);var line=marker.gameObject.AddComponent<LineRenderer>();
                line.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Environments/Stage_01Gimmicks/MAT_StageOneLeafZone.mat");line.useWorldSpace=false;line.loop=true;line.widthMultiplier=.1f;line.positionCount=4;
                line.SetPositions(new[]{new Vector3(-2,0,-1),new Vector3(-2,0,1),new Vector3(2,0,1),new Vector3(2,0,-1)});
            }
            var lower=root.GetComponentsInChildren<StageOneDocumentMechanism>().First(m=>m.Kind==StageOneDocumentMechanismKind.LowerBranch);
            Array(lower,"_movingArt",new[]{branch.parent,marker});
            if (root.transform.Find("Coop_4_ReplacementCloth") == null)
                Set(lower,"_requiredCloth",null);
            var target=(LeafInstallTarget)new SerializedObject(lower).FindProperty("_leafTarget").objectReferenceValue;
            var gate=target.GetComponent<StageOneTargetGate>();if(gate==null)gate=target.gameObject.AddComponent<StageOneTargetGate>();
            Set(gate,"_mechanism",lower);Set(gate,"_requiresCompleted",false);
        }
    }
}
