using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Camcorder viewfinder HUD — estilo Handycam 90s.
/// Muestra: frustum de proyección en 3D, límites de tilt, indicador REC y timecode.
/// </summary>
public class CorderVisual : MonoBehaviour
{
    [Header("References")]
    public Camera camcorderCamera;
    public CamcorderRecorder recorder;
    public CamcorderMotor motor;
    public CamcorderController controller;

    [Header("Toggle")]
    public bool showViewfinder = true;
    public bool showFrustum = true;
    public bool showTiltLimits = true;

    [Header("Frustum")]
    [SerializeField] private float frustumDistance = 5f;
    [SerializeField] private float frustumEdgeWidth = 3;

    [Header("Tilt Limits")]
    [SerializeField] private float tiltArcRadius = 1.5f;
    [SerializeField] private int arcSegments = 20;

    [Header("Style")]
    [SerializeField] private Color hudColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color recColor = new Color(1f, 0.08f, 0.08f, 0.95f);
    [SerializeField] private Color prepColor = new Color(1f, 0.85f, 0f, 0.9f);

    [Header("Blink")]
    [SerializeField] private float recBlinkSpeed = 2.5f;

    // UI
    private Canvas hudCanvas;
    private CanvasGroup hudGroup;

    // REC indicator
    private Image recDot;
    private Text recText;
    private Text timecodeText;



    // GL
    private Material glMaterial;

    // State
    private float recordingTime = 0f;
    private bool wasRecording = false;

    // ?????????????????????????????????????????????????????????????????

    private void Awake()
    {
        CreateGLMaterial();
        CreateHUD();
    }

    private void OnDestroy()
    {
        if (glMaterial != null)
            DestroyImmediate(glMaterial);
    }

    private void Update()
    {
        if (hudCanvas == null) return;

        bool camActive = camcorderCamera != null && camcorderCamera.gameObject.activeInHierarchy;
        hudCanvas.gameObject.SetActive(camActive && showViewfinder);

        if (!camActive || !showViewfinder) return;

        CamcorderMode mode = GetCurrentMode();
        UpdateRecIndicator(mode);
        UpdateTimecode(mode);

    }

    // ??????????????????????????? State ???????????????????????????????

    private CamcorderMode GetCurrentMode()
    {
        if (controller != null)
            return controller.CurrentCamMode;

        if (recorder != null && recorder.IsRecording)
            return CamcorderMode.Recording;

        return CamcorderMode.Idle;
    }

    // ??????????????????????? GL Drawing ??????????????????????????????

