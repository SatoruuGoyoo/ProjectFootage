using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD visual del camcorder. Zero GC alloc por frame en condiciones normales.
/// Requiere: Unity 2021+ (Text legacy o TMPro con adaptación mínima).
/// </summary>
[RequireComponent(typeof(Canvas))]
public sealed class CorderVisual : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

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

    // ─── GL rendering ─────────────────────────────────────────────────────────

    private Material _glMat;

    // Vértices del frustum reutilizados (sin new Vector3 cada frame)
    private readonly Vector3[] _frustumVerts = new Vector3[4]; // tl, tr, bl, br

    // Colores precalculados para los tres estados (evita new Color en hot-path)
    private Color _idleFrameColor;
    private Color _recFrameColor;
    private Color _prepFrameColor;

    // ─── HUD ──────────────────────────────────────────────────────────────────

    private Canvas _hudCanvas;
    private Image _recDot;
    private Text _recText;
    private Text _timecodeText;

    // StringBuilder reutilizable para el timecode → 0 alloc
    private readonly StringBuilder _sb = new StringBuilder(8, 8);

    // ─── State ────────────────────────────────────────────────────────────────

    private float _recordingTime;
    private bool _wasRecording;
    private CamcorderMode _lastMode = (CamcorderMode)(-1); // fuerza primera actualización

    // =========================================================================
    // Unity lifecycle
    // =========================================================================

    private void Awake()
    {
        CacheFrameColors();
        CreateGLMaterial();
        CreateHUD();
    }

    private void Update()
    {
        bool camActive = camcorderCamera != null
                      && camcorderCamera.gameObject.activeInHierarchy;

        if (_hudCanvas != null)
            _hudCanvas.gameObject.SetActive(camActive);

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

#if UNITY_EDITOR
    private void OnValidate() => CacheFrameColors();
#endif

    // =========================================================================
    // HUD – construcción (Awake, una sola vez)
    // =========================================================================

    private void CreateHUD()
    {
        var canvasGO = new GameObject("CorderVisual_Canvas");
        canvasGO.transform.SetParent(transform);

        _hudCanvas = canvasGO.AddComponent<Canvas>();
        _hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _hudCanvas.sortingOrder = 90;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var grp = canvasGO.AddComponent<CanvasGroup>();
        grp.interactable = false;
        grp.blocksRaycasts = false;

        CreateRecIndicator(canvasGO.transform);
        _hudCanvas.gameObject.SetActive(false);
    }

    private void CreateRecIndicator(Transform parent)
    {
        var container = MakeRect("RecIndicator", parent,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(30, -30), new Vector2(220, 50));

        var dotGO = MakeRect("RecDot", container.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(14, 14));
        _recDot = dotGO.AddComponent<Image>();
        _recDot.color = recColor;

        var recGO = MakeRect("RecText", container.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(22, 2), new Vector2(80, 20));
        _recText = recGO.AddComponent<Text>();
        _recText.font = GetMonoFont();
        _recText.fontSize = 20;
        _recText.fontStyle = FontStyle.Bold;
        _recText.color = recColor;
        _recText.alignment = TextAnchor.MiddleLeft;
        _recText.text = "REC";

        var tcGO = MakeRect("Timecode", container.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, -22), new Vector2(200, 18));
        _timecodeText = tcGO.AddComponent<Text>();
        _timecodeText.font = GetMonoFont();
        _timecodeText.fontSize = 15;
        _timecodeText.color = hudColor;
        _timecodeText.alignment = TextAnchor.MiddleLeft;
        _timecodeText.text = "00:00:00";
    }

    // =========================================================================
    // HUD – actualización por frame (hot-path, zero alloc)
    // =========================================================================

    private void UpdateRecIndicator(CamcorderMode mode)
    {
        bool isRec = mode == CamcorderMode.Recording;
        bool isPrep = mode == CamcorderMode.Preparing;

        bool showIndicator = isRec || isPrep;
        _recDot.gameObject.SetActive(showIndicator);
        _recText.gameObject.SetActive(showIndicator);

        if (!showIndicator) return;

        if (isRec)
        {
            float b = (Mathf.Sin(Time.time * recBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

            // Reusar color precalculado, solo mutar alpha → sin new Color
            Color c = _recFrameColor;
            c.a = b;
            _recDot.color = c;
            _recText.color = c;

            // Solo asignar string si cambió el modo (el texto "REC" no cambia frame a frame)
            if (_lastMode != CamcorderMode.Recording)
                _recText.text = "REC";
        }
        else // isPrep
        {
            _recDot.color = _prepFrameColor;

            float b = Mathf.Sin(Time.time * 6f) > 0f ? 1f : 0.2f;
            Color c = _prepFrameColor;
            c.a = b;
            _recText.color = c;

            if (_lastMode != CamcorderMode.Preparing)
                _recText.text = "STBY";
        }
    }

    private void UpdateTimecode(CamcorderMode mode)
    {
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

        _timecodeText.text = _sb.ToString(); // ← única alloc inevitable (string inmutable)
        // Si usás TextMeshPro, reemplazá con: _timecodeText.SetText(_sb);  → 0 alloc total
    }

    /// <summary>Escribe un entero de 0-99 como dos dígitos sin alloc.</summary>
    private static void AppendTwoDigits(StringBuilder sb, int value)
    {
        sb.Append((char)('0' + value / 10));
        sb.Append((char)('0' + value % 10));
    }

    // =========================================================================
    // Utilities
    // =========================================================================

    private static GameObject MakeRect(
        string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    private static Font GetMonoFont()
        => Font.CreateDynamicFontFromOSFont("Consolas", 16)
        ?? Font.CreateDynamicFontFromOSFont("Courier New", 16)
        ?? Font.CreateDynamicFontFromOSFont("Arial", 16);
}