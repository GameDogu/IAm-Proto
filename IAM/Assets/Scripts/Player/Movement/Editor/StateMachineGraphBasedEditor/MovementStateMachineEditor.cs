﻿#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

public class MovementStateMachineEditor : EditorWindow
{

    [SerializeField] MovementStateMachineEditorSettings settings;

    public event Action<EditorStateNode> OnNodeSelected;
    public event Action OnNodeDeselected;

    EntityEditedInfoWindow entityInfoEditor;
    public MovementStateMachine StateMachine => entityInfoEditor.EntityEdited;
    StateEditorWindow stateInfoEditor;

    NodeStyle style
    {
        get
        {
            if (settings == null)
                return NodeStyle.Default();
            else
                return settings.NodeStyle;
        }
    }

    public float TransitionArrowSize
    {
        get
        {
            if (settings == null)
                return 15f;
            return settings.ArrowSize;
        }
    }

    public float TransitionOutPointSize
    {
        get
        {
            if (settings == null)
                return 5f;
            return settings.TransitionOutPointSize;
        }
    }

    public float TransitionOutPointRadiusSelected
    {
        get
        {
            if (settings == null)
                return 1.25f;
            return settings.TransitionOutPointRadiusSelected;
        }
    }
    public float TransitionOutPointRadius
    {
        get
        {
            if (settings == null)
                return 0.9f;
            return settings.TransitionOutPointRadius;
        }
    }

    bool changed;

    List<EditorStateNode> nodes;
    public List<EditorStateNode> Nodes
    {
        get
        {
            if (nodes == null)
                nodes = new List<EditorStateNode>();
            return nodes;
        }
        protected set
        {
            nodes = value;
        }
    }

    List<EditorTransition> transitions;
    public List<EditorTransition> Transitions
    {
        get
        {
            if (transitions == null)
                transitions = new List<EditorTransition>();
            return transitions;
        }
        protected set { transitions = value; }
    }

    EditorStateNode selectedNode;
    public EditorStateNode SelectedNode
    {
        get { return selectedNode; }
        protected set
        {
            if (value != null)
            {
                if (selectedNode != null)
                    selectedNode.Deselect();
                selectedNode = value;
                OnNodeSelected?.Invoke(selectedNode);
            }
            else if (value == null && selectedNode != null)
            {
                selectedNode.Deselect();
                selectedNode = value;
                OnNodeDeselected?.Invoke();
            }
        }
    }

    public Color TransitionColor
    {
        get
        {
            if (settings == null)
                return Color.black;
            return settings.TransitionColor;
        }
    }

    Vector2 offset;
    Vector2 drag;

    TransitionCreationHelper _transitionCreationHelper;
    TransitionCreationHelper transitionCreationHelper
    {
        get
        {
            if (_transitionCreationHelper == null)
                _transitionCreationHelper = new TransitionCreationHelper(this);
            return _transitionCreationHelper;
        }
    }

