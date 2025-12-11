namespace PingMonitor.Services;

public sealed record PingTargetOptions(
    string Name,
    string Host,
    int IntervalMs,
    int TimeoutMs
);
