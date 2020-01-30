using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable,RequireComponent(typeof(EntityMovementHandler))]
public abstract class EntityMovementOption : MonoBehaviour
{
    public event Action<EntityMovementOption> OnStateChangeAction;
    EntityMovementHandler _handler = null;
    EntityMovementHandler handler
    {
        get
        {
            if(_handler == null)
                _handler = GetComponent<EntityMovementHandler>();
            return _handler;
        }
    }
    [SerializeField] protected Player player => handler.Player;

    public void OnValidate()
    {
        Validate();
    }

    [ContextMenu("Register To MovementHandler")]
    private void Reset()
    {
        handler.AddMovementOption(this);
    }
    
    protected virtual void Validate() { }

    public void StartUp()
    {
        Initialize();
    }

    public abstract void Stop();

    /// <summary>
    /// call base.Initialize() if you don't register
    /// update and fixed update call on your own
    /// </summary>
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

    protected void InvokeStateChangeEvent()
    {
        OnStateChangeAction?.Invoke(this);
    }
}