    [MenuItem("Movement/Statemachine Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<MovementStateMachineEditor>();

        //AnimatorController Icon
        //BlendTree Icon
        window.titleContent = new GUIContent() { text = "Movement Statemachine Editor", image = EditorGUIUtility.IconContent("AnimatorController Icon").image };

        window.Initialize();
    }

    void Initialize()
    {
        //TODO if anything
        entityInfoEditor = CreateWindow<EntityEditedInfoWindow>(GetType());
        stateInfoEditor = CreateWindow<StateEditorWindow>(GetType(), entityInfoEditor.GetType());

        entityInfoEditor.Initialize(this, stateInfoEditor);
        stateInfoEditor.Initialize(this, entityInfoEditor);

        entityInfoEditor.OnEditedEntityChanged -= OnEntityEditedChanged;
        entityInfoEditor.OnEditedEntityChanged += OnEntityEditedChanged;

        entityInfoEditor.OnEntityReloaded -= OnEntityEditedChanged;
        entityInfoEditor.OnEntityReloaded += OnEntityEditedChanged;

        changed = false;

        Nodes = new List<EditorStateNode>();
        Transitions = new List<EditorTransition>();
    }

    void OnEntityEditedChanged()
    {
        SelectedNode = null;
        Nodes.Clear();
        transitionCreationHelper.Reset();
        Transitions.Clear();
        offset = Vector2.zero;
        drag = Vector2.zero;
        LoadEntity();
    }

    private void LoadEntity()
    {
        //TODO for each state end transition -> create drawables
        if (StateMachine == null)
            return;
        if (!StateMachine.IsLoaded)
            StateMachine.LoadFromData();

        if (StateMachine.StateCount == 0 && StateMachine.IsLoaded)
        {
            //quick fix maybe load failed?
            StateMachine.ReloadFromData();
        }

        CreateNewNodes();
        CreateNewTransitions();

        Repaint();
    }

    private void CreateNewTransitions()
    {
        for (int i = 0; i < StateMachine.StateCount; i++)
        {
            var state = StateMachine.States[i];
            for (int j = 0; j< state.TransitionCount; j++)
            {
                var trans = state.Transitions[j];

                EditorTransition editTrans = new EditorTransition(trans, this);
                Transitions.Add(editTrans);
            }
        }
    }

    public bool HasEventModifierPrioritizingEditorHandling(Event e)
    {
        return e.control;
    }

    private void CreateNewNodes()
    {
        if (StateMachine.StateCount > 0)
        {
            float degChangePerNode = 360f / (float)StateMachine.StateCount;

            for (int i = 0; i < StateMachine.StateCount; i++)
            {
                Vector2 pos = GetNodePosition(degChangePerNode, position.width,position.height,100f,50f, i, -90f);
                CreateEditorNode(StateMachine.States[i],pos);
            }
        }
    }

    internal void RemoveTransitionsForNodeOfType(EditorStateNode editorStateNode, Type[] types)
    {
        TransitionRequest[] requestTypes = new TransitionRequest[types.Length];

        for (int i = 0; i < types.Length; i++)
        {
            requestTypes[i] = TransitionRequest.Factory.BuildRequest(types[i]);
        }


        for (int i = Transitions.Count-1; i >= 0; i--)
        {
            var tran = transitions[i].Transition;
            if (tran.StateBelongingToID == editorStateNode.ID)
            {
                //this transition belongs to this node
                //check if we have a type in our list same as this transition
                if (requestTypes.Any(t => t.IsSameRequest(tran.Type)))
                {
                    transitions.RemoveAt(i);
                }
            }
        }
        
    }

    private Vector2 GetNodePosition(float degChangePerNode, float width,float height,float nodeWidth, float nodeHeight, int i,float shift= 0f)
    {
        float rad = (shift + (degChangePerNode * i))*Mathf.Deg2Rad;
        float wR = (width * .5f) - (nodeWidth * .5f);
        float hR = (height * .5f) - (nodeHeight * .5f);
        return new Vector2(Mathf.Cos(rad)*wR, Mathf.Sin(rad)*hR) + new Vector2(wR,hR);
    }

    public int GetTransitionIdx(EditorTransition editorTransition)
    {
        return Transitions.IndexOf(editorTransition);
    }

    public List<int> GetIndicesOfTransitions(IReadOnlyList<Transition> transitions)
    {
        List<int> idxList = new List<int>();

        for (int i = 0; i < transitions.Count; i++)
        {
            idxList.Add(Transitions.FindIndex(tran => tran.Transition == transitions[i]));
        }

        return idxList;
    }

    public void OnGUI()
    {     
        Draw();

        if (entityInfoEditor.HasEntity)
        {
            changed |= ProcessNodeEvents(Event.current);
        }
        changed |= ProcessEvent(Event.current);
        if (changed) Repaint();
    }

    private bool ProcessNodeEvents(Event current)
    {
        bool change = false;
        for (int i = 0; i < Nodes.Count; i++)
        {
            change |= Nodes[i].ProcessEvent(current);
        }
        return change;
    }

    private bool ProcessEvent(Event e)
    {
        drag = Vector2.zero;
        switch (e.type)
        {
            case EventType.MouseDown:
                HandleMouseDown(e);
                return true;
            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    if (e.control && SelectedNode != null)
                    {
                        HandleNodeLayoutRotation(e);
                    }
                    else
                        Drag(e.delta);
                    return true;
                }
                
                return false;
            default:
                return false;
        }
    }

