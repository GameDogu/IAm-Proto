using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that allows Entity to grab onto walls
/// </summary>
[System.Serializable, PossibleTransitionRequestTypes(typeof(WallGrabTransitionRequest),typeof(WallGrabReleaseTransitionRequest))]
public class EntityWallGrab : UpdateOnlyMovementOption
{
    /// <summary>
    /// the default material the entity has whilst moving around
    /// </summary>
    [Header("PhysicsMaterial")]
    [SerializeField] PhysicMaterial defaultMovmentMaterial = null;
    /// <summary>
    /// the physics material the entity has whilst grabbing onto the wall
    /// </summary>
    [SerializeField] PhysicMaterial wallGrabMaterial = null;

    /// <summary>
    /// The key that needs to be pressed to grab onto the wall
    /// </summary>
    [Header("Keys")]
    [SerializeField] KeyCode grabKey = KeyCode.F;
    //Extract player wallgrab from player movement
    /// <summary>
    /// The collider of the player
    /// </summary>
    Collider playerCollider => player.Collider;

    /// <summary>
    /// if player is on steep contact surface
    /// </summary>
    bool OnSteep => player.CollisionHandler.OnSteep;

    /// <summary>
    /// Is the the grab key pressed
    /// </summary>
    public bool IsGrabbing => Input.GetKey(grabKey);

    /// <summary>
    /// name of the component when displayed
    /// </summary>
    public override string Name => "Wall Grab";

    /// <summary>
    /// state change request dispatched on wall grabbing
    /// </summary>
    WallGrabTransitionRequest wallGrabRequest = new WallGrabTransitionRequest();
    /// <summary>
    /// state change request made when grab key is releassed
    /// </summary>
    WallGrabReleaseTransitionRequest relaseRequest = new WallGrabReleaseTransitionRequest();

    /// <summary>
    /// validates component
    /// </summary>
    protected override void Validate()
    {
        playerCollider.material = defaultMovmentMaterial;
    }

    /// <summary>
    /// Initializes component for this states handler
    /// </summary>
    /// <param name="handler">the movement handler of a movement state</param>
    protected override void Initialize(StateMovementHandler handler)
    {
        base.Initialize(handler);
        Validate();
    }


    /// <summary>
    /// update procedure
    /// checks if wall grabbing 
    /// dispatches request and changes physics material
    /// otwherwise 
    /// dispatch release request and reset physics material
    /// </summary>
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

/// <summary>
/// State change request on wall grab
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.PlayerInput,"On Wall Grab","The player is on a wall and is holding the wall grab key")]
public class WallGrabTransitionRequest : TransitionRequest { }

/// <summary>
/// state change request on grab release
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.InputOrPhysics, "On Wall Grab End","The player is no longer holding the wall grab key, or the wall he was grabbing onto does no longer exist")]
public class WallGrabReleaseTransitionRequest : TransitionRequest { }

