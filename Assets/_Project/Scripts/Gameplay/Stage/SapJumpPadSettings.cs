using UnityEngine;
namespace Herbalist.Levels
{
    [CreateAssetMenu(menuName = "Herbalist/Stage/Sap Jump Pad")]
    public sealed class SapJumpPadSettings : ScriptableObject
    {
        [Min(1)] public int requiredRiders = 2;
        [Min(.05f)] public float chargeUnits = 1.0f;
        [Min(.01f)] public float injectionGrace = .2f;
        [Min(.05f)] public float flightDuration = 1.4f;
        [Min(.1f)] public float gravity = 18f;
        [Min(.05f)] public float cooldown = 1.5f;
        [Min(.01f)] public float standingTolerance = .3f;
        [Min(.05f)] public float presentationDuration = .65f;
        [Min(0)] public float presentationRise = 1.2f;
    }
}
