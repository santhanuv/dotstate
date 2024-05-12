using DotState.Contracts;

namespace DotState;

internal class StateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>
{
    private readonly IDictionary<TState, IStateRepresentation<TState, TTrigger>> _stateRepresentations;
    internal StateMachine()
    {
        _stateRepresentations = new Dictionary<TState, IStateRepresentation<TState, TTrigger>>();
    }

    public IStateRepresentation<TState, TTrigger>? GetStateRepresentation(TState state)
    {
        _stateRepresentations.TryGetValue(state, out var stateRep);
        return stateRep;
    }

    public void RegisterState(TState state, IStateRepresentation<TState, TTrigger> stateRepresentation)
    {
        _stateRepresentations.TryAdd(state, stateRepresentation);
    }
}
