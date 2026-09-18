using System;
using Herbalist.Abilities;
using Herbalist.Player;
using UnityEditor;
using UnityEngine;
namespace Herbalist.Editor
{
    public static class SapAbilityChecks
    {
        public static string Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Play mode required");
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            var manager = player.GetComponent<PlayerAbilityController>();
            var sap = manager.Sap;
            bool wasEnabled = manager.enabled; bool wasUnlocked = manager.Unlocked; var originalKind = manager.Kind; var originalPosition = player.transform.position;
            var fixtures = new GameObject("SapCheckFixtures"); fixtures.SetActive(false);
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube); board.transform.SetParent(fixtures.transform);
            board.transform.position = new Vector3(100, 2, 8); board.transform.localScale = new Vector3(8, 4, 0.5f);
            var target = board.AddComponent<LeafInstallTarget>(); SetInt(target, "targetId", 9001); SetBool(target, "acceptsPin", true);
            var receiver = board.AddComponent<SapReceiver>(); SetInt(receiver, "receiverId", 9001); SetObject(receiver, "leafTarget", target);
            var sourceObject = new GameObject("Source", typeof(SapSource)); sourceObject.transform.SetParent(fixtures.transform); sourceObject.transform.position = new Vector3(100, 1, 1);
            fixtures.SetActive(true);
            var settings = sap.Settings;
            var created = new System.Collections.Generic.List<GameObject>();
            uint cycle = 0, use = 0;
            Ray aim = new Ray(new Vector3(100, 1, -1), Vector3.forward);
            Action<bool> tick = fire => manager.Tick(cycle, fire ? ++use : use, aim, 1f / 60);
            Action extract = () => { cycle++; tick(false); Require(sap.Controlling, "extract"); for (int i=0;i<150;i++) tick(false); };
            Action<SapDeposit> arrive = value => { for (int i=0;i<300 && value.State == SapState.Flying;i++) value.Tick(1f/60); };
            Func<SapDeposit> place = () =>
            {
                extract(); var held = sap.Held;
                Require(sap.Ready && Vector3.Distance(held.transform.position, sap.HoverPosition(aim)) < settings.arrivalTolerance, "hover beside face");
                Require(sap.CanPlace, "valid placement preview");
                var launch = held.transform.position; tick(true);
                Require(held.State == SapState.Flying && !sap.Controlling && !player.Motor.MovementLocked, "launch and unlock");
                Require(Vector3.Distance(held.StreamStart, launch) < 0.001f, "flight stream starts at held blob");
                arrive(held); Require(held.State == SapState.Attached && !held.HasStream, "arrival installs and ends stream"); return held;
            };
            Func<float, LeafProjectile> leafAt = x =>
            {
                var leaf = UnityEngine.Object.Instantiate(manager.Leaf.Settings.offlinePrefab); created.Add(leaf.gameObject);
                leaf.Initialize(manager.Leaf.Settings, player.transform, LeafMode.Pin, new Vector3(x,1,1), new Vector3(x,1,10), l => {}, true);
                for (int i=0;i<60 && leaf.State == LeafState.Flying;i++) leaf.Tick(1f/60);
                Require(leaf.Installed, "leaf installed"); return leaf;
            };
            try
            {
                manager.enabled = false; manager.RevokeLeaf(); Require(manager.UnlockSap(), "unlock sap");
                sap.Configure(() => { var value = UnityEngine.Object.Instantiate(settings.offlinePrefab); created.Add(value.gameObject); return value; }, value => {}, true);
                player.Motor.Teleport(new Vector3(100, 0, 0)); Physics.SyncTransforms();
                extract(); Require(player.Motor.MovementLocked && !player.Motor.TryJump(), "control movement and jump lock");
                cycle++; tick(false); Require(!sap.Controlling && !player.Motor.MovementLocked && sap.ActiveCount == 0, "cancel cleanup");
                var first = place(); Require(receiver.Supplied && !player.Motor.MovementLocked, "receiver supplied and unlock");
                first.Tick(settings.lifetime - 0.1f);
                extract(); var refresh = sap.Held; tick(true); arrive(refresh); Require(sap.ActiveCount == 1, "same position refresh without duplicate");
                first.Tick(0.2f); Require(first.State == SapState.Attached, "refreshed timer");
                first.Tick(settings.lifetime); Require(first.State == SapState.Complete && !receiver.Supplied, "expiry and receiver drained");
                var bound = place(); var leaf = leafAt(100);
                Require(leaf.State == LeafState.Bound && bound.State == SapState.Bound, "sap-first binding");
                bound.Tick(settings.lifetime * 3); Require(bound.State == SapState.Bound, "bound persistence");
                leaf.BeginReturn(); Require(bound.State == SapState.Complete && !receiver.Supplied, "recall clears sap");
                var separate = place(); var distant = leafAt(102.5f);
                Require(distant.State == LeafState.Installed && separate.State == SapState.Attached, "position-specific binding"); distant.Finish(); separate.Finish();
                var earlyLeaf = leafAt(100); var lateSap = place();
                Require(earlyLeaf.State == LeafState.Installed && lateSap.State == SapState.Attached, "leaf-first never binds retroactively"); earlyLeaf.Finish(); lateSap.Finish();
                extract(); aim = new Ray(new Vector3(100,1,0), Vector3.down); tick(true); Require(sap.Controlling && !sap.CanPlace, "unsupported surface rejected");
                cycle++; tick(false);
                player.Motor.Teleport(new Vector3(120,0,0)); cycle++; tick(false); Require(!sap.Controlling, "extraction range enforced");
                return "PASS: sap unlock, extract, face-right hover, launch, arrival, stream lifecycle, placement, movement/jump lock, cancel, refresh, expiry, receiver events, positional/ordered binding, persistent bound sap, recall cleanup, invalid placement, range.";
            }
            finally
            {
                sap.Clear(); foreach (var go in created) if (go != null) UnityEngine.Object.Destroy(go);
                UnityEngine.Object.Destroy(fixtures); player.Motor.Teleport(originalPosition);
                sap.Configure(() => UnityEngine.Object.Instantiate(settings.offlinePrefab), value => UnityEngine.Object.Destroy(value.gameObject), false);
                manager.RevokeLeaf();
                manager.Tick(manager.Input.CycleSequence, manager.Input.UseSequence, aim, 0);
                if (wasUnlocked) { if (originalKind == PlayerAbilityKind.Sap) manager.UnlockSap(); else manager.UnlockLeaf(); }
                manager.enabled = wasEnabled;
            }
        }
        private static void Require(bool value, string label) { if (!value) throw new Exception("Sap check failed: " + label); }
        private static void SetInt(UnityEngine.Object target, string name, int value) { var so=new SerializedObject(target);so.FindProperty(name).intValue=value;so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetBool(UnityEngine.Object target, string name, bool value) { var so=new SerializedObject(target);so.FindProperty(name).boolValue=value;so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetObject(UnityEngine.Object target, string name, UnityEngine.Object value) { var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
