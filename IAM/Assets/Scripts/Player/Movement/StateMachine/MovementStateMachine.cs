using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TEMP
using System.Linq;

public class MovementStateMachine : MonoBehaviour
{
    static uint idGen = 0;
    static uint IDGen => idGen++;

    #region Members

    [SerializeField] List<EntityMovementOption> generalMovementOptions = null;

    public List<EntityMovementOption> GeneralMovementOptions => generalMovementOptions;

    [SerializeField] Player player = default;

    public Player Player => player;

    public MovementState CurrentState { get; protected set; }

    [SerializeField] List<MovementState> movementStates = new List<MovementState>();

    public int StateCount => movementStates.Count;

    Dictionary<uint, MovementState> idMappedMovementStates = new Dictionary<uint, MovementState>();

    Queue<TransitionRequest> stateChangeRequests = new Queue<TransitionRequest>();

    CollisionHandlerEventHandler _collisionHandlerTransitionEventHandler;
    CollisionHandlerEventHandler collisionHandlerTransitionEventHandler
    {
        get
        {
            if (_collisionHandlerTransitionEventHandler == null)
                _collisionHandlerTransitionEventHandler = new CollisionHandlerEventHandler(this);
            return _collisionHandlerTransitionEventHandler;
        }
    }

    #endregion

    #region Unity Built In Functions

    public void Awake()
    {

        CurrentState = new MovementState(0, "Everything Bagel", this);
        var options = Enumerable.Range(0, generalMovementOptions.Count);
        CurrentState.Initialize(options.ToList());

        BuildStateMappingAndInitialize();
    }

    private void Start()
    {
        CurrentState.Start(null);
    }

    private void Update()
    {
        HandlePotentialTransition();

        CurrentState.Update();

        HandlePotentialTransition();
    }

    private void FixedUpdate()
    {
        HandlePotentialTransition();

        player.CollisionHandler.UpdateState();

        CurrentState.FixedUpdate();

        player.CollisionHandler.ClearState();

        HandlePotentialTransition();
    }

    //private void OnDrawGizmos()
    //{
    //    if (movementStates != null)
    //    {
    //        for (int i = 0; i < movementStates.Count; i++)
    //        {
    //            movementStates[i].Print(Debug.unityLogger);
    //        }
    //    }
    //}
#endregion

    public EntityMovementOption GetMovementOption(int idx)
    {
        if (idx >= generalMovementOptions.Count)
            throw new IndexOutOfRangeException();
        return generalMovementOptions[idx];
    }

    public int GetIndexForMovementOption(EntityMovementOption option)
    {
        return generalMovementOptions.IndexOf(option);
    }

    public void AddGeneralMovementOption(EntityMovementOption option)
    {
        if (generalMovementOptions == null)
        {
            Debug.Log("New List");
            generalMovementOptions = new List<EntityMovementOption>();
        }

        if (!generalMovementOptions.Contains(option))
        {
            generalMovementOptions.Add(option);
        }

    }

    public void RemoveGeneralMovementOption(EntityMovementOption opt)
    {
        int idx = generalMovementOptions.IndexOf(opt);

        if (idx < 0)
            return;

        if (idx < StateCount - 1)
        {
            generalMovementOptions[idx] = generalMovementOptions[StateCount - 1];
            idx = StateCount - 1;
        }
        generalMovementOptions.RemoveAt(idx);


        for (int i = 0; i < StateCount; i++)
        {
            var st = movementStates[i];
            st.RemoveMovementOption(opt);
        }
    }

    public bool AddNewState(MovementState movementState)
    {
        if (!movementStates.Exists(st => st.ID == movementState.ID))
        {
            movementStates.Add(movementState);
            return true;
        }
        return false;
    }

    public MovementState AddNewState()
    {
        uint id = IDGen;
        var st = new MovementState(id, $"State {id}", this);
        movementStates.Add(st);
        return st;
    }

    MovementState GetStateByID(uint id)
    {
        if (idMappedMovementStates.ContainsKey(id))
        {
            return idMappedMovementStates[id];
        }
        throw new Exception($"Trying to transition to non recognized state {id}");
    }

    public void OnStateMarkedAsInitial(MovementState newlyMarked)
    {
        for (int i = 0; i < StateCount; i++)
        {
            var state = movementStates[i];
            if (state != newlyMarked)
            {
                state.InitialState = false;
            }
        }
    }

