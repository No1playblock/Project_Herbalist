using System;
using System.Collections.Generic;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Player;
using UnityEditor;
using UnityEngine;

public static class SapHoseChecks
{
    public static string Run()
    {
        var objects = new List<UnityEngine.Object>();
        var marks = new List<SapDeposit>();
        try
        {
            var source = AssetDatabase.LoadAssetAtPath<SapAbilitySettings>("Assets/_Project/Data/Abilities/Sap/SO_SapAbilitySettings.asset");
            var settings = UnityEngine.Object.Instantiate(source); objects.Add(settings);
            var root = new GameObject("HoseCheckTemporary"); objects.Add(root);
            Vector3 origin = new Vector3(10000, 10000, 10000);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); objects.Add(wall);
            wall.transform.position = origin + Vector3.forward * 5; wall.transform.localScale = new Vector3(10, 10, 1);
            var held = UnityEngine.Object.Instantiate(settings.offlinePrefab); objects.Add(held.gameObject);
            held.Initialize(settings, origin, _ => {}, true);
            Func<SapDeposit> create = () => {
                var mark = UnityEngine.Object.Instantiate(settings.offlinePrefab); objects.Add(mark.gameObject);
                mark.Initialize(settings, origin, _ => {}, true);
                typeof(SapDeposit).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(mark, null);
                marks.Add(mark); return mark;
            };
            Physics.SyncTransforms();
            var hose = new SapHoseSprayer();
            hose.Tick(held, new Ray(origin, Vector3.up), true, 1, settings, create);
            Require(held.HasStream && held.HoseStream && marks.Count == 0, "empty-space spray");
            Require(Vector3.Distance(held.StreamDestination, origin) > 1, "advancing stream");
            hose.Reset();
            hose.Tick(held, new Ray(origin, Vector3.forward), true, 1, settings, create);
            Require(marks.Count == 1 && marks[0].IsPlaced, "wall mark");
            var decal = marks[0].GetComponentInChildren<SapSurfaceMarkPresentation>(true);
            Require(decal != null && decal.Available && decal.gameObject.activeInHierarchy, "authored surface decal presentation");
            var volume = marks[0].GetComponentInChildren<SapSurfaceVolumePresentation>(true);
            Require(volume != null && volume.Available && volume.gameObject.activeInHierarchy, "authored raised surface volume");
            Require(Mathf.Approximately(volume.transform.localScale.z, settings.surfaceVolumeThickness), "configured liquid thickness");
            Require(!marks[0].Visual.gameObject.activeSelf, "legacy blob hidden while decal is active");
            float first = marks[0].MarkScale;
            marks[0].Tick(4);
            hose.Tick(held, new Ray(origin, Vector3.forward), true, 1, settings, create);
            Require(marks.Count == 1 && marks[0].MarkScale > first, "merge and grow");
            hose.Tick(held, new Ray(origin, Vector3.forward), false, 1, settings, create);
            Require(!held.HasStream, "release stops stream");
            marks[0].Tick(4.9f); Require(marks[0].IsPlaced, "last-hit lifetime refreshed");
            marks[0].Tick(.11f); Require(marks[0].State == SapState.Complete, "five-second expiry");

            // Expired marks no longer receive hits.
            hose.Tick(held, new Ray(origin, Vector3.forward), true, 1, settings, create);
            Require(marks.Count == 2, "new mark after expiry");
            hose.Tick(held, new Ray(origin, Vector3.forward), true, 100, settings, create);
            Require(marks[1].MarkScale <= settings.hoseMaxScale, "bounded growth");
            wall.transform.localScale = Vector3.one;
            var farWall = GameObject.CreatePrimitive(PrimitiveType.Cube); objects.Add(farWall);
            farWall.transform.position = origin + new Vector3(.35f, 0, .85f) * settings.controlRange;
            farWall.transform.localScale = new Vector3(3, 3, 1);
            Physics.SyncTransforms();
            hose.Tick(held, new Ray(origin, new Vector3(.35f, 0, .85f).normalized), true, 1, settings, create);
            Require(Vector3.Distance(held.StreamDestination, origin) > settings.controlRange * .6f, "near-to-far continuous spray");
            Require(marks.Count == 3, "far target hit without release");
            marks[2].Finish(); marks.RemoveAt(2);
            UnityEngine.Object.DestroyImmediate(farWall);


            marks[1].Finish();
            wall.SetActive(false);
            var target = wall.AddComponent<LeafInstallTarget>();
            var targetData = new SerializedObject(target);
            targetData.FindProperty("targetId").intValue = 2000000000;
            targetData.ApplyModifiedPropertiesWithoutUndo();
            var receiver = wall.AddComponent<SapReceiver>();
            var receiverData = new SerializedObject(receiver);
            receiverData.FindProperty("receiverId").intValue = 2000000000;
            receiverData.ApplyModifiedPropertiesWithoutUndo();
            wall.SetActive(true); Physics.SyncTransforms();
            hose.Tick(held, new Ray(origin, Vector3.forward), true, 1, settings, create);
            Require(marks.Count == 3, "receiver mark");
            var leafSettings = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>("Assets/_Project/Data/Abilities/Leaf/SO_LeafAbilitySettings.asset"));
            objects.Add(leafSettings); leafSettings.permanentInstallation = true;
            var leaf = UnityEngine.Object.Instantiate(leafSettings.offlinePrefab); objects.Add(leaf.gameObject);
            leaf.Initialize(leafSettings, root.transform, origin, origin + Vector3.forward * 10, _ => {}, true);
            typeof(LeafProjectile).GetMethod("Install", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(leaf,
                new object[] { target, marks[2].transform.position, Vector3.back, Vector3.forward });
            Require(leaf.Installed && marks[2].IsPlaced, "leaf and water install independently");
            leaf.BeginReturn(); Require(marks[2].IsPlaced && receiver.Supplied, "leaf recall does not drain water");
            marks[2].Tick(settings.hoseMarkLifetime + .1f);
            Require(marks[2].State == SapState.Complete, "water expires independently after recall");
            leaf.Finish();
            hose.Tick(held, new Ray(origin, Vector3.forward), true, 100, settings, create);
            var water = marks[marks.Count - 1];
            leaf.Initialize(leafSettings, root.transform, origin, origin + Vector3.forward * 10, _ => {}, true);
            typeof(LeafProjectile).GetMethod("Install", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(leaf,
                new object[] { target, water.transform.position, Vector3.back, Vector3.forward });
            water.Tick(settings.hoseMarkLifetime + .1f); leaf.Tick(leafSettings.lifetime * 10);
            Require(water.State == SapState.Complete && leaf.Installed, "water expiry does not remove permanent leaf");
            leaf.Finish();

            var playerGo = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PF_Player.prefab"));
            objects.Add(playerGo);
            var player = playerGo.GetComponent<PlayerController>();
            player.View.Initialize();
            var sap = playerGo.GetComponent<SapControlAbility>();
            typeof(SapControlAbility).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(sap, null);
            player.View.SetBodyYaw(30);
            var hoverA = sap.HoverPosition(new Ray(origin, Vector3.forward));
            var hoverB = sap.HoverPosition(new Ray(origin, Vector3.right));
            Require(Vector3.Distance(hoverA, hoverB) < .001f, "body-relative hover independent of camera");
            Require(Mathf.Abs((hoverA-playerGo.transform.position).y - settings.hoseHoverOffset.y) < .001f, "chest height");

            var leafAbility = playerGo.GetComponent<LeafThrowAbility>();
            leafAbility.Configure(() => {
                var leaf = UnityEngine.Object.Instantiate(leafAbility.Settings.offlinePrefab); objects.Add(leaf.gameObject); return leaf;
            }, _ => {}, true);
            Require(leafAbility.TryThrow(new Ray(playerGo.transform.position + Vector3.up, Vector3.right)), "leaf fired");
            Require(Mathf.Abs(Mathf.DeltaAngle(player.View.BodyYaw, 90)) < .01f, "leaf faces aim");

            return "PASS: empty-space spray, advancing stream, wall hit, merge/growth/cap, release, refreshed expiry, body-fixed chest hover, leaf-facing, independent water/leaf lifetimes, near-to-far spray";
        }
        finally
        {
            foreach (var mark in marks) if (mark != null)
                typeof(SapDeposit).GetMethod("OnDisable", BindingFlags.Instance|BindingFlags.NonPublic).Invoke(mark, null);
            for (int i=objects.Count-1; i>=0; i--) if (objects[i] != null) UnityEngine.Object.DestroyImmediate(objects[i]);
        }
    }
    private static void Require(bool value, string name) { if (!value) throw new Exception("Hose check failed: " + name); }
}
