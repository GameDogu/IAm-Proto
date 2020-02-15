﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTransition : IEditorDrawable
{
    uint fromID;
    uint toID;

    MovementStateMachineEditor editor;
    public Transition Transition { get; protected set; }

    EditorStateNode fromNode;
    EditorStateNode toNode;

    bool causeFoldOut;

    public EditorTransition(EditorStateNode fromNode, EditorStateNode toNode,MovementStateMachineEditor editor)
    {
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        this.fromNode = fromNode ?? throw new ArgumentNullException(nameof(fromNode));
        this.toNode = toNode ?? throw new ArgumentNullException(nameof(toNode));
        fromID = fromNode.ID;
        toID = toNode.ID;

        Transition = new Transition(toID, fromNode.State);
        fromNode.State.AddTransition(Transition);
        causeFoldOut = false;
    }

    public EditorTransition(Transition t, MovementStateMachineEditor editor)
    {
        this.editor = editor;
        this.Transition = t;

        fromID = t.StateBelongingToID;
        toID = t.NextStateID;

        fromNode = editor.GetStateByID(fromID);
        toNode = editor.GetStateByID(toID);
        causeFoldOut = false;
    }


    public void DrawInEditor()
    {
        EditorGUILayout.LabelField("Transition", new GUIStyle("BoldLabel"));

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("From:", $"{fromID}");
        EditorGUILayout.LabelField("To:", $"{toID}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Activation:", new GUIStyle("BoldLabel"));
        if (Transition.Type != null)
        {
            //TODO change to 
            //Transition.Type.GetInfo()
            var info = Attribute.GetCustomAttribute(Transition.Type.GetType(), typeof(TransitionRequestInfoAttribute)) as TransitionRequestInfoAttribute;

            if (info != null)
            {
                EditorGUILayout.LabelField(info.DisplayName);
                causeFoldOut = EditorGUILayout.Foldout(causeFoldOut,new GUIContent($"Caused By:{info.Type.ToString()}"));
                if (causeFoldOut)
                {
                    var st = new GUIStyle("Label");
                    st.wordWrap = true;
                    EditorGUILayout.LabelField(info.DetailedCause,st);
                }
            }
            else
            {
                //debug info
                Debug.LogWarning($"No Transition Request Info Attribute exists for {Transition.Type.GetType().Name}");
                EditorGUILayout.LabelField(new GUIContent(Transition.Type.GetType().Name, "Warning: No Transition Request Info Attribute Exists"));
            }

        }

        DrawChangeActivatorButton();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Remove"))
        {
            RemoveTransition();
        }

        if (GUILayout.Button("Save"))
        {
            editor.Save();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawChangeActivatorButton()
    {
        if (GUILayout.Button("Change Activator"))
        {
            GenericMenu men = new GenericMenu();
            var possRequest = editor.StateMachine.GetPossibleTransitionRequests(fromNode.State);

            for (int i = 0; i < possRequest.Count; i++)
            {
                var option = possRequest[i];
                TransitionRequestInfoAttribute att = Attribute.GetCustomAttribute(option.GetType(), typeof(TransitionRequestInfoAttribute)) as TransitionRequestInfoAttribute;
                if (att != null)
                {
                    if (att.Type == TransitionRequestInfoAttribute.RequestType.None)
                        break;
                    men.AddItem(new GUIContent(att.Type.ToString() +"/"+att.DisplayName,att.Type.ToString()), false, () => SetTransitionActivator(option));
                }
                else
                {
                    men.AddItem(new GUIContent("Other/"+option.GetType().Name, "Warning: No Transition Request Info Attribute Exists"), false, () => SetTransitionActivator(option));                    
                }
                
            }
            men.ShowAsContext();
        }
    }

    private void SetTransitionActivator(TransitionRequest req)
    {
        Transition.Type = req;
    }

    public void Draw()
    {
        DrawTransitionLine(fromNode.Rect.center, toNode.Rect.center);

        if (Handles.Button(Vector2.Lerp(fromNode.Rect.center, toNode.Rect.center, 0.5f), Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            OnTransitionClicked();
        }
    }

    public static void DrawTransitionLine(Vector2 from, Vector2 to)
    {
        Handles.DrawLine(from, to);
    }

    private void OnTransitionClicked()
    {
        GenericMenu men = new GenericMenu();
        men.AddItem(new GUIContent("Edit"), false, InspectTransition);
        men.AddItem(new GUIContent("Remove"), false, RemoveTransition);
        men.ShowAsContext();
    }

    private void RemoveTransition()
    {
        fromNode.State.RemoveTransition(Transition);
        editor.RemoveTransition(this);
    }

    private void InspectTransition()
    {
        editor.InformOfTransitionEdit(this);
    }
}