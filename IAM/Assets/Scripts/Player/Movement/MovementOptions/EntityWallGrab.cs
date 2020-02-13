using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, PossibleTransitionRequestTypes(typeof(WallGrabTransitionRequest),typeof(WallGrabReleaseTransitionRequest))]
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

    public override string Name => "Wall Grab";

    WallGrabTransitionRequest wallGrabRequest = new WallGrabTransitionRequest();
    WallGrabReleaseTransitionRequest relaseRequest = new WallGrabReleaseTransitionRequest();

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
            RequestStateChange(wallGrabRequest);
            playerCollider.material = wallGrabMaterial;
        }
        else
        {
            RequestStateChange(relaseRequest);
            playerCollider.material = defaultMovmentMaterial;
        }
    }
}

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.PlayerInput,"On Wall Grab","The player is on a wall and is holding the wall grab key")]
public class WallGrabTransitionRequest : TransitionRequest { }

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.InputOrPhysics, "On Wall Grab End","The player is no longer holding the wall grab key, or the wall he was grabbing onto does no longer exist")]
public class WallGrabReleaseTransitionRequest : TransitionRequest { }

