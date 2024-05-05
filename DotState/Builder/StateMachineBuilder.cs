using DotState.Contracts;

namespace DotState.Builder;

public class StateMachineBuilder<TState, TTrigger> : IStateMachineBuilder<TState, TTrigger>
{
    private readonly IDictionary<TState, IStateBuilder<TState, TTrigger>> _stateBuilders;

    public StateMachineBuilder()
    {
        _stateBuilders = new Dictionary<TState, IStateBuilder<TState, TTrigger>>();
    }

    public IStateMachine<TState, TTrigger> Build()
    {
        var stateMachine = new StateMachine<TState, TTrigger>();

        foreach (var builder in _stateBuilders.Values)
        {
            BuildStateRepresentation(builder, stateMachine);
        }

        foreach (var builder in _stateBuilders.Values)
        {
            if (builder.Parent != null)
            {
                var child = stateMachine.GetStateRepresentation(builder.State) ?? throw new ArgumentNullException($"Unable to find state \"{builder.State}\"");
                var parent = stateMachine.GetStateRepresentation(builder.Parent) ?? throw new ArgumentNullException($"Unable to find state \"{builder.Parent}\"");

                parent.AddChild(child);
            }
        }

        return stateMachine;
    }

    public IStateBuilder<TState, TTrigger> Configure(TState source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var stateBuilder = new StateBuilder<TState, TTrigger>(source);

        _stateBuilders.TryAdd(source, stateBuilder);

        return stateBuilder;
    }

    public IStateBuilder<TState, TTrigger> GetStateBuilder(TState state)
    {
        return _stateBuilders[state];
    }

    private static IStateMachine<TState, TTrigger> BuildStateRepresentation(IStateBuilder<TState, TTrigger> builder, IStateMachine<TState, TTrigger> stateMachine)
    {
        if (stateMachine.GetStateRepresentation(builder.State) == null)
        {
            var stateRep = new StateRepresentation<TState, TTrigger>(builder.State, builder.GetEntryAction(), builder.GetExitAction());
            stateMachine.RegisterState(builder.State, stateRep);
        }

        return stateMachine;
    }
}
