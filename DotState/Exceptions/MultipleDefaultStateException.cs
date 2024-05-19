namespace DotState.Exceptions;

public class MultipleDefaultStateException<TState> : Exception
{
    public TState State { get; init; }
    public IList<TState> DefaultStates { get; init; }

    internal MultipleDefaultStateException(TState state, params TState[] defaultStates) 
        : this(CreateMessageString(state, defaultStates), null, state, defaultStates) {}

    internal MultipleDefaultStateException(Exception? innerException, TState state, params TState[] defaultStates)
        : this(CreateMessageString(state, defaultStates), innerException, state, defaultStates) {}
    internal MultipleDefaultStateException(string message, TState state, params TState[] defaultStates)
        : this(message, null, state, defaultStates) { }

    internal MultipleDefaultStateException(string message, Exception? innerException, TState state, params TState[] defaultStates) 
        : base(message, innerException) 
    {
        State = state;
        DefaultStates = defaultStates;
    }

    private static string CreateMessageString(TState state, params TState[] defaultStates)
    {
        var defaultStatesAsString = string.Join(",", defaultStates.Select(s => $"\"{s}\""));
        return "Multiple default states, " + defaultStatesAsString + $" for State \"{state}\"";
    }
}