    private void CreateGLMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        glMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        glMaterial.SetInt("_ZWrite", 0);
        glMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
    }

    private void OnRenderObject()
    {
        if (camcorderCamera == null || !camcorderCamera.gameObject.activeInHierarchy) return;
        if (Camera.current == camcorderCamera) return;

        CamcorderMode mode = GetCurrentMode();
        Color color = GetStateColor(mode);

        if (showFrustum) DrawFrustum(color, mode);
        if (showTiltLimits) DrawTiltLimits(color, mode);
    }

    private Color GetStateColor(CamcorderMode mode)
    {
        switch (mode)
        {
            case CamcorderMode.Preparing: return prepColor;
            case CamcorderMode.Recording: return recColor;
            default: return new Color(hudColor.r, hudColor.g, hudColor.b, 0.5f);
        }
    }

    private void DrawFrustum(Color color, CamcorderMode mode)
    {
        Transform camT = camcorderCamera.transform;
        float fov = camcorderCamera.fieldOfView;
        float aspect = camcorderCamera.aspect;

        float halfHeight = frustumDistance * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * aspect;

        Vector3 origin = camT.position;
        Vector3 center = origin + camT.forward * frustumDistance;
        Vector3 up = camT.up * halfHeight;
        Vector3 right = camT.right * halfWidth;

        Vector3 tl = center + up - right;
        Vector3 tr = center + up + right;
        Vector3 bl = center - up - right;
        Vector3 br = center - up + right;

        glMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);

        // Relleno semitransparente
        GL.Begin(GL.QUADS);
        Color fill = color;
        fill.a = (mode == CamcorderMode.Recording) ? 0.06f : 0.03f;
        GL.Color(fill);
        GL.Vertex(tl); GL.Vertex(tr); GL.Vertex(br); GL.Vertex(bl);
        GL.End();

        GL.Begin(GL.LINES);

        // Líneas origen ? esquinas (tenues)
        Color edge = color; edge.a = color.a * 0.3f;
        GL.Color(edge);
        GLLine(origin, tl); GLLine(origin, tr);
        GLLine(origin, bl); GLLine(origin, br);

        // Rectángulo far plane
        GL.Color(color);
        GLLine(tl, tr); GLLine(tr, br);
        GLLine(br, bl); GLLine(bl, tl);

        // Crosshair
        Color cross = color; cross.a = color.a * 0.6f;
        GL.Color(cross);
        float cs = halfHeight * 0.15f;
        GLLine(center + camT.up * cs, center + camT.up * -cs);
        GLLine(center + camT.right * -cs, center + camT.right * cs);

        // Brackets de esquina
        float bl2 = halfHeight * 0.2f;
        GL.Color(color);
        GLLine(tl, tl + camT.right * bl2); GLLine(tl, tl - camT.up * bl2);
        GLLine(tr, tr - camT.right * bl2); GLLine(tr, tr - camT.up * bl2);
        GLLine(bl, bl + camT.right * bl2); GLLine(bl, bl + camT.up * bl2);
        GLLine(br, br - camT.right * bl2); GLLine(br, br + camT.up * bl2);

        GL.End();
        GL.PopMatrix();
    }

    private void DrawTiltLimits(Color color, CamcorderMode mode)
    {
        if (motor == null) return;

        Transform pivot = camcorderCamera.transform.parent != null
            ? camcorderCamera.transform.parent
            : camcorderCamera.transform;

        float minAngle = motor.tiltMinAngle;
        float maxAngle = motor.tiltMaxAngle;

        glMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        GL.Begin(GL.LINES);

        // Arco
        Color arc = color; arc.a = color.a * 0.25f;
        GL.Color(arc);
        DrawArc(pivot, minAngle, maxAngle, tiltArcRadius, arcSegments);

        // Líneas límite
        Color lim = color; lim.a = color.a * 0.6f;
        GL.Color(lim);
        GLLine(pivot.position, pivot.position + GetTiltDirection(pivot, minAngle) * tiltArcRadius * 1.2f);
        GLLine(pivot.position, pivot.position + GetTiltDirection(pivot, maxAngle) * tiltArcRadius * 1.2f);

        // Dirección actual
        GL.Color(new Color(1f, 1f, 1f, 0.9f));
        GLLine(pivot.position, pivot.position + camcorderCamera.transform.forward * tiltArcRadius * 0.9f);

        GL.End();
        GL.PopMatrix();
    }

    private void DrawArc(Transform pivot, float fromAngle, float toAngle, float radius, int segments)
    {
        Vector3 prev = pivot.position + GetTiltDirection(pivot, fromAngle) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(fromAngle, toAngle, t);
            Vector3 point = pivot.position + GetTiltDirection(pivot, angle) * radius;
            GLLine(prev, point);
            prev = point;
        }
    }

    private Vector3 GetTiltDirection(Transform pivot, float angle)
        => Quaternion.AngleAxis(angle, pivot.right) * pivot.forward;

    private void GLLine(Vector3 a, Vector3 b) { GL.Vertex(a); GL.Vertex(b); }

    // ??????????????????????? HUD Creation ????????????????????????????

    private void CreateHUD()
    {
        GameObject canvasGO = new GameObject("CorderVisual_Canvas");
        canvasGO.transform.SetParent(transform);

        hudCanvas = canvasGO.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 90;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        hudGroup = canvasGO.AddComponent<CanvasGroup>();
        hudGroup.interactable = false;
        hudGroup.blocksRaycasts = false;

        CreateRecIndicator(canvasGO.transform);


        hudCanvas.gameObject.SetActive(false);
    }

    private void CreateRecIndicator(Transform parent)
    {
        GameObject container = MakeRect("RecIndicator", parent,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(40, -35), new Vector2(280, 60));

        // Dot rojo
        GameObject dotGO = MakeRect("RecDot", container.transform,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, 0), new Vector2(18, 18));
        recDot = dotGO.AddComponent<Image>();
        recDot.color = recColor;

        // Texto REC / STBY
        GameObject recGO = MakeRect("RecText", container.transform,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(26, 0), new Vector2(60, 24));
        recText = recGO.AddComponent<Text>();
        recText.font = GetMonoFont();
        recText.fontSize = 22;
        recText.fontStyle = FontStyle.Bold;
        recText.color = recColor;
        recText.alignment = TextAnchor.MiddleLeft;
        recText.text = "REC";

        // Timecode
        GameObject tcGO = MakeRect("Timecode", container.transform,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, -28), new Vector2(200, 20));
        timecodeText = tcGO.AddComponent<Text>();
        timecodeText.font = GetMonoFont();
        timecodeText.fontSize = 16;
        timecodeText.color = hudColor;
        timecodeText.alignment = TextAnchor.MiddleLeft;
        timecodeText.text = "00:00:00";
    }



    // ??????????????????????? HUD Updates ?????????????????????????????

    private void UpdateRecIndicator(CamcorderMode mode)
    {
        bool isRec = mode == CamcorderMode.Recording;
        bool isPrep = mode == CamcorderMode.Preparing;

        recDot.gameObject.SetActive(isRec || isPrep);
        recText.gameObject.SetActive(isRec || isPrep);

        if (isRec)
        {
            float blink = (Mathf.Sin(Time.time * recBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            recDot.color = new Color(recColor.r, recColor.g, recColor.b, blink);
            recText.color = new Color(recColor.r, recColor.g, recColor.b, blink);
            recText.text = "REC";
        }
        else if (isPrep)
        {
            recDot.color = prepColor;
            float blink = Mathf.Sin(Time.time * 6f) > 0 ? 1f : 0.2f;
            recText.color = new Color(prepColor.r, prepColor.g, prepColor.b, blink);
            recText.text = "STBY";
        }
    }

    private void UpdateTimecode(CamcorderMode mode)
    {
        if (mode == CamcorderMode.Recording)
        {
            recordingTime += Time.deltaTime;
            wasRecording = true;
        }
        else if (wasRecording && mode == CamcorderMode.Idle)
        {
            recordingTime = 0f;
            wasRecording = false;
        }

        int total = Mathf.FloorToInt(recordingTime);
        int hours = total / 3600;
        int minutes = (total % 3600) / 60;
        int seconds = total % 60;

        string sep = (mode == CamcorderMode.Recording && Mathf.Sin(Time.time * 4f) > 0) ? ":" : " ";
        timecodeText.text = $"{hours:00}{sep}{minutes:00}{sep}{seconds:00}";
        timecodeText.color = (mode == CamcorderMode.Recording) ? hudColor : hudColor;
    }



    // ??????????????????????? Utilities ???????????????????????????????

    private GameObject MakeRect(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return go;
    }

    private Font GetMonoFont()
    {
        return Font.CreateDynamicFontFromOSFont("Consolas", 16)
            ?? Font.CreateDynamicFontFromOSFont("Courier New", 16)
            ?? Font.CreateDynamicFontFromOSFont("Arial", 16);
    }
}