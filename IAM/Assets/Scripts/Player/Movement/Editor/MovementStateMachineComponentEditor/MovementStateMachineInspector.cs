using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovementStateMachine))]
public class MovementStateMachineInspector : Editor
{
    public override void OnInspectorGUI()
    {
        MovementStateMachine tar = target as MovementStateMachine;

        var playerProp = serializedObject.FindProperty(tar.PlayerPropertyName);

        EditorGUILayout.ObjectField(playerProp, new GUIContent("Player: "));

        MovementStateMachineData data = tar.Data;

        data = EditorGUILayout.ObjectField("Data:", data, typeof(MovementStateMachineData), false) as MovementStateMachineData;

        if (data != null)
        {
            tar.MovementStateMachineDataAssetPath = AssetDatabase.GetAssetPath(data);
        }

        if (GUILayout.Button("Save"))
        {
            MovementStateMachineData.Save(tar);
        }
    }
}
