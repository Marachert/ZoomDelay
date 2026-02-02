using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogAnalyzer.App.Converters;

public sealed class IndentThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int level = 0;
        if (value is int intValue)
        {
            level = intValue;
        }

        double indentSize = 16.0d;
        return new Thickness(level * indentSize, 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
