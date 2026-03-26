using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime visual debug para el sistema de Camcorder.
/// Dibuja frustum de la cámara, límites de tilt e indicador REC/HOLD.
/// Todo visible en Build.
/// 
/// REQUIERE: Agregar a CamcorderController la propiedad pública:
///     public CamcorderMode CurrentCamMode => currentCamMode;
/// </summary>
public class CorderVisual : MonoBehaviour
{
    [Header("References")]
    public Camera camcorderCamera;
    public CamcorderRecorder recorder;
    public CamcorderMotor motor;
    public CamcorderController controller;

    [Header("Frustum")]
    public bool showFrustum = true;
    [SerializeField] private float frustumDistance = 5f;

    [Header("Tilt Limits")]
    public bool showTiltLimits = true;
    [SerializeField] private float tiltArcRadius = 1.5f;
    [SerializeField] private int arcSegments = 20;

    [Header("REC Indicator")]
    public bool showRecIndicator = true;
    [SerializeField] private float recDotSize = 14f;
    [SerializeField] private float recBlinkSpeed = 2f;
    [SerializeField] private Vector2 recOffset = new Vector2(20f, 20f);

    [Header("State Colors")]
    public Color idleColor = new Color(0f, 0.85f, 0.85f, 0.7f);
    public Color preparingColor = new Color(1f, 0.9f, 0f, 0.85f);
    public Color recordingColor = new Color(1f, 0.1f, 0.1f, 0.9f);

    // GL Drawing
    private Material glMaterial;

    // Runtime UI
    private Canvas debugCanvas;
    private Image recDotImage;
    private Text recLabel;

    private void Awake()
    {
        CreateGLMaterial();
        CreateDebugUI();
    }

    private void OnDestroy()
    {
        if (glMaterial != null)
            DestroyImmediate(glMaterial);
    }

    private void Update()
    {
        bool camActive = camcorderCamera != null && camcorderCamera.gameObject.activeInHierarchy;
        CamcorderMode mode = GetCurrentMode();

        UpdateDebugUI(camActive, mode);
    }

    // Se ejecuta después del render de cada cámara — dibuja las líneas GL
    // Filtra para NO dibujar cuando renderiza la camcorder (evita que las líneas se graben)
    private void OnRenderObject()
    {
        if (camcorderCamera == null || !camcorderCamera.gameObject.activeInHierarchy) return;

        // No dibujar GL en la cámara de la camcorder — solo en la cámara principal
        if (Camera.current == camcorderCamera) return;

        CamcorderMode mode = GetCurrentMode();
        Color color = GetStateColor(mode);

        if (showFrustum)
            DrawFrustum(color);

        if (showTiltLimits)
            DrawTiltLimits(color);
    }

    #region ?? State ??

    private CamcorderMode GetCurrentMode()
    {
        // Usa la propiedad pública que hay que agregar al controller
        // Si no existe, fallback por IsRecording del recorder
        if (controller != null)
        {
            return controller.CurrentCamMode;
        }

        // Fallback: inferir estado desde el recorder
        if (recorder != null && recorder.IsRecording)
            return CamcorderMode.Recording;

        return CamcorderMode.Idle;
    }

    private Color GetStateColor(CamcorderMode mode)
    {
        switch (mode)
        {
            case CamcorderMode.Preparing: return preparingColor;
            case CamcorderMode.Recording: return recordingColor;
            default: return idleColor;
        }
    }

    #endregion

    #region ?? GL Drawing (Frustum + Tilt Limits) ??

