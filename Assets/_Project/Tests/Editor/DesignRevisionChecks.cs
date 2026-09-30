using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Herbalist.Player;
using Herbalist.Levels;
using Herbalist.Abilities;
using Herbalist.StageOne;
using Herbalist.Networking;
using Herbalist.Presentation;
using Object = UnityEngine.Object;
namespace Herbalist.Editor
{
    public static class DesignRevisionChecks
    {
        private static void Check(bool ok, string label) { if (!ok) throw new Exception(label); }
        private static T Read<T>(object obj, string field) => (T)obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        private static void Write(object obj, string field, object value) => obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
        public static string Authored()
        {
            Check(!Application.isPlaying, "Edit mode required");
            foreach (string path in new[] { CurrentStagePaths.Exterior, CurrentStagePaths.Maze, CurrentStagePaths.Altar })
            {
                var scene = EditorSceneManager.OpenScene(path);
                var level = Object.FindFirstObjectByType<StageLevel>();
                Check(level != null && level.abilityRules != null && level.abilityRules.contextualUse, "stage rules " + path);
                Check(level.abilityRules.leaf.permanentInstallation && level.abilityRules.leaf.installedCapacity && level.abilityRules.leaf.rangeHeightMultiplier == 3, "leaf rules");
                Check(Object.FindFirstObjectByType<Herbalist.GameUI.GameOverlayHud>() != null && Object.FindFirstObjectByType<Camera>() != null, "authored UI/camera");
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true)) Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0, "missing script " + t.name);
                var ids = Object.FindObjectsByType<LeafInstallTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Select(x => x.Id).ToArray();
                Check(ids.All(x => x > 0) && ids.Distinct().Count() == ids.Length, "unique leaf IDs " + path);
                if (path == CurrentStagePaths.Exterior)
                {
                    var layout = AssetDatabase.LoadAssetAtPath<ExteriorLayoutSettings>(PdfExteriorLayoutBuilder.SettingsPath);
                    Check(layout != null, "PDF layout settings");
                    Check(Object.FindObjectsByType<ExteriorJumpPad>(FindObjectsSortMode.None).Length == layout.branches.Count(x => x.kind == ExteriorBranchKind.JumpPad), "PDF jump branches");
                    Check(Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None).Length == layout.cloths.Length, "PDF interactive blue cloths");
                    Check(Object.FindObjectsByType<LandingCheckpoint>(FindObjectsSortMode.None).Length >= layout.branches.Length, "branch landing checkpoints");
                    var gaps = GameObject.Find("PdfExteriorLayout").transform.Cast<Transform>().Where(x => x.name.StartsWith("LeafBridge_")).ToArray();
                    Check(gaps.Length == layout.leafBridges.Length && gaps.All(x => x.GetComponentsInChildren<Collider>().All(c => c.bounds.size.y > c.bounds.size.z || c.bounds.size.y > c.bounds.size.x)), "green gaps contain walls, no prebuilt bridge floor");
                    Check(Object.FindFirstObjectByType<StageOneFlow>().entranceCondition != null && level.nextScenePath == CurrentStagePaths.Maze, "exterior gating/transition");
                }
                else if (path == CurrentStagePaths.Maze)
                {
                    Check(Object.FindFirstObjectByType<DescendingMazeFlow>() != null, "descending maze flow");
                    Check(Object.FindObjectsByType<SapMazeBoard>(FindObjectsSortMode.None).Length == 3 && level.nextScenePath == CurrentStagePaths.Altar, "three boards and altar destination");
                }
                else Check(Object.FindObjectsByType<AltarRing>(FindObjectsSortMode.None).Length == 3 && Object.FindFirstObjectByType<FinalStageExit>() != null, "three rotating rings/final exit");
            }
            return "PASS authored: three scenes, unique target IDs, preauthored UGUI, exterior routes/checkpoints, exactly two persistent rafts and altar.";
        }
        private static LeafProjectile Install(LeafAbilitySettings settings, Transform owner, LeafInstallTarget target)
        {
            var leaf = Object.Instantiate(settings.offlinePrefab);
            leaf.Initialize(settings, owner, owner.position, target.transform.position, x => Object.Destroy(x.gameObject), false);
            typeof(LeafProjectile).GetMethod("Install", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(leaf, new object[] { target, target.transform.position, Vector3.up, Vector3.forward });
            return leaf;
        }
        private static StageActor Partner(StageActor actor)
        {
            var copy = Object.Instantiate(actor.gameObject); copy.name = "RevisionCheck_Partner";
            copy.GetComponent<PlayerController>().SetLocalControl(false);
            var result = copy.GetComponent<StageActor>(); result.offlineSlot = 1;
            result.Abilities.ConfigureNetwork(true); return result;
        }
        public static string Exterior()
        {
            Check(Application.isPlaying && Object.FindFirstObjectByType<ExteriorStageFlow>() != null, "Play revision exterior");
            var actor = StageActor.All.First(x => x.Local); var player = actor.GetComponent<PlayerController>();
            var partner = Partner(actor); var leaves = new List<LeafProjectile>();
            var originalState = player.Motor.CaptureState();
            var pads = Object.FindObjectsByType<ExteriorJumpPad>(FindObjectsSortMode.None).OrderBy(x => x.name).ToArray();
            try
            {
                actor.Abilities.RevokeLeaf(); actor.Abilities.UnlockLeaf();
                for (int i = 0; i < 3; i++)
                {
                    var target = Read<LeafInstallTarget[]>(pads[i], "_sockets")[0];
                    var leaf = Install(actor.Abilities.Leaf.Settings, actor.transform, target); leaves.Add(leaf);
                    Read<List<LeafProjectile>>(actor.Abilities.Leaf, "leaves").Add(leaf);
                    leaf.Tick(20); Check(leaf.Installed, "leaf permanent beyond old lifetime");
                }
                var fourth = Read<LeafInstallTarget[]>(pads[3], "_sockets")[0];
                player.Motor.Teleport(fourth.transform.position + new Vector3(0, .2f, -2)); Physics.SyncTransforms();
                var ray = new Ray(fourth.transform.position + Vector3.up * 3, Vector3.down);
                actor.Abilities.Tick(actor.Abilities.Input.CycleSequence + 1, actor.Abilities.Input.UseSequence, ray, .3f, false, true);
                Check(leaves[0].State == LeafState.Returning && actor.Abilities.Leaf.ActiveCount == 3, "oldest recalled, slot reserved for new flight");
                player.Motor.Teleport(fourth.transform.position + Vector3.back * 30);
                Check(!actor.Abilities.Leaf.TryThrowAtTarget(ray) && actor.Abilities.Leaf.RangeRejected, "player-relative range rejects without consuming");
                actor.Abilities.Tick(actor.Abilities.Input.CycleSequence + 1, actor.Abilities.Input.UseSequence + 1, ray, .1f);
                Check(leaves[1].State == LeafState.Returning, "left click recalls oldest installed leaf without a mode");
                actor.Abilities.Leaf.Clear(); leaves.Clear();
                var pad = pads[0]; var socket = Read<LeafInstallTarget[]>(pad, "_sockets")[0]; var inlet = Read<SapInjectionPort[]>(pad, "_ports")[1];
                var installed = Install(actor.Abilities.Leaf.Settings, actor.transform, socket); leaves.Add(installed);
                player.Motor.Teleport(socket.SocketPosition + Vector3.up * .04f); Physics.SyncTransforms();
                for (int i = 0; i < 15; i++) player.Motor.Simulate(Vector3.zero, .02f);
                Check(player.Motor.IsGrounded, "rider grounded on leaf");
                float initialRiderHeight = player.transform.position.y;
                float initialLeafHeight = socket.SocketPosition.y;
                var partnerPlayer = partner.GetComponent<PlayerController>();
                partnerPlayer.Motor.Teleport(socket.SocketPosition + Vector3.right * .8f + Vector3.up * .04f); Physics.SyncTransforms();
                for (int i = 0; i < 15; i++) partnerPlayer.Motor.Simulate(Vector3.zero, .02f);
                float initialPartnerHeight = partner.transform.position.y;
                for (int i = 0; i < 31; i++) { inlet.Inject(.1f); pad.Tick(.1f); }
                Check(player.Motor.MovementLocked && partnerPlayer.Motor.MovementLocked, "both standing riders automatically carried without Space");
                var padSettings = Read<SapJumpPadSettings>(pad, "_settings");
                pad.Tick(Mathf.Max(0, padSettings.eruptionRiseDuration - (padSettings.flightDuration - Read<float>(pad, "_flight"))) + .00001f);
                installed.Tick(.001f);
                Check(Mathf.Abs(socket.SocketPosition.y - initialLeafHeight - padSettings.presentationRise) < .02f, "fast eruption uses world height despite scaled hole parent");
                Check(Vector3.Distance(installed.transform.position, socket.SocketPosition) < .01f, "installed leaf follows fast eruption socket");
                var jet = Read<GameObject[]>(pad, "_jets")[0];
                Check(jet.activeSelf && Mathf.Abs(jet.GetComponent<Renderer>().bounds.size.y - padSettings.presentationRise) < .02f, "sap jet grows with leaf height");
                Check(Mathf.Abs(player.transform.position.y - initialRiderHeight - padSettings.presentationRise) < .02f && Mathf.Abs(partner.transform.position.y - initialPartnerHeight - padSettings.presentationRise) < .02f, "both riders delivered to leaf apex");
                Check(!player.Motor.MovementLocked && !partnerPlayer.Motor.MovementLocked && player.Motor.CaptureState().ExternalAirControl && Mathf.Abs(player.Motor.Velocity.y) < .01f, "apex release unlocks normal gravity fall without ballistic jump");
                player.Motor.Simulate(Vector3.right * player.Tuning.moveSpeed, .1f); Check(player.Motor.Velocity.x > 0, "air steering changes horizontal velocity");
                Check(player.Motor.Velocity.y < 0, "gravity takes over after apex release");
                // The opposite direction also erupts without a leaf/rider requirement.
                Write(pad, "_cooldown", 0f); Write(pad, "_flight", 0f);
                var otherInlet = Read<SapInjectionPort[]>(pad, "_ports")[0];
                for (int i = 0; i < 31; i++) { otherInlet.Inject(.1f); pad.Tick(.1f); }
                Check(Read<int>(pad, "_output") == 1 && Read<float>(pad, "_flight") > 0, "symmetric empty port erupts");
                partner.Abilities.RevokeLeaf(); partner.Abilities.UnlockSap();
                partner.Abilities.Sap.Configure(() => Object.Instantiate(partner.Abilities.Sap.Settings.offlinePrefab), sap => Object.Destroy(sap.gameObject), false);
                partner.GetComponent<PlayerController>().Motor.Teleport(inlet.transform.position + Vector3.back * 2);
                partner.Abilities.Sap.TickContextual(new Ray(inlet.transform.position + Vector3.up * 3, Vector3.down), true, .2f);
                Check(partner.Abilities.Sap.Held != null && partner.Abilities.Sap.Ready, "context sap requires no source");
                partner.Abilities.Sap.TickContextual(ray, false, .1f); Check(partner.Abilities.Sap.Held == null, "release stops context injection");
                var cloth = Object.FindObjectsByType<HangingCloth>(FindObjectsSortMode.None).First();
                var knot = Read<Transform>(cloth, "_knot");
                player.Motor.Teleport(knot.position - Vector3.up); player.Motor.SetMovementLock(actor.Abilities.Sap, false);
                partner.GetComponent<PlayerController>().Motor.Teleport(knot.position - Vector3.up);
                Check(HangingCloth.TryInteract(actor, out _) && HangingCloth.TryInteract(partner, out _), "two players grip knot");
                var beforeKnot = knot.position;
                var expectedSwing = Quaternion.Euler(0, player.View.Yaw, 0) * Vector3.right;
                cloth.ReceiveMovement(player, Vector2.right); cloth.Tick(.1f);
                Check(Vector3.Dot(knot.position - beforeKnot, expectedSwing) > 0, "cloth moves in camera-relative input direction");
                Check(player.Motor.MovementLocked && partner.GetComponent<PlayerController>().Motor.MovementLocked, "cloth movement locked");
                Check(HangingCloth.TryInteract(actor, out _) && !player.Motor.MovementLocked, "release clears only cloth locks");
                HangingCloth.TryInteract(partner, out _);
                return "PASS exterior: permanent/capacity/range leaf, symmetric eruption, automatic paired leaf ride, apex release/gravity/air steering, sap without extraction, paired cloth grab/release.";
            }
            finally { actor.Abilities.Leaf.Clear(); foreach (var leaf in leaves) if (leaf != null) leaf.Finish(); player.Motor.RestoreState(originalState); Object.Destroy(partner.gameObject); }
        }
        public static string Altar()
        {
            Check(Application.isPlaying && Object.FindFirstObjectByType<RotatingAltar>() != null, "Play altar");
            var actor = StageActor.All.First(x => x.Local); var partner = Partner(actor); var altar = Object.FindFirstObjectByType<RotatingAltar>();
            var rings = Read<AltarRing[]>(altar, "_rings"); var leaves = new List<LeafProjectile>();
            try
            {
                Read<SapInjectionPort>(altar, "_inlet").Inject(1); altar.Tick(.1f);
                foreach (var ring in rings)
                {
                    float before = ring.CurrentAngle; altar.Tick(.1f); Check(ring.CurrentAngle != before, "independent rotation");
                    var leaf = Install(actor.Abilities.Leaf.Settings, actor.transform, Read<LeafInstallTarget>(ring, "_leafTarget")); leaves.Add(leaf);
                    before = ring.CurrentAngle; altar.Tick(.1f); Check(ring.CurrentAngle == before && !altar.CanExit, "wrong-angle freeze retained");
                    leaf.BeginReturn(); altar.Tick(.1f); Check(ring.CurrentAngle != before, "recall resumes rotation");
                    var aligned = Install(actor.Abilities.Leaf.Settings, actor.transform, Read<LeafInstallTarget>(ring, "_leafTarget")); leaves.Add(aligned); Write(ring, "_angle", 0f);
                }
                altar.Tick(3.1f); Check(altar.CanExit, "aligned stones become stairs");
                var exit = Object.FindFirstObjectByType<FinalStageExit>(); var zone = Read<StageArrivalZone>(exit, "_arrival");
                actor.GetComponent<PlayerController>().Motor.Teleport(zone.PlayerArrivalPoint); partner.GetComponent<PlayerController>().Motor.Teleport(zone.PlayerArrivalPoint + Vector3.right);
                exit.Tick(); Check(exit.Complete, "paired final arrival clears");
                return "PASS altar: different rotations, wrong-angle hold, recall resume, aligned stair transformation and paired exit.";
            }
            finally { foreach (var leaf in leaves) if (leaf != null) leaf.Finish(); Object.Destroy(partner.gameObject); }
        }
        public static string Maze()
        {
            Check(Application.isPlaying && Object.FindFirstObjectByType<DescendingMazeFlow>() != null, "Play revision maze");
            var actor = StageActor.All.First(x => x.Local); var partner = Partner(actor);
            actor.Abilities.RevokeLeaf(); actor.Abilities.UnlockSap(); partner.Abilities.RevokeLeaf(); partner.Abilities.UnlockLeaf();
            var flow = Object.FindFirstObjectByType<DescendingMazeFlow>();
            var boards = Read<SapMazeBoard[]>(flow, "_mazes"); var rafts = Read<Transform[]>(flow, "_rafts");
            var identities = rafts.ToArray();
            var screen = Object.FindFirstObjectByType<PlayScreenController>();
            Check(screen.Mode == PlayScreenMode.Personal && screen.IsLocked && !screen.TryToggle(), "personal viewport forced");
            foreach (var board in boards)
            {
                var copy = Object.Instantiate(board.Definition); copy.initialVolume = 500; copy.speed = 1;
                board.ConfigureOfflineTestDefinition(copy);
            }
            try
            {
                // Keep the final goal one step short to avoid a scene load inside this synchronous check.
                for (int maze = 0; maze < boards.Length; maze++)
                {
                    for (int tick = 0; tick < 100 && flow.CurrentMaze < maze; tick++) flow.Tick(.1f);
                    Check(flow.CurrentMaze == maze, "automatic descent to maze " + maze);
                    actor.GetComponent<PlayerController>().Motor.Teleport(rafts[0].position + Vector3.up * .05f);
                    partner.GetComponent<PlayerController>().Motor.Teleport(rafts[1].position + Vector3.up * .05f);
                    var board = boards[maze]; board.SetPlayable(true);
                    Check(board.ReceiveAbilityCycle(actor.Abilities, 1) && board.Controlling, "R begins maze control");
                    var path = SapMazeRoute.Find(board.Definition, board.Definition.startNode, board.Definition.goalNode);
                    for (int edge = 1; edge < path.Count; edge++)
                    {
                        if (maze == 2 && edge == path.Count - 1) break;
                        var delta = board.Definition.nodes[path[edge]] - board.Definition.nodes[path[edge - 1]];
                        var move = delta.normalized;
                        for (int tick = 0; tick < 500 && board.State.From != path[edge]; tick++)
                        {
                            board.ReceiveMovement(actor.GetComponent<PlayerController>(), move);
                            flow.Tick(.01f);
                            if (board.State.Phase == MazePhase.Solved) break;
                        }
                        Check(board.State.From == path[edge] || board.State.Phase == MazePhase.Solved, "routed edge " + edge);
                    }
                    if (maze < 2)
                    {
                        Check(board.State.Phase == MazePhase.Solved, "maze solved " + maze);
                        Check(rafts.SequenceEqual(identities), "same two raft objects retained");
                    }
                    else
                    {
                        board.ReceiveAbilityCycle(actor.Abilities, 1);
                        Check(!board.Controlling && !actor.GetComponent<PlayerController>().Motor.MovementLocked, "R resumes player movement");
                        var before = board.State; board.Tick(.1f);
                        Check(before.From == board.State.From && before.To == board.State.To && before.Progress == board.State.Progress, "parked sap position retained");
                    }
                }
                return "PASS maze: personal screen/Tab lock, R input routing, graph paths, automatic descent without reboarding, same raft identities and parked position retention.";
            }
            finally { Object.Destroy(partner.gameObject); }
        }
    }
}
