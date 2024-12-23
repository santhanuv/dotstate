﻿namespace DotState.Exceptions;

public class InvalidTransitionException<TState, TTrigger> : Exception
{
    internal InvalidTransitionException(TState state, TTrigger trigger)
        : this(state, trigger, CreateMessageString(state, trigger), null)
    {
    }

    internal InvalidTransitionException(TState state, TTrigger trigger, string message)
        : this(state, trigger, message, null)
    {
    }

    internal InvalidTransitionException(TState state, TTrigger trigger, Exception inner)
        : this(state, trigger, CreateMessageString(state, trigger), inner)
    {
    }

    internal InvalidTransitionException(TState state, TTrigger trigger, string message, Exception? inner)
        : base(message, inner)
    {
        State = state;
        Trigger = trigger;
    }

    public TState State { get; private set; }
    public TTrigger Trigger { get; private set; }

    private static string CreateMessageString(TState state, TTrigger trigger)
    {
        return $"Cannot transition from state \"{state}\" on trigger \"{trigger}\"";
    }
}