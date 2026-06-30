using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD visual del camcorder. Zero GC alloc por frame en condiciones normales.
/// La jerarquía de UI se arma a mano en el Editor; este script solo controla
/// las referencias asignadas por Inspector (no crea GameObjects en runtime).
/// </summary>
public sealed class CorderVisual : MonoBehaviour
{
    // ─── Inspector: referencias de cámara/lógica ───────────────────────────────

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

    // ─── Inspector: referencias de UI (armadas a mano en el Editor) ───────────

    [Header("HUD UI (asignar a mano en el Editor)")]
    [Tooltip("Root que se prende/apaga según si la cámara del camcorder está activa. Puede ser el Canvas, un panel, o un CanvasGroup, lo que vos quieras toggle-ar.")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private Image recDot;
    [SerializeField] private TMP_Text recText;
    [SerializeField] private TMP_Text timecodeText;
    [SerializeField] private Image recordingIndicatorImage;

    // ─── GL rendering ─────────────────────────────────────────────────────────

    private Material _glMat;

    // Vértices del frustum reutilizados (sin new Vector3 cada frame)
    private readonly Vector3[] _frustumVerts = new Vector3[4]; // tl, tr, bl, br

    // Colores precalculados para los tres estados (evita new Color en hot-path)
    private Color _idleFrameColor;
    private Color _recFrameColor;
    private Color _prepFrameColor;

    // StringBuilder reutilizable para el timecode → 0 alloc
    private readonly StringBuilder _sb = new StringBuilder(8, 8);

    // ─── State ────────────────────────────────────────────────────────────────

    private float _recordingTime;
    private bool _wasRecording;
    private CamcorderMode _lastMode = (CamcorderMode)(-1); // fuerza primera actualización

    // Cache para evitar reconstruir el string del timecode si nada cambió
    private int _lastTotalSeconds = -1;
    private char _lastSep = '\0';

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

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

        // Calcular el modo UNA SOLA VEZ por frame y cacharlo
        CamcorderMode mode = GetCurrentMode();

        UpdateRecIndicator(mode);
        UpdateTimecode(mode);

        _lastMode = mode;
    }

    private void OnRenderObject()
    {
        // Guard clauses baratas antes de hacer cualquier trabajo de GL
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

        // tl=0, tr=1, bl=2, br=3
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

    // =========================================================================
    // GL helpers
    // =========================================================================

    /// <summary>Precalcula los 4 vértices del frustum en _frustumVerts (sin alloc).</summary>
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

    // =========================================================================
    // State helpers
    // =========================================================================

    private CamcorderMode GetCurrentMode()
    {
        if (controller != null) return controller.CurrentCamMode;
        if (recorder != null && recorder.IsRecording) return CamcorderMode.Recording;
        return CamcorderMode.Idle;
    }

    /// <summary>
    /// Precalcula los colores derivados de los campos serializados.
    /// Llamar en Awake y también desde OnValidate para preview en Editor.
    /// </summary>
    private void CacheFrameColors()
    {
        _recFrameColor = recColor;
        _prepFrameColor = prepColor;
        _idleFrameColor = new Color(hudColor.r, hudColor.g, hudColor.b, 0.5f);
    }

    // Retorna referencia a color precalculado → sin new Color en hot-path
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
        if (timecodeText == null)
            Debug.LogWarning($"[CorderVisual] '{name}': falta asignar timecodeText en el Inspector.", this);
    }

#if UNITY_EDITOR
    private void OnValidate() => CacheFrameColors();
#endif

    // =========================================================================
    // HUD – actualización por frame (hot-path, zero alloc)
    // =========================================================================

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
                recordingIndicatorImage.color = _recFrameColor; // <- nuevo

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

    private void UpdateTimecode(CamcorderMode mode)
    {
        if (timecodeText == null) return;

        if (mode == CamcorderMode.Recording)
        {
            _recordingTime += Time.deltaTime;
            _wasRecording = true;
        }
        else if (_wasRecording && mode == CamcorderMode.Idle)
        {
            _recordingTime = 0f;
            _wasRecording = false;
        }

        // Separador parpadeante sin alloc de string
        char sep = (mode == CamcorderMode.Recording && Mathf.Sin(Time.time * 4f) > 0f)
                 ? ':'
                 : ' ';

        int tot = Mathf.FloorToInt(_recordingTime);

        // Si ni el segundo entero ni el separador parpadeante cambiaron, no hay nada
        // que actualizar: nos ahorramos el ToString() (y su alloc) frame a frame.
        if (tot == _lastTotalSeconds && sep == _lastSep) return;

        _lastTotalSeconds = tot;
        _lastSep = sep;

        int hh = tot / 3600;
        int mm = (tot % 3600) / 60;
        int ss = tot % 60;

        // Construir "HH:MM:SS" con StringBuilder (reutilizado) → 0 alloc
        _sb.Clear();
        AppendTwoDigits(_sb, hh);
        _sb.Append(sep);
        AppendTwoDigits(_sb, mm);
        _sb.Append(sep);
        AppendTwoDigits(_sb, ss);

        timecodeText.SetText(_sb); // TMP_Text toma el StringBuilder directo → 0 alloc total
    }

    /// <summary>Escribe un entero de 0-99 como dos dígitos sin alloc.</summary>
    private static void AppendTwoDigits(StringBuilder sb, int value)
    {
        sb.Append((char)('0' + value / 10));
        sb.Append((char)('0' + value % 10));
    }
}