#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One-click builder for the camcorder recordings menu, gallery-grid style
// (like the reference: slots laid out in columns, filling left-to-right,
// top-to-bottom). Builds the functional hierarchy (chrome / grid view /
// playback view), the recording slots with selection borders, and wires the
// CamcorderMenuUI references. Navigation stays linear for now — this pass is
// visual only. Drop in the real sprites and tweak spacing after building.
public class CamcorderMenuSetupTool : EditorWindow
{
    private int slotCount = 5;
    private int columns = 2;
    private Vector2 cellSize = new Vector2(300f, 190f);
    private Vector2 cellSpacing = new Vector2(24f, 24f);
    private Canvas targetCanvas;
    private bool worldSpaceCanvas = false;

    [MenuItem("Tools/Camcorder/Setup Recordings Menu")]
    private static void Open()
    {
        var win = GetWindow<CamcorderMenuSetupTool>("Camcorder Menu");
        win.minSize = new Vector2(340, 340);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Camcorder Recordings Menu (Gallery Grid)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
        worldSpaceCanvas = EditorGUILayout.Toggle("New Canvas = World Space", worldSpaceCanvas);
        EditorGUILayout.HelpBox("Use your existing camcorder canvas if you have one, so the render setup is kept. If empty, a new canvas is created.", MessageType.None);

        EditorGUILayout.Space();
        slotCount = EditorGUILayout.IntField("Slot Count", slotCount);
        columns = Mathf.Max(1, EditorGUILayout.IntField("Columns", columns));
        cellSize = EditorGUILayout.Vector2Field("Cell Size", cellSize);
        cellSpacing = EditorGUILayout.Vector2Field("Cell Spacing", cellSpacing);

        EditorGUILayout.Space();
        if (GUILayout.Button("Build Camcorder Menu", GUILayout.Height(36)))
            Build();
    }

    private void Build()
    {
        Canvas canvas = targetCanvas != null ? targetCanvas : CreateCanvas();

        var root = NewUI("CamcorderMenu", canvas.transform);
        Stretch(root);

        // Chrome (always visible while the menu is open)
        var chrome = NewUI("Chrome_Panel", root.transform);
        Stretch(chrome);
        var titleGO = NewText("Title", chrome.transform, "----- RECORDINGS -----", 22, TextAlignmentOptions.Top);
        AnchorTop(titleGO.GetComponent<RectTransform>(), 24f);

        // Grid view (gallery of recordings)
        var gridView = NewUI("Grid_Panel", root.transform);
        Stretch(gridView);

        var recordingsPanel = NewUI("RecordingsPanel", gridView.transform);
        var recRT = recordingsPanel.GetComponent<RectTransform>();
        recRT.anchorMin = Vector2.zero;
        recRT.anchorMax = Vector2.one;
        recRT.offsetMin = new Vector2(180f, 40f);
        recRT.offsetMax = new Vector2(-40f, -90f);

        var grid = recordingsPanel.AddComponent<GridLayoutGroup>();
        grid.cellSize = cellSize;
        grid.spacing = cellSpacing;
        grid.padding = new RectOffset(40, 40, 40, 40);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        var slots = new Image[slotCount];
        var borders = new Image[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            var cellGO = NewUI("Slot_" + (i + 1), recordingsPanel.transform);

            var borderGO = NewUI("Border", cellGO.transform);
            var borderRT = borderGO.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = new Vector2(-5f, -5f);
            borderRT.offsetMax = new Vector2(5f, 5f);
            var borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.13f, 0.45f, 1f, 1f);
            borderImg.raycastTarget = false;
            borderGO.SetActive(false);
            borders[i] = borderImg;

            var thumbGO = NewUI("Thumbnail", cellGO.transform);
            var thumbRT = thumbGO.GetComponent<RectTransform>();
            thumbRT.anchorMin = Vector2.zero;
            thumbRT.anchorMax = Vector2.one;
            thumbRT.offsetMin = Vector2.zero;
            thumbRT.offsetMax = Vector2.zero;
            var thumbImg = thumbGO.AddComponent<Image>();
            thumbImg.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            thumbImg.raycastTarget = false;
            thumbImg.preserveAspect = true;
            slots[i] = thumbImg;
        }

        var noRecGO = NewUI("NoRecordingPanel", gridView.transform);
        Stretch(noRecGO);
        var noRecText = NewText("NoRecordingText", noRecGO.transform, "NO RECORDINGS", 26, TextAlignmentOptions.Center);
        Stretch(noRecText);

        // Playback view (full-screen video)
        var playback = NewUI("PlaybackPanel", root.transform);
        Stretch(playback);

        var rawGO = NewUI("Playback_img", playback.transform);
        Stretch(rawGO);
        var raw = rawGO.AddComponent<RawImage>();
        raw.raycastTarget = false;

        var noDataGO = NewUI("NoData_OVR", playback.transform);
        Stretch(noDataGO);
        var noDataText = NewText("NoDataText", noDataGO.transform, "NO DATA", 26, TextAlignmentOptions.Center);
        Stretch(noDataText);
        noDataGO.SetActive(false);

        playback.SetActive(false);

        // Component + wiring
        var ui = root.GetComponent<CamcorderMenuUI>();
        if (ui == null) ui = root.AddComponent<CamcorderMenuUI>();

        var so = new SerializedObject(ui);
        so.FindProperty("noRecordingPanel").objectReferenceValue = noRecGO;
        so.FindProperty("noRecordingText").objectReferenceValue = noRecText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("recordingsPanel").objectReferenceValue = recordingsPanel;

        var slotsProp = so.FindProperty("recordingSlots");
        slotsProp.arraySize = slotCount;
        for (int i = 0; i < slotCount; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];

        var bordersProp = so.FindProperty("selectionBorders");
        bordersProp.arraySize = slotCount;
        for (int i = 0; i < slotCount; i++)
            bordersProp.GetArrayElementAtIndex(i).objectReferenceValue = borders[i];

        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(root, "Build Camcorder Menu");
        Selection.activeGameObject = root;

        EditorUtility.DisplayDialog("Camcorder Menu",
            "Gallery-grid camcorder menu built and wired.\n\n" +
            "Next steps:\n" +
            "- Add CamcorderMenuController + sibling components to CamcorderMenu.\n" +
            "- In the controller's Views: assign Grid_Panel and PlaybackPanel.\n" +
            "- In VideoPlayback: assign Playback_img (RawImage) and NoData_OVR.\n" +
            "- Drop in the real sprites and adjust cell size / insets.\n\n" +
            "Note: navigation is still linear (W/S). 2D grid navigation is a separate controller change.",
            "OK");
    }

    private Canvas CreateCanvas()
    {
        var go = new GameObject("CamcorderMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>();
        c.renderMode = worldSpaceCanvas ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        return c;
    }

    private GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private GameObject NewText(string name, Transform parent, string text, float size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return go;
    }

    private void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void AnchorTop(RectTransform rt, float topOffset)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, -topOffset - 40f);
        rt.offsetMax = new Vector2(0f, -topOffset);
    }
}
#endif
