using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class NodeStyle
{
    static NodeStyle defaultInstance;

    public float Width;
    public float Height;
    public GUIStyle style;
    public GUIStyle initialStateStyle;
    public GUIStyle selectedStyle;
    public GUIStyle initialStateStyleSelected;

    public static NodeStyle Default()
    {
        if (defaultInstance == null)
        {
            CreateDefault();
        }
        return defaultInstance;
    }

    private static void CreateDefault()
    {
        defaultInstance = new NodeStyle();
        defaultInstance.style = new GUIStyle();
        defaultInstance.style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        defaultInstance.style.alignment = TextAnchor.MiddleCenter;
        defaultInstance.style.border = new RectOffset(12, 12, 12, 12);

        defaultInstance.initialStateStyle = new GUIStyle();
        defaultInstance.initialStateStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
        defaultInstance.initialStateStyle.alignment = TextAnchor.MiddleCenter;
        defaultInstance.initialStateStyle.border = new RectOffset(12, 12, 12, 12);

        defaultInstance.selectedStyle = new GUIStyle();
        defaultInstance.selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        defaultInstance.selectedStyle.alignment = TextAnchor.MiddleCenter;
        defaultInstance.selectedStyle.border = new RectOffset(12, 12, 12, 12);


        defaultInstance.initialStateStyleSelected = new GUIStyle();
        defaultInstance.initialStateStyleSelected.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D;
        defaultInstance.initialStateStyleSelected.alignment = TextAnchor.MiddleCenter;
        defaultInstance.initialStateStyleSelected.border = new RectOffset(12, 12, 12, 12);

        defaultInstance.Width = 100f;
        defaultInstance.Height = 50f;

    }
}
