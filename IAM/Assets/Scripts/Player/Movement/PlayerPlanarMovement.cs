using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlanarMovement : PlayerMovement
{
    [Header("Speed and acceleration")]
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxAccelleration = 10f, maxAirAcceleration = 1f;

    Rigidbody body => player.Body;
    Vector3 desiredVelocity;
    Vector3 velocity => player.MovementHandler.Velocity;

    Vector3 contactNormal => player.CollisionHandler.ContactNormal;

    bool OnGround => player.CollisionHandler.OnGround;

    protected override void Initialize()
    {
        RegisterUpdateCall(UpdateProcedure);
        RegisterFixedUpdateCall(FixedUpdateProcedure);
    }

    private void UpdateProcedure()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
    }

    private void FixedUpdateProcedure()
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

        player.MovementHandler.AddVelocity( xAxis * (newX - currentX) + zAxis * (newZ - currentZ));
    }
}
