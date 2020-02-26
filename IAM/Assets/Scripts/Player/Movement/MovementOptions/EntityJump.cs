using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that allows entity to jump
/// </summary>
[System.Serializable,PossibleTransitionRequestTypes(typeof(JumpTransitionRequest))]
public class EntityJump : DualLoopMovementOption
{
    /// <summary>
    /// jump height
    /// </summary>
    [Header("Jump")]
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    /// <summary>
    /// how many jumps when in air
    /// </summary>
    [SerializeField, Range(0, 10)] int jumpsInAir = 2;
    /// <summary>
    /// direction change bonus when jumping away from a wall
    /// </summary>
    [SerializeField, Range(0f, 10f)] float wallJumpDirectionChangeBonus = 2f;
    /// <summary>
    /// key prssed for jumping
    /// </summary>
    [Header("Keys")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;

    /// <summary>
    /// collisision handler for the palyer
    /// </summary>
    PlayerCollisionHandler collisionHandler => player.CollisionHandler;

    /// <summary>
    /// is player on ground
    /// </summary>
    bool OnGround => collisionHandler.OnGround;
    /// <summary>
    /// is player in contact with steep surface(eg wall)
    /// </summary>
    bool OnSteep => collisionHandler.OnSteep;

    /// <summary>
    /// contact normal of non steep surface
    /// </summary>
    Vector3 contactNormal => collisionHandler.ContactNormal;
    /// <summary>
    /// contact normal of steep surface
    /// </summary>
    Vector3 steepNormal => collisionHandler.SteepNormal;

    /// <summary>
    /// velocity of the entity
    /// </summary>
    Vector3 velocity => handler.Velocity;

    /// <summary>
    /// Because of the collision data delay we're still considered grounded the step after the jump was initiated
    /// </summary>
    public bool RecentlyJumped => stepsSinceLastJump <= 2;

    /// <summary>
    /// display name of component
    /// </summary>
    public override string Name => "Jump";

    /// <summary>
    /// state change request made by this component
    /// </summary>
    JumpTransitionRequest jumpRequest = new JumpTransitionRequest();

    /// <summary>
    /// tracks if player desires to jump to comunicate it from the update
    /// procedure to the fixed update procedure
    /// </summary>
    bool desiredJump;
    /// <summary>
    /// jump pahse (how many jumps since last grounded)
    /// </summary>
    int jumpPhase;
    /// <summary>
    /// steps taken since last time in air
    /// </summary>
    int stepsSinceLastJump;

    /// <summary>
    /// Initializes base class and registers to collision handler update events
    /// </summary>
    /// <param name="handler">the movement handler of any movement state</param>
    protected override void Initialize(StateMovementHandler handler)
    {
        base.Initialize(handler);
        //Collision Handler events
        RegisterCollisonHandlerStartStateUpdate();
        RegisterCollisionHandlerGroundedStateUpdate();
    }

    /// <summary>
    /// register to update start of collision handler, ensuring no double registering happens
    /// </summary>
    void RegisterCollisonHandlerStartStateUpdate()
    {
        UnregisterCollisonHandlerUpdateStart();
        player.CollisionHandler.OnStateUpdateStart += OnCollisionHandlreStateUpdateStart;
    }

    /// <summary>
    /// unregister from collision handler update start
    /// </summary>
    void UnregisterCollisonHandlerUpdateStart()
    {
        player.CollisionHandler.OnStateUpdateStart -= OnCollisionHandlreStateUpdateStart;
    }

    /// <summary>
    /// register to grounded update of collision handler, ensuring no double registering
    /// </summary>
    void RegisterCollisionHandlerGroundedStateUpdate()
    {
        UnregisterCollisionHandlerGroundedStateUpdate();
        player.CollisionHandler.OnGroundedStateUpdate += OnCollisionHandlerUpdateGrounded;
    }

    /// <summary>
    /// unregister to grounded update of collision handler
    /// </summary>
    void UnregisterCollisionHandlerGroundedStateUpdate()
    {
        player.CollisionHandler.OnGroundedStateUpdate -= OnCollisionHandlerUpdateGrounded;
    }

    /// <summary>
    /// update procedure, check if jump key pressed
    /// </summary>
    protected override void UpdateProcedure()
    {
        desiredJump |= Input.GetKeyDown(jumpKey);
    }

    /// <summary>
    /// increase steps since last jump
    /// </summary>
    private void OnCollisionHandlreStateUpdateStart()
    {
        stepsSinceLastJump += 1;
    }

    /// <summary>
    /// check and reset jump phase if necessary
    /// </summary>
    private void OnCollisionHandlerUpdateGrounded() 
    {
        if (stepsSinceLastJump > 1)
                ResetJumpPhase();
    }

    /// <summary>
    /// fixed update procedure,
    /// if jump desired
    /// dispathc state change request
    /// and jump
    /// </summary>
    protected override void FixedUpdateProcedure()
    {
        if (desiredJump)
        {
            //we now are jumpings
            //TODO MOVE REQUEST TO UPDATE PROCEDURE
            RequestStateChange(jumpRequest);
            desiredJump = false;
            Jump();
        }
    }
    
    /// <summary>
    /// sets jump phase to 0
    /// </summary>
    public void ResetJumpPhase()
    {
        jumpPhase = 0;
    }

    /// <summary>
    /// handles velocity change calculations when jumping
    /// based on where we are jumping from
    /// what jump phase we are in
    /// </summary>
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
        jumpDirection = (jumpDirection + (Vector3.up * (1f - Vector3.Dot(jumpDirection.normalized, Vector3.up)))).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        handler.AddVelocity( jumpDirection * jumpSpeed + bonus);

    }

    /// <summary>
    /// handles wall jumping bonuses
    /// </summary>
    /// <param name="jumpDirection">the jump direction of this jump</param>
    /// <returns>the jump directiopn modified with any wall jump bonuses</returns>
    private Vector3 HandleWallJump(Vector3 jumpDirection)
    {
        return jumpDirection.normalized * wallJumpDirectionChangeBonus;
    }

    /// <summary>
    /// determins jump height based on jump phase
    /// </summary>
    /// <param name="jumpPhase">current jump phase</param>
    /// <returns>jump height modified by anything the current jump phase dictates</returns>
    private float DetermineJumpHeight(int jumpPhase)
    {
        return jumpHeight * (float)jumpPhase;
    }
}

/// <summary>
/// State change request dispatched by jump component
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.PlayerInput, "On Jump","The player has pressed the jump button and the fixed update procedure acknowledged this")]
public class JumpTransitionRequest : TransitionRequest
{}