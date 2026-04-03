using UnityEngine;

[ExecuteAlways]
public class DebugControlsUI : MonoBehaviour
{
    [System.Serializable]
    public struct ControlEntry
    {
        public string key;
        public string description;
    }

    [Header("Style")]
    [SerializeField] private int fontSize = 16;
    [SerializeField] private float colKeyWidth = 210f;
    [SerializeField] private float colValWidth = 310f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color keyColor = Color.white;
    [SerializeField] private Color bgColor = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] private Color schemeColor = Color.yellow;

    [Header("Editor Preview")]
    [Tooltip("En editor sin Play, elegí qué scheme previsualizar.")]
    [SerializeField] private ControlSchemeManager.Scheme editorPreviewScheme;

    [Header("Tank Controls")]
    [SerializeField]
    private ControlEntry[] tankControls = new ControlEntry[]
    {
        new ControlEntry { key = "W/S",                    description = "Avanzar / Retroceder (exploración)" },
        new ControlEntry { key = "A/D",                    description = "Girar personaje (exploración)" },
        new ControlEntry { key = "Click Derecho",          description = "Sacar / ocultar camcorder" },
        new ControlEntry { key = "Mouse (cámara arriba)",  description = "Apuntar camcorder" },
        new ControlEntry { key = "Mantener Click Izq",     description = "Preparar y grabar" },
        new ControlEntry { key = "TAB",                    description = "Abrir / Cerrar Menú Camcorder" },
        new ControlEntry { key = "M",                      description = "Cambiar controles" },
    };

    [Header("Modern Controls")]
    [SerializeField]
    private ControlEntry[] modernControls = new ControlEntry[]
    {
        new ControlEntry { key = "WASD",                   description = "Moverse (relativo a cámara)" },
        new ControlEntry { key = "Click Derecho",          description = "Sacar / ocultar camcorder" },
        new ControlEntry { key = "Mouse (cámara arriba)",  description = "Apuntar camcorder + rotar" },
        new ControlEntry { key = "WASD (cámara arriba)",   description = "Moverse estilo shooter" },
        new ControlEntry { key = "Mantener Click Izq",     description = "Preparar y grabar" },
        new ControlEntry { key = "TAB",                    description = "Abrir / Cerrar Menú Camcorder" },
        new ControlEntry { key = "M",                      description = "Cambiar controles" },
    };


    private GUIStyle labelStyle;
    private GUIStyle keyStyle;
    private GUIStyle schemeStyle;
    private GUIStyle bgStyle;
    private Texture2D bgTex;
    private int lastFontSize;
    private Color lastBgColor;

    private void BuildStyles()
    {
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Normal,
            wordWrap = false,
        };
        labelStyle.normal.textColor = textColor;

        keyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            wordWrap = false,
        };
        keyStyle.normal.textColor = keyColor;

        schemeStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize + 4,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        schemeStyle.normal.textColor = schemeColor;

        if (bgTex == null) bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, bgColor);
        bgTex.Apply();

        bgStyle = new GUIStyle(GUI.skin.box);
        bgStyle.normal.background = bgTex;

        lastFontSize = fontSize;
        lastBgColor = bgColor;
    }

    private bool NeedsRebuild()
    {
        return labelStyle == null
            || lastFontSize != fontSize
            || lastBgColor != bgColor;
    }

    private void OnGUI()
    {
        if (NeedsRebuild()) BuildStyles();

   
        bool isTank;
        if (Application.isPlaying && ControlSchemeManager.Instance != null)
            isTank = ControlSchemeManager.Instance.CurrentScheme == ControlSchemeManager.Scheme.Tank;
        else
            isTank = editorPreviewScheme == ControlSchemeManager.Scheme.Tank;

        ControlEntry[] controls = isTank ? tankControls : modernControls;
        string schemeName = isTank ? "TANK" : "MODERN";

        if (controls == null || controls.Length == 0) return;

      
        float padding = 12f;
        float lineHeight = fontSize + 6f;
        float totalWidth = colKeyWidth + colValWidth + padding * 2;
        float headerHeight = fontSize + 14f;
        float totalHeight = headerHeight + (controls.Length * lineHeight) + padding * 2 + 8f;

        float x = (Screen.width - totalWidth) * 0.5f;
        float y = 8f;

       
        GUI.Box(new Rect(x, y, totalWidth, totalHeight), GUIContent.none, bgStyle);

       
        GUI.Label(
            new Rect(x, y + padding * 0.5f, totalWidth, headerHeight),
            $"[ {schemeName} ]",
            schemeStyle
        );

    
        float startY = y + padding + headerHeight;

    
        keyStyle.normal.textColor = keyColor;
        labelStyle.normal.textColor = textColor;
        schemeStyle.normal.textColor = schemeColor;

        for (int i = 0; i < controls.Length; i++)
        {
            float lineY = startY + i * lineHeight;

            GUI.Label(
                new Rect(x + padding, lineY, colKeyWidth, lineHeight),
                controls[i].key,
                keyStyle
            );

            GUI.Label(
                new Rect(x + padding + colKeyWidth, lineY, colValWidth, lineHeight),
                "=  " + controls[i].description,
                labelStyle
            );
        }
    }
}