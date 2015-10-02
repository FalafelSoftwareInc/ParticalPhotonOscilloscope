using System;
using Windows.UI.Xaml.Data;

namespace Falafel.Utilities
{
    /// <summary>
    ///     Two way IValueConverter that lets you bind a property on a bindable object
    ///     that can be an empty string value to a dependency property that should 
    ///     be set to null in that case
    /// </summary>
    public class StringFormatValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string formatString = parameter != null ? parameter.ToString() : string.Empty;
            return string.Format(System.Globalization.CultureInfo.CurrentUICulture, formatString, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
