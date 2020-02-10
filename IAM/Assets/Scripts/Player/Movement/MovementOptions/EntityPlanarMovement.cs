using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityPlanarMovement : DualLoopMovementOption
{
    [Header("Speed and acceleration")]
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxAccelleration = 10f, maxAirAcceleration = 1f;

    Rigidbody body => player.Body;
    Vector3 desiredVelocity;
    Vector3 velocity => handler.Velocity;

    Vector3 contactNormal => player.CollisionHandler.ContactNormal;

    bool OnGround => player.CollisionHandler.OnGround;

    public override string Name => "Basic Movement";

    PlanarMovementTransitionRequest movementRequest = new PlanarMovementTransitionRequest();
    public override TransitionRequest TransitionRequst => movementRequest;

    protected override void UpdateProcedure()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        //maybe invoke state change 
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        if (desiredVelocity.sqrMagnitude > 0)
        {
            RequestStateChange();
        }

    }

    protected override void FixedUpdateProcedure()
    {
        AdjustVelocity();
    }

    Vector3 ProjectOnContactPlain(Vector3 vec)
    {
        return vec - contactNormal * Vector3.Dot(vec, contactNormal);
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlain(transform.right).normalized;
        Vector3 zAxis = ProjectOnContactPlain(transform.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float accel = OnGround ? maxAccelleration : maxAirAcceleration;
        float maxSpeedChange = accel * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        handler.AddVelocity( xAxis * (newX - currentX) + zAxis * (newZ - currentZ));
    }
}

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.PlayerInput,"On Movement Input")]
public class PlanarMovementTransitionRequest : TransitionRequest { }
