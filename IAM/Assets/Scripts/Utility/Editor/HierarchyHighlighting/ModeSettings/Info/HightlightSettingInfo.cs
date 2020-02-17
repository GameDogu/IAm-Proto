using UnityEngine;
using UnityEditor;

[System.Serializable]
public class HighlightSettingInfo
{
    static readonly Color defaultBGColor = new Color(.76f, .76f, .76f, 1f);
    [SerializeField] public Color backgroundColor = new Color(.76f,.76f,.76f,1f);
    [SerializeField] public Color textColor = Color.black;
    [SerializeField] public FontStyle Style = FontStyle.Normal;

    public HighlightSettingInfo()
    {
        backgroundColor = new Color(.76f, .76f, .76f, 1f);
        textColor = Color.black;
        Style = FontStyle.Normal;
    }

    public void Draw(GameObject obj, Rect inSelectionRect, bool objIsSelected)
    {
        DrawBackgroundRect(inSelectionRect, objIsSelected);

        DrawIcon(obj, inSelectionRect, objIsSelected);

        DrawLabel(obj, inSelectionRect, objIsSelected);
    }

    private int GetParentCount(GameObject obj)
    {
        var t = obj.transform;
        int count = 0;

        while (t.parent != null)
        {
            count++;
            t = t.parent;
        }

        return count;
    }

    public bool DrawInspectorGUI()
    {
        bool change = false;
        Color oldC = backgroundColor;
        backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);

        if (!oldC.Equals(backgroundColor))
        {
            change = true;
        }
        oldC = textColor;
        textColor = EditorGUILayout.ColorField("Text Color", textColor);

        if (!oldC.Equals(textColor))
            change = true;

        var oldS = Style;
        Style = (FontStyle)EditorGUILayout.EnumFlagsField("Font Style", Style);

        if (oldS != Style)
            change = true;
        return change;
    }

    public void DrawLabel(GameObject obj, Rect inSelectionRect, bool objIsSelected)
    {
        Color tC = objIsSelected ? Color.white : textColor;
        var content = GetContent(obj);
        Rect labelRect = new Rect(inSelectionRect);

        //magic numbers don't touch
        int parentCount = GetParentCount(obj);
        float mul = 0.25f * Mathf.Pow(0.25f, parentCount);
        labelRect.x += ((content.image.width - 2)*mul) + (14 * parentCount);
        if (parentCount == 0)
        {
            labelRect.x += 3;
        }
        labelRect.y += 1;

        EditorGUI.LabelField(labelRect, obj.name, new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = tC },
            fontStyle = this.Style
        }
        );
    }

    GUIContent GetContent(GameObject obj)
    {
        return EditorGUIUtility.ObjectContent(obj, obj.GetType());
    }

    public void DrawIcon(GameObject obj, Rect inSelectionRect, bool objIsSelected)
    {
        var iconRect = new Rect(inSelectionRect);
        var content = GetContent(obj);

        iconRect.x -= 2f;
        iconRect.y -= 1f;
        iconRect.size *= 1.15f;

        GUI.Label(iconRect, content.image);
    }

    public void DrawBackgroundRect(Rect inSelectionRect, bool objIsSelected)
    {
        Color bgColor = (objIsSelected) ? GUI.skin.settings.selectionColor : this.backgroundColor;

        if (objIsSelected)
        {
            //draw neutral in back to hide any transparency and have overlapping text from it
            EditorGUI.DrawRect(inSelectionRect, backgroundColor);
        }
        EditorGUI.DrawRect(inSelectionRect, bgColor);
    }
}