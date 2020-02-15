using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;


public class Transition
{
    MovementState stateBelongingTo;
    public uint StateBelongingToID => stateBelongingTo.ID;
    public TransitionRequest Type { get; set; }
    public uint NextStateID { get; protected set; }

    public Transition(uint nextState, MovementState state)
    {
        stateBelongingTo = state;
        Type = TransitionRequest.Factory.BuildRequest(typeof(TransitionRequestNone));
        NextStateID = nextState;
    }

    public Transition(TransitionRequest request, uint nextState, MovementState state)
    {
        if (state != null)
        {
            stateBelongingTo = state;
        }
        this.Type = request;
        NextStateID = nextState;
    }

    public Transition(Type requestType, uint nextState, MovementState state):this(TransitionRequest.Factory.BuildRequest(requestType),nextState,state)
    {}

    public Transition(TransitionRequest requestType, uint nextState, uint stateID,MovementStateMachine machine)
    {
        this.stateBelongingTo = machine.GetStateByID(stateID);
        this.Type = requestType;
        NextStateID = nextState;
    }

    public Transition(Type requestType, uint nextState, uint stateID,MovementStateMachine machine):this(TransitionRequest.Factory.BuildRequest(requestType),nextState,stateID,machine)
    {}

    public bool CheckRequest(TransitionRequest request)
    {
        return Type.IsSameRequest(request);
    }

    [Serializable]
    public class Data
    {
        [SerializeField] uint stateID;
        public uint StateID => stateID;
        [SerializeField] uint nextState;
        public uint NextState => nextState;
        [SerializeField] string requestType;
        public string ReqeustType => requestType;

        public Data(Transition t)
        {
            stateID = t.StateBelongingToID;
            nextState = t.NextStateID;
            requestType = t.Type.GetType().Name;
        }

        public Transition Create(MovementStateMachine machine)
        {
            return new Transition(System.Type.GetType(requestType), nextState, stateID,machine);
        }
    }

}

