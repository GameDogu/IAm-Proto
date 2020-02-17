using UnityEngine;

public static class EditorGUIDrawUtility
{
    static GUIStyle lineStyle;
    public static GUIStyle LineStyle
    {
        get
        {
            if (lineStyle == null)
                CreateLineStyle();
            return lineStyle;
        }
    }

    private static void CreateLineStyle()
    {
        lineStyle = new GUIStyle("Box");
        lineStyle.border.top = lineStyle.border.bottom = 1;
        lineStyle.margin.top = lineStyle.margin.bottom = 1;
        lineStyle.padding.top = lineStyle.padding.bottom = 1;
    }

    public static void DrawLine(float height = 1f)
    {
        GUILayout.Box(GUIContent.none, LineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(height));
    }

}
