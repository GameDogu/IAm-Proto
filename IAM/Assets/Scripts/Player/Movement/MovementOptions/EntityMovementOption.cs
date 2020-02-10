using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, RequireComponent(typeof(MovementStateMachine))]
public abstract class EntityMovementOption : MonoBehaviour
{
    protected MovementStateMachine EntiyMovementStateMachine;
    protected StateMovementHandler handler => EntiyMovementStateMachine.CurrentState.MovementHandler;
    public abstract string Name { get; }
    public abstract TransitionRequest TransitionRequst{ get; }

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

    protected void RequestStateChange()
    {
        handler.RequestStateChange(TransitionRequst);
    }
}
