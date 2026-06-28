using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SequenceRunner))]
public class SequenceRunnerEditor : Editor
{
    private SequenceRunner runner;
    private List<Type> stepTypes;
    private List<SequenceStep> cachedSteps = new List<SequenceStep>();
    private bool showAddDropdown = false;
    private string searchFilter = "";
    private Dictionary<int, bool> stepFoldouts = new Dictionary<int, bool>();
    private int draggingIndex = -1;
    private int dragTargetIndex = -1;
    private List<Rect> stepRects = new List<Rect>();
    private int dragControlId = -1;

    private static readonly Dictionary<string, Color> StepColors = new Dictionary<string, Color>
    {
        { "wait",       new Color(0.25f, 0.45f, 0.75f) },
        { "lock",       new Color(0.75f, 0.25f, 0.25f) },
        { "unlock",     new Color(0.75f, 0.25f, 0.25f) },
        { "block",      new Color(0.75f, 0.25f, 0.25f) },
        { "unblock",    new Color(0.75f, 0.25f, 0.25f) },
        { "playfmod",   new Color(0.25f, 0.65f, 0.35f) },
        { "cinematic",  new Color(0.55f, 0.25f, 0.75f) },
        { "door",       new Color(0.80f, 0.50f, 0.15f) },
        { "trigger",    new Color(0.80f, 0.50f, 0.15f) },
        { "gameobject", new Color(0.70f, 0.65f, 0.10f) },
        { "light",      new Color(0.70f, 0.65f, 0.10f) },
        { "collider",   new Color(0.70f, 0.65f, 0.10f) },
        { "player",     new Color(0.20f, 0.70f, 0.70f) },
        { "speak",      new Color(0.20f, 0.70f, 0.70f) },
        { "feedback",   new Color(0.20f, 0.70f, 0.70f) },
        { "show",       new Color(0.20f, 0.70f, 0.70f) },
    };

    private Color GetStepColor(SequenceStep step)
    {
        string name = step.GetType().Name.ToLower();
        foreach (var kv in StepColors)
            if (name.Contains(kv.Key)) return kv.Value;
        return new Color(0.35f, 0.35f, 0.35f);
    }

    private void OnEnable()
    {
        runner = (SequenceRunner)target;
        RefreshStepTypes();
        RefreshSteps();
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (Application.isPlaying && runner.IsRunning)
            Repaint();
    }

    private void RefreshStepTypes()
    {
        stepTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(SequenceStep)) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    private void RefreshSteps()
    {
        cachedSteps.Clear();
        foreach (Transform child in runner.transform)
        {
            var step = child.GetComponent<SequenceStep>();
            if (step != null) cachedSteps.Add(step);
        }
        stepRects = new List<Rect>(new Rect[cachedSteps.Count]);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);

        var current = runner.GetStepsFromChildren();
        if (current.Length != cachedSteps.Count || !current.SequenceEqual(cachedSteps))
            RefreshSteps();

        DrawStepsHeader();

        if (cachedSteps.Count == 0)
        {
            EditorGUILayout.HelpBox("No hay steps. Usá '+ Add Step' para agregar uno.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < cachedSteps.Count; i++)
                DrawStep(i);
        }

        ProcessDragEvents();

        EditorGUILayout.Space(8);
        DrawPlayButton();
        EditorGUILayout.Space(4);
        DrawAddStepSection();
    }

    private void DrawStepsHeader()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Steps ({cachedSteps.Count})", EditorStyles.boldLabel);
        if (GUILayout.Button("Colapsar todo", EditorStyles.miniButton, GUILayout.Width(90)))
            for (int i = 0; i < cachedSteps.Count; i++) stepFoldouts[i] = false;
        if (GUILayout.Button("Expandir todo", EditorStyles.miniButton, GUILayout.Width(90)))
            for (int i = 0; i < cachedSteps.Count; i++) stepFoldouts[i] = true;
        EditorGUILayout.EndHorizontal();

