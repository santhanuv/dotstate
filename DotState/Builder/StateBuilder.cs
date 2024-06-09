using DotState.Contracts;
using DotState.Exceptions;

namespace DotState.Builder;

/// <summary>
/// An abstract parent builder class for states.
/// 
/// <see cref="CompositeStateBuilder{TState, TTrigger}"/>
/// <see cref="ElementStateBuilder{TState, TTrigger}"/>
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TTrigger"></typeparam>
public abstract class StateBuilder<TState, TTrigger> : IStateBuilder<TState, TTrigger>
{
    private readonly StateMachineBuilder<TState, TTrigger> _machineBuilder;
    private readonly IDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> _transitions;
    private Action<TState, TTrigger>? _onEntry = null;
    private Action<TState, TTrigger>? _onExit = null;

    /// <summary>
    /// The state associated with the builder.
    /// </summary>
    public TState State {  get; private set; }

    /// <summary>
    /// The default state.
    /// </summary>
    /// <value>For element state, <c>DefaultState</c> is same as <see cref="State"/></value>
    public TState DefaultState { get; init; }

    /// <summary>
    /// The parent state.
    /// </summary>
    /// <value>Null if state doesn't have a parent state. By default it is set to Null.</value>
    public IStateBuilder<TState, TTrigger>? Parent { get; private set; } = null;

    /// <summary>
    /// Initializes an instance of Class <c>StateBuilder</c>
    /// </summary>
    /// <param name="machineBuilder">An instance of Class <c>StateMachineBuilder</c> associated with the state builder.</param>
    /// <param name="state">The state associated with the builder.</param>
    /// <param name="defaultState">The default state. For element state, this should be same as Param <c>state</c></param>
    /// <param name="parent">The parent state. Default value is Null.</param>
    protected StateBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState state, TState defaultState, IStateBuilder<TState, TTrigger>? parent)
        :this(machineBuilder, state, defaultState, parent, null) {}

    /// <summary>
    /// Initializes an instance of Class <c>StateBuilder</c>
    /// </summary>
    /// <param name="machineBuilder">An instance of Class <c>StateMachineBuilder</c> associated with the state builder.</param>
    /// <param name="state">The state associated with the builder.</param>
    /// <param name="defaultState">The default state. For element state, this should be same as Param <c>state</c></param>
    /// <param name="parent">The parent state.Default value is Null.</param>
    /// <param name="transitions">
    /// A collection containing the mapping between triggers and it's associated <c>TransitionBuilder</c>.
    /// </param>
    protected StateBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState state, TState defaultState,
    IStateBuilder<TState, TTrigger>? parent, IDictionary<TTrigger, TransitionBuilder<TState, TTrigger>>? transitions)
    {
        _machineBuilder = machineBuilder;
        State = state;
        DefaultState = defaultState;
        Parent = parent;
        _transitions = transitions ?? new Dictionary<TTrigger, TransitionBuilder<TState, TTrigger>>();
    }

    /// <summary>
    /// Adds a new transition from <c>State</c> to another.
    /// </summary>
    /// <param name="trigger">The trigger on which the transition should be initiated.</param>
    /// <param name="destination">The target state of the transition.</param>
    /// <param name="gaurd">An optional garud condition. Activates transition only if the garud evaluates to true.</param>
    /// <returns>The state builder with the new transition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either trigger or destination is Null.</exception>
    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination, Func<TState, TTrigger, bool>? gaurd)
    {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        
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

        if (_machineBuilder.GetStateBuilder(destination) == null) _machineBuilder.RegisterElementState(destination);

        return this;
    }

    /// <summary>
    /// Adds a new transition from <c>State</c> to another.
    /// </summary>
    /// <param name="trigger">The trigger on which the transition should be initiated.</param>
    /// <param name="destination">The target state of the transition.</param>
    /// <returns>The state builder with the new transition.</returns>
    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination)
    {
        return AddTransition(trigger, destination, null);
    }

    /// <summary>
    /// Ignores transitions on the specified trigger.
    /// </summary>
    /// <param name="trigger">The trigger to ignore.</param>
    /// <returns>The state builder.</returns>
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

    internal abstract IStateRepresentation<TState, TTrigger> Build(IStateMachine<TState, TTrigger> stateMachine);
    
    internal abstract void SetupRelations(IStateMachine<TState, TTrigger> stateMachine);
    
    internal IReadOnlyDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> GetAllTransitions()
    {
        return (IReadOnlyDictionary<TTrigger, TransitionBuilder<TState, TTrigger>>)_transitions;
    }

    IReadOnlyDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> IStateBuilder<TState, TTrigger>.GetAllTransitions()
    {
        return GetAllTransitions();
    }
}
