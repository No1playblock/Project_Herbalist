using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Herbalist.Development;
using Herbalist.Levels;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Herbalist.Editor
{
    [InitializeOnLoad]
    internal sealed class HerbalistQaWindow : EditorWindow
    {
        private const string SelectedLabel = "Herbalist.QA.SelectedLabel";
        private const string Pending = "Herbalist.QA.Pending";
        private const string TwoPlayers = "Herbalist.QA.Multiplayer";
        private const string HostSlot = "Herbalist.QA.HostSlot";
        private const string SapSlot = "Herbalist.QA.SapSlot";
        private const string Status = "Herbalist.QA.Status";
        private const string PreviousStart = "Herbalist.QA.PreviousStart";
        private const string Current = "Herbalist.QA.Current";
        private const string ClientPid = "Herbalist.QA.ClientPid";
        private static string ProjectDirectory => Path.GetDirectoryName(Application.dataPath);
        internal static string ClientPath => Path.Combine(ProjectDirectory, "Builds", "HerbalistQA", "Client", "HerbalistQA.exe");
        private Vector2 _scroll;
        private bool _multiplayer;
        private int _hostSlot, _sapSlot;
        private readonly bool[] _expanded = { true, true, true, true, true, true, true };

        static HerbalistQaWindow()
        {
            EditorApplication.playModeStateChanged += OnPlayState;
            EditorApplication.quitting += StopClient;
        }

        [MenuItem("Window/Herbalist/QA Tool")]
        private static void Open() => GetWindow<HerbalistQaWindow>("Herbalist QA");

        private void OnEnable()
        {
            _multiplayer = SessionState.GetBool(TwoPlayers, false);
            _hostSlot = SessionState.GetInt(HostSlot, 0);
            _sapSlot = SessionState.GetInt(SapSlot, 0);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Herbalist QA", EditorStyles.boldLabel);
            _multiplayer = EditorGUILayout.ToggleLeft("Photon 2인 테스트", _multiplayer);
            if (_multiplayer)
            {
                _hostSlot = EditorGUILayout.Popup("내 캐릭터 (Host)", _hostSlot, new[] { "두영", "소담" });
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
                    if (GUILayout.Button(File.Exists(ClientPath) ? "QA 클라이언트 다시 빌드" : "QA 클라이언트 빌드")) BuildClient();
                using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer))
                    if (GUILayout.Button("2인 테스트 시작")) StartPoint("s1/start", "1스테이지 첫 조작부터 2인 테스트");
            }
            _sapSlot = EditorGUILayout.Popup("진행 지점에서 수액을 맡을 캐릭터", _sapSlot, new[] { "두영", "소담" });
            string status = SessionState.GetString(Status, "");
            if (status.Length > 0) EditorGUILayout.HelpBox(status, MessageType.Info);
            if (EditorApplication.isPlaying && SessionState.GetString(Current, "").Length > 0 &&
                GUILayout.Button("QA 플레이 종료")) EditorApplication.ExitPlaymode();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer))
            {
                if (Group(0, "타이틀 · 로비 · 오프닝"))
                {
                    Point("menu/title", "타이틀 첫 화면");
                    using (new EditorGUI.DisabledScope(!_multiplayer))
                    {
                        Point("menu/host", "방 생성 직후");
                        Point("menu/join", "두 번째 유저 접속 직후");
                        Point("menu/ready", "두영·소담 선택 완료, 시작 직전");
                    }
                    Point("s1/opening", "오프닝 영상 시작");
                    Point("s1/start", "1스테이지 첫 조작");
                }

                if (Group(1, "1스테이지 · 채집과 제조"))
                {
                    One("leaf-herb", "잎 약초 채집 직전");
                    One("leaf-gather", "잎 약초 채집 미니게임");
                    One("sap-herb", "수액 약초 채집 직전");
                    One("sap-gather", "수액 약초 채집 미니게임");
                    One("transfer", "약초 전달 직전");
                    One("leaf-craft", "잎 포션 제조 시작");
                    One("sap-craft", "수액 포션 제조 시작");
                    One("drink", "첫 포션 복용 직전");
                    One("potions", "두 포션 복용 완료");
                }

                if (Group(2, "1스테이지 · 등반과 체크포인트"))
                {
                    One("forest-before", "숲 출구 진입 직전");
                    One("forest-saved", "숲 출구 체크포인트 등록 직후");
                    One("first-platform", "첫 발판 오르기 직전");
                    One("first-landed", "첫 발판 착지 직후");
                    foreach (var checkpoint in ReadCheckpoints())
                        One("save-" + checkpoint.index, "저장 직후 · " + checkpoint.label);
                }

                if (Group(3, "1스테이지 · 협동과 전환"))
                {
                    One("inlet", "첫 천을 내리는 수액 주입구");
                    One("cloth", "첫 천 잡기");
                    One("leaf-cross", "잎 가지 발판 건너기");
                    One("water-block", "잎으로 물줄기 막기");
                    One("cloth-lower", "수액으로 천 내리기");
                    One("grow", "수액으로 버섯 키우기");
                    One("launch", "잎 설치·수액 발사 점프대");
                    One("cloth-swap", "빨간 천 교체");
                    One("last-cloth", "마지막 천 건너기");
                    One("arrival-one", "정상 한 명 도착, 파트너 대기");
                    One("arrival-two", "두 번째 플레이어 도착 직전");
                    One("transition", "1 → 2 전환 영상");
                    Point("s2/arrival", "2스테이지 뗏목 착지");
                }

                if (Group(4, "2스테이지 · 미로"))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        EditorGUILayout.LabelField("미로 " + i, EditorStyles.miniBoldLabel);
                        Point($"s2/{i}-descent", "뗏목 하강 시작");
                        Point($"s2/{i}-start", "첫 조작");
                        Point($"s2/{i}-leak", "첫 누수 직전");
                        Point($"s2/{i}-refill", "수액 보충 직전");
                        Point($"s2/{i}-retry", "수액 소진 후 재시도");
                        Point($"s2/{i}-exit", "출구 도달 직전");
                    }
                    Point("s2/final", "미로 3 완료 직전 · 마지막 씬 전환");
                }

                if (Group(5, "3스테이지 · 부적과 가지"))
                {
                    Ride("start", "부적 탑승 전");
                    Ride("one", "한 명 탑승, 파트너 대기");
                    Ride("both", "두 명 탑승 후 출발");
                    Ride("leaf", "잎 조준·설치·회수 구간");
                    Ride("t1", "T1 분기 직전");
                    Ride("drop1", "첫 번째 낙하 직전");
                    Ride("t2", "T2 분기 직전");
                    Ride("t3", "T3 분기 직전");
                    Ride("stones", "돌 구간 진입");
                    Ride("retry", "돌 충돌 후 재시작");
                    Ride("t4", "T4 분기 직전");
                    Ride("drop2", "두 번째 낙하 직전");
                }

                if (Group(6, "착지 · 엔딩"))
                {
                    Ride("landing", "최종 지형 착지 직전");
                    Ride("landed", "착지 후 하차 직전");
                    Ride("moon", "달의 눈물 접근 직전");
                    Ride("ending", "엔딩 카메라 시작");
                    Ride("credits", "엔딩 크레딧 영상 시작");
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private bool Group(int index, string label) =>
            _expanded[index] = EditorGUILayout.Foldout(_expanded[index], label, true);

        private void One(string point, string label) => Point("s1/" + point, label);
        private void Ride(string point, string label) => Point("s3/" + point, label);

        private void Point(string point, string label)
        {
            if (GUILayout.Button(label, EditorStyles.miniButton)) StartPoint(point, label);
        }

        private void StartPoint(string point, string label)
        {
            if (_multiplayer && point != "menu/title" && point != "menu/host" && !File.Exists(ClientPath))
            {
                Report("Photon 2인 테스트를 시작하려면 QA 클라이언트를 먼저 빌드하세요.");
                return;
            }
            if (!EditorApplication.isPlaying && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SessionState.SetString(Pending, point);
            SessionState.SetBool(TwoPlayers, _multiplayer);
            SessionState.SetInt(HostSlot, _hostSlot);
            SessionState.SetInt(SapSlot, _sapSlot);
            SessionState.SetString(SelectedLabel, label);
            Report("준비 중 · " + label);
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
            else EnterPending();
        }

        private static void EnterPending()
        {
            string point = SessionState.GetString(Pending, "");
            if (point.Length == 0) return;
            string path = HerbalistQa.SceneFor(point, SessionState.GetBool(TwoPlayers, false));
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (scene == null)
            {
                SessionState.EraseString(Pending);
                Report("테스트 씬을 찾을 수 없습니다. " + path);
                return;
            }
            var previous = EditorSceneManager.playModeStartScene;
            SessionState.SetString(PreviousStart, previous != null ? AssetDatabase.GetAssetPath(previous) : "");
            EditorSceneManager.playModeStartScene = scene;
            EditorApplication.EnterPlaymode();
        }

        private static async void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                StopClient();
                SessionState.EraseString(Current);
                EditorApplication.delayCall += EnterPending;
                return;
            }
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                StopClient();
                return;
            }
            if (state != PlayModeStateChange.EnteredPlayMode) return;

            string point = SessionState.GetString(Pending, "");
            if (point.Length == 0) return;
            SessionState.EraseString(Pending);
            SessionState.SetString(Current, point);
            var previous = SessionState.GetString(PreviousStart, "");
            SessionState.EraseString(PreviousStart);
            EditorSceneManager.playModeStartScene = previous.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
            try
            {
                string room = "qa-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                await HerbalistQa.Run(point, SessionState.GetBool(TwoPlayers, false), room,
                    SessionState.GetInt(HostSlot, 0), SessionState.GetInt(SapSlot, 0), StartClient, Report);
                if (EditorApplication.isPlaying) Report("준비 완료 · " + SessionState.GetString(SelectedLabel, point));
            }
            catch (OperationCanceledException) when (!EditorApplication.isPlaying) { }
            catch (Exception error) { Report(error.Message); Debug.LogException(error); }
        }

        internal static bool BuildClient(BuildOptions extraOptions = BuildOptions.None)
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                // The shader build preprocessor traverses build scenes and reopens the active scene by path.
                if (string.IsNullOrEmpty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path))
                    EditorSceneManager.OpenScene(HerbalistQa.Main);
                Directory.CreateDirectory(Path.GetDirectoryName(ClientPath));
                var options = new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    target = BuildTarget.StandaloneWindows64,
                    locationPathName = ClientPath,
                    options = BuildOptions.Development | extraOptions
                };
                var result = BuildPipeline.BuildPlayer(options);
                bool succeeded = result != null && result.summary.result == BuildResult.Succeeded;
                Report(succeeded ? "QA 클라이언트 빌드 완료" :
                    "QA 클라이언트 빌드 실패 · " + (result == null ? "결과 없음" : result.summary.result.ToString()));
                return succeeded;
            }
            catch (Exception error) { Report(error.Message); Debug.LogException(error); return false; }
            finally
            {
                if (setup.Length > 0 && setup.All(scene => !string.IsNullOrEmpty(scene.path)))
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }

        internal static void StartClient(string room, int slot, bool select)
        {
            StopClient();
            string log = Path.Combine(Path.GetDirectoryName(ClientPath), "Client.log");
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = ClientPath,
                WorkingDirectory = Path.GetDirectoryName(ClientPath),
                Arguments = $"-herbalist-qa-client {room} {slot} {(select ? "select" : "join")} -force-d3d11 -screen-fullscreen 0 -screen-width 960 -screen-height 540 -logFile \"{log}\"",
                UseShellExecute = false
            }) ?? throw new InvalidOperationException("QA 클라이언트를 실행하지 못했습니다.");
            SessionState.SetInt(ClientPid, process.Id);
        }

        internal static void StopClient()
        {
            int pid = SessionState.GetInt(ClientPid, 0);
            SessionState.EraseInt(ClientPid);
            if (pid == 0) return;
            try
            {
                using var process = Process.GetProcessById(pid);
                if (!process.HasExited && process.MainModule?.FileName == ClientPath)
                {
                    if (!process.CloseMainWindow()) process.Kill();
                }
            }
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }
            catch (System.ComponentModel.Win32Exception error) { Debug.LogWarning(error.Message); }
        }

        private static void Report(string message)
        {
            SessionState.SetString(Status, message);
            foreach (var window in Resources.FindObjectsOfTypeAll<HerbalistQaWindow>()) window.Repaint();
        }

        private static (int index, string label)[] _checkpoints;
        private static DateTime _checkpointWriteTime;

        internal static (int index, string label)[] ReadCheckpoints()
        {
            string path = Path.Combine(ProjectDirectory, HerbalistQa.StageOne);
            DateTime time = File.GetLastWriteTimeUtc(path);
            if (_checkpoints != null && _checkpointWriteTime == time) return _checkpoints;
            _checkpointWriteTime = time;
            // Read authored checkpoint IDs from the scene without opening it or changing the current scene setup.
            if (!File.Exists(path)) return Array.Empty<(int, string)>();
            string scene = File.ReadAllText(path);
            var documents = System.Text.RegularExpressions.Regex.Split(scene, "(?m)^--- !u!");
            var byId = documents.Skip(1).ToDictionary(doc => doc.Substring(doc.IndexOf('&') + 1).Split(' ', '\r', '\n')[0]);
            string traversal = documents.FirstOrDefault(doc => doc.Contains("  _sharedCheckpoints:"));
            if (traversal == null) return Array.Empty<(int, string)>();
            string groups = traversal.Split(new[] { "  _sharedCheckpoints:" }, StringSplitOptions.None)[1]
                .Split(new[] { "\n  _bareTrees:" }, StringSplitOptions.None)[0];
            var matches = System.Text.RegularExpressions.Regex.Matches(groups, @"(?m)^  - surfaces:[\s\S]*?recoveryPoint: \{fileID: (-?\d+)\}");
            return _checkpoints = matches.Cast<System.Text.RegularExpressions.Match>().Select((match, i) =>
            {
                var files = System.Text.RegularExpressions.Regex.Matches(match.Value, @"(?m)^    - \{fileID: (-?\d+)\}");
                foreach (System.Text.RegularExpressions.Match file in files)
                {
                    string id = file.Groups[1].Value;
                    if (id == "0") continue;
                    if (!byId.TryGetValue(id, out var collider)) return (i, $"체크포인트 {i + 1}");
                    if (System.Text.RegularExpressions.Regex.IsMatch(collider, @"(?m)^  m_Enabled: 0\s*$")) continue;
                    var owner = System.Text.RegularExpressions.Regex.Match(collider, @"m_GameObject: \{fileID: (-?\d+)\}");
                    if (!byId.TryGetValue(owner.Groups[1].Value, out var gameObject)) return (i, $"체크포인트 {i + 1}");
                    if (System.Text.RegularExpressions.Regex.IsMatch(gameObject, @"(?m)^  m_IsActive: 0\s*$")) continue;
                    var name = System.Text.RegularExpressions.Regex.Match(gameObject, @"(?m)^  m_Name: ([^\r\n]+)");
                    return (i, $"{i + 1}. {name.Groups[1].Value}");
                }
                return (i, null);
            }).Where(item => item.Item2 != null).ToArray();
        }
    }
}
