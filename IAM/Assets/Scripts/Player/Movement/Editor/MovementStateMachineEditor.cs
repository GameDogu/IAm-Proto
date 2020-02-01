using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using System;

public class MovementStateMachineEditor : EditorWindow
{
    EntityEditedInfoWindow entityInfoEditor;
    StateEditorWindow stateInfoEditor;


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
        entityInfoEditor = CreateWindow<EntityEditedInfoWindow>(GetType());
        stateInfoEditor = CreateWindow<StateEditorWindow>(GetType(), entityInfoEditor.GetType());

        entityInfoEditor.Initialize(this, stateInfoEditor);
        stateInfoEditor.Initialize(this, entityInfoEditor);

        entityInfoEditor.OnEditedEntityChanged -= OnEntityEditedChanged;
        entityInfoEditor.OnEditedEntityChanged += OnEntityEditedChanged;
    }

    void OnEntityEditedChanged()
    {

    }

    public void OnGUI()
    {
        // Draw();
        HandleEvent();
    }

    private void HandleEvent()
    {
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                HandleMouseDown(Event.current);
                break;
            default:
                break;
        }

    }

    private void HandleMouseDown(Event current)
    {
        if (current.button == 1)
        {
            GenericMenu men = new GenericMenu();
            men.AddItem(new GUIContent("Create New State"), false, CreateNewState );
            men.ShowAsContext();
        }
    }

    void CreateNewState()
    {
        MovementState state = entityInfoEditor.EntityEdited.AddNewState();
        stateInfoEditor.CurrentStateEdited = state;
    }

    private void Draw()
    {
        DrawHeader();
        DrawBody();
        DrawFooter();
    }

    private void DrawFooter()
    {
        throw new NotImplementedException();
    }

    private void DrawBody()
    {
        throw new NotImplementedException();
    }

    private void DrawHeader()
    {
        throw new NotImplementedException();
    }
}
