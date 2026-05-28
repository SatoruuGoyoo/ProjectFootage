/// <summary>
/// Abstracción para mover y rotar al jugador a una posición en el mundo.
/// CorridorTeleporter depende de esta interfaz, no de tipos concretos.
/// </summary>
public interface IPlayerMover
{
    void MoveTo(UnityEngine.Vector3 worldPosition, UnityEngine.Quaternion worldRotation);
}
