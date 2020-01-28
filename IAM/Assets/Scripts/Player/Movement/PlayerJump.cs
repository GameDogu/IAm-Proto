﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : PlayerMovement
{
    [Header("Jump")]
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0, 10)] int jumpsInAir = 2;
    [SerializeField, Range(0f, 10f)] float wallJumpDirectionChangeBonus = 2f;
    [Header("Keys")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    PlayerCollisionHandler collisionHandler => player.CollisionHandler;

    bool OnGround => collisionHandler.OnGround;
    bool OnSteep => collisionHandler.OnSteep;

    Vector3 contactNormal => collisionHandler.ContactNormal;
    Vector3 steepNormal => collisionHandler.SteepNormal;

    Vector3 velocity => player.MovementHandler.Velocity;

    /// <summary>
    /// Because of the collision data delay we're still considered grounded the step after the jump was initiated
    /// </summary>
    public bool RecentlyJumped => stepsSinceLastJump <= 2;

    bool desiredJump;
    int jumpPhase, stepsSinceLastJump;

    protected override void Initialize()
    {
        RegisterUpdateCall(UpdateProcedure);
        RegisterFixedUpdateCall(FixedUpdateProcedure);

        //Collision Handler events
        RegisterCollisonHandlerStartStateUpdate();
        RegisterCollisionHandlerGroundedStateUpdate();

    }

    public override void Stop()
    {
        UnregisterUpdateCall(UpdateProcedure);
        UnregisterFixedUpdateCall(FixedUpdateProcedure);
        UnregisterCollisonHandlerUpdateStart();
        UnregisterCollisionHandlerGroundedStateUpdate();
    }

    void RegisterCollisonHandlerStartStateUpdate()
    {
        UnregisterCollisonHandlerUpdateStart();
        player.CollisionHandler.OnStateUpdateStart += OnCollisionHandlreStateUpdateStart;
    }

    void UnregisterCollisonHandlerUpdateStart()
    {
        player.CollisionHandler.OnStateUpdateStart -= OnCollisionHandlreStateUpdateStart;
    }

    void RegisterCollisionHandlerGroundedStateUpdate()
    {
        UnregisterCollisionHandlerGroundedStateUpdate();
        player.CollisionHandler.OnGroundedStateUpdate += OnCollisionHandlerUpdateGrounded;
    }

    void UnregisterCollisionHandlerGroundedStateUpdate()
    {
        player.CollisionHandler.OnGroundedStateUpdate -= OnCollisionHandlerUpdateGrounded;
    }

    private void UpdateProcedure()
    {
        desiredJump |= Input.GetKeyDown(jumpKey);
    }

    private void OnCollisionHandlreStateUpdateStart()
    {
        stepsSinceLastJump += 1;
    }

    private void OnCollisionHandlerUpdateGrounded() 
    {
        if (stepsSinceLastJump > 1)
                ResetJumpPhase();
    }

    private void FixedUpdateProcedure()
    {
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }
    }

    public void ResetJumpPhase()
    {
        jumpPhase = 0;
    }

    private void Jump()
    {
        Vector3 jumpDirection;
        Vector3 bonus = Vector3.zero;

        if (OnGround)
            jumpDirection = contactNormal;
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;//reset to alow for more jumps after wall jump
            bonus = HandleWallJump(jumpDirection);
        }
        else if (jumpsInAir > 0 && jumpPhase <= jumpsInAir)
        {
            if (jumpPhase == 0)
                jumpPhase = 1;//skip one to negate posssible extra jump after falling of surface
            jumpDirection = contactNormal;
        }
        else
            return;

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * DetermineJumpHeight(jumpPhase));
        //allow to gain height from wall jump eg
        //jump up bias added higher bigger when steeper contact
        // 0 when on ground as already jumping up and (0,1,0) when contact orthogonal to (0,1,0)
        jumpDirection = (jumpDirection + (Vector3.up * (1f - Vector3.Dot(jumpDirection.normalized, Vector3.up)))).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        player.MovementHandler.AddVelocity( jumpDirection * jumpSpeed + bonus);

    }

    private Vector3 HandleWallJump(Vector3 jumpDirection)
    {
        return jumpDirection.normalized * wallJumpDirectionChangeBonus;
    }

    private float DetermineJumpHeight(int jumpPhase)
    {
        return jumpHeight * (float)jumpPhase;
    }
}
