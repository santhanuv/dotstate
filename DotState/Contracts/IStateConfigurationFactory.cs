namespace DotState.Contracts;

internal interface IStateConfigurationFactory<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public IStateConfiguration<TState, TTrigger> CreateStateConfiguration(TState state);
    public Transition<TState, TTrigger> CreateTransition(IStateConfiguration<TState, TTrigger> destination);
}
