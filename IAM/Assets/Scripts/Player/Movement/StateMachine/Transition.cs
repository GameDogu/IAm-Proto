using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Transition
{
    uint stateBelongingToID;
    public TransitionRequest Type { get; set; }
    public uint NextStateID { get; protected set; }

    public Transition(uint nextState, MovementState state)
    {
        stateBelongingToID = state.ID;
        Type = null;
        NextStateID = nextState;
    }

    public Transition(TransitionRequest request, uint nextState, MovementState state)
    {
        if (state != null)
        {
            stateBelongingToID = state.ID;
        }
        this.Type = request;
        NextStateID = nextState;
    }

    public Transition(Type requestType, uint nextState, MovementState state):this(TransitionRequestFactory.BuildRequest(requestType),nextState,state)
    {}

    public Transition(TransitionRequest request, uint nextState, uint stateID)
    {
        this.stateBelongingToID = stateID;
        this.Type = request;
        NextStateID = nextState;
    }

    public string Serialize()
    {
        return $"{nameof(stateBelongingToID)}:{stateBelongingToID};{nameof(Type)}:{Type.GetType()};{nameof(NextStateID)}:{NextStateID}";
    }

    public static Transition Deserialize(string data)
    {
        string[] vars = SplitMemberData(data, "Wrong serialized data.",';',3);

        var stateBelongToData = SplitMemberData(vars[0], "Wrong state belonging to serialization.");

        uint stateBelongingTo = 0;
        if (!uint.TryParse(stateBelongToData[1], out stateBelongingTo))
            throw new Exception($"Failed parsing state belonging to ID. Data can't be deserialized to transition.\n data: {data}");

        var typeData = SplitMemberData(vars[1]);
        TransitionRequest request = TransitionRequestFactory.BuildRequest(typeData[1]);

        var nextIDdata = SplitMemberData(vars[2], "Wrong next state serialization.");

        uint nextID = 0;
        if (!uint.TryParse(nextIDdata[1], out nextID))
            throw new Exception($"Failed parsing next state ID. Data can't be deserialized to transition.\n data: {data}");

        return new Transition(request, nextID, stateBelongingTo);
    }

    static string[] SplitMemberData(string data, string exceptionAddon = "", char splitChar = ':', int resExpectedLength = 2)
    {
        var res = data.Split(splitChar);

        if (res.Length < resExpectedLength)
            throw new Exception($"{exceptionAddon} Data can't be deserialize to transition.");
        return res;
    }

    public bool CheckRequest(TransitionRequest request)
    {
        return Type.IsSameRequest(request);
    }
}