    private void Drag(Vector2 delta)
    {
        drag = delta;
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].Drag(drag);
        }
    }

    private void HandleMouseDown(Event e)
    {
        if (e.button == 0)
        {
            if (e.control && SelectedNode != null)
            {
                HandleNodeLayoutRotation(e);
                return;
            }

            SelectedNode = null;
            if (transitionCreationHelper.IsMakingTransition)
                transitionCreationHelper.Reset();
        }
        if (e.button == 1)
        {
            HandleRightClick(e);
        }
    }

    private void HandleNodeLayoutRotation(Event e)
    {
        SelectedNode.RotateLayout(e.mousePosition,e.type==EventType.MouseDown?-1:1);
        e.Use();
    }

    private void HandleRightClick(Event e)
    {
        if (entityInfoEditor.HasEntity)
            MakeContextMenu(e.mousePosition);
    }

    internal void InformOfTransitionEdit(EditorTransition editorTransition)
    {
        stateInfoEditor.CurrentObjectDisplayed = editorTransition;
    }

    private void MakeContextMenu(Vector2 mousePosition)
    {
        GenericMenu men = new GenericMenu();
        men.AddItem(new GUIContent("Create New State"), false, () => CreateNewState(mousePosition));
        men.ShowAsContext();
    }

    public void RemoveTransition(EditorTransition editorTransition)
    {
        Transitions.Remove(editorTransition);
    }

    public void Save()
    {
        entityInfoEditor.Save();
        Repaint();
    }

    void CreateNewState(Vector2 pos)
    {
        MovementState state = entityInfoEditor.EntityEdited.AddNewState();
        CreateEditorNode(state,pos);
    }

    private void CreateEditorNode(MovementState state,Vector2 pos)
    {
        EditorStateNode newNode = new EditorStateNode(this, state, pos, style);
        nodes.Add(newNode);

        newNode.OnRightClick -= OnNodeRightClicked;
        newNode.OnRightClick += OnNodeRightClicked;

        newNode.OnDestroyed -= RemoveNode;
        newNode.OnDestroyed += RemoveNode;

        newNode.SelectNode();
    }

    private void OnNodeRightClicked(EditorStateNode obj)
    {
        GenericMenu nodeMenu = new GenericMenu();

        if(obj.IsInitialState)
            nodeMenu.AddItem(new GUIContent("Unmakr As Initial"), false, () => obj.SetInitialState(false));
        else
            nodeMenu.AddItem(new GUIContent("Mark As Initial"), false, () => { obj.SetInitialState(true); InformOfIntialMark(obj.State); });

        nodeMenu.AddItem(new GUIContent("Destroy"), false, () => obj.Destroy());
        nodeMenu.ShowAsContext();
    }

    public void OnTransitionOutPointClicked(TransitionOutPoint outpoint)
    {
        transitionCreationHelper.OnTransitionOutPointClicked(outpoint);
        //TODO
    }

    public void InformOfIntialMark(MovementState state)
    {
        StateMachine.OnStateMarkedAsInitial(state);
        Repaint();
    }

    public void RemoveNode(EditorStateNode n,MovementState state)
    {
        entityInfoEditor.RemoveState(state);
        nodes.Remove(n);
    }

    private void Draw()
    {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        if (!entityInfoEditor.HasEntity)
            return;
        DrawTransitions();
        transitionCreationHelper.Draw(Event.current.mousePosition);
        DrawNodes();
        DrawGUI();
    }

    private void DrawGUI()
    {
        if (StateMachine == null)
            return;
        //TODO
        if (GUILayout.Button("Save"))
        {
            MovementStateMachineData data = null;
            MovementStateMachineData.Save(StateMachine,out data);
            if (data != null)
            {
                StateMachine.MovementStateMachineDataAssetPath = AssetDatabase.GetAssetPath(data);
            }
        }
    }

    /// <summary>
    /// https://gram.gs/gramlog/creating-node-based-editor-unity/
    /// </summary>
    /// <param name="gridSpacing"></param>
    /// <param name="gridOpacity"></param>
    /// <param name="gridColor"></param>
    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    void DrawTransitions()
    {
        for (int i = Transitions.Count - 1; i >= 0; i--)
        {
            Transitions[i].Draw();
        }
    }

    void DrawNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].DrawNode();
        }
    }

    public bool SetNodeSelected(EditorStateNode node)
    {
        if (transitionCreationHelper.IsMakingTransition)
        {
            transitionCreationHelper.OnNodeClicked(node);
            return false;
        }
        else
            SelectedNode = node;
        return true;
    }

    internal void CreateTransition(TransitionOutPoint from, EditorStateNode to)
    {
        var trans = new EditorTransition(from,to,this);
        Transitions.Add(trans);
        changed = true;
    }

    public EditorStateNode GetStateByID(uint id)
    {
        return Nodes.Find(node => node.ID == id);
    }
}

public class TransitionCreationHelper
{
    MovementStateMachineEditor editor;

    TransitionOutPoint from;
    EditorStateNode to;

    public bool IsMakingTransition => from != null;

    public TransitionCreationHelper(MovementStateMachineEditor editor)
    {
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public void OnNodeClicked(EditorStateNode n)
    {
        to = n;
        TryFinishTransition();
    }

    public void OnTransitionOutPointClicked(TransitionOutPoint pnt)
    {
        if (pnt.HasTransition)
            return;
        from = pnt;
    }

    public void Draw(Vector2 mousePos)
    {
        if (from != null)
        {
            EditorTransition.DrawTransitionLine(from.Position, mousePos,editor.TransitionArrowSize,editor.TransitionColor);
        }
    }

    public void Reset()
    {
        from = null;
        to = null;
    }

    private void TryFinishTransition()
    {
        if (from != null && !from.HasTransition)
        {
            editor.CreateTransition(from, to);
        }
        Reset();
    }

}