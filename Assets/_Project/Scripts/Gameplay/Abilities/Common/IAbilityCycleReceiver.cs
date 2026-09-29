namespace Herbalist.Abilities
{
    public interface IAbilityCycleReceiver
    {
        bool ReceiveAbilityCycle(PlayerAbilityController player,uint steps);
    }
}
