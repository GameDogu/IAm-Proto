using System;
using System.Collections.Generic;
using System.Linq;

public class NodeLayout
{
    EditorStateNode node;
    MovementState state => node.State;

    List<TransitionOutPoint> outPoints;

    public float Rotation { get; set; }
    public float Radius
    {
        get
        {
            if (node.IsSelected)
                return node.Editor.TransitionOutPointRadiusSelected;
            return node.Editor.TransitionOutPointRadius;
        }
    }

    public NodeLayout(EditorStateNode node)
    {
        this.node = node ?? throw new ArgumentNullException(nameof(node));
        outPoints = new List<TransitionOutPoint>();
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        var possTransitionTypes = node.Editor.StateMachine.GetPossibleTransitionRequests(state);

        //remove all out points which type is not in possTransTypes
        for (int i = outPoints.Count-1; i >= 0; i--)
        {
            var p = outPoints[i];
            if (!possTransitionTypes.Exists(t => t.IsSameRequest(p.Type)))
            {
                outPoints.RemoveAt(i);
            }
        }

        //add new outPoints for all the transition types not in outpoints
        for (int i = 0; i < possTransitionTypes.Count; i++)
        {
            var t = possTransitionTypes[i];
            if (!outPoints.Exists(p => p.Type.IsSameRequest(t)))
            {
                outPoints.Add(new TransitionOutPoint(node,this, t, node.Editor.TransitionOutPointSize));
            }
        }

        //place all outpoints sensibly(hopefully)
        float angleStep = 360f / (float)outPoints.Count;

        for (int i = 0; i < outPoints.Count; i++)
        {
            outPoints[i].Angle = angleStep * i;
        }

    }

    public void Draw()
    {
        for (int i = outPoints.Count-1; i >= 0; i--)
        {
            outPoints[i].Draw();
        }
    }

    public void OnTransitionRemoved(EditorTransition trans)
    {
        outPoints.ForEach(p =>
        {
            if (trans == p.Transition)
            {
                p.Transition = null;
            }
        });
    }

    public TransitionOutPoint GetTransitionOutPoint(TransitionRequest request)
    {
        return outPoints.Find(p => p.Type.IsSameRequest(request));
    }

    public IReadOnlyList<TransitionOutPoint> GetEmptyTransitionOutPoints()
    {
        return outPoints.Where(p => !p.HasTransition).ToList();
    }
}
