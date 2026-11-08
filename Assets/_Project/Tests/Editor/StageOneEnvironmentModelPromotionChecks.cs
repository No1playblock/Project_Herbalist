using System;
using System.Collections.Generic;
using System.Linq;
using Herbalist.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Herbalist.Editor
{
    public static class StageOneEnvironmentModelPromotionChecks
    {
        private static IEnumerable<Ray> Rays(Bounds b, Vector3 direction)
        {
            var u = direction.y != 0 ? Vector3.right : Vector3.up;
            var v = Vector3.Cross(direction, u);
            float distance = b.extents.magnitude + 2f;
            foreach (float a in new[] { -.7f, -.35f, 0f, .35f, .7f })
                foreach (float c in new[] { -.7f, -.35f, 0f, .35f, .7f })
                    yield return new Ray(b.center - direction * distance +
                        Vector3.Scale(b.extents, u) * a + Vector3.Scale(b.extents, v) * c, direction);
        }

        public static string RunPlay()
        {
            if (!Application.isPlaying || Time.frameCount < 2) throw new Exception("Run in advancing Play mode.");
            var root = SceneManager.GetActiveScene().GetRootGameObjects()
                .Single(g => g.name == StageOneEnvironmentModelPromotion.RootName);
            var filters = root.GetComponentsInChildren<MeshFilter>();
            if (filters.Length != 26 || root.GetComponentsInChildren<MeshCollider>().Length != 26)
                throw new Exception("Expected 26 visual meshes and matching solid colliders.");
            Physics.SyncTransforms();
            var failures = new List<string>(); int rayModels = 0, stairTops = 0, landings = 0, underBranch = 0;
            var player = Object.FindObjectsByType<PlayerController>().First(p => p.LocallyControlled);
            var playerCapsule = player.GetComponent<CharacterController>();
            var probe = new GameObject("EnvironmentStairProbe") { hideFlags = HideFlags.HideAndDontSave };
            probe.SetActive(false); probe.layer = player.gameObject.layer; probe.transform.localScale = player.transform.lossyScale;
            var capsule = probe.AddComponent<CharacterController>();
            capsule.height = playerCapsule.height; capsule.radius = playerCapsule.radius;
            capsule.center = playerCapsule.center; capsule.skinWidth = playerCapsule.skinWidth;
            capsule.stepOffset = playerCapsule.stepOffset; capsule.slopeLimit = playerCapsule.slopeLimit;
            var tree = Object.FindObjectsByType<MeshCollider>().Single(c => c.name == "seonag_tree");
            var clearanceObject = new GameObject("StairClearanceProbe") { hideFlags = HideFlags.HideAndDontSave };
            clearanceObject.transform.localScale = probe.transform.localScale;
            var clearance = clearanceObject.AddComponent<CapsuleCollider>(); clearance.enabled = false;
            clearance.height = capsule.height; clearance.radius = capsule.radius; clearance.center = capsule.center;
            try
            {
                foreach (var filter in filters)
                {
                    var collider = filter.GetComponent<MeshCollider>(); var renderer = filter.GetComponent<Renderer>();
                    if (filter.sharedMesh == null || collider.sharedMesh != filter.sharedMesh || !collider.enabled ||
                        collider.isTrigger || collider.convex || renderer.sharedMaterials.Any(m => m == null))
                    { failures.Add(filter.name + ": mesh/material/collider mismatch"); continue; }
                    var b = collider.bounds;
                    var directions = new[] { Vector3.down, Vector3.up, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
                    bool hit = directions.SelectMany(d => Rays(b, d)).Any(ray => collider.Raycast(ray, out _, b.extents.magnitude * 2 + 4));
                    if (hit) rayModels++; else failures.Add(filter.name + ": no physical surface");
                    if (!filter.name.StartsWith("wood_stair")) continue;
                    var tops = new List<RaycastHit>();
                    foreach (var ray in Rays(b, Vector3.down))
                        if (collider.Raycast(ray, out var top, b.extents.magnitude * 2 + 4) && top.normal.y > .65f) tops.Add(top);
                    if (tops.Count > 0) stairTops++; else { failures.Add(filter.name + ": no walkable top"); continue; }
                    bool landed = false;
                    float height = capsule.height * probe.transform.lossyScale.y;
                    float footOffset = (capsule.center.y - capsule.height * .5f) * probe.transform.lossyScale.y;
                    // Some authored steps sit beneath a tree branch. Test both a drop from above
                    // and occupying the step from its side; the latter must also have real headroom.
                    for (int approach = 0; approach < 2 && !landed; approach++)
                    foreach (var top in tops)
                    {
                        probe.SetActive(false);
                        probe.transform.position = top.point + Vector3.up *
                            ((approach == 0 ? height + 2f : .4f) - footOffset);
                        probe.SetActive(true); Physics.SyncTransforms();
                        var flags = capsule.Move(Vector3.down * (approach == 0 ? height + 3f : .8f));
                        var feet = probe.transform.position + Vector3.up * footOffset;
                        if ((flags & CollisionFlags.Below) != 0 && Physics.Raycast(feet + Vector3.up * .5f,
                            Vector3.down, out var ground, 1.2f, ~0, QueryTriggerInteraction.Ignore) && ground.collider == collider)
                        {
                            bool overlap = Physics.ComputePenetration(clearance, probe.transform.position, probe.transform.rotation,
                                tree, tree.transform.position, tree.transform.rotation, out _, out var distance);
                            if (overlap && distance > capsule.skinWidth * probe.transform.lossyScale.x + .02f) continue;
                            landed = true; if (approach == 1) underBranch++; break;
                        }
                    }
                    if (landed) landings++; else failures.Add(filter.name + ": no clear player-sized standing area");
                }
                string report = "Frame " + Time.frameCount + ": physical models " + rayModels + "/26, stair tops " +
                    stairTops + "/19, player-sized stair support " + landings + "/19 (" + underBranch +
                    " beneath branches). " + string.Join("; ", failures);
                System.IO.File.WriteAllText("Temp/Stage1EnvironmentPromotionPlay.txt", report);
                if (failures.Count > 0) throw new Exception(report);
                return "PASS: " + report;
            }
            finally { probe.SetActive(false); Object.Destroy(probe); Object.Destroy(clearanceObject); }
        }
    }
}
