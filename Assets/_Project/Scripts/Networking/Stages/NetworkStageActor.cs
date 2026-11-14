using Fusion;
using UnityEngine;
namespace Herbalist.StageOne
{
    [RequireComponent(typeof(StageActor))]
    public sealed class NetworkStageActor : NetworkBehaviour
    {
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_Command(StageCommand command, StageInteraction interaction)
        {
            if (command < StageCommand.Interact || command > StageCommand.Recover) return;
            string result = GetComponent<StageActor>().ExecuteCommand(command, interaction);
            RPC_Feedback(result);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void RPC_Feedback(string message) { GetComponent<StageActor>().SetFeedback(message); }
    }
}
