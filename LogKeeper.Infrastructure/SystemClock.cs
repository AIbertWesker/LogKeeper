using LogKeeper.Abstractions;

namespace LogKeeper.Infrastructure;

public sealed class SystemClock : ILogKeeperClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