    private void CreateGLMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        glMaterial = new Material(shader)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        glMaterial.SetInt("_ZWrite", 0);
        glMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
    }

    private void DrawFrustum(Color color)
    {
        Transform camT = camcorderCamera.transform;
        float fov = camcorderCamera.fieldOfView;
        float aspect = camcorderCamera.aspect;

        // Calcular esquinas del frustum a la distancia indicada
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
        GL.Begin(GL.LINES);
        GL.Color(color);

        // Líneas desde el origen a cada esquina
        GLLine(origin, tl);
        GLLine(origin, tr);
        GLLine(origin, bl);
        GLLine(origin, br);

        // Rectángulo del far plane
        GLLine(tl, tr);
        GLLine(tr, br);
        GLLine(br, bl);
        GLLine(bl, tl);

        // Cruz central en el far plane
        Color dimColor = color * 0.4f;
        dimColor.a = color.a * 0.5f;
        GL.Color(dimColor);
        Vector3 midTop = (tl + tr) * 0.5f;
        Vector3 midBot = (bl + br) * 0.5f;
        Vector3 midLeft = (tl + bl) * 0.5f;
        Vector3 midRight = (tr + br) * 0.5f;
        GLLine(midTop, midBot);
        GLLine(midLeft, midRight);

        GL.End();
        GL.PopMatrix();
    }

    private void DrawTiltLimits(Color color)
    {
        if (motor == null) return;

        // El pivote del tilt es el padre del camcorderCamera (o el propio transform del motor)
        Transform pivot = camcorderCamera.transform.parent != null
            ? camcorderCamera.transform.parent
            : camcorderCamera.transform;

        float minAngle = motor.tiltMinAngle;
        float maxAngle = motor.tiltMaxAngle;

        Color arcColor = color * 0.5f;
        arcColor.a = color.a * 0.6f;

        Color limitColor = color;
        limitColor.a = 1f;

        glMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);
        GL.Begin(GL.LINES);

        // Arco de rango de tilt
        GL.Color(arcColor);
        DrawArc(pivot, minAngle, maxAngle, tiltArcRadius, arcSegments);

        // Líneas límite (más visibles)
        GL.Color(limitColor);
        Vector3 minDir = GetTiltDirection(pivot, minAngle);
        Vector3 maxDir = GetTiltDirection(pivot, maxAngle);

        GLLine(pivot.position, pivot.position + minDir * tiltArcRadius * 1.2f);
        GLLine(pivot.position, pivot.position + maxDir * tiltArcRadius * 1.2f);

        // Línea del ángulo actual
        Color currentColor = Color.white;
        currentColor.a = 0.9f;
        GL.Color(currentColor);
        Vector3 currentDir = camcorderCamera.transform.forward;
        GLLine(pivot.position, pivot.position + currentDir * tiltArcRadius * 0.8f);

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
        // El tilt rota alrededor del eje X local del pivote
        // Ángulo negativo = mirar arriba, positivo = mirar abajo (como en CamcorderMotor)
        return Quaternion.AngleAxis(angle, pivot.right) * pivot.forward;
    }

    private void GLLine(Vector3 a, Vector3 b)
    {
        GL.Vertex(a);
        GL.Vertex(b);
    }

    #endregion

    #region ?? Runtime Debug UI ??

    private void CreateDebugUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("CorderVisual_Canvas");
        canvasGO.transform.SetParent(transform);
        debugCanvas = canvasGO.AddComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        debugCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // ?? REC Indicator ??
        GameObject recContainer = CreateUIElement("RecIndicator", canvasGO.transform);
        RectTransform recRect = recContainer.GetComponent<RectTransform>();
        recRect.anchorMin = new Vector2(1, 0); // Bottom-right
        recRect.anchorMax = new Vector2(1, 0);
        recRect.pivot = new Vector2(1, 0);
        recRect.anchoredPosition = new Vector2(-recOffset.x, recOffset.y);
        recRect.sizeDelta = new Vector2(80, 28);

        // REC Dot
        GameObject dotGO = CreateUIElement("Dot", recContainer.transform);
        recDotImage = dotGO.AddComponent<Image>();
        recDotImage.color = recordingColor;
        RectTransform dotRect = dotGO.GetComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(0, 0.5f);
        dotRect.anchorMax = new Vector2(0, 0.5f);
        dotRect.pivot = new Vector2(0, 0.5f);
        dotRect.anchoredPosition = new Vector2(0, 0);
        dotRect.sizeDelta = new Vector2(recDotSize, recDotSize);

        // REC Label
        GameObject labelGO = CreateUIElement("Label", recContainer.transform);
        recLabel = labelGO.AddComponent<Text>();
        recLabel.text = "REC";
        recLabel.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        recLabel.fontSize = 16;
        recLabel.fontStyle = FontStyle.Bold;
        recLabel.color = Color.white;
        recLabel.alignment = TextAnchor.MiddleLeft;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(recDotSize + 6, 0);
        labelRect.offsetMax = Vector2.zero;

        // Empezar oculto
        debugCanvas.gameObject.SetActive(false);
    }

    private void UpdateDebugUI(bool camActive, CamcorderMode mode)
    {
        if (debugCanvas == null) return;

        debugCanvas.gameObject.SetActive(camActive);
        if (!camActive) return;

        // REC indicator
        bool isRecording = mode == CamcorderMode.Recording;
        bool isPreparing = mode == CamcorderMode.Preparing;

        recDotImage.gameObject.transform.parent.gameObject.SetActive(
            showRecIndicator && (isRecording || isPreparing)
        );

        if (isRecording)
        {
            // Blink del dot
            float alpha = (Mathf.Sin(Time.time * recBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            recDotImage.color = new Color(recordingColor.r, recordingColor.g, recordingColor.b, alpha);
            recLabel.text = "REC";
            recLabel.color = Color.white;
        }
        else if (isPreparing)
        {
            recDotImage.color = preparingColor;
            recLabel.text = "HOLD";
            recLabel.color = preparingColor;
        }
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    #endregion
}