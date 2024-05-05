using DotState.Contracts;
using DotState.Exceptions;

namespace DotState;

internal class Transition<TState, TTrigger> : ITransition<TState, TTrigger>
{
    public TState Source { get; private set; }
    private IDictionary<IStateRepresentation<TState, TTrigger>, Func<TState, TTrigger, bool>> _gaurds;

    public Transition(TState source)
    {
        Source = source;
        _gaurds = new Dictionary<IStateRepresentation<TState, TTrigger>, Func<TState, TTrigger, bool>>();
    }

    public void AddGaurd(Func<TState, TTrigger, bool> gaurd, IStateRepresentation<TState, TTrigger> destination)
    {
        if (destination == null) { throw new ArgumentNullException(nameof(destination)); }

        if (!_gaurds.TryAdd(destination, gaurd))
        {
            throw new InvalidOperationException($"A gaurd to {destination} already exists");
        }
    }

    public IStateRepresentation<TState, TTrigger>? GetDestination(TState currentState, TTrigger currentTrigger)
    {
        var selectedGaurd = _gaurds.Where(gaurd => gaurd.Value.Invoke(currentState, currentTrigger));

        if (selectedGaurd.Count() > 1)
        {
            var innerException = new Exception($"Multiple states are possible from {currentState} on {currentTrigger}");
            throw new InvalidTransitionException<TState, TTrigger>(currentState, currentTrigger, innerException);
        }

        return selectedGaurd.Select(gaurd => gaurd.Key).FirstOrDefault();
    }
}
