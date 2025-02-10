using System;
using Microsoft.UI.Xaml.Data;

namespace SantoriniAI.Converters
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double originalValue && parameter is string percentageString && double.TryParse(percentageString, out double percentage))
            {
                // Convert the parameter to an actual percentage
                return originalValue * (percentage / 100.0);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
