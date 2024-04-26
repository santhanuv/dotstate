using DotState.Contracts;

namespace DotState;

internal class StateConfigurationFactory<TState, TTrigger> : IStateConfigurationFactory<TState, TTrigger>
    where TState : notnull where TTrigger : notnull
{
    public IStateConfiguration<TState, TTrigger> CreateStateConfiguration(TState state)
    {
        return new StateConfiguration<TState, TTrigger>(state);
    }

    public Transition<TState, TTrigger> CreateTransition(IStateConfiguration<TState, TTrigger> destination)
    {
        return new Transition<TState, TTrigger>(destination);
    }
}
