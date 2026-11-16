using System;
using System.Linq;
using Herbalist.Abilities;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class StageOneClothPhotoCorrection
    {
        private static T Read<T>(Object o, string name) where T : Object =>
            (T)new SerializedObject(o).FindProperty(name).objectReferenceValue;
        private static void Dirty(Object o)
        {
            EditorUtility.SetDirty(o);
            if (o is Component c && PrefabUtility.IsPartOfPrefabInstance(c))
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        }
        private static Vector3 End(MeshFilter f, bool top)
        {
            var b = f.sharedMesh.bounds;
            return f.transform.TransformPoint(new Vector3(b.center.x, top ? b.max.y : b.min.y, b.center.z));
        }

        public static string Capture(string name, Vector3 position, Vector3 target, float fieldOfView = 40f)
        {
            var go = new GameObject("ClothPhotoReviewCamera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = go.AddComponent<Camera>();
            camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
            camera.fieldOfView = fieldOfView; camera.farClipPlane = 1000;
            var texture = new RenderTexture(900, 700, 24);
            var pixels = new Texture2D(900, 700, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = texture; camera.Render(); RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0, 0, 900, 700), 0, 0); pixels.Apply();
                string path = "Temp/ClothPhotoFix_20261108/" + name + ".png";
                System.IO.File.WriteAllBytes(path, pixels.EncodeToPNG()); return path;
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous;
                Object.DestroyImmediate(pixels); Object.DestroyImmediate(texture); Object.DestroyImmediate(go);
            }
        }

        public static string Apply()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (Application.isPlaying || scene.path != StageOneDocumentAuthoring.ScenePath)
                throw new InvalidOperationException("Imported Stage One Edit mode required.");
            if (GameObject.Find("Stage1_DocumentGimmicks/ClothSimulations") != null)
                throw new InvalidOperationException("Two-end Cloth skinning is installed; edit its authored bones and constraints instead of reapplying the old rigid-cloth correction.");
            System.IO.Directory.CreateDirectory("Temp/ClothPhotoFix_20261108");
            EditorSceneManager.SaveScene(scene, "Temp/ClothPhotoFix_20261108/before.unity", true);
            int count = 0;
            foreach (var cloth in Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None))
            {
                var art = Read<Transform>(cloth, "_externalVisualRoot");
                if (art == null || !cloth.name.StartsWith("Coop_")) continue;
                var fabric = art.Find("fabric").GetComponent<MeshFilter>();
                var knot = art.Find("knot_low").GetComponent<MeshFilter>();
                bool carried = Read<StageOneDocumentMechanism>(cloth, "_deploymentMechanism") != null;
                Vector3 top = End(fabric, true);
                if (!carried)
                {
                    var rotation = Quaternion.FromToRotation(fabric.transform.TransformVector(Vector3.up), Vector3.up);
                    art.SetPositionAndRotation(top + rotation * (art.position - top), rotation * art.rotation);
                    Dirty(art);
                }
                // The mesh and the interaction point use the same physical knot.
                knot.transform.rotation = fabric.transform.rotation;
                Vector3 axis = fabric.transform.TransformVector(Vector3.up).normalized;
                knot.transform.position += End(fabric, false) + axis * .08f - End(knot, true);
                Dirty(knot.transform);
                top = End(fabric, true);
                var grip = Read<Transform>(cloth, "_knot");
                cloth.transform.position = top;
                grip.position = knot.transform.TransformPoint(knot.sharedMesh.bounds.center);
                Dirty(cloth.transform); Dirty(grip);
                var proxy = cloth.transform.Find("WaterHitProxy");
                if (proxy != null)
                {
                    var bounds = fabric.GetComponent<Renderer>().bounds;
                    proxy.SetPositionAndRotation(bounds.center, Quaternion.identity);
                    var box = proxy.GetComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Max(1, bounds.size.x), bounds.size.y, Mathf.Max(1, bounds.size.z));
                    Dirty(proxy); Dirty(box);
                }
                if (carried)
                {
                    var mechanism = Read<StageOneDocumentMechanism>(cloth, "_deploymentMechanism");
                    var ms = new SerializedObject(mechanism);
                    var pivot = Read<Transform>(mechanism, "_motionPivot");
                    Vector3 p = pivot != null ? pivot.position : mechanism.transform.position;
                    Vector3 finalTop = p + Quaternion.Euler(ms.FindProperty("_rotation").vector3Value) * (top - p)
                        + ms.FindProperty("_offset").vector3Value;
                    var destination = Read<Transform>(cloth, "_deployedGrip");
                    // Gravity fixes X/Z below the actual attachment, retaining the authored grip height.
                    destination.position = new Vector3(finalTop.x, destination.position.y, finalTop.z);
                    Dirty(destination);
                }
                count++;
            }

            var root = GameObject.Find("Stage1_DocumentGimmicks/OutletCorrections_20261104").transform;
            var pad = root.GetComponentsInChildren<ExteriorJumpPad>(true).Single();
            var left = GameObject.Find("DesignStage1_Traversal/origin_tree (4)/InletOpening").transform;
            var right = GameObject.Find("DesignStage1_Traversal/origin_tree (3)/InletOpening").transform;
            var ps = new SerializedObject(pad);
            var target = (LeafInstallTarget)ps.FindProperty("_sockets").GetArrayElementAtIndex(0).objectReferenceValue;
            var inlet = (SapInjectionPort)ps.FindProperty("_ports").GetArrayElementAtIndex(0).objectReferenceValue;
            var jet = (GameObject)ps.FindProperty("_jets").GetArrayElementAtIndex(0).objectReferenceValue;
            pad.transform.position = left.position;
            target.transform.SetPositionAndRotation(left.position + Vector3.up * .12f, Quaternion.identity);
            target.Socket.SetPositionAndRotation(left.position + Vector3.up * .12f, Quaternion.identity);
            inlet.transform.SetPositionAndRotation(right.position, Quaternion.identity);
            var aim = Read<Transform>(inlet, "_aimPoint");
            aim.position = right.position + Vector3.up * .35f;
            jet.transform.position = target.SocketPosition;
            var marker = target.transform.Find("LeafInstallMarker").GetComponent<LineRenderer>();
            marker.enabled = false; // The original blue opening already marks this socket.
            foreach (var o in new Object[] { pad.transform, target.transform, target.Socket, inlet.transform, aim, jet.transform, marker }) Dirty(o);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return $"Aligned {count} imported cloth meshes and real grips; deployed red cloth hangs below attachment; moved existing paired pad to branch_hole (1), photo-left leaf / photo-right water. Existing network object and target ID preserved.";
        }
    }
}
