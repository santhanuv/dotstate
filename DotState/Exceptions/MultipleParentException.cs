namespace DotState.Exceptions;

public class MultipleParentException<TState> : Exception
{
    public TState State { get; init; }
    public IList<TState> ParentStates { get; init; }

    internal MultipleParentException(TState state, params TState[] parentStates)
        : this(CreateMessageString(state, parentStates), null, state, parentStates) { }

    internal MultipleParentException(Exception? innerException, TState state, params TState[] parentStates)
        : this(CreateMessageString(state, parentStates), innerException, state, parentStates) { }
    internal MultipleParentException(string message, TState state, params TState[] parentStates)
        : this(message, null, state, parentStates) { }

    internal MultipleParentException(string message, Exception? innerException, TState state, params TState[] parentStates)
        : base(message, innerException)
    {
        State = state;
        ParentStates = parentStates;
    }

    private static string CreateMessageString(TState state, params TState[] parentStates)
    {
        var parentStatesAsString = string.Join(",", parentStates.Select(s => $"\"{s}\""));
        return "Multiple parent states, " + parentStatesAsString + $" for State \"{state}\"";
    }
}
