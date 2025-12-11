using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PingMonitor;

public sealed partial class HistoryWindow : Window
{
    public HistoryWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
