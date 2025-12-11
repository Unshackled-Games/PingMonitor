using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace PingMonitor;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ConfigureLifetime);

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static void ConfigureLifetime(IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.Startup += (_, _) => ApplyBorderlessChrome(desktop.MainWindow);
        ApplyBorderlessChrome(desktop.MainWindow);
    }

    private static void ApplyBorderlessChrome(Window? window)
    {
        if (window is null)
            return;

        window.ExtendClientAreaToDecorationsHint = true;
        window.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        window.ExtendClientAreaTitleBarHeightHint = -1;
    }
}
