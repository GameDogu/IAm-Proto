using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

public class MovementStateMachineEditor : EditorWindow
{
    public event Action<EditorStateNode> OnNodeSelected;
    public event Action OnNodeDeselected;

    EntityEditedInfoWindow entityInfoEditor;
    public MovementStateMachine StateMachine => entityInfoEditor.EntityEdited;
    StateEditorWindow stateInfoEditor;

    GUIStyle normalStyle;
    GUIStyle selectedStyle;

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

    EditorStateNode selectedNode;
    public EditorStateNode SelectedNode
    {
        get { return selectedNode; }
        protected set
        {
            if (value != null && value != selectedNode)
            {
                selectedNode = value;
                OnNodeSelected?.Invoke(selectedNode);
            }
            else if (value == null && selectedNode != null)
            {
                selectedNode.DeselectNode();
                selectedNode = value;
                OnNodeDeselected?.Invoke();
            }
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
            return transitionCreationHelper;
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
        changed = false;

        Nodes = new List<EditorStateNode>();

        normalStyle = new GUIStyle();
        normalStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        normalStyle.alignment = TextAnchor.MiddleCenter;
        normalStyle.border = new RectOffset(12, 12, 12, 12);

        selectedStyle = new GUIStyle();
        selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedStyle.alignment = TextAnchor.MiddleCenter;
        selectedStyle.border = new RectOffset(12, 12, 12, 12);

    }

    void OnEntityEditedChanged()
    {
        SelectedNode = null;
        Nodes.Clear();
        offset = Vector2.zero;
        drag = Vector2.zero;
        LoadEntity();
    }

    private void LoadEntity()
    {
        //TODO for each state end transition -> create drawables
        Debug.Log("TODO");
        Repaint();
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
            SelectedNode = null;
        }
        if (e.button == 1)
        {
            HandleRightClick(e);
        }
    }

    private void HandleRightClick(Event e)
    {
        if (entityInfoEditor.HasEntity)
            MakeContextMenu(e.mousePosition);
    }

    private void MakeContextMenu(Vector2 mousePosition)
    {
        GenericMenu men = new GenericMenu();
        men.AddItem(new GUIContent("Create New State"), false, () => CreateNewState(mousePosition));
        men.ShowAsContext();
    }

    public void Save()
    {
        entityInfoEditor.Save();
    }

    void CreateNewState(Vector2 pos)
    {
        MovementState state = entityInfoEditor.EntityEdited.AddNewState();
        CreateEditorNode(state,pos);
    }

    private void CreateEditorNode(MovementState state,Vector2 pos)
    {
        EditorStateNode newNode = new EditorStateNode(this, state, pos, new NodeStyle() { Width = 100, Height = 50, style = normalStyle, selectedStyle = selectedStyle });
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

        nodeMenu.AddItem(new GUIContent("Create Transition"), false, () => transitionCreationHelper.OnNodeClicked(obj));
        nodeMenu.AddItem(new GUIContent("Destroy"), false, () => obj.Destroy());
        nodeMenu.ShowAsContext();
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
        DrawNodes();
        DrawTransitions();
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
        //TODO
    }

    void DrawNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].Draw();
        }
    }

    public void SetNodeSelected(EditorStateNode node)
    {
        SelectedNode = node;
    }

    internal void CreateTransition(EditorStateNode from, EditorStateNode to)
    {
        throw new NotImplementedException();
    }
}

public class TransitionCreationHelper
{
    MovementStateMachineEditor editor;

    EditorStateNode from;
    EditorStateNode to;

    public TransitionCreationHelper(MovementStateMachineEditor editor)
    {
        this.editor = editor ?? throw new ArgumentNullException(nameof(editor));
    }

    public void OnNodeClicked(EditorStateNode n)
    {
        if (from == null)
            from = n;
        else
        {
            to = n;
            FinishTransition();
        }
    }

    private void FinishTransition()
    {
        editor.CreateTransition(from, to);
        from = to = null;
    }
}