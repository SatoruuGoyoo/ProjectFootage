using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    public event Action OnPlayerEntered;
    public event Action OnPlayerExited;
    public bool PlayerInside { get; private set; }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        PlayerInside = true;
        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        PlayerInside = false;
        OnPlayerExited?.Invoke();
    }
}