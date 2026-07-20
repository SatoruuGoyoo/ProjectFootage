using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;


/// <summary>
/// Drives the main menu intro sequence:
///   1. Wait for player input (Space, via el nuevo Input System).
///   2. Animate the camera from a wide shot to the menu screen.
///   3. Reveal the main menu canvas.
///
/// Uses a coroutine-based state machine instead of Update() booleans
/// for clarity and zero per-frame overhead while idle.
/// </summary>
public sealed class MainMenuIntro : MonoBehaviour
{
    // ── Events ────────────────────────────────────────────────────────────
    public event Action OnIntroComplete;

    // ── Inspector Config ──────────────────────────────────────────────────
    [Header("Scene References")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private GameObject pressStartText;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject titleObject;
    [SerializeField] private GameObject subtitleObject;

    [Header("Camera — Wide Shot (start)")]
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 startRotation;

    [Header("Camera — Menu Screen (end)")]
    [SerializeField] private Vector3 endPosition;
    [SerializeField] private Vector3 endRotation;

    [Header("Settings")]
    [SerializeField, Min(0.1f)] private float moveDuration = 2f;

    // ── Cached rotations ──────────────────────────────────────────────────
    private Quaternion _startRot;
    private Quaternion _endRot;

    // ── Input ─────────────────────────────────────────────────────────────
    private InputAction _startAction;
    private bool _startPressed;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _startRot = Quaternion.Euler(startRotation);
        _endRot = Quaternion.Euler(endRotation);

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
        mainCamera.SetPositionAndRotation(startPosition, _startRot);
        SetActive(pressStartText, true);
        SetActive(titleObject, true);
        SetActive(subtitleObject, true);
        SetActive(mainMenuCanvas, false);
    }

    // ── Intro Sequence ────────────────────────────────────────────────────

    private IEnumerator RunIntroSequence()
    {
        yield return WaitForStartKey();

        SetActive(pressStartText, false);
        SetActive(titleObject, false);
        SetActive(subtitleObject, false);

        yield return AnimateCamera();

        SetActive(mainMenuCanvas, true);
        OnIntroComplete?.Invoke();
    }

    private IEnumerator WaitForStartKey()
    {
        _startPressed = false;
        while (!_startPressed)
            yield return null;

        FMODUnity.RuntimeManager.PlayOneShot("event:/MainMenu/UI - UX/UI - ButtonClick");
    }

    private IEnumerator AnimateCamera()
    {
        for (float t = 0f; t < moveDuration; t += Time.deltaTime)
        {
            float smooth = EaseInOut(t / moveDuration);
            mainCamera.SetPositionAndRotation(
                Vector3.Lerp(startPosition, endPosition, smooth),
                Quaternion.Slerp(_startRot, _endRot, smooth)
            );
            yield return null;
        }

        mainCamera.SetPositionAndRotation(endPosition, _endRot);
    }

    // ── Utility ───────────────────────────────────────────────────────────

    /// <summary>Smoothstep easing (slow in, slow out).</summary>
    private static float EaseInOut(float t) => t * t * (3f - 2f * t);

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (mainCamera == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] mainCamera not assigned.", this);
        if (pressStartText == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] pressStartText not assigned.", this);
        if (mainMenuCanvas == null) Debug.LogWarning($"[{nameof(MainMenuIntro)}] mainMenuCanvas not assigned.", this);
    }
#endif
}