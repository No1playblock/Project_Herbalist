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
        private enum CinematicMessage { SceneReady = 1, Begin, Prepared, Play, Hold, Finished }

        private sealed class CinematicParticipant
        {
            public bool Prepared;
            public bool Finished;
            public int HoldSequence;
            public double? HoldingSince;
        }

        private sealed class CinematicRun
        {
            public int Id;
            public int SapSlot;
            public int Sequence;
            public bool Playing;
            public StageCinematicView View;
            public readonly Dictionary<PlayerRef, CinematicParticipant> Participants = new();
            public readonly TaskCompletionSource<bool> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private CinematicRun _stageCinematic;
        private int _cinematicId;

        private int CinematicSceneIndex(string destination)
        {
            if (string.IsNullOrEmpty(settings.stageOneCinematicScenePath) ||
                SceneManager.GetActiveScene().path != settings.playScenePath ||
                destination != settings.stageOneCinematicDestinationPath) return -1;

            int index = SceneUtility.GetBuildIndexByScenePath(settings.stageOneCinematicScenePath);
            if (index < 0) Debug.LogError("Stage 1 cinematic is missing from Build Settings.");
            return index;
        }

        private async Task RunStageCinematic(NetworkRunner stageRunner, PlayerRef[] players, int sapSlot, int sceneIndex)
        {
            var cinematic = new CinematicRun { Id = ++_cinematicId, SapSlot = sapSlot };
            foreach (var player in players) cinematic.Participants.Add(player, new CinematicParticipant());
            _stageCinematic = cinematic;
            await stageRunner.LoadScene(SceneRef.FromIndex(sceneIndex), LoadSceneMode.Single);

            double deadline = Time.realtimeSinceStartupAsDouble + 30;
            bool playbackDeadlineSet = false;
            while (!cinematic.Completion.Task.IsCompleted)
            {
                if (runner != stageRunner || stopping) throw new OperationCanceledException();
                double now = Time.realtimeSinceStartupAsDouble;
                if (cinematic.Playing && !playbackDeadlineSet)
                {
                    deadline = now + cinematic.View.PlaybackLimitSeconds + 5;
                    playbackDeadlineSet = true;
                }
                if (cinematic.Playing && cinematic.Participants.Values.All(peer => peer.HoldingSince is double since &&
                    now - since >= cinematic.View.SkipHoldSeconds))
                    cinematic.Completion.TrySetResult(true);
                if (now >= deadline) throw new TimeoutException("The cinematic did not synchronize with both players.");
                await Task.Yield();
            }
            await cinematic.Completion.Task;
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
            if (cinematic == null || cinematic.View != null ||
                SceneManager.GetActiveScene().path != settings.stageOneCinematicScenePath) return;

            var view = FindAnyObjectByType<StageCinematicView>();
            if (view == null) return;
            cinematic.View = view;
            view.Prepare(cinematic.SapSlot,
                () => SendLocalCinematic(cinematic, CinematicMessage.Prepared),
                held => SendLocalCinematic(cinematic, CinematicMessage.Hold, held ? (byte)1 : (byte)0),
                () => SendLocalCinematic(cinematic, CinematicMessage.Finished));
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
            if (source.IsServer)
            {
                var cinematic = _stageCinematic;
                if (cinematic == null) return true;
                if (message == CinematicMessage.SceneReady && !cinematic.Playing &&
                    cinematic.Participants.ContainsKey(player) && player != source.LocalPlayer)
                {
                    source.SendReliableDataToPlayer(player,
                        ReliableKey.FromInts(CinematicProtocol, cinematic.Id, (int)CinematicMessage.Begin, ++cinematic.Sequence),
                        new[] { (byte)cinematic.SapSlot });
                }
                else if (cinematic.Id == id) RecordCinematic(cinematic, player, message, sequence, data[0]);
                return true;
            }

            if (message == CinematicMessage.Begin && id > 0 && data[0] <= 1 && _stageCinematic == null &&
                SceneManager.GetActiveScene().path == settings.stageOneCinematicScenePath)
            {
                _stageCinematic = new CinematicRun { Id = id, SapSlot = data[0] };
                AttachStageCinematicView();
            }
            else if (message == CinematicMessage.Play && _stageCinematic is { } cinematic &&
                cinematic.Id == id && !cinematic.Playing && cinematic.View != null)
            {
                cinematic.Playing = true;
                cinematic.View.Play();
            }
            return true;
        }

        private void RecordCinematic(CinematicRun cinematic, PlayerRef player, CinematicMessage message, int sequence, byte value)
        {
            if (cinematic.Completion.Task.IsCompleted || !cinematic.Participants.TryGetValue(player, out var peer)) return;
            switch (message)
            {
                case CinematicMessage.Prepared when !cinematic.Playing:
                    peer.Prepared = true;
                    if (cinematic.Participants.Values.All(participant => participant.Prepared))
                    {
                        cinematic.Playing = true;
                        BroadcastCinematic(cinematic, CinematicMessage.Play);
                        cinematic.View.Play();
                    }
                    break;
                case CinematicMessage.Hold when cinematic.Playing && value <= 1 && sequence > peer.HoldSequence:
                    peer.HoldSequence = sequence;
                    peer.HoldingSince = value == 0 ? null : peer.HoldingSince ?? Time.realtimeSinceStartupAsDouble;
                    break;
                case CinematicMessage.Finished when cinematic.Playing:
                    peer.Finished = true;
                    if (cinematic.Participants.Values.All(participant => participant.Finished))
                        cinematic.Completion.TrySetResult(false);
                    break;
            }
        }

        private void ClearStageCinematic()
        {
            var cinematic = _stageCinematic;
            _stageCinematic = null;
            if (cinematic == null) return;
            if (cinematic.View != null) cinematic.View.Stop();
            cinematic.Completion.TrySetCanceled();
        }
    }
}
