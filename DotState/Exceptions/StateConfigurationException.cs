namespace DotState.Exceptions;

public class StateConfigurationException<TState> : Exception
{
    public TState State { get; init; }

    internal StateConfigurationException(TState state) : this(state, CreateMessageString(state), null) {}
    
    internal StateConfigurationException(TState state, string message): this(state, message, null) { }
    internal StateConfigurationException(TState state, Exception inner) : this(state, CreateMessageString(state), inner) { }
    internal StateConfigurationException(TState state, string message, Exception? inner) : base(message, inner) 
    {
        State = state;
    }

    private static string CreateMessageString(TState state)
    {
        return $"Unexpected error while configuring State \"{state}\"";
    }
}
