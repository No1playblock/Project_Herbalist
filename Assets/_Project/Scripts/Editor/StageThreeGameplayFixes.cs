using System;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Herbalist.Editor
{
    [InitializeOnLoad]
    public static class StageThreeGameplayFixes
    {
        private const string Pending = "Herbalist.StageThreeFixChecks";
        static StageThreeGameplayFixes()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
                    EditorApplication.update += RunPending;
            };
        }
        public static void StartChecks()
        {
            if (EditorApplication.isPlaying) throw new Exception("Stop current play first");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new Exception("Save current scene first");
            EditorSceneManager.OpenScene(CurrentStagePaths.BranchRide);
            SessionState.SetBool(Pending, true); EditorApplication.isPlaying = true;
        }
        private static void RunPending()
        {
            if (!EditorApplication.isPlaying || Time.frameCount < 5) return;
            EditorApplication.update -= RunPending;
            SessionState.SetBool(Pending, false);
            string report;
            try { report = BranchRideChecks.Runtime(); }
            catch (Exception error) { report = "FAIL: " + error; }
            System.IO.File.WriteAllText("Temp/Stage3GameplayFixChecks.txt", report);
            Debug.Log(report); EditorApplication.isPlaying = false;
        }
        public static string Apply()
        {
            if (EditorApplication.isPlaying) throw new Exception("Stop play before authoring");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != CurrentStagePaths.BranchRide) throw new Exception("Open main Stage Three");
            var sceneRoots = scene.GetRootGameObjects();
            var level = sceneRoots.SelectMany(r => r.GetComponentsInChildren<StageLevel>(true)).First();
            const string legacyRules = "Assets/_Project/Data/Stages/Stage_03/SO_BranchRideAbilityRules.asset";
            if (AssetDatabase.GetAssetPath(level.abilityRules) != legacyRules)
                throw new InvalidOperationException("This scene uses its own ability tuning. Edit its StageLevel ability rules instead of applying the legacy defaults.");
            var reference = EditorSceneManager.OpenPreviewScene(StageOneEnvironmentModelPromotion.Main);
            try
            {
                var roots = reference.GetRootGameObjects();
                var referencePlayer = roots.SelectMany(r => r.GetComponentsInChildren<CharacterController>(true)).First();
                float referenceHeight = referencePlayer.height * Mathf.Abs(referencePlayer.transform.lossyScale.y);
                var player = sceneRoots.SelectMany(r => r.GetComponentsInChildren<CharacterController>(true)).First();
                float height = player.height * Mathf.Abs(player.transform.lossyScale.y);
                float ratio = height / referenceHeight;
                // Restore the four targets accidentally resized when 'ring' was misread.
                foreach (var port in sceneRoots.SelectMany(r => r.GetComponentsInChildren<SapInjectionPort>(true)))
                {
                    if (!port.GetComponentsInChildren<MeshRenderer>().Any(r => r.name == "OuterTarget")) continue;
                    port.transform.localScale = Vector3.one;
                    EditorUtility.SetDirty(port.transform);
                }
                var referenceRules = roots.SelectMany(r => r.GetComponentsInChildren<StageLevel>(true)).First().abilityRules;
                const string sapPath = "Assets/_Project/Data/Stages/Stage_03/SO_BranchRideSap.asset";
                var sap = AssetDatabase.LoadAssetAtPath<SapAbilitySettings>(sapPath);
                if (sap == null) { sap = UnityEngine.Object.Instantiate(level.abilityRules.sap); AssetDatabase.CreateAsset(sap, sapPath); }
                sap.orbitCenterOffset = referenceRules.sap.orbitCenterOffset * ratio;
                sap.orbitRadius = referenceRules.sap.orbitRadius * ratio;
                sap.orbitTubeRadius = referenceRules.sap.orbitTubeRadius * ratio;
                sap.orbitRipple = referenceRules.sap.orbitRipple * ratio;
                level.abilityRules.sap = sap;
                EditorUtility.SetDirty(sap);
                var original = level.abilityRules.leaf;
                const string path = "Assets/_Project/Data/Stages/Stage_03/SO_BranchRideLeaf.asset";
                var leaf = AssetDatabase.LoadAssetAtPath<LeafAbilitySettings>(path);
                if (leaf == null) { leaf = UnityEngine.Object.Instantiate(original); AssetDatabase.CreateAsset(leaf, path); }
                var referenceLeaf = referenceRules.leaf;
                leaf.offlinePrefab = referenceLeaf.offlinePrefab;
                leaf.installedScaleMultiplier = referenceLeaf.installedScaleMultiplier;
                leaf.flyingVisualScaleMultiplier = referenceLeaf.flyingVisualScaleMultiplier;
                level.abilityRules.leaf = leaf;
                EditorUtility.SetDirty(leaf); EditorUtility.SetDirty(level.abilityRules);
                AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
                return $"Player height ratio {ratio:F4}; body water orbit radius {sap.orbitRadius}, tube radius {sap.orbitTubeRadius}, center {sap.orbitCenterOffset}. Four targets restored to original scale. Stage Three installed leaf scale {leaf.installedScaleMultiplier}.";
            }
            finally { EditorSceneManager.ClosePreviewScene(reference); }
        }
    }
}
