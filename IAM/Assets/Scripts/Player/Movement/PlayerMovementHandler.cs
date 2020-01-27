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

    Vector3 velocity;
    public Vector3 Velocity => velocity;
    
    public Vector3 PlayerDirection => velocity.normalized;

    public float Speed => velocity.magnitude;

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
        velocity = body.velocity;

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
        velocity += vel;
    }

    private void FixedUpdate()
    {
        velocity = body.velocity;
        player.CollisionHandler.UpdateState();

        OnFixedUpdate?.Invoke();

        body.velocity = velocity;

        player.CollisionHandler.ClearState();
    }

    public void AlignVelocityWithContactNormal(Vector3 normal)
    {
        float dot = Vector3.Dot(velocity, normal);
        if (dot > 0f)
        {
            velocity = (velocity - normal * dot).normalized * Speed;
        }
    }

    public void ResetJumpPhase()
    {
        if (jumpHandler)
            jumpHandler.ResetJumpPhase();
    }

}
