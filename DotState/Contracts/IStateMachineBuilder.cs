namespace DotState.Contracts;

public interface IStateMachineBuilder<TState, TTrigger>
{
    public IStateBuilder<TState, TTrigger> CompositeState(TState state, TState defaultState);
    public IStateBuilder<TState, TTrigger> ElementState(TState state);
    public IStateBuilder<TState, TTrigger>? GetStateBuilder(TState state);
    public IStateMachine<TState, TTrigger> Build();
}
