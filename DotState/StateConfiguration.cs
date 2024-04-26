using DotState.Contracts;

namespace DotState;

internal class StateConfiguration<TState, TTrigger> : IStateConfiguration<TState, TTrigger>
    where TState : notnull where TTrigger : notnull
{
    private readonly IStateConfigurationFactory<TState, TTrigger> _factory;
    private readonly TState _state;
    private readonly Dictionary<TTrigger, Transition<TState, TTrigger>> transitions;

    public StateConfiguration(TState state)
    {
        transitions = new();
        _state = state;
        _factory = new StateConfigurationFactory<TState, TTrigger>();
    }

    public void AddTransition(TTrigger trigger, Transition<TState, TTrigger> transition)
    {
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
