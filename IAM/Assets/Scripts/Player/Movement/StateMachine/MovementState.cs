using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Movement state part ot a larger movement state machine
/// </summary>
public class MovementState : State<MovementState>
{
    /// <summary>
    /// The state machine this state belongs to
    /// </summary>
    public MovementStateMachine StateMachine { get; protected set; }

    /// <summary>
    /// list of all movement options this state allows
    /// </summary>
    List<EntityMovementOption> movementOptions;

    /// <summary>
    /// readonly accessor the list of movement options this state allows
    /// </summary>
    public IReadOnlyList<EntityMovementOption> MovementOptions => movementOptions;

    /// <summary>
    /// list of all transitions this state has
    /// </summary>
    List<Transition> transitions;

    /// <summary>
    /// readonly accessor the list of transitions this state has
    /// </summary>
    public IReadOnlyList<Transition> Transitions => transitions;

    /// <summary>
    /// count of transitions this state has
    /// </summary>
    public int TransitionCount => transitions.Count;

    /// <summary>
    /// flag if this is the initial state of it's corresponding movement state machine
    /// </summary>
    public bool IsInitialState { get; set; }

    /// <summary>
    /// The movement handler  of the state machine
    /// </summary>
    public StateMovementHandler MovementHandler { get; protected set; }

    /// <summary>
    /// check if state is initialized
    /// </summary>
    bool isInitialized;

    /// <summary>
    /// priority mapping for transition requests
    /// </summary>
    Dictionary<TransitionRequest, int> requestPriorityMapping;

    /// <summary>
    /// least priority one after the least prioritized mapped request
    /// </summary>
    int minPriorityValue;

    /// <summary>
    /// Accessor to all currently mapped requests 
    /// </summary>
    public IReadOnlyCollection<TransitionRequest> PriorityMappedRequests => requestPriorityMapping.Keys;
    
    public List<PriorityTransitionRequest> PrioritizedTransitionRequestList
    {
        get
        {
            List<PriorityTransitionRequest> list = new List<PriorityTransitionRequest>();

            foreach (var item in requestPriorityMapping)
            {
                list.Add(new PriorityTransitionRequest(item.Key, item.Value));
            }
            list.Sort((p1, p2) => p1.HasPriorityOver(p2) ? -1 : 1);
            return list;
        }
    }


    public MovementState(uint id,string name,MovementStateMachine stateMachine):base(id,name)
    {
        this.StateMachine = stateMachine;
        movementOptions = new List<EntityMovementOption>();
        transitions = new List<Transition>();
        requestPriorityMapping = new Dictionary<TransitionRequest, int>();
        minPriorityValue = 0;
        MovementHandler = new StateMovementHandler(this,stateMachine.Player);
        isInitialized = false;
    }

    /// <summary>
    /// fills movement options based on Type Name
    /// </summary>
    /// <param name="optionsTypeName">list of type names of allowed movement options for state</param>
    protected void FillMovementOptions(List<string> optionsTypeName)
    {
        for (int i = 0; i < optionsTypeName.Count; i++)
        {
            var opt = StateMachine.GetMovementOption(optionsTypeName[i]);
            movementOptions.Add(opt);
        }
    }


    /// <summary>
    /// initializes state
    /// </summary>
    /// <param name="movementOptionsForState">list of the allowed movement option type names</param>
    /// <param name="requestPriorities">the data to build it from</param>
    void Initialize(List<string> movementOptionsForState, List<PriorityTransitionRequest.Data> requestPriorities)
    {
        if (isInitialized)
            return;
        InitializeAllowedMovements(movementOptionsForState);
        MovementHandler.Initialize();
        InitializeRequestPriorites(requestPriorities);
        isInitialized = true;
    }

    /// <summary>
    /// initializes state if not already
    /// </summary>
    /// <param name="movementOptionsForState">list of the allowed movement option type names</param>
    void InitializeAllowedMovements(List<string> movementOptionsForState)
    {
        FillMovementOptions(movementOptionsForState);
    }

    /// <summary>
    /// initializes the priority mapping for requests
    /// </summary>
    /// <param name="requestPriorities">the data to build it from</param>
    void InitializeRequestPriorites(List<PriorityTransitionRequest.Data> requestPriorities)
    {
        Debug.Log("TODO");
    }

    /// <summary>
    /// on validate calls movement handler validate
    /// </summary>
    public void OnValidate()
    {
        MovementHandler.OnValidate();
    }

