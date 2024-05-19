namespace DotState.Contracts;

internal interface IStateRepresentation<TState, TTrigger>
{
    public TState State { get; init; }
    public TState DefaultState { get; }
    public IStateRepresentation<TState, TTrigger>? Parent { get; set; }
    public Action<TState, TTrigger>? OnEntry { get; set; } 
    public Action<TState, TTrigger>? OnExit { get; set; }
    public IDictionary<TTrigger, ITransition<TState, TTrigger>> Transitions { get; set; }

    public ITransition<TState, TTrigger>? GetTransition(TTrigger trigger);
    public IStateRepresentation<TState, TTrigger> GetDefaultStateRepresentation();
}