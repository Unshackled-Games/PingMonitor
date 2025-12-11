using System.Collections.Generic;

namespace PingMonitor.ViewModels;

public sealed class PingHistoryNodeViewModel
{
    public PingHistoryNodeViewModel(string header, string? description, IReadOnlyList<PingHistoryNodeViewModel>? children)
    {
        Header = header;
        Description = description;
        Children = children;
    }

    public string Header { get; }
    public string? Description { get; }
    public IReadOnlyList<PingHistoryNodeViewModel>? Children { get; }

    public bool HasChildren => Children is { Count: > 0 };
}
