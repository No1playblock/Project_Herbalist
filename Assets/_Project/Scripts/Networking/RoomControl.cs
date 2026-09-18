using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusion;
using Fusion.Sockets;
using Herbalist.Networking;
using UnityEngine;

namespace Herbalist.GameUI
{
    // Reliable control messages remain active while gameplay simulation is paused.
    public sealed class RoomControl : MonoBehaviour
    {
        [Serializable] private sealed class Packet
        {
            public string kind;
            public int choice = -1;
            public bool pause;
            public int[] players;
            public int[] choices;
            public int owner;
            public int revision;
        }
        private readonly Dictionary<int, int> selections = new();
        private int pauseOwner, revision, receivedRevision = -1, sequence;
        private bool requestedSync;
        private const int Protocol = 0x48554931;
        public static RoomControl Instance => FusionLobbySession.Instance != null ? FusionLobbySession.Instance.GetComponent<RoomControl>() : null;
        private FusionLobbySession Session => GetComponent<FusionLobbySession>();
        private NetworkRunner Runner => Session.Runner;
        public bool IsHost => Runner != null && Runner.IsRunning && Runner.IsServer;
        public bool Paused => pauseOwner != 0;
        public bool OwnsPause => Runner != null && pauseOwner == Runner.LocalPlayer.RawEncoded;
        public int LocalId => Runner != null ? Runner.LocalPlayer.RawEncoded : 0;
        public int LocalChoice => Choice(LocalId);
        public int Choice(int id) => selections.TryGetValue(id, out var value) ? value : -1;
        public int Occupant(int role) { foreach (var p in selections) if (p.Value == role) return p.Key; return 0; }
        public int[] Players => selections.Keys.OrderBy(x => x).ToArray();
        public bool Ready => selections.Count == 2 && selections.Values.Contains(0) && selections.Values.Contains(1);
        public void ResetRoom() { selections.Clear(); pauseOwner = 0; revision = 0; receivedRevision = -1; requestedSync = false; GameplayPause.SetOffline(false); }
        private void Update()
        {
            if (Runner == null || !Runner.IsRunning) return;
            if (IsHost) RefreshMembers();
            else if (!requestedSync && Session.State == LobbyState.Waiting) { requestedSync = true; Send(new Packet { kind = "sync" }); }
        }
        public void RefreshMembers()
        {
            if (!IsHost) return;
            var ids = Runner.ActivePlayers.Select(p => p.RawEncoded).ToArray();
            bool changed = false;
            foreach (int id in ids) if (!selections.ContainsKey(id)) { selections[id] = -1; changed = true; }
            foreach (int id in selections.Keys.ToArray()) if (!ids.Contains(id)) { selections.Remove(id); if (pauseOwner == id) pauseOwner = 0; changed = true; }
            if (changed) Broadcast();
        }
        public void Select(int role) => Send(new Packet { kind = "select", choice = role });
        public void RequestPause(bool value) => Send(new Packet { kind = "pause", pause = value });
        public void StartGame() { if (IsHost && Ready && Session.State == LobbyState.Waiting) Session.StartSelectedGame(); }
        private void Send(Packet packet)
        {
            if (Runner == null || !Runner.IsRunning) return;
            if (IsHost) Handle(Runner.LocalPlayer.RawEncoded, packet);
            else Runner.SendReliableDataToServer(Key(), Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet)));
        }
        private ReliableKey Key() => ReliableKey.FromInts(Protocol, ++sequence, 0, 0);
        public void Receive(NetworkRunner runner, PlayerRef sender, ReliableKey key, ReadOnlySpan<byte> bytes)
        {
            key.GetInts(out int protocol, out _, out _, out _);
            if (protocol != Protocol || bytes.Length > 4096) return;
            Packet p;
            try { p = JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(bytes)); } catch { return; }
            if (p == null) return;
            if (runner.IsServer) Handle(sender.RawEncoded, p);
            else if (p.kind == "state" && p.players != null && p.choices != null && p.players.Length == p.choices.Length && p.players.Length <= 2 && p.revision > receivedRevision)
            {
                receivedRevision = p.revision; selections.Clear();
                for (int i = 0; i < p.players.Length; i++) selections[p.players[i]] = p.choices[i];
                pauseOwner = p.owner;
            }
        }
        private void Handle(int sender, Packet p)
        {
            RefreshMembers();
            if (!selections.ContainsKey(sender)) return;
            if (p.kind == "select" && Session.State == LobbyState.Waiting && p.choice >= -1 && p.choice <= 1)
            {
                if (p.choice == -1 || !selections.Any(pair => pair.Key != sender && pair.Value == p.choice)) selections[sender] = p.choice;
            }
            else if (p.kind == "pause" && Session.State == LobbyState.Playing)
            {
                if (p.pause && pauseOwner == 0) pauseOwner = sender;
                else if (!p.pause && pauseOwner == sender) pauseOwner = 0;
            }
            else if (p.kind != "sync") return;
            Broadcast();
        }
        private void Broadcast()
        {
            var p = new Packet { kind = "state", players = Players, owner = pauseOwner, revision = ++revision };
            p.choices = p.players.Select(Choice).ToArray();
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(p));
            foreach (var id in Runner.ActivePlayers) if (id != Runner.LocalPlayer) Runner.SendReliableDataToPlayer(id, Key(), bytes);
        }
    }
}
