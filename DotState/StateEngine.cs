using DotState.Contracts;
using DotState.Exceptions;

namespace DotState;

public class StateEngine<TState, TTrigger>
{
    private readonly IStateMachine<TState, TTrigger> _machine;
    private IStateRepresentation<TState, TTrigger> _stateRep;

    public StateEngine(IStateMachine<TState, TTrigger> machine, TState initialState)
    {
        _machine = machine;
        var stateRep = _machine.GetStateRepresentation(initialState) ?? throw new Exception($"State \"{initialState}\" is not registered");
        this._stateRep = stateRep;
    }

    public TState ExecuteTransition(TTrigger trigger)
    {
        var state = _stateRep.State;
        var transition = _stateRep.GetTransition(trigger) ?? throw new InvalidTransitionException<TState, TTrigger>(state, trigger);
        var nextStateRep = transition.GetDestination(state, trigger) ?? throw new InvalidTransitionException<TState, TTrigger>(state, trigger); 

        _stateRep = nextStateRep;

        var nextState = _stateRep.State;

        _stateRep.OnExit?.Invoke(nextState, trigger);
        _stateRep.OnEntry?.Invoke(nextState, trigger);

        return nextState;
    }

    public TState GetCurrentState()
    {
        return _stateRep.State;
    }
}
