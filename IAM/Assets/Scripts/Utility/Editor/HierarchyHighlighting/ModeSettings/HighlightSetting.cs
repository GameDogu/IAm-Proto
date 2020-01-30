using UnityEngine;
using UnityEditor;

public abstract class HighlightSetting
{
    [SerializeField] public HighlightSettingInfo Settings;
    public abstract void HightlightObjectOnMetConditon(GameObject obj, Rect inRect, bool isSelected);
    public abstract bool CheckHighlightCondition(GameObject obj);
    bool foldout = false;

    public HighlightSetting()
    {
        Settings = new HighlightSettingInfo();
    }

    public virtual bool DrawInspectorGUI()
    {
        foldout = EditorGUILayout.Foldout(foldout, "Settings");
        if (foldout)
            return Settings.DrawInspectorGUI();
        return false;
    }

}
