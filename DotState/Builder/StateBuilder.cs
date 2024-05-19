using DotState.Contracts;
using DotState.Exceptions;

namespace DotState.Builder;

public abstract class StateBuilder<TState, TTrigger> : IStateBuilder<TState, TTrigger>
{
    private readonly StateMachineBuilder<TState, TTrigger> _machineBuilder;
    private readonly IDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> _transitions;
    private Action<TState, TTrigger>? _onEntry = null;
    private Action<TState, TTrigger>? _onExit = null;
    
    public TState State {  get; private set; }
    public TState DefaultState { get; init; }
    public IStateBuilder<TState, TTrigger>? Parent { get; private set; } = null;

    protected StateBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState source, TState defaultState,
        StateBuilder<TState, TTrigger>? parent)
    {
        _machineBuilder = machineBuilder;
        State = source;
        DefaultState = defaultState;
        Parent = parent;
        _transitions = new Dictionary<TTrigger, TransitionBuilder<TState, TTrigger>>();
    }

    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination, Func<TState, TTrigger, bool>? gaurd)
    {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        
        if (_machineBuilder.GetStateBuilder(State) == null) _machineBuilder.RegisterElementState(destination);

        _transitions.TryGetValue(trigger, out var transition);

        if (transition == null)
        {
            transition = new TransitionBuilder<TState, TTrigger>(_machineBuilder, State);
            _transitions.Add(trigger, transition);
        }
        
        if (gaurd == null)
        {
            transition.ToDestination(destination);
        }
        else
        {
            transition.ToDestination(destination, gaurd);
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

    public IStateBuilder<TState, TTrigger> ChildOf(TState parent)
    {
        if (parent == null) throw new ArgumentNullException(nameof(parent));

        if (Parent != null && Parent.State != null && !Parent.State.Equals(parent))
        {
            throw new MultipleParentException<TState>(State, Parent.State, parent);
        }

        var stateBuilder = _machineBuilder.GetStateBuilder(parent)
            ?? _machineBuilder.RegisterElementState(parent);

        Parent = stateBuilder;
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

    internal abstract IStateRepresentation<TState, TTrigger> Build(IStateMachine<TState, TTrigger> stateMachine);
    
    internal abstract void SetupRelations(IStateMachine<TState, TTrigger> stateMachine);
}
