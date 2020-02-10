using System;

public abstract class TransitionRequest
{
    //public uint RequestorID { get; protected set; }

    //public TransitionRequest(uint id)
    //{
    //    RequestorID = id;
    //}

    public  bool IsSameRequest(TransitionRequest req)
    {
        //if (req.RequestorID != RequestorID)
        //    return false;
        return this.GetType() == req.GetType();
    }

}

[AttributeUsage(AttributeTargets.Class)]
public class TransitionRequestInfoAttribute : Attribute
{
    public enum RequestType { PlayerInput, Physics }
    public RequestType Type { get; protected set; }
    public string DisplayName { get; protected set; }

    public TransitionRequestInfoAttribute(RequestType type, string displayName)
    {
        Type = type;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    }
}

public static class TransitionRequestFactory
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

