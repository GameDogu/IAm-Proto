using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[System.Serializable]
public class LayerHighlightSetting : HighlightSetting
{
    [SerializeField] LayerMask mask;

    public LayerHighlightSetting():base()
    {

    }

    public override bool CheckHighlightCondition(GameObject obj)
    {
        return mask == (mask | 1 << obj.layer);
    }

    public override bool DrawInspectorGUI()
    {
        bool change = false;
        LayerMask old = mask;
        LayerMask tempMask = EditorGUILayout.MaskField("Mask",InternalEditorUtility.LayerMaskToConcatenatedLayersMask(mask), InternalEditorUtility.layers);

        mask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

        if (mask != old)
            change = true;

        return change|base.DrawInspectorGUI();
    }

    public override void HightlightObjectOnMetConditon(GameObject obj, Rect inRect, bool isSelected)
    {
        if (CheckHighlightCondition(obj))
        {
            Settings.Draw(obj, inRect, isSelected);
        }
    }
}
