namespace DotState.Contracts;

internal interface IStateRepresentation<TState, TTrigger>
{
    public TState State { get; init; }
    public IStateRepresentation<TState, TTrigger>? Parent { get; set; }
    public IList<IStateRepresentation<TState, TTrigger>> Children { get; }
    public Action<TState, TTrigger>? OnEntry { get; set; } 
    public Action<TState, TTrigger>? OnExit { get; set; }
    public IDictionary<TTrigger, ITransition<TState, TTrigger>> Transitions { get; set; }

    public ITransition<TState, TTrigger>? GetTransition(TTrigger trigger);
    public IList<IStateRepresentation<TState, TTrigger>> AddChild(IStateRepresentation<TState, TTrigger> stateRepresentation);
}