using Fusion;
using System.Linq;
using UnityEngine;
namespace Herbalist.Networking
{
    public sealed class PlayerSpawnLayout : MonoBehaviour
    {
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject offlinePlayer;
        private void Awake()
        {
            // Preserve direct offline testing without leaving an extra player in network sessions.
            offlinePlayer.SetActive(FusionLobbySession.Instance == null);
        }
        public void SpawnPlayers(NetworkRunner runner)
        {
            if (!runner.IsServer) return;
            var players = runner.ActivePlayers.OrderBy(p => p.RawEncoded).ToArray();
            if (spawnPoints.Length < players.Length) throw new System.InvalidOperationException("Not enough player spawn points.");
            for (int i = 0; i < players.Length; i++)
            {
                if (runner.TryGetPlayerObject(players[i], out _)) continue;
                int slot = i;
                var point = spawnPoints[i];
                var obj = runner.Spawn(playerPrefab, point.position, point.rotation, players[i],
                    (r, spawned) => spawned.GetComponent<NetworkPlayer>().Slot = slot);
                runner.SetPlayerObject(players[i], obj);
            }
        }
    }
}
