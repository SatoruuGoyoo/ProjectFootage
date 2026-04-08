using System;
using UnityEngine;

/// <summary>
/// Colocá este componente en cualquier GameObject con Collider Trigger.
/// El HorrorEventManager lo referencia directamente en las entradas de tipo SpatialZone.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HorrorZoneTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Si true, el trigger solo se activa una vez y luego se desactiva.")]
    [SerializeField] private bool disableAfterTrigger = false;

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        OnPlayerEntered?.Invoke();
        if (disableAfterTrigger) gameObject.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        OnPlayerExited?.Invoke();
    }
}
