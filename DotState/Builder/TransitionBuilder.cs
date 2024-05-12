using DotState.Contracts;

namespace DotState.Builder;

public class TransitionBuilder<TState, TTrigger> : ITransitionBuilder<TState, TTrigger>
{
    public TState Source {  get; private set; }
    private readonly IDictionary<TState, IList<Func<TState, TTrigger, bool>>> _destinationGaurds;

    internal TransitionBuilder(TState source)
    {
        Source = source;
        _destinationGaurds = new Dictionary<TState, IList<Func<TState, TTrigger, bool>>>();
    }

    internal void AddDestination(TState destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        var gaurdList = new List<Func<TState, TTrigger, bool>>();

        if(!_destinationGaurds.TryAdd(destination, gaurdList))
        {
            throw new InvalidOperationException($"A transition to {destination} already exists");
        }
    }

    public void AddGaurd(Func<TState, TTrigger, bool> gaurd, TState destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        if (gaurd == null) throw new ArgumentNullException(nameof(gaurd));

        _destinationGaurds.TryGetValue(destination, out var gaurdList);
        
        if (gaurdList == null)
        {
            gaurdList = new List<Func<TState, TTrigger, bool>>() { gaurd };
            _destinationGaurds.TryAdd(destination, gaurdList);
        }
        else
        {
            gaurdList.Add(gaurd);
        }
    }

    internal ITransition<TState, TTrigger> Build(IStateMachine<TState, TTrigger> stateMachine)
    {
        var transitions = new Dictionary<IStateRepresentation<TState, TTrigger>,  IList<Func<TState, TTrigger, bool>>>();

        foreach (var destinationGaurd in _destinationGaurds)
        {
            var destinationStateRep = stateMachine.GetStateRepresentation(destinationGaurd.Key) ??
                throw new InvalidOperationException($"No configuration available for {destinationGaurd.Key}");

            transitions.Add(destinationStateRep, destinationGaurd.Value);
        }

        return new Transition<TState, TTrigger>(Source, transitions);
    }
}
