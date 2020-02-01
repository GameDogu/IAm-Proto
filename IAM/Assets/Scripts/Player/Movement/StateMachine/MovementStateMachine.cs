using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TEMP
using System.Linq;

public class MovementStateMachine : MonoBehaviour
{
    [SerializeField] List<EntityMovementOption> generalMovementOptions = null;
    [SerializeField] Player player = default;
    public Player Player => player;
    public MovementState CurrentState { get; protected set; }
    [SerializeField] List<MovementState> movementStates;

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
        throw new NotImplementedException();
    }
}
