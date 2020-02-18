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

    public MovementStateMachineEditor Editor { get; protected set; }
    public MovementState State { get; protected set; }

    public uint ID => State.ID;

    public bool IsInitialState => State.IsInitialState;

    Rect nodeRect;

    public int TransitionCount => State.TransitionCount;

    public Rect Rect => nodeRect;

    public float Width
    {
        get => nodeRect.width;
        set => nodeRect.width = value;
    }

    public float Height
    {
        get => nodeRect.height;
        set => nodeRect.height = value;
    }

    Bounds bounds;

    public Vector2 Position
    {
        get { return nodeRect.position; }
        protected set
        {
            nodeRect.position = value;
        }
    }


    NodeStyle style;
    public bool IsSelected { get; protected set; }

    NodeLayout nodeLayout;

    public EditorStateNode(MovementStateMachineEditor editor, MovementState state, Vector2 position, NodeStyle style)
    {
        this.Editor = editor ?? throw new ArgumentNullException(nameof(editor));
        State = state ?? throw new ArgumentNullException(nameof(state));
        nodeRect = new Rect(position, new Vector2(style.Width, style.Height));

        bounds = new Bounds(nodeRect.center, nodeRect.size);
        this.style = style;
        nodeLayout = new NodeLayout(this);
    }

    public bool IntersectPoint(Ray r, out Vector2 point)
    {
        point = Vector2.zero;

        float dist;
        if (bounds.IntersectRay(r, out dist))
        {
            point = r.origin + r.direction * dist;
            return true;
        }
        return false;
    }

    public void DrawNode()
    {
        var st = GetStyle();
        FigureOutRectSize(st);
        GUI.Box(nodeRect, State.Name, st);
        if(IsSelected)
            nodeLayout.Draw();
    }

    void FigureOutRectSize(GUIStyle st)
    {
        var potentialSize = st.CalcSize(new GUIContent(State.Name));
        potentialSize += new Vector2(st.border.left + st.border.right, st.border.top + st.border.bottom);
        Vector2 size = nodeRect.size;

        size.x = Mathf.Max(potentialSize.x, style.Width);
        size.y = Mathf.Max(potentialSize.y, style.Height);

        nodeRect.size = size;
    }

    private GUIStyle GetStyle()
    {
        if (IsInitialState)
        {
            if (IsSelected)
                return style.initialStateStyleSelected;
            else
                return style.initialStateStyle;
        }
        else if (IsSelected)
            return style.selectedStyle;
        return style.style;
    }

    public bool ProcessEvent(Event e)
    {
        if (nodeRect.Contains(e.mousePosition) && !Editor.HasEventModifierPrioritizingEditorHandling(e))
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
                    else if (e.button == 1)
                    {
                        OnRightClick?.Invoke(this);
                        e.Use();
                        return true;
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

    public void RotateLayout(Vector2 mousePos,int invert = 1)
    {
        Vector2 mouseDir = (mousePos - nodeRect.center).normalized;

        float angle = invert * Vector2.SignedAngle(Vector2.right, mouseDir);

        nodeLayout.Rotation = angle;
    }

    public void Drag(Vector2 dragOffset)
    {
        Position += dragOffset;
    }

    public void SelectNode()
    {
        
        IsSelected = Editor.SetNodeSelected(this);

    }

    public void Deselect()
    {
        IsSelected = false;
    }

    public void DrawInEditor()
    {
        State.Name = EditorGUILayout.TextField("Editing:", State.Name);

        EditorGUILayout.Space();

        bool prev = State.IsInitialState;
        State.IsInitialState = EditorGUILayout.Toggle("Initial State", State.IsInitialState);

        if (State.IsInitialState && !prev)
            Editor.InformOfIntialMark(State);

        EditorGUILayout.Space();

        DisplayCurrentAllowedOptions();

        EditorGUILayout.Space();

        DisplayAddOptionButton();
    }

    public void SetInitialState(bool value)
    {
        State.IsInitialState = value;
    }

    private void DisplayAddOptionButton()
    {
        if (GUILayout.Button("Add Allowed Movement"))
        {
            GenericMenu men = new GenericMenu();
            for (int i = 0; i < Editor.StateMachine.GeneralMovementOptions.Count; i++)
            {
                var option = Editor.StateMachine.GeneralMovementOptions[i];
                if(!State.ContainsMovementOption(option))
                    men.AddItem(new GUIContent(option.Name), false, () => { State.AddMovementOption(option);nodeLayout.UpdateLayout(); Editor.Save(); });
            }
            men.ShowAsContext();
        }
    }

    private void DisplayCurrentAllowedOptions()
    {
        EditorGUILayout.LabelField("Currently Allowed Options:");
        EditorGUI.indentLevel += 1;
        for (int i = State.MovementOptions.Count - 1; i >= 0; i--)
        {
            var option = State.MovementOptions[i];
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(option.Name);
            if (GUILayout.Button("X"))
            {
                State.RemoveMovementOption(option);
                var att = Attribute.GetCustomAttribute(option.GetType(), typeof(PossibleTransitionRequestTypesAttribute)) as PossibleTransitionRequestTypesAttribute;
                if (att != null)
                {
                    Editor.RemoveTransitionsForNodeOfType(this, att.Types);
                }
                nodeLayout.UpdateLayout();
                Editor.Save();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel -= 1;
    }

    public void Destroy()
    {
        OnDestroyed?.Invoke(this,State);
    }

    public Vector2 LocalToWindowPos(Vector2 localPosition)
    {
        return localPosition + Rect.center;
    }

    public void OnTransitionRemoved(EditorTransition editorTransition)
    {
        nodeLayout.OnTransitionRemoved(editorTransition);
    }

    public TransitionOutPoint GetTransitionOutPoint(TransitionRequest request)
    {
        return nodeLayout.GetTransitionOutPoint(request);
    }

    public IReadOnlyList<TransitionOutPoint> GetEmptyTransitionPoints()
    {
        return nodeLayout.GetEmptyTransitionOutPoints();
    }
}
