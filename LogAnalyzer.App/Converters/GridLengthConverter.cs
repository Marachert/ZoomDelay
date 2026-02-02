using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogAnalyzer.App.Converters;

public sealed class GridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width && !double.IsNaN(width))
        {
            return new GridLength(width, GridUnitType.Pixel);
        }

        return new GridLength(1, GridUnitType.Auto);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
