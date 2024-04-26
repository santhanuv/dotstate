namespace dotstate;

public class Transition<TState> where TState : class, Enum
{
    private TState _source;
    private TState _destination;
    public Transition(TState source, TState destination)
    {
        _source = source;
        _destination = destination;
    }
}
