using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    public Camera ActiveCamera => currentCamera;

    private Camera currentCamera;
    private readonly List<CameraZone> activeZones = new();

    private void Awake() => Instance = this;

    public void RegisterZone(CameraZone zone)
    {
        if (activeZones.Contains(zone)) return;

        activeZones.Add(zone);
        ResolveActiveZone();
    }

    public void UnregisterZone(CameraZone zone)
    {
        if (!activeZones.Remove(zone)) return;

        ResolveActiveZone();
    }

    private void ResolveActiveZone()
    {
        if (activeZones.Count == 0)
        {
            // no queda ninguna zona pisada, se mantiene la última cámara activa
            return;
        }

        // gana la de mayor prioridad, y entre empates la última en entrar
        CameraZone winner = activeZones[0];
        for (int i = 1; i < activeZones.Count; i++)
        {
            if (activeZones[i].priority >= winner.priority)
                winner = activeZones[i];
        }

        SetCamera(winner.zoneCamera);
    }

    public void SetCamera(Camera newCam)
    {
        if (currentCamera == newCam || newCam == null) return;

        if (currentCamera != null) currentCamera.gameObject.SetActive(false);
        currentCamera = newCam;
        currentCamera.gameObject.SetActive(true);
        currentCamera.GetComponent<FixedCameraController>()?.OnActivated();
    }
}