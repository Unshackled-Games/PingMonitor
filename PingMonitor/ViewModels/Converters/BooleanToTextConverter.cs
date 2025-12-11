using System.Globalization;
using Avalonia.Data.Converters;

namespace PingMonitor.ViewModels.Converters;

public sealed class BooleanToTextConverter : IValueConverter
{
    public static readonly BooleanToTextConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "Pause" : "Resume";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
