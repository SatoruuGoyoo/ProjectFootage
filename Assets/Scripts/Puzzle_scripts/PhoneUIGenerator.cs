//using UnityEngine;
//using UnityEngine.UI;
//using UnityEditor;
//using TMPro;

//public static class PhoneUIGenerator
//{
//    [MenuItem("Tools/Generate Phone UI")]
//    public static void Generate()
//    {
//        // ── Canvas ────────────────────────────────────────────────
//        var canvasGo = new GameObject("PhoneCanvas");
//        var canvas = canvasGo.AddComponent<Canvas>();
//        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//        canvas.sortingOrder = 10;

//        var scaler = canvasGo.AddComponent<CanvasScaler>();
//        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//        scaler.referenceResolution = new Vector2(1920, 1080);

//        canvasGo.AddComponent<GraphicRaycaster>();

//        // ── Panel raíz ────────────────────────────────────────────
//        var panel = MakeRect("Phone_Panel", canvasGo.transform, new Vector2(280, 440));
//        SetColor(panel, new Color(0.10f, 0.10f, 0.10f));
//        SetAnchors(panel, Vector2.one * 0.5f, Vector2.one * 0.5f);

//        // ── Pantalla ──────────────────────────────────────────────
//        var screen = MakeStretchRect("Screen", panel.transform,
//            new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.95f));
//        SetColor(screen, new Color(0.05f, 0.07f, 0.09f));

//        var displayText = MakeText("DisplayText", screen.transform, "_", 28,
//            TextAlignmentOptions.BottomRight, new Color(0.89f, 0.91f, 0.94f));
//        StretchFull(displayText, new Vector2(8, 8), new Vector2(-8, -8));

//        // ── Grid ──────────────────────────────────────────────────
//        var grid = MakeStretchRect("Grid", panel.transform,
//            new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.73f));
//        SetColor(grid, Color.clear);

//        var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
//        layout.cellSize = new Vector2(72f, 56f);
//        layout.spacing = new Vector2(8f, 8f);
//        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//        layout.constraintCount = 3;
//        layout.childAlignment = TextAnchor.UpperCenter;

//        string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "0", "#" };
//        foreach (var label in labels)
//            MakeDialButton(label, grid.transform);

//        // ── Botón borrar ──────────────────────────────────────────
//        var clearBtn = MakeStretchRect("Btn_Clear", panel.transform,
//            new Vector2(0.05f, 0.08f), new Vector2(0.45f, 0.17f));
//        SetColor(clearBtn, new Color(0.16f, 0.16f, 0.16f));
//        clearBtn.gameObject.AddComponent<Button>();
//        MakeText("Label", clearBtn.transform, "⌫ borrar", 14,
//            TextAlignmentOptions.Center, new Color(0.89f, 0.91f, 0.94f));

//        // ── Botón llamar ──────────────────────────────────────────
//        var callBtn = MakeStretchRect("Btn_Call", panel.transform,
//            new Vector2(0.50f, 0.08f), new Vector2(0.95f, 0.17f));
//        SetColor(callBtn, new Color(0.10f, 0.48f, 0.24f));
//        callBtn.gameObject.AddComponent<Button>();
//        MakeText("Label", callBtn.transform, "Llamar", 14,
//            TextAlignmentOptions.Center, new Color(0.89f, 0.91f, 0.94f));

//        // ── Registrar Undo y seleccionar ──────────────────────────
//        Undo.RegisterCreatedObjectUndo(canvasGo, "Generate Phone UI");
//        Selection.activeGameObject = canvasGo;

//        Debug.Log("[PhoneUIGenerator] UI generada. Ahora podés editarla libremente.");
//    }

//    // ──────────────────────────────────────────────────────────────
//    // Helpers
//    // ──────────────────────────────────────────────────────────────

//    private static void MakeDialButton(string label, Transform parent)
//    {
//        var rt = MakeRect($"Btn_{label}", parent, Vector2.zero);
//        SetColor(rt, new Color(0.16f, 0.16f, 0.16f));
//        rt.gameObject.AddComponent<Button>();
//        MakeText("Label", rt.transform, label, 22,
//            TextAlignmentOptions.Center, new Color(0.89f, 0.91f, 0.94f));
//    }

//    private static RectTransform MakeRect(string name, Transform parent, Vector2 size)
//    {
//        var go = new GameObject(name);
//        go.transform.SetParent(parent, false);
//        var rt = go.AddComponent<RectTransform>();
//        rt.sizeDelta = size;
//        go.AddComponent<Image>();
//        return rt;
//    }

//    private static RectTransform MakeStretchRect(string name, Transform parent,
//        Vector2 anchorMin, Vector2 anchorMax)
//    {
//        var go = new GameObject(name);
//        go.transform.SetParent(parent, false);
//        var rt = go.AddComponent<RectTransform>();
//        rt.anchorMin = anchorMin;
//        rt.anchorMax = anchorMax;
//        rt.offsetMin = rt.offsetMax = Vector2.zero;
//        go.AddComponent<Image>();
//        return rt;
//    }

//    private static RectTransform MakeText(string name, Transform parent,
//        string text, float size, TextAlignmentOptions alignment, Color color)
//    {
//        var go = new GameObject(name);
//        go.transform.SetParent(parent, false);
//        var tmp = go.AddComponent<TextMeshProUGUI>();
//        tmp.text = text;
//        tmp.fontSize = size;
//        tmp.color = color;
//        tmp.alignment = alignment;
//        var rt = go.GetComponent<RectTransform>();
//        StretchFull(rt);
//        return rt;
//    }

//    private static void StretchFull(RectTransform rt,
//        Vector2 offsetMin = default, Vector2 offsetMax = default)
//    {
//        rt.anchorMin = Vector2.zero;
//        rt.anchorMax = Vector2.one;
//        rt.offsetMin = offsetMin;
//        rt.offsetMax = offsetMax;
//    }

//    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
//    {
//        rt.anchorMin = min;
//        rt.anchorMax = max;
//    }

//    private static void SetColor(RectTransform rt, Color color)
//    {
//        var img = rt.GetComponent<Image>();
//        if (img) img.color = color;
//    }
//}