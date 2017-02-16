﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Cognitivo.Converters
{
    public class LowerCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // unnecessary
            throw new NotImplementedException();
        }
    }
}