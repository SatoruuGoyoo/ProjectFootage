//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.UI;

//public class InventoryUISetupTool : EditorWindow
//{
//    private enum Corner { LowerRight, LowerLeft, UpperRight, UpperLeft }
//    private enum LayoutKind { Horizontal, Grid }

//    private Corner corner = Corner.LowerRight;
//    private LayoutKind layoutKind = LayoutKind.Horizontal;
//    private float iconSize = 96f;
//    private float spacing = 12f;
//    private float margin = 24f;
//    private Canvas targetCanvas;

//    [MenuItem("Tools/Inventory/Setup Inventory UI")]
//    private static void Open()
//    {
//        var win = GetWindow<InventoryUISetupTool>("Inventory UI");
//        win.minSize = new Vector2(320, 260);
//    }

//    private void OnGUI()
//    {
//        EditorGUILayout.LabelField("Inventory UI Setup", EditorStyles.boldLabel);
//        EditorGUILayout.Space();

//        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);
//        EditorGUILayout.HelpBox("If empty, a new Screen Space Overlay canvas is created.", MessageType.None);

//        EditorGUILayout.Space();
//        corner = (Corner)EditorGUILayout.EnumPopup("Corner", corner);
//        layoutKind = (LayoutKind)EditorGUILayout.EnumPopup("Layout", layoutKind);
//        iconSize = EditorGUILayout.FloatField("Icon Size", iconSize);
//        spacing = EditorGUILayout.FloatField("Spacing", spacing);
//        margin = EditorGUILayout.FloatField("Margin", margin);

//        EditorGUILayout.Space();
//        if (GUILayout.Button("Build Inventory UI", GUILayout.Height(36)))
//            Build();
//    }

//    private void Build()
//    {
//        Canvas canvas = targetCanvas != null ? targetCanvas : CreateCanvas();

//        var panelGO = new GameObject("InventoryPanel", typeof(RectTransform));
//        Undo.RegisterCreatedObjectUndo(panelGO, "Create Inventory UI");
//        panelGO.transform.SetParent(canvas.transform, false);

//        var panelRT = panelGO.GetComponent<RectTransform>();
//        ApplyCorner(panelRT);

//        if (layoutKind == LayoutKind.Horizontal)
//        {
//            var h = panelGO.AddComponent<HorizontalLayoutGroup>();
//            h.spacing = spacing;
//            h.childAlignment = AnchorForCorner();
//            h.childControlWidth = false;
//            h.childControlHeight = false;
//            h.childForceExpandWidth = false;
//            h.childForceExpandHeight = false;
//        }
//        else
//        {
//            var g = panelGO.AddComponent<GridLayoutGroup>();
//            g.cellSize = new Vector2(iconSize, iconSize);
//            g.spacing = new Vector2(spacing, spacing);
//            g.childAlignment = AnchorForCorner();
//        }

//        var fitter = panelGO.AddComponent<ContentSizeFitter>();
//        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
//        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

//        GameObject iconPrefab = CreateIconPrefab();

//        var uiGO = new GameObject("InventoryUI");
//        Undo.RegisterCreatedObjectUndo(uiGO, "Create Inventory UI");
//        uiGO.transform.SetParent(canvas.transform, false);
//        var inv = uiGO.AddComponent<InventoryUI>();

//        var so = new SerializedObject(inv);
//        so.FindProperty("iconContainer").objectReferenceValue = panelRT;
//        so.FindProperty("iconPrefab").objectReferenceValue = iconPrefab.GetComponent<Image>();
//        so.ApplyModifiedProperties();

//        Selection.activeGameObject = panelGO;
//        EditorUtility.DisplayDialog("Inventory UI",
//            "Inventory UI built.\n\nAssign the generated Icon Prefab in your project if you want to reuse it, and make sure each Collectible references an ItemData with an icon.",
//            "OK");
//    }

//    private Canvas CreateCanvas()
//    {
//        var canvasGO = new GameObject("InventoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
//        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Inventory Canvas");
//        var c = canvasGO.GetComponent<Canvas>();
//        c.renderMode = RenderMode.ScreenSpaceOverlay;
//        var scaler = canvasGO.GetComponent<CanvasScaler>();
//        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//        scaler.referenceResolution = new Vector2(1920, 1080);
//        return c;
//    }

//    private GameObject CreateIconPrefab()
//    {
//        var iconGO = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
//        var rt = iconGO.GetComponent<RectTransform>();
//        rt.sizeDelta = new Vector2(iconSize, iconSize);
//        var img = iconGO.GetComponent<Image>();
//        img.preserveAspect = true;
//        img.raycastTarget = false;

//        string dir = "Assets/Prefabs";
//        if (!AssetDatabase.IsValidFolder(dir))
//            AssetDatabase.CreateFolder("Assets", "Prefabs");

//        string path = AssetDatabase.GenerateUniqueAssetPath(dir + "/ItemIcon.prefab");
//        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(iconGO, path);
//        DestroyImmediate(iconGO);
//        return prefab;
//    }

//    private void ApplyCorner(RectTransform rt)
//    {
//        Vector2 anchor = corner switch
//        {
//            Corner.LowerRight => new Vector2(1, 0),
//            Corner.LowerLeft => new Vector2(0, 0),
//            Corner.UpperRight => new Vector2(1, 1),
//            Corner.UpperLeft => new Vector2(0, 1),
//            _ => new Vector2(1, 0)
//        };
//        rt.anchorMin = anchor;
//        rt.anchorMax = anchor;
//        rt.pivot = anchor;

//        float x = anchor.x == 1 ? -margin : margin;
//        float y = anchor.y == 1 ? -margin : margin;
//        rt.anchoredPosition = new Vector2(x, y);
//    }

//    private TextAnchor AnchorForCorner() => corner switch
//    {
//        Corner.LowerRight => TextAnchor.LowerRight,
//        Corner.LowerLeft => TextAnchor.LowerLeft,
//        Corner.UpperRight => TextAnchor.UpperRight,
//        Corner.UpperLeft => TextAnchor.UpperLeft,
//        _ => TextAnchor.LowerRight
//    };
//}
//#endif
