using UnityEngine;

namespace Herbalist.Networking
{
    [CreateAssetMenu(menuName = "Herbalist/Lobby Settings")]
    public sealed class LobbySettings : ScriptableObject
    {
        public bool manualCharacterSelection;
        [Min(2)] public int playerCount = 2;
        [Min(1)] public int roomNameLimit = 32;
        [Min(1)] public float connectTimeout = 30;
        public string mainScenePath;
        public string playScenePath;
        [SerializeField, Tooltip("Grant configured prototype abilities only when starting a stage directly from the lobby.")]
        private bool _grantPrototypeAbilitiesOnDirectStart;
        public bool GrantPrototypeAbilitiesOnDirectStart => _grantPrototypeAbilitiesOnDirectStart;
        public string fixedRegion = "asia";
        public string appVersion = "herbalist-lobby-test-1";
        public string idleMessage = "Create a room or join your partner's room.";
        public string connectingMessage = "Connecting to Photon...";
        public string waitingFormat = "{0} / {1} connected — waiting for your partner";
        public string loadingMessage = "Both players are here. Starting...";
        public string leavingMessage = "Leaving room...";
        public string invalidRoomMessage = "Enter a room name using letters, numbers, - or _.";
        public string missingAppIdMessage = "Set a valid Fusion 2 App ID in PhotonAppSettings before connecting.";
        public string playingMessage = "Connected to play scene";
        public string hostLeftMessage = "Host disconnected. Please create a new room.";
        public string failureFormat = "Connection failed: {0}";
    }
}
