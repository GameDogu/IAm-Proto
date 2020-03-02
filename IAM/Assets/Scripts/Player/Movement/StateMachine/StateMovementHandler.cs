using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

/// <summary>
/// Handles all the movment options for a state, provides update and fixed update event for them
/// </summary>
[Serializable]
public class StateMovementHandler
{
    /// <summary>
    /// event potentially invoked(if someone is registered) every update cycle of unity
    /// </summary>
    public event Action OnUpdate;
    /// <summary>
    /// event potentially invoked(if someone is registered) every fixed update cycle of unity
    /// </summary>
    public event Action OnFixedUpdate;

    public StateMovementHandler(MovementState belongingTo,Player player)
    {
        stateBelongingToo = belongingTo;
        this.player = player;
    }

    /// <summary>
    /// the player the state machine belongs to
    /// </summary>
    [SerializeField] Player player = null;
    /// <summary>
    /// getter for player relic of old design
    /// </summary>
    public Player Player => player;

    /// <summary>
    /// the movement state this handler belongs to
    /// </summary>
    [NonSerialized]MovementState stateBelongingToo;

    /// <summary>
    /// rigidbody of the player
    /// </summary>
    Rigidbody body => player.Body;

    /// <summary>
    /// velocity accumulated by every movement option this fixed update
    /// </summary>
    public Vector3 Velocity { get; protected set; }
    
    /// <summary>
    /// direction of player
    /// </summary>
    public Vector3 PlayerDirection => Velocity.normalized;

    /// <summary>
    /// speed of player
    /// </summary>
    public float Speed => Velocity.magnitude;

    /// <summary>
    /// wall grab option if state has one
    /// </summary>
    EntityWallGrab grabHandler = null;
    /// <summary>
    /// check if entity is grabbing
    /// </summary>
    public bool IsGrabbing => grabHandler != null ? grabHandler.IsGrabbing : false;

    /// <summary>
    /// jump handler of entity
    /// </summary>
    EntityJump jumpHandler = null;

    /// <summary>
    /// finds grab handler and jump hanlder of state belonging to if existing
    /// </summary>
    public void OnValidate()
    {
        if (stateBelongingToo.MovementOptions != null)
        {
            grabHandler = Find<EntityWallGrab>();
            jumpHandler = Find<EntityJump>();
        }

    }

    /// <summary>
    /// find a certain type of movement option for this state
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Find<T>()where T : EntityMovementOption
    {
        for (int i = 0; i < stateBelongingToo.MovementOptions.Count; i++)
        {
            if (stateBelongingToo.MovementOptions[i] != null && stateBelongingToo.MovementOptions[i] is T)
                return stateBelongingToo.MovementOptions[i] as T;
        }
        return null;
    }

    /// <summary>
    /// initializes all the movement options of the state it belongs to to this handler
    /// </summary>
    public void Initialize()
    {
        for (int i = 0; i < stateBelongingToo.MovementOptions.Count; i++)
        {
            stateBelongingToo.MovementOptions[i].Init(this);
        }
    }

    /// <summary>
    /// makes sure handler is validated
    /// </summary>
    public void Start()
    {
        OnValidate();
    }

    /// <summary>
    /// updte called by movment state
    /// </summary>
    public void Update()
    {
        Velocity = body.velocity;
        OnUpdate?.Invoke();
    }

    /// <summary>
    /// adds a velocity vector to current velocity accumulating all options change this update cycle
    /// </summary>
    /// <param name="vel"></param>
    public void AddVelocity(Vector3 vel)
    {
        Velocity += vel;
    }

    /// <summary>
    /// fixed update cycle called by the state
    /// stores velocity of body into Velocity at start
    /// and sets body velocity at end to current Velocity
    /// </summary>
    public void FixedUpdate()
    {
        Velocity = body.velocity;

        OnFixedUpdate?.Invoke();

        body.velocity = Velocity;
    }

    /// <summary>
    /// checks if any player action might prevent ground snapping of the collision handler
    /// </summary>
    /// <returns>true if jumphandler recently jumped, false otherwise, or if no jump handler exists</returns>
    public bool CheckPlayerActionPreventsGroundSnapping()
    {
        if (jumpHandler != null)
            return jumpHandler.RecentlyJumped;
        return false;
    }

    /// <summary>
    /// requests state change from movement state
    /// </summary>
    /// <param name="request">the request being made</param>
    public void RequestStateChange(TransitionRequest request)
    {
        stateBelongingToo.RequestStateChange(request);
    }
}
