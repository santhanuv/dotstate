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
        BuildStateRepresentations(stateMachine);

        foreach (var builder in _stateBuilders.Values)
        {
            builder.SetupStateRelations(stateMachine);
        }

        return stateMachine;
    }

    public IStateBuilder<TState, TTrigger> Configure(TState source)
    {
        return GetOrRegisterState(source);
    }

    public IStateBuilder<TState, TTrigger>? GetStateBuilder(TState state)
    {
        _stateBuilders.TryGetValue(state, out var builder);
        return builder;
    }

    internal StateBuilder<TState, TTrigger> GetOrRegisterState(TState state) 
    {
        if (state == null) throw new ArgumentNullException(nameof(state));

        _stateBuilders.TryGetValue(state, out var stateBuilder);

        if (stateBuilder == null)
        {
            stateBuilder = new StateBuilder<TState, TTrigger>(this, state);
            _stateBuilders.TryAdd(state, stateBuilder);
        }

        return stateBuilder;
    }

    private void BuildStateRepresentations(IStateMachine<TState, TTrigger> stateMachine)
    {
        foreach (var stateBuilder in _stateBuilders.Values)
        {
            var stateRep = new StateRepresentation<TState, TTrigger>(stateBuilder.State, 
                stateBuilder.GetEntryAction(), stateBuilder.GetExitAction());

            stateMachine.RegisterState(stateBuilder.State, stateRep);
        }
    }
}
