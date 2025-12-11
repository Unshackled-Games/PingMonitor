using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PingMonitor.ViewModels;

namespace PingMonitor;

public sealed partial class MainWindow : Window
{
    private readonly MonitorViewModel _vm;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        _vm = MonitorViewModel.CreateFromAppSettings();
        DataContext = _vm;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _vm.Dispose();
        DataContext = null;
    }
}
