using Herbalist.Abilities;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class SapHoseTubeSetup
    {
        private static readonly string[] PrefabPaths =
        {
            "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_SapDeposit.prefab",
            "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_NetworkSapDeposit.prefab"
        };

        [MenuItem("Herbalist/Abilities/Apply Sap Hose Tube")]
        public static void Apply()
        {
            foreach (string path in PrefabPaths)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var presentation = root.GetComponentInChildren<SapStreamPresentation>(true);
                    if (presentation == null) throw new System.Exception("Missing SapStreamPresentation: " + path);
                    var serialized = new SerializedObject(presentation);
                    var line = serialized.FindProperty("stream").objectReferenceValue as LineRenderer;
                    if (line == null) throw new System.Exception("Missing stream: " + path);
                    Transform tube = line.transform.Find("TubeMesh");
                    if (tube == null)
                    {
                        tube = new GameObject("TubeMesh").transform;
                        tube.SetParent(line.transform, false);
                    }
                    var filter = tube.GetComponent<MeshFilter>();
                    if (filter == null) filter = tube.gameObject.AddComponent<MeshFilter>();
                    var renderer = tube.GetComponent<MeshRenderer>();
                    if (renderer == null) renderer = tube.gameObject.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = line.sharedMaterial;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.enabled = false;
                    serialized.FindProperty("_tubeFilter").objectReferenceValue = filter;
                    serialized.FindProperty("_tubeRenderer").objectReferenceValue = renderer;
                    serialized.FindProperty("_coreWidth").floatValue = .68f;
                    serialized.FindProperty("_radialSegments").intValue = 16;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Sap hose tubes applied to solo and network prefabs.");
        }
    }
}
