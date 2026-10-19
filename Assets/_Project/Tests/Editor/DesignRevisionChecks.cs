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
            foreach (string path in new[] { CurrentStagePaths.StageOne, CurrentStagePaths.Maze, CurrentStagePaths.Altar })
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
                if (path == CurrentStagePaths.StageOne)
                {
                    Check(Object.FindFirstObjectByType<StageOneFlow>() != null &&
                          level.nextScenePath == CurrentStagePaths.Maze, "stage one flow/transition");
                }
                else if (path == CurrentStagePaths.Maze)
                {
                    var flow = Object.FindFirstObjectByType<DescendingMazeFlow>();
                    Check(flow != null, "descending maze flow");
                    Check(Object.FindObjectsByType<SapMazeBoard>(FindObjectsSortMode.None).Length == 3 && level.nextScenePath == CurrentStagePaths.Altar, "three boards and altar destination");
                    var imported = GameObject.Find("ImportedMazeVisuals");
                    Check(imported != null && imported.GetComponentsInChildren<MeshRenderer>().Length == 3, "three imported maze faces");
                    Check(Read<StageTwoPulleyPresentation>(flow, "_pulleyPresentation") != null, "imported pulley presentation connected");
                    foreach (var board in Object.FindObjectsByType<SapMazeBoard>(FindObjectsSortMode.None))
                        Check(!board.transform.Find("MazeWalls").GetComponent<MeshRenderer>().enabled &&
                              board.transform.Find("MazeWalls").GetComponent<MeshCollider>().sharedMesh != null,
                              "imported face with graph wall collision");
                }
                else Check(Object.FindObjectsByType<AltarRing>(FindObjectsSortMode.None).Length == 3 && Object.FindFirstObjectByType<FinalStageExit>() != null, "three rotating rings/final exit");
            }
            return "PASS authored: three scenes, unique target IDs, preauthored UGUI, stage one transition, maze and altar.";
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
            var pulley = Read<StageTwoPulleyPresentation>(flow, "_pulleyPresentation");
            var firstWheel = Read<Transform[]>(pulley, "_wheels")[0];
            // Offline Play may have already advanced while the test command was sent.
            Write(flow, "_current", -1); Write(flow, "_phase", 0); Write(flow, "_travel", 0f);
            var entryY = Read<Transform[]>(flow, "_stations")[0].position.y;
            foreach (var raft in rafts) raft.position = new Vector3(raft.position.x, entryY, raft.position.z);
            pulley.Present(0f);
            var wheelAtEntry = firstWheel.localRotation;
            var identities = rafts.ToArray();
            var screen = Object.FindFirstObjectByType<PlayScreenController>();
            Check(screen.Mode == PlayScreenMode.Personal && screen.IsLocked && !screen.TryToggle(), "personal viewport forced");
            foreach (var board in boards)
            {
                var copy = Object.Instantiate(board.Definition); copy.initialVolume = 500; copy.speed = 1;
                board.ConfigureOfflineTestDefinition(copy);
            }
            Check(boards[0].Surface.MazeCollider.sharedMesh.name == "Low_2" &&
                boards[1].Surface.MazeCollider.sharedMesh.name == "Low_1", "imported maze order");
            try
            {
                // Keep the final goal one step short to avoid a scene load inside this synchronous check.
                for (int maze = 0; maze < boards.Length; maze++)
                {
                    for (int tick = 0; tick < 100 && flow.CurrentMaze < maze; tick++) flow.Tick(.1f);
                    Check(flow.CurrentMaze == maze, "automatic descent to maze " + maze);
                    if (maze == 0)
                    {
                        Check(Quaternion.Angle(wheelAtEntry, firstWheel.localRotation) > 1f, "imported pulley turns with raft descent");
                        Check(rafts.All(r => r.GetComponentsInChildren<MeshRenderer>().Count(v => v.enabled) >= 2), "imported platforms ride both rafts");
                    }
                    actor.GetComponent<PlayerController>().Motor.Teleport(rafts[0].position + Vector3.up * .05f);
                    partner.GetComponent<PlayerController>().Motor.Teleport(rafts[1].position + Vector3.up * .05f);
                    var board = boards[maze]; board.SetPlayable(true);
                    Check(board.ReceiveAbilityCycle(actor.Abilities, 1) && board.Controlling, "R begins maze control");
                    var path = new System.Collections.Generic.List<Vector2>();
                    Check(board.Surface != null && board.Surface.TryFindPath(board.SurfaceStart, board.SurfaceGoal, path),
                        "mesh channel connects start and goal " + maze);
                    if (maze == 1)
                    {
                        // These Low_1 passages were wrongly removed when a deep
                        // decorative point on the board rim set the depth cutoff.
                        foreach (var passage in new[] { new Vector2(2.77f, 3.6f), new Vector2(-4.5f, 7.2f) })
                        {
                            Check(Vector2.Distance(board.Surface.Snap(passage), passage) < .08f,
                                "marked passage remains open " + passage);
                            var through = new System.Collections.Generic.List<Vector2>();
                            Check(board.Surface.TryFindPath(board.SurfaceStart, passage, through) &&
                                board.Surface.TryFindPath(passage, board.SurfaceGoal, through),
                                "marked passage connects to both ends " + passage);
                        }
                    }
                    for (int edge = 1; edge < path.Count; edge++)
                    {
                        if (maze == 2 && edge >= path.Count - 2) break;
                        for (int tick = 0; tick < 100 && Vector2.Distance(board.SapPosition,path[edge]) > .06f; tick++)
                        {
                            var move = (path[edge] - board.SapPosition).normalized;
                            board.ReceiveMovement(actor.GetComponent<PlayerController>(), move);
                            flow.Tick(.01f);
                            if (board.State.Phase == MazePhase.Solved) break;
                        }
                        Check(Vector2.Distance(board.SapPosition,path[edge]) <= .06f || board.State.Phase == MazePhase.Solved,
                            "mesh channel step " + maze + "/" + edge);
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
                        var before = board.SapPosition; board.Tick(.1f);
                        Check(Vector2.Distance(before,board.SapPosition)<.001f, "parked sap position retained");
                    }
                }
                return "PASS maze: personal screen/Tab lock, R input routing, mesh channels, automatic descent without reboarding, same raft identities and parked position retention.";
            }
            finally { Object.Destroy(partner.gameObject); }
        }

        public static string MazeTrailLeak()
        {
            Check(Application.isPlaying, "Play revision maze");
            var board = Object.FindObjectsByType<SapMazeBoard>(FindObjectsSortMode.None)
                .First(x => x.name == "SapMaze_1");
            var leak = board.Leaks[0];
            var socket = Read<Vector2[]>(board, "_surfaceLeaks")[0];
            var state = Read<MazeRunState>(board, "_state");
            var oldPosition = Read<Vector2>(board, "_surfacePosition");
            var points = Read<Vector2[]>(board, "_trailPoints");
            var oldPoints = points.ToArray();
            var oldFirst = Read<int>(board, "_trailFirst");
            var oldCount = Read<int>(board, "_trailCount");
            var oldPlayable = Read<bool>(board, "_playable");
            var oldLeakMask = Read<int>(board, "_leakMask");
            var dummy = new GameObject("MazeTrailLeakCheckLeaf").AddComponent<LeafProjectile>();
            try
            {
                board.SetPlayable(true);
                var testState = state;
                testState.Phase = MazePhase.Running;
                testState.Volume = board.Definition.initialVolume;
                Write(board, "_state", testState);
                Write(board, "_surfacePosition", socket + Vector2.right * .6f);
                points[0] = socket - Vector2.right * .4f;
                Write(board, "_trailFirst", 0);
                Write(board, "_trailCount", 1);
                board.Tick(.5f);
                Check(board.State.Volume < testState.Volume && board.AnyLeak,
                    "unblocked leak drains after the head has passed");

                typeof(LeafProjectile).GetField("<State>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(dummy, LeafState.Installed);
                leak.Target.Attach(dummy);
                var blockedVolume = board.State.Volume;
                board.Tick(.5f);
                Check(Mathf.Approximately(board.State.Volume, blockedVolume) && !board.AnyLeak,
                    "installed leaf stops trail leakage");

                leak.Target.Detach(dummy);
                testState = Read<MazeRunState>(board, "_state");
                testState.Volume = board.Definition.initialVolume * .1f;
                Write(board, "_state", testState);
                Write(board, "_trailCount", 1);
                board.Tick(.5f);
                Check(Mathf.Approximately(board.State.Volume, testState.Volume) && !board.AnyLeak,
                    "leak stops once shrinking tail no longer reaches the socket");
                return "PASS maze trail leak: exposed trail drains, installed leaf blocks, short tail stops draining.";
            }
            finally
            {
                leak.Target.Detach(dummy);
                Object.Destroy(dummy.gameObject);
                Write(board, "_state", state);
                Write(board, "_surfacePosition", oldPosition);
                System.Array.Copy(oldPoints, points, points.Length);
                Write(board, "_trailFirst", oldFirst);
                Write(board, "_trailCount", oldCount);
                Write(board, "_leakMask", oldLeakMask);
                board.SetPlayable(oldPlayable);
            }
        }
    }
}
