using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ForceRecompile 
{
    [MenuItem("Edit/Force Recompile")]
    public static void RecompileUnityEditor()
    {
        BuildTargetGroup target = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        string rawSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);

        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, rawSymbols + "a");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(target, rawSymbols);
    }
}
