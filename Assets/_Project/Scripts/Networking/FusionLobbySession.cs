using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.Networking
{
    public enum LobbyState { Idle, Connecting, Waiting, Loading, Playing, Leaving, Error }

    public sealed class FusionLobbySession : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private LobbySettings settings;
        [SerializeField] private NetworkRunner runnerPrefab;
        public static FusionLobbySession Instance { get; private set; }
        public LobbyState State { get; private set; }
        public string Message { get; private set; }
        public string RoomName { get; private set; }
        public bool HasNetworkSession => runner != null || connecting || started || loadRequested;
        public int ConnectedCount => runner != null && runner.IsRunning ? runner.ActivePlayers.Count() : 0;
        public event Action Changed;
        private NetworkRunner runner;
        public NetworkRunner Runner => runner;
        private bool connecting, stopping, loadRequested, started;
        private CancellationTokenSource cancellation;
        private struct StagePlayerState { public int Slot; public Herbalist.Abilities.PlayerAbilityKind Ability; }
        private readonly Dictionary<PlayerRef, StagePlayerState> stagePlayers = new();
        private bool stageTransition;
        public int ResolveStageSlot(PlayerRef player, int fallback) => stagePlayers.TryGetValue(player,out var state)?state.Slot: (Herbalist.GameUI.RoomControl.Instance != null && Herbalist.GameUI.RoomControl.Instance.Choice(player.RawEncoded) >= 0 ? Herbalist.GameUI.RoomControl.Instance.Choice(player.RawEncoded) : fallback);
        public bool RestoreStageAbility(PlayerRef player, Herbalist.Abilities.PlayerAbilityController ability)
        {
            if(runner==null || !runner.IsServer || !stagePlayers.TryGetValue(player,out var state))return false;
            return state.Ability==Herbalist.Abilities.PlayerAbilityKind.Leaf?ability.UnlockLeaf():ability.UnlockSap();
        }
        public async void AdvanceStage(string scenePath)
        {
            if(runner==null || !runner.IsServer || stageTransition || State!=LobbyState.Playing)return;
            int sceneIndex=SceneUtility.GetBuildIndexByScenePath(scenePath);
            if(sceneIndex<0){Debug.LogError("Next stage is missing from Build Settings: "+scenePath);return;}
            var current=runner.ActivePlayers.ToArray();
            if(current.Length!=settings.playerCount)return;
            var saved=new Dictionary<PlayerRef,StagePlayerState>();
            foreach(var id in current)
            {
                if(!runner.TryGetPlayerObject(id,out var obj))return;
                var player=obj.GetComponent<NetworkPlayer>();var ability=obj.GetComponent<Herbalist.Abilities.PlayerAbilityController>();
                if(player==null||ability==null||!ability.Unlocked)return;
                saved[id]=new StagePlayerState{Slot=player.Slot,Ability=ability.Kind};
            }
            stageTransition=true;stagePlayers.Clear();foreach(var pair in saved)stagePlayers.Add(pair.Key,pair.Value);
            SetState(LobbyState.Loading,settings.loadingMessage);
            try
            {
                foreach(var id in current)if(runner.TryGetPlayerObject(id,out var obj))runner.Despawn(obj);
                await runner.LoadScene(SceneRef.FromIndex(sceneIndex),LoadSceneMode.Single);
            }
            catch(Exception exception){await FinishAsync(string.Format(settings.failureFormat,exception.Message));}
            finally{stageTransition=false;}
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetState(LobbyState.Idle, settings.idleMessage);
        }

        public async Task ConnectAsync(string requestedName, bool host)
        {
            if (connecting || stopping || runner != null) return;
            string name = (requestedName ?? string.Empty).Trim().ToLowerInvariant();
            if (name.Length == 0 || name.Length > settings.roomNameLimit || name.Any(c => !((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_')))
            { SetState(LobbyState.Error, settings.invalidRoomMessage); return; }
            if (!Guid.TryParse(PhotonAppSettings.Global.AppSettings.AppIdFusion, out _))
            { SetState(LobbyState.Error, settings.missingAppIdMessage); return; }
            stagePlayers.Clear(); stageTransition = false;
            Herbalist.GameUI.RoomControl.Instance?.ResetRoom();
            connecting = true; loadRequested = false; started = false; RoomName = name;
            SetState(LobbyState.Connecting, settings.connectingMessage);
            cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(settings.connectTimeout));
            try
            {
                // Only the transport prefab is created at runtime. All UI is scene-authored.
                runner = Instantiate(runnerPrefab);
                DontDestroyOnLoad(runner.gameObject);
                runner.AddCallbacks(this);
                runner.ProvideInput = true;
                var app = PhotonAppSettings.Global.AppSettings.GetCopy();
                app.FixedRegion = settings.fixedRegion;
                app.AppVersion = settings.appVersion;
                var scenes = new NetworkSceneInfo();
                scenes.AddSceneRef(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(settings.mainScenePath)), LoadSceneMode.Single);
                var result = await runner.StartGame(new StartGameArgs
                {
                    GameMode = host ? GameMode.Host : GameMode.Client,
                    SessionName = name,
                    PlayerCount = settings.playerCount,
                    EnableClientSessionCreation = false,
                    IsOpen = true, IsVisible = false,
                    Scene = scenes,
                    SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
                    CustomPhotonAppSettings = app,
                    StartGameCancellationToken = cancellation.Token
                });
                if (!result.Ok) { await FinishAsync(string.Format(settings.failureFormat, result.ShutdownReason)); return; }
                started = true;
                RefreshWaiting();
                TryStart();
            }
            catch (Exception exception) { await FinishAsync(string.Format(settings.failureFormat, exception.Message)); }
            finally { connecting = false; cancellation?.Dispose(); cancellation = null; }
        }

        private void RefreshWaiting()
        {
            if (!started || loadRequested || State == LobbyState.Playing || stopping) return;
            SetState(LobbyState.Waiting, string.Format(settings.waitingFormat, ConnectedCount, settings.playerCount));
        }
        public void StartSelectedGame() => TryStart(true);
        private async void TryStart(bool requested = false)
        {
            if (!started || runner == null || !runner.IsServer || loadRequested || stopping || ConnectedCount < settings.playerCount) return;
            if (settings.manualCharacterSelection && (!requested || Herbalist.GameUI.RoomControl.Instance == null || !Herbalist.GameUI.RoomControl.Instance.Ready)) return;
            loadRequested = true;
            runner.SessionInfo.IsOpen = false;
            SetState(LobbyState.Loading, settings.loadingMessage);
            try
            {
                int sceneIndex = SceneUtility.GetBuildIndexByScenePath(settings.playScenePath);
                if (sceneIndex < 0) throw new InvalidOperationException("Play scene is missing from Build Settings.");
                await runner.LoadScene(SceneRef.FromIndex(sceneIndex), LoadSceneMode.Single);
            }
            catch (Exception exception) { await FinishAsync(string.Format(settings.failureFormat, exception.Message)); }
        }
        public Task LeaveAsync() => FinishAsync(null);
        public void ClearError() { if (runner == null && !connecting && !stopping) SetState(LobbyState.Idle, settings.idleMessage); }

        private async Task FinishAsync(string error)
        {
            if (stopping) return;
            stopping = true;
            SetState(LobbyState.Leaving, settings.leavingMessage);
            var oldRunner = runner;
            runner = null; started = false; stagePlayers.Clear();
            Herbalist.GameUI.RoomControl.Instance?.ResetRoom();
            try
            {
                if (oldRunner != null)
                {
                    oldRunner.RemoveCallbacks(this);
                    await oldRunner.Shutdown();
                    if (oldRunner != null) Destroy(oldRunner.gameObject);
                }
                if (SceneManager.GetActiveScene().path != settings.mainScenePath)
                {
                    var operation = SceneManager.LoadSceneAsync(settings.mainScenePath);
                    while (operation != null && !operation.isDone) await Task.Yield();
                }
            }
            catch (Exception exception) { error = string.Format(settings.failureFormat, exception.Message); }
            finally
            {
                stopping = false; loadRequested = false;
                SetState(error == null ? LobbyState.Idle : LobbyState.Error, error ?? settings.idleMessage);
            }
        }

        private void SetState(LobbyState state, string message)
        {
            State = state; Message = message; Changed?.Invoke();
            Debug.Log($"[Herbalist Lobby] {state}: {message}");
        }
        private void OnDestroy()
        {
            if (Instance != this) return;
            cancellation?.Cancel();
            if (runner != null) runner.RemoveCallbacks(this);
            Instance = null;
        }
        public void OnPlayerJoined(NetworkRunner r, PlayerRef player) { RefreshWaiting(); TryStart(); }
        public void OnPlayerLeft(NetworkRunner r, PlayerRef player) { if (r.IsServer && r.TryGetPlayerObject(player, out var obj)) r.Despawn(obj); if (!loadRequested && State != LobbyState.Playing) RefreshWaiting(); }
        public async void OnShutdown(NetworkRunner r, ShutdownReason reason)
        { if (!stopping && !connecting) await FinishAsync(string.Format(settings.failureFormat, reason)); }
        public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        { if (!loadRequested && r.ActivePlayers.Count() < settings.playerCount) request.Accept(); else request.Refuse(); }
        public void OnSceneLoadStart(NetworkRunner r) { if (loadRequested || started) SetState(LobbyState.Loading, settings.loadingMessage); }
        public void OnSceneLoadDone(NetworkRunner r)
        {
            if (SceneManager.GetActiveScene().path == settings.playScenePath || Herbalist.Levels.StageLevel.Instance != null)
            {
                var layout = FindFirstObjectByType<PlayerSpawnLayout>();
                if (layout != null) layout.SpawnPlayers(r);
                SetState(LobbyState.Playing, settings.playingMessage);
            }
            else if (started) RefreshWaiting();
        }
        public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason reason) { }
        public void OnConnectFailed(NetworkRunner r, NetAddress address, NetConnectFailedReason reason) { }
        public void OnConnectedToServer(NetworkRunner r) { }
        public void OnInput(NetworkRunner r, NetworkInput input) { if (NetworkPlayer.Local != null) input.Set(NetworkPlayer.Local.ReadLocalInput()); }
        public void OnInputMissing(NetworkRunner r, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner r, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner r, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner r, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { Herbalist.GameUI.RoomControl.Instance?.Receive(r, player, key, data); }
        public void OnReliableDataProgress(NetworkRunner r, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> sessions) { }
        public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
        public async void OnHostMigration(NetworkRunner r, HostMigrationToken token) { await FinishAsync(settings.hostLeftMessage); }
        public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr message) { }
    }
}
