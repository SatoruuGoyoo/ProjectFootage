using System.Collections.Generic;
using UnityEngine;

public class InstructionsUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private UIPositioner positioner;

    [Header("Entries")]
    [Tooltip("Padre de las entradas. Ponéle un Horizontal o Vertical Layout Group.")]
    [SerializeField] private Transform entryContainer;
    [SerializeField] private InstructionEntryUI entryPrefab;
    [SerializeField] private int prewarmCount = 4;

    private readonly List<InstructionEntryUI> _pool = new();

    private IReadOnlyList<InteractionInstruction> _interactableSet;
    private UIPositioner.ScreenPosition _interactablePosition;
    private IReadOnlyList<InteractionInstruction> _modalSet;
    private UIPositioner.ScreenPosition _modalPosition;
    private IReadOnlyList<InteractionInstruction> _displayed;
    private UIPositioner.ScreenPosition _displayedPosition;
    private bool _isVisible;

    private void Awake()
    {
        _isVisible = true;
        Prewarm();
        SetVisible(false);
    }

    private void OnEnable()
    {
        GameEvents.OnInstructionsShown += OnInteractableShown;
        GameEvents.OnInstructionsHidden += OnInteractableHidden;
        GameEvents.OnModalInstructionsShown += OnModalShown;
        GameEvents.OnModalInstructionsHidden += OnModalHidden;
        UILayerManager.OnModalChanged += OnModalChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnInstructionsShown -= OnInteractableShown;
        GameEvents.OnInstructionsHidden -= OnInteractableHidden;
        GameEvents.OnModalInstructionsShown -= OnModalShown;
        GameEvents.OnModalInstructionsHidden -= OnModalHidden;
        UILayerManager.OnModalChanged -= OnModalChanged;
    }

    private void OnInteractableShown(IReadOnlyList<InteractionInstruction> set, UIPositioner.ScreenPosition position)
    {
        _interactableSet = set;
        _interactablePosition = position;
        Refresh();
    }

    private void OnInteractableHidden()
    {
        _interactableSet = null;
        Refresh();
    }

    private void OnModalShown(IReadOnlyList<InteractionInstruction> set, UIPositioner.ScreenPosition position)
    {
        _modalSet = set;
        _modalPosition = position;
        Refresh();
    }

    private void OnModalHidden()
    {
        _modalSet = null;
        Refresh();
    }

    private void OnModalChanged(bool modalOpen) => Refresh();

    private IReadOnlyList<InteractionInstruction> ResolveSet(out UIPositioner.ScreenPosition position)
    {
        if (_modalSet != null && _modalSet.Count > 0)
        {
            position = _modalPosition;
            return _modalSet;
        }

        position = _interactablePosition;

        if (UILayerManager.IsModalOpen) return null;
        if (_interactableSet != null && _interactableSet.Count > 0) return _interactableSet;
        return null;
    }

    private void Refresh()
    {
        IReadOnlyList<InteractionInstruction> set = ResolveSet(out UIPositioner.ScreenPosition position);

        if (ReferenceEquals(set, _displayed) && position == _displayedPosition)
        {
            SetVisible(set != null);
            return;
        }

        _displayed = set;
        _displayedPosition = position;

        if (set == null)
        {
            HideAllEntries();
            SetVisible(false);
            return;
        }

        positioner?.SetPosition(position);

        for (int i = 0; i < set.Count; i++)
        {
            InstructionEntryUI entry = GetEntry(i);
            if (entry == null) break;

            entry.Bind(set[i]);
            entry.gameObject.SetActive(true);
        }

        for (int i = set.Count; i < _pool.Count; i++)
            _pool[i].gameObject.SetActive(false);

        SetVisible(true);
    }

    private void HideAllEntries()
    {
        for (int i = 0; i < _pool.Count; i++)
            _pool[i].gameObject.SetActive(false);
    }

    private void Prewarm()
    {
        if (entryContainer == null || entryPrefab == null) return;

        for (int i = 0; i < prewarmCount; i++)
        {
            InstructionEntryUI entry = Instantiate(entryPrefab, entryContainer);
            entry.gameObject.SetActive(false);
            _pool.Add(entry);
        }
    }

    private InstructionEntryUI GetEntry(int index)
    {
        if (index < _pool.Count) return _pool[index];
        if (entryContainer == null || entryPrefab == null) return null;

        InstructionEntryUI entry = Instantiate(entryPrefab, entryContainer);
        _pool.Add(entry);
        return entry;
    }

    private void SetVisible(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }
}