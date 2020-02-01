using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovementState : State<MovementState>
{
    public MovementStateMachine StateMachine { get; protected set; }

    [SerializeField] public List<EntityMovementOption> MovementOptions { get; protected set; }
    [SerializeField] List<Transition> transitions;

    Queue<EntityMovementOption> stateChangeRequests;
    public StateMovementHandler MovementHandler { get; protected set; }

    public MovementState(uint id,string name,MovementStateMachine stateMachine):base(id,name)
    {
        this.StateMachine = stateMachine;
        MovementOptions = new List<EntityMovementOption>();
        transitions = new List<Transition>();
        MovementHandler = new StateMovementHandler(this,stateMachine.Player);
        stateChangeRequests = new Queue<EntityMovementOption>();
    }

    protected void FillMovementOptions(List<int> optionsIndexed)
    {
        for (int i = 0; i < optionsIndexed.Count; i++)
        {
            MovementOptions.Add(StateMachine.GetMovementOption(optionsIndexed[i]));
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

    public Transition CheckTransitionRequest(EntityMovementOption option)
    {
        return transitions.Find(tra => tra.Activator == option);
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
        MovementOptions.Remove(option);
    }

    public void AddTransition(Transition transition)
    {
        transitions.Add(transition);
    }

    public void RemoveTransition(Transition transition)
    {
        transitions.Remove(transition);
    }

    public void HandleStateChangeRequest()
    {
        if (stateChangeRequests.Count > 0)
        {
            Transition trans = null;
            while (stateChangeRequests.Count > 0 && trans == null)
            {
                var activator = stateChangeRequests.Dequeue();
                trans = CheckTransitionRequest(activator);
            }
            if (trans != null)
            {
                StateMachine.Transition(trans);
            }
        }
    }


    public void EnqueRequestStateChange(EntityMovementOption requestor)
    {
        stateChangeRequests.Enqueue(requestor);
    }

    public void ClearStateChangeRequests()
    {
        stateChangeRequests.Clear();
    }

}

