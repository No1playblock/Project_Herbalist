using UnityEngine;
namespace Herbalist.Player
{
    public interface IPlayerMovementReceiver
    {
        void ReceiveMovement(PlayerController player, Vector2 movement);
    }
}