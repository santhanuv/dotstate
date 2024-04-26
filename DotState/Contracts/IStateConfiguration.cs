namespace DotState.Contracts;

internal interface IStateConfiguration<TState, TTrigger> where TState : notnull where TTrigger : notnull
{
    public Transition<TState, TTrigger>? GetTransition(TTrigger trigger);
    public void AddTransition(TTrigger trigger, Transition<TState, TTrigger> transition);
    public TState GetState();
}