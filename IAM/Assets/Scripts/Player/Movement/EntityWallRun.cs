using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityWallRun : FixedUpdateOnlyMovementOption
{
 
    [Header("Wall Running")]
    [SerializeField, Range(0f, 100f)] int minWallRunSpeed = 2;
    [SerializeField, Range(0f, 90f)] float maxWallRunAngle = 30f;
    [SerializeField] WallRunTimer wallRunMultiplierOverTime;

    [Header("Utility")]
    [SerializeField] GameObject indicator = null;

    public override string Name => "Wall Run";

    bool OnSteep => player.CollisionHandler.OnSteep;
    bool OnGround => player.CollisionHandler.OnGround;

    float maxWallRunDotProd;

    float wallRunTimer = 0f;
    float minTimerValue, maxTimerValue;

    bool stateChangeInvoke = true;

    Vector3 velocity => handler.Velocity;

    Vector3 steepNormal => player.CollisionHandler.SteepNormal;

    Vector3 playerDirection => handler.PlayerDirection;

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

    protected override void Initialize(StateMovementHandler handler)
    {
        base.Initialize(handler);
        Validate();
    }

    protected override void FixedUpdateProcedure()
    {
        if (OnSteep && !OnGround && !handler.IsGrabbing)
        {
            //check if wall run
            if (stateChangeInvoke)
            {
                stateChangeInvoke = false;
                RequestStateChange();
            }
            EvaluateWallRun();
        }
        else
        {
            wallRunTimer = 0f;
            indicator.SetActive(false);
            stateChangeInvoke = true;
        }
    }

    private void EvaluateWallRun()
    {
        if (velocity.XZ().sqrMagnitude < minWallRunSpeed * minWallRunSpeed)
            return;

        float dotValue = Vector3.Dot(velocity.XZ().normalized, steepNormal.XZ().normalized);
        if (dotValue <= 0 && dotValue >= maxWallRunDotProd)
        {
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
