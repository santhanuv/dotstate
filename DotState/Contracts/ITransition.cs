namespace DotState.Contracts;

internal interface ITransition<TState, TTrigger>
{
    public void AddGaurd(Func<TState, TTrigger, bool> gaurd, IStateRepresentation<TState, TTrigger> destination);
    public IStateRepresentation<TState, TTrigger>? GetDestination(TState currentState, TTrigger currentTrigger);
}
