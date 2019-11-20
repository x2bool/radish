using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Rbmk.Radish.Views.Converters
{
    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return -d;
            }
            else if (value is string s)
            {
                if (double.TryParse(s, out d))
                {
                    return -d;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}