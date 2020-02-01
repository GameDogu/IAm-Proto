using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Transition
{
    [NonSerialized]uint stateBelongingToID;
    public EntityMovementOption Activator { get; protected set; }
    public uint NextStateID { get; protected set; }

    public Transition(EntityMovementOption activator, uint nextStateID,MovementState state)
    {
        this.stateBelongingToID = state.ID;
        this.Activator = activator ?? throw new ArgumentNullException(nameof(activator));
        NextStateID = nextStateID;
    }

    public Transition(int activatorIDX, uint nextStateID, MovementState state)
    {
        this.stateBelongingToID = state.ID;
        this.Activator = state.StateMachine.GetMovementOption(activatorIDX);
        NextStateID = nextStateID;
    }

    public override bool Equals(object obj)
    {
        if (obj is Transition)
        {
            var tra = obj as Transition;

            if (tra.NextStateID == NextStateID && tra.Activator == Activator)
                return true;

        }
        return false;
    }

    public override int GetHashCode()
    {
        var hashCode = 62885100;
        hashCode = hashCode * -1521134295 + EqualityComparer<EntityMovementOption>.Default.GetHashCode(Activator);
        hashCode = hashCode * -1521134295 + NextStateID.GetHashCode();
        return hashCode;
    }
}

