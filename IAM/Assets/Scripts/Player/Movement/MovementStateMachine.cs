using System;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Reflection;
using UnityEngine;

public class MovementStateMachine
{
    Player player;
    Dictionary<uint, MovementState> states;
    MovementState currentState;

    public MovementStateMachine(Player p)
    {
        player = p;
        states = new Dictionary<uint, MovementState>();
    }

    public MovementStateMachine(Player p, List<MovementState> states):this(p)
    {
        for (int i = 0; i < states.Count; i++)
        {
            this.states.Add(states[i].ID,states[i]);
        }
    }

    public MovementStateMachine(Player p, List<MovementState> states, uint initial):this(p,states)
    {
        currentState = this.states[initial];
    }

    public MovementStateMachine(Player p, List<MovementState> states, MovementState initial) : this(p, states)
    {
        currentState = initial;
    }

    public T CreateNewState<T>(string name) where T : MovementState
    {
        T state = (T)Activator.CreateInstance(typeof(T), name, this);
        return state;
    }

    public T CreateAndAddNewState<T>(string name) where T : MovementState
    {
        T state = CreateNewState<T>(name);

        states.Add(state.ID, state);
        return state;
    }

    public void TransitionToState(uint ID)
    {
        if (states.ContainsKey(ID))
        {
            currentState?.OnTranssitionOut(player);
            var prev = currentState;
            currentState = states[ID];
            currentState.OnTransitionInto(player,prev);
        }
    }

    public void Update()
    {
        currentState.Update(player);
    }

    public bool QueryIfActionAllowed(PlayerMovement.MovementActions desiredAction)
    {
        return currentState.CheckActionAllowed(player, desiredAction);
    }
}

public abstract class MovementState
{
    public const char Seperator = '|';
    static uint idCounter = 0;
    static uint IDGen => idCounter++;

    MovementStateMachine machine;
    public readonly uint ID;
    public readonly string Name;
    List<Transition> transitions;

    public MovementState(string name, MovementStateMachine machine)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
        ID = IDGen;
        Name = name;
        transitions = new List<Transition>();
    }

    protected MovementState(uint id, string name, MovementStateMachine machine)
    {
        ID = id;
        if (id > idCounter)
            idCounter = id;
        Name = name;
        this.machine = machine;
        transitions = new List<Transition>();
    }

    public void Update(Player p)
    {
        for (int i = 0; i < transitions.Count; i++)
        {
            var t = transitions[i];
            if (t.Check(p))
            {
                machine.TransitionToState(t.ToID);
            }
        }
    }

    public abstract void OnTransitionInto(Player p, MovementState previous);

    public abstract void OnTranssitionOut(Player p);

    public abstract bool CheckActionAllowed(Player p, PlayerMovement.MovementActions action);

    void AddTransition(Transition newTrans)
    {
        transitions.Add(newTrans);
    }

    void RemoveTransition(Transition t)
    {
        transitions.Remove(t);
    }

    void AddTransitions(List<Transition> newTrans)
    {
        transitions.AddRange(newTrans);
    }

    public abstract void Deserialize(string[] further);

    public string Serialize()
    {
        string s = $"{GetType().FullName}{Seperator}";

        s += $"{ID}{Seperator}";
        s += $"{Name}{Seperator}";
        s += $"{transitions.Count}{Seperator}";
        for (int i = 0; i < transitions.Count; i++)
        {
            s += transitions[i].Serialize();
            if (i < transitions.Count - 1)
                s += Seperator;
        }

        return s;
    }

    public static MovementState Deserialize(MovementStateMachine mach, string s)
    {
        string[] state = s.Split(Seperator);

        var ass = Assembly.GetExecutingAssembly();
        Type stateType = ass.GetType(state[0]);

        uint ID = uint.Parse(state[1]);
        string name = state[2];
        int count = int.Parse(state[3]);

        MovementState newState = (MovementState)Activator.CreateInstance(stateType, ID, name, mach);

        for (int i = 0; i < count; i++)
        {
            int idx = 4 + i;
            newState.AddTransition(TransitionFactory.CreateTransition(state[idx]));
        }

        //check if further params
        if (state.Length > 4 + count)
        {
            //have further params
            string[] further = new string[state.Length - (4+count)];
            Array.Copy(state, 4 + count, further, 0, state.Length - (4 + count));
            newState.Deserialize(further);
        }

        return newState;
    }

}

public abstract class Transition
{
    public const char Seperator = ';';
    public readonly uint ToID;

    public Transition(uint toID)
    {
        ToID = toID;
    }

    public abstract bool Check(Player p);

    public virtual string Serialize()
    {    
        var s = $"{GetType().FullName}"+Seperator;

        s += $"{ToID}";
        return s;
    }

    public abstract void Deserialize(string[] furtherParams);
}

public static class TransitionFactory
{
    public static Transition CreateTransition(string tr)
    {
        string[] para = tr.Split(Transition.Seperator);
        var ass = Assembly.GetExecutingAssembly();

        var type = ass.GetType(para[0]);
        Transition t = (Transition)Activator.CreateInstance(type,uint.Parse(para[1]));
        if (para.Length > 2)
        {
            string[] further = new string[para.Length - 2];
            Array.Copy(para, 2, further, 0, para.Length - 2);
            t.Deserialize(further);
        }
        return t;
    }
}


