using UnityEngine;
namespace Herbalist.GameUI
{
    public static class GameplayPause
    {
        private static bool offline;
        public static bool IsPaused => RoomControl.Instance != null && Herbalist.Networking.FusionLobbySession.Instance.HasNetworkSession ? RoomControl.Instance.Paused : offline;
        public static void SetOffline(bool value) => offline = value;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() => offline = false;
    }
}
