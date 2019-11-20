using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace Rbmk.Radish.Views.FileExplorer
{
    public class PathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string[] path)
            {
                return path.LastOrDefault() ?? "";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}