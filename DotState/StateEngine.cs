using DotState.Contracts;
using DotState.Exceptions;

namespace DotState;

public class StateEngine<TState, TTrigger>
{
    private readonly IStateMachine<TState, TTrigger> _machine;
    private IStateRepresentation<TState, TTrigger> _currentState;
    public TState CurrentState { get { return _currentState.State; } }
    public bool IgnoreInvalidTriggers { get; set; } = false;

    public StateEngine(IStateMachine<TState, TTrigger> machine, TState initialState)
    {
        _machine = machine;
        var stateRep = _machine.GetStateRepresentation(initialState) ?? throw new Exception($"State \"{initialState}\" is not registered");
        this._currentState = stateRep;
    }

    public TState ExecuteTransition(TTrigger trigger)
    {
        var state = _currentState.State ?? throw new NullReferenceException($"Unexpected transition from invalid state");
        var transition = _currentState.GetTransition(trigger);
        var nextStateRep = transition?.GetDestination(state, trigger); 
        
        if (transition == null || nextStateRep == null)
        {
            if (IgnoreInvalidTriggers) return state; 
            else throw new InvalidTransitionException<TState, TTrigger>(state, trigger);
        }

        _currentState = nextStateRep;
        var nextState = _currentState.State;

        if (!state.Equals(nextState))
        {
            _currentState.OnExit?.Invoke(state, trigger);
            _currentState.OnEntry?.Invoke(nextState, trigger);

        }

        return nextState;
    }
}
