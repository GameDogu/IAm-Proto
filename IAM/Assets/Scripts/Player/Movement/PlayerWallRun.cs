using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRun : PlayerMovement
{
 
    [Header("Wall Running")]
    [SerializeField, Range(0f, 100f)] int minWallRunSpeed = 2;
    [SerializeField, Range(0f, 90f)] float maxWallRunAngle = 30f;
    [SerializeField] WallRunTimer wallRunMultiplierOverTime;

    [Header("Utility")]
    [SerializeField] GameObject indicator = null;

    bool OnSteep => player.CollisionHandler.OnSteep;
    bool OnGround => player.CollisionHandler.OnGround;

    float maxWallRunDotProd;

    float wallRunTimer = 0f;
    float minTimerValue, maxTimerValue;

    Vector3 velocity => player.MovementHandler.Velocity;

    Vector3 steepNormal => player.CollisionHandler.SteepNormal;

    Vector3 playerDirection => player.MovementHandler.PlayerDirection;

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

    protected override void Initialize()
    {
        Validate();
        RegisterFixedUpdateCall(FixedUpdateProcedure);
    }

    public override void Stop()
    {
        UnregisterFixedUpdateCall(FixedUpdateProcedure);
    }

    private void FixedUpdateProcedure()
    {
        if (OnSteep && !OnGround && !player.MovementHandler.IsGrabbing)
        {
            //check if wall run
            EvaluateWallRun();
        }
        else
        {
            wallRunTimer = 0f;
            indicator.SetActive(false);
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
                player.MovementHandler.AddVelocity( -Physics.gravity * Time.deltaTime * wallRunMultiplierOverTime.Evaluate(wallRunTimer));
            }
        }
    }
}
