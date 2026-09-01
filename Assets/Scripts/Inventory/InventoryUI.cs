using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Image iconPrefab;

    private bool _subscribed;

    private void OnEnable() => TrySubscribe();
    private void Start() => TrySubscribe();

    private void OnDisable()
    {
        if (ItemRegistry.Instance != null)
            ItemRegistry.Instance.OnItemAdded -= AddIcon;
        _subscribed = false;
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (ItemRegistry.Instance == null) return;

        ItemRegistry.Instance.OnItemAdded += AddIcon;
        _subscribed = true;
    }

    private void AddIcon(ItemData item)
    {
        if (item == null || item.icon == null) return;
        if (iconContainer == null || iconPrefab == null) return;

        Image icon = Instantiate(iconPrefab, iconContainer);
        icon.sprite = item.icon;
        icon.enabled = true;
    }
}