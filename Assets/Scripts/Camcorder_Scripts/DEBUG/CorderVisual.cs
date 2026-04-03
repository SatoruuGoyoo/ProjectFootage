using UnityEngine;
using UnityEngine.UI;

public class CorderVisual : MonoBehaviour
{
    [Header("References")]
    public Camera camcorderCamera;
    public CamcorderRecorder recorder;
    public CamcorderController controller;

    [Header("Frame")]
    [SerializeField] private float frustumDistance = 5f;
    [SerializeField] private Material frameMaterial;       // opcional — si no asignás usa Sprites/Default
    [SerializeField] private float lineWidth = 0.03f;

    [Header("Style")]
    [SerializeField] private Color hudColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color recColor = new Color(1f, 0.08f, 0.08f, 0.95f);
    [SerializeField] private Color prepColor = new Color(1f, 0.85f, 0f, 0.9f);

    [Header("Blink")]
    [SerializeField] private float recBlinkSpeed = 2.5f;

    private LineRenderer[] edges = new LineRenderer[4];

    // UI
    private Canvas hudCanvas;
    private Image recDot;
    private Text recText;
    private Text timecodeText;

    // State
    private float recordingTime = 0f;
    private bool wasRecording = false;

    // ?????????????????????????????????????????????

    private void Awake()
    {
        CreateEdges();
        CreateHUD();
    }

    private void Update()
    {
        bool camActive = camcorderCamera != null && camcorderCamera.gameObject.activeInHierarchy;
        SetEdgesActive(camActive);
        if (hudCanvas != null) hudCanvas.gameObject.SetActive(camActive);
        if (!camActive) return;

        var mode = GetCurrentMode();
        UpdateFrame(mode);
        UpdateRecIndicator(mode);
        UpdateTimecode(mode);
    }

    // ?? State ?????????????????????????????????????

    private CamcorderMode GetCurrentMode()
    {
        if (controller != null) return controller.CurrentCamMode;
        if (recorder != null && recorder.IsRecording) return CamcorderMode.Recording;
        return CamcorderMode.Idle;
    }

    // ?? Frame ?????????????????????????????????????

    private void CreateEdges()
    {
        // Sprites/Default: soporta vertex colors (startColor/endColor del LineRenderer)
        // y respeta el depth buffer por defecto — las paredes tapan el marco.
        // Si el usuario asigna su propio material, se usa ese.
        Material mat = frameMaterial != null
            ? frameMaterial
            : new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };

        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject($"FrameEdge_{i}");
            go.transform.SetParent(transform);

            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.numCapVertices = 4;
            lr.numCornerVertices = 4;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.material = mat;

            edges[i] = lr;
        }
    }

    private void SetEdgesActive(bool active)
    {
        foreach (var e in edges)
            if (e != null) e.gameObject.SetActive(active);
    }

    private void UpdateFrame(CamcorderMode mode)
    {
        var camT = camcorderCamera.transform;
        float halfH = frustumDistance * Mathf.Tan(camcorderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfW = halfH * camcorderCamera.aspect;

        Vector3 c = camT.position + camT.forward * frustumDistance;
        Vector3 tl = c + camT.up * halfH - camT.right * halfW;
        Vector3 tr = c + camT.up * halfH + camT.right * halfW;
        Vector3 bl = c - camT.up * halfH - camT.right * halfW;
        Vector3 br = c - camT.up * halfH + camT.right * halfW;

        edges[0].SetPositions(new[] { tl, tr });
        edges[1].SetPositions(new[] { tr, br });
        edges[2].SetPositions(new[] { br, bl });
        edges[3].SetPositions(new[] { bl, tl });

        Color col = mode switch
        {
            CamcorderMode.Recording => recColor,
            CamcorderMode.Preparing => prepColor,
            _ => new Color(hudColor.r, hudColor.g, hudColor.b, 0.5f)
        };

        foreach (var e in edges)
        {
            e.startColor = col;
            e.endColor = col;
        }
    }

    // ?? HUD ???????????????????????????????????????

    private void CreateHUD()
    {
        var canvasGO = new GameObject("CorderVisual_Canvas");
        canvasGO.transform.SetParent(transform);

        hudCanvas = canvasGO.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 90;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var grp = canvasGO.AddComponent<CanvasGroup>();
        grp.interactable = false;
        grp.blocksRaycasts = false;

        CreateRecIndicator(canvasGO.transform);
        hudCanvas.gameObject.SetActive(false);
    }

    private void CreateRecIndicator(Transform parent)
    {
        // Contenedor anclado top-left
        var container = MakeRect("RecIndicator", parent,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(30, -30), new Vector2(220, 50));

        // Fila superior: [dot] [REC/STBY]
        // Dot — pivot left-center, no se superpone con el texto
        var dotGO = MakeRect("RecDot", container.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(14, 14));
        recDot = dotGO.AddComponent<Image>();
        recDot.color = recColor;

        // Texto REC/STBY — empieza justo después del dot (14 + 6 = 20)
        var recGO = MakeRect("RecText", container.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(22, 2), new Vector2(80, 20));
        recText = recGO.AddComponent<Text>();
        recText.font = GetMonoFont();
        recText.fontSize = 20;
        recText.fontStyle = FontStyle.Bold;
        recText.color = recColor;
        recText.alignment = TextAnchor.MiddleLeft;
        recText.text = "REC";

        // Timecode — debajo de la fila superior
        var tcGO = MakeRect("Timecode", container.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, -22), new Vector2(200, 18));
        timecodeText = tcGO.AddComponent<Text>();
        timecodeText.font = GetMonoFont();
        timecodeText.fontSize = 15;
        timecodeText.color = hudColor;
        timecodeText.alignment = TextAnchor.MiddleLeft;
        timecodeText.text = "00:00:00";
    }

    private void UpdateRecIndicator(CamcorderMode mode)
    {
        bool isRec = mode == CamcorderMode.Recording;
        bool isPrep = mode == CamcorderMode.Preparing;

        recDot.gameObject.SetActive(isRec || isPrep);
        recText.gameObject.SetActive(isRec || isPrep);

        if (isRec)
        {
            float b = (Mathf.Sin(Time.time * recBlinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            recDot.color = new Color(recColor.r, recColor.g, recColor.b, b);
            recText.color = new Color(recColor.r, recColor.g, recColor.b, b);
            recText.text = "REC";
        }
        else if (isPrep)
        {
            recDot.color = prepColor;
            float b = Mathf.Sin(Time.time * 6f) > 0 ? 1f : 0.2f;
            recText.color = new Color(prepColor.r, prepColor.g, prepColor.b, b);
            recText.text = "STBY";
        }
    }

    private void UpdateTimecode(CamcorderMode mode)
    {
        if (mode == CamcorderMode.Recording) { recordingTime += Time.deltaTime; wasRecording = true; }
        else if (wasRecording && mode == CamcorderMode.Idle) { recordingTime = 0f; wasRecording = false; }

        int tot = Mathf.FloorToInt(recordingTime);
        string sep = (mode == CamcorderMode.Recording && Mathf.Sin(Time.time * 4f) > 0) ? ":" : " ";
        timecodeText.text = $"{tot / 3600:00}{sep}{(tot % 3600) / 60:00}{sep}{tot % 60:00}";
    }

    // ?? Utils ?????????????????????????????????????

    private GameObject MakeRect(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
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

    private Font GetMonoFont()
        => Font.CreateDynamicFontFromOSFont("Consolas", 16)
        ?? Font.CreateDynamicFontFromOSFont("Courier New", 16)
        ?? Font.CreateDynamicFontFromOSFont("Arial", 16);
}