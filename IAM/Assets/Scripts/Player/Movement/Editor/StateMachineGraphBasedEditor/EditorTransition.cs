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

    EditorStateNode fromNode;
    EditorStateNode toNode;

    bool causeFoldOut;

    public EditorTransition(EditorStateNode fromNode, EditorStateNode toNode,MovementStateMachineEditor editor)
    {
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
        this.fromNode = fromNode ?? throw new ArgumentNullException(nameof(fromNode));
        this.toNode = toNode ?? throw new ArgumentNullException(nameof(toNode));
        fromID = fromNode.ID;
        toID = toNode.ID;

        Transition = new Transition(toID, fromNode.State);
        fromNode.State.AddTransition(Transition);
        causeFoldOut = false;
    }

    public EditorTransition(Transition t, MovementStateMachineEditor editor)
    {
        this.editor = editor;
        this.Transition = t;

        fromID = t.StateBelongingToID;
        toID = t.NextStateID;

        fromNode = editor.GetStateByID(fromID);
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
            RemoveTransition();
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
            var possRequest = editor.StateMachine.GetPossibleTransitionRequests(fromNode.State);

            for (int i = 0; i < possRequest.Count; i++)
            {
                var option = possRequest[i];
                TransitionRequestInfoAttribute att = Attribute.GetCustomAttribute(option.GetType(), typeof(TransitionRequestInfoAttribute)) as TransitionRequestInfoAttribute;
                if (att != null)
                {
                    if (att.Type == TransitionRequestInfoAttribute.RequestType.None)
                        break;
                    men.AddItem(new GUIContent(att.Type.ToString() +"/"+att.DisplayName,att.Type.ToString()), false, () => SetTransitionActivator(option));
                }
                else
                {
                    men.AddItem(new GUIContent("Other/"+option.GetType().Name, "Warning: No Transition Request Info Attribute Exists"), false, () => SetTransitionActivator(option));                    
                }
                
            }
            men.ShowAsContext();
        }
    }

    private void SetTransitionActivator(TransitionRequest req)
    {
        Transition.Type = req;
    }

    public void Draw()
    {
        int transitionsBetweenNodesCount = toNode.State.CountTransitionsTo(fromNode.ID);
        transitionsBetweenNodesCount += fromNode.State.CountTransitionsTo(toNode.ID);

        Vector2[] drawPoints = FigureOutPoints(fromNode.Rect, toNode.Rect,
            FigureOuttransitionPlacementNumber(transitionsBetweenNodesCount)          
            , transitionsBetweenNodesCount);
        DrawTransitionLine(drawPoints[0], drawPoints[1], editor.TransitionArrowSize, toNode);

        if (Handles.Button(Vector2.Lerp(drawPoints[0],drawPoints[1], 0.5f), Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            OnTransitionClicked();
        }
    }

    private int FigureOuttransitionPlacementNumber(int countTnToFn)
    {
        //if transition towards fromNode from toNode
        //i want the "older transition" to have placment priority towards the center
        //so check the editors transiton index
        if (countTnToFn > 0)
        {
            IReadOnlyList<Transition> transitionsToNode = toNode.State.GetTransitionsTo(fromNode.ID);
            IReadOnlyList<Transition> transitionsFromNode = fromNode.State.GetTransitionsTo(toNode.ID, Transition);

            int thisTransIdx = editor.GetTransitionIdx(this);
            List<int> editorListIdxs = editor.GetIndicesOfTransitions(transitionsToNode);

            editorListIdxs.AddRange(editor.GetIndicesOfTransitions(transitionsFromNode));

            editorListIdxs.Add(thisTransIdx);
            editorListIdxs.Sort();

            return editorListIdxs.IndexOf(thisTransIdx);
        }
        else
        {
            //just the normal index of my node
            return fromNode.State.GetTransitionIDX(Transition);
        }
    }

    private Vector2[] FigureOutPoints(Rect center1, Rect center2,int transitionPlacementNum, int transitionCount)
    {
        Vector2[] points = new Vector2[2];

        float mod = 1f;
        if (transitionPlacementNum % 2 == 0)
        {
            mod = 1f;
        }
        else
        {
            mod = -1f;
        }

        float t = (((float)transitionPlacementNum / (float)transitionCount) * 0.5f * mod) + 0.5f;
        float absDot = Mathf.Abs(Vector2.Dot(center1.center.normalized, center2.center.normalized));

        Vector2 minPos1, maxPos1,minPos2,maxPos2;
        if (absDot >= Mathf.Cos(45f * Mathf.Deg2Rad))
        {
            //use x axis to scale
            minPos1 = new Vector2(center1.xMin, center1.center.y);
            maxPos1 = new Vector2(center1.xMax, center1.center.y);

            minPos2 = new Vector2(center2.xMin, center2.center.y);
            maxPos2 = new Vector2(center2.xMax, center2.center.y);
        }
        else
        {
            //use y axis to sacel
            minPos1 = new Vector2(center1.center.x, center1.yMin);
            maxPos1 = new Vector2(center1.center.x, center1.yMax);

            minPos2 = new Vector2(center2.center.x, center2.yMin);
            maxPos2 = new Vector2(center2.center.x, center2.yMax);
        }

        points[0] = Vector2.Lerp(minPos1, maxPos1, t);
        points[1] = Vector2.Lerp(minPos2, maxPos2, t);

        return points;
    }

    public static void DrawTransitionLine(Vector2 from, Vector2 to,float arrowSize,EditorStateNode toNode = null)
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
        Handles.DrawPolyLine(from, toPoint, p1, p2, toPoint);
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
        men.AddItem(new GUIContent("Remove"), false, RemoveTransition);
        men.ShowAsContext();
    }

    private void RemoveTransition()
    {
        fromNode.State.RemoveTransition(Transition);
        editor.RemoveTransition(this);
    }

    private void InspectTransition()
    {
        editor.InformOfTransitionEdit(this);
    }
}