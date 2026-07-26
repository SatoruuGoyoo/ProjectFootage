using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Punto central de culling manual. Mantiene dos fuentes de visibilidad
/// independientes —la cámara fija activa y la zona actual del jugador
/// (camcorder)— y muestra la unión de ambas. Cada fuente solo se recalcula
/// cuando cambia (cambio de cámara, o el jugador cruza un trigger de
/// zona), nunca por frame, y el diff evita tocar renderers que ya estaban
/// en el estado correcto.
/// </summary>
public sealed class OcclusionCullingManager : MonoBehaviour
{
    private static OcclusionCullingManager _instance;

    public static OcclusionCullingManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject("OcclusionCullingManager").AddComponent<OcclusionCullingManager>();
            return _instance;
        }
    }

    public IReadOnlyCollection<OcclusionZone> Visible => _visible;

    private readonly HashSet<OcclusionZone> _fixedCameraZones = new HashSet<OcclusionZone>();
    private readonly HashSet<OcclusionZone> _playerZones = new HashSet<OcclusionZone>();
    private readonly HashSet<OcclusionZone> _visible = new HashSet<OcclusionZone>();
    private readonly HashSet<OcclusionZone> _nextVisible = new HashSet<OcclusionZone>();

    private OcclusionZone _currentPlayerZone;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Todo arranca oculto; lo único visible es lo que después declaren
        // explícitamente la cámara fija activa o la zona del jugador. Este
        // Awake corre siempre antes de cualquier llamada a Instance, sea
        // por orden normal de escena o porque el getter recién lo creó.
        OcclusionZone[] allZones = FindObjectsOfType<OcclusionZone>();
        for (int i = 0; i < allZones.Length; i++)
            allZones[i].SetVisible(false);
    }

    public void SetPlayerZone(OcclusionZone zone)
    {
        if (_currentPlayerZone == zone) return;
        _currentPlayerZone = zone;

        _playerZones.Clear();
        AddWithMargin(zone, _playerZones, margin: 1);

        Recompute();
    }

    public void SetFixedCameraZones(IReadOnlyList<OcclusionZone> zones, int margin = 1)
    {
        _fixedCameraZones.Clear();

        if (zones != null)
        {
            for (int i = 0; i < zones.Count; i++)
                AddWithMargin(zones[i], _fixedCameraZones, margin);
        }

        Recompute();
    }

    // Además de lo listado a mano, camina 'margin' saltos más por el grafo
    // de vecinos. El guard de 'target.Add' evita recorrer dos veces la
    // misma zona si el grafo tiene ciclos.
    private static void AddWithMargin(OcclusionZone zone, HashSet<OcclusionZone> target, int margin)
    {
        if (zone == null || !target.Add(zone)) return;
        if (margin <= 0) return;

        IReadOnlyList<OcclusionZone> neighbors = zone.VisibleNeighbors;
        for (int i = 0; i < neighbors.Count; i++)
            AddWithMargin(neighbors[i], target, margin - 1);
    }

    private void Recompute()
    {
        _nextVisible.Clear();
        _nextVisible.UnionWith(_fixedCameraZones);
        _nextVisible.UnionWith(_playerZones);

        foreach (OcclusionZone zone in _visible)
        {
            if (!_nextVisible.Contains(zone))
                zone.SetVisible(false);
        }

        foreach (OcclusionZone zone in _nextVisible)
        {
            if (!_visible.Contains(zone))
                zone.SetVisible(true);
        }

        _visible.Clear();
        _visible.UnionWith(_nextVisible);
    }
}