using DotState.Contracts;

namespace dotstate;

public class Transition<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    private readonly IStateConfiguration<TState, TTrigger> _destination;
    private Func<TState, bool> _predicate;


    public Transition(IStateConfiguration<TState, TTrigger> destination) : this(destination, (_) => true) { }

    public Transition(IStateConfiguration<TState, TTrigger> destination, Func<TState, bool> predicate)
    {
        _destination = destination;
        _predicate = predicate;
    }

    public bool CanTransition(TState currentState)
    {
        return _predicate(currentState);
    }

    public IStateConfiguration<TState, TTrigger>? ExecuteTransition(TState currentState)
    {
        if (CanTransition(currentState))
        {
            return _destination;
        }

        return null;
    }
}
