using UnityEngine;
using UnityEditor;

[System.Serializable]
public class TagHightlightSetting : HighlightSetting
{
    [SerializeField] public string Name;

    public TagHightlightSetting():base()
    {

    }

    public override void HightlightObjectOnMetConditon(GameObject obj, Rect inRect, bool isSelected)
    {
        if(CheckHighlightCondition(obj))
        {
            Settings.Draw(obj, inRect, isSelected);
        }
    }

    public override bool DrawInspectorGUI()
    {
        var change = false;
        var oldName = Name;
        Name = EditorGUILayout.TextField("Tag: ",Name);
        if (Name != oldName)
            change = true;
        return change | base.DrawInspectorGUI();
    }

    public override bool CheckHighlightCondition(GameObject obj)
    {
        if (Name == null || Name == "")
            return false;
        return obj.CompareTag(Name);
    }
}
