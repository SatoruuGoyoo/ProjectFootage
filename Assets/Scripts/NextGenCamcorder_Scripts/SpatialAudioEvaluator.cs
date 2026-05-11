using UnityEngine;

public static class SpatialAudioEvaluator
{
    /// <summary>
    /// Calcula qué tan fuerte debería sonar una fuente para la cámara en este momento.
    /// </summary>
    /// <param name="cameraPosition">Posición de la cámara en el mundo</param>
    /// <param name="cameraRotation">Rotación de la cámara en el mundo</param>
    /// <param name="source">La fuente de audio a evaluar</param>
    /// <param name="angleInfluence">
    /// 0 = solo importa la distancia (fuente omnidireccional)
    /// 1 = distancia × ángulo completo (micrófono muy direccional)
    /// 0.6 es un buen default — similar a un micrófono cardioide
    /// </param>
    public static float ComputeVolume(
        Vector3 cameraPosition,
        Quaternion cameraRotation,
        ISpatialAudioSource source,
        float angleInfluence = 0.6f)
    {
        float dist = Vector3.Distance(cameraPosition, source.WorldPosition);
        float maxDist = source.MaxAudibleDistance;

        // Fuera del radio → silencio total
        if (maxDist > 0f && dist >= maxDist) return 0f;

        // distFactor: la fuente evalúa su propia curva de caída
        // normalizedDist: 0 = estás en la fuente, 1 = estás en el borde del radio
        float normalizedDist = maxDist > 0f ? dist / maxDist : 0f;
        float distFactor = source.EvaluateDistanceFalloff(normalizedDist);

        // angleFactor: producto punto entre forward de cámara y dirección a la fuente
        // Resultado: 1.0 si está justo enfrente, 0.5 si está a 90°, 0.0 si está atrás
        Vector3 toSource = (source.WorldPosition - cameraPosition).normalized;
        Vector3 camForward = cameraRotation * Vector3.forward;
        float dot = Vector3.Dot(camForward, toSource);
        float angleFactor = Mathf.Clamp01((dot + 1f) * 0.5f);

        // angleInfluence mezcla entre "solo distancia" y "distancia × ángulo"
        float finalAngleFactor = Mathf.Lerp(1f, angleFactor, angleInfluence);

        return distFactor * finalAngleFactor;
    }
}