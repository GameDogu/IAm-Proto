using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TEMP
using System.Linq;

public class MovementStateMachine : MonoBehaviour
{
    /// <summary>
    /// State ID gen tracker
    /// </summary>
    static uint idGen = 0;
    /// <summary>
    /// State ID generator
    /// </summary>
    static uint IDGen => idGen++;

    #region Members
    /// <summary>
    /// player the movement state machine belongs to
    /// </summary>
    [SerializeField] Player player = default;

    /// <summary>
    /// property name of player, for custrom inspector
    /// </summary>
    public string PlayerPropertyName => nameof(player);

    /// <summary>
    /// player getter
    /// </summary>
    public Player Player => player;

    /// <summary>
    /// List of all movements this entity can do at any point
    /// </summary>
    [HideInInspector] List<EntityMovementOption> generalMovementOptions = new List<EntityMovementOption>();

    /// <summary>
    /// readonly accessor to movement options of entity
    /// </summary>
    public IReadOnlyList<EntityMovementOption> GeneralMovementOptions => generalMovementOptions;

    /// <summary>
    /// the current state of the machine
    /// </summary>
    public MovementState CurrentState { get; protected set; }

    /// <summary>
    /// list of all states this machine has
    /// </summary>
    [HideInInspector] List<MovementState> movementStates = new List<MovementState>();

    /// <summary>
    /// readonly accessor of states of this machine
    /// </summary>
    public IReadOnlyList<MovementState> States => movementStates;

    /// <summary>
    /// count of states
    /// </summary>
    public int StateCount => movementStates.Count;

    /// <summary>
    /// path to the asset storing the state machine data
    /// (tried a weird workaround for the thing where prefabs couldn't reference
    /// scriptable objects, but they can have differing strings)
    /// </summary>
    [SerializeField] string movementStateMachineDataAssetPath;

    /// <summary>
    /// path to asset setting it resets isLoaded and nulls data it had previously
    /// </summary>
    [HideInInspector] public string MovementStateMachineDataAssetPath
    {
        get { return movementStateMachineDataAssetPath; }
        set
        {
            if (value != movementStateMachineDataAssetPath)
            {
                IsLoaded = false;
                data = null;
                movementStateMachineDataAssetPath = value;
            }
        }
    }

    /// <summary>
    /// state machine data from the asset path
    /// </summary>
    MovementStateMachineData data;

    /// <summary>
    /// getter for state machine data, tries to load from path if null
    /// </summary>
    public MovementStateMachineData Data
    {
        get
        {
            if (data == null)
                TryLoadData();
            return data;
        }
    }

    /// <summary>
    /// flag to now if machine is loaded or it needs loading from data
    /// </summary>
    public bool IsLoaded { get; protected set; }

    /// <summary>
    /// flag if id mapped state dictionary is mapped
    /// </summary>
    bool isMapped;
    
    /// <summary>
    /// mapping from state ID to state for faster access
    /// </summary>
    Dictionary<uint, MovementState> idMappedMovementStates = new Dictionary<uint, MovementState>();

    /// <summary>
    /// the transition to the next state
    /// </summary>
    NextStateTransitionTracker stateTransitionTracker = new NextStateTransitionTracker();

    /// <summary>
    /// handler for collision handler events that mihght cause a state transition
    /// </summary>
    CollisionHandlerEventHandler _collisionHandlerTransitionEventHandler;
    /// <summary>
    /// getter for collision handler ensuring not null (in editor mostly)
    /// </summary>
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

    /// <summary>
    /// propagates unity on validate call to individual states
    /// </summary>
    private void OnValidate()
    {
        for (int i = 0; i < movementStates.Count; i++)
        {
            movementStates[i].OnValidate();
        }
    }

    /// <summary>
    /// ensures collision handler set up, loads state machine from data,
    /// and builds state mapping
    /// </summary>
    public void Awake()
    {
        collisionHandlerTransitionEventHandler.SetUp();

        LoadFromData();

        BuildStateMapping();
    }

    /// <summary>
    /// starts current state (initial state in that case)
    /// </summary>
    private void Start()
    {
        CurrentState.Start(null);
    }

    /// <summary>
    /// handles potential state transitions and calls update on current state
    /// </summary>
    private void Update()
    {
        HandlePotentialTransition();

        CurrentState.Update();

        HandlePotentialTransition();
    }

    /// <summary>
    /// Handles potential transition, handles the collision handler state update calls
    /// calls fixed update on current state
    /// </summary>
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

