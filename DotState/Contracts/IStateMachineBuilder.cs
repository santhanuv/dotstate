namespace DotState.Contracts;

public interface IStateMachineBuilder<TState, TTrigger>
{
    public IStateBuilder<TState, TTrigger> Configure(TState state);
    public IStateBuilder<TState, TTrigger>? GetStateBuilder(TState state);
    public IStateMachine<TState, TTrigger> Build();
}
