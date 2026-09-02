using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemRegistry : MonoBehaviour
{
    public static ItemRegistry Instance { get; private set; }

    public event Action<ItemData> OnItemAdded;
    public event Action<ItemData> OnItemRemoved;

    private readonly HashSet<string> _collectedIds = new();
    private readonly List<ItemData> _items = new();

    public IReadOnlyList<ItemData> Items => _items;

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
        if (item == null || string.IsNullOrEmpty(item.itemId)) return;
        if (!_collectedIds.Add(item.itemId)) return;

        _items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    public bool Remove(ItemData item) => item != null && Remove(item.itemId);

    public bool Remove(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        if (!_collectedIds.Remove(itemId)) return false;

        ItemData removed = null;
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            if (_items[i] == null || _items[i].itemId != itemId) continue;
            removed = _items[i];
            _items.RemoveAt(i);
        }

        OnItemRemoved?.Invoke(removed);
        return true;
    }

    public bool Has(ItemData item) => item != null && Has(item.itemId);

    public bool Has(string itemId) => !string.IsNullOrEmpty(itemId) && _collectedIds.Contains(itemId);
}