using System.Collections.Generic;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Herbalist.EditorTools
{
    // A duplicated prefab needs a new scene ID so leaf installation remains unambiguous in Fusion.
    [InitializeOnLoad]
    internal static class StageOneJumpPadPlacementWatcher
    {
        private static readonly HashSet<StageOneJumpPadAuthoring> KnownInstances = new();
        private static bool _queued;
        private static bool _scanning;

        static StageOneJumpPadPlacementWatcher()
        {
            EditorApplication.hierarchyChanged += QueueScan;
            QueueScan();
        }

        private static void QueueScan()
        {
            if (_queued || _scanning || EditorApplication.isPlayingOrWillChangePlaymode) return;
            _queued = true;
            EditorApplication.delayCall += Scan;
        }

        [MenuItem("Tools/Herbalist/Stage 1/Assign Jump Pad IDs")]
        private static void AssignJumpPadIds() => Scan();

        private static void Scan()
        {
            _queued = false;
            if (_scanning || EditorApplication.isPlayingOrWillChangePlaymode ||
                PrefabStageUtility.GetCurrentPrefabStage() != null) return;

            _scanning = true;
            try
            {
                var pads = new List<StageOneJumpPadAuthoring>(
                    Object.FindObjectsByType<StageOneJumpPadAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None));
                var current = new HashSet<StageOneJumpPadAuthoring>(pads);
                KnownInstances.IntersectWith(current);
                pads.Sort((a, b) =>
                {
                    int knownOrder = KnownInstances.Contains(b).CompareTo(KnownInstances.Contains(a));
                    return knownOrder != 0 ? knownOrder : string.CompareOrdinal(a.name, b.name);
                });

                var used = new HashSet<int>();
                int nextId = 10000;
                foreach (var target in Object.FindObjectsByType<LeafInstallTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (!target.gameObject.scene.IsValid() || PrefabStageUtility.GetPrefabStage(target.gameObject) != null) continue;
                    nextId = Mathf.Max(nextId, target.Id + 1);
                    if (target.GetComponentInParent<StageOneJumpPadAuthoring>(true) == null && target.Id > 0)
                        used.Add(target.Id);
                }

                foreach (var pad in pads)
                {
                    if (!pad.gameObject.scene.IsValid() || PrefabStageUtility.GetPrefabStage(pad.gameObject) != null) continue;
                    var target = pad.GetComponentInChildren<LeafInstallTarget>(true);
                    if (target == null) continue;
                    if (target.Id <= 0 || !used.Add(target.Id))
                    {
                        while (used.Contains(nextId)) nextId++;
                        var serialized = new SerializedObject(target);
                        serialized.FindProperty("targetId").intValue = nextId;
                        serialized.ApplyModifiedProperties();
                        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                        EditorSceneManager.MarkSceneDirty(pad.gameObject.scene);
                        used.Add(nextId++);
                    }
                    KnownInstances.Add(pad);
                }
            }
            finally { _scanning = false; }
        }
    }
}
