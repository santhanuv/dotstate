using DotState.Contracts;

namespace DotState.Builder;

public class StateMachineBuilder<TState, TTrigger> : IStateMachineBuilder<TState, TTrigger>
{
    private readonly IDictionary<TState, StateBuilder<TState, TTrigger>> _stateBuilders;

    public StateMachineBuilder()
    {
        _stateBuilders = new Dictionary<TState, StateBuilder<TState, TTrigger>>();
    }

    public IStateMachine<TState, TTrigger> Build()
    {
        var stateMachine = new StateMachine<TState, TTrigger>();
        foreach (var stateBuilder in _stateBuilders.Values)
        {
            stateBuilder.Build(stateMachine);
        }

        foreach (var builder in _stateBuilders.Values)
        {
            builder.SetupRelations(stateMachine);
        }

        return stateMachine;
    }

    public IStateBuilder<TState, TTrigger> CompositeState(TState state, TState defaultState)
    {
        var stateBuilder = GetStateBuilder(state);

        return stateBuilder
            ?? RegisterCompositeState(state, defaultState);
    }

    public IStateBuilder<TState, TTrigger> ElementState(TState state)
    {
        return GetStateBuilder(state) ?? RegisterElementState(state);
    }

    public IStateBuilder<TState, TTrigger>? GetStateBuilder(TState state)
    {
        _stateBuilders.TryGetValue(state, out var builder);
        return builder;
    }

    internal CompositeStateBuilder<TState, TTrigger> RegisterCompositeState(TState state, TState defaultState)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        IStateBuilder<TState, TTrigger>? stateBuilder = GetStateBuilder(state);

        if (stateBuilder != null) throw new InvalidOperationException($"State \"{state}\" has been registered already.");
        

        var compositeStateBuilder = new CompositeStateBuilder<TState, TTrigger>(this, state, defaultState);
        _stateBuilders.TryAdd(state, compositeStateBuilder);
        return compositeStateBuilder;

    }

    internal StateBuilder<TState, TTrigger> RegisterElementState(TState state) 
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        var elementState = new ElementStateBuilder<TState, TTrigger>(this, state);
        var hasRegistered = _stateBuilders.TryAdd(state, elementState);
        
        if (!hasRegistered) throw new InvalidOperationException($"State \"{state}\" has been registered already.");

        return elementState;
    }
}
