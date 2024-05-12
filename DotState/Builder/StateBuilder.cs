using DotState.Contracts;

namespace DotState.Builder;

public class StateBuilder<TState, TTrigger> : IStateBuilder<TState, TTrigger>
{
    private readonly StateMachineBuilder<TState, TTrigger> _machineBuilder;
    private readonly IDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> _transitions;
    private Action<TState, TTrigger>? _onEntry = null;
    private Action<TState, TTrigger>? _onExit = null;
    private readonly IList<StateBuilder<TState, TTrigger>> _children;
    
    public TState State {  get; private set; }
    // Default state is the null state for enum & null for class state
    public StateBuilder<TState, TTrigger>? Parent { get; private set; } = null;

    internal StateBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState source) 
        : this(machineBuilder, source, null, null) {}

    internal StateBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState source, 
        StateBuilder<TState, TTrigger>? parent, IList<StateBuilder<TState, TTrigger>>? children)
    {
        _machineBuilder = machineBuilder;
        State = source;
        Parent = parent;
        _children = children ?? new List<StateBuilder<TState, TTrigger>>();
        _transitions = new Dictionary<TTrigger, TransitionBuilder<TState, TTrigger>>();
    }

    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination, Func<TState, TTrigger, bool>? gaurd)
    {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        _machineBuilder.GetOrRegisterState(destination);

        _transitions.TryGetValue(trigger, out var transition);

        if (transition == null)
        {
            transition = new TransitionBuilder<TState, TTrigger>(State);
            _transitions.Add(trigger, transition);
        }
        
        if (gaurd == null)
        {
            transition.AddDestination(destination);
        }
        else
        {
            transition.AddGaurd(gaurd, destination);
        }

        return this;
    }

    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination)
    {
        return AddTransition(trigger, destination, null);
    }

    public IStateBuilder<TState, TTrigger> IgnoreTrigger(TTrigger trigger)
    {
        return AddTransition(trigger, State, null);
    }

    public IStateBuilder<TState, TTrigger> IgnoreTrigger(TTrigger trigger, Func<TState, TTrigger, bool>? gaurd)
    {
        return AddTransition(trigger, State, gaurd);
    }

    public IStateBuilder<TState, TTrigger> SubStateOf(TState parent)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));

        Parent = _machineBuilder.GetOrRegisterState(parent);
        return this;
    }

    public IStateBuilder<TState, TTrigger> OnEntry(Action<TState, TTrigger> action)
    {
        _onEntry = action;
        return this;
    }

    public IStateBuilder<TState, TTrigger> OnExit(Action<TState, TTrigger> action)
    {
        _onExit = action;
        return this;
    }

    internal void SuperStateOf(TState child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));

        if (!_children.Any(c => child.Equals(c.State)))
        {
            _children.Add(_machineBuilder.GetOrRegisterState(child));
        }
        else throw new InvalidOperationException($"Child state \"{child}\" already configured");
    }

    internal Action<TState, TTrigger>? GetEntryAction()
    {
        return _onEntry;
    }

    internal Action<TState, TTrigger>? GetExitAction()
    {
        return _onExit;
    }

    internal IDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> GetAllTransitions()
    {
        return _transitions;
    }

    internal void SetupStateRelations(IStateMachine<TState, TTrigger> stateMachine)
    {
        var stateRep = stateMachine.GetStateRepresentation(State) ??
            throw new InvalidOperationException($"Unexpected error when building configuration for {State}");

        var parentStateRep = Parent != null ? stateMachine.GetStateRepresentation(Parent.State) : null;
        parentStateRep?.AddChild(stateRep);

        stateRep.Transitions = _transitions.ToDictionary(t => t.Key, t => t.Value.Build(stateMachine));
    }
}
