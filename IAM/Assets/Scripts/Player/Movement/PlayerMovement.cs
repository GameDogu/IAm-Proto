using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    //public enum MovementActions
    //{
    //    Walk,
    //    Run,
    //    Jump,
    //    AirJump,
    //    WallJump,
    //    WallRun,
    //    WallGrab
    //}

    [Header("Speed and acceleration")]
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxAccelleration = 10f,maxAirAcceleration = 1f;
    [Header("Jump")]
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0, 10)] int jumpsInAir = 2;
    [SerializeField, Range(0f, 10f)] float wallJumpDirectionChangeBonus = 2f;

    [Header("Wall Running")]
    [SerializeField, Range(0f, 100f)] int minWallRunSpeed = 2;
    [SerializeField, Range(0f, 90f)] float maxWallRunAngle = 30f;
    [SerializeField] WallRunTimer wallRunMultiplierOverTime;
 
    [Header("PhysicsMaterial")]
    [SerializeField] PhysicMaterial defaultMovmentMaterial = null;
    [SerializeField] PhysicMaterial wallGrabMaterial = null;

    [Header("Keys")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode grabKey = KeyCode.F;

    [Header("Utility")]
    [SerializeField] Player player = null;
    [SerializeField] GameObject indicator = null;
    Rigidbody body => player.Body;
    Collider playerCollider => player.Collider;

    Vector3 desiredVelocity, velocity;
    Vector3 Velocity => velocity;
    
    Vector3 playerDirection => velocity.normalized;

    bool desiredJump;
    int jumpPhase;

    Vector3 contactNormal => player.PlayerCollisionHandler.ContactNormal;
    Vector3  steepNormal => player.PlayerCollisionHandler.SteepNormal;

    public float Speed => velocity.magnitude;

    bool OnGround => player.PlayerCollisionHandler.OnGround;
    bool OnSteep => player.PlayerCollisionHandler.OnSteep;

    float maxWallRunDotProd;  

    float wallRunTimer = 0f;
    float minTimerValue, maxTimerValue;

    private void OnValidate()
    {
        
        maxWallRunDotProd = Mathf.Cos((90f+maxWallRunAngle) * Mathf.Deg2Rad);
        playerCollider.material = defaultMovmentMaterial;

        if (wallRunMultiplierOverTime == null)
        {
            wallRunMultiplierOverTime = new WallRunTimer(1f, .95f);
        }

        wallRunMultiplierOverTime.OnValidate();

        minTimerValue = wallRunMultiplierOverTime.MinTimerValue;
        maxTimerValue = wallRunMultiplierOverTime.MaxTimerValue;
    }

    private void Awake()
    {
        OnValidate(); 
    }

    // Update is called once per frame
    void Update()
    {

        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        velocity = body.velocity;
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y)*maxSpeed;

        desiredJump |= Input.GetKeyDown(jumpKey);

        if (Input.GetKey(grabKey) && OnSteep)
        {
            playerCollider.material = wallGrabMaterial;
        }
        else
        {
            playerCollider.material = defaultMovmentMaterial;
        }

        TempFunctionality();

    }

    private void TempFunctionality()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            var trail = GetComponent<TrailRenderer>();
            trail.Clear();
        }
    }

    public void ResetJumpPhase()
    {
        jumpPhase = 0;
    }

    private void FixedUpdate()
    {
        velocity = body.velocity;
        player.PlayerCollisionHandler.UpdateState();

        AdjustVelocity();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        if (OnSteep && !OnGround && !Input.GetKey(grabKey))
        {
            //check if wall run
            EvaluateWallRun();
        }
        else
        {
            wallRunTimer = 0f;
            indicator.SetActive(false);
        }

        body.velocity = velocity;

        player.PlayerCollisionHandler.ClearState();
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
                velocity += -Physics.gravity * Time.deltaTime * wallRunMultiplierOverTime.Evaluate(wallRunTimer);
                
            }
        }
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

        player.PlayerCollisionHandler.ResetStepsSinceLastJump(); 
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * DetermineJumpHeight(jumpPhase));
        //allow to gain height from wall jump eg
        //jump up bias added higher bigger when steeper contact
        // 0 when on ground as already jumping up and (0,1,0) when contact orthogonal to (0,1,0)
        jumpDirection = (jumpDirection + (Vector3.up*(1f-Vector3.Dot(jumpDirection.normalized, Vector3.up)))).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed,0f);
        }

        velocity += jumpDirection * jumpSpeed + bonus;

    }

    private Vector3 HandleWallJump(Vector3 jumpDirection)
    {
        return jumpDirection.normalized * wallJumpDirectionChangeBonus;
    }

    private float DetermineJumpHeight(int jumpPhase)
    {
        return jumpHeight*(float)jumpPhase;
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

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    public void AlignVelocityWithContactNormal(Vector3 normal)
    {
        float dot = Vector3.Dot(velocity, normal);
        if (dot > 0f)
        {
            velocity = (velocity - normal * dot).normalized * Speed;
        }
    }

}
