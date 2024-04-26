namespace dotstate;

public class MachineConfig<TState, TTrigger> where TState : class, Enum where TTrigger : class, Enum
{
    private readonly Dictionary<TTrigger, Transition<TState>> triggers;

    public MachineConfig()
    {
        triggers = new();
    }

    public void AddTransition(
        TState source,
        TTrigger triggerID,
        TState destination)
    {
        var transition = new Transition<TState>(source, destination);
        triggers.Add(triggerID, transition);
    }
}
