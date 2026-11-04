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
        private static void Write(object obj, string field, object value) => obj.GetType().GetField(field, Private).SetValue(obj, value);
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
                    var end = stream.transform.Find("FlowEnd").position;
                    var point = Vector3.Lerp(stream.transform.position, end, .75f);
                    var cc = actor.GetComponent<CharacterController>(); player.Motor.Teleport(point - actor.transform.TransformVector(cc.center)); Physics.SyncTransforms();
                    Check(traversal.TouchesBareTree(cc), "Open stream obstructs player at the mushroom route: " + mechanism.name);
                    leaf = UnityEngine.Object.Instantiate(settings.leaf.offlinePrefab);
                    leaf.Initialize(settings.leaf, actor.transform, target.SocketPosition, target.SocketPosition, null, true);
                    Check(leaf.TryInstallAtSocket(target), "Install leaf plug: " + mechanism.name); mechanism.Tick(.02f); Physics.SyncTransforms();
                    Check(!stream.activeSelf && !stream.GetComponent<Collider>().enabled, "Leaf removes both water and obstruction.");
                    Check(blockers.Where(m => m != mechanism).All(m => Read<GameObject>(m, "_jet").activeSelf), "Other outlets remain independently open.");
                    leaf.Tick(5.01f); mechanism.Tick(.02f); Check(stream.activeSelf && stream.GetComponent<Collider>().enabled, "Timed recall reopens the corresponding stream.");
                    UnityEngine.Object.DestroyImmediate(leaf.gameObject); leaf = null;
                }
                player.Motor.Teleport(saved); Physics.SyncTransforms();
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

                var eject = root.GetComponentsInChildren<StageOneDocumentMechanism>().First(m => m.Kind == StageOneDocumentMechanismKind.EjectObstacle);
                var water = Read<GameObject>(eject, "_jet");
                Write(eject, "_amount", 0f); Write(eject, "_completed", false);
                Write(eject, "_lastPresentedAmount", 0f); Write(eject, "_ejectionJetRemaining", 0f);
                Read<SapInjectionPort>(eject, "_port").Consume(); eject.Tick(.001f);
                Check(!water.activeSelf && water.GetComponent<Collider>() == null, "Stone outlet is initially off and adds no gameplay obstruction.");
                Write(Read<HangingCloth>(eject, "_requiredCloth"), "_used", true);
                player.Motor.Teleport(Read<BoxCollider>(eject, "_activationArea").bounds.center); Physics.SyncTransforms();
                var ejectPort = Read<SapInjectionPort>(eject, "_port");
                ejectPort.Inject(.25f); eject.Tick(.25f); Check(water.activeSelf && eject.Progress > 0 && !eject.Solved, "Water begins while stone is moving.");
                ejectPort.Inject(1); eject.Tick(1); Check(eject.Solved && water.activeSelf, "Water remains visible as stone clears the path.");
                eject.Tick(1.3f); Check(!water.activeSelf && eject.Solved, "Water cue ends while cleared path remains open.");
                return "PASS: three horizontal 04_Musroom outlets, actual-water leaf queries, independent block/reopen cycles and player obstruction; right-hole real water charge launches rider from left leaf; stone ejection water starts with motion and ends without reblocking path.";
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
