using UnityEngine;
using Herbalist.StageOne;
using Herbalist.Networking;
using Herbalist.Abilities;
using Herbalist.GameUI;
using Herbalist.Presentation;
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
            var clip = StageCinematicClip.StageTwoExit;
            foreach (var actor in StageActor.All)
            {
                if (!actor.Local) continue;
                _offlineUnlocked = actor.Abilities.Unlocked;
                _offlineKind = actor.Abilities.Kind;
                _offlineBothUnlocked = actor.Abilities.OfflineBothUnlocked;
                if (StageOneFlow.Instance is { } stageOne)
                {
                    bool sodamHasSap = (stageOne.RoleFor(actor) == CharacterRole.Sodam) ==
                        (_offlineKind == PlayerAbilityKind.Sap);
                    clip = sodamHasSap ? StageCinematicClip.SodamSap : StageCinematicClip.DuyeongSap;
                }
                break;
            }
            _loading = true;
            string destination = level.nextScenePath;
            string cinematic = level.OfflineCinematicScenePath;
            if (!TryPlayOfflineCinematic(cinematic, destination, clip))
                SceneManager.LoadSceneAsync(destination);
            return true;
        }

        internal static bool TryPlayOfflineCinematic(string cinematic, string destination, StageCinematicClip clip)
        {
            if (string.IsNullOrEmpty(cinematic) || SceneUtility.GetBuildIndexByScenePath(cinematic) < 0 ||
                SceneUtility.GetBuildIndexByScenePath(destination) < 0)
                return false;
            SceneManager.LoadSceneAsync(cinematic).completed += _ => PlayOfflineCinematic(destination, clip);
            return true;
        }

        private static void PlayOfflineCinematic(string destination, StageCinematicClip clip)
        {
            var view = Object.FindAnyObjectByType<StageCinematicView>();
            if (view == null)
            {
                SceneManager.LoadSceneAsync(destination);
                return;
            }

            AsyncOperation preload = null;
            view.Prepare(clip, () =>
            {
                // Keep the cinematic scene alive while Unity reads the next scene.
                preload = SceneManager.LoadSceneAsync(destination, LoadSceneMode.Single);
                preload.allowSceneActivation = false;
                view.Play();
            }, null, () => preload.allowSceneActivation = true);
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
