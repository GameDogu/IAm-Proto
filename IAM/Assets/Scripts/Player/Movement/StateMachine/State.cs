using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class State<T>
{
    public uint ID { get; protected set; }
    [SerializeField]string name = "";
    public string Name => name;

    protected abstract void Start(T prevState);
}

[Serializable]
public class MovementState : State<MovementState>
{
    [SerializeField]List<PlayerMovement> allowedMovements;

    protected override void Start(MovementState prevState)
    {
        HandleAllowedMovements(prevState.allowedMovements);
    }

    private void HandleAllowedMovements(List<PlayerMovement> prevAllowedMovements)
    {
        //i want to loop over all my allowed movements
        //if they are also in the prev states movement i don't do anything
        //otherwise i call StartUp on it
        
        //loop over the list in the prev state if they are not in my list
        //end em.
    }
}