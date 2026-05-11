using UnityEngine;

public static class SpatialAudioEvaluator
{

    public static float ComputeVolume(
        Vector3 cameraPosition,
        Quaternion cameraRotation,
        ISpatialAudioSource source,
        float angleInfluence = 0.6f)
    {
        float dist = Vector3.Distance(cameraPosition, source.WorldPosition);
        float maxDist = source.MaxAudibleDistance;

        if (maxDist > 0f && dist >= maxDist) return 0f;

        float normalizedDist = maxDist > 0f ? dist / maxDist : 0f;
        float distFactor = source.EvaluateDistanceFalloff(normalizedDist);

        Vector3 toSource = (source.WorldPosition - cameraPosition).normalized;
        Vector3 camForward = cameraRotation * Vector3.forward;
        float dot = Vector3.Dot(camForward, toSource);
        float angleFactor = Mathf.Clamp01((dot + 1f) * 0.5f);

        float finalAngleFactor = Mathf.Lerp(1f, angleFactor, angleInfluence);
        return distFactor * finalAngleFactor;
    }

    public static float ComputeVolume(
        Vector3 cameraPosition,
        Quaternion cameraRotation,
        Vector3 sourcePosition,
        float maxDistance,
        float angleInfluence = 0.6f)
    {
        float dist = Vector3.Distance(cameraPosition, sourcePosition);
        if (maxDistance > 0f && dist >= maxDistance) return 0f;

        float distFactor = maxDistance > 0f ? 1f - (dist / maxDistance) : 1f;

        Vector3 toSource = (sourcePosition - cameraPosition).normalized;
        Vector3 camForward = cameraRotation * Vector3.forward;
        float dot = Vector3.Dot(camForward, toSource);
        float angleFactor = Mathf.Clamp01((dot + 1f) * 0.5f);

        float finalAngleFactor = Mathf.Lerp(1f, angleFactor, angleInfluence);
        return distFactor * finalAngleFactor;
    }
}