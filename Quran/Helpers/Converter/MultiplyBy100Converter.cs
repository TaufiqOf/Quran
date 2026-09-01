using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Quran.Helpers.Converter;

public class MultiplyBy100Converter : IValueConverter
{
    public static readonly MultiplyBy100Converter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue * 100.0;
        }

        if (value is float floatValue)
        {
            return floatValue * 100.0f;
        }

        if (value is decimal decimalValue)
        {
            return decimalValue * 100m;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue / 100.0;
        }

        if (value is float floatValue)
        {
            return floatValue / 100.0f;
        }

        if (value is decimal decimalValue)
        {
            return decimalValue / 100m;
        }

        return value;
    }
}