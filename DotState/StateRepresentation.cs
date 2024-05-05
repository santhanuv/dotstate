using DotState.Contracts;

namespace DotState;

internal class StateRepresentation<TState, TTrigger> : IStateRepresentation<TState, TTrigger>
{
    public TState State { get; init; }
    public IStateRepresentation<TState, TTrigger>? Parent { get; set; } = null!;
    public IList<IStateRepresentation<TState, TTrigger>> Children { get; } = new List<IStateRepresentation<TState, TTrigger>>();
    private readonly Dictionary<TTrigger, Transition<TState, TTrigger>> _transitions;

    public Action<TState, TTrigger>? OnEntry { get; set; } = null;
    public Action<TState, TTrigger>? OnExit { get; set; } = null;

    public StateRepresentation(
        TState state,
        Action<TState, TTrigger>? onEntry,
        Action<TState, TTrigger>? onExit
        )
    {
        _transitions = new();
        State = state;
        OnEntry = onEntry;
        OnExit = onExit;
    }

    internal void AddTransition(TTrigger trigger, Transition<TState, TTrigger> transition)
    {
        if (!_transitions.TryAdd(trigger, transition))
        {
            throw new InvalidOperationException($"Transition for {trigger} already exists.");
        }
    }

    public ITransition<TState, TTrigger>? GetTransition(TTrigger trigger)
    {
        _transitions.TryGetValue(trigger, out var transition);
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
