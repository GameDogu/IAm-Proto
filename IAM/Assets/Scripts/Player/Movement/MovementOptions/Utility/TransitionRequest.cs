using System;

public abstract class TransitionRequest
{
    //public uint RequestorID { get; protected set; }

    //public TransitionRequest(uint id)
    //{
    //    RequestorID = id;
    //}

    public virtual bool IsSameRequest(TransitionRequest req)
    {
        //if (req.RequestorID != RequestorID)
        //    return false;
        return this.GetType() == req.GetType();
    }

    public TransitionRequestInfoAttribute GetInfo()
    {
        return Attribute.GetCustomAttribute(GetType(), typeof(TransitionRequestInfoAttribute)) as TransitionRequestInfoAttribute;
    }

    public static class Factory
    {
        public static TransitionRequest BuildRequest(string type)
        {
            var t = Type.GetType(type);
            return BuildRequest(t);
        }

        public static TransitionRequest BuildRequest(Type type)
        {
            if (!(type.IsSubclassOf(typeof(TransitionRequest))))
                throw new Exception($"No such Transition Request Exits {type}");
            return Activator.CreateInstance(type) as TransitionRequest;
        }
    }
}

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.None, "No Activation","No Cause")]
public class TransitionRequestNone : TransitionRequest
{
    public override bool IsSameRequest(TransitionRequest req)
    {
        return false;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class TransitionRequestInfoAttribute : Attribute
{
    public enum RequestType { PlayerInput, Physics, InputOrPhysics,None }
    public RequestType Type { get; protected set; }
    public string DisplayName { get; protected set; }
    public string DetailedCause { get; protected set; }

    public TransitionRequestInfoAttribute(RequestType type, string displayName,string detailedCause)
    {
        Type = type;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        DetailedCause = detailedCause;
    }
}


