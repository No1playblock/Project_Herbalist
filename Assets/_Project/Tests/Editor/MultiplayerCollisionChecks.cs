using System;
using System.Collections.Generic;
using Herbalist.Interaction;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class MultiplayerCollisionChecks
    {
        // Exercises real CharacterController.Move contacts, including Fusion's enable/disable restore.
        public static string Run()
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Play mode required");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PF_Stage01ModelingNetworkPlayer.prefab");
            var probe = new GameObject("MultiplayerCollisionProbe");
            probe.layer = prefab.layer;
            probe.transform.localScale = prefab.transform.localScale;
            var controller = probe.AddComponent<CharacterController>();
            EditorUtility.CopySerialized(prefab.GetComponent<CharacterController>(), controller);
            int checkedSurfaces = 0, landed = 0, walls = 0;
            var failures = new List<string>();
            var groups = new Dictionary<string, int>();
            try
            {
                var colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
                foreach (var surface in colliders)
                {
                    if (surface == controller || !surface.enabled || surface.isTrigger ||
                        (surface.gameObject.layer != 18 && surface.gameObject.layer != 22 && !(surface is TerrainCollider))) continue;
                    string category = surface.gameObject.layer == 22 ? "Tree" : surface is TerrainCollider ? "Terrain" : "Traversal";
                    if (Physics.GetIgnoreLayerCollision(probe.layer, surface.gameObject.layer))
                    { failures.Add(surface.name + ": ignored layer"); continue; }
                    var b = surface.bounds;
                    RaycastHit top = default;
                    bool found = false;
                    foreach (float x in new[] { .5f, .25f, .75f, .1f, .9f })
                    {
                        foreach (float z in new[] { .5f, .25f, .75f, .1f, .9f })
                        {
                            var origin = new Vector3(Mathf.Lerp(b.min.x, b.max.x, x), b.max.y + 2, Mathf.Lerp(b.min.z, b.max.z, z));
                            if (surface.Raycast(new Ray(origin, Vector3.down), out top, b.size.y + 4) &&
                                Vector3.Angle(top.normal, Vector3.up) <= controller.slopeLimit - 1) { found = true; break; }
                        }
                        if (found) break;
                    }
                    if (!found) { walls++; continue; }
                    checkedSurfaces++;
                    controller.enabled = false;
                    probe.transform.position = top.point + Vector3.up * 2;
                    controller.enabled = true;
                    Physics.SyncTransforms();
                    bool contact = false;
                    for (int tick = 0; tick < 120; tick++)
                    {
                        var pos = probe.transform.position;
                        controller.enabled = false; probe.transform.position = pos; controller.enabled = true;
                        OneWayPlatform.PrepareMove(controller, Vector3.down * 8);
                        if ((controller.Move(Vector3.down * (8f / 60f)) & CollisionFlags.Below) != 0) { contact = true; break; }
                        if (probe.transform.position.y < top.point.y - 3) break;
                    }
                    if (contact)
                    {
                        landed++;
                        if (!groups.ContainsKey(category)) groups[category] = 0;
                        groups[category]++;
                    }
                    else failures.Add(surface.name + ": no landing at " + top.point);
                }
                string result = "Surfaces=" + checkedSurfaces + ", landed=" + landed + ", non-walkable=" + walls + "; ";
                foreach (var pair in groups) result += pair.Key + "=" + pair.Value + "; ";
                if (failures.Count > 0) throw new Exception(result + string.Join("\n", failures));
                return "PASS " + result;
            }
            finally { OneWayPlatform.Release(controller); Object.DestroyImmediate(probe); }
        }
    }
}
