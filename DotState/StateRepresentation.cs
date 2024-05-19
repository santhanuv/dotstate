using DotState.Contracts;

namespace DotState;

internal class StateRepresentation<TState, TTrigger> : IStateRepresentation<TState, TTrigger>
{
    private readonly IStateMachine<TState, TTrigger> _machine;
    public TState State { get; init; }
    public TState DefaultState {  get; init; }
    public IStateRepresentation<TState, TTrigger>? Parent { get; set; } = null!;
    public IDictionary<TTrigger, ITransition<TState, TTrigger>> Transitions { get; set; }

    public Action<TState, TTrigger>? OnEntry { get; set; } = null;
    public Action<TState, TTrigger>? OnExit { get; set; } = null;

    public StateRepresentation(IStateMachine<TState, TTrigger> machine, TState state, TState defaultState, Action<TState, TTrigger>? onEntry, Action<TState, TTrigger>? onExit)
        :this(machine, state, defaultState, onEntry, onExit, null) {}

    public StateRepresentation(IStateMachine<TState, TTrigger> machine,TState state, TState defaultState, Action<TState, TTrigger>? onEntry, Action<TState, TTrigger>? onExit,
        IDictionary<TTrigger, ITransition<TState, TTrigger>>? transitions)
    {
        _machine = machine;
        State = state;
        DefaultState = defaultState;
        OnEntry = onEntry;
        OnExit = onExit;
        Transitions = transitions ?? new Dictionary<TTrigger, ITransition<TState, TTrigger>>();
    }

    public ITransition<TState, TTrigger>? GetTransition(TTrigger trigger)
    {
        Transitions.TryGetValue(trigger, out var transition);
        return transition;
    }

    public IStateRepresentation<TState, TTrigger> GetDefaultStateRepresentation()
    {
        return _machine.GetStateRepresentation(DefaultState) 
            ?? throw new Exception($"No configuration available for State \"{DefaultState}\".");
    }
}
