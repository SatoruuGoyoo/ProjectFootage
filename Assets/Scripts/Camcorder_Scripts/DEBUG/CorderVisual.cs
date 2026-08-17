using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CorderVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera camcorderCamera;
    [SerializeField] private VideoRecorder recorder;
    [SerializeField] private CamcorderController controller;

    [Header("Frame")]
    [SerializeField] private float frustumDistance = 5f;

    [Header("Style")]
    [SerializeField] private Color hudColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color recColor = new Color(1f, 0.08f, 0.08f, 0.95f);
    [SerializeField] private Color prepColor = new Color(1f, 0.85f, 0f, 0.90f);

    [Header("Blink")]
    [SerializeField] private float recBlinkSpeed = 2.5f;

    [Header("HUD UI (asignar a mano en el Editor)")]
    [Tooltip("Root que se prende/apaga según si la cámara del camcorder está activa.")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private Image recDot;
    [SerializeField] private TMP_Text recText;
    [SerializeField] private Image recordingProgressBar;
    [SerializeField] private Image recordingIndicatorImage;

    private Material _glMat;

    private readonly Vector3[] _frustumVerts = new Vector3[4];
    private Color _idleFrameColor;
    private Color _recFrameColor;
    private Color _prepFrameColor;

    private bool _wasRecording;
    private CamcorderMode _lastMode = (CamcorderMode)(-1);

    private void Awake()
    {
        CacheFrameColors();
        CreateGLMaterial();
        ValidateReferences();

        if (hudRoot != null)
            hudRoot.SetActive(false);
    }

    private void Update()
    {
        bool camActive = camcorderCamera != null
                      && camcorderCamera.gameObject.activeInHierarchy;

        if (hudRoot != null)
            hudRoot.SetActive(camActive);

        if (!camActive) return;

        CamcorderMode mode = GetCurrentMode();

        UpdateRecIndicator(mode);
        UpdateProgressBar(mode);

        _lastMode = mode;
    }

    private void OnRenderObject()
    {
        if (_glMat == null) return;
        if (camcorderCamera == null) return;
        if (!camcorderCamera.gameObject.activeInHierarchy) return;
        if (Camera.current == camcorderCamera) return;

        CamcorderMode mode = GetCurrentMode();
        Color col = GetFrameColor(mode);

        ComputeFrustumVerts();

        _glMat.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(col);

        GL.Vertex(_frustumVerts[0]); GL.Vertex(_frustumVerts[1]); // top
        GL.Vertex(_frustumVerts[1]); GL.Vertex(_frustumVerts[3]); // right
        GL.Vertex(_frustumVerts[3]); GL.Vertex(_frustumVerts[2]); // bottom
        GL.Vertex(_frustumVerts[2]); GL.Vertex(_frustumVerts[0]); // left

        GL.End();
        GL.PopMatrix();
    }

    private void OnDestroy()
    {
        if (_glMat != null) Destroy(_glMat);
    }

    private void ComputeFrustumVerts()
    {
        Transform camT = camcorderCamera.transform;
        float halfH = frustumDistance * Mathf.Tan(camcorderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfW = halfH * camcorderCamera.aspect;

        Vector3 c = camT.position + camT.forward * frustumDistance;
        Vector3 up = camT.up * halfH;
        Vector3 rt = camT.right * halfW;

        _frustumVerts[0] = c + up - rt; // tl
        _frustumVerts[1] = c + up + rt; // tr
        _frustumVerts[2] = c - up - rt; // bl
        _frustumVerts[3] = c - up + rt; // br
    }

    private void CreateGLMaterial()
    {
        var shader = Shader.Find("Hidden/Internal-Colored");
        _glMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        _glMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _glMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _glMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _glMat.SetInt("_ZWrite", 0);
        _glMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
    }

    private CamcorderMode GetCurrentMode()
    {
        if (controller != null) return controller.CurrentCamMode;
        if (recorder != null && recorder.IsRecording) return CamcorderMode.Recording;
        return CamcorderMode.Idle;
    }

    private void CacheFrameColors()
    {
        _recFrameColor = recColor;
        _prepFrameColor = prepColor;
        _idleFrameColor = new Color(hudColor.r, hudColor.g, hudColor.b, 0.5f);
    }

    private Color GetFrameColor(CamcorderMode mode) => mode switch
    {
        CamcorderMode.Recording => _recFrameColor,
        CamcorderMode.Preparing => _prepFrameColor,
        _ => _idleFrameColor,
    };

    private void ValidateReferences()
    {
        if (hudRoot == null)
            Debug.LogWarning($"[CorderVisual] '{name}': falta asignar hudRoot en el Inspector.", this);
        if (recDot == null)
            Debug.LogWarning($"[CorderVisual] '{name}': falta asignar recDot en el Inspector.", this);
        if (recText == null)
            Debug.LogWarning($"[CorderVisual] '{name}': falta asignar recText en el Inspector.", this);
        if (recordingProgressBar == null)
            Debug.LogWarning($"[CorderVisual] '{name}': falta asignar recordingProgressBar en el Inspector.", this);
    }

#if UNITY_EDITOR
    private void OnValidate() => CacheFrameColors();
#endif

    private void UpdateRecIndicator(CamcorderMode mode)
    {
        if (recDot == null || recText == null) return;

        bool isRec = mode == CamcorderMode.Recording;
        bool isPrep = mode == CamcorderMode.Preparing;

        bool showIndicator = isRec || isPrep;
        recDot.gameObject.SetActive(showIndicator);
        recText.gameObject.SetActive(showIndicator);

        if (!showIndicator)
        {
            if (recordingIndicatorImage != null)
                recordingIndicatorImage.color = hudColor;
            return;
        }

        if (isRec)
        {
            float b = (Mathf.Sin(Time.time * recBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

            Color c = _recFrameColor;
            c.a = b;
            recDot.color = c;
            recText.color = c;

            if (recordingIndicatorImage != null)
                recordingIndicatorImage.color = _recFrameColor;

            if (_lastMode != CamcorderMode.Recording)
                recText.text = "REC";
        }
        else // isPrep
        {
            if (recordingIndicatorImage != null)
                recordingIndicatorImage.color = hudColor;

            recDot.color = _prepFrameColor;

            float b = Mathf.Sin(Time.time * 6f) > 0f ? 1f : 0.2f;
            Color c = _prepFrameColor;
            c.a = b;
            recText.color = c;

            if (_lastMode != CamcorderMode.Preparing)
                recText.text = "STBY";
        }
    }

    private void UpdateProgressBar(CamcorderMode mode)
    {
        if (recordingProgressBar == null) return;

        if (mode == CamcorderMode.Recording)
        {
            if (!recordingProgressBar.gameObject.activeSelf)
                recordingProgressBar.gameObject.SetActive(true);

            float target = controller != null ? controller.CurrentRecordingTarget : 0f;
            float elapsed = controller != null ? controller.CurrentRecordingElapsed : 0f;
            float progress = target > 0f ? Mathf.Clamp01(elapsed / target) : 0f;
            recordingProgressBar.fillAmount = progress;
            _wasRecording = true;
        }
        else
        {
            if (recordingProgressBar.gameObject.activeSelf)
            {
                recordingProgressBar.fillAmount = 0f;
                recordingProgressBar.gameObject.SetActive(false);
            }
            _wasRecording = false;
        }
    }
}