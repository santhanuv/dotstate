using DotState.Contracts;
using DotState.Exceptions;

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
            throw new StateConfigurationException<TState>(State);
        var defaultStateRep = stateMachine.GetStateRepresentation(DefaultState)
            ?? throw new StateConfigurationException<TState>(DefaultState);

        if (Parent != null)
        {
            var parentStateRep = stateMachine.GetStateRepresentation(Parent.State)
                ?? throw new StateConfigurationException<TState>(Parent.State);
            
            if (stateRep.Parent != null && stateRep.Parent.State != null && !stateRep.Parent.State.Equals(Parent.State))
                throw new MultipleParentException<TState>(State, stateRep.Parent.State, Parent.State);

            stateRep.Parent = parentStateRep;
        }

        if (defaultStateRep.Parent != null && State != null && !State.Equals(defaultStateRep.Parent.State))
        {
            throw new MultipleParentException<TState>(defaultStateRep.State, defaultStateRep.Parent.State, State);
        }
        defaultStateRep.Parent = stateRep;
        
        stateRep.Transitions = GetAllTransitions().ToDictionary(t => t.Key, t => t.Value.Build(stateMachine));
    }
}
