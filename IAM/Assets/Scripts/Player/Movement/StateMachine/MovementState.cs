using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementState : State<MovementState>
{
    public MovementStateMachine StateMachine { get; protected set; }

    List<EntityMovementOption> movementOptions;

    public IReadOnlyList<EntityMovementOption> MovementOptions => movementOptions;

    List<Transition> transitions;

    public IReadOnlyList<Transition> Transitions => transitions;

    public int TransitionCount => transitions.Count;

    public bool IsInitialState { get; set; }

    public StateMovementHandler MovementHandler { get; protected set; }

    bool isInitialized;

    public MovementState(uint id,string name,MovementStateMachine stateMachine):base(id,name)
    {
        this.StateMachine = stateMachine;
        movementOptions = new List<EntityMovementOption>();
        transitions = new List<Transition>();
        MovementHandler = new StateMovementHandler(this,stateMachine.Player);
        isInitialized = false;
    }

    protected void FillMovementOptions(List<string> optionsIndexed)
    {
        for (int i = 0; i < optionsIndexed.Count; i++)
        {
            var opt = StateMachine.GetMovementOption(optionsIndexed[i]);
            movementOptions.Add(opt);
        }
    }
    
    void Initialize(List<string> movementOptionsForState)
    {
        if (isInitialized)
            return;
        FillMovementOptions(movementOptionsForState);

        MovementHandler.Initialize();
        isInitialized = true;
    }

    void OnValidate()
    {
        MovementHandler.OnValidate();
    }

    public override void Start(MovementState prevState)
    {
        MovementHandler.Start();
    }

    // Update is called once per frame
    public void Update()
    {
        MovementHandler.Update();
    }

    public void FixedUpdate()
    {
        MovementHandler.FixedUpdate();
    }

    public Transition CheckTransitionRequest(TransitionRequest request)
    {
        return transitions.Find(tra => tra.CheckRequest(request));
    }

    public void RequestStateChange(TransitionRequest request)
    {
        StateMachine.EnqueRequestStateChange(request);
    }

    public void AddMovementOption(EntityMovementOption option)
    {
        if (MovementOptions == null)
        {
            movementOptions = new List<EntityMovementOption>();
        }

        if (movementOptions.Contains(option))
            return; //already contained don't double add
        movementOptions.Add(option);
    }

    public void RemoveMovementOption(EntityMovementOption option)
    {
        if (movementOptions.Remove(option))
        {
            PossibleTransitionRequestTypesAttribute att = Attribute.GetCustomAttribute(option.GetType(), typeof(PossibleTransitionRequestTypesAttribute)) as PossibleTransitionRequestTypesAttribute;
            if (att != null)
            {
                for (int i = transitions.Count-1; i >= 0 ; i--)
                {
                    var tran = transitions[i];

                    for (int j = 0; j < att.Types.Length; j++)
                    {
                        var type = TransitionRequest.Factory.BuildRequest(att.Types[j]);
                        if (tran.Type.IsSameRequest(type))
                        {
                            transitions.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }

    public bool ContainsMovementOption(EntityMovementOption option)
    {
        return movementOptions.Contains(option);
    }

    public void AddTransition(Transition transition)
    {
        transitions.Add(transition);
    }

    public bool RemoveTransition(Transition transition)
    {
        return transitions.Remove(transition);
    }

    internal void Print(ILogger l)
    {
        l.Log(Name);
        if (MovementOptions != null)
        {
            for (int i = 0; i < MovementOptions.Count; i++)
            {
                l.Log(MovementOptions[i].Name);
            }
        }
    }

    public void RemoveTransition(uint nextStateID)
    {
        for (int i =transitions.Count-1; i >=0; i--)
        {
            var tran = transitions[i];
            if (tran.NextStateID == nextStateID)
            {
                transitions.RemoveAt(i);
            }
        }
    }

    [Serializable]
    public class Data
    {
        [SerializeField] uint id;
        public uint ID => id;
        [SerializeField] string name;
        public string Name => name;
        [SerializeField] bool isInitial;
        public bool IsInitial => isInitial;
        [SerializeField] List<string> allowedMovements;
        public IReadOnlyList<string> AllowedMovements => allowedMovements;
        [SerializeField] List<Transition.Data> dataTransitions;
        public IReadOnlyList<Transition.Data> DataTransitions => dataTransitions;

        public Data(MovementState state)
        {
            id = state.ID;
            name = state.Name;
            isInitial = state.IsInitialState;
            allowedMovements = new List<string>();
            for (int i = 0; i < state.movementOptions.Count; i++)
            {
                allowedMovements.Add(state.movementOptions[i].GetType().Name);
            }
            dataTransitions = new List<Transition.Data>();
            for (int i = 0; i < state.transitions.Count; i++)
            {
                dataTransitions.Add(new Transition.Data(state.transitions[i]));
            }
        }
        public MovementState Create(MovementStateMachine machineFor)
        {
            MovementState state = new MovementState(id, name, machineFor);
            state.IsInitialState = isInitial;

            state.Initialize(allowedMovements);

            for (int i = 0; i < dataTransitions.Count; i++)
            {
                state.AddTransition(dataTransitions[i].Create(machineFor));
            }

            return state;
        }
    }

}

