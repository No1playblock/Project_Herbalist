using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Herbalist.GameUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Herbalist.Networking
{
    public sealed partial class FusionLobbySession
    {
        private const int CinematicProtocol = 0x5343494E;
        private const byte OpeningCinematic = 2;
        private enum CinematicMessage { SceneReady = 1, Begin, Prepared, Play, Hold, Finished, DestinationReady, Votes, ViewReady, Reveal, Revealed }

        private sealed class CinematicParticipant
        {
            public bool Prepared;
            public bool Finished;
            public bool Voted;
            public bool DestinationReady;
            public bool ViewReady;
            public bool Revealed;
            public int HoldSequence;
            public double? HoldingSince;
        }

        private sealed class CinematicRun
        {
            public int Id;
            public byte Clip;
            public string DestinationPath;
            public int Sequence;
            public bool Playing;
            public bool DestinationLoadStarted;
            public bool DestinationAcknowledged;
            public bool Revealing;
            public bool ArrivalReleased;
            public StageCinematicView View;
            public readonly Dictionary<PlayerRef, CinematicParticipant> Participants = new();
        }

        private CinematicRun _stageCinematic;
        private int _cinematicId;
#if UNITY_EDITOR
        internal bool QaCinematicPlaying => _stageCinematic is { Playing: true };
#endif
        internal bool HoldingCinematicArrival => _stageCinematic is { DestinationLoadStarted: true, ArrivalReleased: false };

        private int CinematicSceneIndex(string destination)
        {
            if (string.IsNullOrEmpty(settings.stageOneCinematicScenePath)) return -1;
            string source = SceneManager.GetActiveScene().path;
            bool opening = source == settings.mainScenePath && destination == settings.playScenePath;
            bool stageTwo = source == settings.playScenePath && destination == settings.stageOneCinematicDestinationPath;
            if (!opening && !stageTwo) return -1;
            int index = SceneUtility.GetBuildIndexByScenePath(settings.stageOneCinematicScenePath);
            if (index < 0) throw new InvalidOperationException("Stage cinematic is missing from Build Settings.");
            return index;
        }

        private async Task RunStageCinematic(NetworkRunner stageRunner, PlayerRef[] players, int clip, int sceneIndex, int destinationIndex)
        {
            var cinematic = new CinematicRun
            {
                Id = ++_cinematicId,
                Clip = (byte)clip,
                DestinationPath = SceneUtility.GetScenePathByBuildIndex(destinationIndex)
            };
            foreach (var player in players) cinematic.Participants.Add(player, new CinematicParticipant());
            _stageCinematic = cinematic;
            await stageRunner.LoadScene(SceneRef.FromIndex(sceneIndex), LoadSceneMode.Single);
            await WaitForCinematic(stageRunner, cinematic, () => cinematic.Playing, 30);

            double playbackDeadline = Time.realtimeSinceStartupAsDouble + cinematic.View.PlaybackLimitSeconds + 5;
            cinematic.DestinationLoadStarted = true;
            var load = stageRunner.LoadScene(SceneRef.FromIndex(destinationIndex), LoadSceneMode.Additive, setActiveOnLoad: true);
            await WaitForCinematic(stageRunner, cinematic, () =>
            {
                bool videoComplete = cinematic.Participants.Values.All(peer => peer.Finished || peer.Voted) ||
                    Time.realtimeSinceStartupAsDouble >= playbackDeadline;
                if (videoComplete) cinematic.View.ShowLoading();
                return videoComplete && cinematic.Participants.Values.All(peer => peer.DestinationReady);
            }, Math.Max(settings.cinematicLoadTimeout, cinematic.View.PlaybackLimitSeconds + 10));
            await load;

            var layout = FindSpawnLayout(SceneManager.GetSceneByPath(cinematic.DestinationPath));
            if (layout == null) throw new InvalidOperationException("Stage arrival spawn layout not found: " + _stageEntryId);
            layout.SpawnPlayers(stageRunner);
            await WaitForLocalCinematicCamera(stageRunner, cinematic);
            SendLocalCinematic(cinematic, CinematicMessage.ViewReady);
            await WaitForCinematic(stageRunner, cinematic, () => cinematic.Participants.Values.All(peer => peer.ViewReady),
                settings.cinematicLoadTimeout);

            cinematic.Revealing = true;
            BroadcastCinematic(cinematic, CinematicMessage.Reveal);
            await RevealCinematic(stageRunner, cinematic);
            await WaitForCinematic(stageRunner, cinematic, () => cinematic.Participants.Values.All(peer => peer.Revealed),
                settings.cinematicLoadTimeout);

            cinematic.ArrivalReleased = true;
            SetState(LobbyState.Playing, settings.playingMessage);
            await stageRunner.UnloadScene(SceneRef.FromIndex(sceneIndex));
        }

        private async Task WaitForCinematic(NetworkRunner source, CinematicRun cinematic, Func<bool> complete, double timeout)
        {
            double deadline = Time.realtimeSinceStartupAsDouble + timeout;
            while (runner == source && _stageCinematic == cinematic && !stopping)
            {
                if (source.IsServer && cinematic.Playing) UpdateCinematicHolds(cinematic);
                if (complete()) return;
                if (Time.realtimeSinceStartupAsDouble >= deadline) throw new TimeoutException("Stage cinematic synchronization timed out.");
                await Task.Yield();
            }
            throw new OperationCanceledException();
        }

        private async Task WaitForLocalCinematicCamera(NetworkRunner source, CinematicRun cinematic, double playbackAllowance = 0)
        {
            await WaitForCinematic(source, cinematic, () => NetworkPlayer.Local != null &&
                NetworkPlayer.Local.Player.View.Camera.isActiveAndEnabled, settings.cinematicLoadTimeout + playbackAllowance);
            // Let the newly enabled gameplay camera render a frame behind the cover.
            await Task.Yield();
        }

        private async Task RevealCinematic(NetworkRunner source, CinematicRun cinematic)
        {
            if (source != runner || _stageCinematic != cinematic || stopping) return;
            cinematic.View?.Hide();
            // Keep the arrival at its initial pose until each remaining peer can see the scene.
            await Task.Yield();
            SendLocalCinematic(cinematic, CinematicMessage.Revealed);
            if (!source.IsServer && source == runner && _stageCinematic == cinematic)
            {
                SetState(LobbyState.Playing, settings.playingMessage);
            }
        }

        private void OnStageCinematicSceneLoaded(NetworkRunner source)
        {
            if (source.IsServer) AttachStageCinematicView();
            else source.SendReliableDataToServer(
                ReliableKey.FromInts(CinematicProtocol, 0, (int)CinematicMessage.SceneReady, 0), new byte[] { 0 });
        }

        private void AttachStageCinematicView()
        {
            var cinematic = _stageCinematic;
            if (cinematic == null || cinematic.View != null) return;
            var view = FindAnyObjectByType<StageCinematicView>();
            if (view == null || view.gameObject.scene.path != settings.stageOneCinematicScenePath) return;
            cinematic.View = view;
            view.Prepare(cinematic.Clip == OpeningCinematic, cinematic.Clip,
                () => SendLocalCinematic(cinematic, CinematicMessage.Prepared),
                held => SendLocalCinematic(cinematic, CinematicMessage.Hold, held ? (byte)1 : (byte)0),
                () => SendLocalCinematic(cinematic, CinematicMessage.Finished));
        }

        private async void MarkCinematicDestinationLoaded(NetworkRunner source)
        {
            var cinematic = _stageCinematic;
            if (cinematic == null || cinematic.DestinationAcknowledged ||
                !SceneManager.GetSceneByPath(cinematic.DestinationPath).isLoaded) return;
            cinematic.DestinationAcknowledged = true;
            SendLocalCinematic(cinematic, CinematicMessage.DestinationReady);
            if (source.IsServer) return;
            try
            {
                // A client can preload the destination well before the host finishes playing the clip and spawns the actors.
                await WaitForLocalCinematicCamera(source, cinematic, cinematic.View.PlaybackLimitSeconds);
                SendLocalCinematic(cinematic, CinematicMessage.ViewReady);
            }
            catch (OperationCanceledException) { }
            catch (TimeoutException exception) { await FinishAsync(string.Format(settings.failureFormat, exception.Message)); }
        }

        private void SendLocalCinematic(CinematicRun cinematic, CinematicMessage message, byte value = 0)
        {
            if (_stageCinematic != cinematic || runner == null || !runner.IsRunning) return;
            int sequence = ++cinematic.Sequence;
            if (runner.IsServer) RecordCinematic(cinematic, runner.LocalPlayer, message, sequence, value);
            else runner.SendReliableDataToServer(ReliableKey.FromInts(CinematicProtocol, cinematic.Id, (int)message, sequence),
                new[] { value });
        }

        private void BroadcastCinematic(CinematicRun cinematic, CinematicMessage message, byte value = 0)
        {
            var key = ReliableKey.FromInts(CinematicProtocol, cinematic.Id, (int)message, ++cinematic.Sequence);
            foreach (var player in cinematic.Participants.Keys)
                if (player != runner.LocalPlayer) runner.SendReliableDataToPlayer(player, key, new[] { value });
        }

        private bool ReceiveStageCinematic(NetworkRunner source, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data)
        {
            key.GetInts(out int protocol, out int id, out int kind, out int sequence);
            if (protocol != CinematicProtocol) return false;
            if (source != runner || data.Length != 1) return true;

            var message = (CinematicMessage)kind;
            var cinematic = _stageCinematic;
            if (source.IsServer)
            {
                if (cinematic == null) return true;
                if (message == CinematicMessage.SceneReady && !cinematic.Playing &&
                    cinematic.Participants.ContainsKey(player) && player != source.LocalPlayer)
                {
                    source.SendReliableDataToPlayer(player,
                        ReliableKey.FromInts(CinematicProtocol, cinematic.Id, (int)CinematicMessage.Begin, ++cinematic.Sequence),
                        new[] { cinematic.Clip });
                }
                else if (cinematic.Id == id) RecordCinematic(cinematic, player, message, sequence, data[0]);
                return true;
            }

            if (message == CinematicMessage.Begin && id > 0 && data[0] <= OpeningCinematic && cinematic == null)
            {
                byte clip = data[0];
                _stageCinematic = new CinematicRun
                {
                    Id = id,
                    Clip = clip,
                    DestinationPath = clip == OpeningCinematic ? settings.playScenePath : settings.stageOneCinematicDestinationPath,
                    DestinationLoadStarted = true
                };
                AttachStageCinematicView();
                return true;
            }

            if (cinematic == null || cinematic.Id != id || cinematic.View == null) return true;
            if (message == CinematicMessage.Play && !cinematic.Playing)
            {
                cinematic.Playing = true;
                cinematic.View.Play();
            }
            else if (message == CinematicMessage.Votes)
            {
                cinematic.View.ShowVotes(data[0] & 15, data[0] >> 4);
            }
            else if (message == CinematicMessage.Reveal && !cinematic.Revealing)
            {
                cinematic.Revealing = true;
                _ = RevealCinematic(source, cinematic);
            }
            return true;
        }

        private void RecordCinematic(CinematicRun cinematic, PlayerRef player, CinematicMessage message, int sequence, byte value)
        {
            if (!cinematic.Participants.TryGetValue(player, out var peer)) return;
            switch (message)
            {
                case CinematicMessage.Prepared:
                    peer.Prepared = true;
                    TryPlayCinematic(cinematic);
                    break;
                case CinematicMessage.Hold when cinematic.Playing && !peer.Voted && value <= 1 && sequence > peer.HoldSequence:
                    peer.HoldSequence = sequence;
                    if (value == 1) peer.HoldingSince ??= Time.realtimeSinceStartupAsDouble;
                    else
                    {
                        UpdateCinematicHolds(cinematic);
                        peer.HoldingSince = null;
                    }
                    break;
                case CinematicMessage.Finished when cinematic.Playing:
                    peer.Finished = true;
                    break;
                case CinematicMessage.DestinationReady when cinematic.DestinationLoadStarted:
                    peer.DestinationReady = true;
                    break;
                case CinematicMessage.ViewReady when cinematic.DestinationLoadStarted:
                    peer.ViewReady = true;
                    break;
                case CinematicMessage.Revealed when cinematic.Revealing:
                    peer.Revealed = true;
                    break;
            }
        }

        private void TryPlayCinematic(CinematicRun cinematic)
        {
            if (cinematic.Playing || cinematic.View == null || cinematic.Participants.Values.Any(peer => !peer.Prepared)) return;
            cinematic.Playing = true;
            BroadcastCinematic(cinematic, CinematicMessage.Play);
            cinematic.View.Play();
            PublishCinematicVotes(cinematic);
        }

        private void UpdateCinematicHolds(CinematicRun cinematic)
        {
            double now = Time.realtimeSinceStartupAsDouble;
            bool changed = false;
            foreach (var peer in cinematic.Participants.Values)
            {
                if (peer.Voted || peer.HoldingSince is not double since || now - since < cinematic.View.SkipHoldSeconds) continue;
                peer.Voted = true;
                peer.HoldingSince = null;
                changed = true;
            }
            if (changed) PublishCinematicVotes(cinematic);
        }

        private void PublishCinematicVotes(CinematicRun cinematic)
        {
            int votes = cinematic.Participants.Values.Count(peer => peer.Voted);
            int total = cinematic.Participants.Count;
            cinematic.View?.ShowVotes(votes, total);
            BroadcastCinematic(cinematic, CinematicMessage.Votes, (byte)((total << 4) | votes));
        }

        private void OnCinematicSceneUnloaded(Scene scene)
        {
            if (scene.path != settings.stageOneCinematicScenePath || _stageCinematic?.Revealing != true) return;
            if (runner != null && !runner.IsServer) ClearStageCinematic();
        }

        private void ClearStageCinematic()
        {
            var cinematic = _stageCinematic;
            _stageCinematic = null;
            if (cinematic?.View != null) cinematic.View.Stop();
        }
    }
}
