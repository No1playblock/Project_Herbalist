using UnityEngine;
namespace Herbalist.GameUI
{
    public sealed class LoadingSpinner : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond=180;
        private void Update()=>transform.Rotate(0,0,-degreesPerSecond*Time.unscaledDeltaTime);
    }
}
