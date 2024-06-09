using DotState.Contracts;
using DotState.Exceptions;

namespace DotState.Builder;

/// <summary>
/// A builder class for the state machine.
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
/// <typeparam name="TTrigger">The trigger type</typeparam>
public class StateMachineBuilder<TState, TTrigger> : IStateMachineBuilder<TState, TTrigger>
{
    private readonly IDictionary<TState, StateBuilder<TState, TTrigger>> _stateBuilders;

    /// <summary>
    /// Initializes a new instance of Class <c>StateMachineBuilder</c>.
    /// </summary>
    public StateMachineBuilder()
    {
        _stateBuilders = new Dictionary<TState, StateBuilder<TState, TTrigger>>();
    }

    /// <summary>
    /// Method <c>Build</c> creates an instance of the state machine with the registered state.
    /// </summary>
    /// <returns>An instance of state machine.</returns>
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

    /// <summary>
    /// Method <c>CompositeState</c> registers the state as composite state in the state machine.
    /// 
    /// If not registered, the default state is registered as an element state by default.
    /// </summary>
    /// <param name="state">The state to register</param>
    /// <param name="defaultState">The default child state of the composite state.</param>
    /// <returns>A builder for the composite state.</returns>
    /// <exception cref="MultipleDefaultStateException{TState}">Thrown if multiple default state is registered for the state.</exception>
    /// <exception cref="ArgumentNullException">Thrown when either the state or the default state is null.</exception>
    /// <exception cref="StateConfigurationException{TState}">Thrown if the state is already registered.</exception>
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

    /// <summary>
    /// Method <c>ElementState</c> registers the state as element state in the state machine.
    /// </summary>
    /// <param name="state">The state to register.</param>
    /// <returns>A builder for the element state.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the state is null.</exception>
    /// <exception cref="StateConfigurationException{TState}">Thrown if state is already registered.</exception>
    public IStateBuilder<TState, TTrigger> ElementState(TState state)
    {
        return GetStateBuilder(state) ?? RegisterElementState(state);
    }

    /// <summary>
    /// Method <c>GetStateBuiilder</c> gets the registered state builder for the given state.
    /// </summary>
    /// <param name="state">The registered state</param>
    /// <returns>State Builder for the state if it is registered; otherwise, Null.</returns>
    public IStateBuilder<TState, TTrigger>? GetStateBuilder(TState state)
    {
        _stateBuilders.TryGetValue(state, out var builder);
        return builder;
    }

    /// <summary>
    /// Method <c>RegisterCompositeState</c> register the state as composite state in the state machine.
    /// 
    /// If not registered, the default state is registered as an element state by default.
    /// </summary>
    /// <param name="state">The state to register</param>
    /// <param name="defaultState">The default child state of the composite state.</param>
    /// <returns>A composite state builder for the state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the state or the default state is null.</exception>
    /// <exception cref="StateConfigurationException{TState}">Thrown if the state is already registered.</exception>
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

    /// <summary>
    /// Method <c>RegisterCompositeState</c> registers the state builder as composite state builder in the state machine. Can be used to re-register 
    /// an element state, as a composite state.
    /// 
    /// If not registered, the default state is registered as an element state by default.
    /// </summary>
    /// <param name="stateBuilder">The state builder to register as composite.</param>
    /// <param name="defaultState">The default child state of the composite state.</param>
    /// <returns>A state builder for the composite state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the state or the default state is null.</exception>
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

    /// <summary>
    /// Method <c>RegisterElementState</c> register the state as element state in the state machine.
    /// </summary>
    /// <param name="state">The state to register</param>
    /// <returns>A state builder for the element state.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the state is null.</exception>
    /// <exception cref="StateConfigurationException{TState}">Thrown if state is already registered.</exception>
    internal StateBuilder<TState, TTrigger> RegisterElementState(TState state) 
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        var elementStateBuilder = new ElementStateBuilder<TState, TTrigger>(this, state);
        var hasRegistered = _stateBuilders.TryAdd(state, elementStateBuilder);
        
        if (!hasRegistered) throw new StateConfigurationException<TState>(state, $"State \"{state}\" has been registered already.");

        return elementStateBuilder;
    }
}
