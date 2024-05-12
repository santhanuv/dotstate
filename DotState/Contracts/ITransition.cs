namespace DotState.Contracts;

internal interface ITransition<TState, TTrigger>
{
    public IDictionary<IStateRepresentation<TState, TTrigger>, IList<Func<TState, TTrigger, bool>>> DestinationGaurds { get; set; }

    public IStateRepresentation<TState, TTrigger>? GetDestination(TState currentState, TTrigger currentTrigger);
}