    /// <summary>
    /// starts this state calls movement handler start
    /// </summary>
    /// <param name="prevState">the previous state</param>
    public override void Start(MovementState prevState)
    {
        MovementHandler.Start();
    }

    /// <summary>
    /// Update called by the state machine every unity update cycle
    /// </summary>
    public void Update()
    {
        MovementHandler.Update();
    }

    /// <summary>
    /// fixed update called by state machine every unity fixed update cycle
    /// </summary>
    public void FixedUpdate()
    {
        MovementHandler.FixedUpdate();
    }

    /// <summary>
    /// checks if this state has a transition correponding to this request
    /// </summary>
    /// <param name="request">the request type</param>
    /// <returns>the transition if the state has one, null otherwise</returns>
    public Transition CheckTransitionRequest(TransitionRequest request)
    {
        return transitions.Find(tra => tra.CheckRequest(request));
    }

    /// <summary>
    /// informs state machine of state change request
    /// </summary>
    /// <param name="request">the transition request type</param>
    public void RequestStateChange(TransitionRequest request)
    {
        var tra = CheckTransitionRequest(request);
        if (tra != null)
        {
            StateMachine.EnqueRequestStateChange(GetPriorityTransitionRequest(request),tra);
        }
    }

    /// <summary>
    /// makes a prioriticed request, either from mapping
    /// or if request unmapped it gets the lowest possible priority (less then the least prioriticed mapped request)
    /// </summary>
    /// <param name="request">the request we want to get the priority for</param>
    /// <returns>a prioriticed transition request</returns>
    public PriorityTransitionRequest GetPriorityTransitionRequest(TransitionRequest request)
    {
        //TODO couple request type with priority
        if (requestPriorityMapping.ContainsKey(request))
        {
            return new PriorityTransitionRequest(request, requestPriorityMapping[request]);
        }
        return new PriorityTransitionRequest(request, minPriorityValue);
    }

    /// <summary>
    /// adds a movment option to the state if nor already contained
    /// </summary>
    /// <param name="option">the option being added</param>
    public void AddMovementOption(EntityMovementOption option)
    {
        if (MovementOptions == null)
        {
            movementOptions = new List<EntityMovementOption>();
        }

        if (movementOptions.Contains(option))
            return; //already contained don't double add
        movementOptions.Add(option);

        //add it's requests to the mapping at the end
        var att = Attribute.GetCustomAttribute(option.GetType(), typeof(PossibleTransitionRequestTypesAttribute)) as PossibleTransitionRequestTypesAttribute;
        if (att != null)
        {
            for (int i = 0; i < att.Types.Length; i++)
            {
                var req = TransitionRequest.Factory.BuildRequest(att.Types[i]);

                AddPriorityRequest(req,minPriorityValue);
            }
        }
    }

    /// <summary>
    /// removes a movment option from the state, and all it's associated transitions that it might cause
    /// </summary>
    /// <param name="option">the option being removed</param>
    public void RemoveMovementOption(EntityMovementOption option)
    {
        if (movementOptions.Remove(option))
        {
            PossibleTransitionRequestTypesAttribute att = Attribute.GetCustomAttribute(option.GetType(), typeof(PossibleTransitionRequestTypesAttribute)) as PossibleTransitionRequestTypesAttribute;
            if (att != null)
            {
                for (int i = 0; i < att.Types.Length; i++)
                {
                    var type = TransitionRequest.Factory.BuildRequest(att.Types[i]);

                    RemoveTransition(type);
                    //remove from mapping (relinearize if last one removed)
                    RemovePriorityMapping(type,i==att.Types.Length-1);
                }
            }
        }
    }

    /// <summary>
    /// chcks if state contains movement option
    /// </summary>
    /// <param name="option">the option we want to know if it contains</param>
    /// <returns>true if contained false otherwise</returns>
    public bool ContainsMovementOption(EntityMovementOption option)
    {
        return movementOptions.Contains(option);
    }

    /// <summary>
    /// adds a transition to the state
    /// </summary>
    /// <param name="transition">the transition to add</param>
    public void AddTransition(Transition transition)
    {
        transitions.Add(transition);
    }

    /// <summary>
    /// counts the transitions a state has to anohter state
    /// </summary>
    /// <param name="idxTo">the ID of the state we want to know all the transitions to</param>
    /// <returns>the count of transitions to the state with that ID</returns>
    public int CountTransitionsTo(uint idxTo)
    {
        return transitions.Count(tra => tra.NextStateID == idxTo);
    }

