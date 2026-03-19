namespace LogKeeper.Abstractions;

public interface ILogKeeperClock
{
    public DateTimeOffset UtcNow { get; }
}
