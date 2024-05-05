using DotState.Contracts;

namespace DotState.Builder;

public class TransitionBuilder<TState, TTrigger> : ITransitionBuilder<TState, TTrigger>
{
    public TState Source {  get; private set; }
    private IDictionary<TState, Func<TState, TTrigger, bool>> _gaurds;

    public TransitionBuilder(TState source)
    {
        Source = source;
        _gaurds = new Dictionary<TState, Func<TState, TTrigger, bool>>();
    }

    public void AddGaurd(Func<TState, TTrigger, bool> gaurd, TState destination)
    {
        if (destination == null) { throw new ArgumentNullException(nameof(destination)); }
        
        if (!_gaurds.TryAdd(destination, gaurd)) 
        {
            throw new InvalidOperationException($"A gaurd to {destination} already exists");
        }
    }
}