    /// <summary>
    /// get all transitions to a state wiht a certain id,
    /// can potentially exclude one
    /// </summary>
    /// <param name="id">the id of the state we want the transitions to go to</param>
    /// <param name="exclude">a transition we might want to exclude</param>
    /// <returns>a readonly list of transitions to a certian state</returns>
    public IReadOnlyList<Transition> GetTransitionsTo(uint id,Transition exclude = null)
    {
        if (exclude == null)
            return transitions.Where(tra => tra.NextStateID == id).ToList();
        else
            return transitions.Where(tra => (tra.NextStateID == id && tra != exclude)).ToList();
    }

    /// <summary>
    /// returns the index of a transition in the transition list
    /// </summary>
    /// <param name="trans">the transition we want the index for</param>
    /// <returns></returns>
    public int GetTransitionIDX(Transition trans)
    {
        return transitions.IndexOf(trans);
    }

    /// <summary>
    /// removes a transition from the state
    /// </summary>
    /// <param name="transition">the transition to remove</param>
    /// <returns>if it could be removed or not</returns>
    public bool RemoveTransition(Transition transition)
    {
        return transitions.Remove(transition);
    }

    /// <summary>
    /// removes transition based on request type
    /// </summary>
    /// <param name="type">the type of transition to remove</param>
    /// <returns>if it could be removed or not</returns>
    public bool RemoveTransition(TransitionRequest type)
    {
        for (int i = transitions.Count-1; i >= 0; i--)
        {
            if (transitions[i].Type.IsSameRequest(type))
            {
                transitions.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// logs a state to a logger
    /// </summary>
    /// <param name="l">the logger we want to log to</param>
    internal void Print(ILogger l)
    {
        l.Log(Name);
        if (MovementOptions != null)
        {
            for (int i = 0; i < MovementOptions.Count; i++)
            {
                l.Log(MovementOptions[i].Name);
            }
        }
    }

    /// <summary>
    /// removes all transitions based on next state ID
    /// </summary>
    /// <param name="nextStateID">ID of next state</param>
    public void RemoveTransition(uint nextStateID)
    {
        for (int i =transitions.Count-1; i >=0; i--)
        {
            var tran = transitions[i];
            if (tran.NextStateID == nextStateID)
            {
                transitions.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// gets all possible transition requests for a state
    /// </summary>
    /// <returns>list of all possible transition requests for state</returns>
    public List<TransitionRequest> GetPossibleTransitionRequests()
    {
        return StateMachine.GetPossibleTransitionRequests(this);
    }

    /// <summary>
    /// adds a new priority request mapping
    /// </summary>
    /// <param name="request">the new request</param>
    /// <param name="value">the new priority value</param>
    /// <param name="updateIfExisting">if true already existing value is updated</param>
    public void AddPriorityRequest(TransitionRequest request,int value, bool updateIfExisting = true)
    {
        bool change = false;
        if (!requestPriorityMapping.ContainsKey(request))
        {
            //Add new
            change = true;
            requestPriorityMapping.Add(request, value);
        }
        else if (updateIfExisting)
        {
            change = true;
            requestPriorityMapping[request] = value;
        }
        if (change && value >= minPriorityValue)
            minPriorityValue = value + 1;
    }

    /// <summary>
    /// swaps the priority values of two request types
    /// </summary>
    /// <param name="req1">first type</param>
    /// <param name="req2">second type</param>
    public void SwapPriority(TransitionRequest req1, TransitionRequest req2)
    {
        if (requestPriorityMapping.ContainsKey(req1) && requestPriorityMapping.ContainsKey(req2))
        {
            int valueReq1 = requestPriorityMapping[req1];
            requestPriorityMapping[req1] = requestPriorityMapping[req2];
            requestPriorityMapping[req2] = valueReq1;
        }
    }

    /// <summary>
    /// Removes a transition type
    /// </summary>
    /// <param name="type"></param>
    private void RemovePriorityMapping(TransitionRequest type,bool remapPrioritesToLinear = true)
    {
        requestPriorityMapping.Remove(type);
        if (remapPrioritesToLinear)
        {
            RemapTransitionRequestPriority();
        }
    }

    public void RemapTransitionRequestPriority()
    {
        minPriorityValue = 0;
        requestPriorityMapping.Clear();

        var list = GetPossibleTransitionRequests();
        for (int i = 0; i < list.Count; i++)
        {
            TransitionRequest req = list[i];
            AddPriorityRequest(req, i); 
        }
    }

    /// <summary>
    /// data class for movement state serializeable by unity
    /// </summary>
    [Serializable]
    public class Data
    {
        /// <summary>
        /// state ID
        /// </summary>
        [SerializeField] uint id;
        /// <summary>
        /// state ID accessor
        /// </summary>
        public uint ID => id;
        /// <summary>
        /// state name
        /// </summary>
        [SerializeField] string name;
        /// <summary>
        /// state name accesssor
        /// </summary>
        public string Name => name;
        /// <summary>
        /// is state initial state
        /// </summary>
        [SerializeField] bool isInitial;
        /// <summary>
        /// is initial state accessor
        /// </summary>
        public bool IsInitial => isInitial;
        /// <summary>
        /// list of allowed movements type names
        /// </summary>
        [SerializeField] List<string> allowedMovements;
        /// <summary>
        /// readonly accessor of allowed movements
        /// </summary>
        public IReadOnlyList<string> AllowedMovements => allowedMovements;
        /// <summary>
        /// List of all transitions data
        /// </summary>
        [SerializeField] List<Transition.Data> dataTransitions;
        /// <summary>
        /// readonly accessor to transition data list
        /// </summary>
        public IReadOnlyList<Transition.Data> DataTransitions => dataTransitions;

        /// <summary>
        /// list of all transition request and their priority
        /// </summary>
        [SerializeField] List<PriorityTransitionRequest.Data> requestPriorities;

        /// <summary>
        /// readonly accessor to prioriticed request data
        /// </summary>
        public IReadOnlyList<PriorityTransitionRequest.Data> RequestPriorities => requestPriorities;

        public Data(MovementState state)
        {
            id = state.ID;
            name = state.Name;
            isInitial = state.IsInitialState;
            allowedMovements = new List<string>();
            for (int i = 0; i < state.movementOptions.Count; i++)
            {
                allowedMovements.Add(state.movementOptions[i].GetType().Name);
            }

            requestPriorities = new List<PriorityTransitionRequest.Data>();
            var list = state.PrioritizedTransitionRequestList;

            for (int i = 0; i < list.Count; i++)
            {
                var req = list.ElementAt(i);
                requestPriorities.Add(new PriorityTransitionRequest.Data(req));
            }

            dataTransitions = new List<Transition.Data>();
            for (int i = 0; i < state.transitions.Count; i++)
            {
                dataTransitions.Add(new Transition.Data(state.transitions[i]));
            }
        }

        /// <summary>
        /// creates a movement state based on this data, for the state machine
        /// </summary>
        /// <param name="machineFor">the machine that is gonna contain this state</param>
        /// <returns>the created state</returns>
        public MovementState Create(MovementStateMachine machineFor)
        {
            MovementState state = new MovementState(id, name, machineFor);
            state.IsInitialState = isInitial;

            state.Initialize(allowedMovements, requestPriorities);

            for (int i = 0; i < dataTransitions.Count; i++)
            {
                state.AddTransition(dataTransitions[i].Create(machineFor));
            }

            return state;
        }

    }
}

public struct PriorityTransitionRequest
{
    public TransitionRequest Type { get; private set; }
    public int Priority { get; private set; }

    public PriorityTransitionRequest(TransitionRequest type, int priority) : this()
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Priority = priority;
    }

    public bool HasPriorityOver(PriorityTransitionRequest req)
    {
        return Priority < req.Priority;
    }

    public static PriorityTransitionRequest GetRequestWithPriority(PriorityTransitionRequest first, PriorityTransitionRequest second)
    {
        if (first.HasPriorityOver(second))
            return first;
        else
            return second; 
    }

    public static TransitionRequest GetPrioreticedRequest(PriorityTransitionRequest first, PriorityTransitionRequest second)
    {
        return GetRequestWithPriority(first, second).Type;
    }

    [Serializable]
    public struct Data
    {
        [SerializeField] public string TypeName;
        [SerializeField] public int Priority;

        public Data(PriorityTransitionRequest from)
        {
            TypeName = from.Type.GetType().Name;
            Priority = from.Priority;
        }

        public PriorityTransitionRequest Make()
        {
            return new PriorityTransitionRequest(TransitionRequest.Factory.BuildRequest(TypeName), Priority);
        }

    }
}