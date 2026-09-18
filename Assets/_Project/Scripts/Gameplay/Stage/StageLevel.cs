using UnityEngine;
namespace Herbalist.Levels
{
    [DefaultExecutionOrder(-600)]
    public sealed class StageLevel : MonoBehaviour
    {
        public static StageLevel Instance { get; private set; }
        public string nextScenePath;
        public bool requireEarnedAbilities = true;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics() { Instance=null; }
        private void Awake() { Instance=this; }
        private void OnDestroy() { if(Instance==this)Instance=null; }
    }
}
