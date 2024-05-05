using DotState.Contracts;

namespace DotState;

public class StateMachine<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    private readonly Dictionary<TState, IStateConfiguration<TState, TTrigger>> _stateConfigs;
    private readonly IStateConfigurationFactory<TState, TTrigger> _factory;

    public StateMachine()
    {
        _stateConfigs = new();
        _factory = new StateConfigurationFactory<TState, TTrigger>();
    }

    public StateMachine<TState,TTrigger> Register(TState source, TTrigger trigger, TState destination)
    {
        var sourceConfig = GetStateConfiguration(source);
        var destinationConfig = GetStateConfiguration(destination);
        
        if (sourceConfig == null)
        {
            sourceConfig = _factory.CreateStateConfiguration(source);
            _stateConfigs[source] = sourceConfig;
        }

        if (destinationConfig == null)
        {
            destinationConfig = _factory.CreateStateConfiguration(destination);
            _stateConfigs[destination] = destinationConfig;
        }

        var transition = _factory.CreateTransition(destinationConfig);
        sourceConfig.AddTransition(trigger, transition);
        
        return this;
    }

    public StateMachine<TState, TTrigger> OnEntry(TState state, Action<TState, TTrigger> action)
    {
        var stateConfig = GetStateConfiguration(state);

        if (stateConfig != null)
        {
            stateConfig.OnEntry = action;
        }
        else
        {
            throw new InvalidOperationException($"State \"{state}\" is not registered");
        }

        return this;
    }

    public StateMachine<TState, TTrigger> OnExit(TState state, Action<TState, TTrigger> action)
    {
        var stateConfig = GetStateConfiguration(state);

        if (stateConfig != null)
        {
            stateConfig.OnExit = action;
        }
        else
        {
            throw new InvalidOperationException($"State \"{state}\" is not registered");
        }

        return this;
    }

    internal IStateConfiguration<TState, TTrigger>? GetStateConfiguration(TState state)
    {
        return _stateConfigs.TryGetValue(state, out var configuration) ? configuration : null;
    }

    public bool IsRegisteredState(TState state)
    {
        return _stateConfigs.ContainsKey(state);
    }
}
