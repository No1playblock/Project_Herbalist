using UnityEngine;
using Herbalist.StageOne;
using Herbalist.Networking;
using Herbalist.Abilities;
using Herbalist.GameUI;
using UnityEngine.SceneManagement;
namespace Herbalist.Levels
{
    public static class StageTransition
    {
        private static bool _offlineUnlocked;
        private static PlayerAbilityKind _offlineKind;
        private static bool _offlineBothUnlocked;
        private static bool _loading;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { _offlineUnlocked = _offlineBothUnlocked = _loading = false; }
        public static bool Advance(StageLevel level, string entry = null)
        {
            if (level == null || string.IsNullOrEmpty(level.nextScenePath)) return false;
            var session = FusionLobbySession.Instance;
            if (session != null && session.HasNetworkSession)
            {
                if (session.Runner == null || !session.Runner.IsServer) return false;
                session.AdvanceStage(level.nextScenePath, entry);
                return session.State == LobbyState.Loading;
            }
            if (_loading) return false;
            foreach (var actor in StageActor.All) if (actor.Local) { _offlineUnlocked = actor.Abilities.Unlocked; _offlineKind = actor.Abilities.Kind; _offlineBothUnlocked = actor.Abilities.OfflineBothUnlocked; break; }
            _loading = true;
            string destination = level.nextScenePath;
            string cinematic = level.OfflineCinematicScenePath;
            if (string.IsNullOrEmpty(cinematic) || SceneUtility.GetBuildIndexByScenePath(cinematic) < 0)
                SceneManager.LoadSceneAsync(destination);
            else
                SceneManager.LoadSceneAsync(cinematic).completed += _ => PlayOfflineCinematic(destination);
            return true;
        }

        private static void PlayOfflineCinematic(string destination)
        {
            var view = Object.FindAnyObjectByType<StageCinematicView>();
            if (view == null)
            {
                SceneManager.LoadSceneAsync(destination);
                return;
            }

            view.Prepare(StageCinematicClip.StageTwoExit, view.Play, null,
                () => SceneManager.LoadSceneAsync(destination));
        }
        public static void RestoreOffline(PlayerAbilityController ability)
        {
            if (!_loading) return;
            _loading = false;
            if (!_offlineUnlocked) return;
            if (_offlineKind == PlayerAbilityKind.Sap) ability.UnlockSap(); else ability.UnlockLeaf();
            if (_offlineBothUnlocked) ability.TryUnlockBothOfflineAbilities();
            _offlineBothUnlocked = false;
            _offlineUnlocked = false;
        }
    }
}
