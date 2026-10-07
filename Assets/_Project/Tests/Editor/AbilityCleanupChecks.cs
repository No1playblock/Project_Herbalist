using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Herbalist.Abilities;
using Object = UnityEngine.Object;
namespace Herbalist.Editor
{
    public static class AbilityCleanupChecks
    {
        private static void Check(bool value, string label) { if (!value) throw new Exception(label); }
        private static void Field(Object obj, string field, Object value)
        { var data = new SerializedObject(obj); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Id(Object obj, string field, int value)
        { var data = new SerializedObject(obj); data.FindProperty(field).intValue = value; data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void Install(LeafProjectile leaf, LeafInstallTarget target, Vector3 point)
        { typeof(LeafProjectile).GetMethod("Install", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(leaf, new object[] { target, point, Vector3.back, Vector3.forward }); }
        public static string Authored()
        {
            Check(!Application.isPlaying, "Edit mode required");
            string current = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            int sceneCount = 0;
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/_Project/Scenes" }))
                {
                    var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid)); sceneCount++;
                    foreach (var root in scene.GetRootGameObjects())
                        foreach (var t in root.GetComponentsInChildren<Transform>(true))
                            Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0, "Missing component " + scene.name + "/" + t.name);
                }
                foreach (string path in new[] { "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_LeafProjectile.prefab", "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_NetworkLeafProjectile.prefab" })
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    var data = new SerializedObject(prefab.GetComponent<LeafProjectile>());
                    Check(data.FindProperty("_visual").objectReferenceValue != null && data.FindProperty("_surface").objectReferenceValue != null && data.FindProperty("_attachmentTip").objectReferenceValue != null, "Leaf references " + path);
                    Check(prefab.GetComponentsInChildren<Collider>(true).Length == 1 && prefab.transform.Find("Platform") == null, "Single leaf collider/visual " + path);
                }
                return "PASS: " + sceneCount + " scenes without missing scripts; both leaf prefabs have one collider and preserved tip/visual references.";
            }
            finally { if (!string.IsNullOrEmpty(current)) EditorSceneManager.OpenScene(current); }
        }
        public static string Run()
        {
            var root = new GameObject("TemporaryAbilityCleanupCheck"); root.transform.position = Vector3.one * 8000;
            var leafSettings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>("Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset"));
            var waterSettings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<SapAbilitySettings>("Assets/_Project/Data/Abilities/Sap/SO_SapAbilitySettings.asset"));
            leafSettings.permanentInstallation = true; waterSettings.hoseMarkLifetime = .2f;
            try
            {
                var surface = new GameObject("Wall"); surface.SetActive(false); surface.transform.SetParent(root.transform, false);
                surface.AddComponent<BoxCollider>();
                var target = surface.AddComponent<LeafInstallTarget>(); Id(target, "targetId", 9000001);
                var receiver = surface.AddComponent<SapReceiver>(); Id(receiver, "receiverId", 9000001); surface.SetActive(true);
                int installedEvents = 0, removedEvents = 0;
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                ((UnityEngine.Events.UnityEvent)typeof(LeafInstallTarget).GetField("onInstalled", flags).GetValue(target)).AddListener(() => installedEvents++);
                ((UnityEngine.Events.UnityEvent)typeof(LeafInstallTarget).GetField("onReleased", flags).GetValue(target)).AddListener(() => removedEvents++);
                var leaf = Object.Instantiate(leafSettings.offlinePrefab, root.transform);
                var leafVisual = (GameObject)new SerializedObject(leaf).FindProperty("_visual").objectReferenceValue;
                Vector3 installedVisualScale = leafVisual.transform.localScale;
                leaf.Initialize(leafSettings, root.transform, root.transform.position - Vector3.forward * 3, root.transform.position, _ => { }, true);
                Check(Vector3.Distance(leafVisual.transform.localScale, installedVisualScale * .5f) < .0001f, "Flying leaf is half-size");
                Install(leaf, target, target.transform.position);
                Check(Vector3.Distance(leafVisual.transform.localScale, installedVisualScale) < .0001f, "Installed leaf reaches full size");
                Check(target.HasLeaf && installedEvents == 1, "Installation event");
                var leak = surface.AddComponent<Herbalist.Levels.MazeLeakSocket>(); Field(leak, "_target", target);
                var wet = new GameObject("WetSurface"); wet.transform.SetParent(surface.transform, false); Field(leak, "_wetSurface", wet);
                leak.Present(true); Check(leak.Blocked && !wet.activeSelf, "Leaf blocks leak without water");
                leaf.Tick(leafSettings.lifetime * 10); Check(leaf.Installed, "Permanent leaf without water");
                var water = Object.Instantiate(waterSettings.offlinePrefab, root.transform);
                water.Initialize(waterSettings, target.transform.position, _ => { }, true);
                var leafCollider = (Collider)new SerializedObject(leaf).FindProperty("_surface").objectReferenceValue;
                leafCollider.enabled = false; Physics.SyncTransforms();
                Check(Physics.Raycast(new Ray(surface.transform.position - Vector3.forward * 3, Vector3.forward), out var hit, 5), "Water test receiver hit");
                SapDeposit.ApplyHoseHit(hit, waterSettings, .1f, () => water);
                leafCollider.enabled = true;
                Check(water.IsPlaced && receiver.Supplied, "Hose creates receiver mark");
                leaf.BeginReturn(); Check(water.IsPlaced && receiver.Supplied && removedEvents == 1, "Recall leaves water intact and releases target");
                Check(Vector3.Distance(leafVisual.transform.localScale, installedVisualScale * .5f) < .0001f, "Returning leaf is half-size");
                leak.Present(true); Check(!leak.Blocked && wet.activeSelf, "Recall reopens leak even while water remains");
                Install(leaf, target, target.transform.position);
                water.Tick(.3f); Check(water.State == SapState.Complete && leaf.Installed, "Water expiry leaves leaf intact");
                var socketRoot = new GameObject("SocketTarget"); socketRoot.SetActive(false); socketRoot.transform.SetParent(root.transform, false); socketRoot.transform.localPosition = Vector3.right * 20;
                var socketTarget = socketRoot.AddComponent<LeafInstallTarget>(); Id(socketTarget, "targetId", 9000002);
                var socket = new GameObject("Socket").transform; socket.SetParent(socketRoot.transform, false); socket.localPosition = Vector3.up;
                socket.rotation = Quaternion.Euler(0, 67, 0); Field(socketTarget, "socket", socket);
                var data = new SerializedObject(socketTarget); data.FindProperty("_singleOccupant").boolValue = true; data.ApplyModifiedPropertiesWithoutUndo(); socketRoot.SetActive(true);
                leaf.Finish(); leaf.Initialize(leafSettings, root.transform, socket.position + Vector3.up * 3, socket.position, _ => { }, true);
                Install(leaf, socketTarget, socketRoot.transform.position);
                Check(Vector3.Distance(leaf.transform.position, socket.position) < .001f && Quaternion.Angle(leaf.transform.rotation, socket.rotation) < .001f && !socketTarget.CanInstall, "Socket pose/occupancy");
                socket.position += Vector3.up * 4; leaf.Tick(.01f);
                Check(Vector3.Distance(leaf.transform.position, socket.position) < .001f, "Leaf follows moving socket");
                leaf.Finish(); Check(socketTarget.CanInstall, "Socket reopens on recall/removal");
                return "PASS: permanent leaf, target events, independent water/leaf lifetimes, leak blockage/recall, socket position/orientation/occupancy and moving socket.";
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(leafSettings); Object.DestroyImmediate(waterSettings); }
        }
    }
}
