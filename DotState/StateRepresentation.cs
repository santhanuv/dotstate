using DotState.Contracts;

namespace DotState;

internal class StateRepresentation<TState, TTrigger> : IStateRepresentation<TState, TTrigger>
{
    public TState State { get; init; }
    public IStateRepresentation<TState, TTrigger>? Parent { get; set; } = null!;
    public IList<IStateRepresentation<TState, TTrigger>> Children { get; } = new List<IStateRepresentation<TState, TTrigger>>();
    public IDictionary<TTrigger, ITransition<TState, TTrigger>> Transitions { get; set; }

    public Action<TState, TTrigger>? OnEntry { get; set; } = null;
    public Action<TState, TTrigger>? OnExit { get; set; } = null;

    public StateRepresentation(TState state, Action<TState, TTrigger>? onEntry, Action<TState, TTrigger>? onExit)
        :this(state, onEntry, onExit, null) {}

    public StateRepresentation(TState state, Action<TState, TTrigger>? onEntry, Action<TState, TTrigger>? onExit,
        IDictionary<TTrigger, ITransition<TState, TTrigger>>? transitions)
    {
        State = state;
        OnEntry = onEntry;
        OnExit = onExit;
        Transitions = transitions ?? new Dictionary<TTrigger, ITransition<TState, TTrigger>>();
    }

    public ITransition<TState, TTrigger>? GetTransition(TTrigger trigger)
    {
        Transitions.TryGetValue(trigger, out var transition);
        return transition;
    }

    public IList<IStateRepresentation<TState, TTrigger>> AddChild(IStateRepresentation<TState, TTrigger> stateRepresentation)
    {
        if(!Children.Contains(stateRepresentation))
        {
            Children.Add(stateRepresentation);
        }

        return Children;
    }
}
