using UnityEngine;

public class FlowerSway : MonoBehaviour
{
    [SerializeField] private float swayAngle = 5f;
    [SerializeField] private float swaySpeed = 1.2f;
    [SerializeField] private float sideSwayAngle = 2f;

    private Quaternion startRotation;
    private float phaseA;
    private float phaseB;

    private void Awake()
    {
        startRotation = transform.localRotation;
        phaseA = Random.value * Mathf.PI * 2f;
        phaseB = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        float t = Time.time * swaySpeed;
        float tiltX = Mathf.Sin(t + phaseA) * swayAngle;
        float tiltZ = Mathf.Sin(t * 0.73f + phaseB) * sideSwayAngle;

        transform.localRotation =
            startRotation * Quaternion.Euler(tiltX, 0f, tiltZ);
    }
}