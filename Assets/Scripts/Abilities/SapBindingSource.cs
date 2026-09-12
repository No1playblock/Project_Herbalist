using UnityEngine;
namespace Herbalist.Abilities
{
    // Connection point for the future sap ability. Only its authority calls Deposit.
    public sealed class SapBindingSource : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float lifetime = 5;
        [SerializeField] private GameObject visual;
        private float expires;
        private bool available;
        private LeafProjectile boundLeaf;
        public bool Available => available && (boundLeaf != null || Time.time < expires);
        public void Deposit()
        {
            if (boundLeaf != null) return;
            available = true; expires = Time.time + lifetime;
            if (visual != null) visual.SetActive(true);
        }
        public bool TryBind(LeafProjectile leaf)
        {
            if (!Available || boundLeaf != null) return false;
            ShowBound(leaf); return true;
        }
        public void ShowBound(LeafProjectile leaf)
        {
            boundLeaf = leaf; available = true;
            if (visual != null) visual.SetActive(true);
        }
        public void Release(LeafProjectile leaf)
        {
            if (boundLeaf != leaf) return;
            boundLeaf = null; available = false;
            if (visual != null) visual.SetActive(false);
        }
        private void Update()
        {
            if (available && boundLeaf == null && Time.time >= expires)
            { available = false; if (visual != null) visual.SetActive(false); }
        }
    }
}
