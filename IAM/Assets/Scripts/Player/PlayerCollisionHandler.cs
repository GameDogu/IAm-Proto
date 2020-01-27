using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField] Player player = null;
    Rigidbody body => player.Body;
    PlayerMovementHandler movementHandler => player.MovementHandler;

    [Header("Walkability of Ground")]
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    [SerializeField, Range(0f, 90f)] float maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)] float maxSnapToGroundSpeed = 100f;
    [SerializeField, Range(0f, 1f)] float snapToGoundProbeDist = 1f;
    [SerializeField] LayerMask probeMask = -1;
    [SerializeField] LayerMask stairsMask = -1;

    Vector3 contactNormal, steepNormal;
    public Vector3 ContactNormal => contactNormal;
    public Vector3 SteepNormal => steepNormal;

    int groundContatctCount, steepContactCount;

    public bool OnGround => groundContatctCount > 0;
    public bool OnSteep => steepContactCount > 0;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    float minGroundDotProd, minStairDotProd;

    private void OnValidate()
    {
        minGroundDotProd = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairDotProd = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    public void ClearState()
    {
        groundContatctCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    public void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        body.useGravity = true;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;

            if (stepsSinceLastJump > 1)
                player.MovementHandler.ResetJumpPhase();

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
        contactNormal = hit.normal;
        return true;
    }

    public bool CheckSteepContacts()
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
                contactNormal += normal;
            }
            else if (normal.y > -0.01f)
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

    public void ResetStepsSinceLastJump()
    {
        stepsSinceLastJump = 0;
    }
}
