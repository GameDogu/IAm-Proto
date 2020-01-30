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

    public State(uint id, string name)
    {
        ID = id;
        this.name = name;
    }

    public abstract void Start(T prevState);
}

[Serializable]
public class MovementState : State<MovementState>
{
    //TODO something better than list for contain
    [SerializeField]List<EntityMovementOption> allowedMovements;
    [SerializeField] List<Transition> transitions;

    public MovementState(uint id,string name):base(id,name)
    {
        allowedMovements = new List<EntityMovementOption>();
        transitions = new List<Transition>();
    }

    public override void Start(MovementState prevState)
    {
        HandleAllowedMovements(prevState.allowedMovements);
        InitializeTransitions();
    }

    private void InitializeTransitions()
    {
        for (int i = 0; i < transitions.Count; i++)
        {
            transitions[i].Initialize();
        }
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

    public void AddMovementOption(EntityMovementOption option)
    {
        if (allowedMovements.Contains(option))
            return;
        allowedMovements.Add(option);
    }

    public void RemoveMovementOption(EntityMovementOption option)
    {
        allowedMovements.Remove(option);
    }

    public void AddTransition(Transition transition)
    {
        transitions.Add(transition);
    }

    public void RemoveTransition(Transition transition)
    {
        transitions.Remove(transition);
    }

}

public class Transition
{
    EntityMovementOption activator;
    public uint NextStateID { get; protected set; }

    public Transition(EntityMovementOption activator, uint nextStateID)
    {
        this.activator = activator ?? throw new ArgumentNullException(nameof(activator));
        NextStateID = nextStateID;
    }

    public void Initialize()
    {
        RegisterToStateChangeEvent();
    }

    void RegisterToStateChangeEvent()
    {
        UnregisterFromStateChangeEvent();//no double subbing
        activator.OnStateChangeAction += OnStateChangeEvent;
    }

    void UnregisterFromStateChangeEvent()
    {
        activator.OnStateChangeAction -= OnStateChangeEvent;
    }

    void OnStateChangeEvent(EntityMovementOption invokee)
    {
        throw new NotImplementedException();

        //notify state machine of what state we want to go into next
    }

    public override bool Equals(object obj)
    {
        if (obj is Transition)
        {
            var tra = obj as Transition;

            if (tra.NextStateID == NextStateID && tra.activator == activator)
                return true;

        }
        return false;
    }

    public override int GetHashCode()
    {
        var hashCode = 62885100;
        hashCode = hashCode * -1521134295 + EqualityComparer<EntityMovementOption>.Default.GetHashCode(activator);
        hashCode = hashCode * -1521134295 + NextStateID.GetHashCode();
        return hashCode;
    }
}

