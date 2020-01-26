using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ComponentHighlightSetting : HighlightSetting
{
    static Assembly[] assemblies;
    static string[] assemblyNames;
    public static string[] AssemblyNames
    {
        get
        {
            if(assemblies == null || assemblyNames == null)
            {
                FillAssemnblyArrays();
            }
            return assemblyNames;
        }
    }
    public static int AssemblyCount
    {
        get { return AssemblyNames.Length; }
    }

    private static void FillAssemnblyArrays()
    {
        assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(
       ass => ass.GetName().Name.Contains("UnityEngine")
            && (ass.GetTypes().Where(ty => ty.IsSubclassOf(typeof(Component))).ToList().Count > 0)).ToArray();
        var strAssemblies = assemblies.ToList().ConvertAll(ass => ass.GetName().Name);
        assemblyNames = new string[strAssemblies.Count + 1];

        for (int i = 0; i < strAssemblies.Count; i++)
        {
            var s = strAssemblies[i];
            if (s.Contains("."))
            {
                s = s.Remove(0, s.IndexOf(".") + 1);
            }
            assemblyNames[i] = s;
        }
        assemblyNames[strAssemblies.Count] = "User Created";
    }

    static Assembly userAssembly;
    public static Assembly UserAssembly
    {
        get
        {
            if (userAssembly == null)
            {
                userAssembly = AppDomain.CurrentDomain.GetAssemblies().
           SingleOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");
            }
            return userAssembly;
        }
    }

    public static List<Type> GetComponentsInAssembly(Assembly ass)
    {
        return ass.GetTypes().Where(ty => ty.IsSubclassOf(typeof(Component))).ToList();
    }
    public static Assembly GetAssembly(string name)
    {
        if (assemblies == null)
        {
            FillAssemnblyArrays();
        }
        int i = AssemblyNames.ToList().IndexOf(name);
        if (i < 0)
            return null;
        else
            return GetAssembly(i);
    }
    public static Assembly GetAssembly(int i)
    {
        if (assemblies == null)
        {
            FillAssemnblyArrays();
        }
        return IsUserAssembly(i) ? UserAssembly : assemblies[i];
    }

    public static bool IsUserAssembly(int i)
    {
        return i == assemblies.Length;
    }

    [SerializeField] string type;
    int iComp = 0;
    int iAss = 0;

    public ComponentHighlightSetting():base()
    {
        type = "";
    }

    public override bool DrawInspectorGUI()
    {
        iAss = EditorGUILayout.Popup("Assembly: ",iAss, AssemblyNames);

        var assemblyUsed = GetAssembly(iAss);

        return ShowPossibleComponents(assemblyUsed) | base.DrawInspectorGUI();
    }

    bool ShowPossibleComponents(Assembly ass)
    {
        var comp = GetComponentsInAssembly(ass);
        if (comp.Count > 0)
        {
            var compName = comp.ConvertAll(ty => ty.Name);
            iComp = EditorGUILayout.Popup("Component: ", iComp, compName.ToArray());
            var nType = comp[iComp];
            if (nType.Name != type)
            {
                type = nType.Name;
                return true;
            }
            return false;
        }
        else
        {
            EditorGUILayout.LabelField("No Components in this Assembly");
            return false;
        }
    }

    public override void HightlightObjectOnMetConditon(GameObject obj, Rect inRect, bool isSelected)
    {
        if (type == null)
            return;
        if (CheckHighlightCondition(obj))
        {
            Settings.Draw(obj, inRect, isSelected);
        }
    }

    public override bool CheckHighlightCondition(GameObject obj)
    {
        return obj.GetComponent(type) != null;
    }
}
