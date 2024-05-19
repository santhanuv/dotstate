using DotState.Contracts;

namespace DotState.Builder;

public class TransitionBuilder<TState, TTrigger> : ITransitionBuilder<TState, TTrigger>
{
    public TState Source {  get; private set; }
    private readonly IList<TState> _noGaurdDestinations;
    private readonly IDictionary<TState, IList<Func<TState, TTrigger, bool>>> _destinationGaurds;
    private readonly StateMachineBuilder<TState, TTrigger> _machineBuilder;

    internal TransitionBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState source)
    {
        _machineBuilder = machineBuilder;
        Source = source;
        _destinationGaurds = new Dictionary<TState, IList<Func<TState, TTrigger, bool>>>();
        _noGaurdDestinations = new List<TState>();
    }

    public void ToDestination(TState destination)
    {
        if (destination == null) throw new ArgumentNullException(nameof(destination));
        
        if (_noGaurdDestinations.Contains(destination))
        {
            throw new InvalidOperationException($"A transition to {destination} without any gaurd already exists");
        }

        _destinationGaurds.TryGetValue(destination, out var gaurdList);

        if (gaurdList == null)
        {
            gaurdList = new List<Func<TState, TTrigger, bool>>();
            _destinationGaurds.TryAdd(destination, gaurdList);
        }

        gaurdList.Add((_, _) => true);

        _noGaurdDestinations.Add(destination);

        if (_machineBuilder.GetStateBuilder(destination) == null)
        {
            _machineBuilder.RegisterElementState(destination);
        }
    }

    public void ToDestination(TState destination, Func<TState, TTrigger, bool> gaurd)
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

        if (_machineBuilder.GetStateBuilder(destination) == null)
        {
            _machineBuilder.RegisterElementState(destination);
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
