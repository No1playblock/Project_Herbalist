using UnityEngine;
namespace Herbalist.Audio
{
    public enum SfxSurfaceKind { Dirt, Grass, Wood, Bark }
    public sealed class SfxSurface : MonoBehaviour
    {
        [SerializeField] private SfxSurfaceKind _kind;
        public SfxSurfaceKind Kind => _kind;
    }
}
