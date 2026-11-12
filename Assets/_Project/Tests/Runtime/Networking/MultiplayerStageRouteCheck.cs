#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.Presentation;
using Herbalist.StageOne;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Herbalist.Tests
{
    // Explicit two-process smoke check. Potion and solved-maze state are fixtures.
    public static class MultiplayerStageRouteCheck
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
        private static FieldInfo Field(object target, string name) => target.GetType().GetField(name, Fields);
        private static T Read<T>(object target, string name) => (T)Field(target, name).GetValue(target);
        private static NetworkPlayer Net(StageActor actor) => actor.GetComponent<NetworkPlayer>();
        private static StageActor LocalActor => NetworkPlayer.Local.GetComponent<StageActor>();

        private static async Task Wait(Func<bool> predicate, string description, float seconds = 90f)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline) await Task.Delay(50);
            if (!predicate()) throw new InvalidOperationException("Timeout: " + description);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Run()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-herbalist-stage-route-check");
            if (index < 0 || index + 3 >= args.Length) return;
            bool host = args[index + 1] == "host";
            string checkpoints = args[index + 3];
            Directory.CreateDirectory(checkpoints);
            Application.runInBackground = true;
            Application.targetFrameRate = 45;
            try
            {
                await Wait(() => FusionLobbySession.Instance != null, "main menu");
                var session = FusionLobbySession.Instance;
                await session.ConnectAsync(args[index + 2], host);
                if (session.State == LobbyState.Error) throw new InvalidOperationException(session.Message);
                await Wait(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "room pair");
                RoomControl.Instance.Select(host ? 1 : 0);
                if (host)
                {
                    await Wait(() => RoomControl.Instance.Ready, "distinct roles");
                    RoomControl.Instance.StartGame();
                }
                await Wait(() => StageOneFlow.Instance != null && NetworkPlayer.Local != null &&
                    session.State == LobbyState.Playing && StageActor.All.Count(a => a.Available) == 2, "Stage 1 network actors");
                if (host)
                {
                    var flow = StageOneFlow.Instance;
                    flow.RefreshActors();
                    flow.Progress.Crafted = flow.settings.RequiredMask;
                    foreach (var actor in flow.Actors)
                    {
                        var kind = actor.Slot == 0 ? PlayerAbilityKind.Sap : PlayerAbilityKind.Leaf;
                        var potion = flow.settings.items.First(item => item.type == StageItemType.Potion && item.ability == kind);
                        flow.Progress.Held[actor.Slot] = flow.settings.ItemId(potion);
                        if (flow.Execute(actor, StageCommand.Drink) != flow.settings.successMessage)
                            throw new InvalidOperationException("Potion consumption failed.");
                    }
                }
                await Wait(() => StageActor.All.Count(actor => actor.Available && actor.Abilities.Unlocked) == 2,
                    "both potion abilities");
                Debug.Log("[MultiplayerStageRoute] STAGE1 READY " + (host ? "host" : "client"));
                await CheckClothAndBranchRecovery(host, checkpoints);
                if (host)
                {
                    var flow = StageOneFlow.Instance;
                    foreach (var actor in flow.Actors)
                    {
                        var capsule = actor.GetComponent<CharacterController>();
                        Vector3 center = flow.interior.transform.TransformPoint(flow.interior.center);
                        Net(actor).SetTransportAuthoritatively(true, center - actor.transform.TransformVector(capsule.center));
                    }
                }

                var mazeFlow = await Arrive<DescendingMazeFlow>(session);
                Debug.Log("[MultiplayerStageRoute] STAGE2 ARRIVED " + (host ? "host" : "client"));
                var screen = Object.FindFirstObjectByType<PlayScreenController>();
                if (screen.Mode != PlayScreenMode.Split) screen.TryToggle();
                await Wait(() => screen.IsSplitVisible, "both player views in split screen");
                var boards = Read<SapMazeBoard[]>(mazeFlow, "_mazes");
                int frontLayer = Read<int>(mazeFlow, "_frontLayer");
                int backLayer = Read<int>(mazeFlow, "_backLayer");
                for (int i = 0; i < boards.Length; i++)
                {
                    var board = boards[i];
                    await Wait(() => mazeFlow.CurrentBoard == board && board.Active, "raft station " + i);
                    await Wait(() => StageActor.All.Count(actor => HasMazeSide(actor, frontLayer, backLayer)) == 2,
                        "both local and remote cameras have their own maze side");
                    await ConfirmClient(host, checkpoints, "maze-" + i);
                    if (host)
                    {
                        // Drive the raft and scene transitions without simulating the puzzle.
                        var state = board.State;
                        state.Phase = MazePhase.Solved;
                        Field(board, "_state").SetValue(board, state);
                    }
                }

                var ride = await Arrive<BranchRideFlow>(session);
                await Wait(() => ride.Object != null && ride.Object.IsValid, "attached branch ride");
                AllowHiddenInput();
                var remote = StageActor.All.Single(a => a.Available && a.Slot == 0);
                if (!host)
                {
                    LocalActor.Request(StageCommand.Interact);
                    await Wait(() => BranchRideFlow.IsRiding(LocalActor), "client boarding replicated");
                    var player = NetworkPlayer.Local.Player;
                    float desired = Mathf.Approximately(player.View.ZoomMultiplier, player.Tuning.cameraZoomMinMultiplier)
                        ? player.Tuning.cameraZoomMaxMultiplier : player.Tuning.cameraZoomMinMultiplier;
                    // Feed the ordinary network input without writing the user's zoom preference.
                    player.View.SetRemoteZoom(desired);
                    await Wait(() => Mathf.Approximately(NetworkPlayer.Local.CameraZoomMultiplier, desired),
                        "zoom input acknowledged while mounted");
                }
                else
                {
                    float before = Net(remote).CameraZoomMultiplier;
                    await Wait(() => BranchRideFlow.IsRiding(remote) &&
                        !Mathf.Approximately(Net(remote).CameraZoomMultiplier, before) &&
                        Mathf.Approximately(Net(remote).Player.View.ZoomMultiplier, Net(remote).CameraZoomMultiplier),
                        "host aim camera follows the mounted client's zoom");
                    if (ride.CurrentPhase != BranchRidePhase.Boarding)
                        throw new InvalidOperationException("The ride departed with only one player.");
                    LocalActor.Request(StageCommand.Interact);
                }
                await Wait(() => ride.CurrentPhase == BranchRidePhase.Riding && BranchRideFlow.IsRiding(LocalActor),
                    "both players board and depart");
                Debug.Log("[MultiplayerStageRoute] PASS " + (host ? "host" : "client"));
                await Task.Delay(2000);
                Application.Quit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[MultiplayerStageRoute] FAIL " + (host ? "host " : "client ") + exception);
                Application.Quit(1);
            }
        }

        private static bool HasMazeSide(StageActor actor, int frontLayer, int backLayer)
        {
            if (!actor.Available || !actor.Abilities.Unlocked) return false;
            bool sap = actor.Abilities.Kind == PlayerAbilityKind.Sap;
            int front = 1 << frontLayer;
            int back = 1 << backLayer;
            return (actor.Camera.cullingMask & (front | back)) == (sap ? front : back);
        }

        private static async Task CheckClothAndBranchRecovery(bool host, string checkpoints)
        {
            var cloth = Object.FindObjectsByType<HangingCloth>()
                .Single(item => item.name == "Coop_4_SingleUseCloth");
            await Wait(() => cloth.Object != null && cloth.Object.IsValid, "attached Stage 1 cloth");
            var knot = Read<Transform>(cloth, "_knot");
            var rider = StageActor.All.Single(actor => actor.Available && actor.Abilities.Kind == PlayerAbilityKind.Leaf);
            var sap = StageActor.All.Single(actor => actor.Available && actor.Abilities.Kind == PlayerAbilityKind.Sap);
            var sapStart = sap.transform.position;
            var capsule = rider.GetComponent<CharacterController>();
            var branch = Object.FindObjectsByType<StageOneDocumentMechanism>().Single(item => item.name == "FallingBranch_25");
            var target = Read<LeafInstallTarget>(branch, "_leafTarget");
            var art = Read<Transform[]>(branch, "_movingArt")[0];
            var rest = art.position;
            AllowHiddenInput();
            if (host)
            {
                var sapCapsule = sap.GetComponent<CharacterController>();
                Net(sap).SetTransportAuthoritatively(true, knot.position - sap.transform.TransformVector(sapCapsule.center));
            }
            else
            {
                await Wait(() => Vector3.Distance(sap.GetComponent<CharacterController>().bounds.center, knot.position) < 1f,
                    "sap player at the first cloth");
                LocalActor.Request(StageCommand.Interact);
                await Task.Delay(1000);
                if (HangingCloth.IsRiding(sap) || HangingCloth.IsGrabbing(sap))
                    throw new InvalidOperationException("The first cloth accepted the sap player.");
            }
            await ConfirmClient(host, checkpoints, "cloth-role-checked");
            if (host)
            {
                Net(sap).SetTransportAuthoritatively(true, sapStart);
                Net(rider).SetTransportAuthoritatively(true, knot.position - rider.transform.TransformVector(capsule.center));
                LocalActor.Request(StageCommand.Interact);
            }
            await Wait(() => HangingCloth.IsRiding(rider) && cloth.WasUsed, "leaf player grabs the first cloth");
            await ConfirmClient(host, checkpoints, "cloth-riding");
            if (host)
            {
                // A partner's fall must leave the attached player and the cloth alone.
                Net(sap).SetTransportAuthoritatively(false, sap.transform.position);
                Net(sap).TeleportAuthoritatively(Vector3.down * 100f);
            }
            await Wait(() => !Net(sap).Transported && sap.transform.position.y > -15f &&
                !Mathf.Approximately(sap.transform.position.y, knot.position.y),
                "partner automatically recovers");
            if (!HangingCloth.IsRiding(rider) || !cloth.WasUsed)
                throw new InvalidOperationException("The partner's fall reset the active cloth.");
            await ConfirmClient(host, checkpoints, "partner-fall-preserved-cloth");
            if (host) LocalActor.Request(StageCommand.ReleaseCloth);
            await Wait(() => !HangingCloth.IsRiding(rider) && !Net(rider).Transported && !cloth.WasUsed,
                "rider release resets its cloth");
            await ConfirmClient(host, checkpoints, "cloth-restored");

            if (host)
            {
                Net(rider).SetTransportAuthoritatively(true, target.SocketPosition + Vector3.up * 8f);
                if (!rider.Abilities.Leaf.TryThrowAtSocket(target))
                    throw new InvalidOperationException("Branch fixture leaf throw failed.");
                await Wait(() => target.HasLeaf, "installed branch leaf");
                var surface = Read<BoxCollider>(target.InstalledLeaf, "_surface");
                var feet = surface.transform.TransformPoint(surface.center + Vector3.up * (surface.size.y * .5f));
                Vector3 footOffset = rider.transform.TransformVector(capsule.center) -
                    Vector3.up * capsule.height * Mathf.Abs(capsule.transform.lossyScale.y) * .5f;
                Net(rider).SetTransportAuthoritatively(true, feet - footOffset);
            }
            await Wait(() => branch.Solved && branch.Progress >= 1f && !art.gameObject.activeSelf, "recalled leaf drops the branch");
            await ConfirmClient(host, checkpoints, "branch-dropped");
            if (host) Net(rider).SetTransportAuthoritatively(true, rest + Vector3.up * 12f);
            await Wait(() => !branch.Solved && art.gameObject.activeSelf &&
                Vector3.Distance(art.position, rest) < .01f, "branch resets independently of respawn");
            await ConfirmClient(host, checkpoints, "branch-restored");
            Debug.Log("[MultiplayerStageRoute] STAGE1 CLOTH AND BRANCH PASS " + (host ? "host" : "client"));
        }

        private static void AllowHiddenInput()
        {
            var cursor = Object.FindFirstObjectByType<GameplayCursor>();
            if (cursor != null) cursor.enabled = false;
        }

        private static Task ConfirmClient(bool host, string checkpoints, string name)
        {
            string path = Path.Combine(checkpoints, name);
            if (host) return Wait(() => File.Exists(path), "client observed " + name);
            File.WriteAllText(path, string.Empty);
            return Task.CompletedTask;
        }

        private static async Task<T> Arrive<T>(FusionLobbySession session) where T : MonoBehaviour
        {
            await Wait(() => Object.FindFirstObjectByType<T>() != null && NetworkPlayer.Local != null &&
                session.State == LobbyState.Playing && StageActor.All.Count(a => a.Available && a.Abilities.Unlocked) == 2,
                typeof(T).Name + " with both retained abilities");
            return Object.FindFirstObjectByType<T>();
        }
    }
}
#endif
