using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Transition
{
    public uint StateBelongingToID { get; protected set; }
    public TransitionRequest Type { get; set; }
    public uint NextStateID { get; protected set; }

    public Transition(uint nextState, MovementState state)
    {
        StateBelongingToID = state.ID;
        Type = TransitionRequest.Factory.BuildRequest(typeof(TransitionRequestNone));
        NextStateID = nextState;
    }

    public Transition(TransitionRequest request, uint nextState, MovementState state)
    {
        if (state != null)
        {
            StateBelongingToID = state.ID;
        }
        this.Type = request;
        NextStateID = nextState;
    }

    public Transition(Type requestType, uint nextState, MovementState state):this(TransitionRequest.Factory.BuildRequest(requestType),nextState,state)
    {}

    public Transition(TransitionRequest requestType, uint nextState, uint stateID)
    {
        this.StateBelongingToID = stateID;
        this.Type = requestType;
        NextStateID = nextState;
    }

    public Transition(Type requestType, uint nextState, uint stateID):this(TransitionRequest.Factory.BuildRequest(requestType),nextState,stateID)
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

        public Transition Create()
        {
            return new Transition(System.Type.GetType(requestType), nextState, stateID);
        }
    }

}

