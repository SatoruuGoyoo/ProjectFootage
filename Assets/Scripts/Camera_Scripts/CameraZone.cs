using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [Tooltip("Cámara que se activa al entrar en esta zona")]
    public Camera zoneCamera;

    [Tooltip("Mayor prioridad gana si hay varias zonas solapadas")]
    public int priority = 0;

    private bool _playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || zoneCamera == null) return;
        if (_playerInside) return; // evita doble registro si el collider tiene varios hijos

        _playerInside = true;
        CameraManager.Instance?.RegisterZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!_playerInside) return;

        _playerInside = false;
        CameraManager.Instance?.UnregisterZone(this);
    }

    private void OnDisable()
    {
        // si la zona se desactiva mientras el player está adentro, no se queda colgada en la pila
        if (_playerInside)
        {
            _playerInside = false;
            CameraManager.Instance?.UnregisterZone(this);
        }
    }
}