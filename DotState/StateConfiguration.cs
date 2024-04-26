using DotState.Contracts;

namespace dotstate;

public class StateConfiguration<TState, TTrigger> : IStateConfiguration<TState, TTrigger>
    where TState : notnull where TTrigger : notnull
{
    private readonly TState _state;
    private readonly Dictionary<TTrigger, Transition<TState, TTrigger>> transitions;

    public StateConfiguration(TState state)
    {
        transitions = new();
        _state = state;
    }

    public void AddTransition(TTrigger trigger, IStateConfiguration<TState, TTrigger> destination, Func<TState, bool>? predicate = null)
    {
        Transition<TState, TTrigger> transition = predicate == null ? new(destination) : new(destination, predicate);
        transitions.Add(trigger, transition);
    }

    public Transition<TState, TTrigger>? GetTransition(TTrigger trigger)
    {
        transitions.TryGetValue(trigger, out var transition);
        return transition;
    }

    public TState GetState()
    {
        return _state;
    }
}
