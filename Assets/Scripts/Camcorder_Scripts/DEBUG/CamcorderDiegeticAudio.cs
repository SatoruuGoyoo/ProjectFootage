using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CamcorderDiegeticAudio : MonoBehaviour
{
    [Header("Proximity")]
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private AnimationCurve distanceCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Alignment")]
    [SerializeField, Range(0f, 1f)] private float minDotThreshold = 0.65f;

 
    [SerializeField, Range(0f, 1f)] private float fullVolumeDot = 0.9f;

    private AudioSource audioSource;

    private float baseVolume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        baseVolume = audioSource.volume;
    }

    public float SampleVolume(Vector3 camPosition, Vector3 camForward)
    {
        float distance = Vector3.Distance(camPosition, transform.position);
        if (distance >= maxDistance) return 0f;

        float normalizedDist = distance / maxDistance;
        float proxFactor = distanceCurve.Evaluate(normalizedDist);

        Vector3 toSource = (transform.position - camPosition).normalized;
        float dot = Vector3.Dot(camForward, toSource);

        if (dot < minDotThreshold) return 0f;

        float alignFactor = Mathf.InverseLerp(minDotThreshold, fullVolumeDot, dot);
        alignFactor = Mathf.Clamp01(alignFactor);

        return proxFactor * alignFactor;
    }

    public void ApplyPlaybackVolume(float sampledVolume)
    {
        audioSource.volume = baseVolume * sampledVolume;
    }

    public void ResetVolume()
    {
        audioSource.volume = baseVolume;
    }
}
