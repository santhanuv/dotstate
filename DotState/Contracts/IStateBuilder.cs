using DotState.Builder;

namespace DotState.Contracts;

public interface IStateBuilder<TState, TTrigger>
{
    public TState State { get; }
    public TState? Parent { get; }
    public IList<TState> Children { get; }

    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination, Func<TState, TTrigger, bool> gaurd);
    public IStateBuilder<TState, TTrigger> SubStateOf(TState state);
    public IStateBuilder<TState, TTrigger> OnEntry(Action<TState, TTrigger> action);
    public IStateBuilder<TState, TTrigger> OnExit(Action<TState, TTrigger> action);
    public Action<TState, TTrigger>? GetEntryAction();
    public Action<TState, TTrigger>? GetExitAction();

}
