using DotState.Contracts;
using DotState.Exceptions;

namespace DotState;

internal class Transition<TState, TTrigger> : ITransition<TState, TTrigger>
{
    public TState Source { get; private set; }
    public IDictionary<IStateRepresentation<TState, TTrigger>, IList<Func<TState, TTrigger, bool>>> DestinationGaurds { get; set; }

    public Transition(TState source, IDictionary<IStateRepresentation<TState, TTrigger>, IList<Func<TState, TTrigger, bool>>> destinationGaurds)
    {
       Source = source;
       DestinationGaurds = destinationGaurds;
    }

    public IStateRepresentation<TState, TTrigger>? GetDestination(TState currentState, TTrigger currentTrigger)
    {
        var selectedGaurd = DestinationGaurds
            .Where(dg => dg.Value.All(gl => gl.Invoke(currentState, currentTrigger)));

        if (selectedGaurd.Count() > 1)
        {
            var innerException = new Exception($"Multiple states are possible from {currentState} on {currentTrigger}");
            throw new InvalidTransitionException<TState, TTrigger>(currentState, currentTrigger, innerException);
        }

        return selectedGaurd.Select(gaurd => gaurd.Key).FirstOrDefault();
    }
}
