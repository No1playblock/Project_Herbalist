using Herbalist.Abilities;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Herbalist.Editor
{
    public static class SapOrbitSetup
    {
        [MenuItem("Herbalist/Abilities/Apply Water Orbit")]
        public static void Apply()
        {
            var material = WaterMaterial();
            // Author bases first so variants inherit a single orbit rather than adding duplicates.
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/Characters" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => PrefabUtility.GetPrefabAssetType(AssetDatabase.LoadAssetAtPath<GameObject>(path))
                    == PrefabAssetType.Variant ? 1 : 0);
            foreach (string path in paths)
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var abilities = root.GetComponentsInChildren<SapControlAbility>(true);
                    if (abilities.Length == 0) continue;
                    foreach (var ability in abilities)
                        Attach(ability.gameObject, material);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }

        public static void Attach(GameObject root, Material material)
        {
            var presentation = root.GetComponent<SapOrbitPresentation>();
            if (presentation != null)
            {
                var existing = root.transform.Find("WaterOrbit");
                if (existing != null) existing.GetComponent<MeshRenderer>().sharedMaterial = material;
                return;
            }
            Transform orbit = root.transform.Find("WaterOrbit");
            if (orbit == null)
            {
                orbit = new GameObject("WaterOrbit").transform;
                orbit.SetParent(root.transform, false);
            }
            var filter = orbit.GetComponent<MeshFilter>();
            if (filter == null) filter = orbit.gameObject.AddComponent<MeshFilter>();
            var renderer = orbit.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = orbit.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false; renderer.enabled = false;
            presentation = root.AddComponent<SapOrbitPresentation>();
            var serialized = new SerializedObject(presentation);
            serialized.FindProperty("_filter").objectReferenceValue = filter;
            serialized.FindProperty("_renderer").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Material WaterMaterial()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/InteractiveObjects/Abilities/PF_SapDeposit.prefab");
            var presentation = new SerializedObject(prefab.GetComponent<SapStreamPresentation>());
            var renderer = presentation.FindProperty("_tubeRenderer").objectReferenceValue as MeshRenderer;
            if (renderer == null || renderer.sharedMaterial == null)
                throw new System.InvalidOperationException("Missing authored water cannon material.");
            var material = renderer.sharedMaterial;
            return material;
        }
    }
}
