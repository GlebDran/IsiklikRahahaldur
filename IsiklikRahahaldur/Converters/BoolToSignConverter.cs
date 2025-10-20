using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace IsiklikRahahaldur.Converters
{
    /// <summary>
    /// Конвертирует булево значение в строку: "+" для true (доход), "-" для false (расход).
    /// </summary>
    public class BoolToSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIncome)
            {
                return isIncome ? "+ " : "- ";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
