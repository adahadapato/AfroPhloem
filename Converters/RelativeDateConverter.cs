using System.Globalization;

namespace Phloem.Converters;

/// <summary>
/// Turns a date into "today", "yesterday", "3 days ago" or "12 Oct 2026".
/// Used on the Admin page: "Registered today · Ghana".
/// </summary>
public class RelativeDateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return string.Empty;
        var days = (DateTime.Today - date.Date).Days;
        return days switch
        {
            <= 0 => "today",
            1 => "yesterday",
            < 7 => $"{days} days ago",
            _ => date.ToString("d MMM yyyy", culture)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
