using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Editor/Movement State Machine Editor Settings")]
public class MovementStateMachineEditorData : ScriptableObject
{
    public Vector2 DefaultSize = new Vector2(1920,1080);
    public NodeStyle NodeStyle;

    private void Awake()
    {
       NodeStyle = NodeStyle.Default();
    }
}
