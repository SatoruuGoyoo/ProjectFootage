using UnityEngine;

public interface ISpatialAudioSource
{
    // Posición actual del objeto en el mundo
    Vector3 WorldPosition { get; }

    // Path del evento FMOD (ej: "event:/Ambiente/Radio")
    string FMODPath { get; }

    // Si es 3D posicional o ambiente global
    bool Is3D { get; }

    // Radio máximo en metros — más allá de este, volumen = 0
    float MaxAudibleDistance { get; }

    // Si el objeto está activo en la escena al momento de grabar
    bool IsActiveInScene { get; }

    // Posición en el timeline FMOD al momento de grabar
    // Sirve para sincronizar eventos que ya estaban corriendo (ej: una radio a mitad de canción)
    bool TryGetTimelinePosition(out int milliseconds);

    // Curva de caída: recibe distancia normalizada (0=fuente, 1=radio máximo)
    // y devuelve volumen (0..1). Configurable por fuente en el inspector.
    float EvaluateDistanceFalloff(float normalizedDistance);
}