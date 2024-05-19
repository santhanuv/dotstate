using DotState.Contracts;
using DotState.Exceptions;

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
            if (stateBuilder.GetType() == typeof(CompositeStateBuilder<TState, TTrigger>))
            {
                Console.WriteLine("remove");
            }
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

        if (stateBuilder != null)
        {
            if (stateBuilder.GetType() != typeof(CompositeStateBuilder<TState, TTrigger>))
            {
                return RegisterCompositeState(stateBuilder, defaultState);
            }

            if (stateBuilder.DefaultState != null && !stateBuilder.DefaultState.Equals(defaultState))
                throw new MultipleDefaultStateException<TState>(state, stateBuilder.DefaultState, defaultState);
        }

        return stateBuilder ?? RegisterCompositeState(state, defaultState);
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
        if (defaultState == null) throw new ArgumentNullException(nameof(defaultState));

        var compositeStateBuilder = new CompositeStateBuilder<TState, TTrigger>(this, state, defaultState);
        var hasRegistered = _stateBuilders.TryAdd(state, compositeStateBuilder);

        if (!hasRegistered) throw new StateConfigurationException<TState>(state, $"State \"{state}\" has been registered already.");

        if (GetStateBuilder(defaultState) == null)
        {
            RegisterElementState(defaultState);
        }

        return compositeStateBuilder;
    }

    private CompositeStateBuilder<TState, TTrigger> RegisterCompositeState(IStateBuilder<TState, TTrigger> stateBuilder, TState defaultState)
    {
        if (stateBuilder == null) throw new ArgumentNullException(nameof(stateBuilder));
        if (defaultState == null) throw new ArgumentNullException(nameof(defaultState));

        var compositeStateBuilder = new CompositeStateBuilder<TState, TTrigger>(this, stateBuilder, defaultState);
        _stateBuilders[stateBuilder.State] = compositeStateBuilder;

        if (GetStateBuilder(defaultState) == null)
        {
            RegisterElementState(defaultState);
        }

        return compositeStateBuilder;
    }

    internal StateBuilder<TState, TTrigger> RegisterElementState(TState state) 
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        var elementStateBuilder = new ElementStateBuilder<TState, TTrigger>(this, state);
        var hasRegistered = _stateBuilders.TryAdd(state, elementStateBuilder);
        
        if (!hasRegistered) throw new StateConfigurationException<TState>(state, $"State \"{state}\" has been registered already.");

        return elementStateBuilder;
    }
}
