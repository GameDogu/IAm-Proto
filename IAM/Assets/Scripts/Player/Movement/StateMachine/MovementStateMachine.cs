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
    public List<EntityMovementOption> GeneralMovementOption => generalMovementOptions;
    [SerializeField] Player player = default;
    public Player Player => player;
    public MovementState CurrentState { get; protected set; }
    [SerializeField] List<MovementState> movementStates = new List<MovementState>();
    public int StateCount => movementStates.Count;

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

    public void Awake()
    {
        CurrentState = new MovementState(0, "Everything Bagel", this);
        var options = Enumerable.Range(0, generalMovementOptions.Count);
        CurrentState.Initialize(options.ToList());
    }

    private void Start()
    {
        CurrentState.Start(null);
    }

    private void Update()
    {
        CurrentState.Update();
    }

    private void FixedUpdate()
    {
        CurrentState.FixedUpdate();
    }

    public void Transition(Transition trans)
    {
        //TODO ayy lmao trans people lol
        throw new NotImplementedException();
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
}