    #region General Movement Option
    
    /// <summary>
    /// gets a movement option via type name
    /// </summary>
    /// <param name="typeName">name of the type</param>
    /// <returns>the movement option of this type, if existing</returns>
    public EntityMovementOption GetMovementOption(string typeName)
    {
        return generalMovementOptions.Find(opt => opt.GetType().Name == typeName);
    }

    /// <summary>
    /// finds index of movement option in general option list
    /// </summary>
    /// <param name="option">option we want the index of</param>
    /// <returns>the index of option</returns>
    public int GetIndexForMovementOption(EntityMovementOption option)
    {
        return generalMovementOptions.IndexOf(option);
    }

    /// <summary>
    /// adds a general movementoption to machine, if not already existing
    /// </summary>
    /// <param name="option"></param>
    public void AddGeneralMovementOption(EntityMovementOption option)
    {
        if (generalMovementOptions == null)
        {
            generalMovementOptions = new List<EntityMovementOption>();
        }

        if (!generalMovementOptions.Contains(option))
        {
            generalMovementOptions.Add(option);
        }

    }

    /// <summary>
    /// adds a general movement option from a type
    /// enusres type is subclasss of entitymovement option
    /// </summary>
    /// <param name="t">the type of movement we want to add</param>
    public void AddGeneralMovementOption(Type t)
    {
        if (!t.IsSubclassOf(typeof(EntityMovementOption)))
            return;
        if (gameObject.GetComponent(t) == null)
        {
            //add it
            AddGeneralMovementOption(gameObject.AddComponent(t) as EntityMovementOption);
        }
        else
        {
            AddGeneralMovementOption(gameObject.GetComponent(t) as EntityMovementOption);
        }
    }

    /// <summary>
    /// removes general movement option, propagates change to all states of machine
    /// to keep everything consistent
    /// </summary>
    /// <param name="opt"></param>
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
    #endregion

    #region States

    /// <summary>
    /// adds a new state to the machine if no state has that same ID
    /// </summary>
    /// <param name="movementState">the new state to add</param>
    /// <returns>true if added, false otherwise</returns>
    public bool AddNewState(MovementState movementState)
    {
        if (!movementStates.Exists(st => st.ID == movementState.ID))
        {
            movementStates.Add(movementState);
            return true;
        }
        return false;
    }

    /// <summary>
    /// creats and adds a new state to the  state machine
    /// </summary>
    /// <returns>the newly created state</returns>
    public MovementState AddNewState()
    {
        uint id = IDGen;
        var st = new MovementState(id, $"State {id}", this);
        movementStates.Add(st);
        return st;
    }

    /// <summary>
    /// gets state by it's ID
    /// </summary>
    /// <param name="id">ID of the state</param>
    /// <returns>the state with this ID</returns>
    public MovementState GetStateByID(uint id)
    {
        if (!isMapped)
            BuildStateMapping();

        if (idMappedMovementStates.ContainsKey(id))
        {
            return idMappedMovementStates[id];
        }
        throw new Exception($"Trying to transition to non recognized state {id}");
    }

    /// <summary>
    /// ensures only one state is every marked as initial state of state machine
    /// </summary>
    /// <param name="newlyMarked">the new inital state</param>
    public void OnStateMarkedAsInitial(MovementState newlyMarked)
    {
        for (int i = 0; i < StateCount; i++)
        {
            var state = movementStates[i];
            if (state != newlyMarked)
            {
                state.IsInitialState = false;
            }
        }
    }

    /// <summary>
    /// removes state from state machine (and mapping from id to state)
    /// informs other states of removal to remove all transitions to this state
    /// </summary>
    /// <param name="state">the state being removed</param>
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

    /// <summary>
    /// builds the mapping from ID to state
    /// </summary>
    private void BuildStateMapping()
    {
        if (isMapped)
            return;
        for (int i = 0; i < StateCount; i++)
        {
            var state = movementStates[i];
            idMappedMovementStates.Add(state.ID, state);
        }
        isMapped = true;
    }

    /// <summary>
    /// clears the mapping from ID to state
    /// </summary>
    private void ClearIdMapping()
    {
        idMappedMovementStates.Clear();
        isMapped = false;
    }

    #endregion

    #region Transitions

    /// <summary>
    /// tries to remove transition from every state
    /// until successful or transition not part of any state
    /// </summary>
    /// <param name="transition">the transition being removed</param>
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

