using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Camcorder viewfinder HUD — estilo Handycam 90s.
/// Reemplaza el gizmo debug por un overlay que parece un viewfinder real.
/// Se activa cuando la camcorder está levantada, cambia según el estado.
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
    [SerializeField] private Color dimColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Blink")]
    [SerializeField] private float recBlinkSpeed = 2.5f;

    // UI Elements
    private Canvas hudCanvas;
    private CanvasGroup hudGroup;

    // REC indicator (top-left)
    private Image recDot;
    private Text recText;
    private Text timecodeText;

    // Focus brackets (center)
    private RectTransform[] bracketCorners = new RectTransform[4]; // TL, TR, BL, BR

    // Battery (top-right)
    private Image batteryBody;
    private Image batteryTip;
    private Image[] batteryBars = new Image[4];

    // Bottom bar
    private Text dateText;
    private Text modeText;

    // Scanline overlay
    private Image scanlineOverlay;
    private Material scanlineMaterial;

    // GL Drawing (frustum + tilt)
    private Material glMaterial;

    // State
    private float recordingTime = 0f;
    private bool wasRecording = false;

    private void Awake()
    {
        CreateGLMaterial();
        CreateHUD();
    }

    private void OnDestroy()
    {
        if (scanlineMaterial != null)
            DestroyImmediate(scanlineMaterial);
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
        UpdateFocusBrackets(mode);
        UpdateBattery();
        UpdateBottomBar(mode);
        UpdateScanlines(mode);
    }

    #region — State —

    private CamcorderMode GetCurrentMode()
    {
        if (controller != null)
            return controller.CurrentCamMode;

        if (recorder != null && recorder.IsRecording)
            return CamcorderMode.Recording;

        return CamcorderMode.Idle;
    }

    #endregion

    #region — GL Drawing (Frustum + Tilt in 3D) —

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

        if (showFrustum)
            DrawFrustum(color, mode);

        if (showTiltLimits)
            DrawTiltLimits(color, mode);
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
        Vector3 forward = camT.forward * frustumDistance;
        Vector3 up = camT.up * halfHeight;
        Vector3 right = camT.right * halfWidth;

        Vector3 center = origin + forward;
        Vector3 tl = center + up - right;
        Vector3 tr = center + up + right;
        Vector3 bl = center - up - right;
        Vector3 br = center - up + right;

        glMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);

        // ——— Relleno semitransparente del far plane ———
        GL.Begin(GL.QUADS);
        Color fillColor = color;
        fillColor.a = (mode == CamcorderMode.Recording) ? 0.06f : 0.03f;
        GL.Color(fillColor);
        GL.Vertex(tl); GL.Vertex(tr); GL.Vertex(br); GL.Vertex(bl);
        GL.End();

        // ——— Bordes del frustum ———
        GL.Begin(GL.LINES);

        // Líneas desde el origen a cada esquina (más tenues)
        Color edgeColor = color;
        edgeColor.a = color.a * 0.3f;
        GL.Color(edgeColor);
        GLLine(origin, tl);
        GLLine(origin, tr);
        GLLine(origin, bl);
        GLLine(origin, br);

        // Rectángulo del far plane (más visible)
        GL.Color(color);
        GLLine(tl, tr);
        GLLine(tr, br);
        GLLine(br, bl);
        GLLine(bl, tl);

        // ——— Crosshair en el far plane ———
        Color crossColor = color;
        crossColor.a = color.a * 0.6f;
        GL.Color(crossColor);

        float crossSize = halfHeight * 0.15f;
        Vector3 midTop = center + camT.up * crossSize;
        Vector3 midBot = center - camT.up * crossSize;
        Vector3 midLeft = center - camT.right * crossSize;
        Vector3 midRight = center + camT.right * crossSize;
        GLLine(midTop, midBot);
        GLLine(midLeft, midRight);

        // ——— Brackets en las esquinas del far plane ———
        float bracketLen = halfHeight * 0.2f;
        GL.Color(color);
        // TL
        GLLine(tl, tl + camT.right * bracketLen);
        GLLine(tl, tl - camT.up * bracketLen);
        // TR
        GLLine(tr, tr - camT.right * bracketLen);
        GLLine(tr, tr - camT.up * bracketLen);
        // BL
        GLLine(bl, bl + camT.right * bracketLen);
        GLLine(bl, bl + camT.up * bracketLen);
        // BR
        GLLine(br, br - camT.right * bracketLen);
        GLLine(br, br + camT.up * bracketLen);

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

        // Arco de rango de tilt (tenue)
        Color arcColor = color;
        arcColor.a = color.a * 0.25f;
        GL.Color(arcColor);
        DrawArc(pivot, minAngle, maxAngle, tiltArcRadius, arcSegments);

        // Líneas límite
        Color limitColor = color;
        limitColor.a = color.a * 0.6f;
        GL.Color(limitColor);
        Vector3 minDir = GetTiltDirection(pivot, minAngle);
        Vector3 maxDir = GetTiltDirection(pivot, maxAngle);
        GLLine(pivot.position, pivot.position + minDir * tiltArcRadius * 1.2f);
        GLLine(pivot.position, pivot.position + maxDir * tiltArcRadius * 1.2f);

        // Línea del ángulo actual (blanca, más visible)
        GL.Color(new Color(1f, 1f, 1f, 0.9f));
        Vector3 currentDir = camcorderCamera.transform.forward;
        GLLine(pivot.position, pivot.position + currentDir * tiltArcRadius * 0.9f);

        GL.End();
        GL.PopMatrix();
    }

    private void DrawArc(Transform pivot, float fromAngle, float toAngle, float radius, int segments)
    {
        Vector3 prevPoint = pivot.position + GetTiltDirection(pivot, fromAngle) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(fromAngle, toAngle, t);
            Vector3 point = pivot.position + GetTiltDirection(pivot, angle) * radius;
            GLLine(prevPoint, point);
            prevPoint = point;
        }
    }

    private Vector3 GetTiltDirection(Transform pivot, float angle)
    {
        return Quaternion.AngleAxis(angle, pivot.right) * pivot.forward;
    }

    private void GLLine(Vector3 a, Vector3 b)
    {
        GL.Vertex(a);
        GL.Vertex(b);
    }

    #endregion

    #region — HUD Creation —

    private void CreateHUD()
    {
        // Root Canvas
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
        CreateFocusBrackets(canvasGO.transform);
        CreateBattery(canvasGO.transform);
        CreateBottomBar(canvasGO.transform);
        CreateScanlineOverlay(canvasGO.transform);

        hudCanvas.gameObject.SetActive(false);
    }

    // ——— REC INDICATOR (top-left) ———

    private void CreateRecIndicator(Transform parent)
    {
        // Container
        GameObject container = MakeRect("RecIndicator", parent,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(40, -35), new Vector2(280, 60));

        // REC dot — círculo rojo
        GameObject dotGO = MakeRect("RecDot", container.transform,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(0, 0), new Vector2(18, 18));
        recDot = dotGO.AddComponent<Image>();
        recDot.color = recColor;

        // REC text
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

    // ——— FOCUS BRACKETS (center) ———

    private void CreateFocusBrackets(Transform parent)
    {
        float bracketSize = 70f;
        float thickness = 2f;
        float armLength = 22f;
        float centerOffset = 60f; // Distancia del centro

        // 4 esquinas: TL, TR, BL, BR
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-centerOffset, centerOffset),   // TL
            new Vector2(centerOffset, centerOffset),    // TR
            new Vector2(-centerOffset, -centerOffset),  // BL
            new Vector2(centerOffset, -centerOffset)    // BR
        };

        // Dirección de los brazos para cada esquina
        Vector2[][] armDirs = new Vector2[][]
        {
            new Vector2[] { Vector2.right, Vector2.down },    // TL: brazo derecha + abajo
            new Vector2[] { Vector2.left, Vector2.down },     // TR: brazo izquierda + abajo
            new Vector2[] { Vector2.right, Vector2.up },      // BL: brazo derecha + arriba
            new Vector2[] { Vector2.left, Vector2.up }        // BR: brazo izquierda + arriba
        };

        string[] names = { "BracketTL", "BracketTR", "BracketBL", "BracketBR" };

        for (int i = 0; i < 4; i++)
        {
            GameObject corner = MakeRect(names[i], parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                positions[i], new Vector2(bracketSize, bracketSize));

            bracketCorners[i] = corner.GetComponent<RectTransform>();

            // Brazo horizontal
            GameObject armH = MakeRect("ArmH", corner.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                armDirs[i][0] * (armLength * 0.5f),
                new Vector2(armLength, thickness));
            Image imgH = armH.AddComponent<Image>();
            imgH.color = hudColor;

            // Brazo vertical
            GameObject armV = MakeRect("ArmV", corner.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                armDirs[i][1] * (armLength * 0.5f),
                new Vector2(thickness, armLength));
            Image imgV = armV.AddComponent<Image>();
            imgV.color = hudColor;
        }

        // Punto central pequeño
        GameObject centerDot = MakeRect("CenterDot", parent,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(4, 4));
        Image dotImg = centerDot.AddComponent<Image>();
        dotImg.color = dimColor;
    }

    // ——— BATTERY (top-right) ———

    private void CreateBattery(Transform parent)
    {
        GameObject container = MakeRect("Battery", parent,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-40, -38), new Vector2(48, 22));

        // Body outline
        GameObject bodyGO = MakeRect("Body", container.transform,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        bodyGO.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        bodyGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // Borde exterior
        Image bodyOutline = bodyGO.AddComponent<Image>();
        bodyOutline.color = hudColor;
        batteryBody = bodyOutline;

        // Interior negro
        GameObject innerGO = MakeRect("Inner", bodyGO.transform,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        innerGO.GetComponent<RectTransform>().offsetMin = new Vector2(2, 2);
        innerGO.GetComponent<RectTransform>().offsetMax = new Vector2(-2, -2);
        Image innerImg = innerGO.AddComponent<Image>();
        innerImg.color = new Color(0, 0, 0, 0.7f);

        // Barras de carga
        float barWidth = 8f;
        float barGap = 2f;
        float startX = 4f;
        for (int i = 0; i < 4; i++)
        {
            GameObject barGO = MakeRect($"Bar{i}", bodyGO.transform,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(startX + i * (barWidth + barGap), 0),
                new Vector2(barWidth, 14));
            batteryBars[i] = barGO.AddComponent<Image>();
            batteryBars[i].color = hudColor;
        }

        // Tip (la puntita de la batería)
        GameObject tipGO = MakeRect("Tip", container.transform,
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 0.5f),
            new Vector2(1, 0), new Vector2(4, 10));
        batteryTip = tipGO.AddComponent<Image>();
        batteryTip.color = hudColor;
    }

    // ——— BOTTOM BAR ———

    private void CreateBottomBar(Transform parent)
    {
        // Date (bottom-left)
        GameObject dateGO = MakeRect("DateStamp", parent,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(40, 30), new Vector2(220, 22));
        dateText = dateGO.AddComponent<Text>();
        dateText.font = GetMonoFont();
        dateText.fontSize = 15;
        dateText.color = dimColor;
        dateText.alignment = TextAnchor.MiddleLeft;
        // Fecha fija estilo VHS
        dateText.text = "1997.08.14  PM 11:42";

        // Mode (bottom-right)
        GameObject modeGO = MakeRect("ModeText", parent,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-40, 30), new Vector2(100, 22));
        modeText = modeGO.AddComponent<Text>();
        modeText.font = GetMonoFont();
        modeText.fontSize = 15;
        modeText.color = dimColor;
        modeText.alignment = TextAnchor.MiddleRight;
        modeText.text = "SP";
    }

    // ——— SCANLINE OVERLAY ———

    private void CreateScanlineOverlay(Transform parent)
    {
        // Overlay de scanlines finas sobre toda la pantalla
        // Usa un shader simple que dibuja líneas horizontales
        GameObject scanGO = MakeRect("Scanlines", parent,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        scanGO.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        scanGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        scanlineOverlay = scanGO.AddComponent<Image>();

        // Crear material con shader UI/Default modificado para scanlines
        Shader shader = Shader.Find("UI/VHSScanlines");
        if (shader != null)
        {
            scanlineMaterial = new Material(shader);
            scanlineMaterial.SetFloat("_LineCount", 400f);
            scanlineMaterial.SetFloat("_LineAlpha", 0.08f);
            scanlineOverlay.material = scanlineMaterial;
            scanlineOverlay.color = new Color(1, 1, 1, 0.15f);
        }
        else
        {
            // Fallback: sin scanlines si no encuentra el shader
            scanlineOverlay.color = Color.clear;
        }

        scanlineOverlay.raycastTarget = false;
    }

    #endregion

    #region — HUD Updates —

    private void UpdateRecIndicator(CamcorderMode mode)
    {
        bool isRec = mode == CamcorderMode.Recording;
        bool isPrep = mode == CamcorderMode.Preparing;

        recDot.gameObject.SetActive(isRec || isPrep);
        recText.gameObject.SetActive(isRec || isPrep);

        if (isRec)
        {
            // Parpadeo del dot y texto REC
            float blink = (Mathf.Sin(Time.time * recBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            recDot.color = new Color(recColor.r, recColor.g, recColor.b, blink);
            recText.color = new Color(recColor.r, recColor.g, recColor.b, blink);
            recText.text = "REC";
        }
        else if (isPrep)
        {
            recDot.color = prepColor;
            recText.color = prepColor;
            // STBY parpadea rápido
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
        else
        {
            if (wasRecording && mode == CamcorderMode.Idle)
            {
                recordingTime = 0f;
                wasRecording = false;
            }
        }

        int totalSeconds = Mathf.FloorToInt(recordingTime);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        // Parpadeo de los dos puntos durante grabación
        string separator = (mode == CamcorderMode.Recording && Mathf.Sin(Time.time * 4f) > 0) ? ":" : " ";
        timecodeText.text = $"{hours:00}{separator}{minutes:00}{separator}{seconds:00}";

        timecodeText.color = (mode == CamcorderMode.Recording) ? hudColor : dimColor;
    }

    private void UpdateFocusBrackets(CamcorderMode mode)
    {
        Color targetColor;

        switch (mode)
        {
            case CamcorderMode.Recording:
                targetColor = recColor;
                break;
            case CamcorderMode.Preparing:
                targetColor = prepColor;
                break;
            default:
                targetColor = hudColor;
                break;
        }

        // Efecto breathing sutil en idle
        if (mode == CamcorderMode.Idle)
        {
            float breath = Mathf.Lerp(0.5f, 0.85f, (Mathf.Sin(Time.time * 1.5f) + 1f) * 0.5f);
            targetColor.a = breath;
        }

        foreach (RectTransform corner in bracketCorners)
        {
            Image[] arms = corner.GetComponentsInChildren<Image>();
            foreach (Image arm in arms)
                arm.color = targetColor;
        }
    }

    private void UpdateBattery()
    {
        // Simular batería que baja lentamente (puramente cosmético)
        float fakeLevel = Mathf.PingPong(Time.time * 0.02f, 1f);
        fakeLevel = 1f - fakeLevel; // Empieza llena
        int activeBars = Mathf.CeilToInt(fakeLevel * 4f);
        activeBars = Mathf.Clamp(activeBars, 1, 4);

        for (int i = 0; i < batteryBars.Length; i++)
        {
            batteryBars[i].color = (i < activeBars) ? hudColor : new Color(hudColor.r, hudColor.g, hudColor.b, 0.15f);
        }
    }

    private void UpdateBottomBar(CamcorderMode mode)
    {
        // El modo cambia a LP durante grabación (detalle cosmético)
        modeText.text = (mode == CamcorderMode.Recording) ? "LP" : "SP";
        modeText.color = (mode == CamcorderMode.Recording) ? hudColor : dimColor;
    }

    private void UpdateScanlines(CamcorderMode mode)
    {
        if (scanlineMaterial == null) return;

        // Las scanlines se intensifican un poco durante grabación
        float alpha = (mode == CamcorderMode.Recording) ? 0.12f : 0.06f;
        scanlineMaterial.SetFloat("_LineAlpha", alpha);
    }

    #endregion

    #region — Utilities —

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
        // Intentar fuente monoespaciada, fallback a Arial
        Font font = Font.CreateDynamicFontFromOSFont("Consolas", 16);
        if (font == null)
            font = Font.CreateDynamicFontFromOSFont("Courier New", 16);
        if (font == null)
            font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        return font;
    }

    #endregion
}