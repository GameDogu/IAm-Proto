using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class EntityEditedInfoWindow : EditorWindow
{
    public static void ShowWindow()
    {
        var window = GetWindow<EntityEditedInfoWindow>();
        window.Show();
    }

    public MovementStateMachine EntityEdited { get; protected set; }

    private void OnGUI()
    {
        Draw();
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
