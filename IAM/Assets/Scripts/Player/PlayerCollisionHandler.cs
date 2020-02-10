using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    struct CollisionHandlerStateSheet
    {
        PlayerCollisionHandler handler;

        public int groundContatctCount;
        public int steepContactCount;
        public int stepsSinceLastGrounded;

        public CollisionHandlerStateSheet(PlayerCollisionHandler handler, int groundContatctCount = 0, int steepContactCount = 0, int stepsSinceLastGrounded = 0)
        {
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.groundContatctCount = groundContatctCount;
            this.steepContactCount = steepContactCount;
            this.stepsSinceLastGrounded = stepsSinceLastGrounded;
        }

        public static void CompareAndInvoke(CollisionHandlerStateSheet current,CollisionHandlerStateSheet prev)
        {
            var handler = current.handler;
            if (current.stepsSinceLastGrounded == 1 && prev.stepsSinceLastGrounded == 0)
                handler.OnInAirStart?.Invoke();
            if (current.groundContatctCount >= 1 && prev.stepsSinceLastGrounded > 0)
                handler.OnGroundedStart?.Invoke();
            if (current.steepContactCount >= 1 && prev.steepContactCount < 1)
                handler.OnWallStart?.Invoke();
        }

        public void CopyValues(CollisionHandlerStateSheet src)
        {
            groundContatctCount = src.groundContatctCount;
            steepContactCount = src.steepContactCount;
            stepsSinceLastGrounded = src.stepsSinceLastGrounded;
        }

    }

    public event Action OnStateUpdateStart;

    public event Action OnGroundedStateUpdate;
    public event Action OnGroundedStart;

    public event Action OnInAirStateUpdate;
    public event Action OnInAirStart;

    public event Action OnStateUpdateEnd;

    public event Action OnClearState;

    public event Action OnWallStart;

    [SerializeField] Player player = null;
    Rigidbody body => player.Body;
    StateMovementHandler movementHandler => player.MovementStateMachine.CurrentState.MovementHandler;

    [Header("Walkability of Ground")]
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    [SerializeField, Range(0f, 90f)] float maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)] float maxSnapToGroundSpeed = 100f;
    [SerializeField, Range(0f, 1f)] float snapToGoundProbeDist = 1f;
    [SerializeField] LayerMask probeMask = -1;
    [SerializeField] LayerMask stairsMask = -1;

    public Vector3 ContactNormal { get; protected set; }
    public Vector3 SteepNormal { get; protected set; }

    CollisionHandlerStateSheet prevState;
    CollisionHandlerStateSheet currentState;

    //TODO Copy before potentialy updateing values to prev
    //on (end of) update  state compare and invoke

    int groundContatctCount
    {
        get { return currentState.groundContatctCount; }
        set { currentState.groundContatctCount = value; }
    }

    int steepContactCount
    {
        get { return currentState.steepContactCount; }
        set { currentState.steepContactCount = value; }
    }

    int stepsSinceLastGrounded
    {
        get { return currentState.stepsSinceLastGrounded; }
        set { currentState.stepsSinceLastGrounded = value; }
    }

    public bool OnGround => groundContatctCount > 0;
    public bool OnSteep => steepContactCount > 0;
    
    float minGroundDotProd, minStairDotProd;

    private void OnValidate()
    {
        minGroundDotProd = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairDotProd = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    private void Start()
    {
        prevState = new CollisionHandlerStateSheet(this);
        currentState = new CollisionHandlerStateSheet(this);
    }

    public void ClearState()
    {
        prevState.CopyValues(currentState);
        groundContatctCount = steepContactCount = 0;
        ContactNormal = SteepNormal = Vector3.zero;
        OnClearState?.Invoke();
    }

    public void UpdateState()
    {
        OnStateUpdateStart?.Invoke();

        stepsSinceLastGrounded += 1;
        body.useGravity = true;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            OnGroundedStateUpdate?.Invoke();
            stepsSinceLastGrounded = 0;

            if (groundContatctCount > 1)
            {
                ContactNormal.Normalize();
            }
        }
        else
        {
            ContactNormal = Vector3.up;
            OnInAirStateUpdate?.Invoke();
        }

        CollisionHandlerStateSheet.CompareAndInvoke(currentState, prevState);

        OnStateUpdateEnd?.Invoke();
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || PlayerInputPrevented())
        {
            return false;
        }
        float speed = movementHandler.Speed;
        if (speed > maxSnapToGroundSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, snapToGoundProbeDist, probeMask))
        {
            return false;
        }
        if (hit.normal.y < GetMinDot(hit.transform.gameObject.layer))
        {
            return false;
        }
        groundContatctCount = 1;
        ContactNormal = hit.normal;
        return true;
    }

    private bool PlayerInputPrevented()
    {
        return movementHandler.CheckPlayerActionPreventsGroundSnapping();
    }

    public bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            SteepNormal.Normalize();
            if (SteepNormal.y >= minGroundDotProd)
            {
                groundContatctCount = 1;
                ContactNormal = SteepNormal;
                return true;
            }
        }
        return false;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProd : minStairDotProd;
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
                ContactNormal += normal;
            }
            else if (normal.y > -0.01f)
            {
                steepContactCount += 1;
                SteepNormal += normal;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }
}

