using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable,RequireComponent(typeof(PlayerMovementHandler))]
public abstract class PlayerMovement : MonoBehaviour
{
    PlayerMovementHandler handler = null;
    [SerializeField] protected Player player => handler.Player;

    private void OnValidate()
    {
        handler = GetComponent<PlayerMovementHandler>();
        Validate();
    }

    protected virtual void Validate()
    {}

    public void StartUp()
    {
        handler = GetComponent<PlayerMovementHandler>();
        Initialize();
    }

    protected abstract void Initialize();

    protected void RegisterUpdateCall(Action updateAction)
    {
        handler.OnUpdate -= updateAction;
        handler.OnUpdate += updateAction;
    }

    protected void UnregisterUpdateCall(Action updateAction)
    {
        handler.OnUpdate -= updateAction;
    }

    protected void RegisterFixedUpdateCall(Action updateAction)
    {
        handler.OnFixedUpdate -= updateAction;
        handler.OnFixedUpdate += updateAction;
    }

    protected void UnregisterFixedUpdateCall(Action updateAction)
    {
        handler.OnFixedUpdate -= updateAction;
    }

    protected void AddVelocityToPlayer(Vector3 vel)
    {
        handler.AddVelocity(vel);
    }

}
