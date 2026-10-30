using UnityEngine;
using Herbalist.Abilities;
namespace Herbalist.Levels
{
    public sealed class MazeLeakSocket : MonoBehaviour
    {
        [SerializeField] private LeafInstallTarget _target;
        [SerializeField] private GameObject _wetSurface;
        [SerializeField] private GameObject _stream;
        [SerializeField] private GameObject _approachSplash;
        [SerializeField, Min(0f)] private float _approachDistance = 3f;
        public LeafInstallTarget Target => _target;
        public bool Blocked => _target != null && _target.HasLeaf;
        public void Present(bool leaking, float sapDistance = float.PositiveInfinity)
        {
            bool visible = leaking && !Blocked;
            if (_stream != null) _stream.SetActive(visible);
            if (_wetSurface != null) _wetSurface.SetActive(visible);
            if (_approachSplash != null)
                _approachSplash.SetActive(!Blocked && (leaking || sapDistance <= _approachDistance));
        }
        public void ResetSocket()
        {
            var leaf=_target.InstalledLeaf;
            if(leaf!=null)leaf.Finish();
            Present(false);
        }
    }
}
