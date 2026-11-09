using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    // Explicit correction of the photo-identified outlets. Does not rearrange the imported art.
    public static class StageOneOutletCorrection
    {
        private static int _id;
        private static Material _water, _marker;
        private static SapJumpPadSettings _flowSettings;

        private static Transform New(string name, Transform parent, Vector3 position)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); t.position = position; return t; }
        private static void Set(Object target, string field, Object value)
        { var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target); var p = so.FindProperty(field); p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static T Read<T>(Object target, string field) where T : Object => (T)new SerializedObject(target).FindProperty(field).objectReferenceValue;

        public static string Apply()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != StageOneDocumentAuthoring.ScenePath) throw new InvalidOperationException("Imported Stage One Edit mode required.");
            var root = GameObject.Find("Stage1_DocumentGimmicks").transform;
            if (root.Find("OutletCorrections_20261104") != null) throw new InvalidOperationException("Correction already applied; edit its authored objects.");
            var all = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            _id = all.Select(t => t.GetComponent<LeafInstallTarget>()).Where(t => t != null).Select(t => t.Id).DefaultIfEmpty(620000).Max() + 10;
            _water = SapOrbitSetup.WaterMaterial();
            _marker = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Environments/Stage_01Gimmicks/MAT_StageOneLeafZone.mat");
            _flowSettings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<SapJumpPadSettings>("Assets/_Project/Data/Stages/Stage_01/SO_ModelingMushroomJumpPad.asset"));
            _flowSettings.name = "SO_StageOneOutletWaterPresentation";
            _flowSettings.waterStreamWidth = .55f; _flowSettings.waterStreamSway = 0;
            _flowSettings.waterSprayDropsPerMeterPerSecond = 8f;
            _flowSettings.waterSprayDropSize = new Vector2(.07f, .16f);
            _flowSettings.waterSprayForwardSpeed = 7f;
            _flowSettings.waterSprayMaxDropsPerFrame = 16;
            AssetDatabase.CreateAsset(_flowSettings, "Assets/_Project/Data/Stages/Stage_01/" + _flowSettings.name + ".asset");
            var correction = New("OutletCorrections_20261104", root, Vector3.zero);

            // Photo 1: upper opening takes the leaf and the lower opening takes water.
            var wrong = root.Find("Leaf_1_BlockingEruption");
            var wrongMechanism = wrong.GetComponent<StageOneDocumentMechanism>();
            var oldPlug = Read<LeafInstallTarget>(wrongMechanism, "_leafTarget");
            var upperOpening = oldPlug.SocketPosition;
            var leftSource = all.First(t => t.name == "WallWaterInlet (2)").GetComponent<SapInjectionPort>();
            Vector3 lowerOpening = leftSource.InteractionPoint;
            wrong.gameObject.SetActive(false);
            var pair = New("Photo1_Branch_UpperLeaf_LowerWater", correction, upperOpening);
            pair.gameObject.AddComponent<NetworkObject>(); var pad = pair.gameObject.AddComponent<ExteriorJumpPad>();
            var target = Target("UpperHole_LeafLaunchSocket", upperOpening, Vector3.up, pair, true);
            var inlet = New("LowerHole_WaterInlet", pair, lowerOpening).gameObject.AddComponent<SapInjectionPort>();
            var inletHit = inlet.gameObject.AddComponent<SphereCollider>(); inletHit.radius = 1.1f;
            var aim = New("InjectionAimPoint", inlet.transform, lowerOpening + Vector3.up * .35f); Set(inlet, "_aimPoint", aim);
            var jet = New("UpperHole_LaunchWaterJet", pair, upperOpening).gameObject;
            var line = jet.AddComponent<LineRenderer>(); line.sharedMaterial = _water;
            Set(jet.AddComponent<MushroomWaterJetPresentation>(), "_stream", line); jet.SetActive(false);
            Set(pad, "_settings", AssetDatabase.LoadAssetAtPath<SapJumpPadSettings>("Assets/_Project/Data/Stages/Stage_01/SO_ModelingMushroomJumpPad.asset"));
            SetArray(pad, "_ports", new[] { inlet }); SetArray(pad, "_sockets", new[] { target });
            SetArray(pad, "_leafCenters", new[] { target.Socket }); SetArray(pad, "_jets", new[] { jet });
            var padConfig = new SerializedObject(pad); padConfig.FindProperty("_recallLeafAfterLaunch").boolValue = true; padConfig.ApplyModifiedPropertiesWithoutUndo();

            // Photo 2: only the three 04_Musroom steps, not the adjacent large 03_Mushroom caps.
            var tree = all.First(t => t.name == "seonag_tree").GetComponent<MeshCollider>();
            var route = New("Photo2_MushroomRoute_LeafBlockedOutlets", correction, Vector3.zero);
            var names = new[] { "04_Musroom (10)", "04_Musroom (11)", "04_Musroom (6)" };
            foreach (string name in names)
            {
                var model = all.First(t => t.name == name && t.GetComponent<MeshRenderer>() != null);
                var renderer = model.GetComponent<MeshRenderer>(); var top = SurfaceTop(model.GetComponent<Collider>(), renderer.bounds);
                Vector3 desired = top + Vector3.up * 1.4f;
                Vector3 center = tree.bounds.center; center.y = desired.y;
                Vector3 radial = Vector3.ProjectOnPlane(desired - center, Vector3.up).normalized;
                if (!tree.Raycast(new Ray(desired + radial * 40, -radial), out var bark, 100)) throw new InvalidOperationException("No tree surface by " + name);
                Vector3 normal = Vector3.ProjectOnPlane(bark.normal, Vector3.up).normalized;
                if (normal.sqrMagnitude < .001f) normal = radial;
                Vector3 source = bark.point + normal * .08f;
                var mechanismRoot = New("LeafBlock_" + name, route, source);
                mechanismRoot.gameObject.AddComponent<NetworkObject>(); var mechanism = mechanismRoot.gameObject.AddComponent<StageOneDocumentMechanism>();
                var config = new SerializedObject(mechanism); config.FindProperty("_kind").enumValueIndex = (int)StageOneDocumentMechanismKind.BlockJet; config.ApplyModifiedPropertiesWithoutUndo();
                var mouth = Nozzle("TreeOutlet", source, normal, mechanismRoot);
                var aimPoint = mouth.Find("InjectionAimPoint");
                Vector3 plugPosition = aimPoint != null ? aimPoint.position : source;
                var plug = Target("LeafPlug", plugPosition + normal * .15f, normal, mechanismRoot, false);
                Set(mechanism, "_leafTarget", plug);
                float length = Mathf.Clamp(Vector3.Distance(source, desired) + renderer.bounds.size.magnitude * .12f + 1, 4, 22);
                var stream = Stream("BlockingWaterStream", plugPosition, plugPosition + normal * length, mechanismRoot, true);
                Set(mechanism, "_jet", stream); SetArray(mechanism, "_obstacleColliders", new[] { stream.GetComponent<Collider>() });
            }
            // Same authoritative progress that moves the stone also triggers the visible outlet.
            var eject = root.GetComponentsInChildren<StageOneDocumentMechanism>().First(m => m.Kind == StageOneDocumentMechanismKind.EjectObstacle);
            var ejectConfig = new SerializedObject(eject); var stone = (Transform)ejectConfig.FindProperty("_movingArt").GetArrayElementAtIndex(0).objectReferenceValue;
            Vector3 direction = Vector3.ProjectOnPlane(ejectConfig.FindProperty("_offset").vector3Value, Vector3.up).normalized;
            var outlet = Nozzle("StoneOutlet_BarkMouth", stone.position - direction * .25f, direction, eject.transform);
            var outletAim = outlet.Find("InjectionAimPoint"); Vector3 origin = outletAim != null ? outletAim.position : stone.position;
            var ejectionStream = Stream("StoneOutlet_WaterPush", origin, origin + direction * 7, eject.transform, false);
            ejectionStream.SetActive(false); Set(eject, "_jet", ejectionStream);

            var traversal = root.GetComponent<StageOneTraversal>(); var safety = new SerializedObject(traversal); var bare = safety.FindProperty("_bareTrees");
            var colliders = new List<Object>();
            for (int i = 0; i < bare.arraySize; i++)
            {
                var c = bare.GetArrayElementAtIndex(i).objectReferenceValue as Collider;
                if (c != null && !c.transform.IsChildOf(wrong)) colliders.Add(c);
            }
            SetArray(traversal, "_bareTrees", colliders.ToArray());
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Photo1 paired branch pad; Photo2 three horizontal leaf-blocked outlets; Coop1 stone outlet water cue. Imported model transforms untouched.";
        }
        public static string KeepThreeHorizontalOutlets()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != StageOneDocumentAuthoring.ScenePath) throw new InvalidOperationException("Imported Stage One Edit mode required.");
            var root = GameObject.Find("Stage1_DocumentGimmicks").transform;
            var route = root.Find("OutletCorrections_20261104/Photo2_MushroomRoute_LeafBlockedOutlets");
            var keep = new[] { "LeafBlock_04_Musroom (10)", "LeafBlock_04_Musroom (11)", "LeafBlock_04_Musroom (6)" };
            int removed = 0;
            foreach (var mechanism in route.GetComponentsInChildren<StageOneDocumentMechanism>(true))
            {
                if (!keep.Contains(mechanism.name)) { Object.DestroyImmediate(mechanism.gameObject); removed++; continue; }
                var mouth = mechanism.transform.Find("TreeOutlet");
                var stream = Read<GameObject>(mechanism, "_jet").transform;
                Vector3 direction = Vector3.ProjectOnPlane(stream.Find("FlowEnd").position - stream.Find("FlowSource").position, Vector3.up).normalized;
                if (direction.sqrMagnitude < .001f) throw new InvalidOperationException("No horizontal outlet direction: " + mechanism.name);
                mouth.rotation = Quaternion.LookRotation(direction, Vector3.up);
                var aim = mouth.Find("InjectionAimPoint"); Vector3 source = aim != null ? aim.position : mouth.position;
                var target = Read<LeafInstallTarget>(mechanism, "_leafTarget");
                target.transform.SetPositionAndRotation(source + direction * .15f, mouth.rotation);
                target.Socket.SetPositionAndRotation(target.transform.position, Quaternion.FromToRotation(Vector3.up, direction));
                HorizontalStream(stream, source, direction);
                var fixedStream = stream.GetComponent<StageOneFixedWaterStream>();
                if (Read<CapsuleCollider>(fixedStream, "_pushVolume") != null) RefreshOutletPreview(fixedStream);
            }
            var eject = root.GetComponentsInChildren<StageOneDocumentMechanism>().First(m => m.Kind == StageOneDocumentMechanismKind.EjectObstacle);
            var ejectStream = Read<GameObject>(eject, "_jet").transform;
            var ejectionDirection = Vector3.ProjectOnPlane(ejectStream.forward, Vector3.up).normalized;
            var outlet = eject.transform.Find("StoneOutlet_BarkMouth");
            outlet.rotation = Quaternion.LookRotation(ejectionDirection, Vector3.up);
            var outletAim = outlet.Find("InjectionAimPoint");
            HorizontalStream(ejectStream, outletAim != null ? outletAim.position : outlet.position, ejectionDirection);
            var settings = AssetDatabase.LoadAssetAtPath<SapJumpPadSettings>("Assets/_Project/Data/Stages/Stage_01/SO_StageOneOutletWaterPresentation.asset");
            settings.waterStreamSway = 0; EditorUtility.SetDirty(settings);
            var traversal = root.GetComponent<StageOneTraversal>(); var so = new SerializedObject(traversal); var p = so.FindProperty("_bareTrees");
            var hazards = new List<Object>();
            for (int i = 0; i < p.arraySize; i++) { var c = p.GetArrayElementAtIndex(i).objectReferenceValue; if (c != null) hazards.Add(c); }
            SetArray(traversal, "_bareTrees", hazards.ToArray());
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Removed extra outlets=" + removed + "; three 04_Musroom outlets and stone water cue now perpendicular to the vertical trunk.";
        }
        private static void HorizontalStream(Transform stream, Vector3 source, Vector3 direction)
        {
            float length = Vector3.Distance(stream.Find("FlowSource").position, stream.Find("FlowEnd").position);
            var line = stream.GetComponent<LineRenderer>();
            stream.SetPositionAndRotation(source, line.alignment == LineAlignment.TransformZ
                ? Quaternion.LookRotation(Vector3.down, direction) : Quaternion.LookRotation(direction, Vector3.up));
            stream.Find("FlowSource").position = source; stream.Find("FlowEnd").position = source + direction * length;
            line.SetPositions(new[] { source, source + direction * length });
        }
        private static Vector3 SurfaceTop(Collider collider, Bounds bounds)
        {
            Vector3 top = new(bounds.center.x, bounds.max.y, bounds.center.z);
            if (collider != null && collider.Raycast(new Ray(top + Vector3.up, Vector3.down), out var hit, bounds.size.y + 2)) top = hit.point;
            return top;
        }
        private static Transform Nozzle(string name, Vector3 position, Vector3 normal, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreeSapInletAuthoring.PrefabPath);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent); go.name = name;
            go.transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal, Vector3.up));
            foreach (var port in go.GetComponentsInChildren<SapInjectionPort>(true)) Object.DestroyImmediate(port);
            foreach (var collider in go.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            return go.transform;
        }
        private static LeafInstallTarget Target(string name, Vector3 point, Vector3 normal, Transform parent, bool launch)
        {
            var t = New(name, parent, point); t.rotation = Quaternion.LookRotation(normal, Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > .99f ? Vector3.forward : Vector3.up);
            var target = t.gameObject.AddComponent<LeafInstallTarget>(); var socket = New("LeafSocket", t, point);
            socket.rotation = launch ? Quaternion.identity : Quaternion.FromToRotation(Vector3.up, normal);
            var so = new SerializedObject(target); so.FindProperty("targetId").intValue = ++_id;
            so.FindProperty("socket").objectReferenceValue = socket; so.FindProperty("_singleOccupant").boolValue = true;
            so.FindProperty("_keepLeafUntilUsed").boolValue = launch; so.ApplyModifiedPropertiesWithoutUndo();
            var sphere = t.gameObject.AddComponent<SphereCollider>(); sphere.isTrigger = true; sphere.radius = 1.5f;
            t.gameObject.AddComponent<StageOneLeafZone>();
            var marker = New("LeafInstallMarker", t, point).gameObject.AddComponent<LineRenderer>();
            marker.sharedMaterial = _marker; marker.useWorldSpace = false; marker.loop = true; marker.widthMultiplier = .09f; marker.positionCount = 32;
            for (int i = 0; i < 32; i++) { float a = i * Mathf.PI / 16; marker.SetPosition(i, new Vector3(Mathf.Cos(a) * 1.4f, Mathf.Sin(a) * 1.4f, 0)); }
            return target;
        }
        private static GameObject Stream(string name, Vector3 start, Vector3 end, Transform parent, bool obstacle)
        {
            var t = New(name, parent, start); t.rotation = Quaternion.LookRotation(end - start); t.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            var source = New("FlowSource", t, start); var destination = New("FlowEnd", t, end);
            var line = t.gameObject.AddComponent<LineRenderer>(); line.sharedMaterial = _water; line.useWorldSpace = true;
            line.widthMultiplier = _flowSettings.waterStreamWidth; line.positionCount = 2; line.SetPositions(new[] { start, end }); line.numCapVertices = 5;
            var presentation = t.gameObject.AddComponent<MushroomWaterJetPresentation>(); Set(presentation, "_stream", line);
            var fixedStream = t.gameObject.AddComponent<StageOneFixedWaterStream>(); Set(fixedStream, "_presentationSettings", _flowSettings);
            Set(fixedStream, "_source", source); Set(fixedStream, "_end", destination);
            if (obstacle)
            {
                var capsule = t.gameObject.AddComponent<CapsuleCollider>(); capsule.direction = 2; capsule.radius = .35f;
                capsule.height = Vector3.Distance(start, end) + capsule.radius * 2; capsule.center = Vector3.forward * Vector3.Distance(start, end) * .5f;
                capsule.isTrigger = true;
                Set(fixedStream, "_pushVolume", capsule);
                var pad = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/InteractiveObjects/Stages/Stage_01/PF_MushroomJumpPad.prefab");
                var spray = Read<ParticleSystem>(pad.GetComponentInChildren<MushroomWaterJetPresentation>(true), "_sprayDroplets");
                Set(presentation, "_sprayTemplate", spray);
                RefreshOutletPreview(fixedStream);
            }
            return t.gameObject;
        }

        public static void RefreshOutletPreview(StageOneFixedWaterStream outlet)
        {
            var line = outlet.GetComponent<LineRenderer>();
            var settings = Read<SapJumpPadSettings>(outlet, "_presentationSettings");
            int segments = Mathf.Max(6, settings.waterStreamSegments);
            var points = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                var point = outlet.GetPathPoint((float)i / segments);
                if (!float.IsFinite(point.x) || !float.IsFinite(point.y) || !float.IsFinite(point.z))
                    throw new InvalidOperationException("Invalid water outlet preview point: " + outlet.name);
                points[i] = point;
            }
            var data = new SerializedObject(outlet);
            float mouth = data.FindProperty("_outletWidth").floatValue;
            Undo.RecordObject(line, "Preview curved water outlet");
            line.widthMultiplier = Mathf.Max(mouth, data.FindProperty("_sprayWidth").floatValue);
            line.widthCurve = AnimationCurve.Linear(0f, mouth / line.widthMultiplier, 1f, 1f);
            if (line.alignment == LineAlignment.TransformZ) line.numCapVertices = 0;
            line.positionCount = points.Length;
            line.SetPositions(points);
        }
    }
}
