using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TEMP
using System.Linq;

public class MovementStateMachine : MonoBehaviour
{
    static uint idGen = 0;
    static uint IDGen => idGen++;

    [SerializeField] List<EntityMovementOption> generalMovementOptions = null;
    public List<EntityMovementOption> GeneralMovementOptions => generalMovementOptions;
    [SerializeField] Player player = default;
    public Player Player => player;
    public MovementState CurrentState { get; protected set; }
    [SerializeField] List<MovementState> movementStates = new List<MovementState>();
    public int StateCount => movementStates.Count;

    Dictionary<uint, MovementState> idMappedMovementStates = new Dictionary<uint, MovementState>();

    public EntityMovementOption GetMovementOption(int idx)
    {
        if (idx >= generalMovementOptions.Count)
            throw new IndexOutOfRangeException();
        return generalMovementOptions[idx];
    }

    public int GetIndexForMovementOption(EntityMovementOption option)
    {
        return generalMovementOptions.IndexOf(option);
    }

    public void AddGeneralMovementOption(EntityMovementOption option)
    {
        if (generalMovementOptions == null)
        {
            Debug.Log("New List");
            generalMovementOptions = new List<EntityMovementOption>();
        }

        if (!generalMovementOptions.Contains(option))
        {
            generalMovementOptions.Add(option);
        }

    }

    Queue<EntityMovementOption> stateChangeRequests = new Queue<EntityMovementOption>();

    public void Awake()
    {
        CurrentState = new MovementState(0, "Everything Bagel", this);
        var options = Enumerable.Range(0, generalMovementOptions.Count);
        CurrentState.Initialize(options.ToList());

        BuildStateMappingAndInitialize();
    }

    private void BuildStateMappingAndInitialize()
    {
        for (int i = 0; i < StateCount; i++)
        {
            var state = movementStates[i];
            idMappedMovementStates.Add(state.ID, state);
            if (state.InitialState)
                CurrentState = state;
        }
    }

    public void RemoveState(MovementState state)
    {
        Debug.LogWarning("Not Implemented");
    }

    private void Start()
    {
        CurrentState.Start(null);
    }

    public void RemoveGeneralMovementOption(EntityMovementOption opt)
    {
        int idx = generalMovementOptions.IndexOf(opt);

        if (idx < 0)
            return;

        if (idx < StateCount - 1)
        {
            generalMovementOptions[idx] = generalMovementOptions[StateCount - 1];
            idx = StateCount - 1;
        }
        generalMovementOptions.RemoveAt(idx);


        for (int i = 0; i < StateCount; i++)
        {
            var st = movementStates[i];
            st.RemoveMovementOption(opt);
        }
    }

    MovementState GetStateByID(uint id)
    {
        if (idMappedMovementStates.ContainsKey(id))
        {
            return idMappedMovementStates[id];
        }
        throw new Exception($"Trying to transition to non recognized state {id}");
    }

    private void Update()
    {
        HandlePotentialTransition();

        CurrentState.Update();

        HandlePotentialTransition();
    }

    private void FixedUpdate()
    {
        HandlePotentialTransition();

        CurrentState.FixedUpdate();

        HandlePotentialTransition();
    }

    public void Transition(Transition trans)
    {
        //change to new state
        var prevState = CurrentState;
        CurrentState = GetStateByID(trans.NextStateID);
        ClearStateChangeRequests();
        CurrentState.Start(prevState);
    }

    public bool AddNewState(MovementState movementState)
    {
        if (!movementStates.Exists(st => st.ID == movementState.ID))
        {
            movementStates.Add(movementState);
            return true;
        }
        return false;
    }

    public MovementState AddNewState()
    {
        uint id = IDGen;
        var st = new MovementState(id, $"State {id}", this);
        movementStates.Add(st);
        return st;
    }

    public void OnStateMarkedAsInitial(MovementState newlyMarked)
    {
        for (int i = 0; i < StateCount; i++)
        {
            var state = movementStates[i];
            if (state != newlyMarked)
            {
                state.InitialState = false;
            }
        }
    }

    public void RemoveTransition(Transition transition)
    {
        for (int i = 0; i < StateCount; i++)
        {
            if (movementStates[i].RemoveTransition(transition))
            {
                break;
            }
        }
    }

    public void RemoveTransition(Transition transition, MovementState state)
    {
        state.RemoveTransition(transition);
    }

    //private void OnDrawGizmos()
    //{
    //    if (movementStates != null)
    //    {
    //        for (int i = 0; i < movementStates.Count; i++)
    //        {
    //            movementStates[i].Print(Debug.unityLogger);
    //        }
    //    }
    //}

    public void HandlePotentialTransition()
    {
        if (stateChangeRequests.Count > 0)
        {
            Transition trans = null;
            while (stateChangeRequests.Count > 0 && trans == null)
            {
                var activator = stateChangeRequests.Dequeue();
                trans = CurrentState.CheckTransitionRequest(activator);
            }
            if (trans != null)
            {
                Transition(trans);
            }
        }
        ClearStateChangeRequests();
    }

    public void ClearStateChangeRequests()
    {
        stateChangeRequests.Clear();
    }

    public void EnqueRequestStateChange(EntityMovementOption requestor)
    {
        stateChangeRequests.Enqueue(requestor);
    }
}