        Rect sep = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(sep, new Color(0.1f, 0.1f, 0.1f));
        EditorGUILayout.Space(2);
    }

    private void DrawStep(int index)
    {
        SequenceStep step = cachedSteps[index];
        if (step == null) return;

        bool isRunning = Application.isPlaying && runner.CurrentStepIndex == index;
        bool isCompleted = Application.isPlaying && runner.CurrentStepIndex > index;
        bool isDraggingThis = draggingIndex == index;

        Color stepColor = GetStepColor(step);
        Color bgColor = isDraggingThis
            ? new Color(0.28f, 0.28f, 0.28f)
            : (index % 2 == 0 ? new Color(0.21f, 0.21f, 0.21f) : new Color(0.18f, 0.18f, 0.18f));

        if (!stepFoldouts.ContainsKey(index)) stepFoldouts[index] = true;
        bool expanded = stepFoldouts[index];

        // Drop indicator arriba
        if (draggingIndex >= 0 && dragTargetIndex == index && index <= draggingIndex)
            DrawDropIndicator();

        Rect boxRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(boxRect, bgColor);

        // Barra color izquierda
        if (Event.current.type == EventType.Repaint)
        {
            Color barColor = isRunning ? Color.yellow : (isCompleted ? new Color(0.3f, 0.7f, 0.3f) : stepColor);
            EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.y, 4, boxRect.height), barColor);
        }

        // Guardar rect para hit test del drag
        if (Event.current.type == EventType.Repaint && index < stepRects.Count)
            stepRects[index] = boxRect;

        EditorGUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);

        // Drag handle — registra hotControl
        Rect handleRect = GUILayoutUtility.GetRect(20, EditorGUIUtility.singleLineHeight, GUILayout.Width(20));
        GUI.Label(handleRect, "☰", new GUIStyle(EditorStyles.label)
        {
            fontSize = 13,
            normal = { textColor = draggingIndex == index ? Color.white : new Color(0.55f, 0.55f, 0.55f) }
        });

        Event e = Event.current;
        if (e.type == EventType.MouseDown && handleRect.Contains(e.mousePosition))
        {
            dragControlId = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = dragControlId;
            draggingIndex = index;
            dragTargetIndex = index;
            e.Use();
        }

        GUILayout.Space(4);

        // Status indicator en Play Mode
        if (Application.isPlaying)
        {
            string icon = isRunning ? "▶" : (isCompleted ? "✓" : "○");
            Color iconColor = isRunning ? Color.yellow : (isCompleted ? new Color(0.3f, 0.7f, 0.3f) : new Color(0.4f, 0.4f, 0.4f));
            GUILayout.Label(icon, new GUIStyle(EditorStyles.label) { normal = { textColor = iconColor }, fontStyle = FontStyle.Bold }, GUILayout.Width(16));
        }

        // Foldout clickeable
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = stepColor * 1.4f } };
        string foldIcon = expanded ? "▾" : "▸";
        if (GUILayout.Button($"{foldIcon}  {index}  {step.GetType().Name}", labelStyle, GUILayout.ExpandWidth(true)))
            stepFoldouts[index] = !expanded;

        GUILayout.FlexibleSpace();

        // Botones
        GUI.enabled = index > 0;
        if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(22), GUILayout.Height(18)))
        { MoveStep(index, -1); GUIUtility.ExitGUI(); }

        GUI.enabled = index < cachedSteps.Count - 1;
        if (GUILayout.Button("▼", EditorStyles.miniButtonMid, GUILayout.Width(22), GUILayout.Height(18)))
        { MoveStep(index, 1); GUIUtility.ExitGUI(); }
        GUI.enabled = true;

        if (GUILayout.Button("⧉", EditorStyles.miniButtonMid, GUILayout.Width(22), GUILayout.Height(18)))
        { DuplicateStep(index); GUIUtility.ExitGUI(); }

        GUIStyle deleteStyle = new GUIStyle(EditorStyles.miniButtonRight);
        deleteStyle.normal.textColor = new Color(0.9f, 0.35f, 0.35f);
        if (GUILayout.Button("✕", deleteStyle, GUILayout.Width(22), GUILayout.Height(18)))
        {
            if (EditorUtility.DisplayDialog("Eliminar step", $"¿Eliminar {step.GetType().Name}?", "Sí", "No"))
            {
                Undo.DestroyObjectImmediate(step.gameObject);
                RefreshSteps();
                GUIUtility.ExitGUI();
                return;
            }
        }

        GUILayout.Space(4);
        EditorGUILayout.EndHorizontal();

        // Fields
        if (expanded)
        {
            EditorGUILayout.Space(4);
            var so = new SerializedObject(step);
            so.Update();
            var prop = so.GetIterator();
            prop.NextVisible(true);
            bool hasFields = false;
            while (prop.NextVisible(false))
            {
                hasFields = true;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(28);
                EditorGUILayout.PropertyField(prop, true);
                GUILayout.Space(4);
                EditorGUILayout.EndHorizontal();
            }
            so.ApplyModifiedProperties();

            if (!hasFields)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(28);
                EditorGUILayout.LabelField("Sin parámetros", new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.45f, 0.45f, 0.45f) } });
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();

        // Drop indicator abajo
        if (draggingIndex >= 0 && dragTargetIndex == index && index >= draggingIndex)
            DrawDropIndicator();

        EditorGUILayout.Space(1);
    }

    private void ProcessDragEvents()
    {
        if (draggingIndex < 0) return;

        Event e = Event.current;

        if (e.type == EventType.MouseDrag && GUIUtility.hotControl == dragControlId)
        {
            // Detectar sobre qué step está el mouse
            for (int i = 0; i < stepRects.Count; i++)
            {
                if (stepRects[i].Contains(e.mousePosition))
                {
                    dragTargetIndex = i;
                    break;
                }
            }
            Repaint();
            e.Use();
        }

        if (e.type == EventType.MouseUp && GUIUtility.hotControl == dragControlId)
        {
            GUIUtility.hotControl = 0;

            if (draggingIndex != dragTargetIndex && dragTargetIndex >= 0)
                MoveStepTo(draggingIndex, dragTargetIndex);

            draggingIndex = -1;
            dragTargetIndex = -1;
            dragControlId = -1;
            Repaint();
            e.Use();
        }
    }

    private void DrawDropIndicator()
    {
        Rect r = EditorGUILayout.GetControlRect(false, 2);
        EditorGUI.DrawRect(r, new Color(0.3f, 0.6f, 1f));
    }

    private void DrawPlayButton()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.enabled = Application.isPlaying && !runner.IsRunning;
        GUIStyle playStyle = new GUIStyle(EditorStyles.miniButton)
        {
            fontSize = 12,
            normal = { textColor = Application.isPlaying ? new Color(0.3f, 0.9f, 0.3f) : new Color(0.5f, 0.5f, 0.5f) }
        };
        if (GUILayout.Button("▶  Ejecutar secuencia", playStyle, GUILayout.Width(160), GUILayout.Height(24)))
            runner.StartSequence();
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void MoveStep(int index, int direction)
    {
        int newIndex = index + direction;
        if (newIndex < 0 || newIndex >= cachedSteps.Count) return;
        Undo.RecordObject(runner.transform, "Reorder Step");
        cachedSteps[index].transform.SetSiblingIndex(cachedSteps[newIndex].transform.GetSiblingIndex());
        EditorUtility.SetDirty(runner.gameObject);
        RefreshSteps();
    }

    private void MoveStepTo(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= cachedSteps.Count) return;
        if (toIndex < 0 || toIndex >= cachedSteps.Count) return;
        Undo.RecordObject(runner.transform, "Reorder Step");
        cachedSteps[fromIndex].transform.SetSiblingIndex(cachedSteps[toIndex].transform.GetSiblingIndex());
        EditorUtility.SetDirty(runner.gameObject);
        RefreshSteps();
    }

    private void DuplicateStep(int index)
    {
        SequenceStep original = cachedSteps[index];
        GameObject duplicate = Instantiate(original.gameObject);
        duplicate.name = original.gameObject.name;
        Undo.RegisterCreatedObjectUndo(duplicate, $"Duplicate {original.GetType().Name}");
        duplicate.transform.SetParent(runner.transform, false);
        duplicate.transform.SetSiblingIndex(original.transform.GetSiblingIndex() + 1);
        EditorUtility.SetDirty(runner);
        RefreshSteps();
    }

    private void DrawAddStepSection()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+ Add Step", GUILayout.Width(130), GUILayout.Height(24)))
            showAddDropdown = !showAddDropdown;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (!showAddDropdown) return;

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        searchFilter = EditorGUILayout.TextField("Buscar", searchFilter);
        EditorGUILayout.Space(2);

        var filtered = string.IsNullOrEmpty(searchFilter)
            ? stepTypes
            : stepTypes.Where(t => t.Name.ToLower().Contains(searchFilter.ToLower())).ToList();

        foreach (var type in filtered)
        {
            if (GUILayout.Button(type.Name, GUILayout.Height(20)))
            {
                AddStep(type);
                showAddDropdown = false;
                searchFilter = "";
                GUIUtility.ExitGUI();
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void AddStep(Type stepType)
    {
        var go = new GameObject(stepType.Name);
        Undo.RegisterCreatedObjectUndo(go, $"Add {stepType.Name}");
        go.transform.SetParent(runner.transform, false);
        Undo.AddComponent(go, stepType);
        EditorUtility.SetDirty(runner);
        RefreshSteps();
    }
}