using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Herbalist.Development;
using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.Editor
{
    [InitializeOnLoad]
    internal static class QaVerification
    {
        private const string Running = "Herbalist.QA.Verification";
        private const string Output = "Logs/HerbalistQA/PlayChecks.txt";

        static QaVerification() => EditorApplication.playModeStateChanged += OnState;

        public static void Run()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Output));
            File.WriteAllText(Output, "");
            SessionState.SetBool(Running, true);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(HerbalistQa.Main);
            EditorApplication.EnterPlaymode();
        }

        public static void Build()
        {
            var extra = Environment.GetCommandLineArgs().Contains("-herbalist-build-scripts-only")
                ? BuildOptions.BuildScriptsOnly : BuildOptions.None;
            if (!HerbalistQaWindow.BuildClient(extra)) throw new InvalidOperationException("QA client build failed.");
        }

        private static async void OnState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running, false)) return;
            if (state != PlayModeStateChange.EnteredPlayMode) return;
            EditorSceneManager.playModeStartScene = null;
            int failures = 0;
            bool multiplayer = Environment.GetCommandLineArgs().Contains("-herbalist-verify-multiplayer");
            IEnumerable<string> points = multiplayer ? new[] { "menu/ready", "s1/leaf-gather", "s2/2-start" } : new[]
            {
                "s1/start", "s1/leaf-herb", "s1/sap-gather", "s1/transfer", "s1/leaf-craft", "s1/sap-craft", "s1/drink", "s1/potions",
                "s1/forest-before", "s1/forest-saved", "s1/first-platform", "s1/first-landed",
                "s1/cloth", "s1/inlet", "s1/leaf-cross", "s1/water-block", "s1/cloth-lower", "s1/grow", "s1/launch",
                "s1/cloth-swap", "s1/last-cloth", "s1/arrival-one", "s1/arrival-two",
                "s2/arrival",
                "s3/start", "s3/one", "s3/both", "s3/leaf", "s3/t1", "s3/drop1", "s3/t2", "s3/t3", "s3/stones",
                "s3/retry", "s3/t4", "s3/drop2", "s3/landing", "s3/landed", "s3/moon", "s3/ending", "s3/credits"
            }.Concat(Enumerable.Range(1, 3).SelectMany(i => new[] { "descent", "start", "leak", "refill", "retry", "exit" }.Select(s => $"s2/{i}-{s}"))).Append("s2/final");

            var args = Environment.GetCommandLineArgs();
            int filterIndex = Array.IndexOf(args, "-herbalist-verify-points");
            if (filterIndex >= 0 && args.Length > filterIndex + 1)
                points = args[filterIndex + 1].Split(',').SelectMany(point => point == "checkpoints"
                    ? HerbalistQaWindow.ReadCheckpoints().Select(c => $"s1/save-{c.index}")
                    : new[] { point });
            else if (!multiplayer)
                points = points.Concat(HerbalistQaWindow.ReadCheckpoints().Select(c => $"s1/save-{c.index}"));

            foreach (string point in points)
            {
                try
                {
                    var session = FusionLobbySession.Instance;
                    if (session != null && session.HasNetworkSession) await session.LeaveAsync();
                    HerbalistQaWindow.StopClient();
                    var load = SceneManager.LoadSceneAsync(HerbalistQa.SceneFor(point, multiplayer));
                    while (!load.isDone) await Task.Yield();
                    await Task.Yield();
                    await HerbalistQa.Run(point, multiplayer, "qa-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                        0, 0, HerbalistQaWindow.StartClient, Write);
                    await Task.Yield();
                    if (point == "s1/forest-saved")
                    {
                        var actor = StageActor.All.First(a => a.Available);
                        var position = actor.transform.position;
                        actor.GetComponent<Herbalist.Player.PlayerController>().Motor.Teleport(Vector3.down * 100);
                        await Task.Delay(150);
                        if (Vector3.Distance(actor.transform.position, position) > 3f)
                            throw new InvalidOperationException("복귀 테스트 결과가 시작점과 다릅니다.");
                    }
                    if (point == "s1/arrival-one" || point == "s1/arrival-two")
                    {
                        await Task.Delay(120);
                        if (StageOneFlow.Instance.InsideCount != 1) throw new InvalidOperationException("도착한 인원이 한 명이 아닙니다.");
                    }
                    if (multiplayer && (point is "s1/opening" or "s1/transition"))
                    {
                        var sessionWithVideo = FusionLobbySession.Instance;
                        string destination = point == "s1/opening" ? HerbalistQa.StageOne : HerbalistQa.Maze;
                        double deadline = Time.realtimeSinceStartupAsDouble + 240;
                        while (sessionWithVideo.State != LobbyState.Error && !(sessionWithVideo.State == LobbyState.Playing &&
                            SceneManager.GetActiveScene().path == destination && sessionWithVideo.ConnectedCount == 2 &&
                            StageActor.All.Count(actor => actor.Available) == 2) && Time.realtimeSinceStartupAsDouble < deadline)
                            await Task.Delay(100);
                        if (sessionWithVideo.State != LobbyState.Playing || sessionWithVideo.ConnectedCount != 2 ||
                            SceneManager.GetActiveScene().path != destination || StageActor.All.Count(actor => actor.Available) != 2)
                            throw new InvalidOperationException("영상 종료 후 두 캐릭터가 진입하지 못했습니다. " + sessionWithVideo.Message);
                    }
                    if (point == "s2/final")
                    {
                        double deadline = Time.realtimeSinceStartupAsDouble + 40;
                        while (SceneManager.GetActiveScene().path != HerbalistQa.Ride && Time.realtimeSinceStartupAsDouble < deadline)
                            await Task.Delay(50);
                        if (SceneManager.GetActiveScene().path != HerbalistQa.Ride) throw new InvalidOperationException("3스테이지로 전환되지 않았습니다.");
                    }
                    if (point == "s3/credits" && UnityEngine.Object.FindAnyObjectByType<BranchRideFlow>().CurrentPhase != BranchRidePhase.Complete)
                        throw new InvalidOperationException("엔딩 패널이 열리지 않았습니다.");
                    if (point == "s1/leaf-gather" && StageOneFlow.Instance.Minigame != StageMinigameKind.Gathering)
                        throw new InvalidOperationException("채집 미니게임이 시작되지 않았습니다.");
                    if (point == "s2/2-start" && UnityEngine.Object.FindAnyObjectByType<DescendingMazeFlow>().CurrentMaze != 1)
                        throw new InvalidOperationException("두 번째 미로가 활성화되지 않았습니다.");
                    if (multiplayer && point == "menu/ready" && RoomControl.Instance?.Ready != true)
                        throw new InvalidOperationException("두 캐릭터가 선택되지 않았습니다.");
                    Write("PASS " + point);
                }
                catch (Exception e)
                {
                    failures++;
                    Write("FAIL " + point + " " + e);
                }
            }
            HerbalistQaWindow.StopClient();
            Write("Failures=" + failures);
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }

        private static void Write(string line) => File.AppendAllText(Output, line + Environment.NewLine);
    }
}
