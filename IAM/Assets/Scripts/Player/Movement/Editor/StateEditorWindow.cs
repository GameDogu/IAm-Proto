using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class StateEditorWindow : EditorWindow
{
    public static void ShowWindow()
    {
        var window = GetWindow<StateEditorWindow>();
        window.Show();
    }

    MovementStateMachineEditor stateMachineEditor;
    EntityEditedInfoWindow entityInfoEditor;

    public MovementState CurrentStateEdited { get; set; }

    public void Initialize(MovementStateMachineEditor stateMachineEditor, EntityEditedInfoWindow entityInfoEditor)
    {
        this.stateMachineEditor = stateMachineEditor;
        this.entityInfoEditor = entityInfoEditor;

        entityInfoEditor.OnEditedEntityChanged -= EditedEntityChanged;
        entityInfoEditor.OnEditedEntityChanged += EditedEntityChanged;

    }

    void EditedEntityChanged()
    {
        Debug.Log("hello");
    }

    private void OnGUI()
    {
        Draw();
    }

    private void Draw()
    {
        if (CurrentStateEdited != null)
        {
            DrawHeader();
            DrawBody();
            DrawFooter();
        }
        else
            EditorGUILayout.LabelField("No State Selected");
    }

    private void DrawFooter()
    {
        CurrentStateEdited.Name = EditorGUILayout.TextField("Editing:", CurrentStateEdited.Name, new GUIStyle("BoldLabel"));
    }

    private void DrawBody()
    {
        //throw new NotImplementedException();
    }

    private void DrawHeader()
    {
        //throw new NotImplementedException();
    }
}