using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class EntityEditedInfoWindow : EditorWindow
{
    public event Action OnEditedEntityChanged;

    public static void ShowWindow()
    {
        var window = GetWindow<EntityEditedInfoWindow>();
        window.Show();
    }

    MovementStateMachineEditor stateMachineEditor;
    StateEditorWindow stateEditor;
    MovementStateMachine entityEdited;
    public MovementStateMachine EntityEdited
    {
        get { return entityEdited; }
        protected set
        {
            if (entityEdited != value)
            {
                entityEdited = value;
                OnEditedEntityChanged?.Invoke();
            }
        }
    }

    private void OnGUI()
    {
        Draw();
    }

    public void Initialize(MovementStateMachineEditor stateMachineEditor, StateEditorWindow stateEditor)
    {
        this.stateMachineEditor = stateMachineEditor;
        this.stateEditor = stateEditor;
    }

    private void Draw()
    {
        EntityEdited = EditorGUILayout.ObjectField("Editing", EntityEdited, typeof(MovementStateMachine), true) as MovementStateMachine;

        if (EntityEdited != null)
        {
            for (int i = 0; i < EntityEdited.GeneralMovementOption.Count; i++)
            {
                EditorGUILayout.LabelField($"{EntityEdited.GeneralMovementOption[i].GetType()}");
            }
        }
    }
}
