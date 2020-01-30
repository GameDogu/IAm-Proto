using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

/// <summary>
/// Not abstract class cause unity serialization shits the bed in that case
/// </summary>
public class EntityMovementHandler : MonoBehaviour
{
    public event Action OnUpdate;
    public event Action OnFixedUpdate;

    [SerializeField] Player player = null;
    public Player Player => player;

    /// <summary>
    /// On rename go to custom inspector as well
    /// </summary>
    [SerializeField]List<EntityMovementOption> movementOptions = default;

    Rigidbody body => player.Body;

    public Vector3 Velocity { get; protected set; }
    
    public Vector3 PlayerDirection => Velocity.normalized;

    public float Speed => Velocity.magnitude;

    EntityWallGrab grabHandler = null;
    public bool IsGrabbing => grabHandler != null ? grabHandler.IsGrabbing : false;

    EntityJump jumpHandler = null;

    private void OnValidate()
    {
        if (movementOptions != null)
        {
            grabHandler = Find<EntityWallGrab>();
            jumpHandler = Find<EntityJump>();
        }

    }

    T Find<T>()where T : EntityMovementOption
    {
        for (int i = 0; i < movementOptions.Count; i++)
        {
            if (movementOptions[i] != null && movementOptions[i] is T)
                return movementOptions[i] as T;
        }
        return null;
    }

    private void Awake()
    {
        for (int i = 0; i < movementOptions.Count; i++)
        {
            movementOptions[i].StartUp();
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
        if (jumpHandler != null)
            jumpHandler.ResetJumpPhase();
    }

    public bool CheckPlayerActionPreventsGroundSnapping()
    {
        if (jumpHandler != null)
            return jumpHandler.RecentlyJumped;
        return false;
    }

    public void AddMovementOption(EntityMovementOption option)
    {
        if (movementOptions == null)
        {
            Debug.Log("new list");
            movementOptions = new List<EntityMovementOption>();
        }

        if (movementOptions.Contains(option))
            return; //already contained don't double add
        movementOptions.Add(option);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

}
