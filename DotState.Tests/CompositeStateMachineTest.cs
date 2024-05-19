using DotState.Builder;
using DotState.Contracts;
using DotState.Exceptions;
using FluentAssertions;

namespace DotState.Tests;

public class CompositeStateMachineTest
{
    public enum State
    {
        State1,
        State2, 
        State3,
        State4,
        State5,
    }

    public enum Trigger
    {
        Trigger1,
        Trigger2, 
        Trigger3,
    }

    [Fact]
    public void Configure_State_RegisterStateInMachine()
    {
        // Arrange
        var builder = CreateStateMachineBuilder();
        builder.CompositeState(State.State1, State.State2);

        // Act
        var stateBuilder = builder.GetStateBuilder(State.State1);

        // Assert
        stateBuilder.Should().NotBeNull();
    }

    [Fact]
    public void Configure_MultipleParentsCompositeFirst_ThrowsException()
    {
        // Arrange
        var builder = CreateStateMachineBuilder();
        builder.CompositeState(State.State1, State.State2);

        builder.ElementState(State.State2)
            .ChildOf(State.State3);

        // Act
        var invoking = builder.Invoking(builder => builder.Build());

        // Assert
        invoking
            .Should().Throw<MultipleParentException<State>>();
    }

    [Fact]
    public void Configure_MultipleParentsElementFirst_ThrowsException()
    {
        // Arrange
        var builder = CreateStateMachineBuilder();

        builder.ElementState(State.State2)
            .ChildOf(State.State3);
        
        builder.CompositeState(State.State1, State.State2);
        // Act
        var invoking = builder.Invoking(builder => builder.Build());

        // Assert
        invoking
            .Should().Throw<MultipleParentException<State>>();
    }

    [Fact]
    public void Configure_MultipleParentsForComposite_ThrowsException()
    {
        // Arrange
        var builder = CreateStateMachineBuilder();

        builder.CompositeState(State.State1, State.State2)
            .ChildOf(State.State3);

        // Act
        var invoking = builder.Invoking(builder => builder.CompositeState(State.State1, State.State2).ChildOf(State.State4));

        // Assert
        invoking
            .Should().Throw<MultipleParentException<State>>();
    }

    [Fact]
    public void ExecuteTransition_CompositeState_DefaultStateIsSelected()
    {
        // Arrange
        var builder = CreateStateMachineBuilder();
        builder.ElementState(State.State1)
            .AddTransition(Trigger.Trigger1, State.State2);

        builder.CompositeState(State.State2, State.State3);

        var engine = CreateStateEngine(builder, State.State1);

        // Act
        engine.ExecuteTransition(Trigger.Trigger1);

        // Assert
        engine.CurrentState.Should().Be(State.State3);
    }

    public static IStateMachineBuilder<State, Trigger> CreateStateMachineBuilder()
    {
        return new StateMachineBuilder<State, Trigger>();
    }

    public static StateEngine<State, Trigger> CreateStateEngine(IStateMachineBuilder<State, Trigger> smBuilder, State initalState)
    {
        var stateMachine = smBuilder.Build();
        return new StateEngine<State, Trigger>(stateMachine, initalState);
    }
}
