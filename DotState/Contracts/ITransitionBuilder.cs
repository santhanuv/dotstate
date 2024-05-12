namespace DotState.Contracts;

public interface ITransitionBuilder<TState, TTrigger>
{
    public TState Source { get; }

    public void AddGaurd(Func<TState, TTrigger, bool> gaurd, TState destination);
}
