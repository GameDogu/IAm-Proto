using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Editor/Movement State Machine Editor Settings")]
public class MovementStateMachineEditorSettings : ScriptableObject
{
    public Vector2 DefaultSize = new Vector2(1920,1080);
    public NodeStyle NodeStyle;
    public float ArrowSize = 15f;

    public float TransitionOutPointSize = 5f;

    public float TransitionOutPointRadius = 0.9f;
    public float TransitionOutPointRadiusSelected = 1.25f;

    public Color TransitionColor = Color.black;

    private void Awake()
    {
       NodeStyle = NodeStyle.Default();
    }
}
