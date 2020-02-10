using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovementState : State<MovementState>
{
    public MovementStateMachine StateMachine { get; protected set; }

    [SerializeField] public List<EntityMovementOption> MovementOptions { get; protected set; }
    [SerializeField] List<Transition> transitions;
    [SerializeField] public bool InitialState { get; set; }

    public StateMovementHandler MovementHandler { get; protected set; }

    public MovementState(uint id,string name,MovementStateMachine stateMachine):base(id,name)
    {
        this.StateMachine = stateMachine;
        MovementOptions = new List<EntityMovementOption>();
        transitions = new List<Transition>();
        MovementHandler = new StateMovementHandler(this,stateMachine.Player);
    }

    protected void FillMovementOptions(List<int> optionsIndexed)
    {
        for (int i = 0; i < optionsIndexed.Count; i++)
        {
            var opt = StateMachine.GetMovementOption(optionsIndexed[i]);
            MovementOptions.Add(opt);
        }
    }

    public void Initialize(List<int> movementOptionsForState)
    {
        FillMovementOptions(movementOptionsForState);

        MovementHandler.Initialize();
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
            MovementOptions = new List<EntityMovementOption>();
        }

        if (MovementOptions.Contains(option))
            return; //already contained don't double add
        MovementOptions.Add(option);
    }

    public void RemoveMovementOption(EntityMovementOption option)
    {
        if (MovementOptions.Remove(option))
        {
            for (int i = transitions.Count-1; i >= 0 ; i--)
            {
                var tran = transitions[i];
                if (tran.Type.IsSameRequest(option.TransitionRequst))
                {
                    transitions.RemoveAt(i);
                }
            }
        }
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
}

