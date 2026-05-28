using System;
using UnityEngine;

/// <summary>
/// Agregá este componente al GameObject raíz del Player.
/// No modifica ningún script existente.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerMover : MonoBehaviour, IPlayerMover
{
    private Action<Vector3, Quaternion> _moveStrategy;
    private CharacterController _cc;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        _moveStrategy = _cc != null
            ? MoveWithCharacterController
            : MoveWithTransform;
    }

    public void MoveTo(Vector3 worldPosition, Quaternion worldRotation)
        => _moveStrategy(worldPosition, worldRotation);

    private void MoveWithCharacterController(Vector3 target, Quaternion rotation)
    {
        _cc.enabled = false;
        transform.position = target;
        transform.rotation = FlattenY(rotation); // solo giro en Y, igual que el motor Tank
        _cc.enabled = true;
    }

    private void MoveWithTransform(Vector3 target, Quaternion rotation)
    {
        transform.position = target;
        transform.rotation = FlattenY(rotation);
    }

    /// <summary>
    /// Extrae solo el eje Y de la rotación para que el player
    /// no quede inclinado al teleportar.
    /// </summary>
    private static Quaternion FlattenY(Quaternion q)
    {
        Vector3 euler = q.eulerAngles;
        return Quaternion.Euler(0f, euler.y, 0f);
    }
}
