using DotState.Contracts;
using DotState.Exceptions;

namespace DotState.Builder;

internal class ElementStateBuilder<TState, TTrigger> :
    StateBuilder<TState, TTrigger>, IStateBuilder<TState, TTrigger>
{
    internal ElementStateBuilder(
        StateMachineBuilder<TState, TTrigger> machineBuilder, TState source)
        : base(machineBuilder, source, source, null) { }

    internal ElementStateBuilder(
        StateMachineBuilder<TState, TTrigger> machineBuilder, TState source, StateBuilder<TState, TTrigger>? parent)
        : base(machineBuilder, source, source, parent) { }

    
    internal override IStateRepresentation<TState, TTrigger> Build(IStateMachine<TState, TTrigger> stateMachine)
    {
        var stateRep = new StateRepresentation<TState, TTrigger>(stateMachine, State, State, GetEntryAction(), GetExitAction());
        stateMachine.RegisterState(State, stateRep);

        return stateRep;
    }

    internal override void SetupRelations(IStateMachine<TState, TTrigger> stateMachine)
    {
        var stateRep = stateMachine.GetStateRepresentation(State) ??
            throw new StateConfigurationException<TState>(State);

        if (Parent != null)
        {
            var parentStateRep = stateMachine.GetStateRepresentation(Parent.State)
                ?? throw new StateConfigurationException<TState>(Parent.State);

            if (stateRep.Parent != null && stateRep.Parent.State != null && !stateRep.Parent.State.Equals(Parent.State))
                throw new MultipleParentException<TState>(State, Parent.State, stateRep.Parent.State);
            stateRep.Parent = parentStateRep;
        }

        stateRep.Transitions = GetAllTransitions().ToDictionary(t => t.Key, t => t.Value.Build(stateMachine));
    }
}
