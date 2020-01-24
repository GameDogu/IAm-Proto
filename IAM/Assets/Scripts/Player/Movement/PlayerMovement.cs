using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public enum MovementActions
    {
        Walk,
        Run,
        Jump,
        AirJump,
        WallJump,
        WallRun,
        WallGrab
    }

    [Header("Speed and acceleration")]
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxAccelleration = 10f,maxAirAcceleration = 1f;
    [Header("Jump")]
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0, 10)] int jumpsInAir = 2;
    [SerializeField, Range(0f, 10f)] float wallJumpDirectionChangeBonus = 2f;
    [Header("Walkability of Ground")]
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    [SerializeField, Range(0f, 90f)] float maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)] float maxSnapToGroundSpeed = 100f;
    [SerializeField, Range(0f, 1f)] float snapToGoundProbeDist = 1f;
    [SerializeField] LayerMask probeMask = -1,stairsMask = -1;

    [Header("Keys")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode grabKey = KeyCode.F;

    [Header("Utility")]
    [SerializeField]Rigidbody body = null;
    [SerializeField] MeshRenderer meshRenderer= null;
    [SerializeField] Player player = null;
    Color c;

    Vector3 desiredVelocity,velocity;
    Vector3 contactNormal,steepNormal;

    bool desiredJump;
    int jumpPhase;

    bool desiredGrab;
    bool wasDesiredGrab;

    int groundContatctCount,steepContactCount;
    bool OnGround => groundContatctCount > 0;
    bool OnSteep => steepContactCount > 0;

    float minGroundDotProd,minStairDotProd;
    int stepsSinceLastGrounded,stepsSinceLastJump;

    private void OnValidate()
    {
        minGroundDotProd = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairDotProd = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    private void Awake()

    {
        if (body == null)
            body = GetComponent<Rigidbody>();
        OnValidate();
        c = meshRenderer.material.color;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1<<layer))==0 ? minGroundDotProd : minStairDotProd;
    }

    // Update is called once per frame
    void Update()
    {
        wasDesiredGrab = desiredGrab;

        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        velocity = body.velocity;
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y)*maxSpeed;

        desiredJump |= Input.GetKeyDown(jumpKey);
        if (Input.GetKeyDown(grabKey))
        {
            desiredGrab = !desiredGrab;
            if (desiredGrab && !OnSteep)
                desiredGrab = false;
        }

        if (desiredJump)
            desiredGrab = false; // relase

        if (OnSteep)
        {
            meshRenderer.material.color = Color.black;
        }
        else if (desiredGrab)
        {
            meshRenderer.material.color = Color.white;
        }
        else
        {
            meshRenderer.material.color = c;
        }
    }

    private void FixedUpdate()
    {
        UpdateState();

        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;

        if (desiredGrab)
        {
            TryGrabWall();
        }
        else if (wasDesiredGrab && !desiredGrab)
        {
            ReleaseGrab();
        }

        ClearState();
    }

    private void TryGrabWall()
    {
        if (OnSteep)
        {
            body.velocity = Vector3.zero;
            body.useGravity = false;
        }
    }

    void ReleaseGrab()
    {
        body.useGravity = true;
    }

    private void ClearState()
    {
        groundContatctCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        body.useGravity = true;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;

            if(stepsSinceLastJump > 1)
                jumpPhase = 0;

            if (groundContatctCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }

    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapToGroundSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(body.position, Vector3.down,out RaycastHit hit,snapToGoundProbeDist,probeMask))
        {
            return false;
        }
        if (hit.normal.y < GetMinDot(hit.transform.gameObject.layer))
        {
            return false;
        }
        groundContatctCount = 1;
        contactNormal = hit.normal;

        float dot = Vector3.Dot(velocity, hit.normal);
        if(dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProd)
            {
                groundContatctCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
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

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision col)
    {
        float minDot = GetMinDot(col.gameObject.layer);
        for (int i = 0; i < col.contactCount; i++)
        {
            Vector3 normal = col.GetContact(i).normal;
            if (normal.y >= minDot)
            {
                groundContatctCount += 1;
                contactNormal += normal;
            }
            else if(normal.y > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }
}
