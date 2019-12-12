using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CANViewer.Models
{
    public class HexidecimalConverter : IValueConverter
    {
        public static HexidecimalConverter Singleton = new HexidecimalConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "0x" + ((uint)value).ToString("X8");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
