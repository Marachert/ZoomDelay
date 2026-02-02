using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LogAnalyzer.App.Converters;

public sealed class HeavyWindowBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double score = 0.0d;
        if (value is double doubleValue)
        {
            score = doubleValue;
        }

        Color start = Color.FromArgb(40, 120, 200, 120);
        Color end = Color.FromArgb(200, 60, 160, 80);

        byte a = (byte)(start.A + ((end.A - start.A) * score));
        byte r = (byte)(start.R + ((end.R - start.R) * score));
        byte g = (byte)(start.G + ((end.G - start.G) * score));
        byte b = (byte)(start.B + ((end.B - start.B) * score));

        return new SolidColorBrush(Color.FromArgb(a, r, g, b));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
