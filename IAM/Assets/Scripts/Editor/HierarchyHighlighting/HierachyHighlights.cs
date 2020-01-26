using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
public class HierachyHighlights
{
    private static Color backGroundColor = new Color(.76f,.76f,.76f);
    public static HierarchyHightlightsSettings Settings { get; protected set; }

    static HierachyHighlights()
    {
        FindSettings();
        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyHighlight_OnGUI;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyHighlight_OnGUI;
    }

    private static void FindSettings()
    {
        string[] assGUIDs = AssetDatabase.FindAssets("HighlightSettings");
        if (assGUIDs != null)
        {
            string path = AssetDatabase.GUIDToAssetPath(assGUIDs[0]);
            Settings = AssetDatabase.LoadAssetAtPath<HierarchyHightlightsSettings>(path);
            if (Settings == null)
            {
                Debug.LogError("Highlighting: failed to load asset");
                return;
            }
        }
        else
        {
            Debug.LogError("Highlighting: No asset of name found");
            return;
        }
    }

    static void HierarchyHighlight_OnGUI(int inSelectionID, Rect inSelectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(inSelectionID) as GameObject;
        bool objIsSelected = Selection.instanceIDs.Contains(inSelectionID);

        if (obj != null)
        {
            Settings.CheckIfHighlighted(obj, inSelectionRect, objIsSelected);
        }
    }

    public static void HighlightComponent(System.Type t)
    {
       throw new NotImplementedException();
    }

    public static void HighlightInspectorGUI(UnityEngine.Object target)
    {
        HighlightComponent(target.GetType());
    }

    public static void ChangeSettings(HierarchyHightlightsSettings specialSettings)
    {
        Settings = specialSettings;
    }

    public static  void UseNormalSettings()
    {
        FindSettings();
    }

}
