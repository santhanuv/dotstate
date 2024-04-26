using dotstate;

namespace DotState.Contracts;

public interface IStateConfiguration<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public void AddTransition(TTrigger trigger, IStateConfiguration<TState, TTrigger> destination, Func<TState, bool>? predicate = null);
    public Transition<TState, TTrigger>? GetTransition(TTrigger trigger);
    public TState GetState();
}
