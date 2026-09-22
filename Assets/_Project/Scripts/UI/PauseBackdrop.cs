using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace Herbalist.GameUI
{
    public sealed class PauseBackdrop : MonoBehaviour
    {
        public RawImage image;
        private Texture2D frame;
        private bool previous;
        private void Update()
        {
            bool paused=GameplayPause.IsPaused;
            if(paused&&!previous)StartCoroutine(Capture());
            if(!paused&&previous) { image.gameObject.SetActive(false); if(frame!=null)Destroy(frame); frame=null; }
            previous=paused;
        }
        private IEnumerator Capture()
        {
            image.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            if(!GameplayPause.IsPaused)yield break;
            frame=ScreenCapture.CaptureScreenshotAsTexture();
            image.texture=frame;image.gameObject.SetActive(true);
        }
        private void OnDestroy(){ if(frame!=null)Destroy(frame); }
    }
}
