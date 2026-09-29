using UnityEngine;
using Herbalist.Abilities;
namespace Herbalist.Levels
{
    public sealed class MazeLeakSocket : MonoBehaviour
    {
        [SerializeField] private LeafInstallTarget _target;
        [SerializeField] private SapBindingSource _binding;
        [SerializeField] private GameObject _stream;
        public LeafInstallTarget Target => _target;
        public bool Blocked => _target.InstalledLeaf!=null && _target.InstalledLeaf.Mode==LeafMode.Platform;
        public void AuthorityLeak(bool leaking) { if(leaking && !Blocked)_binding.Deposit(); }
        public void Present(bool leaking){if(_stream!=null)_stream.SetActive(leaking&&!Blocked);}
        public void ResetSocket()
        {
            var leaf=_target.InstalledLeaf;
            if(leaf!=null)leaf.Finish();
            _binding.ClearUnbound();
            Present(false);
        }
    }
}