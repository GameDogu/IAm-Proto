using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HierarchyHightlightsSettings))]
public class HierarchyHightlightsSettingsEditor : Editor
{
    bool change = false;
    public override void OnInspectorGUI()
    {
        ShowEditorGUI();
    }

    public void ShowEditorGUI()
    {
        change = false;

        HierarchyHightlightsSettings tar = (HierarchyHightlightsSettings)target;
        var oldMode = tar.Mode;
        tar.Mode = (HierarchyHightlightsSettings.ColoringMode)EditorGUILayout.EnumPopup("HighlightMode", tar.Mode);

        if (oldMode != tar.Mode)
        {
            change = true;
        }

        switch (tar.Mode)
        {
            case HierarchyHightlightsSettings.ColoringMode.Component:
                DrawComponentBasedColoringInspector(tar);
                break;
            case HierarchyHightlightsSettings.ColoringMode.Layer:
                DrawLayerbasedColoringInspector(tar);
                break;
            case HierarchyHightlightsSettings.ColoringMode.Tag:
                DrawTagBasedColoringInspector(tar);
                break;
            default:
                break;
        }

        if (GUILayout.Button("Save") || change)
        {
            Save(tar);
        }
    }

    void Save(HierarchyHightlightsSettings tar)
    {
        EditorUtility.SetDirty(tar);
        AssetDatabase.SaveAssets();
        EditorApplication.RepaintHierarchyWindow();
    }

    void DrawLayerbasedColoringInspector(HierarchyHightlightsSettings tar)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Layer Based Coloring", new GUIStyle() { fontStyle = FontStyle.Bold });
        HandleList(ref tar.layerSettings);
    }

    void DrawTagBasedColoringInspector(HierarchyHightlightsSettings tar)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tag Based Coloring",new GUIStyle() { fontStyle = FontStyle.Bold});

        HandleList(ref tar.tagSettings);
    }

    void DrawComponentBasedColoringInspector(HierarchyHightlightsSettings tar)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Component Based Coloring", new GUIStyle() { fontStyle = FontStyle.Bold });

        HandleList(ref tar.componentSetting);
    }

    void HandleList<T>(ref List<T> l) where T : HighlightSetting, new()
    {
        if (l == null)
            l = new List<T>();

        int currentEntries = l.Count;
        int newEntries = EditorGUILayout.DelayedIntField("Entries", currentEntries);
        if (newEntries < 0)
            newEntries = currentEntries;

        if (currentEntries != newEntries)
            change = true;

        if (currentEntries > newEntries)
        {
            //we remove
            int diff = currentEntries - newEntries;

            for (int i = 0; i < diff; i++)
            {
                l.RemoveAt(l.Count - 1);
            }
        }
        else if (currentEntries < newEntries)
        {
            int diff = newEntries - currentEntries;
            for (int i = 0; i < diff; i++)
            {
                l.Add(new T());
            }
        }

        for (int i = 0; i < l.Count; i++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent()
            {
                text = $"Element {i}",
            }, new GUIStyle() { fontStyle = FontStyle.Bold });
            EditorGUI.indentLevel += 1;
            change |= l[i].DrawInspectorGUI();
            EditorGUI.indentLevel -= 1;
        }

    }

}
