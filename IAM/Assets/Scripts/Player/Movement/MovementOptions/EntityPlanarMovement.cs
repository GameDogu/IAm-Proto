using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that allows an entity to move on the plane
/// </summary>
[System.Serializable,PossibleTransitionRequestTypes(typeof(PlanarMovementTransitionRequest))]
public class EntityPlanarMovement : DualLoopMovementOption
{
    /// <summary>
    /// the player input space tranform
    /// </summary>
    [SerializeField] Transform playerInputSpace= default;
    /// <summary>
    /// maximum speed of the entity
    /// </summary>
    [Header("Speed and acceleration")]
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    /// <summary>
    /// maximum acceleration of the entity
    /// </summary>
    [SerializeField, Range(0f, 100f)] float maxAccelleration = 10f;
    /// <summary>
    /// maximum acceleration of player in air
    /// 0 if no air movement allowed
    /// </summary>
    [SerializeField, Range(0f, 100f)] float maxAirAcceleration = 1f;

    /// <summary>
    /// rigid body of the palyer
    /// </summary>
    Rigidbody body => player.Body;
    /// <summary>
    /// the velocity the entity desires (based on input)
    /// </summary>
    Vector3 desiredVelocity;
    /// <summary>
    /// the current velocity of the entity
    /// </summary>
    Vector3 velocity => handler.Velocity;

    /// <summary>
    /// contact normal of the entity
    /// </summary>
    Vector3 contactNormal => player.CollisionHandler.ContactNormal;

    /// <summary>
    /// is entity on ground
    /// </summary>
    bool OnGround => player.CollisionHandler.OnGround;

    /// <summary>
    /// display name of component
    /// </summary>
    public override string Name => "Planar Movement";

    /// <summary>
    /// state change request dispatched by component
    /// </summary>
    PlanarMovementTransitionRequest movementRequest = new PlanarMovementTransitionRequest();
   
    /// <summary>
    /// update proceudre of entity
    /// gets input, projects it onto input space
    /// and sets desired velocity
    /// if velocity sqr mag greater than 0 dispatch state change request
    /// </summary>
    protected override void UpdateProcedure()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        //maybe invoke state change 
        if (playerInputSpace)
        {
            Vector3 forward = playerInputSpace.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = playerInputSpace.right;
            right.y = 0;
            right.Normalize();

            desiredVelocity = (forward * playerInput.y + right * playerInput.x) * maxSpeed;
        }
        else
        {
            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        }

        if (desiredVelocity.sqrMagnitude > 0)
        {
            RequestStateChange(movementRequest);
        }
    }

    /// <summary>
    /// adjusts velocity of entity
    /// </summary>
    protected override void FixedUpdateProcedure()
    {
        AdjustVelocity();
    }

    /// <summary>
    /// projects a vecto onto the contact plain
    /// </summary>
    /// <param name="vec">the vector that will be projected</param>
    /// <returns>the vector projected onto the contact plain</returns>
    Vector3 ProjectOnContactPlain(Vector3 vec)
    {
        return vec - contactNormal * Vector3.Dot(vec, contactNormal);
    }

    /// <summary>
    /// adjust velocity of entity by
    /// calculating acceleration and speed change
    /// and applying it correctly projected on the contact plain
    /// to the entitys rigidbody
    /// </summary>
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

/// <summary>
/// state change request dispatched by planr movement component
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.PlayerInput,"On Movement Input","Input vector squared length (of horizontal and vertical axis) is greater than 0")]
public class PlanarMovementTransitionRequest : TransitionRequest { }
