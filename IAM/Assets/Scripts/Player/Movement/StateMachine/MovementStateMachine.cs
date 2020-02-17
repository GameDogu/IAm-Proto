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

    [SerializeField] Player player = default;

    public string PlayerPropertyName => nameof(player);

    public Player Player => player;

    [HideInInspector]List<EntityMovementOption> generalMovementOptions = new List<EntityMovementOption>();

    public IReadOnlyList<EntityMovementOption> GeneralMovementOptions => generalMovementOptions;

    public MovementState CurrentState { get; protected set; }

    [HideInInspector] List<MovementState> movementStates = new List<MovementState>();

    public IReadOnlyList<MovementState> States => movementStates;

    public int StateCount => movementStates.Count;

    [SerializeField]string movementStateMachineDataAssetPath;
    [HideInInspector] public string MovementStateMachineDataAssetPath
    {
        get { return movementStateMachineDataAssetPath; }
        set
        {
            if (value != movementStateMachineDataAssetPath)
            {
                isLoadedFromData = false;
                data = null;
                movementStateMachineDataAssetPath = value;
            }
        }
    }
    MovementStateMachineData data;

    public MovementStateMachineData Data
    {
        get
        {
            if (data == null)
                TryLoadData();
            return data;
        }
    }
    bool isLoadedFromData;
    public bool IsLoaded => isLoadedFromData;

    bool isMapped;
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
        collisionHandlerTransitionEventHandler.SetUp();

        LoadFromData();

        BuildStateMapping();
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

    #region General Movement Option
    public EntityMovementOption GetMovementOption(string typeName)
    {
        return generalMovementOptions.Find(opt => opt.GetType().Name == typeName);
    }

    public int GetIndexForMovementOption(EntityMovementOption option)
    {
        return generalMovementOptions.IndexOf(option);
    }

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

    private void ClearIdMapping()
    {
        idMappedMovementStates.Clear();
        isMapped = false;
    }

    #endregion

    #region Transitions
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

    public MovementStateMachineData Save(string path)
    {
        return MovementStateMachineData.CreateAssetAndSave(this, path);
    }

    private void TryLoadData()
    {
        isLoadedFromData = false;
#if UNITY_EDITOR
        data = UnityEditor.AssetDatabase.LoadAssetAtPath<MovementStateMachineData>(MovementStateMachineDataAssetPath);
#else
        data = Resources.Load<MovementStateMachineData>(MovementStateMachineDataAssetPath);
#endif
    }

    public void LoadFromData()
    {
        if (isLoadedFromData)
            return;

        generalMovementOptions.Clear();
        movementStates.Clear();
        CurrentState = null;
        ClearIdMapping();
        CurrentState = Data.InitializeStateMachine(this);

        idGen = movementStates.Max(state => state.ID)+1;

        isLoadedFromData = true;
    }

    public void ReloadFromData()
    {
        isLoadedFromData = false;
        LoadFromData();
    }

    #endregion

    #region internal classes
    class CollisionHandlerEventHandler
    {
        MovementStateMachine machine;

        OnGroundHitTransitionRequest onGroundHitTransitionRequest = new OnGroundHitTransitionRequest();
        OnInAirTransitionRequest onInAirTransitionRequest = new OnInAirTransitionRequest();
        OnHitWallTransitionRequest onHitWallTransitionRequest = new OnHitWallTransitionRequest();

        public CollisionHandlerEventHandler(MovementStateMachine machine)
        {
            this.machine = machine;
        }

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
#endregion
}
[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics, "On Player Hits Ground","The player model has a collision with a ground collider and was in the air previously")]
public class OnGroundHitTransitionRequest : TransitionRequest
{
    //public TransitionRequestOnGroundHit(uint id) : base(id)
    //{
    //}
}

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics, "On Player Starts Being In Air","The player model is not in contact with any collider, and was in the previous contact")]
public class OnInAirTransitionRequest : TransitionRequest
{
    //public TransitionRequestOnStartBeingInAir(uint id) : base(id)
    //{
    //}
}

[TransitionRequestInfo(TransitionRequestInfoAttribute.RequestType.Physics,"On Player Hits Wall","The player is in contact with a wall collider, and was not in the previous update")]
public class OnHitWallTransitionRequest : TransitionRequest { }