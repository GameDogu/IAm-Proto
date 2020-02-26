using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class of an entity movement option
/// </summary>
[Serializable, RequireComponent(typeof(MovementStateMachine))]
public abstract class EntityMovementOption : MonoBehaviour
{
    /// <summary>
    /// the state machine the option belongs to
    /// </summary>
    protected MovementStateMachine EntiyMovementStateMachine;
    /// <summary>
    /// the movement state handler of the current state
    /// </summary>
    protected StateMovementHandler handler => EntiyMovementStateMachine.CurrentState.MovementHandler;
    /// <summary>
    /// the display name of the movement option
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// the player the option belongs to
    /// </summary>
    [SerializeField] protected Player player => EntiyMovementStateMachine.Player;

    /// <summary>
    /// unity editor on validate, calls virtual function validate
    /// ensures StateMachine member is set
    /// </summary>
    public void OnValidate()
    {
        EntiyMovementStateMachine = GetComponent<MovementStateMachine>();
        Validate();
    }
    
    /// <summary>
    /// component validation
    /// </summary>
    protected virtual void Validate() { }

    /// <summary>
    /// Initializes movemnet optio for a states movement handler
    /// calls Initialize implemented by classes inheriting from base
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    public void Init(StateMovementHandler handler)
    {
        EntiyMovementStateMachine = GetComponent<MovementStateMachine>();
        Initialize(handler);
    }

    /// <summary>
    /// Reset function of unity editor(when added to game object)
    /// Calls OnValidate, and adds this option to the state machine of the gameobject
    /// </summary>
    [ContextMenu("Add to statemachine")]
    private void Reset()
    {
        OnValidate();
        EntiyMovementStateMachine.AddGeneralMovementOption(this);
    }

    /// <summary>
    /// Initialize the component
    /// call base.Initialize() if you don't register
    /// update and fixed update call on your own
    /// </summary>
    protected abstract void Initialize(StateMovementHandler handler);

    /// <summary>
    /// Registers an action to the handlers update event, without double registering
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    /// <param name="updateAction">the action that will be registerd</param>
    protected void RegisterUpdateCall(StateMovementHandler handler,Action updateAction)
    {
        handler.OnUpdate -= updateAction;
        handler.OnUpdate += updateAction;
    }

    /// <summary>
    /// Unregisters an action to the handlers update event
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    /// <param name="updateAction">the action that will be registerd</param>
    protected void UnregisterUpdateCall(StateMovementHandler handler, Action updateAction)
    {
        handler.OnUpdate -= updateAction;
    }

    /// <summary>
    /// Registers an action to the handlers fixed update event, without double registering
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    /// <param name="fixedUpdateAction">the action that will be registerd</param>
    protected void RegisterFixedUpdateCall(StateMovementHandler handler, Action fixedUpdateAction)
    {
        handler.OnFixedUpdate -= fixedUpdateAction;
        handler.OnFixedUpdate += fixedUpdateAction;
    }

    /// <summary>
    /// Unregisters an action to the handlers update event
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    /// <param name="fixedUpdateAction">the action that will be registerd</param>
    protected void UnregisterFixedUpdateCall(StateMovementHandler handler, Action fixedUpdateAction)
    {
        handler.OnFixedUpdate -= fixedUpdateAction;
    }

    /// <summary>
    /// informs handler of state change request
    /// </summary>
    /// <param name="request">the request that is being made</param>
    protected void RequestStateChange(TransitionRequest request)
    {
        handler.RequestStateChange(request);
    }
}

/// <summary>
/// attribute of containing all the possible types of transition requests
/// a movement option can make
/// </summary>
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