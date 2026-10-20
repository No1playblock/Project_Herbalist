using UnityEngine;
namespace Herbalist.Abilities
{
    public interface IAbilityCycleReceiver
    {
        bool ReceiveAbilityCycle(PlayerAbilityController player,uint steps,bool held,Ray aim);
    }
}
