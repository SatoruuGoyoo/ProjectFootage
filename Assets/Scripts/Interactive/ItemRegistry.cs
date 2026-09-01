using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance { get; private set; }

    public event Action<ItemData> OnItemAdded ;

    private readonly HashSet<string> _collectedIds = new();
    private readonly List<ItemData> _items = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Collect(ItemData item)
    {
        if (_items == null || string.IsNullOrEmpty(item.itemId)) return;
        if(!_collectedIds.Add(item.itemId)) return;

        _items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    public bool Has(string itemId) => _collectedIds.Contains(itemId);
}