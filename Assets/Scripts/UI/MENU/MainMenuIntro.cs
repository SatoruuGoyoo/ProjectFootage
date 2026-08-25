using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using SM.UI;

public sealed class MainMenuIntro : MonoBehaviour
{
    // ── Events ────────────────────────────────────────────────────────────
    public event Action OnMenuRevealed;
    public event Action OnIntroComplete;

    // ── Inspector Config ──────────────────────────────────────────────────
    [Header("Scene References")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private GameObject pressStartText;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject titleObject;
    [SerializeField] private GameObject subtitleObject;
    [SerializeField] private MainMenuManager menuManager;

    [Header("Camera Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Camcorder Screen")]
    [SerializeField] private Transform camcorderScreen;
    [SerializeField] private Vector3 screenRotationOffset = new Vector3(0f, 0f, -90f);

    [Header("Transition")]
    [SerializeField, Min(0.1f)] private float transitionDuration = 2f;
    [SerializeField] private AnimationCurve cameraCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve screenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Range(0f, 1f)] private float menuRevealAt = 0.5f;

    [Header("Audio")]
    [SerializeField] private string startEvent = "event:/MainMenu/UI - UX/UI - ButtonClick";

    // ── Input ─────────────────────────────────────────────────────────────
    private InputAction _startAction;
    private bool _startPressed;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _startAction = new InputAction(name: "MenuStart", type: InputActionType.Button, binding: "<Keyboard>/space");
        _startAction.performed += ctx => _startPressed = true;
    }

    private void OnEnable() => _startAction.Enable();

    private void OnDisable() => _startAction.Disable();

    private void OnDestroy() => _startAction.Dispose();

    private void Start()
    {
        InitialState();
        StartCoroutine(RunIntroSequence());
    }

    // ── State Setup ───────────────────────────────────────────────────────

    private void InitialState()
    {
        if (mainCamera != null && startPoint != null)
            mainCamera.SetPositionAndRotation(startPoint.position, startPoint.rotation);

        SetActive(pressStartText, true);
        SetActive(titleObject, true);
        SetActive(subtitleObject, true);
        SetActive(mainMenuCanvas, false);

        if (menuManager != null)
            menuManager.DisableInteraction();
    }

    // ── Intro Sequence ────────────────────────────────────────────────────

    private IEnumerator RunIntroSequence()
    {
        yield return WaitForStartKey();

        SetActive(pressStartText, false);
        SetActive(titleObject, false);
        SetActive(subtitleObject, false);

        yield return Transition();
    }

    private IEnumerator WaitForStartKey()
    {
        _startPressed = false;
        while (!_startPressed)
            yield return null;

        if (!string.IsNullOrEmpty(startEvent))
            RuntimeManager.PlayOneShot(startEvent);
    }

    private IEnumerator Transition()
    {
        bool hasCamera = mainCamera != null && startPoint != null && endPoint != null;
        bool hasScreen = camcorderScreen != null;

        Vector3 camFromPos = hasCamera ? startPoint.position : Vector3.zero;
        Vector3 camToPos = hasCamera ? endPoint.position : Vector3.zero;
        Quaternion camFromRot = hasCamera ? startPoint.rotation : Quaternion.identity;
        Quaternion camToRot = hasCamera ? endPoint.rotation : Quaternion.identity;

        Quaternion screenFromRot = hasScreen ? camcorderScreen.localRotation : Quaternion.identity;
        Quaternion screenToRot = screenFromRot * Quaternion.Euler(screenRotationOffset);

        bool menuRevealed = false;

        for (float t = 0f; t < transitionDuration; t += Time.deltaTime)
        {
            float normalized = Mathf.Clamp01(t / transitionDuration);

            if (hasCamera)
            {
                float k = cameraCurve.Evaluate(normalized);
                mainCamera.SetPositionAndRotation(
                    Vector3.LerpUnclamped(camFromPos, camToPos, k),
                    Quaternion.SlerpUnclamped(camFromRot, camToRot, k)
                );
            }

            if (hasScreen)
                camcorderScreen.localRotation = Quaternion.SlerpUnclamped(screenFromRot, screenToRot, screenCurve.Evaluate(normalized));

            if (!menuRevealed && normalized >= menuRevealAt)
            {
                menuRevealed = true;
                SetActive(mainMenuCanvas, true);
                OnMenuRevealed?.Invoke();
            }

            yield return null;
        }

        if (hasCamera) mainCamera.SetPositionAndRotation(camToPos, camToRot);
        if (hasScreen) camcorderScreen.localRotation = screenToRot;

        if (!menuRevealed)
        {
            SetActive(mainMenuCanvas, true);
            OnMenuRevealed?.Invoke();
        }

        if (menuManager != null)
            menuManager.EnableInteraction();

        OnIntroComplete?.Invoke();
    }

    // ── Utility ───────────────────────────────────────────────────────────

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

#if UNITY_EDITOR
    [ContextMenu("Capture Start Point From Camera")]
    private void CaptureStartPoint() => CapturePoint(startPoint);

    [ContextMenu("Capture End Point From Camera")]
    private void CaptureEndPoint() => CapturePoint(endPoint);

    [ContextMenu("Preview Start Point")]
    private void PreviewStartPoint() => MoveCameraTo(startPoint);

    [ContextMenu("Preview End Point")]
    private void PreviewEndPoint() => MoveCameraTo(endPoint);

    private void CapturePoint(Transform point)
    {
        if (mainCamera == null || point == null) return;
        UnityEditor.Undo.RecordObject(point, "Capture Camera Point");
        point.SetPositionAndRotation(mainCamera.position, mainCamera.rotation);
        UnityEditor.EditorUtility.SetDirty(point);
    }

    private void MoveCameraTo(Transform point)
    {
        if (mainCamera == null || point == null) return;
        UnityEditor.Undo.RecordObject(mainCamera, "Preview Camera Point");
        mainCamera.SetPositionAndRotation(point.position, point.rotation);
    }

    private void OnValidate()
    {
        if (mainCamera == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] mainCamera not assigned.", this);
        if (startPoint == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] startPoint not assigned.", this);
        if (endPoint == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] endPoint not assigned.", this);
        if (mainMenuCanvas == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] mainMenuCanvas not assigned.", this);
        if (menuManager == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] menuManager not assigned.", this);
    }
#endif
}