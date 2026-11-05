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
    public static class StageOneOutletCorrectionChecks
    {
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static T Read<T>(object obj, string field) => (T)obj.GetType().GetField(field, Private).GetValue(obj);
        private static void Check(bool value, string reason) { if (!value) throw new Exception(reason); }

        public static string Play()
        {
            Check(Application.isPlaying && StageOneFlow.Instance != null && StageOneFlow.Instance.Authority, "Offline Stage One Play required.");
            var root = GameObject.Find("Stage1_DocumentGimmicks").transform;
            var correction = root.Find("OutletCorrections_20261104");
            var actor = StageActor.All.First(a => a.Available); var player = actor.GetComponent<PlayerController>();
            Vector3 saved = actor.transform.position; LeafProjectile leaf = null; SapDeposit held = null;
            var settings = StageLevel.Instance.abilityRules;
            try
            {
                var blockers = correction.GetComponentsInChildren<StageOneDocumentMechanism>().Where(m => m.Kind == StageOneDocumentMechanismKind.BlockJet).ToArray();
                Check(blockers.Length == 3 && blockers.All(m => m.name.StartsWith("LeafBlock_04_Musroom")) &&
                    !root.Find("Leaf_1_BlockingEruption").gameObject.activeSelf, "Only three 04_Musroom outlets replace the wrong branch jet.");
                var traversal = root.GetComponent<StageOneTraversal>();
                foreach (var mechanism in blockers)
                {
                    var target = Read<LeafInstallTarget>(mechanism, "_leafTarget"); var stream = Read<GameObject>(mechanism, "_jet");
                    Check(stream.activeSelf && !target.KeepsLeafUntilUsed && target.GetComponent<StageOneLeafUse>() == null, "Outlet open; plug is not a first-footstep platform.");
                    Vector3 outward = stream.transform.forward;
                    Check(Mathf.Abs(Vector3.Dot(outward, Vector3.up)) < .0001f, "Water is perpendicular to the vertical trunk: " + mechanism.name);
                    Physics.SyncTransforms();
                    Check(StageOneLeafZone.Cast(new Ray(target.SocketPosition + outward * 10, -outward), settings.leaf.collisionRadius,
                        12, settings.leaf.hitMask, out var hit) && hit.collider.GetComponentInParent<LeafInstallTarget>() == target, "Leaf reaches outlet through active water: " + mechanism.name);
                    var outlet = stream.GetComponent<StageOneFixedWaterStream>();
                    Check(outlet.GetPathPoint(.25f).y > outlet.GetPathPoint(0).y &&
                        outlet.GetPathPoint(1).y < outlet.GetPathPoint(0).y, "Water rises slightly and curves down.");
                    var point = outlet.GetPathPoint(.9f);
                    var cc = actor.GetComponent<CharacterController>(); player.Motor.Teleport(point - actor.transform.TransformVector(cc.center)); Physics.SyncTransforms();
                    player.Motor.ResetMotion();
                    mechanism.Tick(.02f);
                    var launched = player.Motor.CaptureState();
                    Check(!traversal.TouchesBareTree(cc) && launched.ExternalFlight && launched.ExternalAirControl &&
                        Vector3.Dot(launched.Velocity, outward) > 0, "Open stream pushes the player out without triggering tree recovery.");
                    player.Motor.Launch(-outward * 4 + Vector3.down, player.Tuning.gravity, true);
                    mechanism.Tick(.02f);
                    Check(Vector3.Dot(player.Motor.Velocity, outward) > 0 && Mathf.Approximately(player.Motor.Velocity.y, -1),
                        "Continued contact pushes outward while preserving falling velocity.");
                    player.Motor.ResetMotion();
                    leaf = UnityEngine.Object.Instantiate(settings.leaf.offlinePrefab);
                    leaf.Initialize(settings.leaf, actor.transform, target.SocketPosition, target.SocketPosition, null, true);
                    Check(leaf.TryInstallAtSocket(target), "Install leaf plug: " + mechanism.name); mechanism.Tick(.02f); Physics.SyncTransforms();
                    Check(!stream.activeSelf && !stream.GetComponent<Collider>().enabled, "Leaf removes both water and obstruction.");
                    Check(!player.Motor.CaptureState().ExternalFlight, "A blocked outlet cannot push.");
                    Check(blockers.Where(m => m != mechanism).All(m => Read<GameObject>(m, "_jet").activeSelf), "Other outlets remain independently open.");
                    leaf.Tick(5.01f); mechanism.Tick(.02f); Check(stream.activeSelf && stream.GetComponent<Collider>().enabled, "Timed recall reopens the corresponding stream.");
                    Check(player.Motor.CaptureState().ExternalFlight, "Reopened stream pushes on renewed contact.");
                    UnityEngine.Object.DestroyImmediate(leaf.gameObject); leaf = null;
                }
                player.Motor.Teleport(saved); player.Motor.ResetMotion(); Physics.SyncTransforms();
                var pad = correction.Find("Photo1_Branch_LeftLeaf_RightWater").GetComponent<ExteriorJumpPad>();
                var socket = Read<LeafInstallTarget[]>(pad, "_sockets")[0]; var port = Read<SapInjectionPort[]>(pad, "_ports")[0];
                Check(socket.KeepsLeafUntilUsed && socket.GetComponent<StageOneLeafUse>() == null && Vector3.Distance(socket.SocketPosition, port.transform.position) > 3,
                    "Photo1 left leaf socket and distinct right water inlet.");
                leaf = UnityEngine.Object.Instantiate(settings.leaf.offlinePrefab); leaf.Initialize(settings.leaf, actor.transform, socket.SocketPosition, socket.SocketPosition, null, true);
                Check(leaf.TryInstallAtSocket(socket), "Install left-hole launch leaf."); leaf.Tick(120); Check(leaf.Installed, "Launch leaf remains until use.");
                var surface = Read<Collider>(leaf, "_surface"); Physics.SyncTransforms();
                var capsule = actor.GetComponent<CharacterController>(); var feet = surface.bounds.center; feet.y = surface.bounds.max.y + .02f;
                player.Motor.Teleport(feet - actor.transform.TransformVector(capsule.center) + Vector3.up * capsule.height * Mathf.Abs(capsule.transform.lossyScale.y) * .5f);
                Physics.SyncTransforms(); Check(leaf.Supports(capsule, .64f), "Actual player stands on left-hole leaf.");
                held = UnityEngine.Object.Instantiate(settings.sap.offlinePrefab); held.transform.position = port.InteractionPoint + Vector3.up * 5;
                var hose = new SapHoseSprayer(); var aim = new Ray(held.transform.position, Vector3.down);
                for (int i = 0; i < 100; i++) { hose.Tick(held, aim, true, .02f, settings.sap, () => null); pad.Tick(.02f); }
                Check(pad.ChargeFraction >= 1, "Actual water hits the right inlet and charges the left outlet.");
                pad.Tick(3.1f); Check(player.Motor.Velocity.y > 0 && leaf.State == LeafState.Returning, "Player launches from left leaf; leaf recalls after launch.");
                leaf.BeginReturn(); UnityEngine.Object.DestroyImmediate(leaf.gameObject); leaf = null;
                player.Motor.Teleport(saved); player.Motor.ResetMotion();

                return "PASS: three curved outlets push outward, block and reopen independently; right-hole water charge launches the left-leaf rider.";
            }
            finally
            {
                if (leaf != null) UnityEngine.Object.DestroyImmediate(leaf.gameObject);
                if (held != null) UnityEngine.Object.DestroyImmediate(held.gameObject);
                player.Motor.Teleport(saved); player.Motor.ResetMotion();
            }
        }
    }
}
