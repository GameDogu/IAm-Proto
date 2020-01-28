using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallGrab : PlayerMovement
{
    [Header("PhysicsMaterial")]
    [SerializeField] PhysicMaterial defaultMovmentMaterial = null;
    [SerializeField] PhysicMaterial wallGrabMaterial = null;

    [Header("Keys")]
    [SerializeField] KeyCode grabKey = KeyCode.F;
    //Extract player wallgrab from player movement
    Collider playerCollider => player.Collider;
    bool OnSteep => player.CollisionHandler.OnSteep;

    public bool IsGrabbing => Input.GetKey(grabKey);

    protected override void Validate()
    {
        playerCollider.material = defaultMovmentMaterial;
    }

    protected override void Initialize()
    {
        Validate();
        RegisterUpdateCall(UpdateProcedure);
    }

    public override void Stop()
    {
        UnregisterUpdateCall(UpdateProcedure);
    }

    private void UpdateProcedure()
    {
        if ( IsGrabbing && OnSteep)
        {
            playerCollider.material = wallGrabMaterial;
        }
        else
        {
            playerCollider.material = defaultMovmentMaterial;
        }
    }
}
