using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PingMonitor.Chrome;

namespace PingMonitor;

public partial class MainWindow
{
    private void DragAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var p = e.GetCurrentPoint(this);
        if (!p.Properties.IsLeftButtonPressed)
            return;

        if (IsWithinCard(e.Source) || IsWithinInteractiveControl(e.Source))
            return;

        BeginMoveDrag(e);
    }

    private static bool IsWithinInteractiveControl(object? source)
    {
        for (var cur = source as StyledElement; cur is not null; cur = cur.Parent as StyledElement)
        {
            if (cur is Button)
                return true;
        }

        return false;
    }

    private static bool IsWithinCard(object? source)
    {
        for (var cur = source as StyledElement; cur is not null; cur = cur.Parent as StyledElement)
        {
            if (cur is Border b && b.Classes.Contains("Card"))
                return true;
        }

        return false;
    }

    private void MinimizeClick(object? sender, RoutedEventArgs e)
        => WindowChromeActions.TryMinimize(this);

    private void MaximizeClick(object? sender, RoutedEventArgs e)
        => WindowChromeActions.TryToggleMaximize(this);

    private void TopmostClick(object? sender, RoutedEventArgs e)
        => WindowChromeActions.TryToggleTopmost(this);

    private void CloseClick(object? sender, RoutedEventArgs e)
        => WindowChromeActions.TryClose(this);
}
