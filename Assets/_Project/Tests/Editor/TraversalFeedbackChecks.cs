using System;
using System.Linq;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class TraversalFeedbackChecks
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static T Read<T>(object obj, string field) => (T)obj.GetType().GetField(field, Private).GetValue(obj);
        private static void Call(object obj, string method, params object[] args) => obj.GetType().GetMethod(method, Private).Invoke(obj, args);
        private static void Check(bool value, string message) { if (!value) throw new Exception(message); }

        public static string ClothLoweringVisualsPlay() => ClothSkinBindingChecks.StartLowering();

        public static string ClothReplacementGravityPlay() => ClothSkinBindingChecks.StartLowering();

        public static string LeafReuse()
        {
            Check(!Application.isPlaying, "Edit mode required.");
            var owner = new GameObject("LeafReuseProbe") { hideFlags = HideFlags.HideAndDontSave };
            owner.SetActive(false);
            owner.transform.position = Vector3.down * 10000;
            var target = owner.AddComponent<LeafInstallTarget>();
            var so = new SerializedObject(target);
            so.FindProperty("targetId").intValue = 99999901;
            so.FindProperty("socket").objectReferenceValue = owner.transform;
            so.FindProperty("_singleOccupant").boolValue = true;
            so.FindProperty("_reuseDelayAfterRelease").floatValue = 20;
            so.ApplyModifiedPropertiesWithoutUndo();
            owner.SetActive(true);
            var rules = AssetDatabase.LoadAssetAtPath<StageAbilityRules>("Assets/_Project/Data/Stages/Stage_01/SO_ImportedStageOneRules.asset");
            var settings = UnityEngine.Object.Instantiate(rules.leaf);
            settings.permanentInstallation = false; settings.lifetime = 1;
            LeafProjectile first = null, second = null;
            try
            {
                first = UnityEngine.Object.Instantiate(settings.offlinePrefab);
                first.Initialize(settings, owner.transform, owner.transform.position, owner.transform.position, null, true);
                Check(first.TryInstallAtSocket(target), "Initial installation accepted.");
                Call(target, "TickReuseDelay", 30f);
                first.BeginReturn();
                Check(!target.CanInstall, "Cooldown starts at recall, not initial installation.");
                second = UnityEngine.Object.Instantiate(settings.offlinePrefab);
                second.Initialize(settings, owner.transform, owner.transform.position, owner.transform.position, null, true);
                Check(!second.TryInstallAtSocket(target), "Real projectile cannot install during cooldown.");
                Call(target, "TickReuseDelay", 19.99f);
                Check(!target.CanInstall, "Still blocked just before 20 seconds.");
                // Repeated cleanup of the returning leaf must not restart the cooldown.
                target.Detach(first);
                Call(target, "TickReuseDelay", .02f);
                Check(second.TryInstallAtSocket(target), "Installation allowed after 20 seconds.");
                second.Tick(1.1f);
                Check(second.State == LeafState.Returning && !target.CanInstall, "Automatic expiry also starts cooldown.");
                Call(target, "TickReuseDelay", 20.01f);
                Check(target.CanInstall, "Automatic-expiry cooldown ends normally.");

                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                var targets = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<LeafInstallTarget>(true)).Where(t => t.isActiveAndEnabled).ToArray();
                var branches = targets.Where(t => t.name.StartsWith("LeafBranch_") && t.GetComponent<StageOneLeafUse>() != null).ToArray();
                Check(branches.Length == 4 && branches.All(t => Mathf.Approximately(Read<float>(t, "_reuseDelayAfterRelease"), 20f)), "Four ordinary branch targets have 20 seconds.");
                Check(targets.Where(t => !branches.Contains(t)).All(t => Read<float>(t, "_reuseDelayAfterRelease") == 0), "Other targets, jump pads and outlet plugs keep zero cooldown.");
                return "PASS: recall starts 20-second restriction; blocked projectile rejected at 19.99s; accepted after 20s; duplicate detach does not restart; expiry also starts delay; only four branch targets configured.";
            }
            finally
            {
                if (first != null) UnityEngine.Object.DestroyImmediate(first.gameObject);
                if (second != null) UnityEngine.Object.DestroyImmediate(second.gameObject);
                UnityEngine.Object.DestroyImmediate(owner); UnityEngine.Object.DestroyImmediate(settings);
            }
        }

        public static string MazeThinWall(SapMazeBoard board)
        {
            var surface = board.Surface;
            Check(surface.Build(board.transform), "Surface builds.");
            var from = new Vector2(8.39999f, 2.75018f);
            var to = new Vector2(8.51999f, 2.75018f);
            Check(!surface.IsSegmentOpen(from, to), "Previously missed Maze2 thin wall blocks direct transfer.");
            Check(surface.Move(from, to - from).x < to.x - .001f, "Movement cannot cross the previously missed wall.");
            return "PASS: reproduced Maze2 thin-wall crossing is blocked for both segments and actual movement.";
        }

        public static string Run()
        {
            Check(!Application.isPlaying, "Edit mode required.");
            var s = EditorSceneManager.OpenPreviewScene("Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity");
            try
            {
                var pulley = s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<StageTwoPulleyPresentation>(true)).Single();
                var runs = Read<Transform[]>(pulley, "_ropeRuns");
                var positions = runs.Select(t => t.position).ToArray();
                var children = runs.Select(t => t.GetComponentsInChildren<Transform>(true).Select(c => c.localPosition).ToArray()).ToArray();
                Call(pulley, "CacheRestPose");
                foreach (float descent in new[] { 8f, 17f, 64f, 164f, 0f })
                {
                    pulley.Present(descent);
                    for (int i = 0; i < runs.Length; i++)
                    {
                        Check(Vector3.Distance(runs[i].position, positions[i] + Vector3.down * descent) < .0001f, "Whole rope follows raft past former wrap distance.");
                        var parts = runs[i].GetComponentsInChildren<Transform>(true);
                        for (int j = 1; j < parts.Length; j++) Check(parts[j].localPosition == children[i][j], "Rope links keep authored spacing.");
                    }
                }
                var level = s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<StageLevel>(true)).Single();
                Check(!level.abilityRules.sap.orbitEnabled, "Stage Two disables orbit on actual rules.");
                var standard = AssetDatabase.LoadAssetAtPath<SapAbilitySettings>("Assets/_Project/Data/Stages/Revision_260928/SO_RevisionSap.asset");
                Check(standard.orbitEnabled, "Shared sap settings remain unchanged.");
            }
            finally { EditorSceneManager.ClosePreviewScene(s); }

            var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var streams = active.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<StageOneFixedWaterStream>(true))
                .Where(w => w.isActiveAndEnabled && Read<CapsuleCollider>(w, "_pushVolume") != null).ToArray();
            var dummy = new GameObject("WaterContactProbe") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var controller = dummy.AddComponent<CharacterController>(); controller.height = 2; controller.radius = .3f;
                foreach (var stream in streams)
                {
                    var direction = Vector3.ProjectOnPlane(stream.GetPathPoint(1) - stream.GetPathPoint(0), Vector3.up).normalized;
                    var side = Vector3.Cross(direction, Vector3.up);
                    float width = Mathf.Lerp(Read<float>(stream, "_outletWidth"), Read<float>(stream, "_sprayWidth"), .85f);
                    dummy.transform.position = stream.GetPathPoint(.85f) + side * (width * .45f);
                    Physics.SyncTransforms();
                    Check((bool)stream.GetType().GetMethod("Touches", Private).Invoke(stream, new object[] { controller }), "Full spray width pushes, including between droplets.");
                    dummy.transform.position += side * 30;
                    Physics.SyncTransforms();
                    Check(!(bool)stream.GetType().GetMethod("Touches", Private).Invoke(stream, new object[] { controller }), "Outside spray is unaffected.");
                }
                Check(streams.Length == 3, "Three authored blocking streams checked.");
                foreach (var m in active.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<StageOneDocumentMechanism>(true))
                    .Where(m => m.isActiveAndEnabled && m.Kind == StageOneDocumentMechanismKind.GrowingMushrooms))
                    Check(Read<Vector3>(m, "_growthWorldOffset").magnitude <= 1.5f, "Growing mushroom stays attached while moving slightly outside the tree.");
            }
            finally { UnityEngine.Object.DestroyImmediate(dummy); }
            return "PASS: eight complete ropes follow 0/8/17/64/164m without wrapping; Stage Two orbit off only; all three widening streams have full-width contact and reject distant players; mushroom growth keeps attachment pivots.";
        }
    }
}
