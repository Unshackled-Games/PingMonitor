using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PingMonitor.ViewModels.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
    public static readonly StatusToBrushConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isOk = string.Equals(value as string, "OK", StringComparison.OrdinalIgnoreCase);
        var key = isOk ? "OkBrush" : "FailBrush";

        if (Application.Current?.Resources.TryGetResource(key, theme: null, out var resource) == true
            && resource is IBrush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Color.Parse(isOk ? "#7BCFAD" : "#FFB3BA"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
