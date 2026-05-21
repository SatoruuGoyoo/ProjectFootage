using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance { get; private set; }

    private readonly HashSet<string> collected = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Collect(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        collected.Add(itemId);
    }

    public bool Has(string itemId) => collected.Contains(itemId);
}