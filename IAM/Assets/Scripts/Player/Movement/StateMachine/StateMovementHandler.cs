using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

/// <summary>
/// Not abstract class cause unity serialization shits the bed in that case
/// </summary>

[Serializable]
public class StateMovementHandler
{
    public event Action OnUpdate;
    public event Action OnFixedUpdate;

    public StateMovementHandler(MovementState belongingTo,Player player)
    {
        stateBelongingToo = belongingTo;
        this.player = player;
    }

    [SerializeField] Player player = null;
    public Player Player => player;

    [NonSerialized]MovementState stateBelongingToo;

    Rigidbody body => player.Body;

    public Vector3 Velocity { get; protected set; }
    
    public Vector3 PlayerDirection => Velocity.normalized;

    public float Speed => Velocity.magnitude;

    EntityWallGrab grabHandler = null;
    public bool IsGrabbing => grabHandler != null ? grabHandler.IsGrabbing : false;

    EntityJump jumpHandler = null;

    public void OnValidate()
    {
        if (stateBelongingToo.MovementOptions != null)
        {
            grabHandler = Find<EntityWallGrab>();
            jumpHandler = Find<EntityJump>();
        }

    }

    T Find<T>()where T : EntityMovementOption
    {
        for (int i = 0; i < stateBelongingToo.MovementOptions.Count; i++)
        {
            if (stateBelongingToo.MovementOptions[i] != null && stateBelongingToo.MovementOptions[i] is T)
                return stateBelongingToo.MovementOptions[i] as T;
        }
        return null;
    }

    public void Initialize()
    {
        for (int i = 0; i < stateBelongingToo.MovementOptions.Count; i++)
        {
            stateBelongingToo.MovementOptions[i].Init(this);
        }
    }

    public void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        Velocity = body.velocity;
        OnUpdate?.Invoke();
    }

    public void AddVelocity(Vector3 vel)
    {
        Velocity += vel;
    }

    public void FixedUpdate()
    {
        Velocity = body.velocity;

        OnFixedUpdate?.Invoke();

        body.velocity = Velocity;
    }

    public bool CheckPlayerActionPreventsGroundSnapping()
    {
        if (jumpHandler != null)
            return jumpHandler.RecentlyJumped;
        return false;
    }

    public void RequestStateChange(EntityMovementOption requestor)
    {
        stateBelongingToo.RequestStateChange(requestor);
    }
}