    public void RemoveState(MovementState state)
    {
        if (movementStates.Remove(state))
        {
            idMappedMovementStates.Remove(state.ID);

            for (int i = 0; i < StateCount; i++)
            {
                var otherState = movementStates[i];
                otherState.RemoveTransition(state.ID);
            }
        }
    }

    public void RemoveTransition(Transition transition)
    {
        for (int i = 0; i < StateCount; i++)
        {
            if (movementStates[i].RemoveTransition(transition))
            {
                break;
            }
        }
    }

    public void RemoveTransition(Transition transition, MovementState state)
    {
        state.RemoveTransition(transition);
    }

    private void BuildStateMappingAndInitialize()
    {
        for (int i = 0; i < StateCount; i++)
        {
            var state = movementStates[i];
            idMappedMovementStates.Add(state.ID, state);
            if (state.InitialState)
                CurrentState = state;
        }
    }

    public void Transition(Transition trans)
    {
        //change to new state
        var prevState = CurrentState;
        CurrentState = GetStateByID(trans.NextStateID);
        ClearStateChangeRequests();
        CurrentState.Start(prevState);
    }

    public void HandlePotentialTransition()
    {
        if (stateChangeRequests.Count > 0)
        {
            Transition trans = null;
            while (stateChangeRequests.Count > 0 && trans == null)
            {
                var activator = stateChangeRequests.Dequeue();
                trans = CurrentState.CheckTransitionRequest(activator);
            }
            if (trans != null)
            {
                Transition(trans);
            }
        }
        ClearStateChangeRequests();
    }

    public void ClearStateChangeRequests()
    {
        stateChangeRequests.Clear();
    }

    public void EnqueRequestStateChange(TransitionRequest request)
    {
        stateChangeRequests.Enqueue(request);
    }

    public List<TransitionRequest> GetPossibleTransitionRequests()
    {
        List<TransitionRequest> poss = new List<TransitionRequest>();

        collisionHandlerTransitionEventHandler.AddPossibleReqeusts(ref poss);

        for (int i = 0; i < generalMovementOptions.Count; i++)
        {
            var opt = generalMovementOptions[i];
            poss.Add(opt.TransitionRequst);
        }

        return poss;
    }

    class CollisionHandlerEventHandler
    {
        MovementStateMachine machine;

        OnGroundHitTransitionRequest onGroundHitTransitionRequest = new OnGroundHitTransitionRequest();
        OnInAirTransitionRequest onInAirTransitionRequest = new OnInAirTransitionRequest();
        OnHitWallTransitionRequest onHitWallTransitionRequest = new OnHitWallTransitionRequest();

        public CollisionHandlerEventHandler(MovementStateMachine machine)
        {
            this.machine = machine;
            SetUp();
        }

        private void SetUp()
        {
            var colHandler = machine.Player.CollisionHandler;

            colHandler.OnGroundedStart -= OnHitGround;
            colHandler.OnGroundedStart += OnHitGround;

            colHandler.OnInAirStart -= OnInAir;
            colHandler.OnInAirStart += OnInAir;

            colHandler.OnWallStart -= OnHitWall;
            colHandler.OnWallStart += OnHitWall;
        }

        void OnInAir()
        {
            Enqueue(onInAirTransitionRequest);
        }

        void OnHitGround()
        {
            Enqueue(onGroundHitTransitionRequest);
        }

        void OnHitWall()
        {
            Enqueue(onHitWallTransitionRequest);
        }

        void Enqueue(TransitionRequest request)
        {
            machine.EnqueRequestStateChange(request);
        }

        public void AddPossibleReqeusts(ref List<TransitionRequest> possRequests)
        {
            possRequests.Add(onGroundHitTransitionRequest);
            possRequests.Add(onInAirTransitionRequest);
            possRequests.Add(onHitWallTransitionRequest);
        }
    }
}
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics, "On Player Hits Ground")]
public class OnGroundHitTransitionRequest : TransitionRequest
{
    //public TransitionRequestOnGroundHit(uint id) : base(id)
    //{
    //}
}

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics, "On Player Starts Being In Air")]
public class OnInAirTransitionRequest : TransitionRequest
{
    //public TransitionRequestOnStartBeingInAir(uint id) : base(id)
    //{
    //}
}

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics,"On Player Hits Wall")]
public class OnHitWallTransitionRequest : TransitionRequest { }