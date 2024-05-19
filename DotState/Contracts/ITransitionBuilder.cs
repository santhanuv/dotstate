namespace DotState.Contracts;

public interface ITransitionBuilder<TState, TTrigger>
{
    public TState Source { get; }
    public void ToDestination(TState destination);
    public void ToDestination(TState destination, Func<TState, TTrigger, bool> gaurd);
}
