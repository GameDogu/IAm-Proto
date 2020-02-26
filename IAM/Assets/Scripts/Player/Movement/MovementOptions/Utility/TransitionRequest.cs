using System;

/// <summary>
/// Base class for a transition request
/// </summary>
public abstract class TransitionRequest
{


    /// <summary>
    /// chekc if they are the same type
    /// </summary>
    /// <param name="req">the request to check against</param>
    /// <returns>ture if same type</returns>
    public virtual bool IsSameRequest(TransitionRequest req)
    {
        return this.GetType() == req.GetType();
    }

    /// <summary>
    /// gets the custom info attribute the object might have
    /// </summary>
    /// <returns>the custom TransitionRequestInfoAttribute the class might have, or null if not</returns>
    public TransitionRequestInfoAttribute GetInfo()
    {
        return Attribute.GetCustomAttribute(GetType(), typeof(TransitionRequestInfoAttribute)) as TransitionRequestInfoAttribute;
    }


    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is TransitionRequest)
        {
            return IsSameRequest(obj as TransitionRequest);
        }
        return false;
    }

    /// <summary>
    /// factory to create transition requests
    /// </summary>
    public static class Factory
    {
        /// <summary>
        /// builds request type from a string type name
        /// </summary>
        /// <param name="type">the type name as string</param>
        /// <returns></returns>
        public static TransitionRequest BuildRequest(string type)
        {
            var t = Type.GetType(type);
            return BuildRequest(t);
        }

        /// <summary>
        /// builds request from a type, makes sure the type requested is actually a subclass of TransitionRequest
        /// </summary>
        /// <param name="type">the transition request type</param>
        /// <returns>an instance of a transition request of this type</returns>
        public static TransitionRequest BuildRequest(Type type)
        {
            if (!(type.IsSubclassOf(typeof(TransitionRequest))))
                throw new Exception($"No such Transition Request Exits {type}");
            return Activator.CreateInstance(type) as TransitionRequest;
        }
    }
}

/// <summary>
/// transition request that can't be caused and will never be the same as any other request
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.None, "No Activation","No Cause")]
public class TransitionRequestNone : TransitionRequest
{
    public override bool IsSameRequest(TransitionRequest req)
    {
        return false;
    }
}

/// <summary>
/// attribute for transition requests
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TransitionRequestInfoAttribute : Attribute
{
    /// <summary>
    /// types of request causes
    /// </summary>
    public enum RequestType { PlayerInput, Physics, InputOrPhysics,None }
    /// <summary>
    /// what causes this request
    /// </summary>
    public RequestType Type { get; protected set; }
    /// <summary>
    /// the display name of this request
    /// </summary>
    public string DisplayName { get; protected set; }
    /// <summary>
    /// the cause of the request described in detail
    /// </summary>
    public string DetailedCause { get; protected set; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="type">t</param>
    /// <param name="displayName"></param>
    /// <param name="detailedCause"></param>
    public TransitionRequestInfoAttribute(RequestType type, string displayName,string detailedCause)
    {
        Type = type;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        DetailedCause = detailedCause;
    }
}


