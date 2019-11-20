using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace Rbmk.Radish.Views.FileExplorer
{
    public class FullPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string[] path)
            {
                var separator = Path.DirectorySeparatorChar.ToString();
                if (separator == "/")
                {
                    return "(/" + string.Join(separator, path) + ")";
                }
                return "(" + string.Join(separator, path) + ")";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}