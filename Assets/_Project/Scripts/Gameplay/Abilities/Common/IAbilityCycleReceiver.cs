using UnityEngine;
namespace Herbalist.Abilities
{
    public interface IAbilityCycleReceiver
    {
        bool ReceiveAbilityCycle(PlayerAbilityController player,uint steps,bool held,Ray aim);
    }

    public interface IAbilityPointerReceiver
    {
        void ReceiveAbilityPointer(PlayerAbilityController player, bool held, bool valid, Vector2 boardPoint);
    }
}
