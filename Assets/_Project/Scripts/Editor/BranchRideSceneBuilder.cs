using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Herbalist.Editor
{
    // BranchRide is the authored Stage 3. The removed altar is no longer a template.
    public static class BranchRideSceneBuilder
    {
        public const string Path = CurrentStagePaths.BranchRide;
        [MenuItem("Herbalist/Stages/Open Branch Ride Stage")]
        public static void Create()
        {
            if (Application.isPlaying) throw new System.InvalidOperationException("Stop Play first.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Path) == null)
                throw new System.InvalidOperationException("BranchRide scene is missing: " + Path);
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(Path);
        }
    }
}