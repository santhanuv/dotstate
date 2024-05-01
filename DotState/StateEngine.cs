using DotState.Contracts;
using DotState.Exceptions;

namespace DotState;

public class StateEngine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    private readonly StateMachine<TState, TTrigger> _machine;
    private IStateConfiguration<TState, TTrigger> _stateConfig;

    public StateEngine(StateMachine<TState, TTrigger> machine, TState initialState, TTrigger initialTrigger)
    {
        _machine = machine;
        var stateConfig = _machine.GetStateConfiguration(initialState) ?? throw new Exception($"State \"{initialState}\" is not registered");
        _stateConfig = stateConfig;
        _stateConfig.OnEntry?.Invoke(initialState, initialTrigger);
    }

    public TState ExecuteTransition(TTrigger trigger)
    {
        var transition = _stateConfig.GetTransition(trigger);
        var state = _stateConfig.GetState();

        _stateConfig.OnExit?.Invoke(state, trigger);

        var newStateConfig = transition?.ExecuteTransition(state);

        if (newStateConfig != null)
        {
            _stateConfig = newStateConfig;
        }
        else
        {
            throw new InvalidTransitionException<TState, TTrigger>(state, trigger);
        }

        _stateConfig.OnEntry?.Invoke(state, trigger);
        return _stateConfig.GetState();
    }

    public TState GetCurrentState()
    {
        return _stateConfig.GetState();
    }
}
