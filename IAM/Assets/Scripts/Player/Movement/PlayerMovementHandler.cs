using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMovementHandler : MonoBehaviour
{
    public event Action OnUpdate;
    public event Action OnFixedUpdate;

    [SerializeField] Player player = null;
    public Player Player => player;

    [SerializeField] List<PlayerMovement> possibleMovement = null;

    Rigidbody body => player.Body;

    public Vector3 Velocity { get; protected set; }
    
    public Vector3 PlayerDirection => Velocity.normalized;

    public float Speed => Velocity.magnitude;

    PlayerWallGrab grabHandler = null;
    public bool IsGrabbing => grabHandler != null ? grabHandler.IsGrabbing : false;

    PlayerJump jumpHandler = null;

    private void OnValidate()
    {
        if (possibleMovement != null)
        {
            grabHandler = Find(typeof(PlayerWallGrab)) as PlayerWallGrab;
            jumpHandler = Find(typeof(PlayerJump)) as PlayerJump;
        }
    }

    PlayerMovement Find(Type type)
    {
        for (int i = 0; i < possibleMovement.Count; i++)
        {
            if (possibleMovement[i] != null && possibleMovement[i].GetType() == type)
                return possibleMovement[i];
        }
        return null;
    }

    private void Awake()
    {
        for (int i = 0; i < possibleMovement.Count; i++)
        {
            possibleMovement[i].StartUp();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Velocity = body.velocity;

        OnUpdate?.Invoke();

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

    public void AddVelocity(Vector3 vel)
    {
        Velocity += vel;
    }

    private void FixedUpdate()
    {
        Velocity = body.velocity;
        player.CollisionHandler.UpdateState();

        OnFixedUpdate?.Invoke();

        body.velocity = Velocity;

        player.CollisionHandler.ClearState();
    }

    public void AlignVelocityWithContactNormal(Vector3 normal)
    {
        float dot = Vector3.Dot(Velocity, normal);
        if (dot > 0f)
        {
            Velocity = (Velocity - normal * dot).normalized * Speed;
        }
    }

    public void ResetJumpPhase()
    {
        if (jumpHandler)
            jumpHandler.ResetJumpPhase();
    }

    public bool CheckPlayerActionPreventsGroundSnapping()
    {
        if(jumpHandler)
            return jumpHandler.RecentlyJumped;
        return false;
    }
}
