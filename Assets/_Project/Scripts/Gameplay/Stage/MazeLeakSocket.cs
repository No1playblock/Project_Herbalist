using UnityEngine;
using Herbalist.Abilities;
namespace Herbalist.Levels
{
    public sealed class MazeLeakSocket : MonoBehaviour
    {
        [SerializeField] private LeafInstallTarget _target;
        [SerializeField] private GameObject _wetSurface;
        [SerializeField] private GameObject _stream;
        public LeafInstallTarget Target => _target;
        public bool Blocked => _target != null && _target.HasLeaf;
        public void Present(bool leaking)
        {
            bool visible = leaking && !Blocked;
            if (_stream != null) _stream.SetActive(visible);
            if (_wetSurface != null) _wetSurface.SetActive(visible);
        }
        public void ResetSocket()
        {
            var leaf=_target.InstalledLeaf;
            if(leaf!=null)leaf.Finish();
            Present(false);
        }
    }
}
