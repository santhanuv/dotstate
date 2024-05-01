using DotState.Contracts;

namespace DotState;

internal class StateConfiguration<TState, TTrigger> : IStateConfiguration<TState, TTrigger>
    where TState : notnull where TTrigger : notnull
{
    private readonly TState _state;
    private readonly Dictionary<TTrigger, Transition<TState, TTrigger>> _transitions;

    public Action<TState, TTrigger>? OnEntry { get; set; } = null;
    public Action<TState, TTrigger>? OnExit { get; set; } = null;

    public StateConfiguration(TState state)
    {
        _transitions = new();
        _state = state;
    }

    public void AddTransition(TTrigger trigger, Transition<TState, TTrigger> transition)
    {
        _transitions.Add(trigger, transition);
    }

    public Transition<TState, TTrigger>? GetTransition(TTrigger trigger)
    {
        _transitions.TryGetValue(trigger, out var transition);
        return transition;
    }

    public TState GetState()
    {
        return _state;
    }
}
