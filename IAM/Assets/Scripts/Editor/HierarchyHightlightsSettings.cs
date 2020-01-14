using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Hierarchiy Highlights Settings")]
public class HierarchyHightlightsSettings : ScriptableObject
{
    public enum ColoringMode
    {
        Tag,
        Layer,
        Component,
        None
    }
    [SerializeField] public ColoringMode Mode;

    [SerializeField]public List<TagHightlightSetting> tagSettings;
    [SerializeField] public List<LayerHighlightSetting> layerSettings;
    [SerializeField] public List<ComponentHighlightSetting> componentSetting;

    public void CheckIfHighlighted(GameObject obj, Rect inRect, bool isSelected)
    {
        switch (Mode)
        {
            case ColoringMode.Component:
                CheckDrawComponents(obj, inRect, isSelected, componentSetting);
                break;
            case ColoringMode.Layer:
                Check(obj, inRect, isSelected, layerSettings);
                break;
            case ColoringMode.Tag:
                Check(obj, inRect, isSelected, tagSettings);
                break;
            default:
                break;
        }
    }

    void Check<T>(GameObject obj, Rect inRect, bool isSelected,List<T> settings) where T:HighlightSetting
    {
        if (settings == null)
            return;
        for (int i = 0; i < settings.Count; i++)
        {
            settings[i].HightlightObjectOnMetConditon(obj, inRect, isSelected);
        }
    }

    void CheckDrawComponents(GameObject obj, Rect inRect, bool isSelected, List<ComponentHighlightSetting> settings)
    {
        if (settings == null)
            return;
        var infosToUse = new List<HighlightSettingInfo>();
        for (int i = 0; i < settings.Count; i++)
        {
            if (settings[i].CheckHighlightCondition(obj))
                infosToUse.Add(settings[i].Settings);
        }

        if (infosToUse.Count > 1)
        {
            float sizeX = inRect.size.x / (float)infosToUse.Count;

            for (int i = 0; i < infosToUse.Count; i++)
            {
                //TODO create rect with sizex and in size y
                //change x position of every rect 
                Rect r = new Rect();
                r.x = inRect.x + i * sizeX;
                r.y = inRect.y;
                r.width = sizeX;
                r.height = inRect.height;

                infosToUse[i].DrawBackgroundRect(r, isSelected);
            }

            //draw icon and label
            infosToUse[0].DrawIcon(obj, inRect, isSelected);
            infosToUse[0].DrawLabel(obj, inRect, isSelected);
        }
        else if (infosToUse.Count == 1)
        {
            infosToUse[0].Draw(obj, inRect, isSelected);
        }
    }

}
