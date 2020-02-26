using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Transition between movment states
/// </summary>
public class Transition
{
    /// <summary>
    /// The movement state this transition belongs to
    /// </summary>
    MovementState stateBelongingTo;
    /// <summary>
    /// ID of state this transition belings to
    /// </summary>
    public uint StateBelongingToID => stateBelongingTo.ID;
    /// <summary>
    /// the type of transition request this transition represents
    /// </summary>
    public TransitionRequest Type { get; set; }
    /// <summary>
    /// ID of the next state
    /// </summary>
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

    /// <summary>
    /// checks if this transition rquest is represented by this transition
    /// </summary>
    /// <param name="request">the type of request made</param>
    /// <returns>true if Type is same request as Reqeust</returns>
    public bool CheckRequest(TransitionRequest request)
    {
        return Type.IsSameRequest(request);
    }

    /// <summary>
    /// data of transition serializeable by unity
    /// </summary>
    [Serializable]
    public class Data
    {
        /// <summary>
        /// state belonging to ID
        /// </summary>
        [SerializeField] uint stateID;
        /// <summary>
        /// getter for state belonging to
        /// </summary>
        public uint StateID => stateID;
        /// <summary>
        /// next State ID
        /// </summary>
        [SerializeField] uint nextState;
        /// <summary>
        /// getter for next state ID
        /// </summary>
        public uint NextState => nextState;
        /// <summary>
        /// transition request type name
        /// </summary>
        [SerializeField] string requestType;
        /// <summary>
        /// getter for transition request type name
        /// </summary>
        public string ReqeustType => requestType;

        public Data(Transition t)
        {
            stateID = t.StateBelongingToID;
            nextState = t.NextStateID;
            requestType = t.Type.GetType().Name;
        }

        /// <summary>
        /// creates a transition for a state machine from this data
        /// </summary>
        /// <param name="machine">the machine belonging to</param>
        /// <returns>the newly created transition</returns>
        public Transition Create(MovementStateMachine machine)
        {
            return new Transition(System.Type.GetType(requestType), nextState, stateID,machine);
        }
    }

}

