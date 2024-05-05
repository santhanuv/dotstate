namespace DotState.Contracts;

public interface IStateMachine<TState, TTrigger>
{
    internal IStateRepresentation<TState, TTrigger>? GetStateRepresentation(TState state);
    internal void RegisterState(TState state, IStateRepresentation<TState, TTrigger> stateRepresentation);
}
