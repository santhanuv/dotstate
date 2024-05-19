using DotState.Builder;

namespace DotState.Contracts;

public interface IStateBuilder<TState, TTrigger>
{
    public TState State { get; }
    public TState DefaultState { get; init; }
    public IStateBuilder<TState, TTrigger>? Parent { get; }
    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination);
    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination, Func<TState, TTrigger, bool>? gaurd);
    public IStateBuilder<TState, TTrigger> IgnoreTrigger(TTrigger trigger);
    public IStateBuilder<TState, TTrigger> IgnoreTrigger(TTrigger trigger, Func<TState, TTrigger, bool>? gaurd);
    public IStateBuilder<TState, TTrigger> ChildOf(TState parent);
    public IStateBuilder<TState, TTrigger> OnEntry(Action<TState, TTrigger> action);
    public IStateBuilder<TState, TTrigger> OnExit(Action<TState, TTrigger> action);
}
