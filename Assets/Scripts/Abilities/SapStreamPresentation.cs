using UnityEngine;
namespace Herbalist.Abilities
{
    // Replace this presentation without changing extraction, collision or networking.
    [DefaultExecutionOrder(150)]
    public sealed class SapStreamPresentation : MonoBehaviour
    {
        [SerializeField] private SapDeposit sap;
        [SerializeField] private LineRenderer stream;
        [SerializeField] private Transform[] beads;
        private void LateUpdate()
        {
            bool visible = sap.HasStream && (sap.State == SapState.Extracting || sap.State == SapState.Controlled || sap.State == SapState.Flying);
            stream.enabled = visible;
            var settings = sap.Settings;
            if (visible && settings != null)
            {
                stream.widthMultiplier = settings.streamWidth;
                stream.SetPosition(0, sap.StreamStart);
                stream.SetPosition(1, sap.transform.position);
            }
            for (int i = 0; i < beads.Length; i++)
            {
                beads[i].gameObject.SetActive(visible);
                if (!visible || settings == null) continue;
                float length = Vector3.Distance(sap.StreamStart, sap.transform.position);
                float t = Mathf.Repeat(Time.time * settings.streamFlowSpeed / Mathf.Max(length, settings.radius) + (float)i / beads.Length, 1);
                beads[i].position = Vector3.Lerp(sap.StreamStart, sap.transform.position, t);
                beads[i].localScale = Vector3.one * settings.streamBeadSize;
            }
        }
    }
}
