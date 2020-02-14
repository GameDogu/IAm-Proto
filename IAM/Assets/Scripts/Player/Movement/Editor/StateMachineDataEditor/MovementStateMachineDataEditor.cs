using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MovementStateMachineData))]
public class MovementStateMachineDataEditor : Editor
{
    bool generaldataFoldOut;
    bool statesFoldOut;

    public override void OnInspectorGUI()
    {
        var data = target as MovementStateMachineData;

        HandleGeneralMovementOptions(data);

        HandleStates(data);

    }

    private void HandleStates(MovementStateMachineData data)
    {
        if (data.States == null)
            return;
        statesFoldOut = EditorGUILayout.Foldout(statesFoldOut, "States:", new GUIStyle("BoldLabel"));
        if (statesFoldOut)
        {
            for (int i = 0; i < data.States.Count; i++)
            {
                var st = data.States[i];
                HandleState(st, i);
            }
        }
    }

    private void HandleState(MovementState.Data st, int stIdx)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ID:", $"{st.ID}");
        EditorGUILayout.LabelField("Name:", st.Name);
        EditorGUILayout.EndHorizontal();

        GUIContent content = GetIconBool(st.IsInitial);
        content.text = "Is Initial State:";

        EditorGUILayout.LabelField(content);//is initial

        DisplayList(st.AllowedMovements, new GUIContent("Allowed Movements:"),          mov => EditorGUILayout.LabelField(mov), new GUIStyle("BoldLabel"));
        EditorGUILayout.Space();
        DisplayList(st.Transitions, new GUIContent("Transitions:"),DrawTransition,
            new GUIStyle("BoldLabel"));
    }

    void DrawTransition(Transition.Data tr)
    {
        var transitionRequest = TransitionRequest.Factory.BuildRequest(tr.ReqeustType);

        var info = transitionRequest.GetInfo();

        if (info != null)
        {
            EditorGUILayout.LabelField("Transitioning On:", info.DisplayName);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cause:", new GUIStyle("BoldLabel"));

            var detailedLabelStyle = new GUIStyle("Label");
            detailedLabelStyle.wordWrap = true;

            EditorGUILayout.LabelField(new GUIContent(info.Type.ToString(), info.DetailedCause),detailedLabelStyle);

            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("Transitioning On:", tr.ReqeustType);
        }
        EditorGUILayout.LabelField("Next State ID: ", $"{tr.NextState}");
    }

    void DisplayList<T>(IReadOnlyList<T> list, GUIContent header, Action<T>drawContentFunc, GUIStyle headerStyle)
    {
        if (drawContentFunc == null || list == null)
            return;

        EditorGUILayout.Space();

        if (headerStyle != null)
        {
            EditorGUILayout.LabelField(header,headerStyle);
        }
        else
        {
            EditorGUILayout.LabelField(header);
        }
        EditorGUI.indentLevel -= 1;
        for (int i = 0; i < list.Count; i++)
        {
            drawContentFunc(list[i]);
        }
        EditorGUI.indentLevel += 1;
        EditorGUILayout.Space();
    }

    GUIContent GetIconBool(bool val)
    {
        if (val)
            return EditorGUIUtility.IconContent("TestPassed");
        else
            return EditorGUIUtility.IconContent("TestNormal");
    }

    void HandleGeneralMovementOptions(MovementStateMachineData data) 
    {
        if (data.GeneralOptionsData == null)
            return;
        generaldataFoldOut = EditorGUILayout.Foldout(generaldataFoldOut, "Generally allowed movement options:", new GUIStyle("BoldLabel"));
        if (generaldataFoldOut)
        {
            EditorGUI.indentLevel += 1;
            for (int i = 0; i < data.GeneralOptionsData.Count; i++)
            {
                EditorGUILayout.LabelField(data.GeneralOptionsData[i]);
            }
            EditorGUI.indentLevel -= 1;
        }
    }

}
