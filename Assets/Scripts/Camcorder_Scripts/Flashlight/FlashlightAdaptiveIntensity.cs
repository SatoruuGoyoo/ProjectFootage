using UnityEngine;

public class FlashlightAdaptiveIntensity : MonoBehaviour
{
    public Light flashlight;
    public AnimationCurve intensityByDistance;
    public float maxRaycastDistance = 15f;
    public float smoothTime = 0.15f;
    public LayerMask raycastMask;

    float currentVelocity;

    void Update()
    {
        float distance = maxRaycastDistance;
        if (Physics.Raycast(transform.position, transform.forward, out var hit, maxRaycastDistance, raycastMask, QueryTriggerInteraction.Ignore))
            distance = hit.distance;

        float normalizedDistance = Mathf.Clamp01(distance / maxRaycastDistance);
        float target = intensityByDistance.Evaluate(normalizedDistance);

        flashlight.intensity = Mathf.SmoothDamp(flashlight.intensity, target, ref currentVelocity, smoothTime);
    }
}