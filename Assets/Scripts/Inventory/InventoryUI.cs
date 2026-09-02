using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Image iconPrefab;

    private readonly Dictionary<string, Image> _icons = new();
    private bool _subscribed;

    private void OnEnable() => TrySubscribe();
    private void Start() => TrySubscribe();

    private void OnDisable()
    {
        if (ItemRegistry.Instance != null)
        {
            ItemRegistry.Instance.OnItemAdded -= AddIcon;
            ItemRegistry.Instance.OnItemRemoved -= RemoveIcon;
        }
        _subscribed = false;
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (ItemRegistry.Instance == null) return;

        ItemRegistry.Instance.OnItemAdded += AddIcon;
        ItemRegistry.Instance.OnItemRemoved += RemoveIcon;
        _subscribed = true;

        Rebuild();
    }

    private void Rebuild()
    {
        foreach (var item in ItemRegistry.Instance.Items)
            AddIcon(item);
    }

    private void AddIcon(ItemData item)
    {
        if (item == null || item.icon == null) return;
        if (iconContainer == null || iconPrefab == null) return;
        if (_icons.ContainsKey(item.itemId)) return;

        Image icon = Instantiate(iconPrefab, iconContainer);
        icon.sprite = item.icon;
        icon.enabled = true;
        _icons[item.itemId] = icon;
    }

    private void RemoveIcon(ItemData item)
    {
        if (item == null) return;
        if (!_icons.TryGetValue(item.itemId, out Image icon)) return;

        _icons.Remove(item.itemId);
        if (icon != null) Destroy(icon.gameObject);
    }
}