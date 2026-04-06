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

    [Header("Style")]
    [SerializeField] private Color hudColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color recColor = new Color(1f, 0.08f, 0.08f, 0.95f);
    [SerializeField] private Color prepColor = new Color(1f, 0.85f, 0f, 0.90f);

    [Header("Blink")]
    [SerializeField] private float recBlinkSpeed = 2.5f;

    
    private Material _glMat;

    
    private Canvas hudCanvas;
    private Image recDot;
    private Text recText;
    private Text timecodeText;
    

    private float recordingTime = 0f;
    private bool wasRecording = false;


    private void Awake()
    {
        CreateGLMaterial();
        CreateHUD();
    }

    private void Update()
    {
        bool camActive = camcorderCamera != null && camcorderCamera.gameObject.activeInHierarchy;
        if (hudCanvas != null) hudCanvas.gameObject.SetActive(camActive);
        if (!camActive) return;

        var mode = GetCurrentMode();
        UpdateRecIndicator(mode);
        UpdateTimecode(mode);
    }


    private void OnRenderObject()
    {
        if (camcorderCamera == null || !camcorderCamera.gameObject.activeInHierarchy) return;
        if (Camera.current == camcorderCamera) return;

        var mode = GetCurrentMode();
        Color col = GetFrameColor(mode);

     
        var camT = camcorderCamera.transform;
        float halfH = frustumDistance * Mathf.Tan(camcorderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfW = halfH * camcorderCamera.aspect;

        Vector3 c = camT.position + camT.forward * frustumDistance;
        Vector3 tl = c + camT.up * halfH - camT.right * halfW;
        Vector3 tr = c + camT.up * halfH + camT.right * halfW;
        Vector3 bl = c - camT.up * halfH - camT.right * halfW;
        Vector3 br = c - camT.up * halfH + camT.right * halfW;

        _glMat.SetPass(0);

        GL.PushMatrix();
        GL.Begin(GL.LINES);
        GL.Color(col);

        // Top
        GL.Vertex(tl); GL.Vertex(tr);
        // Right
        GL.Vertex(tr); GL.Vertex(br);
        // Bottom
        GL.Vertex(br); GL.Vertex(bl);
        // Left
        GL.Vertex(bl); GL.Vertex(tl);

        GL.End();
        GL.PopMatrix();
    }

    private void OnDestroy()
    {
        if (_glMat != null) Destroy(_glMat);
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

    private Color GetFrameColor(CamcorderMode mode)
    {
        return mode switch
        {
            CamcorderMode.Recording => recColor,
            CamcorderMode.Preparing => prepColor,
            _ => new Color(hudColor.r, hudColor.g, hudColor.b, 0.5f)
        };
    }

 

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
        var container = MakeRect("RecIndicator", parent,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(30, -30), new Vector2(220, 50));

        var dotGO = MakeRect("RecDot", container.transform,
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(14, 14));
        recDot = dotGO.AddComponent<Image>();
        recDot.color = recColor;

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