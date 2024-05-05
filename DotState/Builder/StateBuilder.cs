using DotState.Contracts;

namespace DotState.Builder;

public class StateBuilder<TState, TTrigger> : IStateBuilder<TState, TTrigger>
{
    public TState State {  get; private set; }
    public TState? Parent { get; private set; } = default;
    public IList<TState> Children { get; private set; }
    private readonly IDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> _transitions;
    private Action<TState, TTrigger>? _onEntry = null;
    private Action<TState, TTrigger>? _onExit = null;

    internal StateBuilder(TState source) : this(source, default, new List<TState>()) {}

    internal StateBuilder(TState source, TState? superState, IList<TState> subStates)
    {
        State = source;
        Parent = superState;
        Children = subStates;
        _transitions = new Dictionary<TTrigger, TransitionBuilder<TState, TTrigger>>();
    }

    public IStateBuilder<TState, TTrigger> AddTransition(TTrigger trigger, TState destination, Func<TState, TTrigger, bool> gaurd)
    {
        _transitions.TryGetValue(trigger, out var transition);

        if (transition == null)
        {
            transition = new TransitionBuilder<TState, TTrigger>(State);
            _transitions.Add(trigger, transition);
        }
        
        transition.AddGaurd(gaurd, destination);

        return this;
    }

    public IStateBuilder<TState, TTrigger> SubStateOf(TState parent)
    {
        Parent = parent;
        return this;
    }

    internal void SuperStateOf(TState child)
    {
        if(!Children.Contains(child))
        {
            Children.Add(child);
        }
    }

    public IStateBuilder<TState, TTrigger> OnEntry(Action<TState, TTrigger> action)
    {
        _onEntry = action;
        return this;
    }

    public IStateBuilder<TState, TTrigger> OnExit(Action<TState, TTrigger> action)
    {
        _onExit = action;
        return this;
    }

    public Action<TState, TTrigger>? GetEntryAction()
    {
        return _onEntry;
    }

    public Action<TState, TTrigger>? GetExitAction()
    {
        return _onExit;
    }

    internal IDictionary<TTrigger, TransitionBuilder<TState, TTrigger>> GetAllTransitions()
    {
        return _transitions;
    }
}
