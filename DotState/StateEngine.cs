using DotState.Contracts;

namespace DotState;

public class StateEngine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    private readonly StateMachine<TState, TTrigger> _machine;
    private IStateConfiguration<TState, TTrigger> _stateConfig;

    public StateEngine(StateMachine<TState, TTrigger> machine, TState initialState)
    {
        _machine = machine;
        var stateConfig = _machine.GetStateConfiguration(initialState);

        if (stateConfig == null)
        {
            throw new Exception($"{initialState} is not registered");
        }

        _stateConfig = stateConfig;
    }

    public bool ExecuteTransition(TTrigger trigger)
    {
        var status = false;

        var transition = _stateConfig.GetTransition(trigger);
        var state = _stateConfig.GetState();

        var newStateConfig = transition?.ExecuteTransition(state);
        if (newStateConfig != null)
        {
            _stateConfig = newStateConfig;
            status = true;
        }
        
        return status;
    }

    public TState GetCurrentState()
    {
        return _stateConfig.GetState();
    }
}