    /// <summary>
    /// removes transition from a state
    /// relic from old design lol
    /// </summary>
    /// <param name="transition">the transition being removed</param>
    /// <param name="state">the state it's removed from</param>
    public void RemoveTransition(Transition transition, MovementState state)
    {
        state.RemoveTransition(transition);
    }

    /// <summary>
    /// transitions between states
    /// </summary>
    /// <param name="trans">the transitin guiding that transition</param>
    public void Transition(Transition trans)
    {
        //change to new state
        var prevState = CurrentState;
        CurrentState = GetStateByID(trans.NextStateID);
        ClearStateChangeRequests();
        CurrentState.Start(prevState);
    }

    /// <summary>
    /// checks if we have any transition request
    /// and searches the current state if any request matches a transition it has
    /// if yes-> transition
    /// </summary>
    public void HandlePotentialTransition()
    {
        if (stateTransitionTracker.HasTransition)
        {
            Transition(stateTransitionTracker.CurrentTransition);
        }
        ClearStateChangeRequests();
    }

    /// <summary>
    /// clears transition request queue
    /// </summary>
    public void ClearStateChangeRequests()
    {
        stateTransitionTracker.Clear();
    }

    /// <summary>
    /// enqueues a state change request
    /// </summary>
    /// <param name="priorityRequest">the request being enqueued</param>
    /// <param name="associatedTransition">the associated transition with this request</param>
    public void EnqueRequestStateChange(PriorityTransitionRequest priorityRequest,Transition associatedTransition)
    {
        stateTransitionTracker.HandlePotentialNewTransition(priorityRequest, associatedTransition);
    }

    /// <summary>
    /// gets all possible transition request that can be generated in this state
    /// </summary>
    /// <param name="st">the state interested in</param>
    /// <returns>list of transition requests this state can create</returns>
    public List<TransitionRequest> GetPossibleTransitionRequests(MovementState st)
    {
        List<TransitionRequest> poss = new List<TransitionRequest>();

        collisionHandlerTransitionEventHandler.AddPossibleReqeusts(ref poss);

        for (int i = 0; i < st.MovementOptions.Count; i++)
        {
            var opt = st.MovementOptions[i];

            PossibleTransitionRequestTypesAttribute att = Attribute.GetCustomAttribute(opt.GetType(), typeof(PossibleTransitionRequestTypesAttribute)) as PossibleTransitionRequestTypesAttribute;
            if (att != null)
            {
                for (int j = 0; j < att.Types.Length; j++)
                {
                    var reqT = TransitionRequest.Factory.BuildRequest(att.Types[j]);
                    poss.Add(reqT);
                }
            }
        }

        return poss;
    }
    #endregion

    #region Serialization

    /// <summary>
    /// fills a data object with all the data of the machine
    /// </summary>
    /// <param name="data">the data object needing filling</param>
    public void FillDataObject(MovementStateMachineData data)
    {
        for (int i = 0; i < generalMovementOptions.Count; i++)
        {
            data.AddGeneralMovementOption(generalMovementOptions[i]);
        }

        for (int i = 0; i < StateCount; i++)
        {
            data.AddState(movementStates[i]);
        }

    }

    /// <summary>
    /// saves the machine to a location
    /// </summary>
    /// <param name="path">the location where to save to</param>
    /// <returns>the serialized object created where the machine is saved</returns>
    public MovementStateMachineData Save(string path)
    {
        return MovementStateMachineData.CreateAssetAndSave(this, path);
    }

    /// <summary>
    /// tries to load machine data form asset path
    /// editor loads from asset datapath
    /// game from Resources
    /// </summary>
    private void TryLoadData()
    {
        IsLoaded = false;
#if UNITY_EDITOR
        data = UnityEditor.AssetDatabase.LoadAssetAtPath<MovementStateMachineData>(MovementStateMachineDataAssetPath);
#else
        data = Resources.Load<MovementStateMachineData>(MovementStateMachineDataAssetPath);
#endif
    }

    /// <summary>
    /// loads machine from data
    /// </summary>
    public void LoadFromData()
    {
        if (IsLoaded)
            return;

        generalMovementOptions.Clear();
        movementStates.Clear();
        CurrentState = null;
        ClearIdMapping();
        CurrentState = Data.InitializeStateMachine(this);

        idGen = movementStates.Max(state => state.ID)+1;

        IsLoaded = true;
    }

