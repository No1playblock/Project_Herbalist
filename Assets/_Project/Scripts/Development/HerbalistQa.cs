#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Threading.Tasks;
using Herbalist.GameUI;
using Herbalist.Levels;
using Herbalist.Networking;
using Herbalist.StageOne;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.Development
{
    public static class HerbalistQa
    {
        public const string Main = "Assets/_Project/Scenes/01_Title/MainMenu.unity";
        public const string StageOne = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_ModelingReference_CharacterBranch_DevelopStage1_Reference.unity";
        public const string Maze = "Assets/_Project/Scenes/Stages/Stage_02/Stage_02_Mazes.unity";
        public const string Ride = "Assets/_Project/Scenes/Stages/Stage_03/Stage_03_EnvironmentModeling_FinalReference.unity";
        public const string Cinematic = "Assets/_Project/Scenes/Stages/Stage_01/Stage_01_to_02_Cinematic.unity";

        public static string SceneFor(string point, bool multiplayer)
        {
            if (multiplayer || point.StartsWith("menu/")) return Main;
            if (point is "s1/opening" or "s1/transition") return Cinematic;
            if (point.StartsWith("s2/")) return Maze;
            if (point.StartsWith("s3/")) return Ride;
            return StageOne;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void StartClient()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-herbalist-qa-client");
            if (i < 0 || args.Length <= i + 3) return;
            try
            {
                Application.runInBackground = true;
                var session = FusionLobbySession.Instance ?? throw new InvalidOperationException("QA 클라이언트는 타이틀 씬에서 시작해야 합니다.");
                await session.ConnectAsync(args[i + 1], false);
                CheckConnected(session);
                if (args[i + 3] == "select")
                {
                    int slot = int.Parse(args[i + 2]);
                    await WaitFor(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "2인 로비");
                    RoomControl.Instance.Select(slot);
                    await WaitFor(() => RoomControl.Instance.LocalChoice == slot, "클라이언트 캐릭터 선택");
                }
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Application.Quit(1);
            }
        }

#if UNITY_EDITOR
        public static async Task Run(string point, bool multiplayer, string roomName, int hostSlot,
            int sapSlot, Action<string, int, bool> startClient, Action<string> report)
        {
            Application.runInBackground = true;
            if (point == "menu/title") return;
            if (!multiplayer)
            {
                if (point.StartsWith("menu/")) throw new InvalidOperationException("이 지점은 2인 모드에서 사용할 수 있습니다.");
                if (point is "s1/opening" or "s1/transition")
                {
                    var view = UnityEngine.Object.FindAnyObjectByType<StageCinematicView>();
                    if (view == null) throw new InvalidOperationException("영상 재생기를 찾을 수 없습니다.");
                    view.Prepare(point == "s1/opening", sapSlot, view.Play, null, () =>
                        SceneManager.LoadSceneAsync(point == "s1/opening" ? StageOne : Maze));
                    return;
                }

                await WaitFor(() => StageActor.All.Any(a => a.Available), "오프라인 플레이어");
                Apply(point, sapSlot);
                return;
            }

            var session = FusionLobbySession.Instance ?? throw new InvalidOperationException("타이틀 씬에서 네트워크 세션을 찾을 수 없습니다.");
            report("Photon 방을 생성하는 중…");
            await session.ConnectAsync(roomName, true);
            CheckConnected(session);
            if (point == "menu/host") return;

            report("두 번째 플레이어 접속을 기다리는 중…");
            bool select = point != "menu/join";
            startClient(roomName, 1 - hostSlot, select);
            await WaitFor(() => session.ConnectedCount == 2 && session.State == LobbyState.Waiting, "2인 로비");
            if (!select) return;

            RoomControl.Instance.Select(hostSlot);
            await WaitFor(() => RoomControl.Instance.Ready, "두영·소담 선택");
            if (point == "menu/ready") return;

            report("테스트 지점의 씬을 불러오는 중…");
            string destination = point.StartsWith("s2/") ? Maze : point.StartsWith("s3/") ? Ride : StageOne;
            bool cinematic = point is "s1/opening" or "s1/transition";
            if (point == "s1/transition") destination = Maze;
            session.ConfigureQaStart(destination, sapSlot, cinematic);
            RoomControl.Instance.StartGame();
            if (cinematic)
            {
                await WaitFor(() => session.State == LobbyState.Error || session.QaCinematicPlaying, "영상 시작", 60);
                if (session.State == LobbyState.Error) throw new InvalidOperationException(session.Message);
                return;
            }
            await WaitFor(() => session.State == LobbyState.Error || session.State == LobbyState.Playing &&
                SceneManager.GetActiveScene().path == destination && NetworkPlayer.Local != null &&
                StageActor.All.Count(a => a.Available) == 2, "씬 진입 및 두 플레이어 생성", 240);
            if (session.State == LobbyState.Error) throw new InvalidOperationException(session.Message);
            Apply(point, sapSlot);
        }

        private static void Apply(string point, int sapSlot)
        {
            var parts = point.Split('/');
            switch (parts[0])
            {
                case "s1":
                    StageOneFlow.Instance.ApplyQaPoint(parts[1], sapSlot);
                    break;
                case "s2":
                    UnityEngine.Object.FindAnyObjectByType<DescendingMazeFlow>().ApplyQaPoint(parts[1], sapSlot);
                    break;
                case "s3":
                    UnityEngine.Object.FindAnyObjectByType<BranchRideFlow>().ApplyQaPoint(parts[1], sapSlot);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(point), point, "등록되지 않은 QA 지점");
            }
        }

#endif
        private static void CheckConnected(FusionLobbySession session)
        {
            if (session.State != LobbyState.Waiting)
                throw new InvalidOperationException(session.Message ?? "Photon에 접속하지 못했습니다.");
        }

        internal static async Task WaitFor(Func<bool> condition, string label, float timeout = 40)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + timeout;
            while (!condition())
            {
                if (!Application.isPlaying) throw new OperationCanceledException();
                if (Time.realtimeSinceStartupAsDouble >= deadline)
                    throw new TimeoutException(label + " 대기 시간이 초과되었습니다.");
                await Task.Delay(50);
            }
        }
    }
}
#endif
