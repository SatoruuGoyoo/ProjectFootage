using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SequenceRunner))]
public class SequenceRunnerEditor : Editor
{
    private SequenceRunner runner;
    private List<Type> stepTypes;
    private List<SequenceStep> cachedSteps = new List<SequenceStep>();
    private ReorderableList reorderableList;
    private bool showAddDropdown = false;
    private string searchFilter = "";

    private void OnEnable()
    {
        runner = (SequenceRunner)target;
        RefreshStepTypes();
        RefreshSteps();
        BuildList();
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
    }

    private void BuildList()
    {
        reorderableList = new ReorderableList(cachedSteps, typeof(SequenceStep), true, true, false, false);

        reorderableList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, $"Steps ({cachedSteps.Count})", EditorStyles.boldLabel);

        reorderableList.elementHeightCallback = GetElementHeight;
        reorderableList.drawElementCallback = DrawElement;
        reorderableList.drawElementBackgroundCallback = DrawElementBackground;

        reorderableList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
        {
            SequenceStep step = cachedSteps[newIndex];
            step.transform.SetSiblingIndex(GetSiblingIndexForListIndex(newIndex));
            EditorUtility.SetDirty(runner.gameObject);
        };
    }

    private int GetSiblingIndexForListIndex(int listIndex)
    {
        if (listIndex <= 0) return 0;
        if (listIndex >= cachedSteps.Count) return runner.transform.childCount - 1;
        return cachedSteps[listIndex - 1].transform.GetSiblingIndex() + 1;
    }

    private float GetElementHeight(int index)
    {
        if (index >= cachedSteps.Count || cachedSteps[index] == null)
            return EditorGUIUtility.singleLineHeight + 8;

        float height = EditorGUIUtility.singleLineHeight + 10; // header

        var so = new SerializedObject(cachedSteps[index]);
        var prop = so.GetIterator();
        bool entered = prop.NextVisible(true);
        if (entered)
        {
            while (prop.NextVisible(false))
                height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        return height + 16; // padding bottom
    }

    private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (Event.current.type != EventType.Repaint) return;

        Color bg = index % 2 == 0
            ? new Color(0.22f, 0.22f, 0.22f, 1f)
            : new Color(0.19f, 0.19f, 0.19f, 1f);

        if (isActive) bg = new Color(0.17f, 0.36f, 0.52f, 1f);

        EditorGUI.DrawRect(rect, bg);
    }

    private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (index >= cachedSteps.Count || cachedSteps[index] == null) return;

        SequenceStep step = cachedSteps[index];

        float x = rect.x + 4;
        float y = rect.y + 6;
        float w = rect.width - 8;

        // Header row
        Rect labelRect = new Rect(x, y, w - 28, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, $"{index}   {step.GetType().Name}", EditorStyles.boldLabel);

        GUIStyle deleteStyle = new GUIStyle(EditorStyles.miniButton);
        deleteStyle.normal.textColor = new Color(0.9f, 0.35f, 0.35f);
        Rect deleteRect = new Rect(rect.xMax - 28, y, 24, EditorGUIUtility.singleLineHeight);

        if (GUI.Button(deleteRect, "✕", deleteStyle))
        {
            if (EditorUtility.DisplayDialog("Eliminar step", $"¿Eliminar {step.GetType().Name}?", "Sí", "No"))
            {
                Undo.DestroyObjectImmediate(step.gameObject);
                RefreshSteps();
                BuildList();
                GUIUtility.ExitGUI();
                return;
            }
        }

        y += EditorGUIUtility.singleLineHeight + 8;

        // Fields
        var so = new SerializedObject(step);
        so.Update();
        var prop = so.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(false))
        {
            float propH = EditorGUI.GetPropertyHeight(prop, true);
            EditorGUI.PropertyField(new Rect(x + 8, y, w - 8, propH), prop, true);
            y += propH + EditorGUIUtility.standardVerticalSpacing;
        }
        so.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);

        // Sync si cambió algo externo
        var current = runner.GetStepsFromChildren();
        if (current.Length != cachedSteps.Count || !current.SequenceEqual(cachedSteps))
        {
            RefreshSteps();
            BuildList();
        }

        if (cachedSteps.Count == 0)
        {
            EditorGUILayout.HelpBox("No hay steps. Usá '+ Add Step' para agregar uno.", MessageType.Info);
        }
        else
        {
            reorderableList.DoLayoutList();
        }

        EditorGUILayout.Space(6);
        DrawAddStepSection();
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
        BuildList();
    }
}