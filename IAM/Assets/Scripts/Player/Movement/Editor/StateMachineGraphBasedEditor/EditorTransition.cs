using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTransition : IEditorDrawable
{
    uint fromID;
    uint toID;

    MovementStateMachineEditor editor;
    public Transition Transition { get; protected set; }

    TransitionOutPoint fromNodeOutPoint;
    EditorStateNode toNode;

    bool causeFoldOut;

    public EditorTransition(TransitionOutPoint fromNodeOutPoint, EditorStateNode toNode,MovementStateMachineEditor editor)
    {
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        this.fromNodeOutPoint = fromNodeOutPoint ?? throw new ArgumentNullException(nameof(fromNodeOutPoint));
        this.toNode = toNode ?? throw new ArgumentNullException(nameof(toNode));
        fromID = fromNodeOutPoint.Node.ID;
        toID = toNode.ID;

        Transition = new Transition(fromNodeOutPoint.Type,toID, fromNodeOutPoint.Node.State);
        fromNodeOutPoint.Node.State.AddTransition(Transition);
        fromNodeOutPoint.Transition = this;
        causeFoldOut = false;
    }

    public EditorTransition(Transition t, MovementStateMachineEditor editor)
    {
        this.editor = editor;
        this.Transition = t;

        fromID = t.StateBelongingToID;
        toID = t.NextStateID;

        fromNodeOutPoint = editor.GetStateByID(fromID).GetTransitionOutPoint(t.Type);
        fromNodeOutPoint.Transition = this;
        toNode = editor.GetStateByID(toID);
        causeFoldOut = false;
    }

    public void DrawInEditor()
    {
        EditorGUILayout.LabelField("Transition", new GUIStyle("BoldLabel"));

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("From:", $"{fromID}");
        EditorGUILayout.LabelField("To:", $"{toID}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Activation:", new GUIStyle("BoldLabel"));
        if (Transition.Type != null)
        {
            //TODO change to 
            //Transition.Type.GetInfo()
            var info = Attribute.GetCustomAttribute(Transition.Type.GetType(), typeof(TransitionRequestInfoAttribute)) as TransitionRequestInfoAttribute;

            if (info != null)
            {
                EditorGUILayout.LabelField(info.DisplayName);
                causeFoldOut = EditorGUILayout.Foldout(causeFoldOut,new GUIContent($"Caused By:{info.Type.ToString()}"));
                if (causeFoldOut)
                {
                    var st = new GUIStyle("Label");
                    st.wordWrap = true;
                    EditorGUILayout.LabelField(info.DetailedCause,st);
                }
            }
            else
            {
                //debug info
                Debug.LogWarning($"No Transition Request Info Attribute exists for {Transition.Type.GetType().Name}");
                EditorGUILayout.LabelField(new GUIContent(Transition.Type.GetType().Name, "Warning: No Transition Request Info Attribute Exists"));
            }
        }

        DrawChangeActivatorButton();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Remove"))
        {
            RemoveTransition(true);
        }

        if (GUILayout.Button("Save"))
        {
            editor.Save();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawChangeActivatorButton()
    {
        if (GUILayout.Button("Change Activator"))
        {
            GenericMenu men = new GenericMenu();
            var possRequest = fromNodeOutPoint.Node.GetEmptyTransitionPoints();

            for (int i = 0; i < possRequest.Count; i++)
            {
                var outPoint = possRequest[i];
                var option = outPoint.Type;
                TransitionRequestInfoAttribute att = Attribute.GetCustomAttribute(option.GetType(), typeof(TransitionRequestInfoAttribute)) as TransitionRequestInfoAttribute;
                if (att != null)
                {
                    if (att.Type == TransitionRequestInfoAttribute.RequestType.None)
                        break;
                    men.AddItem(new GUIContent(att.Type.ToString() +"/"+att.DisplayName,att.Type.ToString()), false, () => SetTransitionActivator(outPoint));
                }
                else
                {
                    men.AddItem(new GUIContent("Other/"+option.GetType().Name, "Warning: No Transition Request Info Attribute Exists"), false, () => SetTransitionActivator(outPoint));                    
                }
                
            }
            men.ShowAsContext();
        }
    }

    private void SetTransitionActivator(TransitionOutPoint point)
    {
        fromNodeOutPoint.Empty();//empty old one

        fromNodeOutPoint = point;//set to new one
        fromNodeOutPoint.Transition = this; //inform new one of transition he ha

        Transition.Type = point.Type;
        Save();
    }

    private void Save()
    {
        editor.Save();
    }

    public void Draw()
    {
        int transitionsBetweenNodesCount = toNode.State.CountTransitionsTo(fromNodeOutPoint.Node.ID);
        transitionsBetweenNodesCount += fromNodeOutPoint.Node.State.CountTransitionsTo(toNode.ID);

        Vector2[] drawPoints = new Vector2[] { fromNodeOutPoint.Position, toNode.Rect.center };

        DrawTransitionLine(drawPoints[0], drawPoints[1], editor.TransitionArrowSize, editor.TransitionColor, toNode);

        if (Handles.Button(Vector2.Lerp(drawPoints[0],drawPoints[1], 0.5f), Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            OnTransitionClicked();
        }
    }
    
    public static void DrawTransitionLine(Vector2 from, Vector2 to,float arrowSize,Color col,EditorStateNode toNode = null)
    {
        Vector2 toPoint = to;
        Bounds b;
        if (toNode != null)
        {
            b = new Bounds(toNode.Rect.center, toNode.Rect.size);
        }
        else
        {
            b = new Bounds(to, Vector2.one * 5);
        }

        Ray r = new Ray(b.center, -(from - toPoint).normalized);
        float dist;
        b.IntersectRay(r, out dist);
        toPoint = r.origin + r.direction * dist;

        Vector2 lineVec = (from - toPoint);
        Vector2 arrowDir = lineVec.normalized;

        var dirP1 = Rotate(arrowDir, 45f);
        var dirp2 = Rotate(arrowDir, -45f);
        var p1 = toPoint + dirP1 * arrowSize;
        var p2 = toPoint + dirp2 * arrowSize;

        Color c = Handles.color;
        Handles.color = col;
        Handles.DrawPolyLine(from, toPoint, p1, p2, toPoint);
        Handles.color = c;
    }

    static Vector2 Rotate(Vector2 vec, float deg)
    {
        float cosRot = Mathf.Cos(deg * Mathf.Deg2Rad);
        float sinRot = Mathf.Sin(deg * Mathf.Deg2Rad);
        return new Vector2(cosRot * vec.x - sinRot * vec.y, sinRot * vec.x + cosRot * vec.y);
    }

    private void OnTransitionClicked()
    {
        InspectTransition();
        GenericMenu men = new GenericMenu();
        men.AddItem(new GUIContent("Edit"), false, InspectTransition);
        men.AddItem(new GUIContent("Remove"), false,()=> RemoveTransition(true));
        men.ShowAsContext();
    }

    private void RemoveTransition(bool informNode)
    {
        fromNodeOutPoint.Node.State.RemoveTransition(Transition);
        editor.RemoveTransition(this);

        if (informNode)
        {
            fromNodeOutPoint.Node.OnTransitionRemoved(this);
        }
    }


    public void Destroy(bool informNode)
    {
        RemoveTransition(informNode);
    }


    private void InspectTransition()
    {
        editor.InformOfTransitionEdit(this);
    }
}