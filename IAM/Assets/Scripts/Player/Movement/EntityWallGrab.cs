using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityWallGrab : UpdateOnlyMovementOption
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

    protected override void Initialize(StateMovementHandler handler)
    {
        base.Initialize(handler);
        Validate();
    }

    protected override void UpdateProcedure()
    {
        if ( IsGrabbing && OnSteep)
        {
            RequestStateChange();
            playerCollider.material = wallGrabMaterial;
        }
        else
        {
            playerCollider.material = defaultMovmentMaterial;
        }
    }
}