    /// <summary>
    /// reloads machine from data
    /// </summary>
    public void ReloadFromData()
    {
        IsLoaded = false;
        LoadFromData();
    }

    #endregion

    #region internal classes and structs
    /// <summary>
    /// handles all the events the collision handler has that might cause state transition
    /// </summary>
    class CollisionHandlerEventHandler
    {
        /// <summary>
        /// the state machine it belongs to
        /// </summary>
        MovementStateMachine machine;

        /// <summary>
        /// request propagted to machine when entity hits ground
        /// </summary>
        OnGroundHitTransitionRequest onGroundHitTransitionRequest = new OnGroundHitTransitionRequest();
        /// <summary>
        /// request being propagated when entity is in air
        /// </summary>
        OnInAirTransitionRequest onInAirTransitionRequest = new OnInAirTransitionRequest();
        /// <summary>
        /// request being propagated when entity hits wall
        /// </summary>
        OnHitWallTransitionRequest onHitWallTransitionRequest = new OnHitWallTransitionRequest();

        public CollisionHandlerEventHandler(MovementStateMachine machine)
        {
            this.machine = machine;
        }

        /// <summary>
        /// registers functions to correct events, ensuring no double registering
        /// </summary>
        public void SetUp()
        {
            var colHandler = machine.Player.CollisionHandler;

            colHandler.OnGroundedStart -= OnHitGround;
            colHandler.OnGroundedStart += OnHitGround;

            colHandler.OnInAirStart -= OnInAir;
            colHandler.OnInAirStart += OnInAir;

            colHandler.OnWallStart -= OnHitWall;
            colHandler.OnWallStart += OnHitWall;
        }

        /// <summary>
        /// enqueues in air request
        /// </summary>
        void OnInAir()
        {
            Enqueue(onInAirTransitionRequest);
        }

        /// <summary>
        /// enqueues on ground request
        /// </summary>
        void OnHitGround()
        {
            Enqueue(onGroundHitTransitionRequest);
        }

        /// <summary>
        /// enqueues on hit wall request
        /// </summary>
        void OnHitWall()
        {
            Enqueue(onHitWallTransitionRequest);
        }

        /// <summary>
        /// enquues a request in the state machiene queue
        /// </summary>
        /// <param name="request">the request being enqueued</param>
        void Enqueue(TransitionRequest request)
        {
            machine.CurrentState.RequestStateChange(request);
        }

        /// <summary>
        /// adds all possible request to a list of requests
        /// </summary>
        /// <param name="possRequests">the list we want to add to</param>
        public void AddPossibleReqeusts(ref List<TransitionRequest> possRequests)
        {
            possRequests.Add(onGroundHitTransitionRequest);
            possRequests.Add(onInAirTransitionRequest);
            possRequests.Add(onHitWallTransitionRequest);
        }
    }

    struct NextStateTransitionTracker
    {
        PriorityTransitionRequest currentRequestPriority;
        public Transition CurrentTransition { get; private set; }

        public bool HasTransition => CurrentTransition != null;

        public void HandlePotentialNewTransition(PriorityTransitionRequest potenetialNewRequest,Transition newAssociatedTransition)
        {
            if (HasTransition)
            {
                if (potenetialNewRequest.HasPriorityOver(currentRequestPriority))
                {
                    Set(potenetialNewRequest, newAssociatedTransition);
                }
            }
            else
                Set(potenetialNewRequest, newAssociatedTransition);
        }

        public void Clear()
        {
            CurrentTransition = null;
        }

        void Set(PriorityTransitionRequest pR, Transition t)
        {
            currentRequestPriority = pR;
            CurrentTransition = t;
        }

    }
#endregion
}

/// <summary>
/// state change request when ground hit
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics, "On Player Hits Ground","The player model has a collision with a ground collider and was in the air previously")]
public class OnGroundHitTransitionRequest : TransitionRequest
{
    //public TransitionRequestOnGroundHit(uint id) : base(id)
    //{
    //}
}

/// <summary>
/// state change request when entity is in air
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics, "On Player Starts Being In Air","The player model is not in contact with any collider, and was in the previous contact")]
public class OnInAirTransitionRequest : TransitionRequest
{
    //public TransitionRequestOnStartBeingInAir(uint id) : base(id)
    //{
    //}
}

/// <summary>
/// state change request when entity hits wall
/// </summary>
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics,"On Player Hits Wall","The player is in contact with a wall collider, and was not in the previous update")]
public class OnHitWallTransitionRequest : TransitionRequest { }