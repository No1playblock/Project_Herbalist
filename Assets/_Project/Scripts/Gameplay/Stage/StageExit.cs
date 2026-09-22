using UnityEngine;
using Herbalist.StageOne;
using Herbalist.Networking;
namespace Herbalist.Levels
{
    [RequireComponent(typeof(StageOneFlow))]
    public sealed class StageExit : MonoBehaviour
    {
        [SerializeField] private StageLevel level;
        private StageOneFlow flow;
        private void Awake() { flow=GetComponent<StageOneFlow>(); }
        private void OnEnable() { flow.onCleared.AddListener(Advance); }
        private void OnDisable() { flow.onCleared.RemoveListener(Advance); }
        private void Advance()
        {
            if(!flow.Authority || level==null || FusionLobbySession.Instance==null) return;
            FusionLobbySession.Instance.AdvanceStage(level.nextScenePath);
        }
    }
}
