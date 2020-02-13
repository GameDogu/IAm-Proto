using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable, RequireComponent(typeof(MovementStateMachine))]
public abstract class EntityMovementOption : MonoBehaviour
{
    protected MovementStateMachine EntiyMovementStateMachine;
    protected StateMovementHandler handler => EntiyMovementStateMachine.CurrentState.MovementHandler;
    public abstract string Name { get; }

    [SerializeField] protected Player player => EntiyMovementStateMachine.Player;

    public void OnValidate()
    {
        EntiyMovementStateMachine = GetComponent<MovementStateMachine>();
        Validate();
    }
    
    protected virtual void Validate() { }

    public void Init(StateMovementHandler handler)
    {
        EntiyMovementStateMachine = GetComponent<MovementStateMachine>();
        Initialize(handler);
    }

    [ContextMenu("Add to statemachine")]
    private void Reset()
    {
        OnValidate();
        EntiyMovementStateMachine.AddGeneralMovementOption(this);
    }

    /// <summary>
    /// call base.Initialize() if you don't register
    /// update and fixed update call on your own
    /// </summary>
    protected abstract void Initialize(StateMovementHandler handler);

    protected void RegisterUpdateCall(StateMovementHandler handler,Action updateAction)
    {
        handler.OnUpdate -= updateAction;
        handler.OnUpdate += updateAction;
    }

    protected void UnregisterUpdateCall(StateMovementHandler handler, Action updateAction)
    {
        handler.OnUpdate -= updateAction;
    }

    protected void RegisterFixedUpdateCall(StateMovementHandler handler, Action updateAction)
    {
        handler.OnFixedUpdate -= updateAction;
        handler.OnFixedUpdate += updateAction;
    }

    protected void UnregisterFixedUpdateCall(StateMovementHandler handler, Action updateAction)
    {
        handler.OnFixedUpdate -= updateAction;
    }

    protected void RequestStateChange(TransitionRequest request)
    {
        handler.RequestStateChange(request);
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PossibleTransitionRequestTypesAttribute : Attribute
{
    public Type[] Types { get; protected set; }

    public PossibleTransitionRequestTypesAttribute(params Type[] types)
    {
        var request = types.Where(ob => ob.IsSubclassOf(typeof(TransitionRequest))).ToArray();
        Types = new Type[request.Length];
        for (int i = 0; i < request.Length; i++)
        {
            Types[i] = request[i];
        }
    }
}