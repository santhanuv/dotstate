using DotState.Builder;
using DotState.Contracts;
using DotState.Exceptions;
using FluentAssertions;
namespace DotState.Tests.Builder;

public class ElementStateMachineTest
{
    public enum State
    {
        State1,
        State2,
        State3,
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
        var smBuilder = CreateStateMachineBuilder();
        var testState = State.State1;

        // Act
        smBuilder.ElementState(testState);
        var stateBuilder = smBuilder.GetStateBuilder(testState);

        // Assert
        stateBuilder.Should().NotBeNull();
        stateBuilder?.State.Should().Be(State.State1, $"State: {testState} was used for configuration.");
    }

    [Fact]
    public void Build_StateMachineBuilder_BuildsStateMachine()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();

        // Act
        smBuilder.ElementState(State.State1)
            .AddTransition(Trigger.Trigger1, State.State2);

        smBuilder.ElementState(State.State2)
            .AddTransition(Trigger.Trigger1, State.State3)
            .AddTransition(Trigger.Trigger2, State.State1);

        var stateMachine = smBuilder.Build();

        // Assert
        stateMachine.Should().NotBeNull();
    }

    [Fact]
    public void CreateStateEngine_WithStateMachineFromStateMachineBuilder_GetStateEngine()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();

        // Act
        smBuilder.ElementState(State.State1)
            .AddTransition(Trigger.Trigger1, State.State2);

        smBuilder.ElementState(State.State2)
            .AddTransition(Trigger.Trigger1, State.State3)
            .AddTransition(Trigger.Trigger2, State.State1);

        var stateMachine = smBuilder.Build();
        var stateEngine = CreateStateEngine(smBuilder, State.State1);

        // Assert
        stateMachine.Should().NotBeNull();
        stateEngine.Should().NotBeNull();
        stateEngine.Should().BeOfType<StateEngine<State, Trigger>>();
    }

    [Fact]
    public void AddTransition_NoGaurd_ExecutesTransition()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();
        var testState = State.State1;
        var trigger = Trigger.Trigger1;
        var testFinalState = State.State2;

        // Act
        smBuilder.ElementState(testState)
            .AddTransition(trigger, testFinalState);
        var stateEngine = CreateStateEngine(smBuilder, testState);
        var newState = stateEngine.ExecuteTransition(trigger);

        // Assert
        newState.Should().Be(testFinalState);
    }

    [Fact]
    public void AddTransition_WithGaurd_ExecutesTransition()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();
        var testStartState = State.State1;
        var trigger = Trigger.Trigger1;
        var testFinalState = State.State2;
        var testFailedFinalState = State.State3;

        // Act
        smBuilder.ElementState(testStartState)
            .AddTransition(trigger, testFinalState, (_, _) => true)
            .AddTransition(trigger, testFailedFinalState, (_, _) => false);

        var stateEngine = CreateStateEngine(smBuilder, testStartState);
        var newState = stateEngine.ExecuteTransition(trigger);

        // Assert
        newState.Should().Be(testFinalState);
        newState.Should().NotBe(testFailedFinalState);
    }

    [Fact]
    public void AddTransition_WithMultipleTrueGaurdsToOneState_ExecutesTransition()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();
        var testStartState = State.State1;
        var trigger = Trigger.Trigger1;
        var testFinalState = State.State2;
        var testFailedFinalState = State.State3;

        // Act
        smBuilder.ElementState(testStartState)
            .AddTransition(trigger, testFinalState, (_, _) => true)
            .AddTransition(trigger, testFinalState, (_, _) => true)
            .AddTransition(trigger, testFailedFinalState, (_, _) => false);

        var stateEngine = CreateStateEngine(smBuilder, testStartState);
        var newState = stateEngine.ExecuteTransition(trigger);

        // Assert
        newState.Should().Be(testFinalState);
        newState.Should().NotBe(testFailedFinalState);
    }

    [Fact]
    public void AddTransition_WithFalseGaurdsToOneState_ExecutesTransition()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();
        var testStartState = State.State1;
        var trigger = Trigger.Trigger1;
        var testFinalState = State.State2;
        var testFailedFinalState = State.State3;

        smBuilder.ElementState(testStartState)
            .AddTransition(trigger, testFinalState, (_, _) => false)
            .AddTransition(trigger, testFailedFinalState, (_, _) => false);

        var stateEngine = CreateStateEngine(smBuilder, testStartState);
        stateEngine.IgnoreTriggerOnFalseGaurds = true;

        // Act
        var newState = stateEngine.ExecuteTransition(trigger);

        // Assert
        newState.Should().Be(testStartState);
        newState.Should().NotBe(testFinalState);
        newState.Should().NotBe(testFailedFinalState);
    }

    [Fact]
    public void AddTransition_WithMultipleTrueFalseGaurdsToOneState_FailsToExecutesTransition()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();
        var testStartState = State.State1;
        var trigger = Trigger.Trigger1;
        var testFinalState = State.State2;
        var testFailedFinalState = State.State3;

        smBuilder.ElementState(testStartState)
            .AddTransition(trigger, testFinalState, (_, _) => true)
            .AddTransition(trigger, testFinalState, (_, _) => false)
            .AddTransition(trigger, testFailedFinalState, (_, _) => false);

        var stateEngine = CreateStateEngine(smBuilder, testStartState);
        stateEngine.IgnoreTriggerOnFalseGaurds = true;

        // Act
        var newState = stateEngine.ExecuteTransition(trigger);

        // Assert
        newState.Should().Be(testStartState);
        newState.Should().NotBe(testFinalState);
        newState.Should().NotBe(testFailedFinalState);
    }

    [Fact]
    public void AddTransition_MultipleGaurdsToTwoState_FailsToExecuteTransition()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();
        var testStartState = State.State1;
        var trigger = Trigger.Trigger1;
        var testFinalState = State.State2;
        var testFailedFinalState = State.State3;
        var errorMessage = $"Cannot transition from state \"{testStartState}\" on trigger \"{trigger}\"";
        
        smBuilder.ElementState(testStartState)
            .AddTransition(trigger, testFinalState, (_, _) => true)
            .AddTransition(trigger, testFailedFinalState, (_, _) => true);
        var stateEngine = CreateStateEngine(smBuilder, testStartState);

        // Act
        var invoking = stateEngine
            .Invoking(se => se.ExecuteTransition(trigger));

        // Assert
        invoking
            .Should().Throw<InvalidTransitionException<State, Trigger>>()
            .WithMessage(errorMessage);
    }

    [Fact]
    public void ExecuteTransition_InvalidTrigger_FailsToExecute()
    {
        // Arrange
        var smBuilder = CreateStateMachineBuilder();
        var testStartState = State.State1;
        var trigger = Trigger.Trigger1;
        var invalidTrigger = Trigger.Trigger2;
        var testFinalState = State.State2;
        var testFailedFinalState = State.State3;
        var errorMessage = $"Cannot transition from state \"{testStartState}\" on trigger \"{invalidTrigger}\"";

        // Act
        smBuilder.ElementState(testStartState)
            .AddTransition(trigger, testFinalState, (_, _) => true)
            .AddTransition(trigger, testFailedFinalState, (_, _) => true);

        var stateEngine = CreateStateEngine(smBuilder, testStartState);

        // Assert
        var newState = stateEngine
            .Invoking(se => se.ExecuteTransition(invalidTrigger))
            .Should().Throw<InvalidTransitionException<State, Trigger>>()
            .WithMessage(errorMessage);
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