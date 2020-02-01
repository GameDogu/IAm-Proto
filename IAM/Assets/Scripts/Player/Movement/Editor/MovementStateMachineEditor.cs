using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using System;

public class MovementStateMachineEditor : EditorWindow
{
    [MenuItem("Movement/Statemachine Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<MovementStateMachineEditor>();

        //AnimatorController Icon
        //BlendTree Icon
        window.titleContent = new GUIContent() { text = "Movement Statemachine Editor", image = EditorGUIUtility.IconContent("AnimatorController Icon").image };

        window.Initialize();
    }

    void Initialize()
    {
        //TODO if anything
        var editor = CreateWindow<EntityEditedInfoWindow>(GetType());
        var stateEditor = CreateWindow<StateEditorWindow>(GetType(), editor.GetType());
    }

    public void OnGUI()
    {

    }

    private void Draw()
    {

    }

}
