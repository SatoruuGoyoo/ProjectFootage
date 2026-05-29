using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameWarmup : MonoBehaviour
{
    public static GameWarmup Instance { get; private set; }

    [Header("Camcorder")]
    [Tooltip("El GO de la Camcorder_camera que renderiza a la RenderTexture")]
    [SerializeField] private GameObject camcorderCameraGO;
    [Tooltip("El visual de la corder, para que también renderice una vez")]
    [SerializeField] private GameObject camcorderVisualGO;
    [Tooltip("Frames que dejamos la corder activa para que se cachee todo")]
    [SerializeField] private int warmupFrames = 3;

    [Header("TMP")]
    [SerializeField] private TMP_Text[] textsToWarm;
    [SerializeField] private string warmString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,!?'\"";

    [Header("Events")]
    public UnityEvent OnWarmupFinished;

    public bool IsFinished { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        StartCoroutine(WarmupRoutine());
    }

    private IEnumerator WarmupRoutine()
    {
        WarmupTMP();
        yield return null;

        yield return StartCoroutine(WarmupCamcorder());

        ClearTMP();

        IsFinished = true;
        OnWarmupFinished?.Invoke();
    }

    private void WarmupTMP()
    {
        foreach (var t in textsToWarm)
        {
            if (t == null) continue;
            t.gameObject.SetActive(true);
            t.SetText(warmString);
        }
    }

    private void ClearTMP()
    {
        foreach (var t in textsToWarm)
        {
            if (t == null) continue;
            t.SetText("");
        }
    }

    private IEnumerator WarmupCamcorder()
    {
        bool cameraWasActive = camcorderCameraGO != null && camcorderCameraGO.activeSelf;
        bool visualWasActive = camcorderVisualGO != null && camcorderVisualGO.activeSelf;

        if (camcorderCameraGO != null) camcorderCameraGO.SetActive(true);
        if (camcorderVisualGO != null) camcorderVisualGO.SetActive(true);

        for (int i = 0; i < warmupFrames; i++) yield return null;

        if (camcorderCameraGO != null) camcorderCameraGO.SetActive(cameraWasActive);
        if (camcorderVisualGO != null) camcorderVisualGO.SetActive(visualWasActive);
    }
}