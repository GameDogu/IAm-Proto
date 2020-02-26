using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that allows the entity to wall run
/// </summary>
[System.Serializable,PossibleTransitionRequestTypes(typeof(WallRunTransitionRequest))]
public class EntityWallRun : FixedUpdateOnlyMovementOption
{
 
    /// <summary>
    /// the minimum speed the entity needs to start wallrunning
    /// </summary>
    [Header("Wall Running")]
    [SerializeField, Range(0f, 100f)] int minWallRunSpeed = 2;

    /// <summary>
    /// the maximum angle the entity can have compared to the wall normal
    /// to even start wall running
    /// </summary>
    [SerializeField, Range(0f, 90f)] float maxWallRunAngle = 30f;

    /// <summary>
    /// multiplier over time to the strenght of the wall run
    /// generally decreases over time making the entity lose height despite
    /// "wall running"
    /// </summary>
    [SerializeField] WallRunTimer wallRunMultiplierOverTime;

    /// <summary>
    /// TEMP indicator activated if wallrunning
    /// </summary>
    [Header("Utility")]
    [SerializeField] GameObject indicator = null;

    /// <summary>
    /// Name of the movment option
    /// </summary>
    public override string Name => "Wall Run";

    /// <summary>
    /// is the player in contact with a steep collider
    /// </summary>
    bool OnSteep => player.CollisionHandler.OnSteep;
    /// <summary>
    /// is the player in contact with the ground
    /// </summary>
    bool OnGround => player.CollisionHandler.OnGround;

    /// <summary>
    /// value the contact normal and player XZ velocity can't be bigger than
    /// </summary>
    float maxWallRunDotProd;

    /// <summary>
    /// timer for duration of current wall run
    /// </summary>
    float wallRunTimer = 0f;

    /// <summary>
    /// min and max timer value based on multiplier over time member
    /// </summary>
    float minTimerValue, maxTimerValue;

    /// <summary>
    /// controlls if this update loop a state change request should be made
    /// happens of the first fixed update cycle when the entity is wall running
    /// </summary>
    bool stateChangeInvoke = true;

    /// <summary>
    /// velocity of the entity
    /// </summary>
    Vector3 velocity => handler.Velocity;
    
    /// <summary>
    /// contact normal of a steep contact (not ground)
    /// </summary>
    Vector3 steepNormal => player.CollisionHandler.SteepNormal;

    /// <summary>
    /// direction of player heading (normalized velocity)
    /// </summary>
    Vector3 playerDirection => handler.PlayerDirection;

    /// <summary>
    /// the state change request made by this movement option
    /// </summary>
    WallRunTransitionRequest wallRunRequest = new WallRunTransitionRequest();

    /// <summary>
    /// validates/recaulates all the members of the class 
    /// </summary>
    protected override void Validate()
    {
        maxWallRunDotProd = Mathf.Cos((90f + maxWallRunAngle) * Mathf.Deg2Rad);

        if (wallRunMultiplierOverTime == null)
        {
            wallRunMultiplierOverTime = new WallRunTimer(1f, .95f);
        }

        wallRunMultiplierOverTime.OnValidate();

        minTimerValue = wallRunMultiplierOverTime.MinTimerValue;
        maxTimerValue = wallRunMultiplierOverTime.MaxTimerValue;
    }

    /// <summary>
    /// Validates and calls base initialize
    /// </summary>
    /// <param name="handler">The handler responsible in this state of the state machine</param>
    protected override void Initialize(StateMovementHandler handler)
    {
        base.Initialize(handler);
        Validate();
    }

    /// <summary>
    /// the fixed update procedure
    /// checks if most general constraints are met for wall running
    /// (not on groundm not grabbing wall, on wall)
    /// or resets
    /// </summary>
    protected override void FixedUpdateProcedure()
    {
        if (OnSteep && !OnGround && !handler.IsGrabbing)
        {
            EvaluateWallRun();
        }
        else
        {
            wallRunTimer = 0f;
            indicator.SetActive(false);
            stateChangeInvoke = true;
        }
    }

    /// <summary>
    /// evaluates if it possible to wall run wiht current angle and speed
    /// and applys changes to entity velocity if that is the case
    /// Further updates the wallrun timer and request state change if first time
    /// the wall run was indeed doable
    /// </summary>
    private void EvaluateWallRun()
    {
        if (velocity.XZ().sqrMagnitude < minWallRunSpeed * minWallRunSpeed)
            return;

        float dotValue = Vector3.Dot(velocity.XZ().normalized, steepNormal.XZ().normalized);

        if (dotValue <= 0 && dotValue >= maxWallRunDotProd)
        {
            
            if (stateChangeInvoke)
            {
                stateChangeInvoke = false;
                RequestStateChange(wallRunRequest);
            }
            //wallrunning
            indicator.SetActive(true);
            wallRunTimer += Time.deltaTime;
            wallRunTimer = Mathf.Clamp(wallRunTimer, minTimerValue, maxTimerValue);
            //if velocity is pointing similar direction as gravity add some opposite force
            if (Vector3.Dot(playerDirection, Physics.gravity.normalized) > 0)
            {
                handler.AddVelocity(-Physics.gravity * Time.deltaTime * wallRunMultiplierOverTime.Evaluate(wallRunTimer));
            }
        }
    }
}

/// <summary>
/// request made by EntityWallRun
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics,"On Wall Run","On Wall, not on ground, and not grabbing wall as well as above a certain speed and movement direction not to steep (or shallow) to the wall")]
public class WallRunTransitionRequest : TransitionRequest{ }
