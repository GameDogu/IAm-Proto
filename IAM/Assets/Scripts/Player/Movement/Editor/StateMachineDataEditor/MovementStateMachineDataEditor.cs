using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(MovementStateMachineData))]
public class MovementStateMachineDataEditor : Editor
{
    bool generaldataFoldOut = true;
    bool statesFoldOut = true;

    Vector2 allowedMovementsScrollPosition;
    Vector2 transitionsScrollPosition;

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
        statesFoldOut = EditorGUILayout.Foldout(statesFoldOut, "States:");
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

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Is Initial State:");//is initial
        EditorGUILayout.LabelField(content);
        EditorGUILayout.EndHorizontal();

        DisplayList(st.AllowedMovements, new GUIContent("State Allowed Movements:"),          DrawAllowedMovement,new GUIStyle("Label"),ref allowedMovementsScrollPosition);

        if (st.DataTransitions.Count > 0)
        {
            DisplayList(st.DataTransitions, new GUIContent("Transitions:"),DrawTransition,
            new GUIStyle("Label"),ref transitionsScrollPosition);
        }
    }

    void DrawAllowedMovement(string st)
    {
        EditorGUIDrawUtility.DrawLine();
        EditorGUILayout.LabelField(st);
        EditorGUIDrawUtility.DrawLine();
        EditorGUILayout.Space();
    }

    void DrawTransition(Transition.Data tr)
    {
        EditorGUIDrawUtility.DrawLine();
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
        EditorGUIDrawUtility.DrawLine();
    }

    void DisplayList<T>(IReadOnlyList<T> list, GUIContent header, Action<T>drawContentFunc, GUIStyle headerStyle, ref Vector2 scrollVec)
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
        scrollVec = EditorGUILayout.BeginScrollView(scrollVec, false, true);
        EditorGUI.indentLevel += 1;
        for (int i = 0; i < list.Count; i++)
        {
            drawContentFunc(list[i]);
        }
        EditorGUI.indentLevel -= 1;
        EditorGUILayout.EndScrollView();
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
        var content = new GUIContent("Machine Movement Options:");

        var style = new GUIStyle("BoldLabel");
        style.wordWrap = true;

        generaldataFoldOut = EditorGUILayout.Foldout(generaldataFoldOut, content, style);
        if (generaldataFoldOut)
        {
            EditorGUI.indentLevel += 1;
            for (int i = 0; i < data.GeneralOptionsData.Count; i++)
            {
                EditorGUILayout.LabelField(data.GeneralOptionsData[i]);
                EditorGUIDrawUtility.DrawLine();
            }
            EditorGUI.indentLevel -= 1;
        }
    }

}
