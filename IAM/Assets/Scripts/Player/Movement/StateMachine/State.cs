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
    //TODO something better than list for contain
    [SerializeField]List<EntityMovementOption> allowedMovements = null;

    protected override void Start(MovementState prevState)
    {
        HandleAllowedMovements(prevState.allowedMovements);
    }

    private void HandleAllowedMovements(List<EntityMovementOption> prevAllowedMovements)
    {
        //i want to loop over all my allowed movements
        //if they are also in the prev states movement i don't do anything
        //otherwise i call StartUp on it
        CompareAndAct(allowedMovements, prevAllowedMovements, (s) => s.StartUp());

        //loop over the list in the prev state if they are not in my list
        //stop them.
        CompareAndAct(prevAllowedMovements, allowedMovements, (s) => s.Stop());
    }

    void CompareAndAct(List<EntityMovementOption> loop, List<EntityMovementOption> contain, Action<EntityMovementOption> action)
    {
        for (int i = 0; i < loop.Count; i++)
        {
            var mOpt = loop[i];
            if (!contain.Contains(mOpt))
                action(mOpt);
        }
    }

}