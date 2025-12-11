namespace PingMonitor.Models;

public sealed class WindowPlacementState
{
    public double? X { get; set; }
    public double? Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? WindowState { get; set; }
    public bool? Topmost { get; set; }
}
