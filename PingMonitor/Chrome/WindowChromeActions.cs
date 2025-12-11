using Avalonia.Controls;

namespace PingMonitor.Chrome;

internal static class WindowChromeActions
{
    public static void TryMinimize(Window? window)
    {
        if (window is { CanMinimize: true } w)
            w.WindowState = WindowState.Minimized;
    }

    public static void TryToggleMaximize(Window? window)
    {
        if (window is not { CanMaximize: true } w)
            return;

        w.WindowState = w.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    public static void TryToggleTopmost(Window? window)
    {
        if (window is null)
            return;

        window.Topmost = !window.Topmost;
    }

    public static void TryClose(Window? window)
        => window?.Close();
}
