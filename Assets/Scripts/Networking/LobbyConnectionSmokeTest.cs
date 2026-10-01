#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;
namespace Herbalist.Networking
{
    // Development builds only: exercises the real lobby in a separate process.
    public static class LobbyConnectionSmokeTest
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Run()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-herbalist-test-join");
            if (index < 0 || index + 1 >= args.Length) return;
            Application.runInBackground = true;
            var session = FusionLobbySession.Instance;
            if (session != null) await session.ConnectAsync(args[index + 1], false);
        }
    }
}
#endif
