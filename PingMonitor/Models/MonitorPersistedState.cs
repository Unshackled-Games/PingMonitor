using System.Collections.Generic;

namespace PingMonitor.Models;

public sealed class MonitorPersistedState
{
    public bool? IsRunning { get; set; }
    public List<MonitorTargetPersistedState> Targets { get; set; } = new();
}

public sealed class MonitorTargetPersistedState
{
    public string? Name { get; set; }
    public string? Host { get; set; }
    public List<PingSampleState> History { get; set; } = new();
}
