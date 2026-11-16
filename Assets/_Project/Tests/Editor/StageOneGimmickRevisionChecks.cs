using System;
using System.Linq;
using System.Reflection;
using Herbalist.Abilities;
using Herbalist.Levels;
using Herbalist.Player;
using Herbalist.StageOne;
using UnityEngine;

namespace Herbalist.Editor
{
    // Run in direct offline Stage One Play mode. Uses actual scene colliders and player capsules.
    public static class StageOneGimmickRevisionChecks
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Require(bool value, string reason) { if (!value) throw new Exception(reason); }
        public static string RunPlay()
        {
            Require(Application.isPlaying && StageLevel.Instance != null &&
                StageLevel.Instance.abilityRules.name == "SO_StageOneGimmickAbilityRules", "Run in revised Stage One Play mode.");
            Require(Herbalist.Networking.FusionLobbySession.Instance == null ||
                !Herbalist.Networking.FusionLobbySession.Instance.HasNetworkSession, "Use offline Play, outside a live session.");
            var actors = StageActor.All.Where(a => a.Available).OrderBy(a => a.Slot).ToArray();
            Require(actors.Length == 2, "Two offline actors required.");
            var positions = actors.Select(a => a.transform.position).ToArray();
            var settings = StageLevel.Instance.abilityRules;
            var zones = UnityEngine.Object.FindObjectsByType<StageOneLeafZone>(FindObjectsSortMode.None);
            var target = zones[0].GetComponent<LeafInstallTarget>();
            var leaf = UnityEngine.Object.Instantiate(settings.leaf.offlinePrefab);
            SapDeposit held = null;
            try
            {
                leaf.Initialize(settings.leaf, actors[0].transform, target.SocketPosition, target.SocketPosition, null, true);
                Require(leaf.TryInstallAtSocket(target), "Install at authored socket.");
                var surface = (Collider)typeof(LeafProjectile).GetField("_surface", Private).GetValue(leaf);
                Physics.SyncTransforms();
                for (int i = 0; i < actors.Length; i++)
                {
                    var cc = actors[i].GetComponent<CharacterController>();
                    var feet = surface.bounds.center;
                    feet.y = surface.bounds.max.y + .02f; feet.x += i == 0 ? -.45f : .45f;
                    Vector3 offset = cc.transform.TransformVector(cc.center) - Vector3.up * cc.height * Mathf.Abs(cc.transform.lossyScale.y) * .5f;
                    actors[i].GetComponent<PlayerController>().Motor.Teleport(feet - offset);
                }
                Physics.SyncTransforms();
                Require(actors.All(a => leaf.Supports(a.GetComponent<CharacterController>(), .35f)), "Both players share one platform.");
                leaf.Tick(4.99f); Require(leaf.Installed, "Standing does not recall leaf and 4.99 seconds is valid.");
                leaf.Tick(.02f); Require(leaf.State == LeafState.Returning && !target.HasLeaf, "5.01 seconds recalls and frees socket.");
                RaycastHit hit;
                Require(StageOneLeafZone.Cast(new Ray(target.SocketPosition + Vector3.back * 3, Vector3.forward), 0, 6,
                    settings.leaf.hitMask, out hit) && hit.collider.GetComponent<StageOneLeafZone>() != null, "Trigger zone is aimable.");
                Require(zones.All(z => z.GetComponents<Collider>().All(c => c.isTrigger)), "Zones have no solid bypass floor.");

                var cloths = UnityEngine.Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None);
                var angle = typeof(HangingCloth).GetField("_angles", Private);
                var velocity = typeof(HangingCloth).GetField("_velocity", Private);
                foreach (var cloth in cloths)
                {
                    Vector2 savedAngle = (Vector2)angle.GetValue(cloth), savedVelocity = (Vector2)velocity.GetValue(cloth);
                    try
                    {
                        angle.SetValue(cloth, Vector2.zero); velocity.SetValue(cloth, Vector2.zero);
                        Require(cloth.PushWithWater(Vector3.right, .1f), "Water drive enabled."); cloth.Tick(.02f);
                        Require(((Vector2)angle.GetValue(cloth)).y > 0, "Right shot pushes right.");
                        angle.SetValue(cloth, Vector2.zero); velocity.SetValue(cloth, Vector2.zero);
                        cloth.PushWithWater(Vector3.left, .1f); cloth.Tick(.02f);
                        Require(((Vector2)angle.GetValue(cloth)).y < 0, "Left shot pushes left.");
                    }
                    finally { angle.SetValue(cloth, savedAngle); velocity.SetValue(cloth, savedVelocity); cloth.Tick(.0001f); }
                }
                var first = cloths[0];
                var visual = (Transform)typeof(HangingCloth).GetField("_authoredVisual", Private).GetValue(first);
                held = UnityEngine.Object.Instantiate(settings.sap.offlinePrefab);
                angle.SetValue(first, Vector2.zero); velocity.SetValue(first, Vector2.zero); first.Tick(.0001f); Physics.SyncTransforms();
                var center = visual.GetComponent<Renderer>().bounds.center;
                held.transform.position = center + Vector3.right * 8;
                var hose = new SapHoseSprayer(); var aim = new Ray(held.transform.position, Vector3.left);
                for (int i = 0; i < 80; i++) { hose.Tick(held, aim, true, .02f, settings.sap, () => null); first.Tick(.02f); Physics.SyncTransforms(); }
                Require(((Vector2)angle.GetValue(first)).y < -.001f, "Actual ballistic hose collision drives cloth.");
                var traversal = UnityEngine.Object.FindFirstObjectByType<StageOneTraversal>();
                Require(traversal != null && traversal.Authority, "Offline traversal authority exists.");
                return "PASS: shared leaf supports both players; timed recall; trigger-only zones; three cloths left/right; actual hose collision.";
            }
            finally
            {
                leaf.BeginReturn(); UnityEngine.Object.Destroy(leaf.gameObject);
                if (held != null) UnityEngine.Object.Destroy(held.gameObject);
                for (int i = 0; i < actors.Length; i++) actors[i].GetComponent<PlayerController>().Motor.Teleport(positions[i]);
            }
        }
    }
}
