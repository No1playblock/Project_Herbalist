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
            var source = AssetDatabase.LoadAssetAtPath<SapAbilitySettings>("Assets/PlayerPrototype/SapAbilitySettings.asset");
            var settings = UnityEngine.Object.Instantiate(source); objects.Add(settings);
            settings.controlMode = SapControlMode.Hose;
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
            targetData.FindProperty("acceptsPin").boolValue = true;
            targetData.ApplyModifiedPropertiesWithoutUndo();
            var receiver = wall.AddComponent<SapReceiver>();
            var receiverData = new SerializedObject(receiver);
            receiverData.FindProperty("receiverId").intValue = 2000000000;
            receiverData.FindProperty("leafTarget").objectReferenceValue = target;
            receiverData.ApplyModifiedPropertiesWithoutUndo();
            wall.SetActive(true); Physics.SyncTransforms();
            hose.Tick(held, new Ray(origin, Vector3.forward), true, 1, settings, create);
            Require(marks.Count == 3, "receiver mark");
            var leafSettings = AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>("Assets/PlayerPrototype/LeafAbilitySettings.asset");
            var bindingLeaf = UnityEngine.Object.Instantiate(leafSettings.offlinePrefab); objects.Add(bindingLeaf.gameObject);
            bindingLeaf.Initialize(leafSettings, root.transform, LeafMode.Pin, origin, origin+Vector3.forward*10, _=>{}, true);
            typeof(LeafProjectile).GetMethod("Install", BindingFlags.Instance|BindingFlags.NonPublic).Invoke(bindingLeaf,
                new object[]{target, marks[2].transform.position, Vector3.back, Vector3.forward});
            Require(marks[2].State == SapState.Bound && bindingLeaf.State == LeafState.Bound, "sap-first binding");
            marks[2].Tick(5.1f);
            Require(marks[2].State == SapState.Complete && bindingLeaf.State == LeafState.Installed, "bound expiry releases leaf without recall");

            var legacy = create();
            legacy.Place(receiver, origin+Vector3.forward*4.5f, Vector3.back);
            Require(legacy.IsPlaced, "legacy placement");
            legacy.Tick(settings.lifetime+.1f);
            Require(legacy.State == SapState.Complete, "legacy expiry");

            var playerGo = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PlayerPrototype/Player.prefab"));
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
            Require(leafAbility.TryThrow(LeafMode.Pin, new Ray(playerGo.transform.position + Vector3.up, Vector3.right)), "leaf fired");
            Require(Mathf.Abs(Mathf.DeltaAngle(player.View.BodyYaw, 90)) < .01f, "leaf faces aim");

            // The old single-deposit capacity must not stop hose hits at new positions.
            var testSettings = UnityEngine.Object.Instantiate(settings); objects.Add(testSettings);
            testSettings.capacity = 2;
            var controlData = new SerializedObject(sap);
            controlData.FindProperty("settings").objectReferenceValue = testSettings;
            controlData.ApplyModifiedPropertiesWithoutUndo();
            sap.Configure(create, _=>{}, true);
            var createHoseMark = typeof(SapControlAbility).GetMethod("CreateHoseMark", BindingFlags.Instance|BindingFlags.NonPublic);
            for (int i=0; i<3; i++)
                Require(createHoseMark.Invoke(sap,null) != null, "hose ignores legacy placement capacity");
            return "PASS: empty-space spray, advancing stream, wall hit, merge/growth/cap, release, refreshed 5s expiry, body-fixed chest hover, leaf-facing, bound expiry/release, legacy placement, near-to-far spray, legacy capacity isolation";
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
