using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PingMonitor.Models;
using PingMonitor.Services;
using PingMonitor.ViewModels;

namespace PingMonitor;

public sealed partial class MainWindow : Window
{
    private readonly MonitorViewModel _vm;
    private readonly AppState _appState;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _appState = AppStateStorage.Load();
        _vm = MonitorViewModel.CreateFromAppSettings(_appState.Monitor);

        DataContext = _vm;
        _vm.ShowHistoryRequested += OnShowHistoryRequested;

        RestoreWindowPlacement(_appState.MainWindow);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        _vm.ShowHistoryRequested -= OnShowHistoryRequested;

        _appState.Monitor = _vm.GetPersistedState();
        _appState.MainWindow = CaptureWindowPlacement();
        AppStateStorage.Save(_appState);

        _vm.Dispose();
        DataContext = null;
    }

    private void OnShowHistoryRequested(object? sender, EventArgs e)
    {
        var historyVm = HistoryWindowViewModel.CreateFromMonitor(_vm);
        var window = new HistoryWindow
        {
            DataContext = historyVm
        };
        window.Show(this);
    }

    private WindowPlacementState CaptureWindowPlacement()
    {
        return new WindowPlacementState
        {
            X = Position.X,
            Y = Position.Y,
            Width = Width,
            Height = Height,
            WindowState = WindowState.ToString(),
            Topmost = Topmost
        };
    }

    private void RestoreWindowPlacement(WindowPlacementState? placement)
    {
        if (placement is null)
            return;

        if (placement.Width is not > 0 || placement.Height is not > 0)
            return;

        var x = (int)Math.Round(placement.X ?? 0);
        var y = (int)Math.Round(placement.Y ?? 0);
        var width = (int)Math.Round(placement.Width.Value);
        var height = (int)Math.Round(placement.Height.Value);

        var rect = new PixelRect(x, y, width, height);

        var screens = Screens;
        if (screens is not null)
        {
            var visible = false;
            foreach (var screen in screens.All)
            {
                if (screen.WorkingArea.Intersects(rect))
                {
                    visible = true;
                    break;
                }
            }

            if (!visible)
                return;
        }

        Position = new PixelPoint(rect.X, rect.Y);
        Width = rect.Width;
        Height = rect.Height;

        if (!string.IsNullOrWhiteSpace(placement.WindowState)
            && Enum.TryParse<WindowState>(placement.WindowState, out var state))
        {
            WindowState = state;
        }

        if (placement.Topmost is { } topmost)
            Topmost = topmost;
    }
}
