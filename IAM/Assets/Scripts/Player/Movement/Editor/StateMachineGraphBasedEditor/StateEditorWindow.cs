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

    public IEditorDrawable currentObjectDisplayed;
    public IEditorDrawable CurrentObjectDisplayed
    {
        get{ return currentObjectDisplayed; }
        set
        {
            currentObjectDisplayed = value;
            Repaint();
        }
    }

    public void Initialize(MovementStateMachineEditor stateMachineEditor, EntityEditedInfoWindow entityInfoEditor)
    {
        this.stateMachineEditor = stateMachineEditor;
        this.entityInfoEditor = entityInfoEditor;

        RegisterToEvents();
    }

    private void RegisterToEvents()
    {
        entityInfoEditor.OnEditedEntityChanged -= EditedEntityChanged;
        entityInfoEditor.OnEditedEntityChanged += EditedEntityChanged;

        stateMachineEditor.OnNodeSelected -= OnNodeSelected;
        stateMachineEditor.OnNodeSelected += OnNodeSelected;

        stateMachineEditor.OnNodeDeselected -= OnNodeDeselected;
        stateMachineEditor.OnNodeDeselected += OnNodeDeselected;
    }

    void EditedEntityChanged()
    {
        //TODO actually nothing right now, huh
        Repaint();
    }

    public void Save()
    {
        entityInfoEditor.Save();
    }

    private void OnGUI()
    {
        Draw();
    }

    private void Draw()
    {
        if (CurrentObjectDisplayed != null)
        {
            CurrentObjectDisplayed.DrawInEditor();
        }
        else
            EditorGUILayout.LabelField("No State Selected");
    }

    void OnNodeSelected(EditorStateNode node)
    {
        CurrentObjectDisplayed = node;
    }

    void OnNodeDeselected()
    {
        if (CurrentObjectDisplayed is EditorStateNode)
            CurrentObjectDisplayed = null;
    }

}