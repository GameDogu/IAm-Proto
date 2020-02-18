using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TransitionOutPoint
{
    public EditorStateNode Node { get; protected set; }

    NodeLayout layout;

    public float Angle { get; set; }

    public EditorTransition Transition { get; set; }

    public bool HasTransition => Transition != null;

    public TransitionRequest Type { get; protected set; }

    float buttonSize;

    string toolTip;

    public Vector2 Position => GetWindowPosition();

    public TransitionOutPoint(EditorStateNode node, NodeLayout layout,TransitionRequest requestType,float buttonSize=5f)
    {
        this.layout = layout;
        this.Node = node ?? throw new ArgumentNullException(nameof(node));
        Transition = null;
        Type = requestType;
        this.buttonSize = buttonSize;
        var info = Type.GetInfo();
        toolTip = info != null ? info.DisplayName : Type.GetType().Name;
    }

    public void Draw()
    {
        var pos = GetWindowPosition();
        Vector2 size = new Vector2(buttonSize, buttonSize)*1.5f;
        Rect r = new Rect(pos - size * .5f, size);

        if (r.Contains(Event.current.mousePosition))
        {
            Vector2 dir = (pos - Node.Rect.center).normalized;
            r.position = Event.current.mousePosition + (Vector2.right*16f);
            var st = new GUIStyle("Tooltip");
            var content = new GUIContent(toolTip);
            r.size = st.CalcSize(content);

            GUI.Label(r, content,st);
        }

        if (Handles.Button(pos, Quaternion.identity, buttonSize, buttonSize * 1.25f, Handles.CircleHandleCap))
        {
            //on click
            OnClick();
        }
    }

    public Vector2 CalcLocalPosition(float offset, float radius)
    {
        float angle = (offset + Angle) * Mathf.Deg2Rad;
        float wR = Node.Width * .5f * radius;
        float hR = Node.Height * .5f * radius;

        return new Vector2(
        Mathf.Cos(angle)*wR,
        Mathf.Sin(angle)*hR
        );
    }

    Vector2 GetWindowPosition()
    {
        return Node.LocalToWindowPos(CalcLocalPosition(layout.Rotation, layout.Radius));
    }


    private void OnClick()
    {
        Node.Editor.OnTransitionOutPointClicked(this);
    }

    public void Destroy()
    {
        if (HasTransition)
        {
            Transition.Destroy(false);
        }
    }

    public void Empty()
    {
        Transition = null;
    }

}