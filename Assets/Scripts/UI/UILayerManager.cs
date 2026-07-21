using System;
using System.Collections.Generic;

/// <summary>
/// Manages UI layer priorities so panels never overlap.
///
/// Each panel registers itself with a Layer. When a new panel wants to show:
///   - If a HIGHER priority panel is open → rechazado (returns false).
///   - If an EQUAL or LOWER priority panel is open → ese panel se cierra primero,
///     luego el nuevo se muestra.
///
/// Layers (de menor a mayor prioridad):
///   1  InteractPrompt
///   2  Feedback
///   3  Readable
///   4  Confirmation
///   98 EntityFeedback ← pista paralela, solo bloqueada por Confirmation
///   99 Subtitles  ← pista paralela, solo bloqueada por Confirmation
///
/// Uso:
///   Al mostrar:  if (!UILayerManager.TryShow(Layer.X, OnForceHide)) return;
///   Al ocultar:  UILayerManager.Release(Layer.X);
/// </summary>
public static class UILayerManager
{
    public enum Layer
    {
        InteractPrompt = 1,
        Feedback = 2,
        Readable = 3,
        Confirmation = 4,
        EntityFeedback = 98,
        Subtitles = 99,
    }

    // Cada layer activo guarda el callback para forzar su cierre.
    private static readonly Dictionary<Layer, Action> _active = new();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Intenta mostrar el panel de la layer dada.
    /// - onForceHide: callback que se llama si otro panel de mayor prioridad
    ///   desplaza a este (o si este desplaza a otro de menor prioridad).
    /// Devuelve true si puede mostrarse, false si fue bloqueado.
    /// </summary>
    public static bool TryShow(Layer layer, Action onForceHide)
    {
        if (layer == Layer.Subtitles || layer == Layer.InteractPrompt || layer == Layer.EntityFeedback)
        {
            if (_active.ContainsKey(Layer.Confirmation)) return false;
            if (layer == Layer.InteractPrompt && _active.ContainsKey(Layer.Readable)) return false;
            RegisterLayer(layer, onForceHide);
            return true;
        }

        foreach (var kv in _active)
        {
            if (kv.Key == Layer.Subtitles) continue;
            if (kv.Key == Layer.InteractPrompt) continue;
            if (kv.Key == Layer.EntityFeedback) continue;
            if ((int)kv.Key > (int)layer) return false;
        }

        ForceCloseLayersUpTo(layer);
        RegisterLayer(layer, onForceHide);
        return true;
    }

    /// <summary>
    /// Llama cuando el panel se oculta por sí solo (timer, input, etc.).
    /// </summary>
    public static void Release(Layer layer)
    {
        _active.Remove(layer);
    }

    /// <summary>
    /// Fuerza el cierre de todos los paneles activos (ej: cambio de escena).
    /// </summary>
    public static void Reset()
    {
        // Disparar todos los callbacks antes de limpiar.
        foreach (var kv in new Dictionary<Layer, Action>(_active))
            kv.Value?.Invoke();
        _active.Clear();
    }

    // ── Internos ──────────────────────────────────────────────────────────────

    private static void RegisterLayer(Layer layer, Action onForceHide)
    {
        // Si ya estaba registrado (ej: feedback nuevo antes de que expire el timer),
        // actualizamos el callback pero NO disparamos el viejo.
        _active[layer] = onForceHide;
    }

    private static void ForceCloseLayersUpTo(Layer incoming)
    {
        var toClose = new List<Layer>();
        foreach (var kv in _active)
        {
            if (kv.Key == Layer.Subtitles) continue;
            if (kv.Key == Layer.InteractPrompt) continue;
            if (kv.Key == Layer.EntityFeedback) continue;
            if ((int)kv.Key <= (int)incoming)
                toClose.Add(kv.Key);
        }
        foreach (var l in toClose)
        {
            _active[l]?.Invoke();
            _active.Remove(l);
        }
    }
}