using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public interface IEditorDrawable
{
    void DrawInEditor();
}


public class EditorStateNode: IEditorDrawable
{
    public event Action<EditorStateNode> OnRightClick;
    public event Action<EditorStateNode, MovementState> OnDestroyed;

    protected MovementStateMachineEditor editor;
    public MovementState State { get; protected set; }

    public Rect nodeRect;

    public Vector2 Position
    {
        get { return nodeRect.position; }
        protected set
        {
            nodeRect.position = value;
        }
    }

    NodeStyle style;
    bool isSelected;

    public EditorStateNode(MovementStateMachineEditor editor, MovementState state, Vector2 position, NodeStyle style)
    {
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        State = state ?? throw new ArgumentNullException(nameof(state));
        nodeRect = new Rect(position, new Vector2(style.Width, style.Height));
        this.style = style;
    }

    public void Draw()
    {
        GUI.Box(nodeRect, State.Name, isSelected?style.selectedStyle:style.style);
    }

    public bool ProcessEvent(Event e)
    {
        if (nodeRect.Contains(e.mousePosition))
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        SelectNode();
                        e.Use();
                        return true;
                    }
                    else if (e.button == 2)
                    {
                        OnRightClick?.Invoke(this);
                        e.Use();
                    }
                    return false;
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        Drag(e.delta);
                        e.Use();
                    }
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }

    public void Drag(Vector2 dragOffset)
    {
        Position += dragOffset;
    }

    public void SelectNode()
    {
        isSelected = true;
        editor.SetNodeSelected(this);
    }

    public void DeselectNode()
    {
        isSelected = false;
    }

    public void DrawInEditor()
    {
        State.Name = EditorGUILayout.TextField("Editing:", State.Name, new GUIStyle("BoldLabel"));

        EditorGUILayout.Space();

        DisplayCurrentAllowedOptions();

        EditorGUILayout.Space();

        DisplayAddOptionButton();
    }

    private void DisplayAddOptionButton()
    {
        if (GUILayout.Button("Add Allowed Movement"))
        {
            GenericMenu men = new GenericMenu();
            for (int i = 0; i < editor.StateMachine.GeneralMovementOptions.Count; i++)
            {
                var option = editor.StateMachine.GeneralMovementOptions[i];
                men.AddItem(new GUIContent(option.Name), false, () => { State.AddMovementOption(option); editor.Save(); });
            }
            men.ShowAsContext();
        }
    }

    private void DisplayCurrentAllowedOptions()
    {

        Debug.Log("Currently Allowed Options:");
        EditorGUI.indentLevel += 1;
        for (int i = State.MovementOptions.Count - 1; i >= 0; i--)
        {
            var option = State.MovementOptions[i];
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(option.Name);
            if (GUILayout.Button("X"))
            {
                State.RemoveMovementOption(option);
                editor.Save();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel -= 1;
    }

    internal void Destroy()
    {
        OnDestroyed?.Invoke(this,State);
    }
}

public struct NodeStyle
{
    public float Width;
    public float Height;
    public GUIStyle style;
    public GUIStyle selectedStyle;
}