namespace PingMonitor.Models;

public sealed class AppState
{
    public MonitorPersistedState? Monitor { get; set; }
    public WindowPlacementState? MainWindow { get; set; }
}
