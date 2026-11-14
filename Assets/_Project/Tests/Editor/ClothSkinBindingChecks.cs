using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class ClothSkinBindingChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static T Read<T>(object obj, string field) => (T)obj.GetType().GetField(field, Private).GetValue(obj);
        public static string Pose()
        {
            if (!Application.isPlaying) throw new Exception("Offline Play required.");
            var rows = new List<string>();
            foreach (var binding in UnityEngine.Object.FindObjectsByType<ClothEndpointBinding>(FindObjectsSortMode.None))
            {
                var cloth = binding.GetComponent<Cloth>(); var skin = binding.GetComponent<SkinnedMeshRenderer>();
                if (Vector3.Distance(binding.transform.lossyScale, Vector3.one) > .001f)
                    throw new Exception("Cloth presentation must use a unit-scale frame.");
                if (!cloth.enabled)
                {
                    if (Read<HangingCloth>(binding, "_driver").name != "Coop_4_ReplacementCloth")
                        throw new Exception("Only the rotating branch uses the endpoint skin without native Cloth.");
                    rows.Add(binding.name + ": follows the authored top and knot bones");
                    continue;
                }
                if (!skin.enabled) { rows.Add(binding.name + ": fully retracted; native Cloth retained"); continue; }
                var mesh = skin.sharedMesh; var source = mesh.vertices; var weights = mesh.boneWeights;
                var poses = mesh.bindposes; var bones = skin.bones;
                var m0 = bones[0].localToWorldMatrix * poses[0]; var m1 = bones[1].localToWorldMatrix * poses[1];
                var seen = new HashSet<Vector3>(); var rest = new List<Vector3>(); var skinWeights = new List<BoneWeight>();
                for (int i = 0; i < source.Length; i++) if (seen.Add(source[i])) { rest.Add(source[i]); skinWeights.Add(weights[i]); }
                var particles = cloth.vertices; var coefficients = cloth.coefficients;
                if (rest.Count != particles.Length) throw new Exception("Welded particle mapping differs.");
                float pinnedError = 0, simulatedMotion = 0; int fixedCount = 0, freeCount = 0;
                for (int i = 0; i < particles.Length; i++)
                {
                    var w = skinWeights[i];
                    var expected = m0.MultiplyPoint3x4(rest[i]) * w.weight0 + m1.MultiplyPoint3x4(rest[i]) * w.weight1;
                    // Unity exposes Cloth particles in the skin root-bone frame.
                    float error = Vector3.Distance(expected, skin.rootBone.TransformPoint(particles[i]));
                    if (coefficients[i].maxDistance == 0) { pinnedError = Mathf.Max(pinnedError, error); fixedCount++; }
                    else { simulatedMotion = Mathf.Max(simulatedMotion, error); freeCount++; }
                }
                if (pinnedError > .01f || fixedCount == 0 || freeCount == 0)
                    throw new Exception(binding.name + " endpoint error=" + pinnedError);
                rows.Add(binding.name + ": pinned error=" + pinnedError.ToString("F6") + "m, middle motion=" + simulatedMotion.ToString("F4") + "m");
            }
            if (rows.Count != 5) throw new Exception("Five active Cloth bindings required.");
            return string.Join("\n", rows);
        }

        private static int _phase, _frame;
        private static double _start;
        private static readonly List<string> Results = new List<string>();
        public static string StartLowering()
        {
            if (!Application.isPlaying) throw new Exception("Fresh offline Play required.");
            EditorApplication.update -= Step;
            Results.Clear(); _phase = 0; _frame = Time.frameCount; _start = EditorApplication.timeSinceStartup;
            foreach (string name in new[] { "Coop_1_SingleUseCloth", "Coop_2_WaterCloth" })
            {
                var driver = GameObject.Find("Stage1_DocumentGimmicks/" + name).GetComponent<HangingCloth>();
                Read<SapInjectionPort>(driver, "_loweringPort").Inject(.02f);
            }
            System.IO.Directory.CreateDirectory("Temp/ClothSkinBinding_20261109");
            System.IO.File.WriteAllText("Temp/ClothSkinBinding_20261109/result.txt", "RUNNING");
            EditorApplication.update += Step;
            return "Started real-frame lowering and branch-deployment Cloth checks; result: Temp/ClothSkinBinding_20261109/result.txt";
        }
        private static void Step()
        {
            if (!Application.isPlaying) { EditorApplication.update -= Step; return; }
            try
            {
                // Observe native simulation after actual game frames, not only undeformed source bounds.
                if (Time.frameCount < _frame + 3) return;
                if (_phase == 0 && EditorApplication.timeSinceStartup - _start < 1.5)
                { Pose(); return; }
                if (_phase == 0)
                {
                    Results.Add("LOWERING PASS\n" + Pose());
                    var first = GameObject.Find("Stage1_DocumentGimmicks/Coop_4_SingleUseCloth").GetComponent<HangingCloth>();
                    typeof(HangingCloth).GetField("_used", Private).SetValue(first, true);
                    typeof(HangingCloth).GetField("_retraction", Private).SetValue(first, 1f);
                    var replacement = GameObject.Find("Stage1_DocumentGimmicks/Coop_4_ReplacementCloth").GetComponent<HangingCloth>();
                    var mechanism = Read<StageOneDocumentMechanism>(replacement, "_deploymentMechanism");
                    if (!mechanism.TryHitBranch()) throw new Exception("Replacement branch cannot deploy.");
                    _phase = 1; _frame = Time.frameCount; _start = EditorApplication.timeSinceStartup;
                    return;
                }
                if (EditorApplication.timeSinceStartup - _start < 2) { Pose(); return; }
                Results.Add("BRANCH DEPLOYMENT PASS\n" + Pose());
                EditorApplication.update -= Step;
                System.IO.File.WriteAllText("Temp/ClothSkinBinding_20261109/result.txt", string.Join("\n", Results));
            }
            catch (Exception e)
            {
                EditorApplication.update -= Step;
                System.IO.File.WriteAllText("Temp/ClothSkinBinding_20261109/result.txt", "FAIL: " + e);
            }
        }
    }
}
