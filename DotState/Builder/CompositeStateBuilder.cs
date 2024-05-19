using DotState.Contracts;

namespace DotState.Builder;

internal class CompositeStateBuilder<TState, TTrigger> : 
    StateBuilder<TState, TTrigger>, IStateBuilder<TState, TTrigger>
{
    internal CompositeStateBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState source, TState defaultState)
        : this(machineBuilder, source, defaultState, null) { }

    internal CompositeStateBuilder(StateMachineBuilder<TState, TTrigger> machineBuilder, TState source, 
        TState defaultState, StateBuilder<TState, TTrigger>? parent) 
        : base(machineBuilder, source, defaultState, parent) 
    {
    }

    internal override IStateRepresentation<TState, TTrigger> Build(IStateMachine<TState, TTrigger> stateMachine)
    {
        var stateRep = new StateRepresentation<TState, TTrigger>
            (stateMachine, State, DefaultState, GetEntryAction(), GetExitAction());
        stateMachine.RegisterState(State, stateRep);

        return stateRep;
    }

    internal override void SetupRelations(IStateMachine<TState, TTrigger> stateMachine)
    {
        var stateRep = stateMachine.GetStateRepresentation(State) ??
            throw new InvalidOperationException($"Unexpected error when building configuration for {State}");

        stateRep.Parent = Parent != null ? stateMachine.GetStateRepresentation(Parent.State) : null;
        stateRep.Transitions = GetAllTransitions().ToDictionary(t => t.Key, t => t.Value.Build(stateMachine));
    }
}
