using UnityEngine;

namespace Herbalist.Levels
{
    // Scene authoring notes only; no input, simulation, or runtime UI.
    public sealed class StageOneCoopRoute : MonoBehaviour
    {
        [SerializeField] private int _gimmickNumber;
        [SerializeField] private HangingCloth _cloth;
        [SerializeField, TextArea] private string _roleOrder;
        public void Configure(int number, HangingCloth cloth, string roleOrder)
        { _gimmickNumber = number; _cloth = cloth; _roleOrder = roleOrder; }
    }
}
