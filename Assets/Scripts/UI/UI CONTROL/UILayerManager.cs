using System;
using System.Collections.Generic;

/// <summary>
/// Arbitra qué panel ocupa cada posición de pantalla.
///
/// Regla: dos paneles nunca comparten fila de pantalla (Upper / Middle / Lower).
/// Se compara por fila y no por celda exacta porque un texto largo en
/// LowerCenter invade LowerRight. Al pedir una fila ocupada, el ocupante recibe
/// su callback de cierre y el nuevo se muestra. Confirmation es modal: mientras
/// está abierta cierra todo y rechaza cualquier otro panel.
///
/// Cada layer admite un solo dueño a la vez. Si otra instancia de la misma
/// layer pide mostrarse, la anterior se cierra primero (no queda huérfana).
///
/// Uso:
///   if (!UILayerManager.TryShow(Layer.X, this, position, ForceHide)) return;
///   UILayerManager.Release(Layer.X, this);
/// </summary>
public static class UILayerManager
{
    public enum Layer
    {
        Feedback = 10,
        TutorialPrompt = 20,
        EntityFeedback = 30,
        Subtitles = 40,
        Readable = 50,
        Confirmation = 60,
    }

    public static event Action<bool> OnModalChanged;

    private struct Entry
    {
        public object Owner;
        public UIPositioner.ScreenPosition Slot;
        public Action ForceHide;
    }

    private static readonly Dictionary<Layer, Entry> _active = new();
    private static readonly List<Layer> _toClose = new();

    public static bool IsModalOpen => _active.ContainsKey(Layer.Confirmation);

    public static bool TryShow(Layer layer, object owner, UIPositioner.ScreenPosition slot, Action onForceHide)
    {
        if (owner == null) return false;

        bool modal = layer == Layer.Confirmation;
        if (IsModalOpen && !modal) return false;

        _toClose.Clear();
        foreach (var kv in _active)
        {
            if (kv.Key == layer)
            {
                if (!ReferenceEquals(kv.Value.Owner, owner)) _toClose.Add(kv.Key);
                continue;
            }

            if (modal || RowOf(kv.Value.Slot) == RowOf(slot)) _toClose.Add(kv.Key);
        }

        CloseEntries(_toClose);

        bool wasModalOpen = IsModalOpen;

        _active[layer] = new Entry
        {
            Owner = owner,
            Slot = slot,
            ForceHide = onForceHide,
        };

        if (modal && !wasModalOpen) OnModalChanged?.Invoke(true);
        return true;
    }

    public static void Release(Layer layer, object owner)
    {
        if (!_active.TryGetValue(layer, out var entry)) return;
        if (!ReferenceEquals(entry.Owner, owner)) return;

        _active.Remove(layer);
        if (layer == Layer.Confirmation) OnModalChanged?.Invoke(false);
    }

    public static void Reset()
    {
        _toClose.Clear();
        foreach (var kv in _active) _toClose.Add(kv.Key);
        CloseEntries(_toClose);
        _active.Clear();
        OnModalChanged?.Invoke(false);
    }

    private static int RowOf(UIPositioner.ScreenPosition position) => position switch
    {
        UIPositioner.ScreenPosition.UpperLeft => 0,
        UIPositioner.ScreenPosition.UpperCenter => 0,
        UIPositioner.ScreenPosition.UpperRight => 0,
        UIPositioner.ScreenPosition.MiddleLeft => 1,
        UIPositioner.ScreenPosition.MiddleCenter => 1,
        UIPositioner.ScreenPosition.MiddleRight => 1,
        _ => 2,
    };

    private static void CloseEntries(List<Layer> layers)
    {
        if (layers.Count == 0) return;

        bool closedModal = false;

        for (int i = 0; i < layers.Count; i++)
        {
            if (!_active.TryGetValue(layers[i], out var entry)) continue;
            _active.Remove(layers[i]);
            if (layers[i] == Layer.Confirmation) closedModal = true;
            entry.ForceHide?.Invoke();
        }

        if (closedModal) OnModalChanged?.Invoke(false);
    }
}