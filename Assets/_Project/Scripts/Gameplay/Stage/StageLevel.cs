using UnityEngine;
namespace Herbalist.Levels
{
    [DefaultExecutionOrder(-600)]
    public sealed class StageLevel : MonoBehaviour
    {
        public static StageLevel Instance { get; private set; }
        public string nextScenePath;
        [SerializeField] private string _offlineCinematicScenePath;
        public string OfflineCinematicScenePath => _offlineCinematicScenePath;
        public bool requireEarnedAbilities = true;
        public StageAbilityRules abilityRules;
        [SerializeField] private bool _playerCameraPostProcessing;
        public bool PlayerCameraPostProcessing => _playerCameraPostProcessing;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() { Instance=null; }
        private void Awake() { Instance=this; }
        private void OnDestroy() { if(Instance==this)Instance=null; }
    }
}
