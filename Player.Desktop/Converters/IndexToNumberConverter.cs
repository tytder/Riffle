using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Player.Desktop.Converters;

public class IndexToNumberConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 &&
            values[0] is object item &&
            values[1] is IEnumerable items)
        {
            int index = 0;
            foreach (var i in items)
            {
                if (Equals(i, item))
                    return index + 1; // 1-based

                index++;
            }
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}