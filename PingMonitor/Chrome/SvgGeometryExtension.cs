using System;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace PingMonitor.Chrome;

public enum SvgIconKind
{
    Minimize,
    Maximize,
    Topmost,
    Close,
}

public sealed class SvgGeometryExtension : MarkupExtension
{
    public SvgIconKind Kind { get; set; }

    private static readonly Lazy<Geometry> Close = new(() => SvgGeometryLoader.LoadOrFallback(
        assetUri: "avares://PingMonitor/Assets/x.svg",
        fallbackPathData: "M18 6 L6 18 M6 6 L18 18"));

    private static readonly Lazy<Geometry> Minimize = new(() => SvgGeometryLoader.LoadOrFallback(
        assetUri: "avares://PingMonitor/Assets/crop-16-9.svg",
        fallbackPathData: "M4 10 A2 2 0 0 1 6 8 H18 A2 2 0 0 1 20 10 V14 A2 2 0 0 1 18 16 H6 A2 2 0 0 1 4 14 Z"));

    private static readonly Lazy<Geometry> Maximize = new(() => SvgGeometryLoader.LoadOrFallback(
        assetUri: "avares://PingMonitor/Assets/square-dashed.svg",
        fallbackPathData: "M3 5 A2 2 0 0 1 5 3 H19 A2 2 0 0 1 21 5 V19 A2 2 0 0 1 19 21 H5 A2 2 0 0 1 3 19 Z"));

    private static readonly Lazy<Geometry> Topmost = new(() => SvgGeometryLoader.LoadOrFallback(
        assetUri: "avares://PingMonitor/Assets/pin.svg",
        fallbackPathData: "M12 2 L12 14 M8 6 H16 M11 14 L9 22 M13 14 L15 22"));

    public override object ProvideValue(IServiceProvider serviceProvider)
        => Kind switch
        {
            SvgIconKind.Minimize => Minimize.Value,
            SvgIconKind.Maximize => Maximize.Value,
            SvgIconKind.Topmost => Topmost.Value,
            SvgIconKind.Close => Close.Value,
            _ => Close.Value
        };
}
